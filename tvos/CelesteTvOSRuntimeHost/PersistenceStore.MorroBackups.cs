#if CELESTE_RUNTIME && TVOS_CELESTE_RUNTIME_HOST && MORRO_VANILLA_ACHIEVEMENTS
using System.Security.Cryptography;
using System.Text;
using Celeste;
using Foundation;
using Morro.Backups;

namespace CelesteTvOSHost;

internal sealed partial class PersistenceStore
{
    private const string PromptAlias = "controllerPrompts";
    private const string HudAlias = "performanceHud";
    private const int MaximumIntentBytes = 32 * 1024;
    private readonly object morroOperationGate = new();
    private bool morroBackupReloadPending;
    private string? restoredPrompt;
    private string? restoredHud;
    private string IntentKey => Key("MorroBackupIntent.v1");

    internal SaveExportSnapshot CreateMorroSaveManagerSnapshot() => WithMorroGuard(CreateMorroSaveManagerSnapshotCore);

    private SaveExportSnapshot CreateMorroSaveManagerSnapshotCore()
    {
        lock (gate)
        {
            if (!MorroAchievements.FlushBackup())
                throw new IOException("Achievement storage is waiting for synchronization.");
            return WithMorroState(CreateReadOnlySaveManagerSnapshot());
        }
    }

    private SaveExportSnapshot WithMorroState(SaveExportSnapshot snapshot) => snapshot with
    {
        Achievements = MorroAchievements.ExportBackup(),
        Preferences = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [PromptAlias] = ReadPrompt(), [HudAlias] = ReadHud()
        }
    };

    internal SaveMutationResult MutateMorroSingleFile(SaveMutationCommand command) =>
        WithMorroGuard(() => MutateMorroSingleFileCore(command));

    private SaveMutationResult MutateMorroSingleFileCore(SaveMutationCommand command)
    {
        lock (gate)
        {
            if (morroBackupReloadPending || defaults.StringForKey(IntentKey) != null)
                return SaveMutationResult.Failed("backup-pending", WithMorroState(CreateExportSnapshotUnsafe()), true);
            SaveMutationResult result = MutateFromSaveManager(command);
            return result with { Snapshot = WithMorroState(result.Snapshot) };
        }
    }

    internal byte[] ExportMorroBackup(SaveExportSnapshot snapshot, string name) => FullBackupArchive.Create(
        new FullBackupState(name, "tvos", snapshot.Files,
            snapshot.Achievements ?? throw new InvalidOperationException("Missing achievement snapshot."),
            snapshot.Preferences ?? throw new InvalidOperationException("Missing preference snapshot.")),
        MorroAchievements.ValidateBackup);

    internal SaveMutationResult RestoreMorroBackup(byte[] archive, SaveExportSnapshot expected)
    {
        try
        {
            FullBackupState backup = FullBackupArchive.Read(archive, MorroAchievements.ValidateBackup,
                MaximumSaveDataBytes, MaximumSettingsBytes + 3 * MaximumSaveDataBytes);
            (string? prompt, string? hud) = backup.Platform == "tvos" ? ValidatePreferences(backup.Preferences) : (ReadPrompt(), ReadHud());
            return ReplaceMorroSession(backup.Files, backup.Achievements, prompt, hud, expected);
        }
        catch (Exception)
        {
            return WithMorroGuard(() =>
            {
                lock (gate)
                    return SaveMutationResult.Failed("backup-invalid", WithMorroState(CreateExportSnapshotUnsafe()), externalMutationRequiresRestart);
            });
        }
    }

    internal SaveMutationResult ResetMorroAchievements(SaveExportSnapshot expected) =>
        ReplaceMorroSession(expected.Files, MorroAchievements.EmptyBackup(), ReadPrompt(), ReadHud(), expected);

    private SaveMutationResult ReplaceMorroSession(IReadOnlyDictionary<string, byte[]?> files,
        string achievements, string? prompt, string? hud, SaveExportSnapshot expected) =>
        WithMorroGuard(() => ReplaceMorroSessionCore(files, achievements, prompt, hud, expected));

    private SaveMutationResult ReplaceMorroSessionCore(IReadOnlyDictionary<string, byte[]?> files,
        string achievements, string? prompt, string? hud, SaveExportSnapshot expected)
    {
        lock (gate)
        {
            bool started = false;
            try
            {
                ThrowIfDisposed();
                if (morroBackupReloadPending || defaults.StringForKey(IntentKey) != null)
                    return SaveMutationResult.Failed("backup-pending", WithMorroState(CreateExportSnapshotUnsafe()), true);
                SaveExportSnapshot current = WithMorroState(CreateExportSnapshotUnsafe());
                if (current.Generation != expected.Generation || current.LogicalHash != expected.LogicalHash ||
                    current.Achievements != expected.Achievements ||
                    current.Preferences![PromptAlias] != expected.Preferences?[PromptAlias] ||
                    current.Preferences![HudAlias] != expected.Preferences?[HudAlias])
                    return SaveMutationResult.ConflictResult(current);
                if (sizeWarningObserved) throw Failure("userdefaults-size-warning", "A storage size warning was observed.");
                achievements = MorroAchievements.NormalizeBackup(achievements);
                ValidatePrompt(prompt); ValidateHud(hud);
                if (files.Count != Files.Length || Files.Any(policy => !files.ContainsKey(policy.LogicalName)))
                    throw new InvalidDataException("A full backup must specify all four files.");
                List<Entry> entries = new();
                foreach (FilePolicy policy in Files)
                {
                    byte[]? payload = files[policy.LogicalName]?.ToArray();
                    if (payload != null) ValidatePayload(policy, payload);
                    entries.Add(new Entry(policy.LogicalName, payload != null, policy.Serializer, payload ?? Array.Empty<byte>()));
                }
                Snapshot candidate = Snapshot.Create(checked(Math.Max(selected?.Generation ?? 0, HighestStoredGeneration()) + 1), entries, FormatVersion);
                byte[] encoded = Encode(candidate, FormatVersion);
                ValidateEnvelopeLength(encoded.Length);
                Candidate a = ReadCandidate("A"), b = ReadCandidate("B");
                string target = ChooseTarget(a, b);
                ValidateBridgeLength(encoded.Length, target == "A" ? b.RawSize : a.RawSize);
                ValidateBridgeLength(encoded.Length, encoded.Length);
                BackupIntent intent = new(current.LogicalHash, candidate.LogicalHash,
                    current.Achievements!, achievements, current.Preferences![PromptAlias], prompt, current.Preferences![HudAlias], hud);
                string encodedIntent = EncodeIntent(intent);

                // From the first preference write onward, failure is pending:
                // the platform cache may differ from disk until synchronization.
                MorroAchievements.SetBackupMutationBlocked(true);
                externalMutationRequiresRestart = true;
                morroBackupReloadPending = true;
                started = true;
                defaults.SetString(encodedIntent, IntentKey);
                SynchronizeBackup();
                if (defaults.StringForKey(IntentKey) != encodedIntent)
                    throw new IOException("The backup recovery record did not verify.");
                WriteBackupReplica(target, encoded, candidate);
                selected = candidate;
                FinishMorroIntent(intent, candidate, useTarget: true);
                return SaveMutationResult.Committed(WithMorroState(CreateExportSnapshotUnsafe()), encoded.Length, BridgeBytes());
            }
            catch (Exception exception)
            {
                LastFailureCategory = started ? "backup-pending" : Category(exception);
                return SaveMutationResult.Failed(LastFailureCategory, WithMorroState(CreateExportSnapshotUnsafe()), externalMutationRequiresRestart);
            }
        }
    }

    // Invoked before Celeste.Run on a cold start, and before soft reload. The
    // existing A/B base authority decides which side of the intent survived.
    internal void RecoverMorroBackup(bool startup) => WithMorroGuard(() =>
    {
        RecoverMorroBackupCore(startup);
        return true;
    });

    private void RecoverMorroBackupCore(bool startup)
    {
        lock (gate)
        {
            string? encoded = defaults.StringForKey(IntentKey);
            if (encoded == null)
            {
                if (startup) MorroAchievements.SetBackupMutationBlocked(false);
                return;
            }
            MorroAchievements.SetBackupMutationBlocked(true);
            externalMutationRequiresRestart = true;
            morroBackupReloadPending = true;
            BackupIntent intent = DecodeIntent(encoded);
            SynchronizeBackup(); // Do not mistake cached values for acknowledged writes.
            Snapshot? durable = Select(ReadCandidate("A"), ReadCandidate("B"));
            string hash = durable?.LogicalHash ?? "none";
            bool target = hash == intent.TargetHash;
            if (!target && hash != intent.OldHash)
                throw new IOException("The backup recovery record does not match the selected save generation.");
            selected = durable;
            FinishMorroIntent(intent, durable, target);
            if (startup)
            {
                ClearMaterializedFiles();
                if (selected != null) Materialize(selected);
                externalMutationRequiresRestart = false;
                morroBackupReloadPending = false;
                MorroAchievements.SetBackupMutationBlocked(false);
            }
        }
    }

    private void FinishMorroIntent(BackupIntent intent, Snapshot? durable, bool useTarget)
    {
        string achievements = useTarget ? intent.TargetAchievements : intent.OldAchievements;
        if (!MorroAchievements.ReplaceBackup(achievements, preserveRecoveryPolicy: !useTarget))
            throw new IOException("Achievement replacement needs a persistence retry.");
        string? prompt = useTarget ? intent.TargetPrompt : intent.OldPrompt;
        string? hud = useTarget ? intent.TargetHud : intent.OldHud;
        WriteMorroPreferences(prompt, hud);
        if (durable != null)
        {
            byte[] encoded = Encode(durable, FormatVersion);
            // A fallback must never cross back into the previous session.
            WriteBackupReplica("A", encoded, durable);
            WriteBackupReplica("B", encoded, durable);
        }
        restoredPrompt = prompt;
        restoredHud = hud;
        defaults.RemoveObject(IntentKey);
        try { SynchronizeBackup(); }
        catch
        {
            // Keep an in-process retry record even when removal only reached cache.
            defaults.SetString(EncodeIntent(intent), IntentKey);
            throw;
        }
        if (defaults.StringForKey(IntentKey) != null) throw new IOException("Backup recovery completion did not verify.");
    }

    private void WriteBackupReplica(string slot, byte[] encoded, Snapshot expected)
    {
        using (NSString key = new(Key(slot)))
        using (NSData data = NSData.FromArray(encoded)) defaults.SetValueForKey(data, key);
        SynchronizeBackup();
        Candidate readBack = ReadCandidate(slot);
        if (readBack.Snapshot?.Generation != expected.Generation || readBack.Snapshot.LogicalHash != expected.LogicalHash)
            throw new IOException("The restored save generation did not verify.");
    }

    private void SynchronizeBackup()
    {
        if (!defaults.Synchronize()) throw new IOException("Backup storage needs a synchronization retry.");
        if (sizeWarningObserved) throw new IOException("Backup storage reported its size limit.");
    }

    // Game-thread only, after the old scene/SaveData are gone. The ordinary
    // controller preference object is refreshed through its existing hook.
    internal bool ApplyMorroBackupPresentation()
    {
        lock (gate)
        {
            if (!morroBackupReloadPending) return false;
            if (defaults.StringForKey(IntentKey) != null) throw new IOException("Backup recovery is incomplete.");
            TvOSControllerPromptHooks.SetMode((TvOSControllerPromptMode)(int)ControllerPromptPolicy.ParseStored(restoredPrompt));
            TvOSPerformanceHudHooks.SetEnabled(restoredHud == "On");
            WriteMorroPreferences(restoredPrompt, restoredHud);
            MorroAchievements.ClearPresentationAfterBackupRestore();
            return true;
        }
    }

    internal void VerifyMorroBackupReload()
    {
        lock (gate)
        {
            if (!morroBackupReloadPending) return;
            if (defaults.StringForKey(IntentKey) != null || !MorroAchievements.FlushBackup())
                throw new IOException("Backup recovery still needs a persistence retry.");
        }
    }

    internal void CompleteMorroBackupReload()
    {
        lock (gate)
        {
            if (!morroBackupReloadPending) return;
            morroBackupReloadPending = false;
            MorroAchievements.SetBackupMutationBlocked(false);
        }
    }

    // Acquire the achievement bridge gate before the persistence gate: automatic
    // recovery can call UserIO while holding the former. Once blocked, no such
    // callback can enter storage until the operation or full reload completes.
    private T WithMorroGuard<T>(Func<T> operation)
    {
        lock (morroOperationGate)
        {
            bool alreadyBlocked = MorroAchievements.BackupMutationBlocked;
            MorroAchievements.SetBackupMutationBlocked(true);
            try { return operation(); }
            finally
            {
                if (!alreadyBlocked && !morroBackupReloadPending)
                    MorroAchievements.SetBackupMutationBlocked(false);
            }
        }
    }

    private string? ReadPrompt()
    {
        string? value = defaults.StringForKey(ControllerPromptPolicy.PreferenceKey);
        ValidatePrompt(value);
        return value;
    }

    private string? ReadHud()
    {
        string? value = defaults.StringForKey(PerformanceHudPolicy.PreferenceKey);
        ValidateHud(value);
        return value;
    }

    private void WriteMorroPreferences(string? prompt, string? hud)
    {
        if (prompt == null) defaults.RemoveObject(ControllerPromptPolicy.PreferenceKey);
        else defaults.SetString(prompt, ControllerPromptPolicy.PreferenceKey);
        if (hud == null) defaults.RemoveObject(PerformanceHudPolicy.PreferenceKey);
        else defaults.SetString(hud, PerformanceHudPolicy.PreferenceKey);
        SynchronizeBackup();
        if (ReadPrompt() != prompt || ReadHud() != hud) throw new IOException("Restored preferences did not verify.");
    }

    private static (string? Prompt, string? Hud) ValidatePreferences(IReadOnlyDictionary<string, string?> values)
    {
        if (values.Count != 2 || !values.TryGetValue(PromptAlias, out string? prompt) || !values.TryGetValue(HudAlias, out string? hud))
            throw new InvalidDataException("The tvOS preference set is invalid.");
        ValidatePrompt(prompt); ValidateHud(hud);
        return (prompt, hud);
    }

    private static void ValidateHud(string? value)
    {
        if (value is not (null or "Off" or "On")) throw new InvalidDataException("The performance HUD preference is invalid.");
    }

    private static void ValidatePrompt(string? value)
    {
        if (value is not (null or "Automatic" or "Xbox" or "PlayStation" or "NintendoSwitch" or "Stadia"))
            throw new InvalidDataException("The controller prompt preference is invalid.");
    }

    private sealed record BackupIntent(string OldHash, string TargetHash, string OldAchievements,
        string TargetAchievements, string? OldPrompt, string? TargetPrompt, string? OldHud, string? TargetHud);

    private static string EncodeIntent(BackupIntent value)
    {
        using MemoryStream body = new();
        using (BinaryWriter writer = new(body, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("MorroTvOSBackup1");
            writer.Write(value.OldHash); writer.Write(value.TargetHash);
            writer.Write(value.OldAchievements); writer.Write(value.TargetAchievements);
            writer.Write(value.OldPrompt ?? ""); writer.Write(value.TargetPrompt ?? "");
            writer.Write(value.OldHud ?? ""); writer.Write(value.TargetHud ?? "");
        }
        byte[] payload = body.ToArray();
        if (payload.Length > MaximumIntentBytes - 32) throw new InvalidDataException("The backup recovery record is too large.");
        body.Write(SHA256.HashData(payload));
        return Convert.ToBase64String(body.ToArray());
    }

    private static BackupIntent DecodeIntent(string value)
    {
        if (value.Length > MaximumIntentBytes * 2) throw new InvalidDataException("The backup recovery record is too large.");
        byte[] bytes = Convert.FromBase64String(value);
        if (bytes.Length is < 32 or > MaximumIntentBytes ||
            !CryptographicOperations.FixedTimeEquals(SHA256.HashData(bytes.AsSpan(0, bytes.Length - 32)), bytes.AsSpan(bytes.Length - 32)))
            throw new InvalidDataException("The backup recovery record is damaged.");
        using MemoryStream input = new(bytes, 0, bytes.Length - 32, writable: false);
        using BinaryReader reader = new(input, Encoding.UTF8);
        if (reader.ReadString() != "MorroTvOSBackup1") throw new InvalidDataException("The backup recovery version is unsupported.");
        string oldHash = reader.ReadString(), targetHash = reader.ReadString();
        string oldAchievements = reader.ReadString(), targetAchievements = reader.ReadString();
        string oldPrompt = reader.ReadString(), targetPrompt = reader.ReadString();
        string oldHud = reader.ReadString(), targetHud = reader.ReadString();
        if (input.Position != input.Length || !ValidHash(oldHash, allowNone: true) || !ValidHash(targetHash, allowNone: false))
            throw new InvalidDataException("The backup recovery record is malformed.");
        MorroAchievements.ValidateBackup(oldAchievements); MorroAchievements.ValidateBackup(targetAchievements);
        ValidatePrompt(oldPrompt.Length == 0 ? null : oldPrompt); ValidatePrompt(targetPrompt.Length == 0 ? null : targetPrompt);
        ValidateHud(oldHud.Length == 0 ? null : oldHud); ValidateHud(targetHud.Length == 0 ? null : targetHud);
        return new(oldHash, targetHash, oldAchievements, targetAchievements,
            oldPrompt.Length == 0 ? null : oldPrompt, targetPrompt.Length == 0 ? null : targetPrompt,
            oldHud.Length == 0 ? null : oldHud, targetHud.Length == 0 ? null : targetHud);
    }

    private static bool ValidHash(string value, bool allowNone) => allowNone && value == "none" ||
        value.Length == 64 && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}
#endif

using System.Text;
using Celeste;
using CelesteTvOSHost;
using Foundation;
using Morro.Achievements;
using Morro.Backups;

int checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FAIL " + name);
    checks++; Console.WriteLine("PASS " + name);
}
NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
string root = Path.Combine(Path.GetTempPath(), "morro-backup-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    PersistenceStore Start()
    {
        defaults.Reset(); MorroAchievements.NewProcess();
        PersistenceStore store = new(root, "tests");
        store.WriteRawForDiagnostics("A", PersistenceStore.EncodeForDiagnostics(7, new[]
        {
            new PersistenceStore.DiagnosticEntry("settings", true, PersistenceStore.SerializerKind.Settings, "settings:old"u8.ToArray()),
            new PersistenceStore.DiagnosticEntry("0", true, PersistenceStore.SerializerKind.SaveData, "save:old"u8.ToArray()),
            new PersistenceStore.DiagnosticEntry("1", true, PersistenceStore.SerializerKind.SaveData, "save:extra"u8.ToArray()),
            new PersistenceStore.DiagnosticEntry("2", false, PersistenceStore.SerializerKind.SaveData, Array.Empty<byte>())
        }));
        store.Restore(); MorroAchievements.Register("CH1");
        defaults.SetString("Xbox", ControllerPromptPolicy.PreferenceKey); defaults.Synchronize();
        return store;
    }
    string TargetAchievements()
    {
        AchievementLedger target = new(new[] { "CH1", "CH2", "ONEUP" }, _ => null, (_, _) => { });
        target.TryUnlock("ONEUP", 321, flushImmediately: false);
        return target.NormalizeBackup(target.ExportBackup());
    }
    FullBackupState Target(string platform = "tvos") => new("Test Session", platform,
        new Dictionary<string, byte[]?> { ["settings"] = "settings:new"u8.ToArray(), ["0"] = "save:new"u8.ToArray(), ["1"] = null, ["2"] = null },
        TargetAchievements(), new Dictionary<string, string?> { ["controllerPrompts"] = "PlayStation", ["performanceHud"] = "On" });
    byte[] Archive(FullBackupState state) => FullBackupArchive.Create(state, MorroAchievements.ValidateBackup);
    void CompleteReload(PersistenceStore store)
    {
        store.RecoverMorroBackup(startup: false);
        PersistenceStore.ExternalMutationReloadTicket ticket = store.PrepareExternalMutationReload();
        store.ApplyMorroBackupPresentation(); store.VerifyMorroBackupReload();
        store.CompleteExternalMutationReload(ticket); store.CompleteMorroBackupReload();
    }

    using (PersistenceStore store = Start())
    {
        SaveExportSnapshot before = store.CreateMorroSaveManagerSnapshot();
        byte[] exported = store.ExportMorroBackup(before, "Harry's Celeste");
        FullBackupState decoded = FullBackupArchive.Read(exported, MorroAchievements.ValidateBackup);
        Check(decoded.Name == "Harry's Celeste" && decoded.Achievements == before.Achievements && decoded.Files["2"] == null, "named backup carries achievements and explicit absence");
        SaveMutationResult result = store.RestoreMorroBackup(Archive(Target()), before);
        Check(result.Success && result.Snapshot.Generation == before.Generation + 1, "full restore commits exactly one base generation");
        Check(MorroAchievements.Has("ONEUP") && !MorroAchievements.Has("CH1") && !MorroAchievements.AutomaticSaveRecovery, "restore replaces achievements and disables historical backfill");
        Check(store.ExternalMutationRequiresRestart && MorroAchievements.BackupMutationBlocked, "old runtime remains blocked until reload");
        MorroAchievements.Register("CH2");
        Check(!MorroAchievements.Has("CH2"), "stale callbacks cannot contaminate restored session");
        Check(store.Commit("stale") && store.ExportLogicalPayloadForFutureSaveManager("0")!.SequenceEqual("save:new"u8.ToArray()), "stale base capture is suppressed");
        Check(store.ReadRawForDiagnostics("A")!.SequenceEqual(store.ReadRawForDiagnostics("B")!), "both replicas belong to restored session");
        CompleteReload(store);
        Check(!File.Exists(Path.Combine(root, "Saves/1.celeste")) && !File.Exists(Path.Combine(root, "Backups/1.celeste")), "missing slots clear live and previous-good copies");
        Check(!store.ExternalMutationRequiresRestart && !MorroAchievements.BackupMutationBlocked && MorroAchievements.PresentationResets == 1, "verified reload releases both guards and clears presentation");
        Check(TvOSControllerPromptHooks.Mode == TvOSControllerPromptMode.PlayStation, "same-platform controller preference applied");
        Check(TvOSPerformanceHudHooks.Enabled, "same-platform performance HUD preference applied");
        SaveExportSnapshot restored = store.CreateMorroSaveManagerSnapshot();
        SaveMutationResult reset = store.ResetMorroAchievements(restored);
        Check(reset.Success && reset.Snapshot.Files["0"]!.SequenceEqual(restored.Files["0"]!) && !MorroAchievements.Has("ONEUP"), "achievement reset keeps exact vanilla files");
        CompleteReload(store);
        defaults.ColdStart(); MorroAchievements.NewProcess();
        Check(!MorroAchievements.Has("CH1") && !MorroAchievements.Has("ONEUP") && !MorroAchievements.AutomaticSaveRecovery, "empty reset survives cold restart without old replica resurrection");
    }

    using (PersistenceStore store = Start())
    {
        SaveExportSnapshot before = store.CreateMorroSaveManagerSnapshot();
        int writes = defaults.Writes;
        FullBackupState invalid = Target() with { Files = new Dictionary<string, byte[]?>(Target().Files) { ["2"] = "wrong-type"u8.ToArray() } };
        SaveMutationResult bad = store.RestoreMorroBackup(Archive(invalid), before);
        Check(!bad.Success && defaults.Writes == writes && store.Generation == before.Generation, "last invalid member rejects whole batch before writes");
        SaveMutationResult stale = store.RestoreMorroBackup(Archive(Target()), before with { Achievements = TargetAchievements() });
        Check(stale.Conflict && defaults.Writes == writes, "achievement mismatch conflicts without writes");
        FullBackupState unknownPreference = Target() with { Preferences = new Dictionary<string, string?> { ["unrelatedPrivateKey"] = "value" } };
        SaveMutationResult unapproved = store.RestoreMorroBackup(Archive(unknownPreference), before);
        Check(!unapproved.Success && defaults.Writes == writes, "same-platform unknown preference aliases cannot write arbitrary defaults");
        SaveMutationResult cross = store.RestoreMorroBackup(Archive(Target("ios")), before);
        Check(cross.Success && defaults.StringForKey(ControllerPromptPolicy.PreferenceKey) == "Xbox", "cross-platform restore keeps local device preferences");
        CompleteReload(store);
    }

    // Fail every synchronization point both before and after an uncertain disk
    // write, then emulate process death: only disk survives, never cached reads.
    for (int failOffset = 1; failOffset <= 8; failOffset++)
    foreach (bool persistFailed in new[] { false, true })
    {
        PersistenceStore store = Start();
        SaveExportSnapshot before = store.CreateMorroSaveManagerSnapshot();
        defaults.FailAt = defaults.Synchronizations + failOffset;
        defaults.PersistFailedWrite = persistFailed;
        SaveMutationResult result = store.RestoreMorroBackup(Archive(Target()), before);
        Check(!result.Success && result.RestartRequired && MorroAchievements.BackupMutationBlocked,
            $"sync failure {failOffset}/{persistFailed} is pending rather than success");
        defaults.ColdStart(); MorroAchievements.NewProcess();
        using PersistenceStore cold = new(root, "tests");
        cold.Restore(); cold.RecoverMorroBackup(startup: true);
        bool isNew = cold.ExportLogicalPayloadForFutureSaveManager("0")!.SequenceEqual("save:new"u8.ToArray());
        Check(isNew ? MorroAchievements.Has("ONEUP") && !MorroAchievements.Has("CH1") && defaults.StringForKey(ControllerPromptPolicy.PreferenceKey) == "PlayStation"
            : MorroAchievements.Has("CH1") && !MorroAchievements.Has("ONEUP") && defaults.StringForKey(ControllerPromptPolicy.PreferenceKey) == "Xbox",
            $"cold recovery {failOffset}/{persistFailed} keeps one complete session");
        Check(!MorroAchievements.BackupMutationBlocked && !cold.ExternalMutationRequiresRestart,
            $"cold recovery {failOffset}/{persistFailed} completes guards");
        // The old object represents a dead process; do not dispose/flush it.
    }

    using (PersistenceStore store = Start())
    {
        SaveExportSnapshot before = store.CreateMorroSaveManagerSnapshot();
        defaults.FailAt = defaults.Synchronizations + 3;
        SaveMutationResult result = store.RestoreMorroBackup(Archive(Target()), before);
        Check(!result.Success, "achievement cached-write failure remains pending");
        int writes = defaults.Writes;
        defaults.FailAt = -1;
        CompleteReload(store);
        Check(defaults.Writes > writes && MorroAchievements.Has("ONEUP") && !MorroAchievements.Has("CH1"), "same-process retry performs writes and finishes exact replacement");
    }
    using (PersistenceStore store = Start())
    {
        SaveExportSnapshot before = store.CreateMorroSaveManagerSnapshot();
        defaults.FailAt = defaults.Synchronizations + 2;
        SaveMutationResult result = store.ResetMorroAchievements(before);
        Check(!result.Success && result.RestartRequired, "same-base achievement reset can remain pending");
        defaults.ColdStart(); MorroAchievements.NewProcess();
        using PersistenceStore cold = new(root, "tests");
        cold.Restore(); cold.RecoverMorroBackup(startup: true);
        Check(!MorroAchievements.Has("CH1") && !MorroAchievements.AutomaticSaveRecovery &&
            cold.ExportLogicalPayloadForFutureSaveManager("0")!.SequenceEqual(before.Files["0"]!), "durable reset intent completes exact achievements with unchanged saves");
    }
    using (PersistenceStore store = Start())
    {
        _ = store.CreateMorroSaveManagerSnapshot();
        defaults.SetString("damaged", "CelesteTvOS.Persistence.Tests.v1.MorroBackupIntent.v1"); defaults.Synchronize();
        bool rejected = false;
        try { store.RecoverMorroBackup(startup: false); }
        catch (Exception exception) when (exception is InvalidDataException or FormatException) { rejected = true; }
        Check(rejected && MorroAchievements.BackupMutationBlocked && store.ExternalMutationRequiresRestart,
            "damaged journal refuses to release either mutation guard");
    }
    Console.WriteLine($"MORRO_TVOS_BACKUPS PASS checks={checks}");
}
finally { Directory.Delete(root, recursive: true); }

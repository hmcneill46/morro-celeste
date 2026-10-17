using System.Collections.Concurrent;
using System.Text;
using CelesteIOSFoundation;

int passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException($"FAIL: {name}");
    passed++;
}

static byte[] Settings(int value) => Encoding.UTF8.GetBytes($"<Settings><Music>{value}</Music></Settings>");
static byte[] Save(int value) => Encoding.UTF8.GetBytes($"<SaveData><Deaths>{value}</Deaths></SaveData>");

string root = Path.Combine(Path.GetTempPath(), "celeste-ios-durability-tests", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    TestBackend backend = new(root);
    TestValidator validator = new();
    CelesteFileDurabilityStore store = new(backend, validator);

    Check(CelesteFileDurabilityStore.IsApprovedLogicalName("settings"), "settings allow-listed");
    Check(CelesteFileDurabilityStore.IsApprovedLogicalName("0"), "slot 0 allow-listed");
    Check(CelesteFileDurabilityStore.IsApprovedLogicalName("1"), "slot 1 allow-listed");
    Check(CelesteFileDurabilityStore.IsApprovedLogicalName("2"), "slot 2 allow-listed");
    Check(!CelesteFileDurabilityStore.IsApprovedLogicalName("3"), "slot 3 rejected");
    Check(!CelesteFileDurabilityStore.IsApprovedLogicalName("../0"), "traversal rejected");
    Check(!CelesteFileDurabilityStore.IsApprovedLogicalName("0.celeste"), "physical name rejected");

    CelesteFileCommitResult first = store.Commit("settings", Settings(1));
    Check(first.Changed && !first.PreviousGoodUpdated, "first Settings write");
    Check(backend.CountCopies("settings") == 1, "first write has primary only");
    Check(store.Load("settings").Data!.SequenceEqual(Settings(1)), "first Settings read");

    CelesteFileCommitResult replacement = store.Commit("settings", Settings(2));
    Check(replacement.Changed && replacement.PreviousGoodUpdated, "Settings replacement rotates previous good");
    Check(backend.Read("settings", CelesteFileCopy.Primary)!.SequenceEqual(Settings(2)), "replacement primary exact");
    Check(backend.Read("settings", CelesteFileCopy.PreviousGood)!.SequenceEqual(Settings(1)), "previous-good Settings exact");
    Check(backend.CountCopies("settings") == 2, "Settings copies bounded to two");

    CelesteFileCommitResult unchanged = store.Commit("settings", Settings(2));
    Check(!unchanged.Changed && !unchanged.PreviousGoodUpdated, "identical Settings write no-op");
    Check(backend.AtomicWriteCount == 3, "identical write performs no atomic replacement");

    CelesteFileRestoreResult restored = store.RestorePreviousGood("settings");
    Check(restored.Restored && restored.Reversible, "previous-good restore is reversible");
    Check(store.Load("settings").Data!.SequenceEqual(Settings(1)), "restore installs exact previous bytes");
    Check(store.LoadPreviousGood("settings")!.SequenceEqual(Settings(2)), "restore rotates current into previous-good");
    CelesteFileRestoreResult undone = store.RestorePreviousGood("settings");
    Check(undone.Restored && undone.Reversible, "second restore acts as undo");
    Check(store.Load("settings").Data!.SequenceEqual(Settings(2)), "restore undo recovers original current bytes");
    store.Delete("settings");
    Check(!store.RestorePreviousGood("settings").Restored, "missing previous-good restore is a no-op");
    store.Commit("settings", Settings(2));
    store.Commit("settings", Settings(3));
    backend.NextFault = TestFault.PrimaryWriteFailure;
    try { store.RestorePreviousGood("settings"); } catch (IOException) { passed++; }
    Check(store.Load("settings").Data!.SequenceEqual(Settings(3)),
        "restore primary failure preserves current primary");
    Check(store.LoadPreviousGood("settings")!.SequenceEqual(Settings(2)),
        "restore primary failure preserves previous-good recovery point");
    backend.NextFault = TestFault.BackupWriteFailure;
    CelesteFileRestoreResult nonReversibleRestore = store.RestorePreviousGood("settings");
    Check(nonReversibleRestore.Restored && !nonReversibleRestore.Reversible,
        "restore remains successful when only undo rotation fails");
    Check(store.Load("settings").Data!.SequenceEqual(Settings(2)),
        "restore verifies primary before attempting undo rotation");

    foreach (string slot in new[] { "0", "1", "2" })
    {
        int value = int.Parse(slot) + 10;
        Check(store.Commit(slot, Save(value)).Changed, $"slot {slot} first write");
        Check(store.Load(slot).Data!.SequenceEqual(Save(value)), $"slot {slot} readable");
        Check(store.Commit(slot, Save(value + 10)).PreviousGoodUpdated, $"slot {slot} replacement backup");
        Check(backend.Read(slot, CelesteFileCopy.PreviousGood)!.SequenceEqual(Save(value)), $"slot {slot} previous good exact");
    }

    backend.Corrupt("0", CelesteFileCopy.Primary, "garbage"u8.ToArray());
    CelesteFileLoadResult corruptRecovery = store.Load("0");
    Check(corruptRecovery.RecoveredFromPreviousGood, "corrupt save primary recovers");
    Check(corruptRecovery.PrimaryWasInvalid, "corrupt primary reported");
    Check(corruptRecovery.Data!.SequenceEqual(Save(10)), "corrupt save recovery bytes exact");
    Check(backend.Read("0", CelesteFileCopy.Primary)!.SequenceEqual(Save(10)), "corrupt primary repaired");

    backend.Delete("1", CelesteFileCopy.Primary);
    CelesteFileLoadResult missingRecovery = store.Load("1");
    Check(missingRecovery.RecoveredFromPreviousGood, "missing save primary recovers");
    Check(!missingRecovery.PrimaryWasInvalid, "missing primary distinguished from invalid");
    Check(missingRecovery.Data!.SequenceEqual(Save(11)), "missing primary recovery exact");

    backend.Corrupt("2", CelesteFileCopy.Primary, Array.Empty<byte>());
    backend.Corrupt("2", CelesteFileCopy.PreviousGood, "<broken>"u8.ToArray());
    CelesteFileLoadResult bothInvalid = store.Load("2");
    Check(bothInvalid.Data is null, "both invalid returns missing semantics");
    Check(bothInvalid.PrimaryWasInvalid && bothInvalid.PreviousGoodWasInvalid, "both invalid reported");
    Check(!store.Exists("2"), "both invalid does not claim slot exists");

    byte[] stable = Save(100);
    store.Commit("2", stable);
    try { store.Commit("2", "not XML"u8.ToArray()); } catch (InvalidDataException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "invalid candidate rejected before mutation");
    try { store.Commit("2", Array.Empty<byte>()); } catch (InvalidDataException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "zero-byte candidate rejected before mutation");

    backend.NextFault = TestFault.BeforeTemporaryCreation;
    try { store.Commit("2", Save(101)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "pre-temporary fault preserves primary");
    Check(!backend.HasTemps, "pre-temporary fault leaves no temp");

    backend.NextFault = TestFault.AfterTemporaryPrepared;
    try { store.Commit("2", Save(102)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "pre-commit fault preserves primary");
    Check(!backend.HasTemps, "pre-commit fault cleaned on next load");

    backend.NextFault = TestFault.BackupWriteFailure;
    try { store.Commit("2", Save(103)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "backup failure blocks primary replacement");

    backend.NextFault = TestFault.PrimaryReadFailure;
    try { store.Commit("2", Save(103)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "primary read failure aborts before mutation");

    backend.NextFault = TestFault.BackupReadFailure;
    try { store.Commit("2", Save(103)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "backup read failure aborts before mutation");

    backend.NextFault = TestFault.PrimaryWriteFailure;
    try { store.Commit("2", Save(104)); } catch (IOException) { passed++; }
    Check(store.Load("2").Data!.SequenceEqual(stable), "primary replacement failure recovers prior state");
    Check(validator.IsValid("2", backend.Read("2", CelesteFileCopy.PreviousGood)!), "primary failure leaves valid backup");

    backend.NextFault = TestFault.AfterCommitCleanupFailure;
    CelesteFileCommitResult cleanupFault = store.Commit("2", Save(105));
    Check(cleanupFault.Changed && !cleanupFault.CleanupSucceeded, "post-commit cleanup fault reports durable success");
    Check(store.Load("2").Data!.SequenceEqual(Save(105)), "post-commit cleanup fault has exact new primary");
    Check(!backend.HasTemps, "subsequent load clears orphan temp");

    store.Commit("0", Save(200));
    ConcurrentBag<Exception> failures = new();
    Parallel.For(201, 233, value =>
    {
        try { store.Commit("0", Save(value)); }
        catch (Exception exception) { failures.Add(exception); }
    });
    Check(failures.IsEmpty, "concurrent saves serialize without failure");
    Check(validator.IsValid("0", store.Load("0").Data!), "concurrent final primary valid");
    Check(validator.IsValid("0", backend.Read("0", CelesteFileCopy.PreviousGood)!), "concurrent previous-good valid");
    Check(backend.CountCopies("0") == 2, "concurrent writes remain bounded");

    for (int value = 300; value < 700; value++) store.Commit("1", Save(value));
    Check(backend.CountCopies("1") == 2, "400-save stress remains bounded");
    Check(!backend.HasTemps, "400-save stress leaves no temp files");
    Check(store.Load("1").Data!.SequenceEqual(Save(699)), "400-save stress latest primary exact");
    Check(backend.Read("1", CelesteFileCopy.PreviousGood)!.SequenceEqual(Save(698)), "400-save stress previous-good exact");

    Check(store.Delete("1"), "delete reports existing state");
    Check(backend.CountCopies("1") == 0, "delete removes primary and backup");
    Check(!store.Delete("1"), "repeat delete reports absent");

    try { store.Load("../settings"); } catch (ArgumentException) { passed++; }
    try { store.Delete("/tmp/0"); } catch (ArgumentException) { passed++; }

    Dictionary<string, byte[]?> sessionA = new(StringComparer.Ordinal)
    { ["settings"] = Settings(8), ["0"] = Save(800), ["1"] = Save(801), ["2"] = Save(802) };
    Dictionary<string, byte[]?> sessionB = new(StringComparer.Ordinal)
    { ["settings"] = Settings(9), ["0"] = Save(900), ["1"] = null, ["2"] = null };
    void VerifySession(IReadOnlyDictionary<string, byte[]?> expected, string label)
    {
        foreach ((string name, byte[]? bytes) in expected)
            foreach (CelesteFileCopy copy in Enum.GetValues<CelesteFileCopy>())
            {
                byte[]? actual = backend.Read(name, copy);
                Check(bytes is null ? actual is null : actual is not null && actual.AsSpan().SequenceEqual(bytes),
                    $"{label} exact {name}/{copy}");
            }
    }
    store.SetSessionReplacementPending(true);
    store.ReplaceSessionFiles(sessionA);
    VerifySession(sessionA, "session A");
    store.ReplaceSessionFiles(sessionB);
    VerifySession(sessionB, "session B clears missing slots and previous-good");
    store.ReplaceSessionFiles(sessionA);
    VerifySession(sessionA, "session A restored independently");
    int beforeInvalid = backend.AtomicWriteCount;
    Dictionary<string, byte[]?> invalid = new(sessionB) { ["0"] = Settings(99) };
    try { store.ReplaceSessionFiles(invalid); } catch (InvalidDataException) { passed++; }
    Check(backend.AtomicWriteCount == beforeInvalid, "invalid complete session causes no writes");
    invalid.Remove("2");
    try { store.ReplaceSessionFiles(invalid); } catch (InvalidDataException) { passed++; }
    Check(backend.AtomicWriteCount == beforeInvalid, "incomplete session causes no writes");
    try { store.Commit("0", Save(999)); } catch (IOException) { passed++; }
    try { store.Delete("0"); } catch (IOException) { passed++; }
    try { store.RestorePreviousGood("0"); } catch (IOException) { passed++; }
    try { store.Load("0"); } catch (IOException) { passed++; }
    VerifySession(sessionA, "pending journal blocks old state access");
    for (int failureAt = 1; failureAt <= 8; failureAt++)
    {
        store.ReplaceSessionFiles(sessionA);
        backend.FailMutationAt = backend.MutationAttemptCount + failureAt;
        bool faulted = false;
        try { store.ReplaceSessionFiles(sessionB); } catch (IOException) { faulted = true; }
        Check(faulted, $"session target fault {failureAt} reported");
        // The durable host journal still contains B. A fresh authority replays
        // that same target before any load, repair or old gameplay write.
        store = new CelesteFileDurabilityStore(backend, validator);
        store.SetSessionReplacementPending(true);
        store.ReplaceSessionFiles(sessionB);
        VerifySession(sessionB, $"cold target replay after fault {failureAt}");
    }
    store.SetSessionReplacementPending(false);
    Check(store.Load("1").Data is null && store.LoadPreviousGood("1") is null,
        "missing slot cannot resurrect from outgoing session");
    IReadOnlyDictionary<string, byte[]?> snapshot = store.CreateSnapshot();
    Check(snapshot.Count == 4 && snapshot["1"] is null && snapshot["2"] is null,
        "snapshot explicitly records absent slots");
    snapshot["0"]![0] = 0;
    Check(store.Load("0").Data!.SequenceEqual(Save(900)), "snapshot cannot mutate live state");

    string escape = Path.Combine(root, "escape");
    Directory.CreateDirectory(escape);
    string link = Path.Combine(root, "symlink-backend");
    Directory.CreateSymbolicLink(link, escape);
    TestBackend symlinkBackend = new(link);
    CelesteFileDurabilityStore symlinkStore = new(symlinkBackend, validator);
    try { symlinkStore.Commit("0", Save(1)); } catch (UnauthorizedAccessException) { passed++; }
    Check(!Directory.EnumerateFiles(escape).Any(), "symlink root cannot escape storage boundary");

    Check(CelesteFileDurabilityStore.MaximumLogicalFileBytes == 64 * 1024 * 1024,
        "iOS sanity limit is not the tvOS persistence budget");
}
finally
{
    if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
}

Console.WriteLine($"PASS: modern iOS durability deterministic tests {passed}");

internal sealed class TestValidator : ICelesteLogicalFileValidator
{
    public bool IsValid(string logicalName, ReadOnlyMemory<byte> data)
    {
        string value = Encoding.UTF8.GetString(data.Span);
        return logicalName == "settings"
            ? value.StartsWith("<Settings>", StringComparison.Ordinal) && value.EndsWith("</Settings>", StringComparison.Ordinal)
            : CelesteFileDurabilityStore.IsApprovedLogicalName(logicalName) && logicalName != "settings" &&
              value.StartsWith("<SaveData>", StringComparison.Ordinal) && value.EndsWith("</SaveData>", StringComparison.Ordinal);
    }
}

internal enum TestFault
{
    None,
    BeforeTemporaryCreation,
    AfterTemporaryPrepared,
    BackupWriteFailure,
    PrimaryWriteFailure,
    PrimaryReadFailure,
    BackupReadFailure,
    AfterCommitCleanupFailure,
}

internal sealed class TestBackend : ICelesteFileDurabilityBackend
{
    private readonly string root;
    internal TestFault NextFault { get; set; }
    internal int AtomicWriteCount { get; private set; }
    internal int MutationAttemptCount { get; private set; }
    internal int FailMutationAt { get; set; } = -1;
    private void CheckMutationFault()
    {
        if (++MutationAttemptCount == FailMutationAt) throw new IOException("Injected session mutation fault.");
    }
    internal bool HasTemps => Directory.Exists(root) && Directory.EnumerateFiles(root, "*.tmp", SearchOption.AllDirectories).Any();

    internal TestBackend(string root) => this.root = Path.GetFullPath(root);

    public byte[]? Read(string logicalName, CelesteFileCopy copy)
    {
        if ((NextFault == TestFault.PrimaryReadFailure && copy == CelesteFileCopy.Primary) ||
            (NextFault == TestFault.BackupReadFailure && copy == CelesteFileCopy.PreviousGood))
        {
            NextFault = TestFault.None;
            throw new IOException("Injected read failure.");
        }
        string path = Resolve(logicalName, copy);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    public void WriteAtomic(string logicalName, CelesteFileCopy copy, ReadOnlyMemory<byte> data)
    {
        CheckMutationFault();
        string path = Resolve(logicalName, copy);
        string directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        TestFault fault = NextFault;
        bool targetedFailure =
            (fault == TestFault.BackupWriteFailure && copy == CelesteFileCopy.PreviousGood) ||
            (fault == TestFault.PrimaryWriteFailure && copy == CelesteFileCopy.Primary) ||
            fault == TestFault.BeforeTemporaryCreation;
        if (targetedFailure)
        {
            NextFault = TestFault.None;
            throw new IOException("Injected atomic write failure.");
        }

        string temporary = Path.Combine(directory, $".{Path.GetFileName(path)}.owned.tmp");
        File.WriteAllBytes(temporary, data.ToArray());
        if (fault == TestFault.AfterTemporaryPrepared)
        {
            NextFault = TestFault.None;
            throw new IOException("Injected failure after replacement preparation.");
        }

        File.Move(temporary, path, overwrite: true);
        AtomicWriteCount++;
        if (fault == TestFault.AfterCommitCleanupFailure && copy == CelesteFileCopy.Primary)
        {
            File.WriteAllBytes(temporary, "orphan"u8.ToArray());
            NextFault = TestFault.None;
            throw new IOException("Injected post-commit cleanup failure.");
        }
        if (fault is not TestFault.PrimaryWriteFailure and not TestFault.AfterCommitCleanupFailure &&
            !(fault == TestFault.BackupWriteFailure && copy == CelesteFileCopy.Primary))
            NextFault = TestFault.None;
    }

    public void Delete(string logicalName, CelesteFileCopy copy)
    {
        CheckMutationFault();
        string path = Resolve(logicalName, copy);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool CleanupOwnedTemporaryFiles(string logicalName)
    {
        foreach (CelesteFileCopy copy in Enum.GetValues<CelesteFileCopy>())
        {
            string path = Resolve(logicalName, copy);
            string temporary = Path.Combine(Path.GetDirectoryName(path)!, $".{Path.GetFileName(path)}.owned.tmp");
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        return true;
    }

    internal void Corrupt(string logicalName, CelesteFileCopy copy, byte[] data)
    {
        string path = Resolve(logicalName, copy);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, data);
    }

    internal int CountCopies(string logicalName) => Enum.GetValues<CelesteFileCopy>()
        .Count(copy => File.Exists(Resolve(logicalName, copy)));

    private string Resolve(string logicalName, CelesteFileCopy copy)
    {
        if (!CelesteFileDurabilityStore.IsApprovedLogicalName(logicalName))
            throw new ArgumentException("Unapproved logical name.", nameof(logicalName));
        if (new DirectoryInfo(root).LinkTarget is not null)
            throw new UnauthorizedAccessException("Symlink storage root rejected.");
        string directory = Path.Combine(root, copy == CelesteFileCopy.Primary ? "Saves" : "Backups");
        string path = Path.GetFullPath(Path.Combine(directory, logicalName + ".celeste"));
        string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Storage path escaped root.");
        return path;
    }
}

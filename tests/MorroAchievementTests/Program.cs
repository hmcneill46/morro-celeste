using System.Security.Cryptography;
using System.Text;
using Morro.Achievements;

int passed = 0;
string[] ids = ["CH1", "STRB1", "PICO8", "ONEUP"];
const string prefix = "Morro.Achievements.v1";

void Test(string name, Action action)
{
    action();
    Console.WriteLine($"PASS {++passed:D2}: {name}");
}
void Check(bool condition) { if (!condition) throw new InvalidOperationException("Assertion failed."); }
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
AchievementLedger Ledger(MemoryStore store) => new(ids, store.Read, store.Write);
string Record(string body) => body + "\n" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));

Test("new installation does not write preferences", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    Check(ledger.UnlockedCount == 0 && !ledger.IsDirty && ledger.Flush() && store.Writes == 0);
});
Test("unlock is immediate, durable and deduplicated", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    Check(ledger.TryUnlock("CH1", 123));
    Check(ledger.Has("CH1") && ledger.UnlockedAtUtcSeconds("CH1") == 123 && !ledger.IsDirty);
    Check(!ledger.TryUnlock("CH1", 456) && notifications == 1 && store.Writes == 2);
    AchievementLedger restored = Ledger(store);
    restored.Unlocked += _ => notifications++;
    Check(restored.Has("CH1") && restored.UnlockedAtUtcSeconds("CH1") == 123 && notifications == 1);
});
Test("backfill preserves unknown dates without notifications", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    Check(ledger.TryUnlock("STRB1", 0, notify: false));
    Check(!ledger.TryUnlock("STRB1", 55) && ledger.UnlockedAtUtcSeconds("STRB1") == 0);
    Check(Ledger(store).UnlockedAtUtcSeconds("STRB1") == 0 && notifications == 0);
});
Test("deferred unlock batch writes two replicas once and retains failed retry state", () =>
{
    MemoryStore store = new() { FailWrites = true };
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    foreach (string id in ids) ledger.TryUnlock(id, 0, notify: false, flushImmediately: false);
    Check(ledger.UnlockedCount == ids.Length && ledger.IsDirty && store.Writes == 0 && notifications == 0);
    Check(!ledger.Flush() && ledger.IsDirty && store.Writes == 1);
    store.FailWrites = false;
    Check(ledger.Flush() && !ledger.IsDirty && store.Writes == 3 && notifications == 0);
    Check(Ledger(store).UnlockedCount == ids.Length);
});
Test("default date is a UTC Unix timestamp", () =>
{
    AchievementLedger ledger = Ledger(new());
    long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    ledger.TryUnlock("CH1");
    long? actual = ledger.UnlockedAtUtcSeconds("CH1");
    Check(actual >= before && actual <= DateTimeOffset.UtcNow.ToUnixTimeSeconds());
});
Test("unknown IDs and invalid dates fail before mutation", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    Throws<ArgumentException>(() => ledger.TryUnlock("OTHER"));
    Throws<ArgumentException>(() => ledger.Has("ch1"));
    Throws<ArgumentException>(() => ledger.UnlockedAtUtcSeconds("OTHER"));
    Throws<ArgumentOutOfRangeException>(() => ledger.TryUnlock("CH1", -1));
    Throws<ArgumentOutOfRangeException>(() => ledger.TryUnlock("CH1", 253402300800));
    Check(ledger.UnlockedCount == 0 && store.Writes == 0);
});
Test("catalog must be bounded, safe and unique", () =>
{
    MemoryStore store = new();
    Throws<ArgumentException>(() => new AchievementLedger([], store.Read, store.Write));
    Throws<ArgumentException>(() => new AchievementLedger(["CH1", "CH1"], store.Read, store.Write));
    Throws<ArgumentException>(() => new AchievementLedger(["bad|id"], store.Read, store.Write));
    Throws<ArgumentException>(() => new AchievementLedger(Enumerable.Range(0, 65).Select(i => "ID" + i).ToArray(), store.Read, store.Write));
});
Test("two valid replicas merge unlocks despite generation order", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|99|CH1=20");
    store.Data[prefix + ".B"] = Record("MORROACH|1|2|STRB1=10");
    AchievementLedger ledger = Ledger(store);
    Check(ledger.Has("CH1") && ledger.Has("STRB1") && store.Writes == 0);
    ledger.TryUnlock("ONEUP", 30);
    Check(store.Data[prefix + ".B"].StartsWith("MORROACH|1|100|", StringComparison.Ordinal));
    Check(Ledger(store).UnlockedCount == 3);
});
Test("conflicting replica dates retain earliest or unknown date", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|1|CH1=20;STRB1=5");
    store.Data[prefix + ".B"] = Record("MORROACH|1|2|CH1=10;STRB1=0");
    AchievementLedger ledger = Ledger(store);
    Check(ledger.UnlockedAtUtcSeconds("CH1") == 10 && ledger.UnlockedAtUtcSeconds("STRB1") == 0);
});
Test("one corrupt replica cannot lose the latest acknowledged unlock", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 1);
    ledger.TryUnlock("STRB1", 2);
    store.Data[prefix + ".B"] += "corrupt";
    AchievementLedger restored = Ledger(store);
    Check(restored.Has("CH1") && restored.Has("STRB1"));
    restored.TryUnlock("ONEUP", 3);
    Check(Ledger(store).Has("CH1") && Ledger(store).Has("ONEUP"));
});
foreach ((string name, string value) in new (string, string)[]
{
    ("truncated", "MORROACH|1|1|CH1=1"),
    ("checksum mismatch", Record("MORROACH|1|1|CH1=1").Replace("CH1=1", "CH1=2", StringComparison.Ordinal)),
    ("schema", Record("MORROACH|2|1|CH1=1")),
    ("unknown ID", Record("MORROACH|1|1|OTHER=1")),
    ("duplicate ID", Record("MORROACH|1|1|CH1=1;CH1=2")),
    ("negative date", Record("MORROACH|1|1|CH1=-1")),
    ("date overflow", Record("MORROACH|1|1|CH1=253402300800")),
    ("zero generation", Record("MORROACH|1|0|CH1=1")),
    ("generation overflow", Record("MORROACH|1|9223372036854775808|CH1=1")),
    ("trailing separator", Record("MORROACH|1|1|CH1=1;")),
    ("oversized", new string('x', AchievementLedger.MaximumRecordCharacters + 1)),
    ("extra field", Record("MORROACH|1|1|CH1=1|oops")),
})
{
    Test($"reject {name} without restoring an unlock", () =>
    {
        MemoryStore store = new();
        store.Data[prefix + ".A"] = value;
        Check(Ledger(store).UnlockedCount == 0 && store.Writes == 0);
    });
}
Test("failed write retains in-memory unlock and retries without a second event", () =>
{
    MemoryStore store = new() { FailWrites = true };
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    Check(ledger.TryUnlock("CH1", 1) && ledger.Has("CH1") && ledger.IsDirty);
    Check(!ledger.Flush() && !ledger.TryUnlock("CH1", 2) && notifications == 1);
    store.FailWrites = false;
    Check(ledger.Flush() && !ledger.IsDirty && notifications == 1 && Ledger(store).Has("CH1"));
});
Test("readback mismatch stays dirty and does not damage previous unlock", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 1);
    store.DropWrites = true;
    ledger.TryUnlock("STRB1", 2);
    Check(ledger.IsDirty && Ledger(store).Has("CH1"));
    store.DropWrites = false;
    Check(ledger.Flush() && Ledger(store).UnlockedCount == 2);
});
Test("post-write exception stays pending despite matching readback", () =>
{
    MemoryStore store = new() { ThrowAfterWrite = true };
    AchievementLedger ledger = Ledger(store);
    Check(ledger.TryUnlock("CH1", 1) && ledger.IsDirty && ledger.Has("CH1"));
    int writes = store.Writes;
    store.ThrowAfterWrite = false;
    Check(ledger.Flush() && !ledger.IsDirty && store.Writes == writes + 2);
    Check(Ledger(store).Has("CH1"));
});
Test("cached unsynchronized records never masquerade as cold durable state", () =>
{
    CachedPreferences store = new() { SynchronizeFails = true };
    AchievementLedger ledger = new(ids, store.Read, store.Write);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    Check(ledger.TryUnlock("CH1", 1) && ledger.IsDirty && ledger.Has("CH1"));
    Check(store.Cache.Count == 1 && store.Disk.Count == 0);
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).UnlockedCount == 0);
    Check(!ledger.Flush() && ledger.IsDirty && store.Cache.Count == 2);
    int writes = store.Writes;
    store.SynchronizeFails = false;
    Check(ledger.Flush() && !ledger.IsDirty && notifications == 1 && store.Writes == writes + 2);
    Check(store.WrittenKeys.Skip(writes).Distinct().Count() == 2);
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).Has("CH1"));
    Check(store.Disk.Count == 2);
});
Test("second replica sync failure keeps a cold recovery copy and requires both retries", () =>
{
    CachedPreferences store = new() { FailOnWrite = 2 };
    AchievementLedger ledger = new(ids, store.Read, store.Write);
    Check(ledger.TryUnlock("CH1", 1) && ledger.IsDirty && store.Writes == 2);
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).Has("CH1"));
    Check(store.Disk.Count == 1 && store.Cache.Count == 2);
    store.FailOnWrite = 0;
    Check(ledger.Flush() && !ledger.IsDirty && store.Writes == 4);
    Check(store.WrittenKeys.Skip(2).Distinct().Count() == 2);
    Check(store.Disk.Count == 2 && new AchievementLedger(ids, store.ReadDisk, store.Write).Has("CH1"));
});
Test("temporary unreadable replica is not overwritten", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|5|CH1=1");
    store.FailReads = true;
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("STRB1", 2);
    Check(ledger.IsDirty && store.Writes == 0);
    store.FailReads = false;
    Check(ledger.Flush() && Ledger(store).UnlockedCount == 2);
});
Test("initial read failure retries restore without requiring a new unlock", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|5|CH1=1");
    store.FailReads = true;
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => notifications++;
    Check(ledger.IsDirty && !ledger.Flush() && store.Writes == 0);
    store.FailReads = false;
    Check(ledger.Flush() && ledger.Has("CH1") && !ledger.IsDirty && notifications == 0);
});
Test("generation exhaustion preserves pending unlock without overflow", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|9223372036854775807|CH1=1");
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("STRB1", 2);
    Check(ledger.Has("CH1") && ledger.Has("STRB1") && ledger.IsDirty && store.Writes == 0);
});
Test("snapshots cannot mutate ledger state", () =>
{
    AchievementLedger ledger = Ledger(new());
    ledger.TryUnlock("CH1", 1);
    IReadOnlyDictionary<string, long> snapshot = ledger.Snapshot();
    ledger.TryUnlock("STRB1", 2);
    Check(snapshot.Count == 1 && ledger.UnlockedCount == 2);
});
Test("parallel duplicate unlocks commit and notify once", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    int notifications = 0;
    ledger.Unlocked += _ => Interlocked.Increment(ref notifications);
    Parallel.For(0, 100, _ => ledger.TryUnlock("CH1", 1));
    Check(ledger.UnlockedCount == 1 && notifications == 1 && store.Writes == 2);
});
Test("all maximum catalog entries fit the bounded format", () =>
{
    MemoryStore store = new();
    string[] fullIds = Enumerable.Range(0, 64).Select(i => new string('A', 61) + i.ToString("D3")).ToArray();
    AchievementLedger ledger = new(fullIds, store.Read, store.Write);
    foreach (string id in fullIds) ledger.TryUnlock(id, 253402300799, notify: false);
    Check(store.Data.Values.All(value => value.Length <= AchievementLedger.MaximumRecordCharacters));
    Check(new AchievementLedger(fullIds, store.Read, store.Write).UnlockedCount == 64);
});
Test("portable export is canonical, bounded and independent of local keys and generations", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("PICO8", 42, notify: false);
    ledger.TryUnlock("CH1", 0, notify: false);
    int writes = store.Writes;
    string backup = ledger.ExportBackup();
    ledger.ValidateBackup(backup);
    Check(backup.Length <= 8192 && backup.Contains("CH1=0;PICO8=42"));
    Check(!backup.Contains(prefix) && !backup.Contains("production") && store.Writes == writes);
    AchievementLedger other = new(ids, _ => null, (_, _) => { }, "Other.Scope");
    other.ValidateBackup(backup);
    Throws<ArgumentException>(() => new AchievementLedger(["CH1"], _ => null, (_, _) => { }).ValidateBackup(backup));
});
Test("normalization changes only recovery policy and explicit restore defaults to false", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 12);
    string prior = ledger.ExportBackup();
    string normalized = ledger.NormalizeBackup(prior);
    Check(prior != normalized && ledger.NormalizeBackup(normalized, true) == prior);
    Check(ledger.ReplaceBackup(prior) && ledger.ExportBackup() == normalized && !ledger.AutomaticSaveRecovery);
    Check(ledger.ReplaceBackup(prior, preserveRecoveryPolicy: true));
    Check(ledger.ExportBackup() == prior && Ledger(store).AutomaticSaveRecovery);
});
Test("exact replacement can remove unlocks and ignores old replicas regardless of generation", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 0);
    string smaller = ledger.ExportBackup();
    ledger.TryUnlock("ONEUP", 23);
    int events = 0;
    ledger.Unlocked += _ => events++;
    Check(ledger.ReplaceBackup(smaller) && ledger.UnlockedCount == 1 && events == 0);
    store.Data[prefix + ".B"] = Record("MORROACH|1|9223372036854775807|ONEUP=1");
    AchievementLedger cold = Ledger(store);
    Check(cold.Has("CH1") && !cold.Has("ONEUP") && !cold.AutomaticSaveRecovery && cold.IsDirty);
    Check(cold.Flush() && Ledger(store).UnlockedCount == 1);
});
Test("a second empty epoch cannot union either legacy or prior replacement progress", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 7);
    Check(ledger.ReplaceBackup(ledger.ExportBackup()));
    string oldEpoch = store.Data[prefix + ".A"];
    Check(ledger.ReplaceBackup(ledger.EmptyBackup()) && ledger.UnlockedCount == 0);
    store.Data[prefix + ".B"] = oldEpoch;
    AchievementLedger cold = Ledger(store);
    Check(cold.UnlockedCount == 0 && !cold.AutomaticSaveRecovery && cold.Flush());
    Check(store.Data.Values.All(value => value.StartsWith("MORROACH|2|2|", StringComparison.Ordinal)));
});
Test("failed replacement retains exact empty target and rejects another pending target", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 1);
    string prior = ledger.ExportBackup();
    store.FailWrites = true;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()) && ledger.IsDirty && ledger.UnlockedCount == 0);
    Check(!ledger.TryUnlock("ONEUP", 2) && ledger.UnlockedCount == 0);
    Throws<InvalidOperationException>(() => ledger.ReplaceBackup(prior));
    Check(Ledger(store).Has("CH1")); // No replacement write was acknowledged.
    store.FailWrites = false;
    Check(ledger.ReplaceBackup(ledger.EmptyBackup()) && !ledger.IsDirty);
    Check(Ledger(store).UnlockedCount == 0 && !Ledger(store).AutomaticSaveRecovery);
});
Test("cached failed synchronization does not acknowledge a replacement", () =>
{
    CachedPreferences store = new();
    AchievementLedger ledger = new(ids, store.Read, store.Write);
    ledger.TryUnlock("CH1", 1);
    store.SynchronizeFails = true;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()) && ledger.IsDirty);
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).Has("CH1"));
    Check(!ledger.Flush() && ledger.UnlockedCount == 0);
    int writes = store.Writes;
    store.SynchronizeFails = false;
    Check(ledger.Flush() && store.Writes == writes + 2);
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).UnlockedCount == 0);
});
Test("interruption after first replacement replica restores only new epoch", () =>
{
    CachedPreferences store = new();
    AchievementLedger ledger = new(ids, store.Read, store.Write);
    ledger.TryUnlock("CH1", 1);
    store.FailOnWrite = store.Writes + 2;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()));
    AchievementLedger cold = new(ids, store.ReadDisk, store.Write);
    Check(cold.UnlockedCount == 0 && !cold.AutomaticSaveRecovery && cold.IsDirty);
    store.FailOnWrite = 0;
    Check(ledger.Flush() && new AchievementLedger(ids, store.ReadDisk, store.Write).UnlockedCount == 0);
});
Test("unreadable old replica delays replacement without losing its pending target", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 1);
    int writes = store.Writes;
    store.FailReads = true;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()) && store.Writes == writes);
    store.FailReads = false;
    Check(ledger.Flush() && Ledger(store).UnlockedCount == 0);
});
Test("one corrupt acknowledged empty replica cannot resurrect removed progress", () =>
{
    MemoryStore store = new();
    AchievementLedger ledger = Ledger(store);
    ledger.TryUnlock("CH1", 1);
    Check(ledger.ReplaceBackup(ledger.EmptyBackup()));
    store.Data[prefix + ".A"] += "corrupt";
    AchievementLedger cold = Ledger(store);
    Check(cold.UnlockedCount == 0 && !cold.AutomaticSaveRecovery && cold.Flush());
});
Test("stale ledger flush adopts a newer replacement instead of merging its old state", () =>
{
    MemoryStore store = new();
    AchievementLedger stale = Ledger(store);
    stale.TryUnlock("CH1", 1);
    AchievementLedger current = Ledger(store);
    Check(current.ReplaceBackup(current.EmptyBackup()));
    int events = 0;
    stale.Unlocked += _ => events++;
    Check(!stale.TryUnlock("ONEUP", 2));
    Check(stale.UnlockedCount == 0 && Ledger(store).UnlockedCount == 0 && events == 0);
});
Test("journal rollback supersedes a partially durable replacement with a fresh epoch", () =>
{
    CachedPreferences store = new();
    AchievementLedger ledger = new(ids, store.Read, store.Write);
    ledger.TryUnlock("CH1", 13);
    string prior = ledger.ExportBackup();
    store.FailOnWrite = store.Writes + 2;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()));
    Check(new AchievementLedger(ids, store.ReadDisk, store.Write).UnlockedCount == 0);
    store.FailOnWrite = 0;
    Check(ledger.ReplaceBackup(prior, preserveRecoveryPolicy: true));
    AchievementLedger cold = new(ids, store.ReadDisk, store.Write);
    Check(cold.Has("CH1") && cold.AutomaticSaveRecovery && cold.ExportBackup() == prior);
    Check(store.Disk.Values.All(value => value.StartsWith("MORROACH|2|2|", StringComparison.Ordinal)));
});
Test("new epoch replaces an exhausted generation but exhausted epoch remains pending", () =>
{
    MemoryStore store = new();
    store.Data[prefix + ".A"] = Record("MORROACH|1|9223372036854775807|CH1=1");
    AchievementLedger ledger = Ledger(store);
    Check(ledger.ReplaceBackup(ledger.EmptyBackup()) && Ledger(store).UnlockedCount == 0);
    store.Data[prefix + ".A"] = Record("MORROACH|2|9223372036854775807|1|0|CH1=1");
    ledger = Ledger(store);
    int writes = store.Writes;
    Check(!ledger.ReplaceBackup(ledger.EmptyBackup()) && ledger.IsDirty && store.Writes == writes);
});
string portableCatalog = Ledger(new()).EmptyBackup().Split('|')[2];
foreach ((string name, string value) in new (string, string)[]
{
    ("checksum", Ledger(new()).EmptyBackup() + "x"),
    ("oversized", new string('x', 8193)),
    ("wrong catalog", Record("MORROACHBACKUP|1|other|0|")),
    ("future schema", Record("MORROACHBACKUP|2|" + portableCatalog + "|0|")),
    ("unknown ID", Record("MORROACHBACKUP|1|" + portableCatalog + "|0|OTHER=1")),
    ("duplicate ID", Record("MORROACHBACKUP|1|" + portableCatalog + "|0|CH1=1;CH1=2")),
    ("negative date", Record("MORROACHBACKUP|1|" + portableCatalog + "|0|CH1=-1")),
    ("overflow date", Record("MORROACHBACKUP|1|" + portableCatalog + "|0|CH1=253402300800")),
    ("invalid policy", Record("MORROACHBACKUP|1|" + portableCatalog + "|2|")),
    ("extra field", Record("MORROACHBACKUP|1|" + portableCatalog + "|0||extra")),
})
{
    Test($"portable {name} is rejected before state or storage mutation", () =>
    {
        MemoryStore store = new();
        AchievementLedger ledger = Ledger(store);
        ledger.TryUnlock("CH1", 1);
        int writes = store.Writes;
        string before = ledger.ExportBackup();
        Throws<ArgumentException>(() => ledger.ValidateBackup(value));
        Throws<ArgumentException>(() => ledger.ReplaceBackup(value));
        Check(ledger.ExportBackup() == before && store.Writes == writes && !ledger.IsDirty);
    });
}
Console.WriteLine($"PASS: {passed} Morro achievement ledger tests");

sealed class MemoryStore
{
    public readonly Dictionary<string, string> Data = new(StringComparer.Ordinal);
    public bool FailWrites;
    public bool DropWrites;
    public bool ThrowAfterWrite;
    public bool FailReads;
    public int Writes;
    public string? Read(string key)
    {
        if (FailReads) throw new IOException("synthetic read failure");
        return Data.GetValueOrDefault(key);
    }
    public void Write(string key, string value)
    {
        Writes++;
        if (FailWrites) throw new IOException("synthetic write failure");
        if (!DropWrites) Data[key] = value;
        if (ThrowAfterWrite) throw new IOException("synthetic post-commit failure");
    }
}

// Models NSUserDefaults' process cache separately from persistent storage.
// SetString is visible to Read before a failed Synchronize has reached disk.
sealed class CachedPreferences
{
    public readonly Dictionary<string, string> Cache = new(StringComparer.Ordinal);
    public readonly Dictionary<string, string> Disk = new(StringComparer.Ordinal);
    public readonly List<string> WrittenKeys = new();
    public bool SynchronizeFails;
    public int FailOnWrite;
    public int Writes;
    public string? Read(string key) => Cache.GetValueOrDefault(key);
    public string? ReadDisk(string key) => Disk.GetValueOrDefault(key);
    public void Write(string key, string value)
    {
        Writes++;
        WrittenKeys.Add(key);
        Cache[key] = value;
        if (SynchronizeFails || Writes == FailOnWrite)
            throw new IOException("synthetic failed preference synchronization");
        foreach (KeyValuePair<string, string> entry in Cache) Disk[entry.Key] = entry.Value;
    }
}

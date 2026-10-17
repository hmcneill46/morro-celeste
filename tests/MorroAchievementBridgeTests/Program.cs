using System.Text.Json;
using Celeste;
using Monocle;
using Morro.Achievements;

int passed = 0;
MemoryPreferences preferences = new();
const string keyPrefix = "Fixture.Morro.Achievements.v1";

void Test(string name, Action action)
{
    action();
    Console.WriteLine($"PASS {++passed:D2}: {name}");
}
void Check(bool condition) { if (!condition) throw new InvalidOperationException("Assertion failed."); }
SaveData Save(AreaStats area) => new() { Areas = new List<AreaStats> { area } };
AreaStats Area(int id, bool complete = false, bool normalHeart = false, bool bsideHeart = false) =>
    new() { ID = id, Modes = new[] { new AreaModeStats { Completed = complete, HeartGem = normalHeart },
        new AreaModeStats { HeartGem = bsideHeart }, new AreaModeStats() } };
string Fingerprint(SaveData save) => JsonSerializer.Serialize(save, new JsonSerializerOptions { IncludeFields = true });
void RecoverWithoutMutation(SaveData save)
{
    string before = Fingerprint(save);
    MorroAchievements.Recover(save);
    Check(Fingerprint(save) == before);
}

// Separate process mode gives the static production bridge one fresh lifetime,
// so full historical recovery does not pre-unlock the boundary tests below.
if (args.Length == 1 && args[0] == "--batch-recovery")
{
    Test("thirty historical unlocks commit once without save changes or notifications", () =>
    {
        MorroAchievements.Initialize(preferences.Read, preferences.Write, keyPrefix);
        SaveData complete = new() { TotalStrawberries = 175 };
        foreach (int id in new[] { 1, 2, 3, 4, 5, 6, 7, 9 })
        {
            AreaStats area = Area(id, complete: true, normalHeart: true, bsideHeart: true);
            area.Cassette = true;
            area.Modes[2].HeartGem = true;
            complete.Areas.Add(area);
        }
        AreaStats farewell = Area(10, complete: true);
        farewell.Modes[0].Strawberries.Add(new EntityID("moon-room", 7));
        complete.Areas.Add(farewell);
        AreaData.Areas[10].Mode[0].MapData.Strawberries.Add(new EntityData
        {
            ID = 7, Level = new() { Name = "moon-room" }, Attributes = new() { ["moon"] = true }
        });
        RecoverWithoutMutation(complete);
        Check(MorroAchievements.UnlockedCount == 30 && preferences.Writes == 2);
        Check(!MorroAchievements.StoragePending && MorroAchievementUI.Notifications.Count == 0);
        Check(!MorroAchievements.Has("ONEUP") && !MorroAchievements.Has("PICO8"));
        foreach (string id in MorroAchievementCatalog.Ids.Where(id => id is not ("ONEUP" or "PICO8")))
            Check(MorroAchievements.UnlockedAtUtcSeconds(id) == 0);
        RecoverWithoutMutation(complete);
        Check(preferences.Writes == 2);
    });
    Console.WriteLine($"PASS: {passed} Morro achievement batch recovery test");
    return;
}

Test("events and recovery before initialization have no side effects", () =>
{
    MorroAchievements.Register("CH1");
    MorroAchievements.Recover(Save(Area(1, complete: true)));
    MorroAchievements.RecoverExistingSaves();
    MorroAchievements.Flush();
    Check(MorroAchievements.UnlockedCount == 0 && MorroAchievementUI.Notifications.Count == 0 && UserIO.Opens == 0);
});
Test("portable validation and empty documents work before host initialization", () =>
{
    string empty = MorroAchievements.EmptyBackup();
    MorroAchievements.ValidateBackup(empty);
    Check(empty == MorroAchievements.NormalizeBackup(empty) && preferences.Writes == 0);
    Check(!MorroAchievements.FlushBackup());
});
Test("initialization loads independent preferences without saving game data", () =>
{
    MorroAchievements.Initialize(preferences.Read, preferences.Write, keyPrefix);
    Check(MorroAchievements.Definitions.Length == 32 && MorroAchievements.UnlockedCount == 0);
    Check(preferences.Writes == 0 && MorroAchievementUI.Notifications.Count == 0);
});
Test("null save is harmless", () =>
{
    MorroAchievements.Recover(null);
    Check(MorroAchievements.UnlockedCount == 0);
});
Test("uncompleted chapters and interludes do not imply chapter achievements", () =>
{
    RecoverWithoutMutation(Save(Area(1)));
    foreach (int id in new[] { 0, 8, 9 }) RecoverWithoutMutation(Save(Area(id, complete: true)));
    Check(MorroAchievements.UnlockedCount == 0);
});
Test("page recovery waits for map data and skips active saves", () =>
{
    List<AreaData> areas = AreaData.Areas;
    AreaData.Areas = null;
    MorroAchievements.RecoverExistingSaves();
    AreaData.Areas = areas;
    UserIO.Saving = true;
    MorroAchievements.RecoverExistingSaves();
    UserIO.Saving = false;
    Check(UserIO.Opens == 0 && UserIO.RequestedNames.Count == 0);
});
Test("unavailable read authority leaves state unchanged", () =>
{
    UserIO.AllowOpen = false;
    MorroAchievements.RecoverExistingSaves();
    UserIO.AllowOpen = true;
    Check(UserIO.Opens == 1 && UserIO.Closes == 0 && MorroAchievements.UnlockedCount == 0);
});
Test("opening achievements before Play scans the three existing slots without mutation", () =>
{
    SaveData first = Save(Area(1, complete: true));
    SaveData third = new();
    UserIO.Slots["0"] = first;
    UserIO.Slots["1"] = null; // Missing or rejected slot must not prevent other reads.
    UserIO.Slots["2"] = third;
    string beforeFirst = Fingerprint(first), beforeThird = Fingerprint(third);
    MorroAchievements.RecoverExistingSaves();
    Check(MorroAchievements.Has("CH1") && MorroAchievements.UnlockedCount == 1);
    Check(UserIO.RequestedNames.SequenceEqual(new[] { "0", "1", "2" }) && UserIO.Closes == 1);
    Check(SaveData.Instance == null && Fingerprint(first) == beforeFirst && Fingerprint(third) == beforeThird);
    Check(MorroAchievementUI.Notifications.Count == 0 && MorroAchievements.UnlockedAtUtcSeconds("CH1") == 0);
});
Test("reopening achievements recognizes an imported slot without playing it", () =>
{
    SaveData imported = Save(Area(2, complete: true));
    UserIO.Slots["2"] = imported;
    UserIO.RequestedNames.Clear();
    string before = Fingerprint(imported);
    MorroAchievements.RecoverExistingSaves();
    Check(MorroAchievements.Has("CH2") && MorroAchievements.UnlockedCount == 2);
    Check(UserIO.RequestedNames.SequenceEqual(new[] { "0", "1", "2" }) && UserIO.Closes == 2);
    Check(SaveData.Instance == null && Fingerprint(imported) == before && MorroAchievementUI.Notifications.Count == 0);
});
Test("pause-menu page recovery uses the live save and avoids stale disk reads", () =>
{
    SaveData live = Save(Area(3, complete: true));
    SaveData.Instance = live;
    UserIO.RequestedNames.Clear();
    int opens = UserIO.Opens;
    string before = Fingerprint(live);
    MorroAchievements.RecoverExistingSaves();
    Check(MorroAchievements.Has("CH3") && MorroAchievements.UnlockedCount == 3);
    Check(UserIO.Opens == opens && UserIO.RequestedNames.Count == 0 && Fingerprint(live) == before);
    Check(ReferenceEquals(SaveData.Instance, live));
    SaveData.Instance = null;
    UserIO.Slots.Clear();
});
Test("sparse imported progress is inspected without initialization", () =>
{
    SaveData sparse = new() { Areas = new() { null, new() { Modes = null }, new() { Modes = new AreaModeStats[] { null } } } };
    MorroAchievements.Recover(sparse);
    Check(MorroAchievements.UnlockedCount == 3 && sparse.Areas.Count == 3 && sparse.Areas[1].Modes == null);
});
for (int chapter = 1; chapter <= 7; chapter++)
{
    int id = chapter;
    Test($"chapter {id} completion recovers only its original chapter event", () =>
    {
        int previous = MorroAchievements.UnlockedCount;
        bool previouslyRecovered = MorroAchievements.Has("CH" + id);
        RecoverWithoutMutation(Save(Area(id, complete: true)));
        Check(MorroAchievements.Has("CH" + id) && MorroAchievements.UnlockedCount == previous + (previouslyRecovered ? 0 : 1));
        Check(MorroAchievements.UnlockedAtUtcSeconds("CH" + id) == 0);
    });
}
foreach ((int total, string id, bool shouldUnlock) in new[]
{
    (29, "STRB1", false), (30, "STRB1", true), (79, "STRB2", false),
    (80, "STRB2", true), (174, "STRB3", false), (175, "STRB3", true)
})
{
    Test($"berry threshold {total} gives {id}={shouldUnlock}", () =>
    {
        SaveData save = new() { TotalStrawberries = total, AssistMode = true, VariantMode = true, CheatMode = true };
        RecoverWithoutMutation(save);
        Check(MorroAchievements.Has(id) == shouldUnlock);
    });
}
Test("cassette must have been collected", () =>
{
    AreaStats area = Area(2);
    RecoverWithoutMutation(Save(area));
    Check(!MorroAchievements.Has("CASS"));
    area.Cassette = true;
    RecoverWithoutMutation(Save(area));
    Check(MorroAchievements.Has("CASS"));
});
Test("epilogue area 8 is not Core for either heart achievement", () =>
{
    int previous = MorroAchievements.UnlockedCount;
    RecoverWithoutMutation(Save(Area(8, normalHeart: true, bsideHeart: true)));
    Check(!MorroAchievements.Has("HEART8") && !MorroAchievements.Has("BSIDE8"));
    Check(MorroAchievements.UnlockedCount == previous);
});
for (int chapter = 1; chapter <= 8; chapter++)
{
    int id = chapter;
    int areaId = chapter == 8 ? 9 : chapter;
    Test($"area {areaId} normal heart maps to HEART{id}", () =>
    {
        int previous = MorroAchievements.UnlockedCount;
        RecoverWithoutMutation(Save(Area(areaId, normalHeart: true)));
        Check(MorroAchievements.Has("HEART" + id) && MorroAchievements.UnlockedCount == previous + 1);
    });
    Test($"area {areaId} B-side needs the heart, not just a completion flag", () =>
    {
        AreaStats area = Area(areaId);
        area.Modes[1].Completed = true;
        RecoverWithoutMutation(Save(area));
        Check(!MorroAchievements.Has("BSIDE" + id));
        area.Modes[1].HeartGem = true;
        int previous = MorroAchievements.UnlockedCount;
        RecoverWithoutMutation(Save(area));
        Check(MorroAchievements.Has("BSIDE" + id) && MorroAchievements.UnlockedCount == previous + 1);
    });
}
Test("C-sides are recovered at 24 hearts, not at 23", () =>
{
    SaveData save = new();
    foreach (int id in new[] { 1, 2, 3, 4, 5, 6, 7, 9 })
    {
        AreaStats area = Area(id, normalHeart: true, bsideHeart: true);
        area.Modes[2].HeartGem = id != 9;
        save.Areas.Add(area);
    }
    Check(save.TotalHeartGems == 23);
    RecoverWithoutMutation(save);
    Check(!MorroAchievements.Has("CSIDES"));
    save.Areas[^1].Modes[2].HeartGem = true;
    RecoverWithoutMutation(save);
    Check(MorroAchievements.Has("CSIDES"));
});
Test("Farewell is recovered only from chapter 9's actual area 10 completion", () =>
{
    RecoverWithoutMutation(Save(Area(10)));
    Check(!MorroAchievements.Has("FAREWELL"));
    RecoverWithoutMutation(Save(Area(10, complete: true)));
    Check(MorroAchievements.Has("FAREWELL"));
});
Test("a golden berry in Farewell is not the moon berry", () =>
{
    EntityData golden = new() { ID = 7, Level = new() { Name = "golden-room" }, Attributes = new() { ["golden"] = true } };
    EntityData moon = new() { ID = 7, Level = new() { Name = "moon-room" }, Attributes = new() { ["moon"] = true } };
    AreaData.Areas[10].Mode[0].MapData.Strawberries.AddRange(new[] { golden, moon });
    AreaStats area = Area(10);
    area.Modes[0].Strawberries.Add(new EntityID("golden-room", 7));
    RecoverWithoutMutation(Save(area));
    Check(!MorroAchievements.Has("WOW"));
});
Test("moon backfill requires both its room and entity ID", () =>
{
    AreaStats area = Area(10);
    area.Modes[0].Strawberries.Add(new EntityID("moon-room", 8));
    area.Modes[0].Strawberries.Add(new EntityID("wrong-room", 7));
    RecoverWithoutMutation(Save(area));
    Check(!MorroAchievements.Has("WOW"));
    area.Modes[0].Strawberries.Add(new EntityID("moon-room", 7));
    RecoverWithoutMutation(Save(area));
    Check(MorroAchievements.Has("WOW"));
});
Test("full save backfill stays silent and does not invent event-only awards", () =>
{
    Check(MorroAchievements.UnlockedCount == 30);
    Check(!MorroAchievements.Has("ONEUP") && !MorroAchievements.Has("PICO8"));
    Check(MorroAchievementUI.Notifications.Count == 0);
    foreach (string id in MorroAchievementCatalog.Ids.Where(id => id is not ("ONEUP" or "PICO8")))
        Check(MorroAchievements.UnlockedAtUtcSeconds(id) == 0);
});
Test("recovery from an empty or replaced save never revokes lifetime unlocks", () =>
{
    int writes = preferences.Writes;
    RecoverWithoutMutation(new SaveData());
    RecoverWithoutMutation(Save(Area(1, complete: true)));
    Check(MorroAchievements.UnlockedCount == 30 && preferences.Writes == writes);
});
Test("soft reload initialization preserves the same ledger", () =>
{
    MorroAchievements.Initialize(_ => throw new InvalidOperationException("must not read again"),
        (_, _) => throw new InvalidOperationException("must not rebind"), "Other.Domain");
    Check(MorroAchievements.UnlockedCount == 30);
});
Test("the original one-up event gives one dated toast and no duplicate", () =>
{
    MorroAchievements.Register("ONEUP");
    MorroAchievements.Register("ONEUP");
    Check(MorroAchievements.Has("ONEUP") && MorroAchievements.UnlockedAtUtcSeconds("ONEUP") > 0);
    Check(MorroAchievementUI.Notifications.SequenceEqual(new[] { "ONEUP" }));
});
Test("a live event stays unlocked while persistence is unavailable", () =>
{
    preferences.FailWrites = true;
    MorroAchievements.Register("PICO8");
    Check(MorroAchievements.Has("PICO8") && MorroAchievements.StoragePending);
    MorroAchievements.Register("PICO8");
    Check(MorroAchievementUI.Notifications.SequenceEqual(new[] { "ONEUP", "PICO8" }));
    Engine.RawDeltaTime = 6f;
    MorroAchievements.Update();
    Check(MorroAchievements.StoragePending);
});
Test("regular update retries storage without replaying notifications", () =>
{
    preferences.FailWrites = false;
    int updates = MorroAchievementUI.Updates;
    MorroAchievements.Update();
    Check(!MorroAchievements.StoragePending && MorroAchievementUI.Updates == updates + 1);
    Check(MorroAchievementUI.Notifications.Count == 2 && MorroAchievements.UnlockedCount == 32);
});
Test("cold ledger restore preserves all awards and only private preference keys", () =>
{
    AchievementLedger cold = new(MorroAchievementCatalog.Ids, preferences.Read, preferences.Write, keyPrefix);
    Check(cold.UnlockedCount == 32);
    Check(cold.UnlockedAtUtcSeconds("ONEUP") > 0 && cold.UnlockedAtUtcSeconds("PICO8") > 0);
    Check(preferences.Data.Keys.Order().SequenceEqual(new[] { keyPrefix + ".A", keyPrefix + ".B" }));
});
Test("backup mutation guard blocks old gameplay, recovery and lifecycle retry", () =>
{
    string before = MorroAchievements.ExportBackup();
    MorroAchievements.SetBackupMutationBlocked(true);
    int writes = preferences.Writes, opens = UserIO.Opens, updates = MorroAchievementUI.Updates;
    MorroAchievements.Register("CH1");
    MorroAchievements.Recover(new SaveData { TotalStrawberries = 175 });
    MorroAchievements.RecoverExistingSaves();
    MorroAchievements.Flush();
    MorroAchievements.Update();
    Check(MorroAchievements.BackupMutationBlocked && MorroAchievements.ExportBackup() == before);
    Check(preferences.Writes == writes && UserIO.Opens == opens && MorroAchievementUI.Updates == updates);
    Check(MorroAchievements.FlushBackup());
});
Test("host can replace with empty progress while blocked and clear presentation", () =>
{
    Check(MorroAchievements.ReplaceBackup(MorroAchievements.EmptyBackup()));
    Check(MorroAchievements.UnlockedCount == 0 && !MorroAchievements.StoragePending);
    MorroAchievements.ClearPresentationAfterBackupRestore();
    Check(MorroAchievementUI.Notifications.Count == 0 && MorroAchievementUI.Resets == 1);
    MorroAchievements.Register("CH1");
    Check(MorroAchievements.UnlockedCount == 0);
    MorroAchievements.SetBackupMutationBlocked(false);
});
Test("explicit reset suppresses save and page backfill across soft and cold reload", () =>
{
    SaveData progress = new() { TotalStrawberries = 175, Areas = new() { Area(1, complete: true) } };
    UserIO.Slots["0"] = progress;
    SaveData.Instance = progress;
    int opens = UserIO.Opens, writes = preferences.Writes;
    RecoverWithoutMutation(progress);
    MorroAchievements.RecoverExistingSaves();
    SaveData.Instance = null;
    MorroAchievements.RecoverExistingSaves();
    MorroAchievements.Initialize(preferences.Read, preferences.Write, keyPrefix);
    Check(MorroAchievements.UnlockedCount == 0 && opens == UserIO.Opens && writes == preferences.Writes);
    AchievementLedger cold = new(MorroAchievementCatalog.Ids, preferences.Read, preferences.Write, keyPrefix);
    Check(cold.UnlockedCount == 0 && !cold.AutomaticSaveRecovery);
});
Test("new original events still earn achievements after an explicit reset", () =>
{
    MorroAchievements.Register("ONEUP");
    Check(MorroAchievements.UnlockedCount == 1 && MorroAchievements.Has("ONEUP"));
    Check(MorroAchievementUI.Notifications.SequenceEqual(new[] { "ONEUP" }));
});
Test("failed host replacement retains exact target and can retry while blocked", () =>
{
    MorroAchievements.SetBackupMutationBlocked(true);
    preferences.FailWrites = true;
    Check(!MorroAchievements.ReplaceBackup(MorroAchievements.EmptyBackup()));
    int writes = preferences.Writes;
    MorroAchievements.Register("CH1");
    MorroAchievements.Flush();
    MorroAchievements.Update();
    Check(MorroAchievements.StoragePending && MorroAchievements.UnlockedCount == 0 && preferences.Writes == writes);
    preferences.FailWrites = false;
    Check(MorroAchievements.FlushBackup() && !MorroAchievements.StoragePending);
    MorroAchievements.ClearPresentationAfterBackupRestore();
    MorroAchievements.SetBackupMutationBlocked(false);
});
Test("journal rollback can preserve prior automatic recovery policy", () =>
{
    string legacy = MorroAchievements.NormalizeBackup(MorroAchievements.EmptyBackup(), automaticSaveRecovery: true);
    Check(MorroAchievements.ReplaceBackup(legacy, preserveRecoveryPolicy: true));
    Check(MorroAchievements.ExportBackup() == legacy);
    RecoverWithoutMutation(Save(Area(1, complete: true)));
    Check(MorroAchievements.Has("CH1") && MorroAchievements.UnlockedAtUtcSeconds("CH1") == 0);
    string recovered = MorroAchievements.ExportBackup();
    Check(MorroAchievements.ReplaceBackup(recovered));
    Check(MorroAchievements.ExportBackup() == MorroAchievements.NormalizeBackup(recovered));
    RecoverWithoutMutation(Save(Area(2, complete: true)));
    Check(!MorroAchievements.Has("CH2"));
});
Test("mutation guard waits for an in-flight award callback before clearing presentation", () =>
{
    Check(MorroAchievements.ReplaceBackup(MorroAchievements.EmptyBackup()));
    MorroAchievements.ClearPresentationAfterBackupRestore();
    using ManualResetEventSlim entered = new(), release = new(), attemptingGuard = new();
    MorroAchievementUI.BeforeQueue = () => { entered.Set(); Check(release.Wait(TimeSpan.FromSeconds(5))); };
    Task award = Task.Run(() => MorroAchievements.Register("PICO8"));
    Check(entered.Wait(TimeSpan.FromSeconds(5)));
    Task replace = Task.Run(() =>
    {
        attemptingGuard.Set();
        MorroAchievements.SetBackupMutationBlocked(true);
        Check(MorroAchievements.ReplaceBackup(MorroAchievements.EmptyBackup()));
        MorroAchievements.ClearPresentationAfterBackupRestore(); // Fixture has no game-thread resources.
    });
    Check(attemptingGuard.Wait(TimeSpan.FromSeconds(5)));
    release.Set();
    Check(Task.WaitAll(new[] { award, replace }, TimeSpan.FromSeconds(5)));
    MorroAchievementUI.BeforeQueue = null;
    Check(MorroAchievements.UnlockedCount == 0 && MorroAchievementUI.Notifications.Count == 0);
    MorroAchievements.SetBackupMutationBlocked(false);
});
Console.WriteLine($"PASS: {passed} Morro achievement bridge tests");

sealed class MemoryPreferences
{
    public readonly Dictionary<string, string> Data = new(StringComparer.Ordinal);
    public int Writes;
    public bool FailWrites;
    public string Read(string key) => Data.GetValueOrDefault(key);
    public void Write(string key, string value)
    {
        Writes++;
        if (FailWrites) throw new IOException("synthetic preference failure");
        Data[key] = value;
    }
}

using System;
using Morro.Achievements;
using Monocle;

namespace Celeste;

// This adapter receives the original game's achievement events. It never
// changes Settings, SaveData, the game's unlock conditions, or assist policy.
public sealed class MorroAchievementDefinition
{
    public readonly string Id, Name, Description, IconPath;
    public readonly bool Hidden;

    public MorroAchievementDefinition(string id, string name, string description, bool hidden, string iconPath)
    {
        Id = id; Name = name; Description = description; Hidden = hidden; IconPath = iconPath;
    }
}

public static class MorroAchievements
{
    public static MorroAchievementDefinition[] Definitions => MorroAchievementCatalog.Entries;
    private static readonly object InitializationGate = new();
    private static volatile AchievementLedger ledger;
    private static float retryIn;
    public static int UnlockedCount => ledger?.UnlockedCount ?? 0;
    public static bool StoragePending => ledger?.IsDirty ?? false;
    public static bool Has(string id) => ledger != null && ledger.Has(id);
    public static long? UnlockedAtUtcSeconds(string id) => ledger?.UnlockedAtUtcSeconds(id);

    public static void Initialize(Func<string, string> read, Action<string, string> write, string keyPrefix)
    {
        lock (InitializationGate)
        {
            if (ledger != null) return; // Soft reload keeps installation-wide progress.
            string[] ids = new string[Definitions.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = Definitions[i].Id;
            AchievementLedger initialized = new AchievementLedger(ids, read, write, keyPrefix);
            initialized.Unlocked += MorroAchievementUI.QueueUnlock;
            ledger = initialized;
        }
    }

    public static void Register(string id) => ledger?.TryUnlock(id);
    public static void Flush() => ledger?.Flush();

    // Opening Achievements before Play must still recognize existing saves.
    // Read through the same authority as File Select, retaining its existing
    // previous-good recovery policy. Never initialize or select these snapshots:
    // that would mutate progress/assist state just to display achievements.
    public static void RecoverExistingSaves()
    {
        if (ledger == null || UserIO.Saving || AreaData.Areas == null || AreaData.Areas.Count == 0) return;
        if (SaveData.Instance != null)
        {
            Recover(SaveData.Instance);
            return;
        }
        if (!UserIO.Open(UserIO.Mode.Read)) return;
        try
        {
            for (int slot = 0; slot < 3; slot++)
                Recover(UserIO.Load<SaveData>(SaveData.GetFilename(slot)));
        }
        finally { UserIO.Close(); }
    }

    public static void Update()
    {
        retryIn -= Engine.RawDeltaTime;
        if (retryIn <= 0)
        {
            retryIn = 5f;
            if (StoragePending) Flush();
        }
        MorroAchievementUI.Update();
    }

    // Read only the progress the vanilla save can actually establish. Historical
    // unlock dates are unknown; restoring old progress produces no toast storm.
    public static void Recover(SaveData save)
    {
        if (ledger == null || save == null || save.Areas == null) return;
        void Restore(string id) => ledger.TryUnlock(id, 0, notify: false, flushImmediately: false);
        if (save.TotalStrawberries >= 30) Restore("STRB1");
        if (save.TotalStrawberries >= 80) Restore("STRB2");
        if (save.TotalStrawberries >= 175) Restore("STRB3");
        int totalHearts = 0;
        foreach (AreaStats area in save.Areas)
        {
            if (area == null || area.Modes == null || area.Modes.Length == 0) continue;
            foreach (AreaModeStats mode in area.Modes)
                if (mode != null && mode.HeartGem) totalHearts++;
            if (area.Cassette) Restore("CASS");
            AreaModeStats normal = area.Modes[0];
            if (normal == null) continue;
            if (area.ID >= 1 && area.ID <= 7 && normal.Completed) Restore("CH" + area.ID);
            int chapter = area.ID == 9 ? 8 : area.ID >= 1 && area.ID <= 7 ? area.ID : 0;
            if (chapter != 0)
            {
                if (normal.HeartGem) Restore("HEART" + chapter);
                if (area.Modes.Length > 1 && area.Modes[1] != null && area.Modes[1].HeartGem)
                    Restore("BSIDE" + chapter);
            }
            if (area.ID != 10) continue;
            if (normal.Completed) Restore("FAREWELL");
            // The golden berry is not the moon berry. Match the original map's
            // moon entity to its exact persisted room/entity ID.
            if (AreaData.Areas == null || AreaData.Areas.Count <= 10 || normal.Strawberries == null) continue;
            AreaData farewell = AreaData.Areas[10];
            if (farewell?.Mode == null || farewell.Mode.Length == 0) continue;
            MapData map = farewell.Mode[0]?.MapData;
            if (map?.Strawberries == null) continue;
            foreach (EntityData berry in map.Strawberries)
                if (berry != null && berry.Bool("moon") && berry.Level != null &&
                    normal.Strawberries.Contains(new EntityID(berry.Level.Name, berry.ID)))
                    Restore("WOW");
        }
        if (totalHearts >= 24) Restore("CSIDES");
        ledger.Flush();
        // ONEUP and PICO8 have no reliable saved flag. Their real events remain
        // authoritative; neither is inferred from chapter or berry totals.
    }
}

// Owned, minimal semantic fixtures. No generated or decompiled game source.
namespace Monocle
{
    public static class Engine { public static float RawDeltaTime = 1f / 60f; }
}

namespace Celeste
{
    public sealed class SaveData
    {
        public static SaveData Instance;
        public static string GetFilename(int slot) => slot.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public int TotalStrawberries;
        public bool AssistMode;
        public bool VariantMode;
        public bool CheatMode;
        public List<AreaStats> Areas = new();
        public int TotalHeartGems => Areas.Sum(area => area.Modes.Count(mode => mode != null && mode.HeartGem));
        public void AfterInitialize() => throw new InvalidOperationException("Page recovery must not initialize a save.");
    }

    public static class UserIO
    {
        public enum Mode { Read, Write }
        public static bool Saving;
        public static bool AllowOpen = true;
        public static int Opens;
        public static int Closes;
        public static readonly Dictionary<string, SaveData> Slots = new();
        public static readonly List<string> RequestedNames = new();
        public static bool Open(Mode mode)
        {
            if (mode != Mode.Read) throw new InvalidOperationException("Page recovery must not open write mode.");
            Opens++;
            return AllowOpen;
        }
        public static T Load<T>(string name) where T : class
        {
            RequestedNames.Add(name);
            return Slots.GetValueOrDefault(name) as T;
        }
        public static void Close() => Closes++;
    }

    public sealed class AreaStats
    {
        public int ID;
        public bool Cassette;
        public AreaModeStats[] Modes = { new(), new(), new() };
    }

    public sealed class AreaModeStats
    {
        public bool Completed;
        public bool HeartGem;
        public HashSet<EntityID> Strawberries = new();
    }

    public readonly record struct EntityID(string Level, int ID);
    public sealed class LevelData { public string Name; }
    public sealed class EntityData
    {
        public LevelData Level;
        public int ID;
        public Dictionary<string, bool> Attributes = new();
        public bool Bool(string name) => Attributes.GetValueOrDefault(name);
    }
    public sealed class MapData { public List<EntityData> Strawberries = new(); }
    public sealed class AreaMode { public MapData MapData = new(); }
    public sealed class AreaData
    {
        public static List<AreaData> Areas = Enumerable.Range(0, 11).Select(_ => new AreaData()).ToList();
        public AreaMode[] Mode = { new(), new(), new() };
    }

    public static class MorroAchievementUI
    {
        public static readonly List<string> Notifications = new();
        public static int Updates;
        public static void QueueUnlock(string id) => Notifications.Add(id);
        public static void Update() => Updates++;
    }

    public static class MorroAchievementCatalog
    {
        public static readonly string[] Ids =
        {
            "CH1", "CH2", "CH3", "CH4", "CH5", "CH6", "CH7", "STRB1", "STRB2", "STRB3",
            "CASS", "ONEUP", "PICO8", "HEART1", "HEART2", "HEART3", "HEART4", "HEART5", "HEART6", "HEART7", "HEART8",
            "BSIDE1", "BSIDE2", "BSIDE3", "BSIDE4", "BSIDE5", "BSIDE6", "BSIDE7", "BSIDE8", "CSIDES", "FAREWELL", "WOW"
        };
        public static readonly MorroAchievementDefinition[] Entries = Ids
            .Select(id => new MorroAchievementDefinition(id, id, id, false, string.Empty)).ToArray();
    }
}

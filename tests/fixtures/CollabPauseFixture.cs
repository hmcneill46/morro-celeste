using System;
using Celeste.Mod;

namespace Celeste
{
    internal readonly record struct AreaKey(int ID);
    internal sealed class SaveData
    {
        internal static SaveData Instance;
        internal AreaKey LastArea, LastArea_Safe;
    }
    internal sealed class StateMachine { internal int State = 11; }
    internal sealed class Player { internal readonly StateMachine StateMachine = new(); }
    internal sealed class Tracker
    {
        internal Player Player = new();
        internal T GetEntity<T>() where T : class => Player as T;
    }
    internal sealed class AudioState
    {
        internal int Applies;
        internal void Apply() => Applies++;
    }
    internal sealed class Session { internal readonly AudioState Audio = new(); }
    internal sealed class OuiChapterPanel
    {
        internal bool Removed;
        internal void RemoveSelf() => Removed = true;
    }
    internal sealed class EntityList
    {
        internal bool Flushed;
        internal void UpdateLists() => Flushed = true;
    }
    internal class Overworld
    {
        internal readonly EntityList Entities = new();
        internal readonly OuiChapterPanel Panel = new();
        internal T GetUI<T>() where T : class => Panel as T;
    }
    internal partial class Level
    {
        internal readonly Tracker Tracker = new();
        internal readonly Session Session = new();
        internal Action OnEndOfFrame;
        internal bool Paused, QuickResetMenu;
        internal (int Index, bool Minimal, bool QuickReset) Arguments;
        // The runner separately proves that the complete generated original
        // pause body equals the pre-fix body. This handle observes its boundary.
        private void AppleEverestCollabOriginalPause(int startIndex, bool minimal, bool quickReset)
        {
            Arguments = (startIndex, minimal, quickReset);
            Paused = true;
            if (quickReset)
            {
                QuickResetMenu = true;
                return;
            }
        }
    }
}

namespace Celeste.Mod
{
    internal sealed class AppleEverestSceneWrappingEntity<T> where T : Celeste.Overworld
    {
        internal readonly T WrappedScene;
        internal readonly Celeste.Level Level;
        internal bool Removed;
        internal AppleEverestSceneWrappingEntity(T scene, Celeste.Level level)
        { WrappedScene = scene; Level = level; }
        internal void RemoveSelf()
        {
            if (!Level.Paused || !WrappedScene.Panel.Removed || !WrappedScene.Entities.Flushed)
                throw new Exception("wrapped UI cleanup order differs");
            Removed = true;
        }
    }
    internal sealed class Credits
    {
        internal bool Cleared;
        internal void Clear() => Cleared = true;
    }
    internal static partial class AppleEverestCollabRuntime
    {
        private static AppleEverestSceneWrappingEntity<Celeste.Overworld> overworldWrapper;
        private static Celeste.AreaKey previousArea;
        private static bool hasPreviousArea;
        private static string forcedMapSid, forcedJournalLevelSet;
        private static Credits chapterCredits;

        internal static void Verify(Celeste.Level level, bool journal, bool quickReset, bool minimal,
            int playerState, bool savePresent)
        {
            var wrapper = new AppleEverestSceneWrappingEntity<Celeste.Overworld>(new(), level);
            overworldWrapper = wrapper;
            previousArea = new(7);
            hasPreviousArea = true;
            forcedMapSid = journal ? null : "Fixture/Map";
            forcedJournalLevelSet = journal ? "Fixture" : null;
            chapterCredits = new();
            level.Tracker.Player.StateMachine.State = playerState;
            Celeste.SaveData.Instance = savePresent ? new() { LastArea = new(9), LastArea_Safe = new(9) } : null;

            level.Pause(3, minimal, quickReset);
            Require(level.Paused && level.Arguments == (3, minimal, quickReset), "pause arguments/behavior changed");
            Require(level.QuickResetMenu == quickReset, "quick-reset branch lost");
            Require(overworldWrapper == null && wrapper.Removed, "lobby card remains over pause menu");
            Require(!hasPreviousArea && forcedMapSid == null && forcedJournalLevelSet == null && chapterCredits.Cleared,
                "stale chapter/journal state remains");
            Require(!savePresent || Celeste.SaveData.Instance.LastArea == new Celeste.AreaKey(7) &&
                Celeste.SaveData.Instance.LastArea_Safe == new Celeste.AreaKey(7), "lobby area was not restored");
            Require(level.Session.Audio.Applies == 1, "lobby audio was not restored once");
            Require(level.Tracker.Player.StateMachine.State == playerState, "player reset before end of frame");
            level.OnEndOfFrame?.Invoke();
            Require(level.Tracker.Player.StateMachine.State == (playerState == 11 ? 0 : playerState),
                "player remained locked or unrelated state was reset");
            level.Pause();
            Require(level.Session.Audio.Applies == 1, "ordinary pause repeated wrapped-scene cleanup");
        }
        private static void Require(bool condition, string message)
        { if (!condition) throw new Exception(message); }
    }
}

internal static class Program
{
    private static void Main()
    {
        int checks = 0;
        foreach (bool journal in new[] { false, true })
        foreach (bool quickReset in new[] { false, true })
        foreach (bool minimal in new[] { false, true })
        foreach (int state in new[] { 11, 0, 3 })
        foreach (bool savePresent in new[] { false, true })
        {
            AppleEverestCollabRuntime.Verify(new(), journal, quickReset, minimal, state, savePresent);
            checks++;
        }
        Console.WriteLine($"PASS: {checks} chapter/journal pause and cleanup cases");
    }
}

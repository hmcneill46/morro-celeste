#nullable enable
using System.Text;
using CelesteIOSFoundation;
using Monocle;
using Morro.Achievements;

// Project-owned semantic fixtures, not generated or decompiled game source.
// Scene assignment is immediate here; real Engine end-of-frame scene scheduling,
// native Files presentation, drawing and physical input need product testing.
namespace Celeste
{
    public static class MorroAchievements
    {
        private static readonly Dictionary<string, string> Values = new();
        private static readonly AchievementLedger Ledger = new(new[] { "CH1", "CH2" },
            key => Values.GetValueOrDefault(key), (key, value) =>
            {
                if (FailWrites > 0)
                {
                    FailWrites--;
                    throw new IOException("Injected achievement replica write failure.");
                }
                Values[key] = value;
            });

        public static int FailWrites;
        public static void Unlock() => Ledger.TryUnlock("CH1");
        public static string EmptyBackup() => Ledger.EmptyBackup();
        public static void ValidateBackup(string value) => Ledger.ValidateBackup(value);
        public static string NormalizeBackup(string value) => Ledger.NormalizeBackup(value);
        public static string ExportBackup() => Ledger.ExportBackup();
        public static bool ReplaceBackup(string value) => Ledger.ReplaceBackup(value);
        public static bool FlushBackup() => Ledger.Flush();
        public static void SetBackupMutationBlocked(bool blocked) { }
        public static void ClearPresentationAfterBackupRestore() { }
    }

    public static class IOSDataFiles
    {
        public delegate void ChoicePresenter(string title, string message,
            IReadOnlyList<string> choices, Action<int?> done);
        public static bool MutationSafe => Engine.Scene is not Level &&
            !UserIO.Saving && !MorroIOSBackups.MutationPending;
    }

    public static partial class IOSStorageHooks
    {
        private static readonly Backend Storage = new();
        private static readonly CelesteFileDurabilityStore DurableStore = new(Storage, new Validator());
        public static bool ValidateLogicalFile(string name, ReadOnlyMemory<byte> data) =>
            new Validator().IsValid(name, data);
        public static Dictionary<string, byte[]> CopyRaw() =>
            Storage.Data.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        public static bool RawEquals(Dictionary<string, byte[]> expected) =>
            expected.Count == Storage.Data.Count && expected.All(pair =>
                Storage.Data.TryGetValue(pair.Key, out byte[]? bytes) && bytes.SequenceEqual(pair.Value));
        public static bool SnapshotEquals(IReadOnlyDictionary<string, byte[]?> expected) =>
            expected.All(pair => new[] { CelesteFileCopy.Primary, CelesteFileCopy.PreviousGood }.All(copy =>
                pair.Value == null ? Storage.Read(pair.Key, copy) == null :
                    Storage.Read(pair.Key, copy) is byte[] bytes && bytes.SequenceEqual(pair.Value)));

        private sealed class Validator : ICelesteLogicalFileValidator
        {
            public bool IsValid(string name, ReadOnlyMemory<byte> data) =>
                Encoding.UTF8.GetString(data.Span).StartsWith(name == "settings" ? "settings:" : "save:",
                    StringComparison.Ordinal);
        }

        private sealed class Backend : ICelesteFileDurabilityBackend
        {
            public readonly Dictionary<string, byte[]> Data = new();
            public byte[]? Read(string name, CelesteFileCopy copy) => Data.GetValueOrDefault(name + copy)?.ToArray();
            public void WriteAtomic(string name, CelesteFileCopy copy, ReadOnlyMemory<byte> data) =>
                Data[name + copy] = data.ToArray();
            public void Delete(string name, CelesteFileCopy copy) => Data.Remove(name + copy);
            public bool CleanupOwnedTemporaryFiles(string name) => true;
        }
    }

    public static class UserIO
    {
        public static bool Saving;
        public static byte[] Serialize<T>(T value) => "settings:current"u8.ToArray();
    }

    public sealed class Settings
    {
        public static Settings Instance = new();
        public static int Reloads;
        public static void Reload() => Reloads++;
    }

    public sealed class SaveData
    {
        public static SaveData? Instance = new();
        public static void NoFileAssistChecks() { }
    }

    public static class IOSTouchControls
    {
        public static void Dispose() { }
        public static void Update() { }
    }

    public static class Input
    {
        public static readonly VirtualButton MenuConfirm = new();
        public static int Initializations;
        public static void Initialize() => Initializations++;
        public static void ResetGrab() { }
    }

    public static class OuiFileSelect { public static bool Loaded = true; }
    public sealed class Level : Scene { }
    public sealed class OuiMainMenu { }
    public sealed class Overworld : Scene
    {
        public object? Current;
        public enum StartMode { MainMenu }
    }
    public sealed class OverworldLoader : Scene { public OverworldLoader(Overworld.StartMode mode) { } }
    public static class Audio
    {
        public static void SetMusic(string? value, bool allowFadeOut) { }
        public static void SetAmbience(string? value) { }
    }
    public static class GFX { public static object? Gui; }
    public static class Dialog { public static object? Language; }
    public static class ActiveFont { public static void DrawOutline(params object[] values) { } }
}

namespace Microsoft.Xna.Framework
{
    public struct Vector2
    {
        public Vector2(float x, float y) { }
        public static Vector2 One => default;
        public static Vector2 operator *(Vector2 value, float multiplier) => value;
    }
    public struct Color
    {
        public static Color Black => default;
        public static Color White => default;
        public static Color operator *(Color value, float multiplier) => value;
    }
}

namespace Microsoft.Xna.Framework.Graphics
{
    public struct Viewport { }
    public sealed class GraphicsDevice { public Viewport Viewport; }
    public enum SpriteSortMode { Deferred }
    public static class BlendState { public static readonly object AlphaBlend = new(); }
    public static class SamplerState { public static readonly object LinearClamp = new(); }
    public static class DepthStencilState { public static readonly object None = new(); }
    public static class RasterizerState { public static readonly object CullNone = new(); }
}

namespace Monocle
{
    public class Scene { }
    public sealed class VirtualButton
    {
        public bool Pressed;
        public void ConsumePress() => Pressed = false;
    }
    public static class Engine
    {
        public static Scene Scene = new();
        public static float TimeRate, TimeRateB, FreezeTimer;
        public static bool DashAssistFreeze;
        public static readonly Graphics Graphics = new();
        public static Microsoft.Xna.Framework.Graphics.Viewport Viewport;
        public static readonly object ScreenMatrix = new();
    }
    public sealed class Graphics
    {
        public readonly Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice = new();
    }
    public sealed class Batch
    {
        public void Begin(params object?[] values) { }
        public void End() { }
    }
    public static class Draw
    {
        public static readonly Batch SpriteBatch = new();
        public static void Rect(params object[] values) { }
    }
}

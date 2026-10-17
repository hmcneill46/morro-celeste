// Owned cache/disk and serializer boundaries; no Apple runtime or game inputs.
// The production persistence transaction, ledger and archive codec are linked.
using System.Text;
using Morro.Achievements;

namespace Foundation
{
    internal class NSObject : IDisposable { public void Dispose() { } }
    internal sealed class NSString(string value) : NSObject { internal string Value => value; }
    internal sealed class NSUrl(string path) : NSObject
    {
        internal string Path => path;
        internal static NSUrl FromFilename(string path) => new(path);
    }
    internal sealed class NSData(byte[] value) : NSObject
    {
        internal nuint Length => (nuint)value.Length;
        internal byte[] ToArray() => value.ToArray();
        internal static NSData FromArray(byte[] value) => new(value.ToArray());
        internal static NSData? FromFile(string path) => File.Exists(path) ? new(File.ReadAllBytes(path)) : null;
        internal bool Save(NSUrl url, bool atomically) { File.WriteAllBytes(url.Path, value); return true; }
    }
    internal sealed class NSUserDefaults
    {
        internal static NSUserDefaults StandardUserDefaults { get; } = new();
        internal static NSString SizeLimitExceededNotification { get; } = new("size-limit");
        private Dictionary<string, object> cache = new(StringComparer.Ordinal);
        private Dictionary<string, object> disk = new(StringComparer.Ordinal);
        internal int Synchronizations { get; private set; }
        internal int FailAt { get; set; } = -1;
        internal bool PersistFailedWrite { get; set; }
        internal int Writes { get; private set; }
        internal string? StringForKey(string key) => cache.GetValueOrDefault(key) as string;
        internal NSData? DataForKey(string key) => cache.GetValueOrDefault(key) is byte[] bytes ? new(bytes) : null;
        internal void SetString(string value, string key) { cache[key] = value; Writes++; }
        internal void SetValueForKey(NSData value, NSString key) { cache[key.Value] = value.ToArray(); Writes++; }
        internal void RemoveObject(string key) { cache.Remove(key); Writes++; }
        internal bool Synchronize()
        {
            Synchronizations++;
            bool fail = Synchronizations == FailAt;
            if (!fail || PersistFailedWrite) disk = Copy(cache);
            return !fail;
        }
        internal void ColdStart() { cache = Copy(disk); FailAt = -1; PersistFailedWrite = false; }
        internal void Reset() { cache.Clear(); disk.Clear(); Synchronizations = Writes = 0; FailAt = -1; PersistFailedWrite = false; }
        internal Dictionary<string, object> DiskSnapshot() => Copy(disk);
        internal Dictionary<string, object> ToDictionary() => Copy(cache);
        private static Dictionary<string, object> Copy(Dictionary<string, object> source) => source.ToDictionary(
            pair => pair.Key, pair => pair.Value is byte[] bytes ? (object)bytes.ToArray() : pair.Value, StringComparer.Ordinal);
    }
    internal sealed class NSNotificationCenter
    {
        internal static NSNotificationCenter DefaultCenter { get; } = new();
        internal NSObject AddObserver(NSString name, Action<NSObject> callback) => new();
        internal void RemoveObserver(NSObject value) { }
    }
}

namespace Celeste
{
    internal static class TvOSSettingsSerializer
    {
        internal static object Deserialize(Stream stream) => FixtureSerializer.Read(stream, "settings:");
    }
    internal static class TvOSSaveDataSerializer
    {
        internal static object Deserialize(Stream stream) => FixtureSerializer.Read(stream, "save:");
    }
    internal static class FixtureSerializer
    {
        internal static object Read(Stream stream, string prefix)
        {
            using MemoryStream body = new(); stream.CopyTo(body);
            if (!new UTF8Encoding(false, true).GetString(body.ToArray()).StartsWith(prefix, StringComparison.Ordinal))
                throw new InvalidDataException("Invalid owned fixture payload.");
            return new object();
        }
    }
    internal static class TvOSStage6PersistenceHooks { internal static void Reset() { } }
    internal enum TvOSControllerPromptMode { Automatic, Xbox, PlayStation, NintendoSwitch, Stadia }
    internal static class TvOSControllerPromptHooks
    {
        internal static TvOSControllerPromptMode Mode;
        internal static void SetMode(TvOSControllerPromptMode value) => Mode = value;
    }
    internal static class TvOSPerformanceHudHooks
    {
        internal static bool Enabled;
        internal static void SetEnabled(bool value) => Enabled = value;
    }
    internal static class MorroAchievements
    {
        private static AchievementLedger ledger = NewLedger();
        internal static bool BackupMutationBlocked { get; private set; }
        internal static int PresentationResets { get; private set; }
        internal static bool AutomaticSaveRecovery => ledger.AutomaticSaveRecovery;
        internal static void NewProcess() { ledger = NewLedger(); BackupMutationBlocked = false; PresentationResets = 0; }
        private static AchievementLedger NewLedger() => new(new[] { "CH1", "CH2", "ONEUP" },
            key => Foundation.NSUserDefaults.StandardUserDefaults.StringForKey(key),
            (key, value) =>
            {
                Foundation.NSUserDefaults store = Foundation.NSUserDefaults.StandardUserDefaults;
                store.SetString(value, key);
                if (!store.Synchronize()) throw new IOException("Fixture synchronization failed.");
            }, "Morro.Achievements.Tests");
        internal static string ExportBackup() => ledger.ExportBackup();
        internal static void ValidateBackup(string value) => ledger.ValidateBackup(value);
        internal static string NormalizeBackup(string value) => ledger.NormalizeBackup(value);
        internal static string EmptyBackup() => ledger.EmptyBackup();
        internal static bool ReplaceBackup(string value, bool preserveRecoveryPolicy = false) => ledger.ReplaceBackup(value, preserveRecoveryPolicy);
        internal static bool FlushBackup() => ledger.Flush();
        internal static void SetBackupMutationBlocked(bool value) => BackupMutationBlocked = value;
        internal static void ClearPresentationAfterBackupRestore() => PresentationResets++;
        internal static void Register(string id) { if (!BackupMutationBlocked) ledger.TryUnlock(id, 123); }
        internal static bool Has(string id) => ledger.Has(id);
    }
}

namespace CelesteTvOSHost
{
    internal static class RuntimeLog
    {
        internal static void Info(string value) { }
        internal static void Warning(string value) { }
        internal static void Error(string value) { }
    }
}

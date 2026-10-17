#if MORRO_VANILLA_ACHIEVEMENTS
using Celeste;
using CelesteIOSFoundation;
using Foundation;
using Morro.Backups;

namespace CelesteIOSRuntimeHost;

// One atomic target journal makes a whole-session restore replayable across
// process termination. Existing Celeste files remain the live storage authority.
internal sealed class IOSMorroBackupStorage : IDisposable
{
    private readonly NSFileManager files = NSFileManager.DefaultManager;
    private readonly NSUserDefaults preferences = NSUserDefaults.StandardUserDefaults;
    private readonly string directory;
    private readonly string journal;

    internal IOSMorroBackupStorage()
    {
        string root = Environment.GetEnvironmentVariable("CELESTE_IOS_STORAGE_ROOT")
            ?? throw new IOException("The iOS storage root is unavailable.");
        directory = Path.Combine(root, "MorroBackups");
        journal = Path.Combine(directory, "restore-target.zip");
        EnsureDirectory(root);
        EnsureDirectory(directory);
        MorroIOSBackups.ReadJournal = ReadJournal;
        MorroIOSBackups.WriteJournal = WriteJournal;
        MorroIOSBackups.DeleteJournal = DeleteJournal;
        MorroIOSBackups.ReadPreferences = ReadPreferences;
        MorroIOSBackups.ValidatePreferences = IOSBackupPreferences.Validate;
        MorroIOSBackups.ApplyPreferences = ApplyPreferences;
        MorroIOSBackups.RecoverBeforeGame();
    }

    private byte[]? ReadJournal()
    {
        EnsureDirectory(directory);
        NSFileAttributes? attributes = files.GetAttributes(journal, out _);
        if (attributes is null)
        {
            if (files.FileExists(journal)) throw new IOException("The pending backup cannot be inspected.");
            return null;
        }
        if (attributes.Type != NSFileType.Regular || attributes.Size < 22 || attributes.Size > MorroIOSBackups.MaximumJournalBytes)
            throw new InvalidDataException("The pending backup is invalid.");
        using NSData data = NSData.FromFile(journal) ?? throw new IOException("The pending backup cannot be read.");
        byte[] bytes = data.ToArray();
        if (bytes.Length != (long)attributes.Size) throw new IOException("The pending backup changed during its read.");
        return bytes;
    }

    private void WriteJournal(byte[] bytes)
    {
        if (bytes.Length is < 22 or > MorroIOSBackups.MaximumJournalBytes)
            throw new InvalidDataException("The pending backup is too large.");
        EnsureDirectory(directory);
        NSFileAttributes? attributes = files.GetAttributes(journal, out _);
        if (attributes is not null && attributes.Type != NSFileType.Regular)
            throw new IOException("The pending backup path is not a regular file.");
        using NSData data = NSData.FromArray(bytes);
        using NSUrl url = NSUrl.FromFilename(journal);
        if (!data.Save(url, atomically: true)) throw new IOException("The pending backup could not be staged.");
    }

    private void DeleteJournal()
    {
        EnsureDirectory(directory);
        if (files.FileExists(journal) && !files.Remove(journal, out _))
            throw new IOException("The completed backup journal could not be removed.");
        if (files.FileExists(journal)) throw new IOException("The completed backup journal remained present.");
    }

    private void EnsureDirectory(string path)
    {
        NSFileAttributes? attributes = files.GetAttributes(path, out _);
        if (attributes is null)
        {
            if (files.FileExists(path) || !files.CreateDirectory(path, true, (NSFileAttributes?)null, out _))
                throw new IOException("The private backup directory is unavailable.");
            attributes = files.GetAttributes(path, out _);
        }
        if (attributes?.Type != NSFileType.Directory)
            throw new IOException("The private backup directory is not a regular directory.");
    }

    private IReadOnlyDictionary<string, string?> ReadPreferences()
    {
        Dictionary<string, string?> result = new(StringComparer.Ordinal);
        foreach ((string alias, string key) in IOSBackupPreferences.Keys)
            result.Add(alias, preferences.StringForKey(key));
        IOSBackupPreferences.Validate(result);
        return result;
    }

    private void ApplyPreferences(IReadOnlyDictionary<string, string?> target)
    {
        IOSBackupPreferences.Validate(target);
        foreach ((string alias, string key) in IOSBackupPreferences.Keys)
        {
            string? value = target[alias];
            if (value is null) preferences.RemoveObject(key);
            else preferences.SetString(value, key);
        }
        if (!preferences.Synchronize()) throw new IOException("Restored preferences need a retry.");
        foreach ((string alias, string key) in IOSBackupPreferences.Keys)
            if (!string.Equals(preferences.StringForKey(key), target[alias], StringComparison.Ordinal))
                throw new IOException("Restored preferences did not verify.");
    }

    public void Dispose()
    {
        MorroIOSBackups.ReadJournal = null;
        MorroIOSBackups.WriteJournal = null;
        MorroIOSBackups.DeleteJournal = null;
        MorroIOSBackups.ReadPreferences = null;
        MorroIOSBackups.ValidatePreferences = null;
        MorroIOSBackups.ApplyPreferences = null;
    }
}
#endif

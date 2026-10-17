#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Morro.Backups;

public sealed record FullBackupState(string Name, string Platform,
    IReadOnlyDictionary<string, byte[]?> Files, string Achievements,
    IReadOnlyDictionary<string, string?> Preferences);

/// <summary>
/// Portable vanilla session archive. Reads only a fixed set of entries into
/// bounded memory; it never extracts paths or writes live game data. Platform
/// authorities validate vanilla serializers and their own preference allowlists.
/// </summary>
public static class FullBackupArchive
{
    public const int MaximumLogicalFileBytes = 64 * 1024 * 1024;
    public const int MaximumTotalFileBytes = 4 * MaximumLogicalFileBytes;
    public const int MaximumArchiveBytes = MaximumTotalFileBytes + 256 * 1024;
    public const int MaximumAchievementBytes = 8192;
    public const int MaximumPreferencesBytes = 128 * 1024;
    public const int MaximumManifestBytes = 16 * 1024;
    private const string ManifestName = "manifest.json";
    private const string AchievementName = "achievements.morro";
    private const string PreferencesName = "preferences.json";
    private static readonly string[] LogicalNames = { "settings", "0", "1", "2" };
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static readonly DateTimeOffset ArchiveTime = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static byte[] Create(FullBackupState state, Action<string> validateAchievements)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(validateAchievements);
        ValidateIdentity(state.Name, state.Platform);
        if (state.Files == null || state.Files.Count != LogicalNames.Length ||
            LogicalNames.Any(name => !state.Files.ContainsKey(name)))
            throw Invalid("A full backup must declare Settings and all three slots, including empty ones.");
        Dictionary<string, byte[]?> payloads = new(StringComparer.Ordinal);
        long total = 0;
        foreach (string name in LogicalNames)
        {
            byte[]? bytes = state.Files[name];
            if (bytes != null)
            {
                if (bytes.Length is < 1 or > MaximumLogicalFileBytes)
                    throw Invalid("A logical file exceeds the full-backup limit.");
                total += bytes.Length;
            }
            payloads.Add(name + ".celeste", bytes?.ToArray());
        }
        if (total > MaximumTotalFileBytes) throw Invalid("The full backup is too large.");
        byte[] achievements = EncodeAchievements(state.Achievements, validateAchievements);
        payloads.Add(AchievementName, achievements);
        payloads.Add(PreferencesName, EncodePreferences(state.Preferences));
        byte[] manifest = EncodeManifest(state.Name, state.Platform, payloads);
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            Add(archive, ManifestName, manifest);
            foreach (KeyValuePair<string, byte[]?> item in payloads)
                if (item.Value != null) Add(archive, item.Key, item.Value);
        }
        if (output.Length > MaximumArchiveBytes) throw Invalid("The full backup is too large.");
        return output.ToArray();
    }

    public static FullBackupState Read(byte[] bytes, Action<string> validateAchievements,
        int maximumFileBytes = MaximumLogicalFileBytes, int maximumTotalBytes = MaximumTotalFileBytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(validateAchievements);
        if (maximumFileBytes is < 1 or > MaximumLogicalFileBytes ||
            maximumTotalBytes is < 1 or > MaximumTotalFileBytes)
            throw new ArgumentOutOfRangeException(nameof(maximumFileBytes));
        if (bytes.Length is < 22 or > MaximumArchiveBytes) throw Invalid("The archive size is invalid.");
        try
        {
            ValidateCentralDirectory(bytes);
            using MemoryStream input = new(bytes, writable: false);
            using ZipArchive archive = new(input, ZipArchiveMode.Read);
            if (archive.Entries.Count is < 3 or > 7) throw Invalid("The archive has unexpected entries.");
            HashSet<string> allowed = new(LogicalNames.Select(name => name + ".celeste"), StringComparer.Ordinal)
            { ManifestName, AchievementName, PreferencesName };
            Dictionary<string, ZipArchiveEntry> entries = new(StringComparer.Ordinal);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                int unixKind = (entry.ExternalAttributes >> 16) & 0xF000;
                if (!allowed.Contains(entry.FullName) || !entries.TryAdd(entry.FullName, entry) ||
                    (unixKind != 0 && unixKind != 0x8000) || (entry.ExternalAttributes & 0x10) != 0)
                    throw Invalid("The archive has duplicate, non-file, or unsupported entries.");
            }
            if (!entries.TryGetValue(ManifestName, out ZipArchiveEntry? manifestEntry))
                throw Invalid("This ZIP is not a Morro full backup. Import older .celeste files individually.");
            using JsonDocument manifest = ParseJson(ReadEntry(manifestEntry, MaximumManifestBytes));
            JsonElement root = manifest.RootElement;
            RequireObject(root, "format", "version", "name", "platform", "files");
            if (String(root, "format") != "morro-full-backup" ||
                !Integer(root, "version", out int version) || version != 1)
                throw Invalid("The full-backup format is unsupported.");
            string name = String(root, "name"), platform = String(root, "platform");
            ValidateIdentity(name, platform);
            JsonElement descriptions = root.GetProperty("files");
            if (descriptions.ValueKind != JsonValueKind.Array || descriptions.GetArrayLength() != 6)
                throw Invalid("The backup manifest must declare every session component.");
            Dictionary<string, byte[]?> payloads = new(StringComparer.Ordinal);
            long total = 0;
            foreach (JsonElement description in descriptions.EnumerateArray())
            {
                RequireObject(description, "name", "present", "bytes", "sha256");
                string filename = String(description, "name");
                if (filename == ManifestName || !allowed.Contains(filename) || payloads.ContainsKey(filename))
                    throw Invalid("The backup manifest has duplicate or unknown components.");
                JsonElement presentValue = description.GetProperty("present");
                if (presentValue.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
                    !Integer(description, "bytes", out int length) || length < 0)
                    throw Invalid("The backup manifest has invalid component metadata.");
                bool present = presentValue.GetBoolean();
                string digest = String(description, "sha256");
                if (!present)
                {
                    if (filename is AchievementName or PreferencesName || length != 0 || digest.Length != 0 || entries.ContainsKey(filename))
                        throw Invalid("An empty slot contains unexpected data.");
                    payloads.Add(filename, null);
                    continue;
                }
                int limit = filename == AchievementName ? MaximumAchievementBytes :
                    filename == PreferencesName ? MaximumPreferencesBytes : maximumFileBytes;
                if (length is < 1 || length > limit || digest.Length != 64 ||
                    digest.Any(character => !Uri.IsHexDigit(character)) ||
                    !entries.TryGetValue(filename, out ZipArchiveEntry? entry) || entry.Length != length)
                    throw Invalid("A backup component is missing or exceeds its limit.");
                if (filename.EndsWith(".celeste", StringComparison.Ordinal))
                {
                    total += length;
                    if (total > maximumTotalBytes) throw Invalid("The game files exceed this device's backup limit.");
                }
                byte[] payload = ReadEntry(entry, limit);
                if (!CryptographicOperations.FixedTimeEquals(SHA256.HashData(payload), Convert.FromHexString(digest)))
                    throw Invalid("A backup component failed its checksum.");
                payloads.Add(filename, payload);
            }
            if (payloads.Count != 6 || LogicalNames.Any(key => !payloads.ContainsKey(key + ".celeste")) ||
                !payloads.ContainsKey(AchievementName) || !payloads.ContainsKey(PreferencesName) ||
                entries.Count != 1 + payloads.Count(pair => pair.Value != null))
                throw Invalid("The archive does not match its complete manifest.");
            string achievementText = Utf8.GetString(payloads[AchievementName]!);
            validateAchievements(achievementText);
            Dictionary<string, byte[]?> files = new(StringComparer.Ordinal);
            foreach (string logical in LogicalNames) files.Add(logical, payloads[logical + ".celeste"]);
            return new FullBackupState(name, platform,
                new ReadOnlyDictionary<string, byte[]?>(files), achievementText,
                DecodePreferences(payloads[PreferencesName]!));
        }
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException or
            FormatException or OverflowException or ArgumentException)
        {
            throw new InvalidDataException("The full backup could not be validated.", exception);
        }
    }

    public static string SuggestedFilename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Celeste backup.zip";
        const string forbidden = "/\\:*?\"<>|";
        string safe = new(name.Take(80).Select(c => char.IsControl(c) || forbidden.Contains(c) ? '_' : c).ToArray());
        safe = safe.Trim().Trim('.');
        return (safe.Length == 0 ? "Celeste backup" : safe) + ".zip";
    }

    private static byte[] EncodeAchievements(string value, Action<string> validate)
    {
        if (value == null || value.Length > MaximumAchievementBytes) throw Invalid("Achievement backup is invalid.");
        byte[] bytes = Utf8.GetBytes(value);
        if (bytes.Length is < 1 or > MaximumAchievementBytes) throw Invalid("Achievement backup is too large.");
        validate(value);
        return bytes;
    }

    private static byte[] EncodeManifest(string name, string platform, Dictionary<string, byte[]?> payloads)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("format", "morro-full-backup"); writer.WriteNumber("version", 1);
            writer.WriteString("name", name); writer.WriteString("platform", platform);
            writer.WriteStartArray("files");
            foreach (KeyValuePair<string, byte[]?> pair in payloads)
            {
                writer.WriteStartObject(); writer.WriteString("name", pair.Key);
                writer.WriteBoolean("present", pair.Value != null); writer.WriteNumber("bytes", pair.Value?.Length ?? 0);
                writer.WriteString("sha256", pair.Value == null ? "" : Convert.ToHexString(SHA256.HashData(pair.Value)));
                writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        return stream.ToArray();
    }

    private static byte[] EncodePreferences(IReadOnlyDictionary<string, string?> preferences)
    {
        if (preferences == null || preferences.Count > 32) throw Invalid("The preference list is invalid.");
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, string?> pair in preferences.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                ValidatePreference(pair.Key, pair.Value);
                writer.WriteString(pair.Key, pair.Value);
            }
            writer.WriteEndObject();
        }
        if (stream.Length > MaximumPreferencesBytes) throw Invalid("The preference data is too large.");
        return stream.ToArray();
    }

    private static IReadOnlyDictionary<string, string?> DecodePreferences(byte[] bytes)
    {
        using JsonDocument document = ParseJson(bytes);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw Invalid("Invalid preference data.");
        Dictionary<string, string?> values = new(StringComparer.Ordinal);
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                throw Invalid("Invalid preference value.");
            string? value = property.Value.GetString();
            ValidatePreference(property.Name, value);
            if (values.Count >= 32 || !values.TryAdd(property.Name, value)) throw Invalid("Duplicate or excessive preferences.");
        }
        return new ReadOnlyDictionary<string, string?>(values);
    }

    private static void ValidatePreference(string name, string? value)
    {
        if (name.Length is < 1 or > 64 || name.Any(c => !char.IsAsciiLetterOrDigit(c)) ||
            (value != null && value.Length > 32768)) throw Invalid("Invalid preference metadata.");
    }

    private static void ValidateIdentity(string name, string platform)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80 || name.Any(char.IsControl) ||
            platform is not ("ios" or "tvos")) throw Invalid("Invalid backup name or platform.");
    }

    private static void Add(ZipArchive archive, string name, byte[] bytes)
    {
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        entry.LastWriteTime = ArchiveTime;
        using Stream target = entry.Open(); target.Write(bytes);
    }

    private static byte[] ReadEntry(ZipArchiveEntry entry, int maximum)
    {
        if (entry.Length is < 1 || entry.Length > maximum) throw Invalid("An archive entry exceeds its limit.");
        byte[] bytes = new byte[checked((int)entry.Length)];
        using Stream source = entry.Open();
        source.ReadExactly(bytes);
        if (source.ReadByte() != -1) throw Invalid("An archive entry exceeds its declared length.");
        return bytes;
    }

    // ZipArchive materializes every central-directory entry before Entries.Count
    // can be checked. Bound that metadata first, including a dishonest EOCD
    // count, so a small compressed archive cannot allocate millions of entries.
    // Our bounded archives never need ZIP64, split volumes, or ZIP signatures.
    private static void ValidateCentralDirectory(byte[] bytes)
    {
        ReadOnlySpan<byte> data = bytes;
        // Generated full backups have no ZIP comment. Requiring the final EOCD
        // prevents an embedded signature from selecting a different directory
        // in this preflight and the framework reader.
        int end = bytes.Length - 22;
        if (BinaryPrimitives.ReadUInt32LittleEndian(data[end..]) != 0x06054B50 ||
            BinaryPrimitives.ReadUInt16LittleEndian(data[(end + 20)..]) != 0)
            throw Invalid("The ZIP directory is missing, incomplete, or unsupported.");
        ReadOnlySpan<byte> eocd = data[end..];
        int count = BinaryPrimitives.ReadUInt16LittleEndian(eocd[10..]);
        uint size = BinaryPrimitives.ReadUInt32LittleEndian(eocd[12..]);
        uint start = BinaryPrimitives.ReadUInt32LittleEndian(eocd[16..]);
        if (count is < 3 or > 7 || BinaryPrimitives.ReadUInt16LittleEndian(eocd[4..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(eocd[6..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(eocd[8..]) != count ||
            size > 65536 || (ulong)start + size != (ulong)end)
            throw Invalid("The ZIP directory exceeds the supported full-backup format.");
        int position = checked((int)start);
        for (int index = 0; index < count; index++)
        {
            if (end - position < 46 || BinaryPrimitives.ReadUInt32LittleEndian(data[position..]) != 0x02014B50)
                throw Invalid("The ZIP directory entries are invalid.");
            ReadOnlySpan<byte> header = data[position..];
            int length = 46 + BinaryPrimitives.ReadUInt16LittleEndian(header[28..]) +
                BinaryPrimitives.ReadUInt16LittleEndian(header[30..]) +
                BinaryPrimitives.ReadUInt16LittleEndian(header[32..]);
            if (length > end - position || BinaryPrimitives.ReadUInt16LittleEndian(header[34..]) != 0 ||
                BinaryPrimitives.ReadUInt32LittleEndian(header[20..]) == uint.MaxValue ||
                BinaryPrimitives.ReadUInt32LittleEndian(header[24..]) == uint.MaxValue ||
                BinaryPrimitives.ReadUInt32LittleEndian(header[42..]) == uint.MaxValue)
                throw Invalid("The ZIP directory entry is unsupported.");
            position += length;
        }
        if (position != end) throw Invalid("The ZIP directory entry count is incorrect.");
    }

    private static JsonDocument ParseJson(byte[] bytes) => JsonDocument.Parse(bytes,
        new JsonDocumentOptions { MaxDepth = 8, CommentHandling = JsonCommentHandling.Disallow });

    private static void RequireObject(JsonElement value, params string[] properties)
    {
        if (value.ValueKind != JsonValueKind.Object) throw Invalid("Expected a manifest object.");
        HashSet<string> remaining = new(properties, StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
            if (!remaining.Remove(property.Name)) throw Invalid("Unexpected or duplicate manifest property.");
        if (remaining.Count != 0) throw Invalid("Incomplete backup manifest.");
    }

    private static string String(JsonElement parent, string key)
    {
        JsonElement value = parent.GetProperty(key);
        if (value.ValueKind != JsonValueKind.String) throw Invalid("Invalid manifest text.");
        return value.GetString()!;
    }

    private static bool Integer(JsonElement parent, string key, out int value)
    {
        value = 0;
        JsonElement element = parent.GetProperty(key);
        return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out value);
    }

    private static InvalidDataException Invalid(string message) => new(message);
}

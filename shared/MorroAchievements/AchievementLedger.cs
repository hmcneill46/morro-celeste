#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Morro.Achievements;

/// <summary>
/// Installation-wide, monotonic unlocks. The platform supplies two small private
/// preference records; ordinary Celeste saves and Settings never enter this store.
/// A zero timestamp means the unlock was recovered from a save with no known date.
/// </summary>
public sealed class AchievementLedger
{
    public const int MaximumAchievements = 64;
    public const int MaximumRecordCharacters = 8192;
    private const long MaximumTimestamp = 253402300799;
    private readonly object gate = new();
    private readonly string[] ids;
    private readonly HashSet<string> approved;
    private readonly Dictionary<string, long> unlocked = new(StringComparer.Ordinal);
    private readonly Func<string, string?> read;
    private readonly Action<string, string> write;
    private readonly string keyA;
    private readonly string keyB;
    private bool dirty;

    public AchievementLedger(IReadOnlyList<string> canonicalIds, Func<string, string?> read,
        Action<string, string> write, string keyPrefix = "Morro.Achievements.v1")
    {
        ArgumentNullException.ThrowIfNull(canonicalIds);
        this.read = read ?? throw new ArgumentNullException(nameof(read));
        this.write = write ?? throw new ArgumentNullException(nameof(write));
        if (canonicalIds.Count is < 1 or > MaximumAchievements)
            throw new ArgumentException("The achievement catalog must contain between 1 and 64 IDs.", nameof(canonicalIds));
        if (string.IsNullOrWhiteSpace(keyPrefix) || keyPrefix.Length > 128)
            throw new ArgumentException("A bounded preference key prefix is required.", nameof(keyPrefix));
        ids = new string[canonicalIds.Count];
        approved = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < ids.Length; i++)
        {
            string id = canonicalIds[i];
            if (!IsSafeId(id) || !approved.Add(id))
                throw new ArgumentException("Achievement IDs must be unique bounded ASCII identifiers.", nameof(canonicalIds));
            ids[i] = id;
        }
        keyA = keyPrefix + ".A";
        keyB = keyPrefix + ".B";
        // Never emit notifications or create preference records during restore.
        bool readA = TryRead(keyA, out Replica? a);
        bool readB = TryRead(keyB, out Replica? b);
        if (readA) Merge(a);
        if (readB) Merge(b);
        dirty = !readA || !readB;
    }

    public event Action<string>? Unlocked;
    public int UnlockedCount { get { lock (gate) return unlocked.Count; } }
    public bool IsDirty { get { lock (gate) return dirty; } }

    public bool Has(string id)
    {
        ValidateId(id);
        lock (gate) return unlocked.ContainsKey(id);
    }

    public long? UnlockedAtUtcSeconds(string id)
    {
        ValidateId(id);
        lock (gate) return unlocked.TryGetValue(id, out long value) ? value : null;
    }

    public IReadOnlyDictionary<string, long> Snapshot()
    {
        lock (gate)
            return new ReadOnlyDictionary<string, long>(new Dictionary<string, long>(unlocked, StringComparer.Ordinal));
    }

    /// <summary>
    /// Returns true exactly once for each newly observed ID. A failed preference
    /// write retains the unlock in memory and leaves IsDirty true for Flush retry.
    /// Backfill passes timestamp zero, notify false and flushImmediately false,
    /// then calls Flush once for the batch; it cannot revoke an unlock.
    /// </summary>
    public bool TryUnlock(string id, long? unlockedAtUtcSeconds = null, bool notify = true, bool flushImmediately = true)
    {
        ValidateId(id);
        long timestamp = unlockedAtUtcSeconds ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (timestamp is < 0 or > MaximumTimestamp)
            throw new ArgumentOutOfRangeException(nameof(unlockedAtUtcSeconds));
        lock (gate)
        {
            if (unlocked.ContainsKey(id)) return false;
            unlocked.Add(id, timestamp);
            dirty = true;
            if (flushImmediately) Flush();
        }
        if (notify) Unlocked?.Invoke(id);
        return true;
    }

    /// <summary>
    /// Writes the merged unlocks to both replicas, replacing the older one first.
    /// Failed writes remain pending even when a preference cache returns the new
    /// value: only the platform writer can confirm its synchronization completed.
    /// </summary>
    public bool Flush()
    {
        lock (gate)
        {
            if (!dirty) return true;
            // A transient read failure must not erase a replica we cannot inspect.
            if (!TryRead(keyA, out Replica? a) || !TryRead(keyB, out Replica? b)) return false;
            Merge(a);
            Merge(b);
            long generation = Math.Max(a?.Generation ?? 0, b?.Generation ?? 0);
            if (generation == long.MaxValue) return false;
            string first = a == null || (b != null && a.Generation <= b.Generation) ? keyA : keyB;
            string second = first == keyA ? keyB : keyA;
            string encoded = Encode(generation + 1);
            // Retry both writes even when their cached values already match.
            // This preserves an explicit persistence acknowledgement for each.
            if (!WriteAndVerify(first, encoded) || !WriteAndVerify(second, encoded)) return false;
            dirty = false;
            return true;
        }
    }

    private bool WriteAndVerify(string key, string encoded)
    {
        try
        {
            write(key, encoded);
            return string.Equals(read(key), encoded, StringComparison.Ordinal);
        }
        catch (Exception) { return false; }
    }

    private void ValidateId(string id)
    {
        if (id == null || !approved.Contains(id))
            throw new ArgumentException("Unknown achievement ID.", nameof(id));
    }

    private static bool IsSafeId(string? id)
    {
        if (id == null || id.Length is < 1 or > 64) return false;
        foreach (char value in id)
            if (value is not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z') and
                not (>= '0' and <= '9') and not '_') return false;
        return true;
    }

    private bool TryRead(string key, out Replica? result)
    {
        result = null;
        try { result = Decode(read(key)); return true; }
        catch (Exception) { return false; }
    }

    private void Merge(Replica? replica)
    {
        if (replica == null) return;
        foreach (KeyValuePair<string, long> entry in replica.Unlocks)
        {
            if (!unlocked.TryGetValue(entry.Key, out long previous)) unlocked.Add(entry.Key, entry.Value);
            // Unknown historical dates remain unknown; otherwise retain the first date.
            else unlocked[entry.Key] = Math.Min(previous, entry.Value);
        }
    }

    private string Encode(long generation)
    {
        StringBuilder body = new("MORROACH|1|");
        body.Append(generation.ToString(CultureInfo.InvariantCulture)).Append('|');
        bool first = true;
        foreach (string id in ids)
        {
            if (!unlocked.TryGetValue(id, out long timestamp)) continue;
            if (!first) body.Append(';');
            first = false;
            body.Append(id).Append('=').Append(timestamp.ToString(CultureInfo.InvariantCulture));
        }
        string value = body.ToString();
        return value + "\n" + Checksum(value);
    }

    private Replica? Decode(string? value)
    {
        if (value == null || value.Length > MaximumRecordCharacters) return null;
        int split = value.LastIndexOf('\n');
        if (split < 0 || value.Length - split - 1 != 64) return null;
        string body = value[..split];
        if (!string.Equals(value[(split + 1)..], Checksum(body), StringComparison.Ordinal)) return null;
        string[] fields = body.Split('|');
        if (fields.Length != 4 || fields[0] != "MORROACH" || fields[1] != "1" ||
            !long.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out long generation) || generation < 1)
            return null;
        Dictionary<string, long> entries = new(StringComparer.Ordinal);
        if (fields[3].Length != 0)
        {
            string[] records = fields[3].Split(';');
            if (records.Length > ids.Length) return null;
            foreach (string record in records)
            {
                int equals = record.IndexOf('=');
                if (equals < 1) return null;
                string id = record[..equals];
                if (!approved.Contains(id) ||
                    !long.TryParse(record[(equals + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out long timestamp) ||
                    timestamp > MaximumTimestamp || !entries.TryAdd(id, timestamp)) return null;
            }
        }
        return new Replica(generation, entries);
    }

    private static string Checksum(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record Replica(long Generation, Dictionary<string, long> Unlocks);
}

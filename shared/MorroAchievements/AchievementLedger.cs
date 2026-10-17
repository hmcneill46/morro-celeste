#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Morro.Achievements;

/// <summary>
/// Monotonic unlocks within a local epoch. Explicit backup replacement starts
/// a new epoch, so an older replica can never restore removed achievements.
/// The platform supplies two small private
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
    private readonly string catalogHash;
    private long epoch;
    private bool automaticSaveRecovery = true;
    private Dictionary<string, long>? pendingReplacement;
    private long? pendingEpoch;
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
        catalogHash = Checksum(string.Join("\n", ids));
        // Never emit notifications or create preference records during restore.
        bool readA = TryRead(keyA, out Replica? a);
        bool readB = TryRead(keyB, out Replica? b);
        MergeCurrentEpoch(a, b);
        dirty = !readA || !readB || epoch > 0 &&
            (a?.Epoch != epoch || b?.Epoch != epoch);
    }

    public event Action<string>? Unlocked;
    public int UnlockedCount { get { lock (gate) return unlocked.Count; } }
    public bool IsDirty { get { lock (gate) return dirty; } }
    public bool AutomaticSaveRecovery { get { lock (gate) return automaticSaveRecovery; } }

    /// <summary>Portable state contains no local keys, scope, epoch or generation.</summary>
    public string ExportBackup()
    {
        lock (gate) return EncodeBackup(unlocked, automaticSaveRecovery);
    }

    public string EmptyBackup() => EncodeBackup(new Dictionary<string, long>(StringComparer.Ordinal), false);

    public void ValidateBackup(string value) => DecodeBackup(value);

    public string NormalizeBackup(string value, bool automaticSaveRecovery = false) =>
        EncodeBackup(DecodeBackup(value).Unlocks, automaticSaveRecovery);

    /// <summary>
    /// Replaces, never unions, the exact portable state. False means the target
    /// remains pending in memory and may already exist in one durable replica;
    /// callers must keep their mutation guard until Flush succeeds or restart
    /// recovery resolves their enclosing transaction. No unlock events are sent.
    /// Explicit replacement disables automatic save recovery persistently.
    /// Only an enclosing platform journal may preserve the old recovery policy
    /// or supersede a pending target to roll back its transaction.
    /// </summary>
    public bool ReplaceBackup(string value, bool preserveRecoveryPolicy = false)
    {
        Backup decoded = DecodeBackup(value);
        Dictionary<string, long> target = decoded.Unlocks;
        bool recovery = preserveRecoveryPolicy && decoded.AutomaticSaveRecovery;
        lock (gate)
        {
            if (pendingReplacement != null)
            {
                if (EncodeBackup(pendingReplacement, automaticSaveRecovery) == EncodeBackup(target, recovery))
                    return Flush();
                if (!preserveRecoveryPolicy)
                    throw new InvalidOperationException("An achievement replacement is already pending.");
            }
            pendingReplacement = target;
            pendingEpoch = null;
            unlocked.Clear();
            foreach (KeyValuePair<string, long> entry in target) unlocked.Add(entry.Key, entry.Value);
            automaticSaveRecovery = recovery;
            dirty = true; // Even an empty reset must leave durable tombstones.
            return Flush();
        }
    }

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
            if (pendingReplacement != null) return false;
            if (unlocked.ContainsKey(id)) return false;
            long eventEpoch = epoch;
            unlocked.Add(id, timestamp);
            dirty = true;
            if (flushImmediately) Flush();
            if (epoch != eventEpoch) return false; // A newer session retired this event.
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
            if (pendingReplacement != null)
            {
                if (pendingEpoch == null)
                {
                    long highest = Math.Max(epoch, Math.Max(a?.Epoch ?? 0, b?.Epoch ?? 0));
                    if (highest == long.MaxValue) return false;
                    pendingEpoch = highest + 1;
                }
                // A different writer must never turn an exact pending target
                // into a union. Keep the operation pending for its coordinator.
                if ((a?.Epoch ?? 0) > pendingEpoch.Value || (b?.Epoch ?? 0) > pendingEpoch.Value)
                    return false;
                epoch = pendingEpoch.Value;
            }
            else MergeCurrentEpoch(a, b);
            long generation = Math.Max(a?.Epoch == epoch ? a.Generation : 0,
                b?.Epoch == epoch ? b.Generation : 0);
            if (generation == long.MaxValue) return false;
            string first = a == null || (b != null &&
                (a.Epoch < b.Epoch || a.Epoch == b.Epoch && a.Generation <= b.Generation)) ? keyA : keyB;
            string second = first == keyA ? keyB : keyA;
            string encoded = Encode(generation + 1);
            // Retry both writes even when their cached values already match.
            // This preserves an explicit persistence acknowledgement for each.
            if (!WriteAndVerify(first, encoded) || !WriteAndVerify(second, encoded)) return false;
            pendingReplacement = null;
            pendingEpoch = null;
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

    private void MergeCurrentEpoch(Replica? a, Replica? b)
    {
        long highest = Math.Max(epoch, Math.Max(a?.Epoch ?? 0, b?.Epoch ?? 0));
        if (highest > epoch)
        {
            unlocked.Clear();
            epoch = highest;
            automaticSaveRecovery = true;
        }
        Merge(a);
        Merge(b);
    }

    private void Merge(Replica? replica)
    {
        if (replica == null || replica.Epoch != epoch) return;
        automaticSaveRecovery &= replica.AutomaticSaveRecovery;
        foreach (KeyValuePair<string, long> entry in replica.Unlocks)
        {
            if (!unlocked.TryGetValue(entry.Key, out long previous)) unlocked.Add(entry.Key, entry.Value);
            // Unknown historical dates remain unknown; otherwise retain the first date.
            else unlocked[entry.Key] = Math.Min(previous, entry.Value);
        }
    }

    private string Encode(long generation)
    {
        string prefix = epoch == 0 && automaticSaveRecovery
            ? "MORROACH|1|" + generation.ToString(CultureInfo.InvariantCulture) + "|"
            : "MORROACH|2|" + epoch.ToString(CultureInfo.InvariantCulture) + "|" +
                generation.ToString(CultureInfo.InvariantCulture) + "|" + (automaticSaveRecovery ? "1" : "0") + "|";
        return WithChecksum(prefix + EncodeEntries(unlocked));
    }

    private string EncodeBackup(IReadOnlyDictionary<string, long> entries, bool recovery) =>
        WithChecksum("MORROACHBACKUP|1|" + catalogHash + "|" + (recovery ? "1" : "0") + "|" + EncodeEntries(entries));

    private string EncodeEntries(IReadOnlyDictionary<string, long> entries)
    {
        StringBuilder body = new();
        bool first = true;
        foreach (string id in ids)
        {
            if (!entries.TryGetValue(id, out long timestamp)) continue;
            if (!first) body.Append(';');
            first = false;
            body.Append(id).Append('=').Append(timestamp.ToString(CultureInfo.InvariantCulture));
        }
        return body.ToString();
    }

    private Replica? Decode(string? value)
    {
        string[]? fields = VerifiedFields(value);
        if (fields == null || fields.Length < 4 || fields[0] != "MORROACH") return null;
        long recordEpoch = 0;
        bool recovery = true;
        string generationField, entriesField;
        if (fields.Length == 4 && fields[1] == "1")
        {
            generationField = fields[2]; entriesField = fields[3];
        }
        else if (fields.Length == 6 && fields[1] == "2" &&
            long.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out recordEpoch) &&
            recordEpoch >= 1 && fields[4] is "0" or "1")
        {
            generationField = fields[3]; entriesField = fields[5]; recovery = fields[4] == "1";
        }
        else return null;
        if (!long.TryParse(generationField, NumberStyles.None, CultureInfo.InvariantCulture, out long generation) || generation < 1)
            return null;
        Dictionary<string, long>? entries = DecodeEntries(entriesField);
        return entries == null ? null : new Replica(recordEpoch, generation, recovery, entries);
    }

    private Backup DecodeBackup(string value)
    {
        string[]? fields = VerifiedFields(value);
        if (fields == null || fields.Length != 5 || fields[0] != "MORROACHBACKUP" ||
            fields[1] != "1" || fields[2] != catalogHash || fields[3] is not ("0" or "1"))
            throw new ArgumentException("Invalid or incompatible achievement backup.", nameof(value));
        Dictionary<string, long> entries = DecodeEntries(fields[4]) ??
            throw new ArgumentException("Invalid achievement backup entries.", nameof(value));
        return new Backup(fields[3] == "1", entries);
    }

    private static string[]? VerifiedFields(string? value)
    {
        if (value == null || value.Length > MaximumRecordCharacters ||
            Encoding.UTF8.GetByteCount(value) > MaximumRecordCharacters) return null;
        int split = value.LastIndexOf('\n');
        if (split < 0 || value.Length - split - 1 != 64) return null;
        string body = value[..split];
        if (!string.Equals(value[(split + 1)..], Checksum(body), StringComparison.Ordinal)) return null;
        return body.Split('|');
    }

    private Dictionary<string, long>? DecodeEntries(string value)
    {
        Dictionary<string, long> entries = new(StringComparer.Ordinal);
        if (value.Length != 0)
        {
            string[] records = value.Split(';');
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
        return entries;
    }

    private static string WithChecksum(string value) => value + "\n" + Checksum(value);

    private static string Checksum(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed record Replica(long Epoch, long Generation, bool AutomaticSaveRecovery,
        Dictionary<string, long> Unlocks);
    private sealed record Backup(bool AutomaticSaveRecovery, Dictionary<string, long> Unlocks);
}

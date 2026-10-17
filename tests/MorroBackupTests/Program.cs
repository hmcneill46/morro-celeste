using System.IO.Compression;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using Morro.Backups;

int checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    checks++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (InvalidDataException) { checks++; return; }
    throw new Exception("Accepted invalid backup: " + name);
}
void Validate(string value)
{
    if (value != "owned-achievement-fixture" && value != "empty-fixture")
        throw new InvalidDataException("Owned fixture rejected.");
}
FullBackupState State(string name, bool empty = false) => new(name, "ios",
    new Dictionary<string, byte[]?> {
        ["settings"] = Encoding.UTF8.GetBytes("owned-settings"),
        ["0"] = empty ? null : Encoding.UTF8.GetBytes("owned-slot-zero"),
        ["1"] = null, ["2"] = empty ? null : Encoding.UTF8.GetBytes("owned-slot-two")
    }, empty ? "empty-fixture" : "owned-achievement-fixture",
    new Dictionary<string, string?> { ["controllerPrompts"] = "Automatic", ["touchLayoutPhone"] = null });

byte[] Modify(byte[] source, Action<Dictionary<string, byte[]>> edit, string? extra = null, int? attributes = null)
{
    Dictionary<string, byte[]> entries = new(StringComparer.Ordinal);
    using (MemoryStream input = new(source))
    using (ZipArchive archive = new(input, ZipArchiveMode.Read))
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            using MemoryStream copy = new();
            using Stream stream = entry.Open(); stream.CopyTo(copy);
            entries.Add(entry.FullName, copy.ToArray());
        }
    edit(entries);
    using MemoryStream output = new();
    using (ZipArchive archive = new(output, ZipArchiveMode.Create, true))
    {
        foreach (var pair in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(pair.Key);
            if (attributes.HasValue && pair.Key == "0.celeste") entry.ExternalAttributes = attributes.Value;
            using Stream stream = entry.Open(); stream.Write(pair.Value);
        }
        if (extra != null)
        {
            using Stream stream = archive.CreateEntry(extra).Open(); stream.WriteByte(1);
        }
    }
    return output.ToArray();
}
byte[] Manifest(byte[] source, Action<JsonObject> edit) => Modify(source, entries =>
{
    JsonObject manifest = JsonNode.Parse(entries["manifest.json"])!.AsObject();
    edit(manifest); entries["manifest.json"] = Encoding.UTF8.GetBytes(manifest.ToJsonString());
});
byte[] Payload(byte[] source, string entry, byte[] payload) => Modify(source, entries =>
{
    entries[entry] = payload;
    JsonObject manifest = JsonNode.Parse(entries["manifest.json"])!.AsObject();
    JsonObject descriptor = manifest["files"]!.AsArray().Select(node => node!.AsObject())
        .Single(node => node["name"]!.GetValue<string>() == entry);
    descriptor["bytes"] = payload.Length; descriptor["sha256"] = Convert.ToHexString(SHA256.HashData(payload));
    entries["manifest.json"] = Encoding.UTF8.GetBytes(manifest.ToJsonString());
});

byte[] harry = FullBackupArchive.Create(State("Harry's Celeste"), Validate);
byte[] test = FullBackupArchive.Create(State("Test Session", true), Validate);
foreach (byte[] bytes in new[] { harry, test, harry })
{
    FullBackupState restored = FullBackupArchive.Read(bytes, Validate);
    bool empty = restored.Name == "Test Session";
    Check(restored.Files.Count == 4 && restored.Files["1"] == null, "Absent slots explicit");
    Check((restored.Files["0"] == null) == empty && (restored.Files["2"] == null) == empty, "Exact session membership");
    Check(restored.Achievements == (empty ? "empty-fixture" : "owned-achievement-fixture"), "Independent achievement states");
    Check(restored.Preferences["controllerPrompts"] == "Automatic" && restored.Preferences["touchLayoutPhone"] == null,
        "Present and default preferences preserved");
    Check(bytes.SequenceEqual(FullBackupArchive.Create(restored, Validate)), "Deterministic portable round trip");
}
Check(FullBackupArchive.Read(FullBackupArchive.Create(State("TV") with { Platform = "tvos" }, Validate), Validate).Platform == "tvos", "Cross-platform identity");
Check(FullBackupArchive.SuggestedFilename("Harry's Celeste") == "Harry's Celeste.zip", "Friendly filename");
Check(!FullBackupArchive.SuggestedFilename("../a\\b:bad\n").Contains('/'), "Safe filename");
Check(FullBackupArchive.SuggestedFilename("...") == "Celeste backup.zip", "Empty filename fallback");
Reject(() => FullBackupArchive.Read(harry, Validate, 4, 100), "Device file cap");
Reject(() => FullBackupArchive.Read(harry, Validate, 100, 20), "Device aggregate cap");
Reject(() => FullBackupArchive.Read(harry[..(harry.Length / 2)], Validate), "Truncated ZIP");
Reject(() => FullBackupArchive.Read(Modify(harry, _ => {}, "../escape"), Validate), "Traversal");
Reject(() => FullBackupArchive.Read(Modify(test, _ => {}, "manifest.json"), Validate), "Duplicate ZIP entry");
Reject(() => FullBackupArchive.Read(Modify(harry, _ => {}, attributes: unchecked((int)0xA0000000)), Validate), "Symlink entry");
Reject(() => FullBackupArchive.Read(Modify(harry, _ => {}, attributes: 0x10), Validate), "Directory entry");
Reject(() => FullBackupArchive.Read(Modify(harry, entries => entries.Remove("achievements.morro")), Validate), "Missing achievements");
Reject(() => FullBackupArchive.Read(Modify(harry, entries => entries["0.celeste"][0] ^= 1), Validate), "Payload checksum");
Reject(() => FullBackupArchive.Read(Modify(harry, entries => entries["1.celeste"] = new byte[] { 1 }), Validate), "Unexpected absent file");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["version"] = 2), Validate), "Unknown version");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["version"] = "1"), Validate), "Wrong version type");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["platform"] = "desktop"), Validate), "Unknown platform");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["unknown"] = "x"), Validate), "Unknown manifest property");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["files"]!.AsArray().RemoveAt(0)), Validate), "Incomplete file declarations");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["files"]![1]!["bytes"] = "5"), Validate), "Wrong length type");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["files"]![1]!["bytes"] = 999999999), Validate), "Inflated length");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["files"]![1]!["name"] = "settings.celeste"), Validate), "Duplicate declarations");
Reject(() => FullBackupArchive.Read(Manifest(harry, m => m["files"]![4]!["present"] = false), Validate), "Absent achievements");
Reject(() => FullBackupArchive.Read(Modify(harry, entries => entries["manifest.json"] = Encoding.UTF8.GetBytes(
    Encoding.UTF8.GetString(entries["manifest.json"]).Replace("\"version\":1", "\"version\":1,\"version\":1"))), Validate), "Duplicate JSON property");
Reject(() => FullBackupArchive.Read(Payload(harry, "preferences.json", Encoding.UTF8.GetBytes("{\"controllerPrompts\":\"A\",\"controllerPrompts\":\"B\"}")), Validate), "Duplicate preference");
Reject(() => FullBackupArchive.Read(Payload(harry, "preferences.json", Encoding.UTF8.GetBytes("{\"controllerPrompts\":42}")), Validate), "Non-string preference");
Reject(() => FullBackupArchive.Read(Payload(harry, "preferences.json", Encoding.UTF8.GetBytes("{\"private.key\":\"x\"}")), Validate), "Non-semantic preference key");
Reject(() => FullBackupArchive.Read(Payload(harry, "achievements.morro", new byte[] { 0xff, 0xfe }), Validate), "Invalid UTF8");
Reject(() => FullBackupArchive.Read(Payload(harry, "achievements.morro", Encoding.UTF8.GetBytes("invalid-achievement")), Validate), "Semantic achievement rejection");
Reject(() => FullBackupArchive.Read(Payload(harry, "0.celeste", new byte[2 * 1024 * 1024]), Validate, 256 * 1024), "Compressed over-limit file");
Reject(() => FullBackupArchive.Create(State(" "), Validate), "Blank name");
Reject(() => FullBackupArchive.Create(State("bad\nname"), Validate), "Control in name");
Reject(() => FullBackupArchive.Create(State("x") with { Files = new Dictionary<string, byte[]?> { ["0"] = null } }, Validate), "Incomplete export");
Reject(() => FullBackupArchive.Create(State("x") with { Achievements = "invalid" }, Validate), "Invalid export achievements");
byte[] ManyEntries()
{
    using MemoryStream stream = new();
    using (ZipArchive zip = new(stream, ZipArchiveMode.Create, true))
        for (int i = 0; i < 1000; i++) zip.CreateEntry("entry" + i);
    return stream.ToArray();
}
byte[] many = ManyEntries();
Reject(() => FullBackupArchive.Read(many, Validate), "Excessive directory count preflight");
BinaryPrimitives.WriteUInt16LittleEndian(many.AsSpan(many.Length - 22 + 8), 3);
BinaryPrimitives.WriteUInt16LittleEndian(many.AsSpan(many.Length - 22 + 10), 3);
Reject(() => FullBackupArchive.Read(many, Validate), "False directory entry count");
byte[] split = harry.ToArray();
BinaryPrimitives.WriteUInt16LittleEndian(split.AsSpan(split.Length - 22 + 4), 1);
Reject(() => FullBackupArchive.Read(split, Validate), "Split ZIP");
byte[] comment = new byte[harry.Length + 4];
harry.CopyTo(comment, 0);
BinaryPrimitives.WriteUInt16LittleEndian(comment.AsSpan(harry.Length - 2), 4);
Reject(() => FullBackupArchive.Read(comment, Validate), "Unsupported ZIP comment");
Console.WriteLine($"PASS: {checks} portable full-backup checks; no device persistence accessed.");

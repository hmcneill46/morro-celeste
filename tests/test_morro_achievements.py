"""Portable achievement tooling checks using tiny, project-owned fixtures.

No Celeste source, artwork, saves, Apple SDK, network, or device is required.
The source fragments are synthetic anchors, not redistributed game classes.
"""

from __future__ import annotations

import contextlib
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest import mock


ROOT = Path(__file__).resolve().parents[1]


def load(name, filename):
    spec = importlib.util.spec_from_file_location(name, ROOT / "scripts" / filename)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


prepare = load("achievement_prepare_tests", "prepare-morro-achievements.py")
verify = load("achievement_verify_tests", "verify-morro-achievements.py")
IDS = (["CH" + str(n) for n in range(1, 8)] +
       ["STRB1", "STRB2", "STRB3", "CASS", "ONEUP", "PICO8"] +
       ["HEART" + str(n) for n in range(1, 9)] +
       ["BSIDE" + str(n) for n in range(1, 9)] + ["CSIDES", "FAREWELL", "WOW"])
ANCHORS = {
    "Celeste/Achievements.cs": ("\t\treturn false;", "\t\tHas(achievement);"),
    "Celeste/Celeste.cs": ("\t\tbase.Update(gameTime);", "\t\tbase.RenderCore();"),
    "Celeste/MenuOptions.cs": ("\t\tviewport.Visible = Settings.Instance.Fullscreen;",
                               "\tprivate static void OpenViewportAdjustment()\n\t{"),
    "Celeste/SaveData.cs": ("\t\tInstance.AfterInitialize();",),
    "Celeste/OuiFileSelect.cs": ("\t\t\t\t\t\tsaveData.AfterInitialize();",),
}


IOS_ANCHORS = {
    "Celeste/IOSDataFilesUI.cs": (
        "    public static bool MutationSafe => Engine.Scene is not Level && !UserIO.Saving;",
        '        Add(new Button("Export All Saves...").Pressed(() => Run(IOSDataFiles.ExportAll)));'),
    "Celeste/IOSStorageHooks.cs": (
        "    public static bool SaveLogicalFile(string logicalName, byte[] payload)\n    {",
        "    public static bool DeleteLogicalFile(string logicalName)\n    {"),
    "Celeste/UserIO.cs": ("\tpublic static void SaveHandler(bool file, bool settings)\n\t{",),
}


class AchievementToolingTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="morro-achievement-tests-")
        self.addCleanup(temporary.cleanup)
        # macOS can expose /var through a symlink; use a resolved fixture root.
        self.root = Path(temporary.name).resolve()
        subprocess.run(["git", "init", "-q", str(self.root)], check=True,
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        (self.root / ".gitignore").write_text(".build/\n")
        self.source = self.root / ".build/celeste-ios/current"
        self.managed = self.source / "managed"
        self.output = self.root / ".build/morro-achievements/output"
        self.cache = self.root / ".build/morro-achievements/cache"
        self.cache.mkdir(parents=True)
        self.managed.mkdir(parents=True)
        for name, anchors in (ANCHORS | IOS_ANCHORS).items():
            path = self.managed / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("// Synthetic, project-owned transform fixture.\n" + "\n".join(anchors) + "\n")
        (self.managed / "Celeste/Achievement.cs").write_text(
            "enum Achievement\n{\n" + ",\n".join(IDS) + "\n}\n")
        (self.managed / "Celeste/Settings.cs").write_text("// Unchanged owned settings fixture.\n")
        content = self.source / "content/Content"
        content.mkdir(parents=True)
        (content / "owned-fixture.txt").write_text("Original owned content.\n")

        for name in (*prepare.tracked_sources("ios"), "scripts/celeste-stage3b.py",
                     "docs/licenses/ACHIEVEMENT_HELPER_LICENSE.txt"):
            target = self.root / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(ROOT / name, target)
        self.catalog = json.loads((ROOT / "managed/morro-achievements.json").read_text())
        self.icon_bytes = {}
        for achievement in self.catalog["achievements"]:
            for kind in ("icon", "lockedIcon"):
                data = ("Owned fixture bytes: " + achievement["id"] + "/" + kind).encode()
                filename = hashlib.sha1(data).hexdigest() + ".jpg"
                self.icon_bytes[filename] = data
                achievement[kind] = {
                    "fileName": filename,
                    "url": "https://cdn.akamai.steamstatic.com/steamcommunity/public/images/apps/504230/" + filename,
                    "sha256": hashlib.sha256(data).hexdigest(),
                }
                (self.cache / filename).write_bytes(data)
        self.catalog_path = self.root / "managed/morro-achievements.json"
        self.write_catalog()
        before = prepare.manifest(self.managed)
        resources = prepare.content_manifest([content])
        stack = contextlib.ExitStack()
        self.addCleanup(stack.close)
        stack.enter_context(mock.patch.object(prepare, "ROOT", self.root))
        stack.enter_context(mock.patch.object(prepare, "CATALOG", self.catalog_path))
        stack.enter_context(mock.patch.object(prepare, "INPUTS", {
            "ios": (before["fileCount"], before["logicalSha256"]),
            "tvos": (before["fileCount"], before["logicalSha256"]),
        }))
        stack.enter_context(mock.patch.object(prepare, "CONTENT_PIN", (
            resources["fileCount"], resources["totalBytes"], resources["logicalSha256"])))
        stack.enter_context(mock.patch.object(verify, "ROOT", self.root))
        stack.enter_context(mock.patch.object(verify, "prepare", prepare))

    def write_catalog(self):
        self.catalog_path = self.root / "managed/morro-achievements.json"
        self.catalog_path.write_text(json.dumps(self.catalog))

    def generate(self, *extra):
        argv = ["prepare-morro-achievements.py", "--platform", "ios", "--source-root", str(self.source),
                "--output-root", str(self.output), "--icon-cache", str(self.cache), "--offline", *extra]
        with mock.patch.object(sys, "argv", argv), contextlib.redirect_stdout(io.StringIO()):
            return prepare.main()

    def icon_path(self):
        return self.output / "content/Content/Graphics/MorroAchievements/CH1.jpg"

    def report(self):
        return json.loads((self.output / "achievement-manifest.json").read_text())

    def write_report(self, value):
        (self.output / "achievement-manifest.json").write_text(json.dumps(value))

    def test_published_catalog_matches_exact_vanilla_enum_and_secrets(self):
        original = json.loads((ROOT / "managed/morro-achievements.json").read_text())
        rows = original["achievements"]
        self.assertEqual(original["steamAppId"], 504230)
        self.assertEqual(original["achievementCount"], 32)
        self.assertEqual([row["id"] for row in rows], IDS)
        self.assertEqual([row["id"] for row in rows if not row["hidden"]], ["CH7", "ONEUP"])
        self.assertTrue(all(type(row["hidden"]) is bool for row in rows))

    def test_catalog_rejects_duplicates_bad_policy_and_untrusted_icon_metadata(self):
        valid = copy.deepcopy(self.catalog)
        cases = (
            lambda c: c.update(steamAppId=1),
            lambda c: c["achievements"].pop(),
            lambda c: c["achievements"][0].update(id="CH2"),
            lambda c: c["achievements"][0].update(hidden=False),
            lambda c: c["achievements"][0].update(hidden=1),
            lambda c: c["achievements"][0].update(name=""),
            lambda c: c["achievements"][0]["icon"].update(fileName="../escape.jpg"),
            lambda c: c["achievements"][0]["icon"].update(url="https://example.invalid/icon.jpg"),
            lambda c: c["achievements"][0]["icon"].update(sha256="invalid"),
        )
        for case in cases:
            with self.subTest(case=cases.index(case)):
                self.catalog = copy.deepcopy(valid)
                case(self.catalog)
                self.write_catalog()
                with self.assertRaises(ValueError):
                    prepare.read_catalog()

    def test_generation_preserves_baseline_and_reproduces_exact_output(self):
        original = prepare.manifest(self.managed)
        self.assertEqual(self.generate(), 0)
        first = self.report()
        self.assertEqual(first["input"], original)
        self.assertEqual(prepare.manifest(self.managed), original)
        self.assertEqual(len(first["icons"]), 64)
        self.assertEqual(verify.validate_source(self.output, "ios"), first)
        self.assertEqual(self.generate("--clean"), 0)
        self.assertEqual(self.report(), first)
        self.assertEqual(prepare.manifest(self.managed), original)

    def test_transform_requires_every_anchor_exactly_once(self):
        for filename, anchors in ANCHORS.items():
            for anchor in anchors:
                for replacement in ("// missing anchor", anchor + "\n" + anchor):
                    with self.subTest(filename=filename, anchor=anchor, duplicate=replacement.startswith(anchor)):
                        candidate = self.root / "candidate"
                        if candidate.exists():
                            shutil.rmtree(candidate)
                        shutil.copytree(self.managed, candidate)
                        path = candidate / filename
                        path.write_text(path.read_text().replace(anchor, replacement))
                        with self.assertRaisesRegex(ValueError, "source lock changed"):
                            prepare.transform(candidate, prepare.read_catalog())

    def test_ios_backup_transform_requires_every_anchor_exactly_once(self):
        for filename, anchors in IOS_ANCHORS.items():
            for anchor in anchors:
                for replacement in ("// missing anchor", anchor + "\n" + anchor):
                    with self.subTest(filename=filename, duplicate=replacement.startswith(anchor)):
                        candidate = self.root / "candidate"
                        if candidate.exists():
                            shutil.rmtree(candidate)
                        shutil.copytree(self.managed, candidate)
                        path = candidate / filename
                        path.write_text(path.read_text().replace(anchor, replacement))
                        with self.assertRaisesRegex(ValueError, "backup source anchor changed"):
                            prepare.transform(candidate, prepare.read_catalog(), "ios")

    def test_tvos_omits_ios_backup_integration(self):
        candidate = self.root / "candidate"
        shutil.copytree(self.managed, candidate)
        prepare.transform(candidate, prepare.read_catalog(), "tvos")
        self.assertTrue((candidate / "Celeste/FullBackupArchive.cs").is_file())
        self.assertFalse((candidate / "Celeste/MorroIOSBackups.cs").exists())
        self.assertEqual((candidate / "Celeste/IOSStorageHooks.cs").read_bytes(),
                         (self.managed / "Celeste/IOSStorageHooks.cs").read_bytes())

    def test_transform_rejects_enum_reorder_missing_or_added_value(self):
        enum = self.managed / "Celeste/Achievement.cs"
        original = enum.read_text()
        changes = (original.replace("CH1,\nCH2", "CH2,\nCH1"),
                   original.replace("CH1,\n", ""), original.replace("WOW\n", "WOW,\nEXTRA\n"))
        for value in changes:
            with self.subTest(value=value):
                enum.write_text(value)
                with self.assertRaisesRegex(ValueError, "original game enum"):
                    prepare.transform(self.managed, prepare.read_catalog())

    def test_offline_staging_requires_valid_cached_bytes(self):
        name = self.catalog["achievements"][0]["icon"]["fileName"]
        path = self.cache / name
        destination = self.root / "icons"
        for state in ("missing", "corrupt", "symlink"):
            with self.subTest(state=state):
                path.unlink(missing_ok=True)
                if state == "corrupt":
                    path.write_bytes(b"corrupt owned fixture")
                elif state == "symlink":
                    target = self.root / "owned-icon"
                    target.write_bytes(self.icon_bytes[name])
                    path.symlink_to(target)
                with mock.patch.object(prepare.urllib.request, "urlopen") as network:
                    with self.assertRaises(ValueError):
                        prepare.stage_icons(prepare.read_catalog(), self.cache, destination, True)
                    network.assert_not_called()

    def test_download_must_match_pinned_bytes_and_size_before_cache_write(self):
        name = self.catalog["achievements"][0]["icon"]["fileName"]
        cached = self.cache / name
        cached.unlink()
        for data in (b"wrong bytes", b"x" * 262145):
            with self.subTest(size=len(data)), \
                    mock.patch.object(prepare.urllib.request, "urlopen", return_value=io.BytesIO(data)):
                with self.assertRaisesRegex(ValueError, "does not match its pin"):
                    prepare.stage_icons(prepare.read_catalog(), self.cache, self.root / "icons", False)
                self.assertFalse(cached.exists())
        with mock.patch.object(prepare.urllib.request, "urlopen", return_value=io.BytesIO(self.icon_bytes[name])):
            rows = prepare.stage_icons(prepare.read_catalog(), self.cache, self.root / "icons", False)
        self.assertEqual(len(rows), 64)
        self.assertEqual(cached.read_bytes(), self.icon_bytes[name])

    def test_icon_verifier_rejects_missing_corrupt_extra_directory_and_link(self):
        self.generate()
        original = self.icon_path().read_bytes()
        directory = self.icon_path().parent
        for state in ("missing", "corrupt", "extra", "directory", "symlink"):
            with self.subTest(state=state):
                target = self.icon_path()
                if state == "missing":
                    target.unlink()
                elif state == "corrupt":
                    target.write_bytes(b"corrupt")
                elif state == "extra":
                    (directory / "extra.jpg").write_bytes(b"extra")
                elif state == "directory":
                    target.unlink()
                    target.mkdir()
                else:
                    target.unlink()
                    target.symlink_to(self.cache / self.catalog["achievements"][0]["icon"]["fileName"])
                with self.assertRaises(ValueError):
                    verify.validate_icons(self.output / "content/Content")
                if target.is_dir():
                    target.rmdir()
                else:
                    target.unlink(missing_ok=True)
                target.write_bytes(original)
                (directory / "extra.jpg").unlink(missing_ok=True)

    def test_owned_path_rejects_escape_and_symlink_ancestors(self):
        allowed = self.root / ".build/morro-achievements"
        for path in (self.root, allowed, allowed / ".." / ".." / "outside", self.root / "outside"):
            with self.subTest(path=path), self.assertRaises(ValueError):
                prepare.owned_path(path)
        outside = self.root / "outside"
        outside.mkdir()
        link = allowed / "linked"
        link.symlink_to(outside, target_is_directory=True)
        for path in (link, link / "nested", link / ".." / "escape"):
            with self.subTest(path=path), self.assertRaises(ValueError):
                prepare.owned_path(path)
        self.assertEqual(prepare.owned_path(allowed / "valid"), allowed / "valid")

    def test_cleanup_refuses_unmarked_changed_or_linked_marker(self):
        self.output.mkdir()
        sentinel = self.output / "preserve.txt"
        sentinel.write_text("Preserve owned fixture.")
        marker = self.output / prepare.MARKER
        for state in ("missing", "changed", "linked"):
            with self.subTest(state=state):
                marker.unlink(missing_ok=True)
                if state == "changed":
                    marker.write_text("not the owned marker\n")
                elif state == "linked":
                    target = self.root / "marker"
                    target.write_text("vanilla-achievements-v1\n")
                    marker.symlink_to(target)
                with self.assertRaisesRegex(ValueError, "existing/unmarked"):
                    self.generate("--clean")
                self.assertTrue(sentinel.is_file())

    def test_existing_output_requires_explicit_clean(self):
        self.generate()
        first = self.report()
        with self.assertRaisesRegex(ValueError, "existing/unmarked"):
            self.generate()
        self.assertEqual(self.report(), first)

    def test_invalid_or_everest_baseline_is_rejected_before_cleanup(self):
        self.generate()
        first = self.report()
        (self.managed / "AppleEverestExternalAssemblyRoots.props").write_text("<Project />")
        with self.assertRaisesRegex(ValueError, "unmodified canonical vanilla"):
            self.generate("--clean")
        self.assertEqual(self.report(), first)

    def test_verifier_rejects_output_tampering_even_with_rewritten_manifest(self):
        self.generate()
        path = self.output / "managed/Celeste/Settings.cs"
        path.write_text("// Unexpected owned fixture mutation.\n")
        with self.assertRaisesRegex(ValueError, "differs from its manifest"):
            verify.validate_source(self.output, "ios")
        report = self.report()
        report["output"] = prepare.manifest(self.output / "managed")
        self.write_report(report)
        with self.assertRaisesRegex(ValueError, "outside the exact achievement derivation"):
            verify.validate_source(self.output, "ios")

    def test_verifier_rejects_changed_baseline_stale_catalog_and_templates(self):
        self.generate()
        files = (self.managed / "Celeste/Settings.cs", self.catalog_path,
                 self.root / "managed/templates/MorroAchievements.cs")
        for path in files:
            with self.subTest(path=path):
                original = path.read_bytes()
                path.write_bytes(original + b"\n")
                with self.assertRaises(ValueError):
                    verify.validate_source(self.output, "ios")
                path.write_bytes(original)

    def test_verifier_rejects_wrong_platform_or_changed_icon_manifest(self):
        self.generate()
        with self.assertRaisesRegex(ValueError, "identity changed"):
            verify.validate_source(self.output, "tvos")
        report = self.report()
        report["icons"][0]["sha256"] = "0" * 64
        self.write_report(report)
        with self.assertRaisesRegex(ValueError, "icon manifest changed"):
            verify.validate_source(self.output, "ios")

    def test_verifier_rejects_base_content_corruption_missing_extra_and_link(self):
        self.generate()
        content = self.output / "content/Content"
        path = content / "owned-fixture.txt"
        original = path.read_bytes()
        for state in ("corrupt", "missing", "extra", "linked"):
            with self.subTest(state=state):
                if state == "corrupt":
                    path.write_bytes(b"changed content")
                elif state == "missing":
                    path.unlink()
                elif state == "extra":
                    (content / "unexpected.txt").write_bytes(b"extra content")
                else:
                    path.unlink()
                    path.symlink_to(self.source / "content/Content/owned-fixture.txt")
                with self.assertRaises(ValueError):
                    verify.validate_source(self.output, "ios")
                path.unlink(missing_ok=True)
                path.write_bytes(original)
                (content / "unexpected.txt").unlink(missing_ok=True)

    def test_changed_base_content_pin_rejects_before_output_cleanup(self):
        self.generate()
        first = self.report()
        (self.source / "content/Content/owned-fixture.txt").write_text("Changed baseline.\n")
        with self.assertRaisesRegex(ValueError, "exact canonical class A"):
            self.generate("--clean")
        self.assertEqual(self.report(), first)

    def test_content_manifest_rejects_duplicate_roots_and_symlinks(self):
        content = self.source / "content/Content"
        with self.assertRaises(ValueError):
            prepare.content_manifest([content, content])
        link = self.root / "linked-content"
        link.symlink_to(content, target_is_directory=True)
        with self.assertRaises(ValueError):
            prepare.content_manifest([link])

    def test_app_gate_requires_matching_host_game_storage_and_aot_markers(self):
        # Tiny structural fixtures exercise rejection, never impersonate a
        # compiled product or claim its methods execute on a device.
        self.generate()
        app = self.root / "owned-fixture.app"
        shutil.copytree(self.output / "content/Content", app / "Content")
        notice = app / "Licenses/AchievementHelper.txt"
        notice.parent.mkdir()
        notice.write_bytes((self.root / "docs/licenses/ACHIEVEMENT_HELPER_LICENSE.txt").read_bytes())
        game = app / "Celeste.dll"
        game.write_bytes(b"MorroAchievements MorroAchievementUI AchievementLedger Recover QueueUnlock "
                         b"FullBackupArchive ReplaceBackup ValidateCentralDirectory MorroIOSBackups")
        host = app / "CelesteIOSRuntimeHost.dll"
        host.write_bytes(b"AppleAchievementStorage IOSMorroBackupStorage" + "Morro.VanillaAchievements.".encode("utf-16le"))
        (app / "Celeste.aotdata.arm64").write_bytes(b"owned structural fixture")
        host_aot = app / "CelesteIOSRuntimeHost.aotdata.arm64"
        host_aot.write_bytes(b"owned structural fixture")
        verify.validate_app(app, "ios")
        original_notice = notice.read_bytes()
        for state in ("missing", "changed", "linked"):
            with self.subTest(notice=state):
                notice.unlink()
                if state == "changed":
                    notice.write_bytes(b"Incomplete notice")
                elif state == "linked":
                    notice.symlink_to(self.root / "docs/licenses/ACHIEVEMENT_HELPER_LICENSE.txt")
                with self.assertRaisesRegex(ValueError, "exact Achievement Helper license"):
                    verify.validate_app(app, "ios")
                notice.unlink(missing_ok=True)
                notice.write_bytes(original_notice)
        with self.assertRaisesRegex(ValueError, "requested platform"):
            verify.validate_app(app, "tvos")
        second = app / "CelesteTvOSRuntimeHost.dll"
        second.write_bytes(host.read_bytes())
        with self.assertRaisesRegex(ValueError, "exactly one"):
            verify.validate_app(app, "ios")
        second.unlink()
        for target in (game, host, host_aot):
            with self.subTest(target=target):
                data = target.read_bytes()
                target.unlink()
                with self.assertRaises(ValueError):
                    verify.validate_app(app, "ios")
                target.write_bytes(data)
        for target in (game, host):
            with self.subTest(target=target, change="missing feature code"):
                data = target.read_bytes()
                target.write_bytes(b"unrelated owned fixture")
                with self.assertRaises(ValueError):
                    verify.validate_app(app, "ios")
                target.write_bytes(data)
        (app / "Content/owned-fixture.txt").write_bytes(b"changed packaged content")
        with self.assertRaisesRegex(ValueError, "exact canonical class A"):
            verify.validate_app(app, "ios")


if __name__ == "__main__":
    unittest.main()

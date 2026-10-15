"""Owned tiny content fixtures: clone fast path, fallback and locked staging."""

import argparse
import ctypes
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import stat
import subprocess
import sys
import tempfile
import unittest
from unittest import mock


SCRIPT = Path(__file__).resolve().parents[1] / "scripts/celeste-stage3b.py"
spec = importlib.util.spec_from_file_location("stage3b", SCRIPT)
stage3b = importlib.util.module_from_spec(spec)
spec.loader.exec_module(stage3b)


class ContentCopyTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="morro-content-copy-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source = self.root / "source fixture"
        self.source.write_bytes(b"project-owned content\0\xff\n")

    def test_clone_success_avoids_subprocess_and_byte_copy(self):
        destination = self.root / "nested" / "Unicode-é.bin"

        def clone(source, target, flags):
            self.assertEqual(source, os.fsencode(self.source))
            self.assertEqual(target, os.fsencode(destination))
            self.assertEqual(flags, 0)
            Path(os.fsdecode(target)).write_bytes(self.source.read_bytes())
            return 0

        with mock.patch.object(stage3b, "_get_clonefile", return_value=clone), \
                mock.patch.object(stage3b.subprocess, "run") as run, \
                mock.patch.object(stage3b.shutil, "copyfile") as copy:
            stage3b.clone_or_copy(self.source, destination)
        run.assert_not_called()
        copy.assert_not_called()
        self.assertEqual(destination.read_bytes(), self.source.read_bytes())

    def test_clone_unavailable_or_failed_retains_cp_fallback(self):
        for clone in (None, mock.Mock(return_value=-1)):
            with self.subTest(clone=clone):
                destination = self.root / "fallback"

                def cp(args, **kwargs):
                    self.assertEqual(args, ["cp", "-c", str(self.source), str(destination)])
                    destination.write_bytes(self.source.read_bytes())
                    return subprocess.CompletedProcess(args, 0)

                with mock.patch.object(stage3b, "_get_clonefile", return_value=clone), \
                        mock.patch.object(stage3b.subprocess, "run", side_effect=cp) as run, \
                        mock.patch.object(stage3b.shutil, "copyfile") as copy:
                    stage3b.clone_or_copy(self.source, destination)
                self.assertEqual(run.call_count, 1)
                copy.assert_not_called()
                self.assertEqual(destination.read_bytes(), self.source.read_bytes())
                destination.unlink()

    def test_failed_clone_and_cp_use_existing_byte_copy_fallback(self):
        destination = self.root / "fallback"
        with mock.patch.object(stage3b, "_get_clonefile", return_value=mock.Mock(return_value=-1)), \
                mock.patch.object(stage3b.subprocess, "run", return_value=subprocess.CompletedProcess([], 1)):
            stage3b.clone_or_copy(self.source, destination)
        self.assertEqual(destination.read_bytes(), self.source.read_bytes())

    def test_final_copy_error_is_not_suppressed(self):
        with mock.patch.object(stage3b, "_get_clonefile", return_value=mock.Mock(return_value=-1)), \
                mock.patch.object(stage3b.subprocess, "run", return_value=subprocess.CompletedProcess([], 1)), \
                mock.patch.object(stage3b.shutil, "copyfile", side_effect=PermissionError("fixture denied")):
            with self.assertRaises(PermissionError):
                stage3b.clone_or_copy(self.source, self.root / "failure")

    def test_unavailable_native_api_is_cached_and_non_darwin_does_not_load_it(self):
        stage3b._get_clonefile.cache_clear()
        self.addCleanup(stage3b._get_clonefile.cache_clear)
        with mock.patch.object(stage3b.sys, "platform", "darwin"), \
                mock.patch.object(stage3b.ctypes, "CDLL", side_effect=OSError("unavailable")) as library:
            self.assertIsNone(stage3b._get_clonefile())
            self.assertIsNone(stage3b._get_clonefile())
            library.assert_called_once()
        stage3b._get_clonefile.cache_clear()
        with mock.patch.object(stage3b.sys, "platform", "linux"), \
                mock.patch.object(stage3b.ctypes, "CDLL") as library:
            self.assertIsNone(stage3b._get_clonefile())
            library.assert_not_called()

    @unittest.skipUnless(sys.platform == "darwin", "macOS clone/cp metadata comparison")
    def test_real_clone_preserves_cp_metadata_and_is_copy_on_write(self):
        clonefile = stage3b._get_clonefile()
        if clonefile is None:
            self.skipTest("clonefile unavailable")
        self.source.chmod(0o640)
        os.utime(self.source, (1000000000, 1000000001))
        subprocess.run(["/usr/bin/xattr", "-w", "user.morro-copy-test", "owned", str(self.source)], check=True)
        expected, actual = self.root / "cp-result", self.root / "native-result"
        subprocess.run(["/bin/cp", "-c", str(self.source), str(expected)], check=True)
        if clonefile(os.fsencode(self.source), os.fsencode(actual), 0) != 0:
            self.skipTest(f"temporary filesystem does not clone (errno {ctypes.get_errno()})")
        actual.unlink()
        with mock.patch.object(stage3b.subprocess, "run", side_effect=AssertionError("unexpected fallback")):
            stage3b.clone_or_copy(self.source, actual)
        self.assertEqual(actual.read_bytes(), expected.read_bytes())
        a, b = actual.stat(), expected.stat()
        self.assertEqual((stat.S_IMODE(a.st_mode), a.st_mtime_ns, a.st_flags),
                         (stat.S_IMODE(b.st_mode), b.st_mtime_ns, b.st_flags))
        self.assertEqual(subprocess.check_output(["/usr/bin/xattr", "-p", "user.morro-copy-test", str(actual)]), b"owned\n")
        self.assertNotEqual(a.st_ino, self.source.stat().st_ino)
        actual.write_bytes(b"changed clone")
        self.assertEqual(self.source.read_bytes(), b"project-owned content\0\xff\n")

    @unittest.skipUnless(sys.platform == "darwin", "macOS existing-destination comparison")
    def test_existing_destination_keeps_original_cp_behavior(self):
        expected, actual = self.root / "existing-cp", self.root / "existing-native"
        for path in (expected, actual):
            path.write_bytes(b"previous fixture bytes")
            path.chmod(0o600)
        subprocess.run(["/bin/cp", "-c", str(self.source), str(expected)], check=True)
        stage3b.clone_or_copy(self.source, actual)
        self.assertEqual(actual.read_bytes(), expected.read_bytes())
        self.assertEqual(stat.S_IMODE(actual.stat().st_mode), stat.S_IMODE(expected.stat().st_mode))


class ContentStagingTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="morro-content-stage-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.game = self.root / "game"
        self.contents = {
            "Effects/Border.xnb": b"XNB-owned-fixture\0\xff",
            "MixedCase/UpperCase.bin": b"owned mixed-case fixture",
            "MixedCase/lowercase.bin": b"different owned fixture",
            "FMOD/Desktop/fixture.bank": b"excluded owned fixture",
        }
        for relative, data in self.contents.items():
            path = self.game / "Content" / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
        self.staged = {k: v for k, v in self.contents.items() if not k.startswith("FMOD/")}

        def aggregate(items):
            digest = hashlib.sha256()
            for name, data in sorted(items.items()):
                digest.update(name.encode() + b"\0" + str(len(data)).encode() + b"\0" +
                              hashlib.sha256(data).hexdigest().encode() + b"\n")
            return digest.hexdigest()

        self.policy = self.root / "policy.json"
        self.policy.write_text(json.dumps({"content": {
            "excludedPrefixes": ["FMOD/"],
            "representativeXnbAssets": [{"relativePath": "Effects/Border.xnb",
                                       "sha256": hashlib.sha256(self.contents["Effects/Border.xnb"]).hexdigest()}],
            "expectedStagedFileCount": len(self.staged),
            "expectedStagedTotalBytes": sum(map(len, self.staged.values())),
            "expectedStagedAggregateSha256": aggregate(self.staged),
            "expectedExcludedFileCount": 1,
            "expectedExcludedTotalBytes": len(self.contents["FMOD/Desktop/fixture.bank"]),
        }}))
        self.input = self.root / "input.json"
        self.input.write_text(json.dumps({"validation": "supported-explicit-fna-input-profile", "content": {
            "aggregateSha256": aggregate(self.contents), "fileCount": len(self.contents),
            "totalBytes": sum(map(len, self.contents.values())),
        }}))

    def args(self, name):
        return argparse.Namespace(game_root=str(self.game), destination=str(self.root / name),
                                  policy=str(self.policy), input_manifest=str(self.input),
                                  output=str(self.root / (name + ".json")))

    def test_fast_path_and_fallback_have_identical_content_and_manifests(self):
        first, second = self.args("fast"), self.args("fallback")
        stage3b.cmd_stage_content(first)
        with mock.patch.object(stage3b, "_get_clonefile", return_value=mock.Mock(return_value=-1)), \
                mock.patch.object(stage3b.subprocess, "run", return_value=subprocess.CompletedProcess([], 1)):
            stage3b.cmd_stage_content(second)
        self.assertEqual(Path(first.output).read_bytes(), Path(second.output).read_bytes())
        for relative, content in self.staged.items():
            self.assertEqual((Path(first.destination) / relative).read_bytes(), content)
            self.assertEqual((Path(second.destination) / relative).read_bytes(), content)
        for relative, content in self.contents.items():
            self.assertEqual((self.game / "Content" / relative).read_bytes(), content)
        self.assertFalse((Path(first.destination) / "FMOD").exists())

    def test_nonempty_destination_and_source_symlink_are_still_rejected(self):
        args = self.args("nonempty")
        Path(args.destination).mkdir()
        (Path(args.destination) / "keep").write_bytes(b"retained")
        with self.assertRaisesRegex(SystemExit, "must be empty"):
            stage3b.cmd_stage_content(args)
        self.assertEqual((Path(args.destination) / "keep").read_bytes(), b"retained")
        (self.game / "Content" / "link").symlink_to(self.game / "Content" / "Effects/Border.xnb")
        with self.assertRaisesRegex(SystemExit, "symlink is forbidden"):
            stage3b.cmd_stage_content(self.args("symlink"))

    def test_representative_content_corruption_is_still_rejected(self):
        def corrupt(source, destination):
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_bytes(b"corrupt fixture")
        with mock.patch.object(stage3b, "clone_or_copy", side_effect=corrupt):
            with self.assertRaisesRegex(SystemExit, "representative XNB staging failed"):
                stage3b.cmd_stage_content(self.args("corrupt"))


if __name__ == "__main__":
    unittest.main()

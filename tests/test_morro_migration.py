"""Migration controls use owned synthetic tools; never sign, install or publish."""
import importlib.util
import copy
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest import mock

ROOT = Path(__file__).resolve().parents[1]


def module(name):
    spec = importlib.util.spec_from_file_location(name.replace("-", "_"), ROOT / "scripts" / (name + ".py"))
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


class HostPolicy(unittest.TestCase):
    def check_host(self, arch="x86_64", os_version="26.6.2", **changes):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            commands = {
                "uname": "echo " + arch,
                "sw_vers": "echo " + os_version,
                "xcodebuild": "printf 'Xcode 26.6\\nBuild version 17F113\\n'",
                "xcrun": "echo 26.5",
                "dotnet": "case \"$*\" in '--version') echo 10.0.302;; 'workload --version') echo 10.0.302.0;; 'workload list') printf 'ios 26.5.10301/10.0.100\\ntvos 26.5.10301/10.0.100\\n';; *) exit 9;; esac",
            }
            commands.update(changes)
            for name, body in commands.items():
                p = root / name
                p.write_text("#!/bin/bash\nset -eu\n" + body + "\n")
                p.chmod(0o755)
            return subprocess.run(["bash", str(ROOT / "scripts/check-ios-host.sh")],
                                  env=dict(os.environ, PATH=str(root) + ":" + os.environ["PATH"]),
                                  text=True, capture_output=True)

    def test_both_qualified_host_variants(self):
        for arch, version in (("arm64", "26.3"), ("x86_64", "26.6.2")):
            with self.subTest(arch=arch):
                self.assertEqual(self.check_host(arch, version).returncode, 0)

    def test_host_and_toolchain_drift_fails(self):
        cases = ({"arch": "i386"}, {"os_version": "26.2"}, {"os_version": "27.0"},
                 {"os_version": "26.bad"}, {"xcodebuild": "printf 'Xcode 26.6\\nBuild version WRONG\\n'"},
                 {"xcrun": "echo 26.4"}, {"dotnet": "echo 10.0.999"},
                 {"xcrun": "[[ $2 != appletvsimulator ]] && echo 26.5 || echo 26.4"})
        for case in cases:
            with self.subTest(case=case):
                self.assertNotEqual(self.check_host(**case).returncode, 0)


class ModernGraph(unittest.TestCase):
    def test_actual_shared_version_binding_and_negative_controls(self):
        verifier = module("verify-celeste-tvos-stage13b")
        project = (ROOT / "tvos/CelesteTvOSRuntimeHost/CelesteTvOSRuntimeHost.csproj").read_text()
        original = (ROOT / "modern-ios/IOSPortVersion.props").read_text()
        with tempfile.TemporaryDirectory() as tmp:
            authority = Path(tmp) / "version.props"
            authority.write_text(original)
            verifier.verify_version_authority(project, authority)
            for old, new in (("../../modern-ios/IOSPortVersion.props", "../../wrong.props"),
                             ("$(IOSPortBuildNumber)</ApplicationVersion>", "0</ApplicationVersion>"),
                             ("<ApplicationVersion>", '<ApplicationVersion Condition="false">'),
                             ("<ApplicationVersion>", "<ApplicationVersion>49</ApplicationVersion><ApplicationVersion>"),
                             ("<PropertyGroup>", '<PropertyGroup Condition="false">')):
                text = project.replace("\\", "/").replace(old, new)
                with self.subTest(old=old), self.assertRaises(SystemExit):
                    verifier.verify_version_authority(text, authority)
            for old, new in (("<IOSPortBuildNumber>49", "<IOSPortBuildNumber>0"),
                             ("<IOSPortSemanticVersion>0.1.1", "<IOSPortSemanticVersion>unknown"),
                             ("<PropertyGroup>", '<PropertyGroup Condition="false">')):
                authority.write_text(original.replace(old, new))
                with self.subTest(authority=old), self.assertRaises(SystemExit):
                    verifier.verify_version_authority(project, authority)
            authority.unlink()
            with self.assertRaises(SystemExit):
                verifier.verify_version_authority(project, authority)
            with self.assertRaises(SystemExit):
                verifier.verify_version_authority("<Project><!-- 16.0.0+stage16b --></Project>", authority)
            verifier.verify_version_authority("<Project><PropertyGroup><AssemblyInformationalVersion>16.0.0+stage16b</AssemblyInformationalVersion></PropertyGroup></Project>", authority)

    def test_real_cloud_graph_check_rejects_missing_and_contaminated_ios_project(self):
        verifier = module("verify-celeste-tvos-stage14")
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            with self.assertRaises(SystemExit):
                verifier.verify_ios_graph(root)
            project = root / "modern-ios/CelesteIOSRuntimeHost/CelesteIOSRuntimeHost.csproj"
            project.parent.mkdir(parents=True)
            project.write_text("<Project><PropertyGroup /></Project>")
            verifier.verify_ios_graph(root)
            project.write_text("<Project>CelesteTvOS.ControllerPrompts</Project>")
            with self.assertRaises(SystemExit):
                verifier.verify_ios_graph(root)


class SourceInventory(unittest.TestCase):
    def test_real_source_guard_rejects_native_host_icon_tampering_and_omission(self):
        verifier = module("verify-morro-layout")
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp).resolve()
            def git(*args):
                return subprocess.check_output(["git", "-c", "user.name=Synthetic fixture",
                    "-c", "user.email=fixture@example.invalid", "-c", "core.hooksPath=/dev/null", *args],
                    cwd=root, stderr=subprocess.DEVNULL, text=True).strip()
            git("init", "-q")
            fixtures = {"native/owned.lock.json": b'{"revision":"owned-fixture"}\n',
                        "modern-ios/OwnedFixture.cs": b"class OwnedFixture {}\n",
                        "managed/RequiredFixture.cs": b"class RequiredFixture {}\n",
                        verifier.ICON_OLD: b"owned synthetic icon bytes"}
            for name, data in fixtures.items():
                target = root / name
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
            git("add", "--all")
            git("commit", "-qm", "Owned foundation fixture")
            baseline = git("rev-parse", "HEAD")
            (root / verifier.ICON_NEW).parent.mkdir(parents=True)
            git("mv", verifier.ICON_OLD, verifier.ICON_NEW)
            with mock.patch.object(verifier, "BASELINE", baseline):
                verifier.make_inventory(root)
                for name in ("native/owned.lock.json", "modern-ios/OwnedFixture.cs", verifier.ICON_NEW):
                    target = root / name
                    original = target.read_bytes()
                    target.write_bytes(original + b"changed meaningful bytes")
                    with self.subTest(path=name), self.assertRaises(ValueError):
                        verifier.make_inventory(root)
                    target.write_bytes(original)
                git("rm", "managed/RequiredFixture.cs")
                with self.assertRaisesRegex(ValueError, "unapproved baseline omission"):
                    verifier.make_inventory(root)

    def test_inventory_rejects_omission_duplicate_and_changed_evidence(self):
        verifier = module("verify-morro-layout")
        actual = {"files": [{"path": "owned.cs", "sha256": "a" * 64, "purpose": "owned runtime"}]}
        verifier.verify(copy.deepcopy(actual), actual)
        for mutation in ("omission", "duplicate", "hash", "reason"):
            saved = copy.deepcopy(actual)
            if mutation == "omission": saved["files"] = []
            elif mutation == "duplicate": saved["files"] *= 2
            elif mutation == "hash": saved["files"][0]["sha256"] = "b" * 64
            else: saved["files"][0].pop("purpose")
            with self.subTest(mutation=mutation), self.assertRaises(ValueError):
                verifier.verify(saved, actual)
        with self.assertRaises(ValueError):
            verifier.purpose("unexplained-file.tmp")


class CloudExport(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.exporter = module("export-morro-cloud-builder")

    def render(self, repository="example/morro-celeste", revision="a" * 40):
        return self.exporter.render(ROOT / "cloud-builder-template", repository, revision)

    def test_binds_both_pin_authorities_and_documentation(self):
        outputs = self.render()
        for name in (".github/workflows/build.yml", "scripts/cloud-common.sh", "README.md"):
            text = outputs[name].decode()
            self.assertIn("example/morro-celeste", text)
            self.assertIn("a" * 40, text)
            self.assertNotIn(self.exporter.OLD_REVISION, text)
        for name in (".github/workflows/cleanup.yml", "scripts/prepare-inputs.py"):
            self.assertEqual(outputs[name], (ROOT / "cloud-builder-template" / name).read_bytes())
        workflow = outputs[".github/workflows/build.yml"].decode()
        self.assertIn("workflow_dispatch:", workflow)
        self.assertNotIn("push:", workflow)

    def test_injection_and_mutable_refs_rejected(self):
        for repository in ("https://github.com/example/morro", "a/b\nx: y", "a/$(echo)", "a/b/c", "../b"):
            with self.subTest(repository=repository), self.assertRaises(ValueError):
                self.render(repository=repository)
        for revision in ("main", "HEAD", "a" * 39, "0" * 40, "A" * 40, "a" * 40 + "\n"):
            with self.subTest(revision=revision), self.assertRaises(ValueError):
                self.render(revision=revision)

    def test_no_overwrite_symlink_extra_files_or_tampering(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp).resolve()
            target = root / "builder"
            outputs = self.render()
            self.exporter.export(target, outputs)
            self.exporter.export(target, outputs, check=True)
            with self.assertRaises(FileExistsError):
                self.exporter.export(target, outputs)
            link = root / "linked"
            link.symlink_to(target, target_is_directory=True)
            with self.assertRaises(ValueError):
                self.exporter.export(link / "child", outputs)
            (target / "extra.txt").write_text("unapproved")
            with self.assertRaises(ValueError):
                self.exporter.export(target, outputs, check=True)
            (target / "extra.txt").unlink()
            (target / "scripts/cloud-common.sh").write_text("wrong pin")
            with self.assertRaises(ValueError):
                self.exporter.export(target, outputs, check=True)


if __name__ == "__main__":
    unittest.main()

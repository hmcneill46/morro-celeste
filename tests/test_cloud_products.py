"""Owned fixtures for platform routing and all-or-nothing private publication."""
import argparse
import copy
import importlib.util
import json
import os
from pathlib import Path
import plistlib
import subprocess
import tempfile
import unittest
from unittest.mock import patch
import zipfile

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("cloud_products", ROOT / "cloud-builder-template/scripts/build-products.py")
products = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(products)


class CloudProductsTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="owned-cloud-products-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.args = argparse.Namespace(platform="both", source_root=self.root / "source",
                                      output_root=self.root / "output", game_root=self.root / "game with spaces",
                                      fmod_root=self.root / "FMOD with spaces", source_sha="a" * 40,
                                      bundle_id="example.tv", ios_bundle_id="example.phone")
        self.args.source_root.mkdir()
        self.calls = []

    def ipa(self, platform, wrong_platform=False, signed=False):
        path = self.args.source_root / ("artifacts/ios/Celeste-iOS-v0-build1-unsigned.ipa"
                                       if platform == "ios" else "dist/Celeste-tvOS-unsigned.ipa")
        path.parent.mkdir(parents=True, exist_ok=True)
        info = {"CFBundleSupportedPlatforms": ["iPhoneOS" if platform == "ios" else "AppleTVOS"],
                "CFBundleIdentifier": "example.phone" if platform == "ios" else "example.tv",
                "UIDeviceFamily": [1, 2] if platform == "ios" else [3]}
        if wrong_platform:
            info["CFBundleSupportedPlatforms"] = ["MacOSX"]
        with zipfile.ZipFile(path, "w") as archive:
            archive.writestr("Payload/Owned.app/Info.plist", plistlib.dumps(info))
            archive.writestr("Payload/Owned.app/Owned", b"owned synthetic fixture, not an app")
            if signed:
                archive.writestr("Payload/Owned.app/embedded.mobileprovision", b"owned marker")
        return path

    def runner(self, command, **kwargs):
        command = list(map(str, command))
        self.calls.append(command)
        name = Path(command[0]).name
        if name.startswith("build-"):
            platform = "ios" if name == "build-ios.sh" else "tvos"
            ipa = self.ipa(platform)
            if platform == "ios":
                (self.args.source_root / ".build/ios-self-build").mkdir(parents=True)
                manifest = {"product": "Celeste iOS", "signing": "unsigned", "rid": "ios-arm64",
                            "configuration": "Release", "fullAOT": True, "fullTrim": True,
                            "useInterpreter": False, "jit": False, "profileId": "owned-profile",
                            "ipaName": ipa.name, "ipaSha256": products.sha256(ipa), "ipaBytes": ipa.stat().st_size}
                (ipa.parent / "build-manifest.json").write_text(json.dumps(manifest))
            else:
                profile = self.args.source_root / ".build/celeste-runtime/stage18c-cloud/celeste-input.json"
                profile.parent.mkdir(parents=True)
                profile.write_text(json.dumps({"profileId": "owned-profile"}))
        elif name == "python3":
            app = Path(command[command.index("--app") + 1])
            self.assertTrue((app / "Owned").is_file(), "iOS verifier must receive the actual extracted IPA")
            self.assertIn("cloud-verify-", str(app))
        return subprocess.CompletedProcess(command, 0)

    def build(self, platform="both", runner=None):
        self.args.platform = platform
        with patch.object(products, "run", side_effect=runner or self.runner):
            products.build(self.args)

    def test_each_selection_routes_to_unsigned_builders_and_real_verifiers(self):
        for selection, expected in (("ios", ["ios"]), ("tvos", ["tvos"]), ("both", ["tvos", "ios"])):
            with self.subTest(selection=selection):
                # Each selection has a fresh source and output, as on the hosted runner.
                self.args.source_root = self.root / selection
                self.args.source_root.mkdir()
                self.args.output_root = self.root / (selection + "-output")
                self.calls = []
                self.build(selection)
                builders = [c for c in self.calls if Path(c[0]).name.startswith("build-")]
                self.assertEqual([Path(c[0]).name for c in builders], [f"build-{p}.sh" for p in expected])
                for call, platform in zip(builders, expected):
                    self.assertIn(str(self.args.game_root), call)
                    self.assertIn(str(self.args.fmod_root), call)
                    self.assertIn("--unsigned" if platform == "ios" else "ipa", call)
                    self.assertNotIn("--install", call)
                verifiers = [Path(c[1] if c[0] == "python3" else c[0]).name for c in self.calls
                             if "verify" in " ".join(c)]
                self.assertEqual(len(verifiers), (3 if "tvos" in expected else 0) + (1 if "ios" in expected else 0))
                self.assertEqual(len(products.verified_products(self.args.output_root, selection)), 2 * len(expected))
                if selection == "both":
                    self.assertTrue(self.calls[3][0].endswith("verify-celeste-tvos-stage16b.sh"))
                    self.assertTrue(self.calls[4][0].endswith("build-ios.sh"))

    def test_second_build_or_verifier_failure_never_completes_the_publish_manifest(self):
        for failure in ("build-ios.sh", "verify-ios-package.py"):
            with self.subTest(failure=failure):
                self.args.source_root = self.root / failure
                self.args.source_root.mkdir()
                self.args.output_root = self.root / (failure + "-output")
                def fail(command, **kwargs):
                    if any(Path(str(arg)).name == failure for arg in command):
                        raise subprocess.CalledProcessError(1, command)
                    return self.runner(command, **kwargs)
                with self.assertRaises(subprocess.CalledProcessError):
                    self.build(runner=fail)
                self.assertFalse((self.args.output_root / "products.json").exists())
                with self.assertRaises(FileNotFoundError):
                    products.verified_products(self.args.output_root, "both")

    def test_invalid_choice_or_identity_never_starts_a_builder(self):
        for choice, identity in (("android", "example.phone"), ("ios", "$(touch injected)"),
                                 ("ios", "example.phone\nmalicious")):
            self.args.ios_bundle_id = identity
            with self.assertRaises(ValueError):
                self.build(choice)
        self.assertEqual(self.calls, [])
        self.assertFalse(self.args.output_root.exists())

    def test_wrong_platform_signed_or_wrong_identity_ipa_rejected(self):
        for wrong, signed, identity in ((True, False, "example.phone"), (False, True, "example.phone"),
                                        (False, False, "example.other")):
            with self.assertRaises(ValueError):
                products.inspect_ipa(self.ipa("ios", wrong, signed), "ios", identity)

    def test_zip_traversal_and_links_rejected_before_extraction(self):
        for name, mode in (("../escape", 0o100644), ("Payload/Owned.app/link", 0o120777)):
            ipa = self.ipa("ios")
            with zipfile.ZipFile(ipa, "a") as archive:
                entry = zipfile.ZipInfo(name)
                entry.create_system = 3
                entry.external_attr = mode << 16
                archive.writestr(entry, "owned fixture")
            with self.assertRaises(ValueError):
                products.inspect_ipa(ipa, "ios", "example.phone")

    def test_mislabeled_or_modified_staged_output_rejected(self):
        self.build("ios")
        with self.assertRaises(ValueError):
            products.verified_products(self.args.output_root, "both")
        with (self.args.output_root / products.names("ios")[0]).open("ab") as stream:
            stream.write(b"changed")
        with self.assertRaises(ValueError):
            products.verified_products(self.args.output_root, "ios")

    def test_github_missing_extra_duplicate_wrong_size_or_digest_rejected(self):
        self.build()
        records = products.verified_products(self.args.output_root, "both")
        release = {"isDraft": True, "isPrerelease": False,
                   "assets": [{"name": r["name"], "size": r["size"], "digest": "sha256:" + r["sha256"]}
                              for r in records]}
        products.verify_release(release, records, True)
        invalid = []
        value = copy.deepcopy(release); value["assets"].pop(); invalid.append(value)
        value = copy.deepcopy(release); value["assets"].append(value["assets"][0]); invalid.append(value)
        value = copy.deepcopy(release); value["assets"][0]["name"] = "extra.ipa"; invalid.append(value)
        value = copy.deepcopy(release); value["assets"][0]["size"] += 1; invalid.append(value)
        value = copy.deepcopy(release); value["assets"][0]["digest"] = "sha256:" + "0" * 64; invalid.append(value)
        value = copy.deepcopy(release); value["isDraft"] = False; invalid.append(value)
        for value in invalid:
            with self.assertRaises(ValueError):
                products.verify_release(value, records, True)

    def test_publish_checks_private_and_no_overwrite_before_upload(self):
        self.build("ios")
        for rejected in ("require-private", "ensure-output-absent"):
            calls = []
            def fail(command, **kwargs):
                calls.append(list(map(str, command)))
                if command[-1] == rejected:
                    raise subprocess.CalledProcessError(1, command)
            with patch.dict(os.environ, GITHUB_REPOSITORY="owner/private-fixture"), \
                    patch.object(products, "run", side_effect=fail), self.assertRaises(subprocess.CalledProcessError):
                products.publish(self.args)
            self.assertFalse(any(c[0] == "gh" for c in calls))

    def test_complete_upload_is_verified_as_draft_before_publication(self):
        self.build()
        records = products.verified_products(self.args.output_root, "both")
        calls = []
        draft = True
        def hosted(command, **kwargs):
            nonlocal draft
            command = list(map(str, command))
            calls.append(command)
            if command[:3] == ["gh", "release", "edit"]:
                draft = False
            release = {"isDraft": draft, "isPrerelease": False,
                       "assets": [{"name": r["name"], "size": r["size"], "digest": "sha256:" + r["sha256"]}
                                  for r in records]}
            return subprocess.CompletedProcess(command, 0, stdout=json.dumps(release))
        summary = self.root / "summary.md"
        with patch.dict(os.environ, GITHUB_REPOSITORY="owner/private-fixture", GITHUB_SHA="b" * 40,
                        GITHUB_STEP_SUMMARY=str(summary)), patch.object(products, "run", side_effect=hosted):
            products.publish(self.args)
        self.assertEqual([c[2] for c in calls if c[0] == "gh"], ["create", "view", "edit", "view"])
        create = next(c for c in calls if c[0] == "gh" and c[2] == "create")
        self.assertIn("--draft", create)
        self.assertEqual(create[-4:], [str(self.args.output_root.resolve() / r["name"]) for r in records])
        for platform in ("tvos", "ios"):
            self.assertIn(products.names(platform)[0], summary.read_text())

    def test_bad_build_manifest_is_rejected_before_the_package_verifier(self):
        def bad_manifest(command, **kwargs):
            result = self.runner(command, **kwargs)
            if Path(str(command[0])).name == "build-ios.sh":
                path = self.args.source_root / "artifacts/ios/build-manifest.json"
                manifest = json.loads(path.read_text())
                manifest["useInterpreter"] = True
                path.write_text(json.dumps(manifest))
            return result
        with self.assertRaises(ValueError):
            self.build("ios", bad_manifest)
        self.assertEqual(len(self.calls), 1)
        self.assertFalse((self.args.output_root / "products.json").exists())


if __name__ == "__main__":
    unittest.main()

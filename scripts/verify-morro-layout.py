#!/usr/bin/env python3
"""Account for every Morro source file and every removal from build 49."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
BASELINE = "34c0b933a4cf2252780ec84e5630847809793eab"
# Exact reviewed cloud-orchestration changes; original game/native authorities
# and historical stage verifiers retain their existing scope.
POST_MIGRATION_CHANGES = {
    'cloud-builder-template/.github/workflows/build.yml': 'ff95752399d0d4a8c8f5c9a455ef7e8550f7133e57452b22ed8133eaf7aa0e51',
    'cloud-builder-template/README.md': 'a1f5a89c6ff47d1436cf5ed47559335f97a7a74022b08c4e78c30d65215e564c',
    'cloud-builder-template/scripts/cloud-common.sh': '7965c6a731944bda0218f4099730d9d72179091aa646595ef87dee45fd2c469d',
    'scripts/export-cloud-builder-template.sh': '8dc33a74566c5518cee11570c0a001821429183d6d82e237f8a76921b9be862a',
}
INVENTORY = "docs/MORRO_FILE_INVENTORY.json"
ICON_OLD = "celestemeow/Assets.xcassets/AppIcon.appiconset/Icon1024.png"
ICON_NEW = "modern-ios/Assets/AppIcon/Icon1024.png"
REMOVED_FILES = {"build.sh", "celestemeow.sln", "fnalibs-ios-builder-celeste",
                 "misc/Celeste.dll.config", "patches/FNA-FNA3D-renderer.patch",
                 "patches/FNA.patch", "patches/SDL-gamecontroller.patch",
                 "patches/remove-steam.patch", "patches/remove-steam2.patch"}
CHANGED = {".gitignore", ".gitmodules", "README.md", "build-ios.sh", "build-tvos.sh",
           "docs/BUILDING.md", "docs/IOS_BUILDING.md", "docs/README.md", "docs/CLOUD_BUILDING.md",
           "scripts/build-ios-celeste.sh", "scripts/check-ios-host.sh",
           "scripts/prepare-ios-foundation.sh", "scripts/verify-celeste-ios-stage24e2.py",
           "scripts/verify-celeste-tvos-stage14.py", "scripts/diagnose-tvos-host.sh",
           "scripts/test-apple-everest-desktop-hookgen.sh", "scripts/verify-fmod-tvos.sh",
           "scripts/verify-celeste-tvos-stage6.sh", "scripts/verify-celeste-tvos-stage14.sh",
           "scripts/verify-repository-stage8b.py", "scripts/verify-celeste-tvos-stage13b.py",
           "scripts/verify-celeste-tvos-stage22b.py"}
PURPOSES = {
    "apple-everest": "Static Everest production authorities, runtime, finite compatibility, fixtures and regression proofs",
    "managed": "Canonical source generation, locked game profiles and platform transforms",
    "modern-ios": "Modern vanilla/static Everest iOS hosts, resources and platform contracts",
    "tvos": "Modern tvOS hosts, storage/controllers, regression tests and release authorities",
    "shared": "Platform-neutral product version and shared Apple runtime contracts",
    "native": "Pinned native sources, producer patches, platform stubs and native acceptance locks",
    "patches": "Exact crash-fix input required by the canonical generation lock",
    "tools": "Host-only static-AOT generation, closure/type analysis and compiled regression tools",
    "tests": "Owned portable negative and regression controls for current tooling",
    "scripts": "Preparation, build, verification, reproduction, deployment or historical regression entry point",
    "cloud-builder-template": "Canonical private vanilla iOS/tvOS Actions builder, input/privacy controls and cleanup",
    "docs": "Modern project guidance, architecture, acceptance history or immutable historical evidence",
    ".github": "Source contribution and issue-reporting templates; no root push workflow",
    ".config": "Pinned host-only decompiler tool manifest",
}
SINGLE_PURPOSES = {
    "FNA": "Pinned recursive FNA runtime sources used by modern hosts and static-AOT generation",
    "LICENSE": "Unchanged original MIT notice and attribution",
    "CREDITS.md": "Explicit game, original port, runtime, native and mod attribution",
    "AGENTS.md": "Project editing, privacy and preservation instructions",
    "README.md": "Morro overview, supported build routes, attribution and Cabrillo link",
    "CONTRIBUTING.md": "Contribution, source-boundary and verification guidance",
    "global.json": "Pinned .NET SDK and workload authority",
    ".gitattributes": "Repository text normalization policy",
    ".gitignore": "Generated output, private inputs and local tool exclusion policy",
    ".gitmodules": "Exact recursive FNA source dependency locations",
    "build-ios.sh": "Public modern vanilla iOS build entry point",
    "build-tvos.sh": "Public modern vanilla tvOS build entry point",
    "Launch macOS Everest Reference.command": "Pinned desktop reference launcher for static compatibility behavior comparisons",
}


def purpose(path: str) -> str:
    if path in SINGLE_PURPOSES:
        return SINGLE_PURPOSES[path]
    prefix = path.split("/", 1)[0]
    if prefix not in PURPOSES:
        raise ValueError("unclassified tracked file: " + path)
    return PURPOSES[prefix] + ": " + Path(path).name


def make_inventory(root: Path) -> dict:
    def git(*args):
        return subprocess.check_output(["git", *args], cwd=root)
    git("merge-base", "--is-ancestor", BASELINE, "HEAD")
    baseline = {}
    for entry in git("ls-tree", "-rz", BASELINE).split(b"\0"):
        if not entry:
            continue
        info, path = entry.decode().split("\t", 1)
        mode, kind, oid = info.split()
        baseline[path] = {"gitMode": mode, "gitObject": oid}
    tracked = {}
    for entry in git("ls-files", "--stage", "-z").split(b"\0"):
        if not entry:
            continue
        info, path = entry.decode().split("\t", 1)
        mode, oid, stage = info.split()
        if stage != "0" or path in tracked:
            raise ValueError("unmerged or duplicate index entry")
        tracked[path] = mode
    retained = []
    for path, mode in sorted(tracked.items()):
        target = root / path
        if target.is_symlink():
            raise ValueError("unexpected tracked symlink: " + path)
        origin = ICON_OLD if path == ICON_NEW else path
        original = baseline.get(origin)
        item = {"path": path, "purpose": purpose(path), "gitMode": mode,
                "disposition": "relocated" if path == ICON_NEW else "retained" if original else "migration-added"}
        if mode == "160000":
            digest = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=target, text=True).strip()
            if not original or digest != original["gitObject"]:
                raise ValueError("submodule revision changed")
            item["gitObject"] = digest
        elif path != INVENTORY:
            content = target.read_bytes()
            item["sha256"] = hashlib.sha256(content).hexdigest()
            if path in POST_MIGRATION_CHANGES and item["sha256"] != POST_MIGRATION_CHANGES[path]:
                raise ValueError("unreviewed post-migration source change: " + path)
            if original:
                old = git("cat-file", "blob", original["gitObject"])
                if content != old:
                    if path in POST_MIGRATION_CHANGES:
                        item["disposition"] = "post-migration-adjusted"
                    elif path not in CHANGED:
                        raise ValueError("unapproved production/source change: " + path)
                    else:
                        item["disposition"] = "migration-adjusted"
        if original:
            item["baselinePath"] = origin
            item["baselineGitObject"] = original["gitObject"]
        retained.append(item)
    accounted = {item["baselinePath"] for item in retained if "baselinePath" in item}
    removed = []
    for path in sorted(set(baseline) - accounted):
        if path not in REMOVED_FILES and not path.startswith("celestemeow/"):
            raise ValueError("unapproved baseline omission: " + path)
        removed.append({"path": path, **baseline[path],
                        "reason": "Unused original Xamarin lane; history and attribution retained. Modern dependency pins remain unchanged."})
    return {"schemaVersion": 1, "baselineCommit": BASELINE,
            "baselineAncestorCount": int(git("rev-list", "--count", BASELINE)),
            "selfHashPolicy": "This inventory lists itself without a content hash to avoid a circular digest.",
            "files": retained, "removedFromWorkingTree": removed}


def verify(saved: dict, actual: dict) -> None:
    if saved != actual:
        raise ValueError("file inventory mismatch: omissions, duplicate rows, modified bytes or changed purposes are not accepted")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--write", action="store_true", help="regenerate after staging intended source paths")
    args = parser.parse_args()
    actual = make_inventory(ROOT)
    if args.write:
        (ROOT / INVENTORY).write_text(json.dumps(actual, indent=2, sort_keys=True) + "\n")
    else:
        verify(json.loads((ROOT / INVENTORY).read_text()), actual)
    print("PASS: %d accounted source entries, %d reasoned removals; baseline ancestry and production authorities preserved" %
          (len(actual["files"]), len(actual["removedFromWorkingTree"])))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

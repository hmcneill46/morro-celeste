#!/usr/bin/env python3
"""Verify the separate vanilla achievement derivation and optional app bundle."""
from __future__ import annotations

import argparse
import importlib.util
import json
import pathlib
import shutil
import tempfile

ROOT = pathlib.Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("morro_achievement_prepare", ROOT / "scripts/prepare-morro-achievements.py")
prepare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prepare)


def require(value: bool, message: str) -> None:
    if not value:
        raise ValueError(message)


def validate_icons(content: pathlib.Path) -> set[str]:
    """Return only verified feature paths; callers still verify all base content."""
    relative_root = pathlib.Path("Graphics/MorroAchievements")
    root = content / relative_root
    require(root.is_dir() and not root.is_symlink(), "achievement artwork directory missing or linked")
    expected = {}
    for row in prepare.read_catalog():
        for kind, suffix in (("icon", ""), ("lockedIcon", "-locked")):
            expected[row["id"] + suffix + ".jpg"] = row[kind]["sha256"]
    require({p.name for p in root.iterdir()} == set(expected), "achievement artwork inventory differs from the pinned 64 files")
    for name, digest in expected.items():
        path = root / name
        require(path.is_file() and not path.is_symlink() and prepare.sha(path) == digest,
                "achievement artwork digest differs: " + name)
    return {(relative_root / name).as_posix() for name in expected}


def validate_app(app: pathlib.Path, platform: str | None = None) -> None:
    notice = app / "Licenses/AchievementHelper.txt"
    require(notice.is_file() and not notice.is_symlink() and
            notice.read_bytes() == (ROOT / "docs/licenses/ACHIEVEMENT_HELPER_LICENSE.txt").read_bytes(),
            "the exact Achievement Helper license notice is missing or changed")
    paths = validate_icons(app / "Content")
    prepare.require_canonical_content(prepare.content_manifest([app / "Content"], paths))
    game = app / "Celeste.dll"
    require(game.is_file(), "achievement game assembly missing")
    game_bytes = game.read_bytes()
    for token in ("MorroAchievements", "MorroAchievementUI", "AchievementLedger", "Recover", "QueueUnlock",
                  "FullBackupArchive", "ReplaceBackup", "ValidateCentralDirectory"):
        require(token.encode() in game_bytes, "achievement game closure missing: " + token)
    hosts = [app / "CelesteIOSRuntimeHost.dll", app / "CelesteTvOSRuntimeHost.dll"]
    hosts = [p for p in hosts if p.is_file()]
    require(len(hosts) == 1, "exactly one Apple game host is required")
    if platform:
        require(hosts[0].name == ("CelesteIOSRuntimeHost.dll" if platform == "ios" else "CelesteTvOSRuntimeHost.dll"),
                "achievement host differs from the requested platform")
    host = hosts[0].read_bytes()
    require(b"AppleAchievementStorage" in host and "Morro.VanillaAchievements.".encode("utf-16le") in host,
            "independent Apple achievement storage is missing")
    ios = hosts[0].name == "CelesteIOSRuntimeHost.dll"
    require((b"IOSMorroBackupStorage" if ios else b"RestoreMorroBackup") in host,
            "full-session backup host integration is missing")
    if ios:
        require(b"MorroIOSBackups" in game_bytes, "in-game iOS backup integration is missing")
    require((app / "Celeste.aotdata.arm64").is_file() and
            (app / (hosts[0].stem + ".aotdata.arm64")).is_file(), "achievement assemblies lack full-AOT data")


def validate_source(root: pathlib.Path, platform: str) -> dict:
    require((root / prepare.MARKER).read_text() == "vanilla-achievements-v1\n", "derived source marker changed")
    report = json.loads((root / "achievement-manifest.json").read_text())
    require(report["schemaVersion"] == 1 and report["feature"] == "morro-vanilla-achievements-v1" and
            report["platform"] == platform, "achievement derivation identity changed")
    require(report["catalogSha256"] == prepare.sha(prepare.CATALOG), "achievement catalog is stale")
    require(report["trackedSources"] == {p: prepare.sha(ROOT / p) for p in prepare.tracked_sources(platform)},
            "achievement runtime templates are stale")
    baseline = ROOT / (".build/celeste-ios/current" if platform == "ios" else ".build/celeste-runtime/stage6-current/audio")
    original = prepare.manifest(baseline / "managed")
    require((original["fileCount"], original["logicalSha256"]) == prepare.INPUTS[platform],
            "canonical vanilla baseline changed")
    require(original == report["input"], "derivation input provenance changed")
    actual = prepare.manifest(root / "managed")
    require(actual == report["output"], "derived managed source differs from its manifest")
    # Recreate the small source-only derivation to check every byte, including
    # all unchanged original call sites. Never regenerate the historical tree.
    with tempfile.TemporaryDirectory(prefix="verify-", dir=ROOT / ".build/morro-achievements") as temporary:
        expected = pathlib.Path(temporary) / "managed"
        shutil.copytree(baseline / "managed", expected, ignore=shutil.ignore_patterns("bin", "obj"))
        prepare.transform(expected, prepare.read_catalog(), platform)
        require(prepare.manifest(expected) == actual, "unexpected change outside the exact achievement derivation")
    paths = validate_icons(root / "content/Content")
    resources = prepare.content_manifest([baseline / "content/Content", baseline / "banks/Content"])
    prepare.require_canonical_content(resources, platform)
    require(report["baseContent"] == resources, "baseline content provenance changed")
    require(prepare.content_manifest([root / "content/Content", root / "banks/Content"], paths) == resources,
            "derived content differs from unchanged canonical game content")
    expected_icons = [{"path": p, "sha256": prepare.sha(root / "content/Content" / p)} for p in sorted(paths)]
    require(sorted(report["icons"], key=lambda r: r["path"]) == expected_icons, "icon manifest changed")
    require(report["achievementCount"] == 32 and report["hiddenCount"] == 30 and
            report["vanillaSaveFormatsChanged"] is False and report["runtimeNetworkAccess"] is False,
            "achievement feature contract changed")
    return report


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=pathlib.Path, required=True)
    parser.add_argument("--platform", choices=prepare.INPUTS, required=True)
    parser.add_argument("--app", type=pathlib.Path)
    args = parser.parse_args()
    validate_source(prepare.owned_path(args.root), args.platform)
    if args.app:
        validate_app(args.app.resolve(), args.platform)
    print("PASS: vanilla " + args.platform + " achievement source/artwork" + (" and AOT package closure" if args.app else "") + "; physical acceptance NOT_RUN")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

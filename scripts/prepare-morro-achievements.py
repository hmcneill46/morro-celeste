#!/usr/bin/env python3
"""Derive a vanilla-only achievement product without editing canonical inputs."""
from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
import pathlib
import re
import shutil
import subprocess
import urllib.request

ROOT = pathlib.Path(__file__).resolve().parents[1]
CATALOG = ROOT / "managed/morro-achievements.json"
MARKER = ".morro-vanilla-achievements"
# The unchanged final historical generators remain the input authorities.
INPUTS = {
    "ios": (944, "2f5d6fffaa151530f48deac7ff402c2c6d475b20eeef5579f1db784d57da2357"),
    "tvos": (934, "1a981bc5994261775c1fea497cd042780fdbbcb24b7390a2f4db6aab1be747d9"),
}
SOURCES = {
    "managed/templates/MorroAchievements.cs": "Celeste/MorroAchievements.cs",
    "managed/templates/MorroAchievementUI.cs": "Celeste/MorroAchievementUI.cs",
    "shared/MorroAchievements/AchievementLedger.cs": "Celeste/AchievementLedger.cs",
}
CONTENT_PIN = (1216, 1158665183, "30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46")
# tvOS keeps its seven FMOD banks in the existing separate accepted FMOD stage.
TVOS_SOURCE_CONTENT_PIN = (1209, 492943607, "0c2802322adb06f2281ca7f8358d56c75e6a5c48ba759171f4dc51a5c049d348")


def sha(path: pathlib.Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def content_manifest(roots: list[pathlib.Path], exclude: set[str] | None = None) -> dict:
    files = {}
    for root in roots:
        if not root.exists():
            continue
        if root.is_symlink():
            raise ValueError("content root must not be linked")
        for path in root.rglob("*"):
            if path.is_symlink():
                raise ValueError("content must not contain symlinks")
            if not path.is_file():
                continue
            relative = path.relative_to(root).as_posix()
            if exclude and relative in exclude:
                continue
            if relative in files:
                raise ValueError("duplicate packaged content path")
            files[relative] = {"path": relative, "size": path.stat().st_size, "sha256": sha(path)}
    digest = hashlib.sha256()
    rows = [files[p] for p in sorted(files)]
    for row in rows:
        digest.update(row["path"].encode() + b"\0" + str(row["size"]).encode() + b"\0" + row["sha256"].encode() + b"\n")
    return {"fileCount": len(rows), "totalBytes": sum(r["size"] for r in rows),
            "logicalSha256": digest.hexdigest(), "files": rows}


def require_canonical_content(value: dict, platform: str | None = None) -> None:
    expected = TVOS_SOURCE_CONTENT_PIN if platform == "tvos" else CONTENT_PIN
    if (value["fileCount"], value["totalBytes"], value["logicalSha256"]) != expected:
        raise ValueError("base content differs from exact canonical class A")


def manifest(root: pathlib.Path) -> dict:
    rows, digest = [], hashlib.sha256()
    for path in sorted(root.rglob("*"), key=lambda p: p.relative_to(root).as_posix()):
        relative = path.relative_to(root)
        if {"bin", "obj"}.intersection(relative.parts):
            continue
        if path.is_symlink():
            raise ValueError("symlinks are not accepted in generated input")
        if not path.is_file():
            continue
        value = sha(path)
        rows.append({"path": relative.as_posix(), "size": path.stat().st_size, "sha256": value})
        digest.update(relative.as_posix().encode() + b"\0" + value.encode() + b"\n")
    return {"fileCount": len(rows), "logicalSha256": digest.hexdigest(), "files": rows}


def read_catalog() -> list[dict]:
    value = json.loads(CATALOG.read_text())
    rows = value["achievements"]
    if value["steamAppId"] != 504230 or len(rows) != 32 or len({r["id"] for r in rows}) != 32:
        raise ValueError("unexpected vanilla achievement catalog")
    if {r["id"] for r in rows if not r["hidden"]} != {"CH7", "ONEUP"}:
        raise ValueError("original Steam secret policy changed")
    for row in rows:
        if not re.fullmatch(r"[A-Z][A-Z0-9]{1,15}", row["id"]):
            raise ValueError("invalid achievement identifier")
        if type(row["hidden"]) is not bool or not row["name"] or not row["description"]:
            raise ValueError("invalid achievement display metadata")
        for kind in ("icon", "lockedIcon"):
            icon = row[kind]
            if not re.fullmatch(r"[0-9a-f]{40}\.jpg", icon["fileName"]):
                raise ValueError("invalid icon filename")
            if icon["url"] != "https://cdn.akamai.steamstatic.com/steamcommunity/public/images/apps/504230/" + icon["fileName"]:
                raise ValueError("icon URL is outside the fixed Steam asset origin")
            if not re.fullmatch(r"[0-9a-f]{64}", icon["sha256"]):
                raise ValueError("invalid icon digest")
    return rows


def replace(path: pathlib.Path, old: str, new: str) -> None:
    text = path.read_text()
    if text.count(old) != 1:
        raise ValueError("achievement integration source lock changed: " + path.name)
    path.write_text(text.replace(old, new))


def transform(managed: pathlib.Path, catalog: list[dict]) -> None:
    enum = (managed / "Celeste/Achievement.cs").read_text()
    ids = re.findall(r"^\s*([A-Z][A-Z0-9]+),?\s*$", enum, re.MULTILINE)
    if ids != [row["id"] for row in catalog]:
        raise ValueError("achievement catalog differs from the original game enum")
    achievement = managed / "Celeste/Achievements.cs"
    replace(achievement, "\t\treturn false;", "\t\treturn MorroAchievements.Has(ID(achievement));")
    replace(achievement, "\t\tHas(achievement);", "\t\tMorroAchievements.Register(ID(achievement));")
    game = managed / "Celeste/Celeste.cs"
    replace(game, "\t\tbase.Update(gameTime);", "\t\tbase.Update(gameTime);\n\t\tMorroAchievements.Update();")
    replace(game, "\t\tbase.RenderCore();", "\t\tbase.RenderCore();\n\t\tMorroAchievementUI.Render();")
    menu = managed / "Celeste/MenuOptions.cs"
    replace(menu, "\t\tviewport.Visible = Settings.Instance.Fullscreen;",
            '\t\tmenu.Add(new TextMenu.SubHeader("MORRO"));\n'
            '\t\tmenu.Add(new TextMenu.Button("Achievements").Pressed(OpenMorroAchievements));\n'
            "\t\tviewport.Visible = Settings.Instance.Fullscreen;")
    replace(menu, "\tprivate static void OpenViewportAdjustment()\n\t{", '''\tprivate static void OpenMorroAchievements()
\t{
\t\tmenu.Visible = false;
\t\tmenu.Focused = false;
\t\tif (Engine.Scene is Overworld overworld) overworld.ShowConfirmUI = false;
\t\tMorroAchievementUI.Open(delegate
\t\t{
\t\t\tmenu.Visible = true;
\t\t\tmenu.Focused = true;
\t\t\tif (Engine.Scene is Overworld current) current.ShowConfirmUI = true;
\t\t});
\t}

\tprivate static void OpenViewportAdjustment()
\t{''')
    replace(managed / "Celeste/SaveData.cs", "\t\tInstance.AfterInitialize();",
            "\t\tInstance.AfterInitialize();\n\t\tMorroAchievements.Recover(Instance);")
    replace(managed / "Celeste/OuiFileSelect.cs", "\t\t\t\t\t\tsaveData.AfterInitialize();",
            "\t\t\t\t\t\tsaveData.AfterInitialize();\n\t\t\t\t\t\tMorroAchievements.Recover(saveData);")
    for source, target in SOURCES.items():
        shutil.copyfile(ROOT / source, managed / target)
    lines = ["namespace Celeste;", "internal static class MorroAchievementCatalog", "{",
             "    internal static readonly MorroAchievementDefinition[] Entries =", "    {"]
    for row in catalog:
        values = [json.dumps(row[key], ensure_ascii=True) for key in ("id", "name", "description")]
        values += [str(row["hidden"]).lower(), json.dumps("Graphics/MorroAchievements/" + row["id"] + ".jpg")]
        lines.append("        new MorroAchievementDefinition(" + ", ".join(values) + "),")
    lines += ["    };", "}", ""]
    (managed / "Celeste/MorroAchievementCatalog.cs").write_text("\n".join(lines))


def stage_icons(catalog: list[dict], cache: pathlib.Path, destination: pathlib.Path, offline: bool) -> list[dict]:
    cache.mkdir(parents=True, exist_ok=True)
    destination.mkdir(parents=True, exist_ok=True)
    rows = []
    for item in catalog:
        for kind, suffix in (("icon", ""), ("lockedIcon", "-locked")):
            icon = item[kind]
            cached = cache / icon["fileName"]
            if cached.is_symlink():
                raise ValueError("icon cache entry must not be a symlink")
            if not cached.exists():
                if offline:
                    raise ValueError("pinned achievement icon missing from offline cache: " + item["id"])
                with urllib.request.urlopen(icon["url"], timeout=30) as response:
                    data = response.read(262145)
                if len(data) > 262144 or hashlib.sha256(data).hexdigest() != icon["sha256"]:
                    raise ValueError("downloaded achievement artwork does not match its pin")
                cached.write_bytes(data)
            if sha(cached) != icon["sha256"]:
                raise ValueError("achievement icon cache digest mismatch: " + item["id"])
            target = destination / (item["id"] + suffix + ".jpg")
            shutil.copyfile(cached, target)
            rows.append({"path": "Graphics/MorroAchievements/" + target.name, "sha256": sha(target)})
    return rows


def owned_path(path: pathlib.Path) -> pathlib.Path:
    path = path.absolute()
    allowed = ROOT / ".build/morro-achievements"
    if any(p.is_symlink() for p in (path, *path.parents)):
        raise ValueError("achievement output/cache must not use symlinks")
    path = path.resolve()
    if allowed not in path.parents:
        raise ValueError("achievement output/cache must be below the owned ignored root, without symlinks")
    subprocess.run(["git", "check-ignore", "--no-index", "-q", str(path / MARKER)], cwd=ROOT, check=True)
    return path


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--platform", choices=INPUTS, required=True)
    parser.add_argument("--source-root", type=pathlib.Path, required=True)
    parser.add_argument("--output-root", type=pathlib.Path, required=True)
    parser.add_argument("--icon-cache", type=pathlib.Path, default=ROOT / ".build/morro-achievements/reference-icons")
    parser.add_argument("--offline", action="store_true", help="require all pinned icons already cached")
    parser.add_argument("--clean", action="store_true", help="replace only a marked derived output")
    args = parser.parse_args()
    source = args.source_root.resolve()
    output, cache = owned_path(args.output_root), owned_path(args.icon_cache)
    if output == cache or output in cache.parents or cache in output.parents:
        raise ValueError("derived output and icon cache must not overlap")
    if source == output or source in output.parents or output in source.parents:
        raise ValueError("canonical input and derived output must not overlap")
    before = manifest(source / "managed")
    if (before["fileCount"], before["logicalSha256"]) != INPUTS[args.platform]:
        raise ValueError("input must be the exact unmodified canonical vanilla tree")
    catalog = read_catalog()
    resources = content_manifest([source / "content/Content", source / "banks/Content"])
    require_canonical_content(resources, args.platform)
    if output.exists():
        if not args.clean or (output / MARKER).is_symlink() or not (output / MARKER).is_file() or (output / MARKER).read_text() != "vanilla-achievements-v1\n":
            raise ValueError("refusing existing/unmarked achievement output")
        shutil.rmtree(output)
    output.mkdir(parents=True)
    (output / MARKER).write_text("vanilla-achievements-v1\n")
    spec = importlib.util.spec_from_file_location("morro_content_copy", ROOT / "scripts/celeste-stage3b.py")
    copier = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(copier)
    for name in ("managed", "content", "banks"):
        if (source / name).is_dir():
            manifest(source / name) # Reject links before copy; never follow a private input escape.
            shutil.copytree(source / name, output / name, ignore=shutil.ignore_patterns("bin", "obj"),
                            copy_function=lambda a, b: copier.clone_or_copy(pathlib.Path(a), pathlib.Path(b)))
    transform(output / "managed", catalog)
    icons = stage_icons(catalog, cache, output / "content/Content/Graphics/MorroAchievements", args.offline)
    report = {"schemaVersion": 1, "feature": "morro-vanilla-achievements-v1", "platform": args.platform,
              "input": before, "output": manifest(output / "managed"), "icons": icons,
              "catalogSha256": sha(CATALOG), "trackedSources": {p: sha(ROOT / p) for p in SOURCES},
              "baseContent": resources,
              "achievementCount": 32, "hiddenCount": sum(r["hidden"] for r in catalog),
              "vanillaSaveFormatsChanged": False, "runtimeNetworkAccess": False,
              "physicalAcceptance": "NOT_RUN"}
    (output / "achievement-manifest.json").write_text(json.dumps(report, indent=2, sort_keys=True) + "\n")
    if manifest(source / "managed") != before:
        raise ValueError("canonical input unexpectedly changed during derivation")
    print("PASS: derived vanilla " + args.platform + " achievements; canonical input unchanged; 32 achievements, 64 pinned icons")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

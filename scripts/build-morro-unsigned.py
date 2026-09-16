#!/usr/bin/env python3
"""Build one fresh unsigned vanilla or build-49 Everest product, serially."""
from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]


def script(name: str, *arguments: object) -> list[object]:
    path = ROOT / "scripts" / name
    return ([sys.executable, path] if path.suffix == ".py" else [path]) + list(arguments)


def environment() -> dict[str, str]:
    env = dict(os.environ)
    if any(key.startswith("Malloc") for key in env):
        raise ValueError("remove allocator experiment variables before a normal product build")
    tools = ROOT / ".build/apple-everest/toolchain/dotnet10"
    if (tools / "dotnet").is_file():
        env["DOTNET_ROOT"] = str(tools)
        env["PATH"] = str(tools) + os.pathsep + env["PATH"]
    env.setdefault("DEVELOPER_DIR", "/Applications/Xcode-26.6.app/Contents/Developer")
    env.update(MSBUILDDISABLENODEREUSE="1", DOTNET_CLI_USE_MSBUILD_SERVER="0",
               UseSharedCompilation="false", PYTHONDONTWRITEBYTECODE="1",
               DOTNET_CLI_TELEMETRY_OPTOUT="1", DOTNET_NOLOGO="1")
    env.setdefault("DOTNET_CLI_HOME", str(ROOT / ".build/morro/dotnet-home"))
    env.setdefault("NUGET_PACKAGES", str(ROOT / ".build/morro/nuget-packages"))
    return env


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--product", choices=("vanilla", "everest49"), required=True)
    parser.add_argument("--platform", choices=("ios", "tvos"), required=True)
    parser.add_argument("--run", required=True, help="new alphanumeric run name")
    parser.add_argument("--bundle-id", required=True, help="explicit unsigned app identity; never installs")
    parser.add_argument("--game-root", type=Path, default=ROOT / ".private/inputs/Celeste")
    parser.add_argument("--fmod-root", type=Path, default=ROOT / ".private/inputs/FMOD")
    parser.add_argument("--package-root", type=Path, default=ROOT / ".private/inputs/packages")
    parser.add_argument("--prepare-only", action="store_true")
    args = parser.parse_args()
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]{0,79}", args.run):
        parser.error("run name must be 1–80 alphanumeric, underscore or hyphen characters")
    if not re.fullmatch(r"[A-Za-z][A-Za-z0-9-]*(?:\.[A-Za-z0-9-]+)+", args.bundle_id):
        parser.error("invalid bundle identifier")
    if subprocess.check_output(["git", "status", "--porcelain"], cwd=ROOT, text=True).strip():
        parser.error("final product generation requires a clean committed checkout")
    for value in (args.game_root, args.fmod_root):
        if not value.is_dir():
            parser.error("required private input missing; see docs/MORRO_BUILDING.md")
    work = ROOT / ".build/morro/runs" / args.run
    if work.exists() or any(p.is_symlink() for p in (work, *work.parents)):
        parser.error("run needs a fresh owned output with no symlink ancestors")
    subprocess.run(["git", "check-ignore", "--no-index", "-q", str(work / "receipt.json")], cwd=ROOT, check=True)
    work.mkdir(parents=True)
    env = environment()
    env["CELESTE_GAME_ROOT"] = str(args.game_root.resolve())
    env["FMOD_SDK_ROOT"] = str(args.fmod_root.resolve())
    records = []
    source = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()

    def run(name: str, command: list[object]) -> None:
        print("Morro: " + name, flush=True)
        start = time.monotonic()
        with (work / (name + ".private.log")).open("xb") as log:
            result = subprocess.run([str(x) for x in command], cwd=ROOT, env=env,
                                    stdout=log, stderr=subprocess.STDOUT)
        records.append({"phase": name, "exitCode": result.returncode,
                        "wallSeconds": round(time.monotonic() - start, 3)})
        receipt = {"sourceCommit": source, "product": args.product, "platform": args.platform,
                   "signing": "unsigned", "installation": "NOT_RUN", "phases": records}
        (work / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(name + " failed; inspect its private run log")

    run("host-check", script("check-ios-host.sh"))
    run("validate-game", script("validate-celeste-input.sh", "--game-root", args.game_root,
                                "--output", work / "game-input.json"))
    if args.product == "vanilla" and not args.prepare_only:
        if args.platform == "ios":
            command = [ROOT / "build-ios.sh", "--unsigned", "--non-interactive"]
        else:
            # The public tvOS builder reuses the supported staged native root.
            # Verify and stage a local native build before its cache decision.
            native_work = ROOT / ".build/tvos-native/self-build"
            native_output = ROOT / "artifacts/tvos-native/self-build"
            if (native_output / "normalized-manifest.json").exists():
                run("verify-tvos-native", script("verify-tvos-native.sh", "--build-dir", native_work, "--output-dir", native_output))
                run("stage-tvos-native", script("prepare-tvos-host-native.sh", "--artifact-dir", native_output,
                                               "--stage1-build-dir", native_work, "--clean"))
            command = [ROOT / "build-tvos.sh", "--mode", "ipa", "--non-interactive", "--no-color"]
        command += ["--game-root", args.game_root, "--fmod-root", args.fmod_root,
                    "--bundle-id", args.bundle_id]
        run("vanilla-product", command)
    else:
        # The expanded preflight uses canonical iOS managed source even for tvOS.
        # These are source preparation steps, never a copied closure or app AOT.
        if args.platform == "ios":
            if not (ROOT / "artifacts/ios-native/normalized-manifest.json").exists():
                run("fetch-ios-native", script("fetch-ios-native-deps.sh"))
                run("build-ios-native", script("build-ios-native.sh"))
            run("verify-ios-native", script("verify-ios-native.sh"))
            run("stage-ios-native", script("prepare-ios-foundation.sh", "--clean"))
            run("stage-ios-fmod", script("prepare-fmod-ios.sh", "--sdk-root", args.fmod_root, "--clean"))
        else:
            native_work = ROOT / ".build/tvos-native/self-build"
            native_output = ROOT / "artifacts/tvos-native/self-build"
            if not (native_output / "normalized-manifest.json").exists():
                run("fetch-tvos-native", script("fetch-tvos-deps.sh", "--build-dir", native_work))
                run("build-tvos-native", script("build-tvos-native.sh", "--build-dir", native_work, "--output-dir", native_output))
            run("verify-tvos-native", script("verify-tvos-native.sh", "--build-dir", native_work, "--output-dir", native_output))
            run("stage-tvos-native", script("prepare-tvos-host-native.sh", "--artifact-dir", native_output,
                                           "--stage1-build-dir", native_work, "--clean"))
            run("stage-tvos-fmod", script("prepare-fmod-tvos.sh", "--sdk-root", args.fmod_root,
                                          "--game-root", args.game_root, "--stage1-artifact-dir", native_output, "--clean"))
            run("canonical-tvos", script("prepare-celeste-tvos-stage6.sh", "--game-root", args.game_root, "--clean"))
        run("canonical-ios", script("prepare-celeste-ios-runtime.sh", "--game-root", args.game_root, "--clean"))
        if args.product == "everest49":
            command = script("build-apple-everest-stage25kn.py", "--package-root", args.package_root,
                             "--chrono-package", args.package_root / "ChronoHelper.zip",
                             "--platform", args.platform, "--signing", "unsigned",
                             "--" + args.platform + "-bundle-id", args.bundle_id,
                             "--work-root", ROOT / ".build/apple-everest/morro" / args.run,
                             "--output", ROOT / "artifacts/apple-everest/morro" / args.run)
            if args.prepare_only:
                command.append("--prepare-only")
            run("everest49-product", command)
    if subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip() != source:
        raise ValueError("source revision changed during build")
    if subprocess.check_output(["git", "status", "--porcelain"], cwd=ROOT, text=True).strip():
        raise ValueError("source tree changed during build")
    print("PASS: Morro unsigned lane; source unchanged; no installation or physical claim")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

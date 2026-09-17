#!/usr/bin/env python3
"""Execute generated pause dispatch and real Collab cleanup with owned UI handles."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def method(source, signature):
    start = source.index(signature)
    opening = source.index("{", start)
    depth, end = 1, opening + 1
    while depth:
        depth += (source[end] == "{") - (source[end] == "}")
        end += 1
    return source[start:end]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--runtime", type=Path, required=True)
    parser.add_argument("--baseline-runtime", type=Path, required=True)
    parser.add_argument("--dotnet", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()
    if output.exists() or not output.is_relative_to(ROOT / ".build"):
        parser.error("use a new private output directory under .build")
    output.mkdir(parents=True)
    level_path = args.runtime / "Celeste/Level.cs"
    collab_path = args.runtime / "Celeste/Mod/AppleEverestStatic/AppleEverestCollabRuntime.cs"
    old_path = args.baseline_runtime / "Celeste/Level.cs"
    level, collab = level_path.read_text(), collab_path.read_text()
    pause = method(level, "public void Pause(")
    original = method(level, "private void AppleEverestCollabOriginalPause(")
    baseline = method(old_path.read_text(), "public void Pause(")
    if original[original.index("{"):] != baseline[baseline.index("{"):]:
        raise ValueError("canonical pause body changed beyond the new post-pause dispatch")
    hook = "global::Celeste.Mod.AppleEverestCollabRuntime.OnPause(this);"
    if pause.count(hook) != 1 or pause.index("AppleEverestCollabOriginalPause(") > pause.index(hook):
        raise ValueError("missing or reordered generated post-pause dispatch")
    generated = ("namespace Celeste { internal partial class Level {\n" + pause + "\n}}\n" +
                 "namespace Celeste.Mod { internal static partial class AppleEverestCollabRuntime {\n" +
                 method(collab, "internal static void OnPause(") + "\n" +
                 method(collab, "private static void CloseOverworld(") + "\n}}\n")
    fixture = ROOT / "tests/fixtures/CollabPauseFixture.cs"
    project = (
        '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
        '<TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
        '<Nullable>disable</Nullable></PropertyGroup><ItemGroup>'
        '<Compile Include="Fixture.cs"/><Compile Include="Actual.cs"/>'
        '</ItemGroup></Project>\n')
    env = {k: v for k, v in os.environ.items() if not k.startswith("Malloc")}
    env.update(DOTNET_ROOT=str(args.dotnet.resolve().parent), MSBUILDDISABLENODEREUSE="1",
               DOTNET_CLI_USE_MSBUILD_SERVER="0", UseSharedCompilation="false")
    results = []
    for name, source in [("fixed", generated), ("missing-dispatch-control", generated.replace(hook, ""))]:
        case = output / name
        case.mkdir()
        (case / "Actual.cs").write_text(source)
        (case / "Fixture.cs").write_bytes(fixture.read_bytes())
        (case / "Probe.csproj").write_text(project)
        command = [str(args.dotnet.resolve()), "build", str(case / "Probe.csproj"), "--nologo",
                   "-c", "Release", "-m:1", "-p:BuildInParallel=false", "-p:UseSharedCompilation=false"]
        build = subprocess.run(command, cwd="/private/tmp", env=env, capture_output=True, text=True)
        (output / (name + "-build.log")).write_text(build.stdout + build.stderr)
        if build.returncode:
            raise ValueError("probe compilation failed: " + name)
        run = subprocess.run([str(args.dotnet.resolve()), str(case / "bin/Release/net10.0/Probe.dll")],
                             cwd=case, env=env, capture_output=True, text=True)
        (output / (name + ".log")).write_text(run.stdout + run.stderr)
        if name == "fixed" and run.returncode:
            raise ValueError("production pause cleanup failed")
        if name != "fixed" and (not run.returncode or "lobby card remains over pause menu" not in run.stderr):
            raise ValueError("omitted-dispatch control did not reproduce the reported overlap")
        results.append({"case": name, "exitCode": run.returncode, "expectedResult": True})
    report = {"status": "PASS", "cases": 48, "missingDispatchRejected": True,
              "originalPauseBodyUnchanged": True, "results": results,
              "sourceSha256": {"Level.cs": digest(level_path), "CollabRuntime.cs": digest(collab_path),
                               "BaselineLevel.cs": digest(old_path), "Fixture.cs": digest(fixture)},
              "scope": "Actual generated dispatch and runtime cleanup with owned UI handles; no GPU or physical-input claim."}
    (output / "result.json").write_text(json.dumps(report, indent=2) + "\n")
    print("PASS: 48 pause/cleanup cases; missing dispatch reproduces the reported overlap")


if __name__ == "__main__":
    main()

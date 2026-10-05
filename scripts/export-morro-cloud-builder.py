#!/usr/bin/env python3
"""Export the private Apple builder bound to an explicit published revision."""
from __future__ import annotations

import argparse
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
FILES = ("README.md", ".github/workflows/build.yml", ".github/workflows/cleanup.yml",
         "scripts/cloud-common.sh", "scripts/prepare-inputs.py", "scripts/build-products.py")
OLD_REPOSITORY = "hmcneill46/morro-celeste"
OLD_REVISION = "cc02b0cc697bd620c0dc864d4af3c60d79a95e85"


def render(template: Path, repository: str, revision: str) -> dict[str, bytes]:
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9-]*/[A-Za-z0-9][A-Za-z0-9_.-]*", repository):
        raise ValueError("source repository must be a literal OWNER/REPO")
    if not re.fullmatch(r"[0-9a-f]{40}", revision) or revision == "0" * 40:
        raise ValueError("source revision must be a nonzero full lowercase commit SHA")
    outputs = {}
    for relative in FILES:
        source = template / relative
        if source.is_symlink() or not source.is_file():
            raise ValueError("missing or linked canonical template file: " + relative)
        text = source.read_text()
        if relative in (".github/workflows/build.yml", "scripts/cloud-common.sh"):
            if text.count(OLD_REPOSITORY) != 1 or text.count(OLD_REVISION) != 1:
                raise ValueError("canonical source pin layout changed: " + relative)
        outputs[relative] = text.replace(OLD_REPOSITORY, repository).replace(OLD_REVISION, revision).encode()
    return outputs


def export(destination: Path, outputs: dict[str, bytes], check: bool = False) -> None:
    destination = destination.absolute()
    if any(p.is_symlink() for p in (destination, *destination.parents)):
        raise ValueError("export destination must have no symlink ancestors")
    if check:
        if not destination.is_dir():
            raise ValueError("missing export directory")
        actual = {p.relative_to(destination).as_posix() for p in destination.rglob("*")
                  if p.is_file() and ".git" not in p.relative_to(destination).parts}
        if actual != set(outputs):
            raise ValueError("export has missing or extra files")
        for relative, data in outputs.items():
            target = destination / relative
            if any(p.is_symlink() for p in (target, *target.parents)) or target.read_bytes() != data:
                raise ValueError("export differs: " + relative)
        return
    # An existing destination is never overwritten, even if it looks empty.
    destination.mkdir(parents=True, exist_ok=False)
    for relative, data in outputs.items():
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        with target.open("xb") as out:
            out.write(data)
        if relative.startswith("scripts/"):
            target.chmod(0o755)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--destination", type=Path, required=True)
    parser.add_argument("--source-repository", required=True)
    parser.add_argument("--source-sha", required=True)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    outputs = render(ROOT / "cloud-builder-template", args.source_repository, args.source_sha)
    subprocess.run(["git", "cat-file", "-e", args.source_sha + "^{commit}"], cwd=ROOT, check=True)
    export(args.destination, outputs, args.check)
    print("PASS: six-file private Apple builder bound to the explicit source revision")
    print("No publication or hosted build performed; the source commit must be reachable there before running Actions.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env bash
set -euo pipefail

expected_dotnet="10.0.302"
expected_workload_set="10.0.302.0"
expected_xcode="26.6"
expected_xcode_build="17F113"
expected_sdk="26.5"
expected_macos="26.3"

fail() { echo "error: $*" >&2; exit 1; }
for tool in python3 dotnet sw_vers uname xcodebuild xcrun; do
  command -v "$tool" >/dev/null || fail "missing required host command: $tool"
done
case "$(uname -m)" in
  arm64|x86_64) ;;
  *) fail "requires a supported arm64 or x86_64 Mac host" ;;
esac
# HOST-C qualified x86_64 macOS 26.6.2 with the same exact device SDKs.
# The host OS floor is independent of the arm64 device deployment target.
host_macos="$(sw_vers -productVersion)"
python3 - "$host_macos" "$expected_macos" <<'PY' || fail "requires macOS 26.3 or later in the 26.x family"
import re, sys
actual, minimum = sys.argv[1:]
if not re.fullmatch(r"26\.[0-9]+(?:\.[0-9]+)?", actual):
    raise SystemExit(1)
raise SystemExit(0 if tuple(map(int, actual.split('.'))) >= tuple(map(int, minimum.split('.'))) else 1)
PY
[[ "$(dotnet --version)" == "$expected_dotnet" ]] || fail "requires .NET SDK $expected_dotnet"
[[ "$(dotnet workload --version)" == "$expected_workload_set" ]] || fail "requires workload set $expected_workload_set"
xcode_version="$(xcodebuild -version)"
grep -Fxq "Xcode $expected_xcode" <<<"$xcode_version" || fail "requires Xcode $expected_xcode"
grep -Fxq "Build version $expected_xcode_build" <<<"$xcode_version" || fail "requires Xcode build $expected_xcode_build"
[[ "$(xcrun --sdk iphoneos --show-sdk-version)" == "$expected_sdk" ]] || fail "iPhoneOS SDK drift"
[[ "$(xcrun --sdk iphonesimulator --show-sdk-version)" == "$expected_sdk" ]] || fail "iPhoneSimulator SDK drift"
[[ "$(xcrun --sdk appletvos --show-sdk-version)" == "$expected_sdk" ]] || fail "AppleTVOS SDK drift"
[[ "$(xcrun --sdk appletvsimulator --show-sdk-version)" == "$expected_sdk" ]] || fail "AppleTVSimulator SDK drift"
workloads="$(dotnet workload list)"
grep -Eq '^ios[[:space:]]+26\.5\.10301/10\.0\.100' <<<"$workloads" || fail "exact iOS workload is unavailable"
grep -Eq '^tvos[[:space:]]+26\.5\.10301/10\.0\.100' <<<"$workloads" || fail "exact tvOS workload is unavailable"
printf 'PASS: modern iOS host doctor (.NET %s, Xcode %s %s, iOS SDK %s)\n' \
  "$expected_dotnet" "$expected_xcode" "$expected_xcode_build" "$expected_sdk"

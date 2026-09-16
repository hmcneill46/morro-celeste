> Historical user-supplied reference, archived during the Morro migration.
> Its older toolchain experiments are evidence, not current Morro build instructions.
> Personal checkout paths and the private artifact hyperlink are sanitized where present.

# Intel / macOS Sequoia vanilla iOS build experiment

Date: 7 September 2026. Starting commit: `d6bdaa4cf6d4c8ccf451d6ac1bfb66cde5d0dc10`.

This is a follow-up to `BUILD_DOCS_AUDIT_2026-09-02.md`, at the user's request to try minimal compatibility modifications rather than stop at the documented toolchain restriction. This follow-up targets vanilla iOS only, not Everest or tvOS. Original build guides remain unchanged.

## Result: unsigned vanilla iOS IPA built successfully on Intel

**Yes: this Intel VM on macOS 15.7.9 can build the vanilla iOS port.** The matching older Microsoft workload and four small project-metadata overrides were sufficient. No game-source, native-source, FMOD, AOT compiler, or Xcode SDK patches were needed in this follow-up.

- IPA: `Celeste-vanilla-iOS-unsigned.ipa` (historical private artifact; not included)
- App: `modern-ios/CelesteIOSRuntimeHost/bin/Release/net10.0-ios26.2/ios-arm64/CelesteIOSRuntimeHost.app`
- IPA size: **881,264,104 bytes** (about 840.4 MiB).
- SHA-256: `ba3b7650815ad73885ed8bd484302d2a605aebccd11a6f17b7125c7f0d61fd3a`.
- Port version **0.1.1**, build **35**; vanilla Celeste content remains 1.4.0.0.
- Matching-toolchain publish exited **0**, approximately **2 minutes 58 seconds** (21:20:59–21:23:57 BST), excluding prerequisite installation, prior native builds and game preparation.
- This is an **unsigned arm64 device** package, not an Intel simulator build. Installation requires subsequent signing. No physical-device launch or gameplay testing was performed.

## Environment and approach

- Intel x86_64 VM, macOS 15.7.9 (24G830).
- Xcode 26.3 (17C529), iPhoneOS SDK 26.2.
- Existing SDK 10.0.302 and workload 26.5.10301 require Xcode 26.6, so are not a matching combination.
- Microsoft publishes an exact matching alternative: .NET SDK/workload set **10.0.202**, iOS workload **26.2.10233**, requiring Xcode **26.3** and macOS **15.6+**. [Official release and installation instructions](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode26.2-10233).
- Reuse the previously generated canonical vanilla Celeste inputs, staged FMOD 1.10.09, and native libraries compiled and semantically verified on this same Intel VM with SDK 26.2. See the first report for their validation and the official native-lock mismatch. These are not newly downloaded game inputs or official SDK-26.5 native artifacts.
- Keep Release, arm64 device, full trimming, Mono full AOT, LLVM, interpreter disabled, and signing disabled. No gameplay-source modifications or newer API stubs are planned.

## Attempts and findings

1. With the original .NET 10.0.302 toolchain, direct publish using `ValidateXcodeVersion=false` got past the version check, but failed in the managed registrar with **MT4162**, **MT2431**, and **NETSDK1144**. The 26.5 bindings contain types introduced in iOS 26.4 that SDK 26.2 cannot compile (CoreNFC, Foundation, CarPlay, AVKit). This confirms that merely disabling the version check is insufficient. Log: `artifacts/ios/logs/intel-publish-2026-09-07.log`.
2. Choose the officially matching older workload instead. Install SDK 10.0.202 x64 alongside 10.0.302 under `/usr/local/share/dotnet`; select it from an ignored working directory, `.build/ios-self-build/intel-toolchain`, with a local `global.json` pin. The repository-root SDK pin is not changed. An installer-script path retained from the previous session no longer existed, so the official script was downloaded again from `https://dot.net/v1/dotnet-install.sh`.
3. Add an opt-in `CelesteIOSTargetFramework` property to the host and FNA; apply the same one-line override to the two already-generated iOS projects. Pass `-p:CelesteIOSTargetFramework=net10.0-ios26.2` for this experiment. Default targets remain `net10.0-ios26.5`; platform-neutral projects remain `net10.0`. Native code and user-supplied game/FMOD files are unchanged in this follow-up. An initial direct retarget was refined to this opt-in override before the matched-toolchain build, avoiding a default downgrade for other machines. A proposed generator change was reverted before use because generated project files participate in the locked source-tree hash: changing its default output would break the normal preparation path. Regenerating inputs will therefore require reapplying the two local project overrides; preparation manifests describe the original pre-override tree, not a newly canonicalized tree.
4. Add an optional `--compile-sdk` argument to the package verifier, defaulting to its original `26.5`. This allows explicitly verifying SDK `26.2` without skipping architecture, platform, minimum-OS, content, AOT, audio, UI, or unsigned-package checks; the JSON records the selected SDK rather than falsely claiming 26.5.
5. Compare every file against the final prepared-game manifest (`ios-managed-stage24e1.json`): of **944 recorded files**, only `Celeste.Content.Modern.csproj` and `Celeste.Modern.csproj` differ. All recorded game-source/resource files retain their original hashes.

## Verification performed

- Matching workload installed successfully: SDK/workload set `10.0.202`, iOS `26.2.10233`, Mono/AOT runtime `10.0.6`. MSBuild evaluation confirmed `TargetFramework=net10.0-ios26.2`, `TargetPlatformVersion=26.2`, `NETCoreSdkVersion=10.0.202`.
- The full publish passed managed compilation, registrar, trimming, Intel-host Mono cross-AOT/LLVM compilation, native linking and IPA creation. The normal .NET publish generated the IPA automatically; an extra manual zip was unnecessary for this direct path.
- `scripts/verify-ios-package.py --lane device --product celeste --signing unsigned --compile-sdk 26.2` passed every existing package check. Evidence: `artifacts/ios/intel-2026-09-07/package-verification.json`.
- Verified arm64-only Mach-O, `platform IOS`, compile SDK `26.2`, minimum iOS `15.0`; iPhone/iPad families, landscape policy, icons, scene delegate, privacy/storage declarations, touch controls and Files UI markers.
- Verified AOT data for every bundled managed assembly, no interpreter component, no forbidden JIT symbols, linked SDL/FNA3D Metal/FMOD closure (FMOD Studio and low-level symbols), all seven FMOD banks, and canonical Content SHA-256 `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`.
- `codesign -dv` reported **“code object is not signed at all”**. The verifier found neither a signature directory nor provisioning profile.
- `unzip -tq` found no compressed-data errors. An independent comparison hashed every one of the **1,301 IPA payload files** against the verified app: all matched, with no duplicate file names or unexpected file entries.
- Negative test: omitting `--compile-sdk 26.2` still rejects this bundle with `compile SDK is not 26.5`, confirming the verifier's default restriction was preserved.
- Python syntax and `git diff --check` passed. The root still selects SDK `10.0.302` and its original iOS/tvOS `26.5.10301` workloads; installing the alternate feature band did not replace them.

Package inspection establishes build/package correctness, not runtime correctness. The successful log contains compiler/analyzer warnings, including platform availability (`CA1416`, `CA1422`), stream reads (`CA2022`), reflection/XML serialization trimming (`IL2026`, `IL2057`, `IL2067`, etc.), and dynamic-code analysis (`IL3050`). They were not suppressed or “fixed” by weakening full trimming/AOT. Gameplay, audio playback, save import/export, and minimum-iOS behavior still require a signed physical-device test.

## What this changes about the documentation conclusions

The original report remains an accurate record of the **pinned, documented** build attempt on 2 September, but its blocker is not a universal Intel/macOS limitation. This experiment now demonstrates the entire iOS unsigned-IPA build on Intel/Sequoia, not just native compilation.

- **“Intel cannot build the port” is too broad.** Intel successfully cross-compiled the full arm64 iOS application here. This does not validate an Intel simulator lane or every Intel machine.
- **Xcode/workload matching is a real constraint.** The documented 26.5 workload genuinely failed with this older SDK even after its version check was disabled. The successful solution used Microsoft's matching 26.2 workload, with the Xcode validation left enabled.
- **macOS Tahoe is required for the documented newer Xcode route, not for every viable version of this port.** Xcode 26.3 plus its matching .NET iOS workload worked on Sequoia 15.7.9.
- **The docs alone do not describe this successful compatibility route.** The older SDK/workload selection, alternate working-directory SDK pin, iOS target override, bypass of repository-specific host/native acceptance checks by direct publishing, and explicit verifier SDK argument were inferred through source inspection and Microsoft's release notes.
- **The stock public builder still does not support this lane.** Its original host restrictions, SDK-26.5 native locks and hard-coded output path remain unchanged. This is a verified local compatibility build using the earlier audit's staged SDK-26.2 inputs, not a newly certified clean-checkout one-command recipe.
- The initial report's prerequisite/documentation findings, including the missing .NET 6 runtime requirement for decompilation, are not undone by this successful publish. All original setup documents were left as requested.

## Exact retained changes and evidence

This follow-up retains only three tracked code-file edits: one opt-in `TargetFramework` line in each of the host/FNA projects, and the explicit expected-SDK option in the package verifier. Two ignored generated project files have matching one-line overrides. The alternate SDK pin is ignored under `.build/ios-self-build/intel-toolchain/global.json`.

The older `build-tvos.sh` edits belong to the 2 September audit and were preserved, not extended here. The iOS generator, root `global.json`, host checks, native lock files and all original guides are unchanged. No commits, uploads, signing operations or tvOS rebuild were performed in this follow-up.

Evidence logs:

- `artifacts/ios/logs/intel-publish-2026-09-07.log` — failed newer-workload/version-check-bypass attempt.
- `artifacts/ios/logs/intel-workload-install-2026-09-07.log` — successful matched-workload installation.
- `artifacts/ios/logs/intel-matched-publish-2026-09-07.log` — successful full-AOT publish, including warnings.
- `artifacts/ios/logs/intel-package-verification-2026-09-07.log` — package verifier pass.
- `artifacts/ios/logs/intel-ipa-integrity-2026-09-07.log` — ZIP integrity pass.

## Reproduction of the compatibility publish (prepared workspace)

These commands are an experimental continuation from the staged inputs left by the first audit, **not** a claim that the unmodified public builder works on Intel. The original host checks, native acceptance locks, root SDK pin, and wrapper's SDK-specific output lookup are not relaxed. Run the direct publish from the alternate SDK directory.

```sh
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/celeste-dotnet-install.sh
bash /tmp/celeste-dotnet-install.sh --version 10.0.202 --architecture x64 --install-dir /usr/local/share/dotnet
cd $HISTORICAL_CHECKOUT/.build/ios-self-build/intel-toolchain
dotnet --version  # must print 10.0.202
dotnet workload install ios --version 10.0.202
```

The alternate directory contains this local `global.json`:

```json
{"sdk":{"version":"10.0.202","rollForward":"disable"}}
```

In each of these four files, the original `TargetFramework` line is followed by the opt-in property shown below:

- `modern-ios/CelesteIOSRuntimeHost/CelesteIOSRuntimeHost.csproj` (tracked)
- `modern-ios/FNA.iOS/FNA.iOS.csproj` (tracked)
- `.build/celeste-ios/current/managed/Celeste.Modern.csproj` (ignored generated file)
- `.build/celeste-ios/current/managed/Celeste.Content.Modern.csproj` (ignored generated file)

```xml
<TargetFramework>net10.0-ios26.5</TargetFramework>
<TargetFramework Condition="'$(CelesteIOSTargetFramework)' != ''">$(CelesteIOSTargetFramework)</TargetFramework>
```

From the alternate SDK directory:

```sh
dotnet publish $HISTORICAL_CHECKOUT/modern-ios/CelesteIOSRuntimeHost/CelesteIOSRuntimeHost.csproj \
  -c Release -r ios-arm64 --self-contained true \
  -p:CelesteIOSTargetFramework=net10.0-ios26.2 \
  -p:IOSProductMode=Celeste -p:EnableFmodDeviceFoundation=true \
  -p:CelesteAppleRepoRoot=$HISTORICAL_CHECKOUT \
  -p:ArchiveOnBuild=false -p:UseInterpreter=false -p:RunAOTCompilation=true \
  -p:MtouchLink=Full -p:TrimMode=full -p:MtouchUseLlvm=true -p:PublishTrimmed=true \
  -p:EnableCodeSigning=false
```

No `ValidateXcodeVersion=false` is used with the matching workload.

The generated IPA is under `modern-ios/CelesteIOSRuntimeHost/bin/Release/net10.0-ios26.2/ios-arm64/publish/CelesteIOSRuntimeHost.ipa`. Copy it to the desired artifact location; no signing credentials are needed to produce it.

Run the bundle check from the repository root:

```sh
python3 scripts/verify-ios-package.py \
  --app modern-ios/CelesteIOSRuntimeHost/bin/Release/net10.0-ios26.2/ios-arm64/CelesteIOSRuntimeHost.app \
  --lane device --product celeste --signing unsigned --compile-sdk 26.2 \
  --output artifacts/ios/intel-2026-09-07/package-verification.json
```

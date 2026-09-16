> Historical user-supplied reference, archived during the Morro migration.
> Its older toolchain experiments are evidence, not current Morro build instructions.
> Personal checkout paths and the private artifact hyperlink are sanitized where present.

# Vanilla iOS and tvOS fresh-machine build/documentation audit

Date: 2 September 2026 (Europe/London). Repository: `hmcneill46/celeste-ios`, branch `tvos-port`, starting commit `d6bdaa4cf6d4c8ccf451d6ac1bfb66cde5d0dc10`.

**Later follow-up (7 September):** A vanilla unsigned iOS IPA was successfully built on this Intel/macOS 15.7.9 VM using a matching older workload and minimal opt-in project changes. See [the Intel iOS build report](INTEL_IOS_BUILD_2026-09-07.md). The results below describe the original pinned-toolchain audit; they are not a claim that every possible Intel compatibility route is blocked.

## Result

**Neither unsigned IPA was produced. A genuine external Xcode/macOS blocker was reached for both products.**

The pinned .NET iOS and tvOS workloads, version `26.5.10301` in workload set `10.0.302.0`, require **Xcode 26.6**. This VM has **Xcode 26.3** on **macOS 15.7.9**. Apple supports Xcode 26.6 on macOS Tahoe 26.2–26.x, not Sequoia 15.x. The matching Microsoft release notes confirm the same requirement. This is not merely a repository policy check. [Apple Xcode requirements](https://developer.apple.com/xcode/system-requirements), [Microsoft 26.5.10301 release](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode26.6-10301).

| Product | Furthest successful work | Blocking result |
| --- | --- | --- |
| tvOS vanilla | Exact game/FMOD validation; complete locked-source fetch; all six native XCFrameworks built; native archive/symbol/platform/link-probe verification passed | Native staging rejected the Xcode-26.3 output hash. A separate invocation of the actual tvOS project's SDK-detection target then confirmed Microsoft's Xcode-26.6 requirement. |
| iOS vanilla | Exact game/FMOD validation; all five native XCFrameworks built; all deeper native checks passed; two clean native builds were reproducible; FMOD staging; canonical Celeste generation; managed Celeste compilation | After narrowly controlled, temporary local compatibility changes, the actual product publish reached Microsoft's Xcode-version error before full AOT/native app completion. |

The documentation is **not yet a complete fresh-machine recipe**, even for a suitably equipped Mac: it omits the .NET 6 runtime needed by its locked decompiler, and the iOS guide omits additional prerequisites/installation commands. A real tvOS FMOD path/discovery bug and a misleading tvOS host-doctor success were fixed minimally. Existing documentation was left unchanged.

## Scope and method

The requested scope was the normal **vanilla** `build-ios.sh` and `build-tvos.sh` paths, unsigned device IPAs, using the supplied game and FMOD downloads. No Everest lane, signing, device installation, cloud upload, or App Store distribution was attempted.

The supplied clone initially had no tracked modifications. Its recursive submodules were already present at the recorded revisions. The documented command-scoped HTTPS rewrite plus recursive submodule update was run successfully; this was an idempotent check, not proof of a second completely fresh clone.

Primary guides used: root `README.md`, `docs/README.md`, `docs/BUILDING.md`, `docs/IOS_BUILDING.md`, `docs/CELESTE_INPUTS.md`, `docs/TROUBLESHOOTING.md`, and the native-pipeline documentation. Scripts, lock files, and Microsoft/Apple documentation were inspected when actual behavior diverged. Those inspections and compatibility experiments are explicitly distinguished below from following the user guide verbatim.

No game files or original FMOD files were edited. The DMG was mounted read-only. Generated proprietary source/content, build products, and logs remain under the repository's ignored roots. No private inputs or outputs were uploaded.

## Initial machine and installed prerequisites

| Item | Initial state / result |
| --- | --- |
| Architecture | Intel `x86_64`, virtualized host supplied by the user |
| macOS | `15.7.9`, build `24G830` |
| Xcode | `26.3`, build `17C529`; selected full Xcode under `/Applications/Xcode.app` |
| Device and simulator SDKs | iOS and tvOS `26.2` |
| Free space | About 457 GiB initially; about 448 GiB after this audit |
| Homebrew | Installed under `/usr/local`; initially only LinearMouse listed |
| .NET / GNU Make / Mono | Absent initially |
| Physical iPhone/iPad | None available; this correctly did not prevent unsigned-mode input/native work |

Installed during the audit:

- `brew install make mono`: GNU Make `4.4.1`, Mono `6.14.1`, and their Homebrew dependencies. Homebrew warned that Intel macOS was unsupported as of September 2026. Its OpenSSL dependency initially reported a post-install failure; `brew postinstall openssl@3` subsequently succeeded. `gmake`, `mono`, and `monodis` were functional.
- Microsoft's official `dotnet-install.sh`, exact SDK `10.0.302`, architecture `x64`, installed under `/usr/local/share/dotnet`, with `/usr/local/bin/dotnet` linked to it. This location did not previously contain a .NET installation.
- `dotnet workload install tvos --version 10.0.302.0` — documented in troubleshooting; succeeded.
- `dotnet workload install ios --version 10.0.302.0` — inferred by substituting `ios` into the documented tvOS command; succeeded.
- **.NET runtime `6.0.36` x64**, installed only after the decompiler failed. This is an undocumented required download in the current normal pipeline.

Final workload listing showed both `ios` and `tvos` at `26.5.10301/10.0.100`, workload version `10.0.302.0`. The x64 SDK successfully downloaded Intel-host-to-arm64-device AOT packs for both products; toolchain acquisition itself did not require Apple silicon.

The .NET SDK's first run also performed its normal first-run setup, including creating an ASP.NET HTTPS development certificate; it was not explicitly trusted. No Apple credentials, signing certificates, provisioning profiles, or devices were configured by this audit.

## Supplied proprietary inputs

### Celeste

The supplied folder was `$HOME/Downloads/celeste-linux`. The unmodified validator identified:

```text
Celeste 1.4.0.0
Store: itch.io
Source platform: Linux
Runtime: FNA 21.3.5
Profile: itch-linux-fna-1.4.0.0
Canonical game: celeste-1.4.0.0-a
Input validation: PASS
```

The docs correctly say Linux files are input data, not a Linux game that must run on the Mac. No Steam, Epic, Legendary, or additional store client was needed. Acquisition through a storefront was not retested because the files had already been downloaded.

### FMOD

The supplied `fmodstudioapi11009ios-installer.dmg` mounted read-only at:

```text
/Volumes/FMOD Programmers API iOS
```

The real SDK root inside it is:

```text
/Volumes/FMOD Programmers API iOS/FMOD Programmers API
```

Both platform validators accepted the inner SDK as **FMOD Engine iOS/tvOS 1.10.09 build 97915**. The tvOS validator additionally passed arm64 TVOS archive/platform/minimum-OS checks. iOS FMOD staging later succeeded, including the reviewed duplicate-symbol localization. The exact version/package advice is correct. The filename's `fmodstudioapi` text is not evidence that the user downloaded the wrong product; the validated payload is the requested SDK.

## Execution record

### tvOS

1. Original `./build-tvos.sh --check-host` correctly collected three missing tools: `dotnet`, `gmake`, and `monodis`.
2. After installing the documented tools/workload, the original doctor **passed** on Intel/macOS 15.7.9/Xcode 26.3/SDK 26.2, with only an Intel warning.
3. The documented noninteractive unsigned workflow was attempted with the actual game path, outer FMOD volume, `--mode ipa`, and a local reverse-DNS bundle identifier.
4. It failed in phase 3 because the original tvOS builder did not resolve the inner `FMOD Programmers API` directory. A bounded resolver was added and the identical command retried.
5. Exact inputs passed. No signing was required. The source fetch completed in **17m14s**. It cloned locked public repositories/mirrors, applied tracked patches, and required no extra manually installed CMake/Ninja package.
6. All six native components built for arm64 tvOS device and simulator in **5m25s**. Native verification completed in **7s**, including archive members, exports, platform/deployment checks, and link probes.
7. `prepare-tvos-host-native.sh` rejected the generated logical checksum:

```text
expected: 6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc
actual:   6f624ce81d85fba108abdf3e9c8c38edd9aaa09cf78cf5d820ff383abce384e4
```

The source revisions were locked and the semantic native checks passed. The logical hash includes compiled object data, so different compiler/SDK output is sufficient to change it. This is consistent with toolchain drift; it is **not** proof that the new binaries are behaviorally identical to the accepted ones. The tvOS acceptance hash was never changed or bypassed.

8. To determine whether fixing that hash alone could help, the actual tvOS project's SDK-detection target was run independently of game staging:

```bash
dotnet msbuild tvos/CelesteTvOSRuntimeHost/CelesteTvOSRuntimeHost.csproj \
  -t:_DetectSdkLocations -p:RuntimeIdentifier=tvos-arm64 \
  -p:Configuration=Release -p:EnableCodeSigning=false -nologo
```

It failed with Microsoft's own message: `.NET for tvOS (26.5.10301) requires Xcode 26.6; current Xcode is 26.3`. Therefore further tvOS hash changes would not solve this machine's normal build.

### iOS

1. Original `./build-ios.sh --doctor` stopped immediately with `requires an Apple-silicon arm64 Mac`.
2. To investigate whether the hardware/OS restriction was fundamental, only the architecture/macOS/Xcode/SDK gates were temporarily changed to warnings. Exact .NET and workload checks remained enforced. The doctor then passed submodules, disk, game, and FMOD validation and correctly warned about the absent physical device.
3. `./build-ios.sh --unsigned --non-interactive` with the real inputs reached native preparation. Source fetch took **3m53s**, native compilation **34s**. The verifier stopped first on SDK 26.2 versus locked 26.5.
4. A temporary SDK-warning diagnostic allowed every deeper native check to run. They passed; output-lock comparison then failed as expected. A second clean native build produced a **byte-for-byte identical normalized manifest**, confirming reproducibility on this host (manifest-file SHA-256 `6f83877d9b8dfe7e4eb560e37c760f781be054eaafb58d62c8ef6ba9851473d1`).
5. For one controlled attempt to reach the real compiler boundary, the local iOS compile-SDK/native-output locks and builder cache hash were temporarily updated to the generated SDK-26.2 set. The original strict verifier logic was restored, so exact local hashes were still checked. No source/dependency revisions, deployment targets, game transforms, or AOT flags were relaxed. The locally described set's logical hash was `c4f20b30d93f570bb717531bf5cb2b4f7627f9fd26a0a15f49e048897f44cc32`.
6. Native staging and FMOD preparation succeeded. Celeste generation then failed because locked `ilspycmd 8.0.0.7246-preview3` targets `net6.0`, while only .NET 10 was installed. The actual error named missing `Microsoft.NETCore.App 6.0.0 (x64)`. Neither normal guide had instructed downloading it.
7. Installing .NET runtime `6.0.36` resolved that failure. Canonical source generation and its deterministic transformations completed in **56s**. The product publish emitted the managed `Celeste.dll`, then stopped after **16s** with Microsoft's Xcode-26.6 requirement. Full AOT, final native app link, package verification, and IPA promotion were not completed.
8. All temporary iOS compatibility edits were restored to the starting tracked contents. They are evidence-gathering experiments, not retained support for an unaccepted toolchain. Generated experiment artifacts remain ignored and do not satisfy the restored native acceptance lock.

## Findings: accuracy, gaps, and required inference

### High impact

1. **Missing .NET 6 runtime download (both platforms).** The guides list .NET SDK 10.0.302 but not the runtime required by the locked ILSpy tool. `scripts/prepare-celeste-managed.sh:217` invokes that tool without a major-version roll-forward policy. A fresh .NET-10-only machine fails. Installing runtime 6.0.36 fixed it without code changes. The docs need an explicit runtime dependency or the project needs a separately validated decompiler/runtime migration; merely restoring the NuGet tool does not install its runtime.

2. **Original tvOS doctor accepted a known-incompatible Xcode.** It printed a host prerequisite pass for Xcode 26.3/SDK 26.2, despite the docs' accepted 26.6/26.5 matrix and the Microsoft workload's strict requirement. The resulting full native work was wasted and the next error was an unexplained checksum. **Fixed in code:** exact Xcode and tvOS SDK checks now fail early with detected/required versions.

3. **FMOD mounted-DMG discovery/path handling was broken on tvOS.** The original auto-discovery examined only `/Volumes/*/doc/revision.txt`, whereas this official DMG nests the SDK one level deeper. Explicitly supplying the outer volume also failed. This contradicts the troubleshooting advice to mount and rerun or point at the mounted volume. **Fixed in code:** bounded outer/inner resolution plus nested auto-discovery. The iOS builder already handled this correctly.

4. **The current VM cannot run the pinned supported toolchain.** The exact Xcode 26.6 requirement is real and documented, but the tvOS beginner prerequisite list does not spell out its macOS Tahoe requirement. “Full Xcode installed” on a Sequoia machine is insufficient. The iOS guide does name macOS 26.3, although its exact equality check is narrower than Apple's Xcode support range.

### Medium impact

5. **The iOS guide is not self-contained for prerequisite installation.** It names the iOS workload but provides no copyable install command. I inferred `dotnet workload install ios --version 10.0.302.0` from tvOS troubleshooting. More importantly, `scripts/check-ios-host.sh:26` also requires the **tvOS workload**, which the iOS “What you need” list does not state. This run had it because both products were in scope; an iOS-only reader could be blocked.

6. **Mono/`monodis` is a hidden iOS prerequisite.** `scripts/validate-celeste-input.sh:62` requires `monodis`. The tvOS guide explicitly explains Mono installation, but the iOS prerequisite list does not, and its top-level command preflight omits `monodis`. This audit already had Mono for tvOS, so the iOS failure was identified by code inspection rather than uninstalling a working dependency.

7. **Apple-silicon-only wording mixes tested policy with technical necessity.** The troubleshooting page accurately calls Intel/virtualized hosts unvalidated; the main/iOS guides frame Apple silicon as required, and iOS hard-rejects Intel. Both native stacks actually compiled for arm64 device/simulator on this Intel VM; Microsoft supplied x64 cross-AOT packs. This disproves a blanket claim that Intel cannot perform those build stages, but does **not** establish complete Intel IPA support because full AOT never ran. Tahoe also supports selected Intel models, so the Xcode OS requirement is not itself synonymous with Apple silicon. [Apple Tahoe compatibility](https://support.apple.com/en-us/122867).

8. **Exact macOS 26.3 is a repository acceptance gate, not Apple's general minimum.** `check-ios-host.sh` compares the entire OS version for equality, rejecting even other Tahoe point versions. Apple lists Xcode 26.6 support starting at Tahoe 26.2. No full build was performed on another Tahoe release, so this report does not bless those combinations; it recommends distinguishing “tested on 26.3” from “cannot work elsewhere.”

9. **The alternate explicit clone example loses the needed Git transport rewrite.** `docs/BUILDING.md` first gives the correct `git -c url.https://github.com/.insteadOf=git://github.com/ submodule update --init --recursive`, but its fallback `git clone --branch tvos-port --recurse-submodules ...` omits that rewrite. The pinned FNA `.gitmodules` really contains `git://github.com/` URLs. The fallback should not be described as equivalent without addressing that. This is a static command inconsistency; the preferred documented update command succeeded here.

### Lower impact / precision

10. **Raw FMOD validator examples require the inner root.** Both standalone validators reject the outer mounted volume, even though the iOS builder (and now fixed tvOS builder) resolves it. The exact-validator examples should clarify which root they require. No validator behavior was changed.

11. **tvOS validation-only mode does not validate artwork as described.** Help/docs describe input/tooling/artwork validation. In the observed run, phase 5 printed that artwork was deferred until the canonical game tree existed, phase 6 was skipped, and validation reported success. That mode did validate the supplied game/FMOD/tooling but did not generate/verify the artwork pipeline. Left unchanged and recorded here.

12. **Exact-version success text can be misleading if gates are relaxed.** During the diagnostic iOS experiment, warnings were hidden in the phase log while the top-level builder still printed `PASS Exact .NET/Xcode/iOS toolchain`. This is not a normal unmodified-path bug—the original checks fail—but anyone implementing broader compatibility should update that success text and warning propagation too. The experiment was reverted.

13. **Output filename examples are illustrative, not the current build number.** The iOS guide says “for example” build 5, while this checkout prints v0.1.1 build 35. This is not a build defect; a successful run would use the current semantic version/build metadata.

## What the docs describe correctly, and what worked

- They clearly separate vanilla public builders from legacy Xamarin source, historical engineering reports, and experimental Everest work.
- The exact Celeste profile and FMOD release/package guidance matched the provided downloads and validators. Input filenames alone were not trusted.
- The preferred clone/submodule HTTPS rewrite is valid and does not require changing global Git configuration.
- The documented tvOS `brew install make mono` guidance supplied the missing native tools. No CMake, Ninja, Steam client, or extra manually acquired native source package was needed for the tested pipeline.
- Public source fetching, native patch application, device/simulator packaging, real FMOD staging on iOS, and deterministic game generation worked. No hand-editing generated Celeste source was needed.
- The normal builders kept full logs in the documented ignored locations and emitted one-minute elapsed/free-space heartbeats during long operations. Error tails and log pointers were useful, though a late checksum error did not explain the toolchain cause.
- Unsigned mode did not request an Apple account/team/device and did not try to sign or install. The iOS doctor treated absent devices as optional as documented.
- Cache reuse worked once the experimental iOS output was exactly described by its temporary local lock. After restoring official locks, that experimental cache is deliberately no longer accepted.
- The final source diff contains only `build-tvos.sh` plus this report. No original guide, dependency lock, game transform, or app source remains changed.

## Retained code changes and verification

Only `build-tvos.sh` was changed: 16 added lines and one replaced line at audit completion.

1. Add pinned Xcode `26.6` / tvOS SDK `26.5` preflight checks. The fixed `--check-host` now rejects this VM immediately with `Detected: 26.3`, `Required: 26.6` and an actionable remedy.
2. Add the same bounded FMOD root behavior already present in the iOS builder, using the tvOS archive marker, and inspect the official nested directory during auto-discovery.

Checks performed:

- `bash -n build-tvos.sh` — passed.
- Explicit outer FMOD volume in the real tvOS build — passed after the resolver fix, then reached native build.
- Cleared only this audit's generated tvOS saved choices with `--reset-config`; `--mode validate` with no FMOD argument/environment override auto-discovered the mounted official SDK and passed. This was tested before adding the final stricter Xcode preflight.
- `scripts/test-tvos-builder-ui.sh` — **21 deterministic tests passed**.
- `scripts/verify-repository-stage8b.py` — passed docs/links/help/permissions/Git-isolation checks.
- `git diff --check` — passed.

The existing static documentation verifier passing is not evidence that all runtime prerequisites are documented: it did not catch the missing .NET 6 runtime exposed by this execution audit.

## Remaining evidence and limits

Useful ignored evidence:

```text
dist/logs/fetch-native.log
dist/logs/build-native.log
dist/logs/verify-native.log
dist/logs/prepare-host-native.log
dist/logs/last-error.txt                  # final early Xcode rejection
artifacts/tvos-native/self-build/normalized-manifest.json
artifacts/tvos-native/self-build/manifest.json
artifacts/ios/logs/build-ios-product.log  # actual Microsoft iOS Xcode rejection
artifacts/ios/logs/generate-celeste-ios.log
artifacts/ios-native/normalized-manifest.json
artifacts/ios-native/verification/
.build/ios-self-build/                   # incomplete product work, not an IPA
```

Logs are reused/overwritten by retries; the first missing-.NET-6 error is recorded in this report from the observed terminal output, not claimed to remain in the latest successful generation log. The final iOS native manifest records the reverted experiment's local SDK-26.2 acceptance description and is not accepted by the restored repository lock.

About 5.0 GiB remains under `.build`, 121 MiB under `artifacts`, and 4.9 GiB in the newly installed .NET tree. Build intermediates were retained to avoid wasting the completed downloads and to support diagnosis. No successful IPA, signed app, or install was produced. Full AOT, final link, final packaging, final app/IPA verifiers, physical gameplay, all nine storefront acquisitions, cloud compilation, and minimum-space behavior were **not** proven by this audit.

## What is needed to resume

For the currently documented, unmodified acceptance path, supply a host matching the repository's accepted setup—most directly an Apple-silicon Mac/VM on **macOS 26.3 with Xcode 26.6 build 17F113 and iOS/tvOS SDK 26.5**—plus the already identified .NET 10 SDK/workloads, Mono/GNU Make, and **.NET 6 runtime**. Apple's broader Xcode range begins at Tahoe 26.2, but the original iOS doctor currently enforces exactly 26.3.

An OS upgrade/reconfiguration was not initiated: it would be a material change to the supplied VM and may depend on its virtual hardware. Nor was the pinned Microsoft workload downgraded or its Xcode-validation target disabled. Microsoft explicitly treats mismatched Xcode/workload combinations as unsupported; a Sequoia/Xcode-26.3 compatibility port would require separate workload/target/native-lock validation, not merely deleting one guard. [Microsoft Xcode requirement guidance](https://learn.microsoft.com/en-us/dotnet/ios/troubleshooting/xcode-requirement).

Once the host is corrected, rerun both explicit unsigned modes with the same supplied input paths. The restored locks should force replacement of incompatible native caches. The docs audit should then continue through full AOT and final IPA verification; this report does not claim those unexecuted stages work.

**Bottom line:** the inputs are correct, substantial build machinery works even on Intel, and the Xcode requirement is genuine. However, a fresh user cannot currently complete either normal build using only the written prerequisite instructions because .NET 6 is missing from them, the iOS setup guide has additional omissions, and the original tvOS builder mishandled the official FMOD DMG and failed to reject an incompatible Xcode early.

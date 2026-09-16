# HOST-C_FINAL_REPORT

**Outcome: HOST_GREEN_BUILD_ONLY.** Both fresh unsigned full-app AOT products pass their K-L wrappers and applicable final linked/AOT/native/platform/content/package checks. HOST-C is ready for further integration review. This is build-only qualification, not signing, installation, physical acceptance or automatic integration approval.

Completed 2026-09-09 15:52 UTC. The root agent performed the work; no subagents or concurrent app builds were used. Raw logs, binaries, proprietary inputs and detailed local receipts remain private outside Git or in verified-ignored owned outputs.

**Source and delivery.**

| Authority | Revision / state |
| --- | --- |
| Original accepted build-46 game/content/semantics | `be8546d4ae411cb491a0c3bbc4e5343ebf9c9651` |
| Reviewed HOST-B tooling / HOST-C start | `632a451da2f62928404a2408826747aa6fc55b20` |
| Final frozen HOST-C tooling and both accepted products | `b4997fa49f4f8b82f6f2984fe9047396e401ea48` |
| Feature branch | `feature/apple-everest-host-c-aot-qualification` |
| Remote delivery | PUSHED; only this new feature ref changed |
| `origin/tvos-port` | Still `632a451da2f62928404a2408826747aa6fc55b20` |
| Canonical app authority | 0.1.1 (46), unchanged |

The user’s integration of HOST-B was independently verified before creating a separate HOST-C clone with pinned recursive submodules. The final apps use build-46 game/content authorities with a **new tooling/source revision**; they are not described as unmodified builds of `be8546d4`. Each final before-AOT, after-AOT, native-link and packaging receipt names the same clean final revision. The final generation/build roots were newly created and no `--reuse-build` option was used.

The feature is available for review at [the remote branch](https://github.com/hmcneill46/celeste-ios/tree/feature/apple-everest-host-c-aot-qualification). No PR was created.

| Commit | Bounded change |
| --- | --- |
| `82e72498e004b25730beb6514b27091b823d6c0c` | Pinned host/compiler/LLVM provenance; explicit signing verification and caller propagation; tests/documentation. |
| `567f00a6a4739a04737010a467f467a1a76585fd` | Canonicalize SDK `iOS`/`tvOS` argument casing to the existing two allowed device platforms. |
| `b4997fa49f4f8b82f6f2984fe9047396e401ea48` | For unsigned construction, clear entitlement input and disable default entitlement discovery; retain rejection of empty entitlement files. |

**Verification corrections and regression evidence.**

Compiler provenance distinguishes the independently observed macOS host from the arm64 device target. Python host architecture, executing dotnet host/RID/SDK base, .NET SDK/workload/runtime versions, Xcode and device SDKs must agree. Only the exact pinned `Microsoft.NETCore.App.Runtime.AOT.osx-{x64|arm64}.Cross.{ios|tvos}-arm64/10.0.10/tools/mono-aot-cross` is accepted. Its actual Mach-O host architecture, version/target output, compiler SHA-256 and same-pack `llc`/`opt` hashes are bound to the receipt and rechecked across AOT, native linking and final verification. Wrong host/target/platform/version/LLVM, altered receipts/tools/source and missing evidence fail.

The actual SDK item still requires full/static LLVM ARM64 AOT. Final Info.plist, LC_BUILD_VERSION, device family, minimum OS, SDK and ABI checks remain. Both device platforms retain the reviewed `arm64-ios` Mono ABI triple; the platform-specific compiler pack and native platform identify the OS. Process observations independently matched the exact compiler and every AOT/process/LLVM argument for Celeste and DJMapHelper on both final device builds. These samples supplement the actual SDK boundary receipts rather than claiming a complete census of short compiler processes.

Signing verification defaults to development and still requires `codesign --verify --strict`; failure never retries as unsigned. Explicit unsigned verification rejects provisioning/signature resources, entitlement files, signed or malformed native images, and requires codesign’s positive unsigned-object result. It runs the same linked factory/profile, forbidden-call, AOT object, native-section, metadata/MVID, companion and packaged-content proofs.

A tvOS attempt exposed the SDK’s generation of an empty `archived-expanded-entitlements.xcent` from its existing empty entitlement declaration despite `EnableCodeSigning=false`. The unsigned caller now also supplies `CodesignEntitlements=` and `EnableDefaultCodesignEntitlements=false`. The pinned SDK’s [entitlement producer](https://github.com/dotnet/macios/blob/dotnet-10.0.1xx-xcode26.6-10301/msbuild/Xamarin.MacDev.Tasks/Tasks/CompileEntitlements.cs) and installed `_CompileEntitlements` condition explain the file. Real SDK target/evaluation checks for both platforms proved unsigned generation is skipped and development’s declaration remains. No packaged file was removed and no gate was relaxed.

Only six files differ from HOST-B: the two app Python verifiers/recorders, their MSBuild provenance target, the canary caller, one owned test file and one sanitized documentation file. No game/runtime/closure transformation/native source, dependency, compiler optimization, architecture/deployment setting, version, expected hash or normalization rule changed. The entitlement correction affects unsigned signing-input construction, not the device’s compiled game/native semantics. No equality of complete app binary digests across different source revisions is asserted.

| Check | Result |
| --- | --- |
| Portable owned fixtures | 42 test methods PASS, including all 29 HOST-B methods; both host variants × both device platforms and positive/negative signing/provenance cases. |
| Existing AppleEverestBuilder deterministic regression suite | 622 tests PASS; its code is unchanged by all three HOST-C commits. |
| Existing K-L content/physical-identity rejection controls | 11 negative controls PASS; no physical tests executed. |
| Actual pinned SDK unsigned/development caller checks | 4 cases PASS; development checks evaluate properties only, with no signing target or credentials. |
| Each final actual product | 15 platform negative controls and 12 AOT/metadata/native tamper controls PASS. |
| Each final linked production proof | 73 selected factories; all existing linked/profile/forbidden/native bindings PASS. |
| Each final chapter-icon proof | 20 icons resolve; both missing-asset controls reject. |
| Review | Complete scoped diff, shell syntax, Git whitespace, source freeze, privacy and pinned submodule checks PASS. |

The portable host/signing fixtures do not constitute an M1 app run or a real signed-product test. Existing negative controls cover wrong platforms/packs/versions/LLVM, source/compiler/receipt tampering, missing/stale evidence, signed-failure separation, false unsigned results, provisioning, nested/ad-hoc signatures and malformed native data. Actual-product controls also reject changed MVID/custom attributes/initialized fields, stale selected IL, native registration/entrypoint/fixups/code, packaged AOT data and missing/undefined guard code.

**Effective host and unchanged prerequisites.**

| Item | Observed / retained |
| --- | --- |
| Guest | x86_64 macOS 26.6.2, build 25G83 |
| CPU resources | 8 physical / 16 logical guest CPUs; guest brand “Intel Core Processor (Skylake)”. Underlying i9-13900K P-core allocation is user-supplied, not independently exposed by the guest. |
| RAM | 51,539,607,552 bytes = 48 GiB |
| Xcode | 26.6 / 17F113, process-local `DEVELOPER_DIR=/Applications/Xcode-26.6.app/Contents/Developer` |
| Device and simulator SDKs | iOS, iOS Simulator, tvOS and tvOS Simulator: 26.5 |
| Compiler | Apple clang 21.0.0, clang-2100.1.1.101 |
| .NET | SDK 10.0.302; workload set 10.0.302.0; installed ios/tvos manifests 26.5.10301/10.0.100; runtime/AOT compiler packs 10.0.10 |
| Other retained pins | SDKs 8.0.424 and 9.0.317; GNU make 4.4.1, Mono 6.14.1, Python 3.14.7, ILSpy 8.0.0.7246-preview3 and its 6.0.36 runtime |
| Global developer selection | Unchanged: CommandLineTools. Both Xcode installations, pairing, symbol caches and simulator runtimes were left untouched. |

Both .NET device SDK-detection targets passed independently. SDK/workload/compiler pins were not changed. Final workload inventory and the .NET 8/9 versions were re-read after the builds.

Celeste 1.4.0.0 FNA and lawful external FMOD 1.10.09 build 97915 were revalidated from recorded VM inputs. Canonical Celeste class-A Content remains 1,216 files / 1,158,665,183 bytes, logical SHA-256 `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`. FMOD’s accepted bindings, native bridge and bank authority were retained, including tvOS FMOD logical identity `40e9fb6ce63d1551611e2ed365934a4dfa56c616be1393e249a4a550fdfdeccc`. The original one-Studio-system architecture and separate vanilla/module/AEVPSV1 persistence authorities remain unchanged; this does not claim physical persistence or audio testing.

The 24 retained public packages plus the separate Chrono package were rehashed. Strawberry Jam remains 1.0.12; no live/latest helpers were substituted. The wrapper’s mandatory preparation/preflight regenerated the actual closures using existing locks. Public package identities are listed at the end of this report.

**Native foundations: validated VM reuse.**

| Foundation / component | Accepted logical identity, revalidated |
| --- | --- |
| iOS complete native set | 9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2 |
| iOS Theorafile | f24cdd931a5ae337d681075d6f093afc7ca303c56652d79bc1510276772d8d21 |
| tvOS complete native set | 6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc |

HOST-B VM run 1 archives/XCFrameworks and the required source/staging metadata were copied into HOST-C-owned locations; their actual file bytes and producer/verifier provenance were checked. Complete current native verifiers passed, including archive/member/platform/minimum/architecture checks, arm64 logical exports, every packaged slice’s symbols/stub rules, link probes, unsigned probes, staged/package equality, licenses and exact normalized identities. Existing staging scripts then prepared the app inputs. The copied packages and manifests were rehashed again after app qualification and remain byte-identical. No previously compiled app object, M1 binary or entire `.build` tree was transplanted. No three-pair native stress benchmark was repeated.

HOST-B’s reviewed native evidence covers all 1,295 compiled Mach-O members plus 29 archive metadata members across the two platform sets, not only Theorafile. The old M1 reference ZIP and all 110 transferred metadata files were revalidated; ZIP SHA-256 remains `50271049e4897b3348cac544fc31422503fdcdd3f0418673ac8e54a27f9053f3`. Its provenance is inspection of retained accepted M1 archives, not original compiler logs or new physical acceptance. The exact HOST-B candidate’s newer M1 regression PASS is user-reported reviewed external evidence, not a local rerun. No M1 binaries were inputs to HOST-C.

**Each final product’s fresh closure.**

| Logical authority | iOS and tvOS exact result |
| --- | --- |
| sharedClosureSha256 | 11e5006c72dde385ca3b41773e3d924c29e7b19979aef96e11a5ad44f211d6b9 |
| managedLogicalSha256 | 906787e52b5d8bc84ab68195532678019beb77947cb7713486af7e1388cffb1f |
| contentLogicalSha256 | 3ee3129bf48f841786482f9bc88311bb58b3c8cbef3f4550d7a8fe2f1064b8ca |
| registrySha256 | 6e5b89f7d952aa98e72640abce0c75522567fb9d48f8b027e3db54f5cf9da72f |

| Readiness gate | Both actual final closures |
| --- | --- |
| A | 920/920 occurrences accepted or vanilla; 0 blocked, 0 unclassified |
| B | 73/73 actual compiled production registrations; 0 missing/unavailable/provider-rejected |
| C | 73/73 closed semantic closures; 0 blocked, 0 unknown |
| D | Real composition PASS; 0 blocked, 0 unknown |

The marker, compiled production proof, all 73 real selector/profile guards, six fresh production DLL comparisons, semantic/composition receipts, actual managed/content bytes and asset manifests were revalidated before AOT and after each wrapper. Both products preserve the two unchanged original SJ maps, exclude the other 126 SJ maps, and retain the accepted regression content. The exact custom bank inventory/load order and audio decisions match the retained accepted closure. No expected identity was edited.

| Original SJ map | SHA-256 |
| --- | --- |
| Beginner lobby (`0-Lobbies/1-Beginner.bin`) | a4e3e20a2f0cc878fe43b32fb8025d7650b20cc6265f69e37bf3110a7cdf47c2 |
| `1-Beginner/Bing_Over_Google.bin` | e770a8d193f217d09a6e153fbe272813d26a04d972df947ac812aa5cfe66f347 |

HOST-A’s three independent closure reproductions remain historical evidence and were preserved. HOST-C did not relabel them as new runs: it freshly validated the actual closure used by each final product, plus its separate preparation/review closure.

**Fresh products and exact artifacts.**

| Check | iOS | tvOS |
| --- | --- | --- |
| Final K-L wrapper exit | 0 — PASS | 0 — PASS |
| Device contract | arm64; universal family [1,2]; minimum iOS 15.0 | arm64; family [3]; minimum tvOS 16.0 |
| Compile SDK / Xcode build | iphoneos26.5 / 17F113 | appletvos26.5 / 17F113 |
| Execution policy | Full trimming; full/static LLVM device AOT; interpreter=false; no JIT | Full trimming; full/static LLVM device AOT; interpreter=false; no JIT |
| Signing status | UNSIGNED_ABSENCE_VERIFIED | UNSIGNED_ABSENCE_VERIFIED |
| Packaged managed assemblies with actual AOT evidence | 43 | 41 |
| App/IPA exact matching payload files | 4254 | 4248 |
| Final independent verification | PASS | PASS |
| IPA bytes | 917355444 | 931548050 |

ios IPA SHA-256: `b8694395ab3d27483e47778cf8784f52ddf95b7419dd592c8d542ee02fd0f051`.

ios packaged native executable SHA-256: `82d82872575063174da62161feb1cefee568d94beea918caf8885b47a2feb2a9`.

tvos IPA SHA-256: `b13f041feae5c68760e0607de113d309645e9d6e755aae53ce480f1577ba39ab`.

tvos packaged native executable SHA-256: `7256a95d19ec1a614957748868c420398560987ed3d9e3386a9c14c2548e9da1`.

Every final IPA has one app and an exact duplicate-free file inventory/byte match with the staged payload. Actual linked assemblies, Mono/LLVM objects, AOT data, selected factory/profile implementations, native code/data/MVID bindings, external assemblies and referenced APIs were checked. All packaged managed assemblies have actual full/static LLVM SDK items, objects and matching packaged AOT data. Native architecture is independently arm64; required FNA3D/FMOD exports and iOS UIKit entry point are present. The actual native registration source requires full AOT mode; forbidden JIT/interpreter and host-only transformation surfaces are absent. The applicable unchanged I-B package checks pass for the actual unsigned IPAs.

Historical aggregate verifiers that bind old signed IPA digests, old stage source refs or physical acceptance were not repurposed as HOST-C gates. Their current linked/product/content/package checks were used where applicable. No historical signed-IPA digest equality or transferred physical PASS is claimed, and no physical/signing verifier was weakened.

**Timings, caches and resources.**

All measured command phases below use `/usr/bin/time -lp` with the real exit status retained directly; there was no tee pipeline to mask an exit. The final serial settings were `MSBUILDDISABLENODEREUSE=1`, `DOTNET_CLI_USE_MSBUILD_SERVER=0`, `UseSharedCompilation=false`, `-m:1`, and `BuildInParallel=false`. No `Malloc*` environment controls were present in either final app command. SDK-internal behavior was left unchanged; no worker tuning or simultaneous app builds occurred.

| Measured command phase (all exit 0) | Wall s | User s | System s | Reported RSS MiB | Free disk GiB before → after |
| --- | --- | --- | --- | --- | --- |
| iOS .NET SDK detection | 0.89 | 0.39 | 0.16 | 81.8 | 1112.742 → 1112.742 |
| tvOS .NET SDK detection | 0.48 | 0.34 | 0.09 | 81.5 | 1112.738 → 1112.737 |
| iOS reused-native verification | 4.25 | 7.56 | 7.87 | 112.0 | 1112.742 → 1112.738 |
| tvOS reused-native verification | 6.78 | 11.58 | 11.72 | 143.9 | 1112.737 → 1112.713 |
| iOS supported native staging | 4.87 | 1.84 | 1.24 | 341.1 | 1112.713 → 1112.661 |
| tvOS supported native staging | 0.32 | 0.12 | 0.15 | 13.0 | 1112.661 → 1112.584 |
| Lawful Celeste validation | 2.98 | 1.72 | 0.45 | 49.6 | 1112.583 → 1112.584 |
| iOS existing FMOD preparation | 5.45 | 0.92 | 0.51 | 75.7 | 1112.584 → 1112.548 |
| tvOS existing FMOD preparation | 8.82 | 4.62 | 1.74 | 78.0 | 1112.548 → 1111.893 |
| Pinned host-tool restore | 0.16 | 0.09 | 0.03 | 41.2 | 1111.893 → 1111.893 |
| iOS canonical runtime preparation | 53.90 | 29.59 | 61.79 | 847.6 | 1111.893 → 1111.830 |
| tvOS canonical runtime preparation | 48.33 | 25.33 | 60.02 | 523.3 | 1111.830 → 1111.408 |
| K-L preparation/review closure + gates | 229.82 | 148.84 | 42.20 | 2868.9 | 1111.770 → 1111.391 |
| Existing tvOS artwork preparation | 13.88 | 2.28 | 1.25 | 392.9 | 1109.660 → 1109.614 |
| FINAL iOS wrapper: preparation/AOT/link/package/checks | 479.05 | 605.22 | 82.49 | 4659.4 | 1101.739 → 1095.756 |
| FINAL tvOS wrapper: preparation/AOT/link/package/checks | 460.30 | 578.86 | 76.21 | 4771.9 | 1095.756 → 1090.777 |
| FINAL iOS independent artifact/negative checks | 22.82 | 19.93 | 3.54 | 592.6 | 1089.923 → 1089.922 |
| FINAL tvOS independent artifact/negative checks | 23.63 | 21.00 | 3.65 | 605.8 | 1089.921 → 1089.917 |

Native preparation is validated reuse, not a fresh native-build timing. The two final wrapper commands total 939.35 seconds (15m39.35s); this is not total HOST-C elapsed time. Their closure generation, managed preparation/trimming, AOT, linking, SDK packaging, wrapper validation and final IPA packaging are included. Independent final checks add 46.45 seconds. Preparation, tests, failed attempts, evidence hashing, cache copies and agent work are separate.

| Observed final wrapper subinterval (wall seconds) | iOS | tvOS |
| --- | --- | --- |
| Wrapper start → closure/real-composition ready marker | 152.22 | 146.30 |
| Ready marker → before-AOT receipt (remaining preparation/managed build/trim) | 57.19 | 52.65 |
| Before-AOT → after-AOT receipt | 173.82 | 168.50 |
| After-AOT → after-native-link receipt | 2.99 | 2.39 |
| After-native-link → wrapper end (combined packaging/verification) | 92.95 | 90.58 |

These subintervals come from actual receipt mtimes and marker observations; they include boundary hash/tool observations and polling overhead. Marker timestamps have about one-second granularity. CPU/RSS were not independently timed for those subintervals. Packaging and wrapper verification cannot be honestly split further from the retained command structure.

| Memory observation | iOS final wrapper | tvOS final wrapper |
| --- | --- | --- |
| Reported command maximum RSS (GiB) | 4.550 | 4.660 |
| Peak sampled sum of scoped process RSS (MiB) | 5832.1 | 6011.4 |
| Scoped 5-second checkpoints | 93 | 91 |
| VM free pages, MiB before → after | 81.4 → 1038.7 | 917.0 → 1123.2 |
| VM wired pages, MiB before → after | 3818.0 → 3793.4 | 3809.1 → 3815.2 |
| VM compressor occupancy, MiB before → after | 655.5 → 576.3 | 576.3 → 875.6 |
| Swap used / swap-in / swap-out observations | 0 / 0 / 0 | 0 / 0 / 0 |

All measured phase endpoints and final sampled checkpoints show zero swap used. Command RSS is not whole-system memory or an aggregate of every build service. The sampled process sum may double-count shared pages and omit services whose command line lacks the owned root. VM free/active/inactive/wired/compressed categories include file-cache effects and must not be equated with the command’s working set. No monitoring overhead was subtracted; these are measured observations, not a claim about an otherwise idle VM’s peak total memory.

The first measured working-volume free space was 1,112.742 GiB after cache staging, comfortably above the requested practical working-space target. Final wrapper free space was 1,101.739 → 1,095.756 GiB for iOS and 1,095.756 → 1,090.777 GiB for tvOS; final verification ended at about 1,089.917 GiB. No broad cleanup or deletion of accepted inputs/artifacts was performed. SDK 8/9/10 and validated host/NuGet/source/download caches were reused in owned locations; app/closure output roots were fresh. Existing public ZIPs were reused after hashing. Initial checkout/submodule and pinned source preparation acquisitions were outside the final warmed-cache app timing. NuGet project restores are recorded in the logs; network transfer bytes were not separately instrumented. No cold-download or M1 speedup claim is made.

**Retained failures and limitations.**

| Preserved attempt / diagnostic | Exit | Wall s | User s | System s | RSS MiB |
| --- | --- | --- | --- | --- | --- |
| 82e7249 iOS: rejected SDK `iOS` spelling before AOT | 1 | 193.67 | 210.09 | 42.26 | 4711.0 |
| 82e7249 tvOS: missing owned artwork before AOT | 1 | 175.20 | 211.69 | 41.12 | 4551.4 |
| 567f00a iOS: wrapper PASS, superseded to retain a same-final-revision pair | 0 | 476.82 | 602.90 | 78.60 | 4668.7 |
| 567f00a tvOS: AOT/link/SDK IPA complete; empty entitlement file rejected | 1 | 428.04 | 565.45 | 73.81 | 4837.3 |
| Auxiliary artifact audit: internal symbols requested from stripped package | 1 | 23.39 | 18.51 | 4.04 | 594.1 |

The first iOS failure required the tested CLI casing correction. The first tvOS failure required generating the missing artwork through the existing owned preparation script. The second tvOS failure required the bounded unsigned-caller correction described above. Both final apps were then rebuilt freshly from the same committed `b4997fa`; the earlier iOS PASS was not relabelled.

The auxiliary final-audit failure was its own unsupported expectation that internal AOT symbol names remain in a stripped package. Both symbols are defined data in the actual native link image, which the unchanged repository verifier binds to packaged code/data and AOT identities. The auxiliary audit now inspects that correct linked image. The original audit/log/output is retained; retry exit is 0. No compiled output, tracked verifier, expected lock or normalization changed for that diagnostic correction.

An initial builder-test command selected SDK 8 from a directory whose global.json pins SDK 10 (exit 145 before testing); the existing temporary-directory invocation convention fixed the command and all 622 tests passed. Early read-only preservation-helper checks also needed newline handling and distinction between user refs and Codex bookkeeping; a broad private-path scan incorrectly flagged the existing generic `/private/tmp` path. These were audit-harness issues, not product or acceptance changes. Known compile/trim analysis warnings remain in private logs; current linked/runtime/forbidden-surface gates pass and no game fixes were introduced.

**Preservation and remote safety.**

The retained HOST-A/HOST-B inventory rehash matches all 163,263 files / 14,458,964,239 bytes, including hashes and recorded metadata. HOST-C’s superseded attempts retain 24,132 and 42,165 unchanged files. Original HOST-A remains at `be8546d4` on its original branch; HOST-B and the vanilla checkout remain at their original revisions/branches. All tracked files and all eight pinned recursive submodules are clean. The original HOST-A historical HOST_YELLOW report remains unchanged.

Codex automatically rotated two internal `refs/codex/turn-diffs/…` bookkeeping refs in the original HOST-A repository during the task. Their exact before/after inventory is retained privately. User branches/tags/remotes, HEAD, tracked source, submodules and preserved evidence were unchanged; those internal rotations are not represented as “all local refs unchanged.” No agent Git command changed the internal refs or reset them.

| Protected remote ref | Verified before and after |
| --- | --- |
| `ios-v0.1.1-rc.1^{}` | 27e16b4724d94d3991b99c4795f680fcb0e5830c |
| `v1.0.0-rc.1^{}` | ee52b0868df091746f134d95d4f020f94f23d4fb |
| `v1.0.0-rc.2^{}` | 641e86e4ed164cdf93f602ce2f11436449654d6e |
| `release/v1.0.0-rc.3` | c8134c8ca7924cf12f48527e714b5242c6024927 |
| `v1.0.0-rc.3` tag | ABSENT |

The new feature branch is the only remote-ref difference. Push followed a privacy review, explicit destination check, absence of executable hooks/workflow files, GitHub workflow/run checks and a dry run. Only the feature branch was pushed, with follow-tags and recursive submodule pushes disabled. No force-push, tag, release, PR, merge, upstream push, integration or GitHub Actions was performed. Workflow and Actions run counts remain zero. Repository settings and global Git configuration were not changed; rejected K-G/K-I/K-K commit history was not imported.

**Exact retained public package identities.**

| Package | Version | ZIP SHA-256 |
| --- | --- | --- |
| BrokemiaHelper | 1.8.5 | c80d7d71cdccef7c9b418e05d53debd197b59717d3cdeade72910ace33d4fbeb |
| CherryHelper | 1.8.2 | 8ca640f9e14f844507e0bbedc2afcb99ef61bec6957a7558cee90e9fabd2fba7 |
| CollabUtils2 | 1.13.4 | 4bcea8a9011edb8b7d27b433c47f1f4a871b8f7a4dd7f2bffac67bb99c7f6ad5 |
| ContortHelper | 1.5.5 | d8b42128a808e68d30329baa9299fdb41bf7d24743635dec3137c36bcc87956a |
| CrystallineHelper | 1.17.2 | 4573f5e45dce0905142cd2b119f4a9a744bdce8ef3199319d1e46b6d1d342747 |
| DJMapHelper | 1.13.4 | 95ab02d657213031b70c3079738b21be399e567ca8a32b1fe8e8effc0778eedb |
| ExtendedVariantMode | 0.50.5 | 4019b362d9ad1b2d3a6a670f833ef6324d735d5791a0bf21b62cf5cfbc0d639d |
| FancyTileEntities | 1.6.2 | c37806dc7e7db4a392c8ab79030701f4cf2046109cc19435b607fca7fe41c8d8 |
| FemtoHelper | 1.15.22 | 5a845805bae30490ed7c9da84410fbe672628b8fe927a830f5f2eb148eb88419 |
| FlaglinesAndSuch | 1.6.80 | fb0fd300f95d77539eb9079aed445c81f263557c1a1715187bd509d39c1eb794 |
| FrostHelper | 1.80.1 | ba6aee8f596eff0f595fd90cb7f136ea13b1e9c2d664fc38f389608b0f9e2c81 |
| HonlyHelper | 1.7.5 | 6a2d0f04a5be3a9c9c3bf66ec7e93701398a64d5a0e72add5df682e860e7d08d |
| JungleHelper | 1.4.10 | a140e21cbb5fd2dcaac70d4d5e36d49476e414861455ae25dedc0678164406cc |
| LunaticHelper | 1.1.1 | b10e044b1dfa412605bee3ba6bfdd4263591099d331d6c1aab1e9ba65bf6a3e5 |
| MaxHelpingHand | 1.40.9 | abfc5d167936410e9b6665018335ee654455e30710caffe01540b232da468fee |
| PandorasBox | 1.0.49 | 25f9c7d6792983ac697e3780667fb0963233c0c08f6e528f5c6d9fe732cded05 |
| StrawberryJam2021 | 1.0.12 | 4e1a2fc12baa3db27da433b93bf26b59f34b3e6b7f760c41d8d127636d020655 |
| StrawberryJam2021Assets | 1.0.1 | 26fab85f20fff89d1adcba2ef926c3447d9996057c7dfc78e5d97f9d96ed7e45 |
| StrawberryJam2021AudioA | 1.0.4 | 81e9cbc39b3a5525c93dfc5b24b675a833a8fb616e8a01cfc0838296f5e37e1d |
| StrawberryJam2021AudioB | 1.0.0 | 70b90f45709956a4d18bfbb5941836c534344a1cf0ab859a50430daa3b76ef42 |
| VivHelper | 1.14.10 | 9a95f51659ccfcb78404f9610307918d6114fc824298fbb8493651ec7c80215a |
| VortexHelper | 1.2.19 | b6280fe2e3c05d355c32a51049854137c41be72d24ec4aec9d997cc7c4394db2 |
| XaphanHelper | 1.0.79 | ca868d06eb05f0c5de55126090e5019210e9d83c3dae9394cbcd155acf8eb16f |
| YetAnotherHelper | 1.2.5 | 73d64e1b3457e2461d3368a01de9f5f31a58bf24e130f3e35ec7280b24814c2d |
| ChronoHelper | 1.3.3 | af46039437fbed52e72941d07e0d9ce6657dacc838516459f7e9e995fea07a18 |

**Handoff.** No unresolved blocker remains for Intel unsigned full-app build qualification. The feature is ready for further integration review, with the signed/M1-host fixtures and external-evidence limits above. Recommended VM role: a pinned, serial unsigned full-AOT build host after review of this feature. The M1 remains the signing/install fallback.

Signing, credentials/provisioning, installation, device operations, physical/iPad testing, worker tuning, Strawberry Jam expansion and Stage 25K-M: **NOT RUN**. No next stage was started.

**Exact next recommended action:** review `feature/apple-everest-host-c-aot-qualification` at `b4997fa49f4f8b82f6f2984fe9047396e401ea48` for the user’s normal manual-fast-forward workflow. Do not integrate automatically.

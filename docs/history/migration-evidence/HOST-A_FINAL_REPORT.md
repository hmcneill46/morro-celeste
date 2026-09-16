**HOST-A_FINAL_REPORT — HOST_YELLOW**

Qualification date: 2026-09-08. Evidence cutoff: 2026-09-08T21:53:33.955409+00:00. Repository: `hmcneill46/celeste-ios`. Source: **`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`**, branch `feature/apple-everest-host-a-build46`, version **0.1.1 (46)**.

Exact build-46 pre-AOT equivalence is proven on this Intel VM. The unchanged K-L wrapper passed preparation, and the existing reproduction machinery passed three independent complete closures, three fresh production compilations, and all four readiness gates. **Neither fresh unsigned device product was built.** The installed Xcode 26.5 is rejected by the exact pinned iOS and tvOS workloads, which require Xcode 26.6. The heavy-build benchmark therefore remains incomplete. This is a setup/evidence result; it does not demonstrate an inability of Intel hardware to produce correct products.

**Source and instruction handling**

The supplied workspace was initially an empty Git repository with no commit or remote. The existing project checkout was clean at the exact requested SHA on `tvos-port`. Its source and configuration were inspected read-only. Before any baseline build, the empty workspace was populated at the exact SHA on the requested HOST-A branch, followed by fresh recursive public submodules. No tracked source, documentation, version, manifest, build setting, or lock was edited. HEAD remained at the exact SHA after baseline establishment.

Applicable ancestor/repository `AGENTS.md` locations were checked; none were found. The current static-AOT and compatibility documents, K-L outcome report, K-L wrapper, bootstrap, reproduction, verification, and relevant source/toolchain/input locks were read. The two supplied Intel/build-document audit reports were used as historical evidence. Their older toolchain, modified vanilla-build configuration, and outcomes were not treated as instructions or transferred to this baseline. No prior modified vanilla-build tree or old product was used.

The current public `tvos-port` ref was independently re-read and resolves to the exact build-46 SHA. All refs in the existing checkout and all pre-existing public release branches/tags remained unchanged. The RC3 tag remains absent. Diagnostic K-G, K-I and K-K commits are absent from the qualification repository's object database and ancestry. No old M1 `.build` tree, closure, or PASS receipt was copied.

**Actual host and toolchain**

| Item | Observed | Reconciliation |
| --- | --- | --- |
| Guest architecture | x86_64 | Native Intel guest execution |
| macOS | 26.6.2, build 25G83; Darwin 25.6.0 | Newer than the legacy doctor's literal macOS 26.3 contract |
| CPU exposed to guest | Intel Core Processor (Skylake); 8 physical / 16 logical CPUs; reported 3.0 GHz | Underlying i9-13900K P-core allocation is user-supplied information, not proven by guest CPUID |
| RAM | 48 GiB (51,539,607,552 bytes) | Matches supplied capacity |
| Free working volume space | 1177.83 GiB initially; 1166.27 GiB at final audit | Far above the 60–80 GiB practical target; no broad cleanup |
| Swap | 0 MiB allocated/used at every captured checkpoint | No swap activity observed in captured VM counters |
| Installed full Xcode | **26.5, build 17F42** | **Mismatch: required 26.6, build 17F113** |
| Device SDKs | iOS 26.5 and tvOS 26.5 | SDK versions match accepted native locks; Xcode application version still fails the workload requirement |
| Default developer selection | Command Line Tools | Full Xcode was selected only through the process environment; global selection was not changed |
| Xcode first-launch check | Existing Xcode returned success | Does not resolve the version mismatch |
| Main .NET SDK | **10.0.302 x64**, runtime 10.0.10 | Exact repository SDK pin installed locally |
| Workload set | **10.0.302.0** | Exact accepted set; iOS/tvOS manifests 26.5.10301/10.0.100 and osx-x64 cross-arm64 AOT packs installed |
| Bootstrap SDKs | **8.0.424**, **9.0.317**, x64 | Exact existing bootstrap pins |
| Reference decompiler | ILSpy **8.0.0.7246-preview3**, .NET runtime 6.0.36 x64 | Exact repository tool restored; locked package SHA verified |
| Auxiliary tools | GNU make 4.4.1; Mono 6.14.1; Python 3.14.7 | Installed host prerequisites; Python changed from initial CLT 3.9 via the Mono dependency installation. No project Python pin was changed |

Both unchanged Microsoft `_DetectSdkLocations` targets were invoked for device arm64, signing disabled, serial MSBuild settings. Each exited 1 with the actual error that .NET iOS/tvOS 26.5.10301 requires Xcode 26.6 while the selected Xcode is 26.5. Microsoft's [pinned workload release](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode26.6-10301) corroborates the requirement. No Xcode check, SDK version check, or project pin was bypassed or altered.

The unchanged general `check-ios-host.sh` also exited 1 because it requires an Apple-silicon arm64 host; its later literal macOS 26.3 check was not reached. The K-L wrapper does not invoke this general doctor. Managed preparation succeeding does not make that doctor pass.

**Inputs and retained content**

The user-supplied Celeste application and external FMOD SDK were found in the explicitly supplied required-files directory. Authoritative input files were left unchanged; FMOD was mounted read-only. No private input remains missing.

Celeste validation passed the unchanged `itch-macos-fna-1.4.0.0` profile, canonical class `celeste-1.4.0.0-a`, game 1.4.0.0/FNA 21.3.5, and zero Everest markers. The content set has **1,216 files / 1,158,665,183 bytes**, SHA-256 `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`.

| Canonical managed input | SHA-256 |
| --- | --- |
| Celeste.exe | `fd73f8a2311fa5737ded550cbad4b75c85b7686b36432f59185e940fcb65fcfe` |
| FNA.dll | `00349b572636c0ed4c97d4e4f74344b3450160c42d4a6b52bcf5e0f2d5193a31` |
| Celeste.Content.dll | `0b8d6195992c8970e8602bf48a7bf89104b83a98e34a07dc0c18d01581711d15` |

FMOD **1.10.09, build 97915** passed the current iOS input validator, including low-level archive SHA `2b374ed776a782fcf5ded58868360c98a6aa0b7301043ba30647885c4f0a4c81` and Studio archive SHA `e8e0d0d9f33c986785ad096c73e57541d1ecd1aab4f0dfd34b75ca947d025573`. The tvOS input validator passed all-member arm64/TVOS platform and minimum-version checks. These are external input checks, not newly built native-product acceptance.

Everest was independently acquired at **`4bbde91b8dbaaddef2ceec75ca0cd6d59b3b8d00`**, with MonoMod **`dfc30a1506d37fb88a2c2be004f525205f46a24c`**. The accepted source/profile and all native/managed/content/tool locks were retained. Five FNA managed bindings were freshly staged from locked public sources with the existing patches and source hashes. Canonical Celeste source was freshly generated with the unchanged preparation script.

The existing fixed-version acquisition script downloaded 24 exact public ZIPs; ChronoHelper was independently acquired from its exact pinned public file. All **25** archives were rehashed, all specified distributed DLL hashes were checked, and their total retained ZIP bytes are **872,346,570 (0.812 GiB)**. No live/latest helper was substituted.

| Package | Accepted version | Validation |
| --- | --- | --- |
| BrokemiaHelper | 1.8.5 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| CherryHelper | 1.8.2 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| CollabUtils2 | 1.13.4 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| ContortHelper | 1.5.5 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| CrystallineHelper | 1.17.2 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| DJMapHelper | 1.13.4 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| ExtendedVariantMode | 0.50.5 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| FancyTileEntities | 1.6.2 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| FemtoHelper | 1.15.22 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| FlaglinesAndSuch | 1.6.80 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| FrostHelper | 1.80.1 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| HonlyHelper | 1.7.5 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| JungleHelper | 1.4.10 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| LunaticHelper | 1.1.1 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| MaxHelpingHand | 1.40.9 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| PandorasBox | 1.0.49 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| StrawberryJam2021 | 1.0.12 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| StrawberryJam2021Assets | 1.0.1 | ZIP size/SHA-256 PASS; content package |
| StrawberryJam2021AudioA | 1.0.4 | ZIP size/SHA-256 PASS; content package |
| StrawberryJam2021AudioB | 1.0.0 | ZIP size/SHA-256 PASS; content package |
| VivHelper | 1.14.10 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| VortexHelper | 1.2.19 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| XaphanHelper | 1.0.79 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| YetAnotherHelper | 1.2.5 | ZIP size/SHA-256 and distributed DLL SHA-256 PASS |
| ChronoHelper | 1.3.3 | Independently downloaded exact pinned ZIP and bank bytes PASS |

The original SJ 1.0.12 ZIP SHA is `4e1a2fc12baa3db27da433b93bf26b59f34b3e6b7f760c41d8d127636d020655`; its root DLL SHA is `8d5b9184204e7e7728965bcf95af5f150f6220dafbfe52fdd6c23e50d47e5258`. The regenerated selection plan is byte-identical to the tracked authority, SHA `3b520b9426c239f089d608f044c6d7c5d490570464986a7c04531b2a1954ba59`.

Exactly the unchanged **Beginner lobby** and **Bing_Over_Google** are mounted. The other **126 original SJ maps are excluded**, while accepted regression content is retained: 20 total selected/regression maps (2 original + 18 owned regression maps), 2,943 mounted files, and 158 managed files. Selection contains 1,398 public files plus two pinned core assets. Complete manifests, semantic decisions and content inventories are identical across the three runs.

| Original map | Bytes | SHA-256 |
| --- | ---: | --- |
| Beginner lobby | 674,648 | `a4e3e20a2f0cc878fe43b32fb8025d7650b20cc6265f69e37bf3110a7cdf47c2` |
| Bing_Over_Google | 135,363 | `e770a8d193f217d09a6e153fbe272813d26a04d972df947ac812aa5cfe66f347` |

The final retained map bytes were rehashed. All seven selected custom bank bytes and their generated load order were verified: vanilla banks 1–7, ChronoHelper 8, Bing 9, shared SJ audio 10, Beginner lobby 11, jamjars 12, CollabUtils2 global collectibles 13, HonlyHelper 14. The existing policy loads banks into Celeste's one Studio system. Separate vanilla, module, and AEVPSV1 persistence authorities remain unchanged and are covered by source/mechanism checks; actual device playback/persistence was not tested.

**Exact logical equivalence and readiness**

| Identity | Required and observed SHA-256 | Result |
| --- | --- | --- |
| Content logical | `3ee3129bf48f841786482f9bc88311bb58b3c8cbef3f4550d7a8fe2f1064b8ca` | EXACT MATCH, preparation + 3/3 runs |
| Managed logical | `906787e52b5d8bc84ab68195532678019beb77947cb7713486af7e1388cffb1f` | EXACT MATCH, preparation + 3/3 runs |
| Factory registry | `6e5b89f7d952aa98e72640abce0c75522567fb9d48f8b027e3db54f5cf9da72f` | EXACT MATCH, preparation + 3/3 runs |
| Shared closure | `11e5006c72dde385ca3b41773e3d924c29e7b19979aef96e11a5ad44f211d6b9` | EXACT MATCH, preparation + 3/3 runs |

| Gate | Fresh evidence | Result |
| --- | --- | --- |
| A — content occurrences | 920/920 accepted or vanilla; blocked 0; unclassified 0 | PASS |
| B — actual production registrations | 73/73 compiled registrations; unavailable 0; rejected providers 0 | PASS |
| C — semantic closure | 73/73 closed; blocked 0; unknown 0 | PASS |
| D — real composition | Actual composition; blocked 0; unknown 0 | PASS |

The unchanged `build-apple-everest-stage25kl.py --prepare-only` completed successfully using a fresh ignored retry root. Gate B compared six freshly compiled production DLLs against the inspected implementation, excluding only the existing COFF timestamp and MVID build fields. All 73 actual selectors and profile guards ran, including negative controls. Gate C binds the accepted lifecycle/source ledger to current compiled guards and verified K-L deltas; its physical-product field remains pending.

The unchanged `reproduce-apple-everest-stage25kl.py` then completed **three fresh generations and three fresh production compilations**. Its complete closure inventories contain **3,123 files**. All three complete snapshots have SHA-256 **`29a42b880cf38c6e869121b9e4e5c45501e6d05fb74ce27a11afc8fc7c1c6990`**. Snapshots include all four gates, providers, manifests, complete implementation comparison, and pinned composition references. The retained preparation tree was independently rehashed and equals those snapshots. Reproduction removed only its own marked temporary trees using its existing cleanup; snapshots and logs remain private.

Each run passed 89,347 autotiler assertions, including 26 actual J cells, and 290 runtime-composition assertions with three graphics cycles. These are fresh host proofs with documented host adapters. They are not transferred physical results or substitutes for device AOT.

**Timings, memory, disk and cache state**

Instrumented command rows use `/usr/bin/time -lp`; real/user/system are seconds, RSS is the reported maximum converted from bytes to MiB. The ChronoHelper download has separately recorded monotonic wall time only. The timing harness writes directly to private logs and preserves the real command exit status, without a `tee` pipeline. RSS is the command's reported figure, not a continuously sampled whole-VM or concurrent process-tree peak.

| Phase | Exit | Real s | User s | System s | Reported RSS MiB | Free disk GiB before → after |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Exact source acquisition | 0 | 0.16 | 0.19 | 0.08 | 26.7 | 1176.46 → 1176.48 |
| Fresh recursive submodules | 0 | 47.01 | 12.23 | 2.54 | 571.2 | 1176.46 → 1176.28 |
| Pinned .NET 8/9 bootstrap | 0 | 871.90 | 3.40 | 6.52 | 20.1 | 1176.36 → 1168.82 |
| 24 pinned public package downloads | 0 | 308.30 | 3.62 | 1.95 | 104.2 | 1176.24 → 1174.44 |
| GNU make/Mono installation (postinstall failed) | 1 | 226.61 | 517.17 | 201.26 | 522.8 | 1175.98 → 1174.54 |
| Read-only FMOD mount | 0 | 9.58 | 0.00 | 0.01 | 4.2 | 1175.97 → 1175.94 |
| Exact .NET 10 installation | 0 | 179.16 | 1.51 | 3.05 | 19.6 | 1175.35 → 1173.93 |
| FMOD iOS input validation | 0 | 1.73 | 0.18 | 0.06 | 41.8 | 1175.03 → 1175.03 |
| FMOD tvOS input validation | 0 | 3.30 | 0.26 | 0.17 | 84.7 | 1175.03 → 1175.01 |
| Pinned iOS/tvOS workload installation | 0 | 497.37 | 26.91 | 13.91 | 127.6 | 1173.47 → 1168.37 |
| Supplementary input audit (path assumption failed) | 1 | 0.08 | 0.06 | 0.01 | 29.1 | 1173.43 → 1173.43 |
| Canonical Celeste input validation | 0 | 2.91 | 1.77 | 0.26 | 54.3 | 1173.34 → 1173.34 |
| OpenSSL postinstall repair | 0 | 1.03 | 0.95 | 0.30 | 96.9 | 1173.34 → 1173.34 |
| Supplementary public ZIP/DLL rehash | 0 | 1.55 | 1.44 | 0.11 | 70.8 | 1173.11 → 1173.11 |
| Fresh locked FNA managed binding staging | 0 | 0.12 | 0.05 | 0.06 | 15.4 | 1172.71 → 1172.70 |
| Pinned Everest/MonoMod acquisition | 0 | 76.12 | 19.92 | 4.27 | 354.7 | 1172.28 → 1171.90 |
| Unchanged host doctor (architecture rejection) | 1 | 0.00 | 0.00 | 0.00 | 1.1 | 1172.09 → 1172.09 |
| Exact selected-content plan regeneration | 0 | 4.92 | 4.60 | 0.32 | 138.0 | 1170.57 → 1170.31 |
| Pinned ILSpy runtime prerequisite | 0 | 30.67 | 0.33 | 0.37 | 11.4 | 1168.37 → 1168.30 |
| Canonical Celeste decompilation/generation | 0 | 55.36 | 28.91 | 60.42 | 817.8 | 1168.30 → 1168.24 |
| iOS SDK detection (Xcode rejection) | 1 | 0.91 | 0.44 | 0.16 | 81.6 | 1168.28 → 1168.28 |
| tvOS SDK detection (Xcode rejection) | 1 | 0.53 | 0.41 | 0.11 | 81.5 | 1168.28 → 1168.28 |
| K-L prepare-only, initial attempt (tool restore missing) | 1 | 279.84 | 132.89 | 40.11 | 3008.9 | 1168.24 → 1166.47 |
| Exact ILSpy local tool registration | 0 | 1.86 | 0.55 | 0.08 | 77.0 | 1166.48 → 1166.47 |
| K-L prepare-only, fresh successful retry | 0 | 143.01 | 76.91 | 33.63 | 244.8 | 1166.47 → 1166.32 |
| Three fresh closures/compiler proofs/four gates | 0 | 265.40 | 142.18 | 74.21 | 244.9 | 1166.32 → 1166.31 |
| K-L regression driver (local release ref missing) | 1 | 69.26 | 80.69 | 12.29 | 736.1 | 1166.31 → 1166.27 |
| Fresh K-E/K-C source checks with actual protected refs | 0 | 0.90 | 0.52 | 0.27 | 38.6 | 1166.27 → 1166.27 |
| Fresh I-B source/FMOD contract checks | 0 | 0.18 | 0.11 | 0.06 | 19.9 | 1166.27 → 1166.27 |
| Device scanner applied to untrimmed assembly (rejected) | 1 | 0.33 | 0.28 | 0.04 | 103.6 | 1166.27 → 1166.27 |
| Supplementary final audit (path assumption failed) | 1 | 2.43 | 1.00 | 0.76 | 63.3 | 1166.26 → 1166.28 |
| Final equivalence/source/ref/map/audio audit | 0 | 4.46 | 1.64 | 1.36 | 61.8 | 1166.27 → 1166.27 |
| Exact ChronoHelper download | 0 | 29.374 | unmeasured | unmeasured | unmeasured | not separately sampled |
| Fresh unsigned iOS full-AOT + packaging | NOT RUN | unmeasured | unmeasured | unmeasured | unmeasured | blocked by Xcode setup |
| Fresh unsigned tvOS full-AOT + packaging | NOT RUN | unmeasured | unmeasured | unmeasured | unmeasured | blocked by Xcode setup |
| Device product/native/linked forbidden-surface verification | NOT RUN | unmeasured | unmeasured | unmeasured | unmeasured | no fresh device products |

Bootstrap downloads and independent prerequisite acquisition overlapped. These setup rows must not be summed into a serial benchmark total; their disk deltas can include another setup phase. The two K-L preparation timings combine bootstrap rechecks, closure generation/application, managed compilation and composition verification. The three-run timing likewise combines generation, compilation and gates; no unsupported split into separate CPU/AOT phases is claimed. The 45.2-minute observation interval includes inspection and setup and is not an AOT performance result.

The qualification began with no local generated build tree and no .NET SDK available on the inspected host paths. SDKs, workloads, NuGet/tool packages, public mods, and upstream sources were acquired afresh. Subsequent preparation and reproduction used these newly populated host caches; closure outputs and production compiler proofs were fresh for each run. Operating-system file caches were not flushed. Total network traffic, retries and all SDK/workload transfer bytes were not separately metered. No M1 speedup comparison is claimed.

Physical RAM is 48 GiB throughout. All before/after swap checkpoints report 0 MiB used. Captured compressor allocation and swap-in/out counters remained zero. These are checkpoints, not a stress-test peak guarantee.

| VM memory category | Initial GiB | Final audit GiB |
| --- | ---: | ---: |
| Free pages | 17.16 | 14.78 |
| Active pages | 13.17 | 13.08 |
| Inactive pages | 10.44 | 11.44 |
| Speculative pages | 4.01 | 5.38 |
| Wired pages | 3.22 | 3.32 |
| Compressor allocation | 0.00 | 0.00 |

Free disk fell by approximately 11.55 GiB across this observation interval; this is volume-wide change, not an exact artifact-size accounting. No authoritative input or accepted artifact was deleted.

**Regression, platform and failure accounting**

Fresh executed mechanism checks passed: 622 builder tests; 50 typed HookGen semantic tests; desktop HookGen/direct-hook ordering and lifecycle controls; 31 save-manager pairing tests; 66 protocol tests; 57 continuity tests; 21 soft-reload tests; 56 input-profile checks; 11 content/evidence identity rejection controls; and 46 K-D source checks. These are host tests and do not claim device pairing or physical testing.

The unchanged K-L regression driver stopped at K-E because the isolated qualification checkout intentionally has no local release tags. It is recorded as an incomplete driver run, not an aggregate PASS. The unchanged K-E (63 checks, including a fresh K-C 59-check historical source view) and I-B (75 checks) scripts were subsequently run read-only against the existing clean checkout at the same exact SHA with its actual protected refs; they passed. No real refs were moved to satisfy a verifier, and no previous PASS result was copied. Later historical checks in the regression driver were not run. Final HEAD, lock, canonical/native, ignored-output and recursive cleanliness checks passed independently.

| Required device contract | Source configuration | Fresh VM product result |
| --- | --- | --- |
| iOS arm64, universal [1,2], minimum iOS 15 | Required RID and source/project family/minimum confirmed | NOT BUILT / NOT VERIFIED |
| tvOS arm64, family [3], minimum tvOS 16 | Required RID and source/project family/minimum confirmed | NOT BUILT / NOT VERIFIED |
| Full trimming and full device AOT | Existing `PublishTrimmed=true`, `TrimMode=full`, `MtouchLink=Full`, `RunAOTCompilation=true` retained | NOT RUN |
| Interpreter disabled / no JIT | Existing `UseInterpreter=false` and static architecture retained | Final linked/native binary NOT VERIFIED |
| Serial benchmark settings | Node reuse disabled; MSBuild server disabled; shared compilation disabled; `-m:1`; `BuildInParallel=false` retained | No AOT benchmark launched |
| Unsigned experimental identities | Platform-specific local IDs resolved; signing disabled | No signing material used; no device products generated |

The initial prepare-only attempt reached the exact logical identities but failed when the isolated CLI home had not registered the locked ILSpy tool. The exact local tool was restored without modifying its manifest, then an entirely fresh prepare-only root passed. The make/Mono install's OpenSSL postinstall step initially returned nonzero; the prescribed postinstall retry passed. Two supplementary outside-tree audit helpers initially used incorrect archive/output path assumptions; only those helpers were corrected. Expected hashes and repository checks were never changed.

An additional device-runtime scanner invocation against the **untrimmed managed preflight assembly** exited 1 on legacy DetourConfig/LegacyDetourContext compatibility surface. The scanner is invoked on linked assemblies in the real product pipeline. This attempted input was not a linked device product; the result does not establish either a correct final runtime or a device-product defect. It remains recorded as a failure, and no production change was made.

The accepted K-L aggregate verifier also retains pre-integration `tvos-port`/K-J and diagnostic-object assumptions, plus separate signed/physical product requirements. Its clean-clone driver would fetch excluded diagnostic history and use an older branch arrangement, so that driver was not used. Fresh exact source/submodules and the existing three-run reproduction supplied the host evidence instead. No aggregate physical/signing verifier was weakened or represented as passing.

The product factory verifier contains an unconditional `codesign --verify --strict` call and reports a signed-product result. Its behavior for the requested unsigned products remains unresolved until such products exist; no credentials, ad-hoc workaround, or verifier modification was introduced to manufacture a host PASS. Historical signed-IPA digest equality was not imposed across hosts.

**Preservation and disposition**

Final HEAD is **`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`**; tracked files are clean; all **8 recursive submodules** are at their original gitlinks and clean. The existing input checkout and all its refs are unchanged. All recorded lock/control-file digests match their initial values, and canonical/native policy remains unchanged from accepted K-J. Outputs are verified ignored or outside Git. Raw logs, generated game code, mod binaries, banks and private paths remain local and private; this report contains no private absolute paths, experimental bundle IDs, device/team identifiers, credentials, signing material, saves or proprietary bytes.

Push: **NOT RUN**. Merge: **NOT RUN**. Tag creation/change: **NOT RUN**. PR/release: **NOT RUN**. GitHub Actions: **NOT RUN (0 invocations)**. Worker tuning: **NOT RUN**. Concurrent AOT builds: **NOT RUN**. Physical testing/device installation/iPad testing: **NOT RUN**. Stage 25K-M: **NOT RUN**. The prior iPhone/Apple TV acceptance belongs to the accepted historical products only; iPad mini 4 testing remains deliberately deferred.

Recommended VM role now: **exact closure preparation, determinism and host regression worker; candidate heavy AOT worker awaiting qualification**. A heavy-build-host recommendation needs both fresh unsigned full-AOT products and applicable final product/native/forbidden-surface evidence. The M1 remains the signing/install fallback.

**Exact next recommended action:** Install **Xcode 26.6 (17F113)** alongside the existing Xcode, complete its first-launch setup, then resume HOST-A at this unchanged SHA with the exact pinned workloads and process-local developer selection. Do not change source pins or start tuning. This manual Xcode setup was not started; Microsoft's [Xcode requirement guidance](https://learn.microsoft.com/en-us/dotnet/ios/troubleshooting/xcode-requirement) describes the supported version requirement.

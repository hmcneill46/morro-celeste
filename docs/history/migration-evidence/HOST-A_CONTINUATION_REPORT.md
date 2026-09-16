**HOST-A continuation — HOST_YELLOW**

Qualification date: 2026-09-09 (Europe/London). Observation start: 2026-09-08T23:54:44.659407+00:00; final preservation audit: 2026-09-09T00:22:59.160334+00:00. Repository: `hmcneill46/celeste-ios`. Branch: `feature/apple-everest-host-a-build46`. Exact source: **`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`**, accepted Stage 25K-L / **0.1.1 (46)**.

The manual Xcode setup blocker is resolved: **Xcode 26.6 / 17F113 and both unchanged .NET device SDK-detection targets PASS**. The four build-46 logical identities, three independent closure snapshots and all four readiness gates remain exact. Fresh native prerequisite builds completed for both platforms. However, the unchanged iOS native verifier rejected a reproducible Theorafile identity mismatch, and the unchanged tvOS native verifier rejected symbol extraction corrupted by mixed `nm` output. **Neither fresh unsigned full-AOT app was built.** Heavy-build qualification remains incomplete; this evidence does not demonstrate that Intel hardware cannot produce correct products.

The original `HOST-A_FINAL_REPORT.md` is preserved byte-for-byte, SHA-256 **`64eb4df76759bf19ad1f812d6480113bc0593ebff3b4629a90903593239074c6`**. This report records the continuation separately and supersedes only its unresolved Xcode status and next-action recommendation.

**Host and effective toolchain**

| Item | Observed continuation state |
| --- | --- |
| Guest architecture / OS | x86_64; macOS 26.6.2 (25G83), Darwin 25.6.0 |
| Guest CPU | Intel Core Processor (Skylake); 8 physical / 16 logical CPUs. Underlying i9-13900K P-core resources remain user-supplied information, not established by guest CPUID. |
| RAM | 48 GiB / 51,539,607,552 bytes |
| Working volume free space | 1148.43 GiB initially → 1145.76 GiB at final audit; well above the 60–80 GiB target |
| Swap | 0 MiB allocated/used at every captured checkpoint; captured swap-in/out and compressor counters remained zero |
| Effective Xcode | **26.6, build 17F113**; first-launch status exit 0 |
| Process-local selection | `DEVELOPER_DIR=/Applications/Xcode-26.6.app/Contents/Developer` |
| Device SDKs | iPhoneOS 26.5; AppleTVOS 26.5 |
| Simulator SDKs | iPhoneSimulator 26.5; AppleTVSimulator 26.5, existing installations |
| Apple compiler | Apple clang 21.0.0 (`clang-2100.1.1.101`), x86_64 host |
| .NET | SDK 10.0.302 x64, workload set 10.0.302.0, iOS/tvOS manifests 26.5.10301/10.0.100; runtime 10.0.10 |
| Retained bootstrap tools | .NET 8.0.424 and 9.0.317; pinned ILSpy 8.0.0.7246-preview3 and its .NET 6.0.36 runtime |
| Auxiliary tools | GNU make 4.4.1; Mono 6.14.1; Python 3.14.7 |

The effective Xcode, SDKs, .NET SDK and workload set now agree with the accepted handoff and locks. Both actual `_DetectSdkLocations` calls used the platform's device-arm64 runtime project, Release, `EnableCodeSigning=false`, `-m:1` and `BuildInParallel=false`; both exited 0. Node reuse, the MSBuild server and shared compilation were disabled through the retained process environment. No project or workload pin was changed.

At continuation entry, the **global** developer selection already pointed to the new Xcode installation. It was left as found, with the requested process-local selection also supplied. Both Xcode app locations and their Info.plist digests, timestamps and inodes match the continuation baseline. The old Xcode 26.5 / 17F42 installation was left intact. No app rename, global selection change, pairing reset, symbol-cache deletion or simulator-runtime reinstall was performed. The user's new device pairing was not inspected or used.

**Retained inputs, exact equivalence and gates**

The successful K-L `--prepare-only` run and the existing three-run reproduction evidence are retained from the original HOST-A run, **not regenerated or borrowed from the M1**. This continuation independently rehashed all 25 pinned public mod archives, the 31 recorded source/control locks, all three complete snapshots, and the retained complete production closure tree. The final audit repeated the logical, map, audio, source and preservation checks. No live/latest package was selected and no prior M1 build tree, generated closure, native binary or PASS receipt was imported.

Celeste **1.4.0.0 FNA** passed the unchanged canonical input validator again: `itch-macos-fna-1.4.0.0`, FNA 21.3.5, zero Everest markers. Its 1,216-file content set remains SHA-256 `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`. External **FMOD 1.10.09, build 97915** passed both current platform input validators. The original SDK disk image was remounted read-only after its earlier mount was absent. No private input or privileged setup item is currently missing.

Everest `4bbde91b8dbaaddef2ceec75ca0cd6d59b3b8d00`, MonoMod `dfc30a1506d37fb88a2c2be004f525205f46a24c`, **Strawberry Jam 1.0.12**, all accepted helper versions, native source revisions, content locks and semantic authorities remain unchanged. The original report retains the complete package inventory and input digests. Authoritative inputs and the existing vanilla checkout were left unchanged.

| Identity | Required and retained SHA-256 | Result |
| --- | --- | --- |
| Shared closure | `11e5006c72dde385ca3b41773e3d924c29e7b19979aef96e11a5ad44f211d6b9` | EXACT MATCH |
| Managed logical | `906787e52b5d8bc84ab68195532678019beb77947cb7713486af7e1388cffb1f` | EXACT MATCH |
| Content logical | `3ee3129bf48f841786482f9bc88311bb58b3c8cbef3f4550d7a8fe2f1064b8ca` | EXACT MATCH |
| Factory registry | `6e5b89f7d952aa98e72640abce0c75522567fb9d48f8b027e3db54f5cf9da72f` | EXACT MATCH |

| Required readiness gate | Retained and revalidated result |
| --- | --- |
| A — content occurrences | 920/920 accepted or vanilla; blocked 0; unclassified 0 |
| B — actual compiled production registrations | 73/73 available; unavailable/missing 0; provider rejected 0 |
| C — semantic closures | 73/73 closed; blocked 0; unknown 0 |
| D — real composition | PASS; blocked 0; unknown 0 |

The three independent fresh generations and production compilations from the original run have identical complete snapshots, each SHA-256 **`29a42b880cf38c6e869121b9e4e5c45501e6d05fb74ce27a11afc8fc7c1c6990`**. Their **3,123-file** complete closure inventory still equals the retained production tree. The evidence contains six freshly compiled production DLLs per proof and all 73 actual selector/profile-guard checks. Existing comparison rules for COFF timestamps and MVIDs were not changed. The retained composition evidence includes 89,347 autotiler assertions, 290 runtime-composition assertions, 26 real J cells and three graphics cycles; these are host evidence, not physical device results.

Exactly the unchanged **Beginner lobby** and **Bing_Over_Google** remain selected; the other **126 original SJ maps are excluded**. Accepted regression content remains: 20 maps total, 2,943 mounted content files and 158 managed files. The selection plan remains SHA-256 `3b520b9426c239f089d608f044c6d7c5d490570464986a7c04531b2a1954ba59`.

| Original map | Bytes | Retained SHA-256 |
| --- | ---: | --- |
| Beginner lobby | 674,648 | `a4e3e20a2f0cc878fe43b32fb8025d7650b20cc6265f69e37bf3110a7cdf47c2` |
| Bing_Over_Google | 135,363 | `e770a8d193f217d09a6e153fbe272813d26a04d972df947ac812aa5cfe66f347` |

Final map bytes, complete manifests and semantic decisions remain identical. Bank bytes and order were rechecked: vanilla 1–7, ChronoHelper 8, Bing 9, shared SJ 10, Beginner lobby 11, jamjars 12, CollabUtils2 collectibles 13, HonlyHelper 14. The static architecture, one FMOD Studio system, and separate vanilla/module/AEVPSV1 persistence authorities remain unchanged. Runtime behavior of a new device product is not asserted.

**Fresh native results and the AOT blockers**

Native foundations were not yet staged on this VM. The existing pinned fetch/build/verify scripts were therefore run in fresh, isolated, verified-ignored roots before any product publication. Repository recipes, patches, build flags, expectations and verifiers were unchanged. Native compilation jobs were sequential; there were no concurrent AOT jobs and no worker-count tuning.

**iOS:** the unchanged native build exited 0 and produced all five XCFrameworks. Archive architecture/platform/minimum checks, exports and both static force-load link probes passed before the verifier rejected the accepted logical lock. Four components match exactly: SDL2, FNA3D, FAudio and ApplePlatformStubs. **Only Theorafile differs at the component identity level.**

| iOS native identity | Accepted lock | Intel VM, both fresh builds |
| --- | --- | --- |
| Complete native set | `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2` | `b5fbcf54aded9d72e9a16c549fdb5c11bccb6ed7a1e03a059cbefa6b83d1a56d` |
| Theorafile component | `f24cdd931a5ae337d681075d6f093afc7ca303c56652d79bc1510276772d8d21` | `f43ee25b42262cc8b7ce7d998d3456ce374c59e91b4afdf0f4ecd81fad48c1c9` |

A second build in a separate fresh output root, using the exact same pinned sources and unchanged settings, produced the **same normalized manifest and all the same Theorafile member hashes**. Both verifier invocations exited 1. Theorafile remains at source commit `0c5504658a3108919e53b625287786a87529de42`; each device/simulator arm64 variant has 37 Mach-O members, SDK 26.5, minimum iOS 15 and 293 exported symbols. This local repeat proves reproducibility of the mismatch, not equivalence to the accepted artifact. The available lock has the accepted component digest but no accepted per-member fingerprints with which to identify the differing members. No claim that this is harmless metadata or equivalent machine code is made. Sanitized current member evidence is retained as `HOST-A_THEORAFILE_MEMBER_HASHES.json`.

**tvOS:** the unchanged native build exited 0 and produced all six XCFrameworks, including MoltenVK. All 12 device/simulator archive inspections completed with the required platforms, SDK 26.5 and minimum tvOS 16. Device archives contain arm64; the unchanged MoltenVK recipe also includes arm64e. Simulator archives include arm64 and x86_64. These native library variants are not app platform-contract acceptance.

The unchanged tvOS verifier exited 1 with:

`missing SDL2/simulator expected symbols: ['SDL_EnclosePoints', 'SDL_RenderFillRectF']`

Read-only diagnosis found both symbols defined in the device arm64 and each simulator architecture. Capturing the same `nm -gjU` command with stdout and stderr separately preserves both symbol names. The unchanged verifier merges stderr into stdout; 50 `no symbols` warning lines interleave into symbol output, replacing complete names with fragments such as `_SDL_Enclos<archive warning>` and `_SDL_Render<archive warning>`. Its exact-name lookup then fails. The symptom was reproduced without modifying the archive or verifier. This identifies an output-handling blocker, not evidence that the two implementations are absent. It does **not** constitute an alternate verifier PASS. Sanitized evidence is retained as `HOST-A_TVOS_SYMBOL_DIAGNOSIS.json`.

The tvOS verifier stopped before its link probes and final normalized manifest. Therefore the required tvOS native identity **`6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc`** has **not been verified** on this VM. Neither failed native foundation was staged as accepted input. No expected hash, wrapper, source, workload, lock or verifier was changed, and no further full-AOT K-L wrapper invocation was launched past these blocked prerequisites.

**Timings, RAM, swap, disk and cache state**

All instrumented continuation rows below use `/usr/bin/time -lp`. Real/user/system values are seconds; macOS reported maximum RSS bytes are converted to MiB. Commands wrote directly to private logs and retained their actual exit status; no `tee` pipeline obscured a failure. Reported RSS is the tool's command-level figure, not a continuously sampled whole-VM or aggregate process-tree peak.

| Continuation phase | Exit | Real s | User s | System s | Reported RSS MiB | Free disk GiB, before → after |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| iOS .NET device SDK detection | 0 | 0.79 | 0.41 | 0.16 | 81.7 | 1148.43 → 1148.43 |
| tvOS .NET device SDK detection | 0 | 0.46 | 0.34 | 0.08 | 80.9 | 1148.43 → 1148.43 |
| FMOD authoritative DMG remount (read-only) | 0 | 0.13 | 0.00 | 0.01 | 4.1 | 1148.42 → 1148.42 |
| Retained inputs / closure / three-run / gate audit | 0 | 2.99 | 1.64 | 0.57 | 64.2 | 1148.42 → 1148.42 |
| Fresh pinned iOS native source acquisition | 0 | 117.62 | 25.55 | 6.93 | 560.6 | 1148.42 → 1147.97 |
| FMOD iOS input validation | 0 | 1.55 | 0.10 | 0.02 | 26.5 | 1147.97 → 1147.97 |
| FMOD tvOS validation: rejected output location | 2 | 0.01 | 0.00 | 0.00 | 1.1 | 1147.97 → 1147.97 |
| FMOD tvOS validation: permitted ignored output | 0 | 2.49 | 0.19 | 0.11 | 80.0 | 1147.97 → 1147.97 |
| Canonical Celeste input validation | 0 | 2.88 | 1.72 | 0.39 | 43.1 | 1147.97 → 1147.97 |
| Fresh iOS native compile + XCFramework packaging | 0 | 31.92 | 10.73 | 3.65 | 130.9 | 1147.97 → 1147.77 |
| iOS native verification: locked identity rejected | 1 | 3.70 | 7.08 | 5.74 | 112.5 | 1147.77 → 1147.77 |
| Fresh pinned tvOS native source acquisition | 0 | 121.75 | 26.65 | 9.04 | 157.7 | 1147.76 → 1147.19 |
| Second isolated iOS source acquisition (cached Git) | 0 | 5.32 | 0.92 | 0.71 | 109.0 | 1147.17 → 1147.06 |
| Second isolated iOS native compile + packaging | 0 | 27.58 | 10.11 | 3.30 | 131.1 | 1147.06 → 1146.87 |
| Second iOS verification: same identity rejected | 1 | 3.27 | 6.79 | 5.63 | 111.0 | 1146.87 → 1146.86 |
| Fresh tvOS native compile + XCFramework packaging | 0 | 263.94 | 29.96 | 8.66 | 134.0 | 1146.86 → 1145.76 |
| tvOS native verification: symbol extraction rejected | 1 | 4.98 | 10.02 | 8.43 | 96.0 | 1145.76 → 1145.76 |
| Final audit, first evidence export | 0 | 4.79 | 2.73 | 1.11 | 72.1 | 1145.76 → 1145.76 |
| Final audit, confirmed separate evidence export | 0 | 4.35 | 2.73 | 1.05 | 62.7 | 1145.76 → 1145.76 |

| Retained prior measurement (2026-09-08) | Real s | User s | System s | Reported RSS MiB |
| --- | ---: | ---: | ---: | ---: |
| Initial host bootstrap, downloads included | 871.90 | 3.40 | 6.52 | 20.1 |
| Successful unchanged K-L prepare-only, combined phases | 143.01 | 76.91 | 33.63 | 244.8 |
| Three fresh closure generations + production compilations + gates | 265.40 | 142.18 | 74.21 | 244.9 |

The prior measurements are labeled historical retained HOST-A evidence and were not rerun today. The original report contains the complete original bootstrap/input/test timing ledger and its failures. Bootstrap, managed generation, compilation and composition were combined where the existing wrapper combined them; no separate timing is invented. Native build rows combine native compilation and XCFramework packaging. They are **not .NET game AOT timings**.

| Requested expensive phase | Continuation result | Timing |
| --- | --- | --- |
| New closure generation | NOT RERUN; validated prior three-run evidence retained | No new generation time |
| Fresh unsigned iOS full-AOT + app packaging | NOT RUN — native identity gate blocked | Unmeasured |
| Fresh unsigned tvOS full-AOT + app packaging | NOT RUN — native verifier blocked | Unmeasured |
| Final app/platform/full-AOT/native-runtime/forbidden-surface verification | NOT RUN — no fresh apps | Unmeasured |

Native source acquisition started with fresh build roots and downloaded exact pinned public Git sources. The second iOS run and tvOS acquisition reused the newly acquired public Git cache where revisions overlapped, while their build outputs were fresh. MoltenVK's pinned dependency graph was acquired through the existing script. Retained .NET SDK/workload/NuGet/tool and exact mod-package caches were used; no SDK or workload was installed or changed in this continuation. OS file caches were not flushed. Total network transfer bytes were not separately metered. Some independent input validation overlapped source acquisition, so setup times and volume deltas must not be summed into a serial AOT benchmark. No M1 speed comparison is claimed.

RAM stayed at 48 GiB. All 38 instrumented before/after swap checkpoints reported 0 MiB used. Captured compressor and swap activity remained zero. No continuous peak-memory or full-AOT stress result is claimed.

| VM memory category | First continuation checkpoint GiB | Final audit GiB |
| --- | ---: | ---: |
| Free pages | 19.93 | 12.53 |
| Active pages | 10.57 | 14.20 |
| Inactive pages | 8.95 | 12.02 |
| Speculative pages | 5.51 | 6.16 |
| Wired pages | 3.03 | 3.07 |
| Compressor allocation | 0.00 | 0.00 |

Free volume space decreased by approximately **2.67 GiB** during the continuation observation interval. This is a volume-wide measurement, not exact artifact storage accounting. No broad cleanup or deletion of authoritative inputs or accepted artifacts was performed.

The FMOD tvOS input validator initially rejected an outside-tree output location (exit 2); its existing interface requires a permitted ignored root. Only the invocation's output path was corrected, and the unchanged validator then passed. A private final-audit export initially shared the timing-record filename; the outside-tree helper's export name was corrected and the preservation audit rerun successfully. Neither issue changed repository code or relaxed a check. The substantive unresolved failures are the two native gates described above.

**Product contract and scope accounting**

| Requirement | Source / retained contract | Fresh VM product result |
| --- | --- | --- |
| iOS | arm64; universal family [1,2]; minimum iOS 15 | NOT BUILT / NOT VERIFIED |
| tvOS | arm64; family [3]; minimum tvOS 16 | NOT BUILT / NOT VERIFIED |
| Trimming and AOT | Full trimming, full device AOT, `UseInterpreter=false`; static no-JIT architecture retained | NOT RUN / final linked binary not verified |
| Serial MSBuild | Node reuse disabled; MSBuild server disabled; shared compilation disabled; `-m:1`; `BuildInParallel=false` | Retained; no AOT benchmark launched |
| Bundle/signing identity | Local experimental platform IDs retained privately; unsigned scope | No signing credentials or device products used |
| Final native and forbidden surfaces | Existing checks retained; failed foundation checks reported | Final linked product checks NOT RUN |

The original general host doctor and historical aggregate/physical/signing-verifier limitations remain documented in the preserved report. Their earlier failures are not relabeled PASS. The K-L aggregate driver was not weakened, and no historical clean-clone driver that imports excluded diagnostic history was used. No signed-IPA equality requirement or historical physical PASS was transferred to this VM. The unsigned behavior of later signing-dependent product checks remains untested because no app exists.

Final audit PASS: exact HEAD and branch; clean tracked files and untracked status; all **8 recursive submodules** clean at unchanged gitlinks; all 31 recorded control digests unchanged; qualification refs unchanged; original checkout and all its refs unchanged; public protected release refs re-read and unchanged; public `tvos-port` still the exact accepted SHA. Diagnostic K-G, K-I and K-K commit objects remain absent from the qualification repository. No source/version/manifest/build-setting/lock/verifier changes or documentation commit occurred.

Generated output roots are verified ignored, and raw logs/evidence are private outside Git. This report and the two sanitized diagnostic JSON files contain no proprietary bytes, generated game source, private absolute paths, local bundle IDs, saves, credentials, signing material, or device/team identifiers. Nothing was tracked or published.

Commits/merges/pushes/force-pushes/tags/PRs/releases: **NOT RUN**. GitHub Actions: **NOT RUN (0 invocations)**. Signing: **NOT RUN**. Worker tuning: **NOT RUN**. Concurrent AOT: **NOT RUN**. Device installation and physical testing: **NOT RUN**. iPad mini 4 testing: **NOT RUN, deliberately deferred**. Stage 25K-M: **NOT RUN**. The accepted physical iPhone/Apple TV results belong only to the historical accepted products.

Recommended VM role: **validated build-46 closure preparation, determinism and host regression worker; candidate heavy AOT host still awaiting native and device-product qualification**. The M1 remains the signing/install fallback. HOST_GREEN_BUILD_ONLY is unavailable until equivalence and both fresh full-AOT products pass; HOST_RED is not established by these validation blockers.

**Exact next recommended action:** Perform a separately scoped native-equivalence diagnosis: compare the accepted M1 Theorafile per-member verification manifest with `HOST-A_THEORAFILE_MEMBER_HASHES.json`, and review the demonstrated tvOS `nm` stream corruption using `HOST-A_TVOS_SYMBOL_DIAGNOSIS.json`. Keep this build-46 checkout frozen and resolve both native gates before resuming the unchanged serial unsigned K-L AOT benchmark. That next task has not been started.

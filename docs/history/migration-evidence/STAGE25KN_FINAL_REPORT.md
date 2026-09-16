# Stage 25K-N — unchanged The Squeeze, build 49

**READY_FOR_PHYSICAL_ACCEPTANCE — both fresh signed build-49 products pass the applicable product checks. Full physical acceptance remains pending. This is not gameplay GREEN or integration approval.**

Build 48 completed iOS full AOT/linking, then failed the strengthened product verifier on the existing out EventDescription parameter. It was not installed. Its original failure and artifact bytes remain preserved. Diagnostic host-verifier correction passed all 17 AOT controls against those artifacts without promoting that attempt.

Build 47 is preserved as a failed physical attempt. Its initial iPhone title, credits, icon, spawn and play were user-confirmed, then death/reload caused a black screen. A retained app log records a null-reference exception in Audio.SetMusic through AudioState.Apply and Level.Reload. The process remained alive at the failure query. No previous physical observation is transferred to build 49.

## Source, scope and delivery

- Start / required unchanged tvos-port: `b65bedd20016dc3482d7702d7f0a9707bc2b1479`.
- Original physically accepted game baseline: `be8546d4ae411cb491a0c3bbc4e5343ebf9c9651` (build 46). HOST-B `632a451da2f62928404a2408826747aa6fc55b20` and HOST-C `b4997fa49f4f8b82f6f2984fe9047396e401ea48` tooling are retained.
- Branch: `feature/apple-everest-sj-snas-expansion`.
- Commit 1: `06c6fe0a5578888cf332cd5ed323aeb1065f1b34` — unchanged The Squeeze finite compatibility/composition; build 47, preserved failed physical attempt.
- Commit 2: `16e839e65a34814582b9299958b079cd8913f027` — audio name correction, build 48; preserved iOS verifier failure, no installation.
- Final frozen commit: `34c0b933a4cf2252780ec84e5630847809793eab` — exact concrete by-reference AOT root verification; canonical version **0.1.1 (49)**.
- The final commit preceded clean reproduction and both actual generation/AOT/link/sign/package/verification runs. No source changes or relabeling of earlier products were used.
- Feature delivery: **PASS_FEATURE_BRANCH_PUSHED_NO_INTEGRATION**. Remote feature SHA: `34c0b933a4cf2252780ec84e5630847809793eab`. No integration was performed.
- Delivered candidate: [exact source revision](https://github.com/hmcneill46/celeste-ios/tree/34c0b933a4cf2252780ec84e5630847809793eab). The user retains the eventual manual fast-forward decision after review and physical acceptance.

## Exact content and audio

Exactly the unchanged Beginner lobby, Bing_Over_Google and snas are selected, plus all 18 prior regression maps (21 total). The other 125 original SJ maps stay excluded. Only Bing and snas are available among the 23 authored lobby destinations. Gym/heartside remain unavailable and the original 21-heart threshold is unchanged. No map/entity/room bytes were edited or acceptance objects injected.

| Authority | SHA-256 / identity |
| --- | --- |
| SJ 1.0.12 ZIP | `4e1a2fc12baa3db27da433b93bf26b59f34b3e6b7f760c41d8d127636d020655` |
| SJ root DLL | `8d5b9184204e7e7728965bcf95af5f150f6220dafbfe52fdd6c23e50d47e5258` |
| snas BIN — 67,718 bytes | `6ad3172d496e8b5b4ce71f1128fe231b162d534419dc823d2af2cea27fc241d9` |
| snas appendix — 14,677 bytes | `f13ab43805a004226d5c7eb82a42a028884be222f988198ce7bcf14708b28a04` |
| Beginner lobby | `a4e3e20a2f0cc878fe43b32fb8025d7650b20cc6265f69e37bf3110a7cdf47c2` |
| Bing | `e770a8d193f217d09a6e153fbe272813d26a04d972df947ac812aa5cfe66f347` |
| CommunalHelper 1.25.5 ZIP | `44f4fb0b277a4900fd2a555e1a73e661140aa7b2455d3c7776cf420193349e3c` |
| CommunalHelper DLL | `4011b959ed4e9cc4cb98bf43f6884ae81361fecc994787640553205029c94a8a` |
| snas bank — 3,320,960 bytes | `08c980621201485026849c90e41b275c442448527d3c98096adeb73d562a289a` |
| GUID companion — 82,696 bytes | `4b8dfb05ba4d790b6acf0a86904cd0e28b7fefd3e9cdcd8a53050ac29cd74039` |

The Squeeze retains initial room `1`, canonical original spawn `(32, 104)` and terrain seed `2989`. Default spawn was checked against canonical authored selection, not parsed-first order. Exact Celeste 1.4.0.0 FNA, FMOD 1.10.09 build 97915, Everest/MonoMod/helper identities remain pinned. Existing public package bytes were rehashed on each preparation; no latest resolution or M1 binaries were used. CommunalHelper contributes only the finite selected closure; EeveeHelper is absent.

The bank order is vanilla 1–7, Chrono 8, Bing 9, shared 10, snas 11, lobby 12, jamjars 13, Collab collectibles 14, Honly 15. The exact bank/GUID registry binds event:/sj21_snas, event:/sj21_snas_flourish and event:/sj21_snas_special, rejects duplicates and retains one FMOD Studio system. All 2,946 closure content files are unchanged from build 47.

Complete selected SID inventory:

- `AppleEverest/CustomAudio`
- `AppleEverest/DJFrozenIl`
- `AppleEverest/Stage25KE`
- `AppleEverest/Stage25KF`
- `AppleEverest/Stage25KH`
- `AppleEverest/Stage25KJMax`
- `AppleEverestStage25KJ/0-Lobbies/1-Fixture`
- `AppleEverestStage25KJ/1-Fixture/1-FinishA`
- `AppleEverestStage25KJ/1-Fixture/2-FinishB`
- `AppleEverestStage25KJ/FactoryProfiles/Atmosphere`
- `AppleEverestStage25KJ/FactoryProfiles/BaseHelpers`
- `AppleEverestStage25KJ/FactoryProfiles/CameraCorridor`
- `AppleEverestStage25KJ/FactoryProfiles/Collab`
- `AppleEverestStage25KJ/FactoryProfiles/CrystalCave`
- `AppleEverestStage25KJ/FactoryProfiles/Frost`
- `AppleEverestStage25KJ/FactoryProfiles/Masks`
- `AppleEverestStage25KJ/FactoryProfiles/MaxMechanics`
- `AppleEverestStage25KJ/FactoryProfiles/WaterGarden`
- `StrawberryJam2021/0-Lobbies/1-Beginner`
- `StrawberryJam2021/1-Beginner/Bing_Over_Google`
- `StrawberryJam2021/1-Beginner/snas`

## Bounded implementation and failure correction

| Issue | Classification and scope |
| --- | --- |
| KN01 bubble | AUTO_IMPLEMENT_BOUNDED_COMPATIBILITY: canonical two-node cassette flight; dead/state-21 guards and cleanup. |
| KN02 RandomSound | AUTO_IMPLEMENT_BOUNDED_COMPATIBILITY: exact empty lists/flags, 12 oneUse and 11 repeat occurrences, Next(1), zero-delay yield and scheduling/removal timing. |
| KN03 flag group | AUTO_IMPLEMENT_BOUNDED_COMPATIBILITY: typed ordinal SID/mode/group flags; switch 422 persistent, gate 423 nonpersistent; leave/reload restoration and closed-creation guards. |
| KN04 exact profiles | AUTO_IMPLEMENT_BOUNDED_COMPATIBILITY: camera/platform/core/MiniHeart constructors, types, initialization and lifecycle; guard-only evidence is insufficient. |
| KN05 graphics | AUTO_FIX_COMPOSITION: full original XML/atlas/terrain/decal/implicit assets; deferred loading preserved. |
| KN06 real composition | AUTO_FIX_COMPOSITION: source/SID/title/icon/default spawn, destinations, bank binding and cross-map restoration. |
| KN07 progression/legacy | AUTO_FIX_COMPOSITION: unchanged 20 old compatibility descriptors plus one snas descriptor; six separate legacy controls remain independent. |
| KN08 mandatory evidence | AUTO_FIX_COMPOSITION: fresh four-gate proofs, identities, caller enforcement and actual linked/AOT/product negatives. |
| KN09 death/reload audio | AUTO_FIX_COMPOSITION: GUID-to-event-name lookup using the existing validated bank registry and the current FMOD system. |

KN10 — AUTO_FIX_COMPOSITION: derive the pinned Mono trailing-underscore spelling for concrete ordinary by-reference parameters. Exact T/t code is still required in both LLVM object and linked image; no omitted root, hash exclusion or normalization change. Eighteen owned signature controls reject unsupported shapes. Actual-product controls reject missing out-parameter code and by-value substitution. This host-only correction changes no game/runtime or native foundation source. Canonical build 49 and the exact new issue-ledger/version-authority pins were independently reviewed; all logical identities remain unchanged.

The exact switch/gate group is SID snas, mode 0, room 3, flag flag_snasberry_switch, legacyMode=true and groupPersistence=true. Creation guards also run before Added, including delayed scene binding. They do not globally disable unrelated canonical behavior.

Build 47 assigned an unchecked null FMOD path to CurrentMusic, then dereferenced it on the next AudioState.Apply. Preserved build-47 generated methods reproduce the same exception with the owned stringless-event response fixture. Build 47 did not record the exact native getPath return code; this remains a provenance limit, not an invented observation. Pinned Everest revision 4bbde91b8dbaaddef2ceec75ca0cd6d59b3b8d00 Audio.cs (SHA-256 fa45530db9c208d0f0a417bc202fd41fb70ed968ff4c7476b359e39cffdc54cf) supplies the GUID-cache reference behavior.

Build 49 uses an exact registered name only for ERR_EVENT_NOTFOUND with a known GUID in the currently loaded system. Unknown/conflicting names, invalid handles, wrong systems and other failures stay rejected. Teardown/failed load clear the reverse table. Canonical music/ambience/AudioState logic is unchanged apart from typed name resolution. The new runtime records the actual native stringless-event result for subsequent device diagnosis. This changes emitted application AOT code; native foundations, dependency semantics, maps, banks and event scheduling remain unchanged.

The mandatory generated-method test executes actual production Audio/AudioState/custom-registry methods through explicit owned FMOD response fixtures: **152 checks, 50 reloads, five lobby/snas/Bing transitions**, including same-instance reuse, fades, parameters, ambience, errors, lifecycle and duplicate rejection. Old code fails the second Apply; corrected generated code passes. These are source-bound host behavior checks, not native audible playback. A preliminary macOS native experiment stopped before bank loading because the lawful Mac game supplied FMOD 1.10.20 rather than the pinned device 1.10.09; it is not counted as a playback PASS.

No major architecture expansion, runtime mod/assembly scanning, live detours/IL mutation, generic DynamicData/reflection, JIT/interpreter, Lua/native plugins or second audio system was introduced. Vanilla/module/AEVPSV1 authorities stay separate. The accepted build-46 codec already writes envelope 2 and accepts 1/2; its bytes are unchanged. K-N introduces no schema migration or downgrade. Old map compatibility descriptors remain unchanged.

## Readiness, determinism and regressions

| Gate | Both actual build-49 closures |
| --- | --- |
| A | 1309/1309 occurrences; zero blocked/unknown |
| B | 83/83 actual production registrations; zero missing |
| C | 83/83 semantic closures; zero blocked/unknown |
| D | Real 21-map composition PASS; zero blocked/unknown |

Counts regenerate from exact source: 973 SJ and all 336 regression occurrences, 83 IDs and 604 raw authored profiles. The proof scopes are 77 selected registrations plus six separately proved legacy controls; those six are not relabeled as selected73 coverage. Existing runtime, differential, terrain, asset, progression and composition proofs remain mandatory. All linked assembly/factory/profile/native/AOT checks run on actual device products, not host-preflight substitutions.

Three fresh normal-wrapper preparations independently compiled and proved the build-49 closure and agreed in every identity. Final clean recursive checkout at 34c0b933a4cf2252780ec84e5630847809793eab regenerated canonical source and the expanded closure without copied generated outputs or PASS receipts. The earlier build-48 clean reproduction had a GitHub DNS failure during pinned HookGen acquisition, followed by a successful complete retry at its original SHA; those records remain separate history. Each final product regenerated and revalidated its own closure again before AOT.

| Logical authority | SHA-256 |
| --- | --- |
| collabManifestSha256 | `111f7cc7cda8f754b30f10097d9964a686a4b83e1d200d0fe57692eee09c1b5f` |
| compositionLogicalSha256 | `15244ab33e8ac980b526a42752740c6b4ff7298b843c975906f4060043f1a76a` |
| contentLogicalSha256 | `92e9ce053b4957230e760933336f2de8910f2762c834aca6a95c35aa95418e80` |
| customAudioManifestSha256 | `31788184560d70e5de36827c7d345fe778178a09026f59d54405b52adb82c11e` |
| customBankLogicalSetSha256 | `b363c32eb1f985c35ced264649656f0266c0c9be79468bfe6d31cc69626db63b` |
| factoryRegistrySha256 | `9d7fa8c198cea379e9e330b81741c02b2e747e97050ab2ec078c5432ca13ff04` |
| levelSetProgressionManifestSha256 | `81e3725848e8cabaa019bb2fcf9b010a5772442beb2de4470a3785b8d7b6fbbc` |
| managedLogicalSha256 | `fedad431cde0516dd2ead5848f6327448e9630dbc5e10a02804643404fd1be62` |
| registrySha256 | `6e5b89f7d952aa98e72640abce0c75522567fb9d48f8b027e3db54f5cf9da72f` |
| semanticLogicalSha256 | `f45f91061d08bb26c52354870f6c663523c666dcba995dfa3778fc3c6fae3245` |
| sharedClosureSha256 | `c236e3e77e6626e1600ce361c3214c62473cf79a279e17a9784f00269ff9bf7d` |

The gameplay registry and generic registry remain distinct. Build-46 and K-M authorities are unchanged historical controls, not the expanded target hashes. Semantic/composition evidence bodies and current source bindings are rehashed; labels alone cannot pass. Both new product verifiers require native AOT definitions for Audio.GetEventName/SetMusic/SetAmbience, AudioState.Apply and the custom resolver/lifecycle.

Current host regressions pass: 724 builder checks, 50 typed-hook checks, 103 K-N contract controls, 29 HOST-B portable native tests, HOST-C host/signing tests, 21 historical K-M tests, pinned desktop HookGen, storage pairing/protocol/continuity, soft reload and canonical input contracts. Existing wrong-provider/version/hash, missing implementation/evidence, unsupported attributes/nodes, missing assets, wrong destinations and unproved creation-branch controls remain. New controls reject missing audio proof and actual missing resolver code. Unknown evidence never defaults to ready.

## Toolchain, native reuse and actual products

Guest: x86_64 macOS 26.6.2 (25G83), 48 GiB RAM, eight physical/16 logical CPUs, guest CPU label Intel Core Processor (Skylake). i9-13900K P-core backing is user-reported, not inferred. Process-local Xcode 26.6 / 17F113 and SDKs 26.5; .NET 10.0.302 / workload set 10.0.302.0; compiler/runtime 10.0.10; pinned host SDKs 8.0.424/9.0.317. Global developer selection, Xcode installations and system settings were unchanged.

Validated VM HOST-B native foundations were reused through supported staging: iOS `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2`; tvOS `6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc`. No fresh native timing or copied M1 binary claim is made. Native/normalization locks remain strict.

| Fresh product | iOS | tvOS |
| --- | --- | --- |
| IPA SHA-256 | f460ec047bcf3ca75baa7e77425be5c1fe7a794b37c8fb2ec496a054183b458e | f1fd9c39681949ea10d68ce0b9d01c6bd6ab6bda9d4febe60819f7c253778a97 |
| IPA bytes | 921310720 | 935547726 |
| Payload files | 4260 | 4254 |
| Actual packaged managed assemblies | 43 | 41 |
| Native image SHA-256 | cee18e87b301b54d721363c7b7cc02519a3ae3b124457bba7329eb997211f879 | fa089e9220fe94da74f54b5908fe282589f9d23ece1a3f00a4f8803d80a9e3d2 |

Both wrappers exited zero. Strict development signatures, selected profile/entitlements and exact app identities passed. Final IPA/payload byte agreement passed. iOS is arm64 universal [1,2], minimum 15; tvOS arm64 family [3], minimum 16; compile SDK 26.5. Full trimming and full/static LLVM device AOT, UseInterpreter=false and no JIT remain enforced. Every packaged assembly has actual linked input, compiler arguments, mono/LLVM objects and matching AOT data. Native code/data/platform/SDK/minimum/family/export and forbidden-surface checks pass. HOST-C binds the actual osx-x64 compiler pack/version/hash/LLVM/invocation and clean before/after source receipts.

- ios: 15 platform negative controls and 17 actual-product AOT negative controls PASS, including missing audio resolver/out-parameter code and by-value substitution. 77 selected plus six separate legacy linked entries verified; unchanged applicable historical IPA checks passed without importing old IPA digests or physical acceptance.
- tvos: 15 platform negative controls and 17 actual-product AOT negative controls PASS, including missing audio resolver/out-parameter code and by-value substitution. 77 selected plus six separate legacy linked entries verified; unchanged applicable historical IPA checks passed without importing old IPA digests or physical acceptance.

## Timings and resource scope

All app builds ran serially: iOS then tvOS, with fresh roots and no stale application objects. MSBuild node reuse/server/shared compilation were disabled; -m:1 and BuildInParallel=false stayed unchanged. No allocator stress variables or worker tuning were used.

| Phase | Wall s | User s | System s | Command max RSS bytes | Free GiB before → after |
| --- | ---: | ---: | ---: | ---: | --- |
| kn49-closure-1 | 202.44 | 401.1 | 52.4 | 4861792256 | 1034.609 → 1034.419 |
| kn49-closure-2 | 169.0 | 340.99 | 45.56 | 5156573184 | 1034.419 → 1034.238 |
| kn49-closure-3 | 171.5 | 354.42 | 46.19 | 5096636416 | 1034.238 → 1034.061 |
| kn49-host-regressions-2 | 67.18 | 84.81 | 12.69 | 826343424 | 1034.633 → 1034.63 |
| ios-full-wrapper-3 | 490.5 | 856.34 | 85.07 | 5063553024 | 1031.682 → 1026.722 |
| ios-final-verification-3 | 28.62 | 27.23 | 4.31 | 561745920 | 1026.722 → 1026.317 |
| tvos-full-wrapper-3 | 492.94 | 873.95 | 84.61 | 5124743168 | 1026.558 → 1021.72 |
| tvos-final-verification-3 | 28.8 | 27.58 | 4.14 | 637710336 | 1021.72 → 1021.719 |

Clean-checkout phase wall seconds, including cold dependency setup: clone 0.114, checkout 0.241, submodules 76.376, recursive-cleanliness 0.504, native-staging 4.191, tool-restore 3.353, canonical-preparation 52.788, expanded-preparation 546.456, host-regressions 81.175. Validated SDK/source/public-package/native caches were reused with hashes; closures and app objects were fresh. Earlier build-48 DNS and verifier failures remain separate history.

Full-wrapper timings combine mandatory preparation, actual AOT/link, signing/package and wrapper verification. Separate final verification is timed independently. Compiler-boundary observations below are wall-clock observations within the wrapper, not isolated CPU timings:

- ios: BEFORE_AOT at 227.97 s, AFTER_AOT at 393.652 s, AFTER_NATIVE_LINK at 396.478 s.
- tvos: BEFORE_AOT at 228.745 s, AFTER_AOT at 394.662 s, AFTER_NATIVE_LINK at 396.894 s.

The tvOS wrapper overlapped the verified iPhone update: private backup, stopping the old process, in-place installation and ordinary launch/console activity. No app builds ran concurrently. These tvOS timing/resource measurements are therefore not an isolated performance benchmark. Historical preservation scanning started only after both product timings completed.

- ios swap before: `vm.swapusage: total = 0.00M  used = 0.00M  free = 0.00M  (encrypted)`; after: `vm.swapusage: total = 0.00M  used = 0.00M  free = 0.00M  (encrypted)`.
- tvos swap before: `vm.swapusage: total = 0.00M  used = 0.00M  free = 0.00M  (encrypted)`; after: `vm.swapusage: total = 0.00M  used = 0.00M  free = 0.00M  (encrypted)`.

Resource evidence uses /usr/bin/time -lp with real child exit status plus disk/swap checkpoints and five-second process samples. Command RSS is not aggregate whole-system or build-service peak memory. Sampling and receipt polling add small unquantified overhead; no profiler was attached. No M1 speedup is inferred. See private raw receipts for exact samples; they are not published.

## Deployment and physical acceptance

Current targets: iPhone 12 Pro Max / iPhone13,4 / iOS 26.1, and Apple TV 4K third generation / AppleTV14,1 / tvOS 26.6. Both exact pinned SDK signing-detection checks passed using local existing development credentials. Team, profile, device and private identifiers are omitted here. VM signing/build qualification is distinct from installation and gameplay acceptance.

- ios installation: **PASS_BUILD49_IN_PLACE_INSTALL_PERSISTENCE_PRESERVED**; supported in-place update, no uninstall or identity change for this replacement. Install command 40.633 s. All 20 backed-up persistence files (121471 bytes) matched byte for byte after installation.
- tvos installation: **PASS_BUILD49_IN_PLACE_INSTALL_PERSISTENCE_PRESERVED**; supported in-place update, no uninstall or identity change for this replacement. Install command 429.236 s. All 1 backed-up persistence files (30071 bytes) matched byte for byte after installation.

A normal stop request did not terminate each old build-47 process. Fresh scoped backups and exact installed-app process binding preceded a targeted forced stop of only that application, followed by the supported in-place update. The iPhone had the previously reported black screen; the Apple TV stop result alone is not evidence of a gameplay hang. Earlier build-47 cross-team deployment and backup/restore records remain separate history.

- ios ordinary launch: **PASS**. The live process was bound to the exact installed build-49 application, with no debug route, injected app arguments or device-child environment. The retained console captures only this application. Launch success does not establish gameplay acceptance.
- tvos ordinary launch: **BLOCKED_DEVICE_ASLEEP**. tvOS rejected foreground launch with “System is asleep — foreground app launch forbidden.” The product remains installed and verified; no code, signing or pairing failure is inferred. Minimum manual action: wake Apple TV using Home/Menu and leave it on the Home Screen for ordinary launch. No wake result has been reported yet.

All build-49 human matrix rows remain pending until explicitly reported for each exact final product. First recheck: real Beginner lobby → The Squeeze jar/panel, correct entry, then room-1 death/retry with visible recovery and audio continuity. The full separate checklist covers the unchanged full route/completion, berries/MiniHeart, bubble/platforms, switch/gate state through re-entry/death/cold resume, music/triggers/restoration, numbered save, complete termination, lobby return/Continue/journal/progression, Bing save/resume, excluded destinations, device lifecycle and controls. Automated launch or a successful host fixture is not physical gameplay acceptance.

Build-46 persistence was privately backed up and supported restored-byte roundtrips passed before build-47 play. New in-place updates take a further scoped backup. Slot isolation, A/B recovery, absent-map quarantine/re-addition and lineage controls must use disposable data through supported methods. Existing host fixture evidence remains separate from pending device-scope observations. No user save is corrupted for testing.

iPad remains **IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY**. Universal static/product requirements remain enforced; no modern-device quality reduction. M1 remains the signing/install fallback if needed.

## Preservation, privacy and remaining action

Historical HOST-A/B/C, K-M and vanilla evidence/native/build outputs: PASS preservation check over 615168 files and 94309190112 bytes; changed entries 0. Build-47 failure artifacts and observations are retained separately.

The final implementation and clean reproduction remained at 34c0b933a4cf2252780ec84e5630847809793eab with clean tracked trees and pinned clean recursive submodules. Historical HOST-A/B/C, K-M and vanilla checkout state was independently checked. All build-47 products and all eleven retained build-48 request artifacts were rehashed unchanged. The feature push changed only the authorized feature branch; tvos-port remained at the K-M starting SHA.

| Authoritative remote ref after feature delivery | Exact value |
| --- | --- |
| `refs/heads/tvos-port` | `b65bedd20016dc3482d7702d7f0a9707bc2b1479` |
| `refs/heads/release/v1.0.0-rc.3` | `c8134c8ca7924cf12f48527e714b5242c6024927` |
| `refs/tags/ios-v0.1.1-rc.1^{}` | `27e16b4724d94d3991b99c4795f680fcb0e5830c` |
| `refs/tags/v1.0.0-rc.1^{}` | `ee52b0868df091746f134d95d4f020f94f23d4fb` |
| `refs/tags/v1.0.0-rc.2^{}` | `641e86e4ed164cdf93f602ce2f11436449654d6e` |
| `refs/tags/v1.0.0-rc.3` | ABSENT |

The isolated checkout does not materialize historical release tags/branches; their authoritative remote values were verified without creating local refs. The present local tracking refs were checked unchanged. There were no active executable hooks or configured custom hook path, zero tracked/remote workflows before and after push, and zero Actions runs at the delivered SHA. Repository settings were unchanged.

Only project-owned tooling/runtime/tests and sanitized stage documents are tracked. All 62 changed files were reviewed as owned UTF-8 text. Raw logs, generated proprietary source, ZIP/DLL/BIN inputs, banks, native archives, apps/IPAs, saves, credentials, team/device identifiers and private paths remain outside Git or verified ignored. No force-push, merge, PR, tag, release, upstream push or Actions invocation occurred. There was no simulator/system/pairing housekeeping, worker tuning, extra map expansion or subsequent stage.

**Integration-ready: NO.** Delivered candidate 34c0b933a4cf2252780ec84e5630847809793eab requires both exact final physical products to complete the mandatory matrix. The next action is to report the iPhone build-49 real-lobby room-1 death/retry result, including respawn, touch controls and music, and wake Apple TV for ordinary launch. Then repeat that first check on Apple TV and continue the separate full physical checklist on both products. Do not merge or begin another map.

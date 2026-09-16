# Morro migration and unsigned rebuild report

**Local migration and four fresh unsigned product builds: PASS.** Strict byte-identical IPA reproduction: **NOT MET**.

The modern vanilla and finite static-Everest build-49 project is now independently buildable in the local `Morro-Celeste` folder. The suggested searchable GitHub name is `morro-celeste`. This is a source migration and automated product result, not new gameplay acceptance. The remaining binary differences are reported below, without weakening any game, native, content or AOT acceptance lock.

## Source, history and contents

Original build-49 source: `34c0b933a4cf2252780ec84e5630847809793eab`. Final Morro tooling source: `6dc6427d1bdeab1a92987a4a1936235e48b7266b`. Branch: `codex/morro-modern-aot-migration`. All four final Morro app builds used this final clean commit. Canonical app version/build remains **0.1.1 (49)**; no bundle identity, save domain or in-game credit was changed for branding.

- `450fc3bfc55de3bbda8594ea4c17abd20576d946` — Establish Morro standalone modern Apple AOT source tree
- `ac5ce57716c7a3c425fca558510de7f518f830e4` — Bind Morro vanilla verification to the modern build-49 foundation
- `d3544cdb615c32cda4671ec7474da252c67e3dfb` — Validate release ancestry in standalone and no-tags cloud checkouts
- `6dc6427d1bdeab1a92987a4a1936235e48b7266b` — Launch non-executable Python build entry points through the interpreter

All 158 original ancestor commits retain their hashes, authorship, dates and messages; the local repository now has 162 commits. Original authors include hmcneill46, rooot, Toshit Chawda and Kyrill. `README.md`, `CREDITS.md` and the unchanged original MIT `LICENSE` explicitly credit RoootTheFox and other contributors. A GitHub fork badge is optional; none is claimed or created locally.

The main repository and all recursive submodules have complete local Git databases inside Morro, with no external object alternates; connectivity checks pass. The source inventory accounts for **992 tracked entries** and **32 reasoned removals**. It retains the modern iOS/tvOS hosts, shared controls/storage, canonical generation, pinned FNA recursive submodules, native producers and locks, static Everest tooling/runtime/authorities, tests, cloud template, and modern current/historical documentation. Removed working-tree material is the unused Xamarin app/solution/build script/config, unused old patches and unused native-builder gitlink. The still-required crash-fix patch remains byte-identical; the original app icon was relocated byte-for-byte. The modern native producer still uses its separately locked original native-builder source dependency. Historical files remain accessible in Git history.

Every retained file and removal has a reason in `docs/MORRO_FILE_INVENTORY.json`; `scripts/verify-morro-layout.py` validates the inventory and unchanged production authorities. Eight original HOST/K-M/K-N reports were copied with identical hashes. The two user-supplied build documents were retained as sanitized historical reference with original/sanitized hash records. None of that history is relabeled as a current PASS.

Morro links to [Cabrillo](https://github.com/hmcneill46/cabrillo-celeste). Its separate active working tree was not changed. Future Cabrillo iPadOS/other Apple-platform scope remains that project’s decision; no additional Morro platform support is claimed.

## Bounded migration adjustments

- Added an unsigned vanilla/Everest convenience entry point, fresh-run/source-clean checks, local tool/input paths, phase receipts and explicit Python-interpreter dispatch for non-executable Python scripts.
- Updated icon lookup and the vanilla iOS host doctor for the already qualified Intel/macOS host while retaining exact Xcode/SDK/workload pins. Made vanilla iOS publishing explicitly serial with shared compilation disabled.
- Replaced obsolete checks of removed Xamarin files with the complete source inventory and retained native/host isolation checks. Updated the modern iOS graph path and README/credits expectations.
- Corrected the vanilla tvOS verifier’s obsolete version literals to validate the existing canonical version import/bindings. Missing, changed, duplicate or conditional bindings still fail.
- Made the existing historical release commits mandatory ancestors in no-tags cloud clones; a present release tag must still match exactly. Missing objects, wrong tags and unrelated ancestry fail.
- Restored the desktop HookGen host builder before running it in a fresh checkout. Added an explicit five-file cloud-template exporter bound to a literal repository and immutable source commit.

There are no changes to game/runtime code, transforms, mod/helper implementations, native code/patches, dependency revisions, acceptance hashes, normalization, map/content/audio selection, persistence or canonical version authority. Source comparison against build 49 and the inventory verify those boundaries.

## Inputs and effective host

The local folder contains ignored, validated private copies of the lawful Celeste 1.4.0.0 FNA input, FMOD 1.10.09 build 97915 headers/revision/device archives, and all 26 exact K-N public package ZIPs. All 1,264 copied private files were rehashed. The Celeste Content tree is 1,216 files / 1,158,665,183 bytes, aggregate `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`. Only the locked shipped settings marker is retained, not user saves. All package/DLL identities remain under the unchanged project authorities; no live/latest resolution was used.

Strawberry Jam 1.0.12 ZIP: `4e1a2fc12baa3db27da433b93bf26b59f34b3e6b7f760c41d8d127636d020655`; DLL: `8d5b9184204e7e7728965bcf95af5f150f6220dafbfe52fdd6c23e50d47e5258`. CommunalHelper remains 1.25.5, ZIP `44f4fb0b277a4900fd2a555e1a73e661140aa7b2455d3c7776cf420193349e3c`. Complete private package copies and raw receipts are outside tracked source.

Observed host: x86_64 macOS 26.6.2 / 25G83, guest CPU string “Intel Core Processor (Skylake)”, 8 physical / 16 logical guest CPUs, 48 GiB RAM. Process-local Xcode 26.6 / 17F113; iOS/iOS Simulator/tvOS/tvOS Simulator SDKs 26.5; .NET 10.0.302, workload set 10.0.302.0, runtime/compiler packs 10.0.10, host SDKs 8.0.424 and 9.0.317 and the pinned .NET 6 runtime for ILSpy. Local copied SDK files were hash-checked. Full Xcode, GNU Make and Mono remain host prerequisites; they are not redistributed in Git.

Global developer selection remains Command Line Tools. Neither Xcode installation, device pairing, simulators nor system allocator settings was changed. The inherited `MallocNanoZone=0` variable was removed only from benchmark subprocesses and recorded. MSBuild node reuse/server/shared compilation were disabled, `-m:1` and `BuildInParallel=false` retained, and app builds ran serially.

## Fresh native foundations and build-49 gates

The normal pinned native producer path was run in new Morro-owned roots; no old compiled app tree or M1 binaries were imported. These foundations were then verified and staged for the app runs. The separate fresh original-source reference checkout reused those verified native inputs through the supported staging paths, with byte-equality receipts; reference native construction is not counted as a fresh native build.

| Platform | Complete accepted native identity |
| --- | --- |
| ios | `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2` |
| tvos | `6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc` |

Both fresh Morro Everest products independently regenerated their actual closure, compiled profiles, semantic/composition evidence and preflight. Each passed: A **1,309/1,309**, zero blocked/unclassified; B **83/83**, zero missing (77 selected-profile factories plus six separate legacy proofs); C **83/83**, zero blocked/unknown; D complete selected real-map composition, zero blocked/unknown. Census remains 604 raw authored profiles and 336 regression occurrences. These are host/product proofs, not physical gameplay observations.

Exactly three unchanged SJ maps are selected: Beginner lobby, `Bing_Over_Google`, and `snas` (The Squeeze), plus all 18 regressions. The other 125 SJ maps remain excluded. The original 15-bank order, one FMOD Studio system, progression and separate persistence authorities are unchanged. Final packaged-content checks cover all 2,946 mounted files and original map bytes.

| Authority | Revalidated identity on both platforms |
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

Each final app is arm64, fully trimmed/full static LLVM device AOT, with interpreter and JIT disabled. iOS is universal family [1,2], minimum 15; tvOS family [3], minimum 16; compile SDK 26.5. The actual linked assemblies, AOT objects, native images, selected profiles, separate legacy entries, compiler before/after receipts, forbidden surfaces and packaged content were checked. The final packaged Everest payloads were reverified, including 15 rejected platform/compiler/LLVM controls per product, using disposable copies only. All eight new IPAs (four reference, four Morro) match every byte of their corresponding app payload and contain no signing/provisioning material. Nothing was signed ad hoc.

## Artifact identities and comparison

References were built fresh from clean recursive source `34c0b933a4cf2252780ec84e5630847809793eab`, not copied from the old signed products. Outside-tree reference adapters route the obsolete iOS host-only check and tvOS version verifier to their corrected equivalents and make iOS serial flags explicit. The original tracked reference source is unchanged. These are verification-only adapted reference builds, not a claim that every unmodified historical stock wrapper passes on the Intel host.

| Product | Fresh original-source unsigned IPA SHA-256 | Final Morro unsigned IPA SHA-256 |
| --- | --- | --- |
| vanilla-ios | `5b988ec808256d7ead9fccc59b5f6a7a0ce9ad8fdba941ec6a196b6a82e29888` | `8bc874b41b0bc6148dbae0959b239d806b7b44bfb87dfc3044a975ec8afb3404` |
| vanilla-tvos | `429d873d963d478cfc283f50b8ed37cf2359dc9aea01c5068f64925ae0d377b6` | `c59d392f5bfdd2af20e373a6d0bd93d306cb3ec8e426dc44f1f8fdd35f17f0bb` |
| everest49-ios | `694d17224f60b5b3256b1bec7f494359c58d8a5e9f33c64921c1d8099cbd28e6` | `822e9b6eac1d4745e93dc6c55981b60b913d2af8e01e1ba9981f718245dc4840` |
| everest49-tvos | `247d8083d06ad1dc5af351213577d3957a13108ee1e5d5ce4b57cb53095891a3` | `38f65fecafc9e6b96a67be592d6a283f73a2b806801771aab4d724076cb3bf2e` |

| Product | Exact equal payload files | Changed files | Missing / added | Byte-identical IPA |
| --- | --- | --- | --- | --- |
| vanilla-ios | 1254/1303 | 49 | 0 / 0 | NO |
| vanilla-tvos | 1248/1290 | 42 | 0 / 0 | NO |
| everest49-ios | 4202/4257 | 55 | 0 / 0 | NO |
| everest49-tvos | 4199/4251 | 52 | 0 / 0 | NO |

No Content, Graphics, Audio, bank or map payload file differs in these exact comparisons. **The requested exact IPA equality is not established.** ZIP metadata and managed build metadata differ; the new source SHA is truthfully embedded where the normal build emits it. Earlier bounded PE inspection identified timestamps, MVID/debug identifiers, checkout-dependent PDB paths and source informational-version attributes. Final native images also differ in the sections recorded below. Those native differences are not called harmless metadata, and no bytes, sections or members were excluded from the exact comparison.

| Product | Linked assemblies / method bodies inspected | Method-body representation matches | Differing native sections |
| --- | --- | --- | --- |
| vanilla-ios | 40 / 41585 | YES | `__DATA/__data`, `__TEXT/__const`, `__TEXT/__cstring`, `__TEXT/__text` |
| vanilla-tvos | 34 / 41893 | YES | `__DATA/__data`, `__TEXT/__const`, `__TEXT/__cstring`, `__TEXT/__text` |
| everest49-ios | 44 / 49206 | YES | `__DATA/__data`, `__TEXT/__const`, `__TEXT/__cstring`, `__TEXT/__objc_stubs`, `__TEXT/__stubs`, `__TEXT/__text` |
| everest49-tvos | 42 / 50130 | YES | `__DATA/__data`, `__TEXT/__const`, `__TEXT/__cstring`, `__TEXT/__text` |

The supplementary Mono.Cecil diagnostic inspects actual linked AOT inputs, not only IL-stripped packaged DLLs. It compares method-instruction/metadata records and has an owned negative fixture where changing return 42 to return 43 is rejected. It is diagnostic evidence, not a new acceptance normalization or proof of native behavioral equivalence. Complete changed-file/field lists are in the adjacent sanitized results JSON; detailed section, PE and disassembly observations are retained privately. A preliminary vanilla iOS comparison located a small number of differing aligned text words near framework SIMD-search routines; their exact cause remains unresolved. No compiler/output byte repair or source-identity spoofing was attempted.

## Measurements

Times use retained `/usr/bin/time -lp` and real exit statuses. “Whole command” includes preparation, proofs, AOT/link, packaging and verification as performed by that wrapper. Native preparation is separate below; warm validated source/package/SDK/NuGet caches are explicitly reused. No stale compiled app objects or `--reuse-build` were used for the final products.

| Final Morro product | Wall s | User s | System s | Reported max RSS GiB | Free GiB before → after |
| --- | --- | --- | --- | --- | --- |
| morro-vanilla-ios-3 | 283.30 | 392.43 | 100.34 | 3.39 | 829.01 → 821.47 |
| morro-vanilla-tvos-3 | 258.71 | 379.00 | 59.52 | 3.47 | 821.47 → 816.95 |
| morro-everest49-ios-3 | 594.71 | 959.52 | 169.83 | 4.82 | 816.95 → 812.07 |
| morro-everest49-tvos-3 | 612.76 | 931.44 | 228.87 | 4.71 | 812.07 → 806.45 |

| Fresh native phase | Wall s | Exit |
| --- | --- | --- |
| ios-native-fetch-1 | 116.69 | 0 |
| ios-native-build-1 | 33.23 | 0 |
| ios-native-verify-1 | 4.37 | 0 |
| tvos-native-fetch-1 | 413.05 | 0 |
| tvos-native-build-1 | 301.58 | 0 |
| tvos-native-verify-1 | 6.20 | 0 |

| Source / platform | AOT through verification + packaging wall s |
| --- | --- |
| Morro / ios | 314.286 |
| Morro / tvos | 308.308 |
| Reference / ios | 311.488 |
| Reference / tvos | 314.628 |

The last table uses the wrapper’s real before-AOT/after-products timestamps. It does not separate compilation from linking, verification or compression. Nested preparation phase times, reference-command timings and all failed-attempt measurements are retained in adjacent JSON/phase receipts. Command RSS is the reported child-command resource measure, not peak whole-VM or build-service memory. Swap was zero at recorded before/after checkpoints; continuous whole-system memory peaks were not measured. Setup/reference runs had differing cache states and some diagnostic/host checks overlapped; these are diagnostic timings, not a controlled speed comparison. No M1 speedup or worker tuning is claimed.

## Regression, cloud and failed attempts

All 74 portable tests covering the final tooling changes pass, including 11 migration tests. Negative controls cover source-inventory omission/tampering, unsupported graph contamination, export pin/path safety, canonical version bindings, missing/wrong release evidence, and non-executable Python entry-point success/nonzero failure. The actual K-N help entry point is exercised. The 21-command current HOST-B/HOST-C/K-M/K-N suite passed at `ac5ce57716c7a3c425fca558510de7f518f830e4`; its recorded scope includes 724 builder tests, 50 hook-semantics tests, HookGen/direct hooks, 31 QR, 66 protocol, 57 continuity, 21 soft-reload and 103 K-N contract controls. Subsequent changes only concerned tag-alias handling and Python launch, with their own final regression tests. Every final K-N product also reran its mandatory fresh production preflight and proofs. Historical literal-count verifiers retain their historical scope.

Cloud input/orchestration tests: 32 PASS. Export parity and actionlint 1.7.12: PASS. A fresh recursive, intentionally no-tags source checkout at the final `6dc6427` revision passes actual layout, release-ancestry, version and repository checks. The five-file test export is bound to an explicitly non-published example destination and the final SHA. The original private **vanilla tvOS** cloud lane is preserved; no new Everest/iOS cloud lane is claimed. No hosted Actions run occurred. An actual end-to-end hosted run remains unverified until Morro is published and private inputs are configured.

Retained failed attempts exposed obsolete Xamarin/verifier assumptions, the old vanilla version literal, missing release-tag aliases in a clean reference, a missing host-builder restore, generated canonical bin/obj contamination from a prior vanilla reference build, the reference checkout’s missing local .NET 10 capsule, and the new convenience wrapper’s attempt to directly execute a non-executable Python script. Each was corrected within tooling/setup scope and retested. Two early diagnostic-parser attempts also failed and were corrected without changing products. Earlier products at `450fc3b` and `d3544cd`, their AOT evidence, and failure logs were preserved separately. They are not relabeled as final `6dc6427` products. All four final app products were rebuilt after the last source correction.

## Preservation, delivery and next action

Original HOST-A/B/C/K-M/K-N checkouts remain clean at their original revisions with pinned clean recursive submodules. The original build-49 signed iOS and tvOS products remain byte-identical to their preserved hashes (`f460ec047bcf3ca75baa7e77425be5c1fe7a794b37c8fb2ec496a054183b458e` and `f1fd9c39681949ea10d68ce0b9d01c6bd6ab6bda9d4febe60819f7c253778a97`). Original reports, caches, vanilla checkout and user saves were not modified. The task-mounted read-only FMOD input volume was normally detached after copying; no system housekeeping was performed.

| Protected remote ref | Value |
| --- | --- |
| refs/heads/release/v1.0.0-rc.3 | `c8134c8ca7924cf12f48527e714b5242c6024927` |
| refs/heads/tvos-port | `b65bedd20016dc3482d7702d7f0a9707bc2b1479` |
| refs/tags/ios-v0.1.1-rc.1^{} | `27e16b4724d94d3991b99c4795f680fcb0e5830c` |
| refs/tags/v1.0.0-rc.1^{} | `ee52b0868df091746f134d95d4f020f94f23d4fb` |
| refs/tags/v1.0.0-rc.2^{} | `641e86e4ed164cdf93f602ce2f11436449654d6e` |
| v1.0.0-rc.3 tag | ABSENT |

Final source/privacy checks find no tracked proprietary inputs/build artifacts, personal absolute paths or credential patterns. Private inputs, generated source, SDKs, logs, binaries and comparison artifacts are ignored or outside Git. No root push workflow or custom hook exists. There is no `origin`; inherited `upstream` has a disabled push URL and `push.default=nothing`. No push, merge, tag creation, repository/settings change, release, PR or Actions was performed. This is local delivery only.

Signing, installation, physical testing and tuning: **NOT RUN**. Original build-49 physical acceptance remains pending; no automated proof substitutes for the iPhone/Apple TV route/death/retry/save/resume matrix. `IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY` remains unchanged. The M1 signing/install fallback is unaffected.

Reproduce locally with `docs/MORRO_BUILDING.md` and a new run name through `scripts/build-morro-unsigned.py`; supply the intended bundle ID explicitly. A source-only Git clone still requires lawful private inputs and the documented host toolchain. For later cloud publication, choose the actual repository, publish the intended commit explicitly, then use `scripts/export-morro-cloud-builder.py` to bind that repository and full SHA. The example export is not a live build destination.

**Next action:** review this migration and the explicit byte-reproducibility gap before publishing Morro or replacing any installed build. If exact IPA byte identity is a hard requirement, resolving the remaining compiler/metadata reproducibility differences is still required. This report does not authorize integration, publication or the next gameplay stage.

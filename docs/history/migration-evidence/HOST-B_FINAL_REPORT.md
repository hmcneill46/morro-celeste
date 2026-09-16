# HOST-B — host-independent native reproducibility

**HOST_B_READY_FOR_M1_NATIVE_REGRESSION**

Date: 2026-09-09. Repository:
`hmcneill46/celeste-ios`. The Intel VM produced **three independent fresh iOS
native foundations and three independent fresh tvOS native foundations**, all
passing the unchanged complete accepted native identities. All three established
defects are corrected in the feature candidate. App AOT and physical acceptance
are outside this result. HOST-A remains historically **HOST_YELLOW**.

## Revision, delivery and authority

- Accepted build-46 baseline: **`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`**, version 0.1.1 (46).
- Final feature/tooling commit: **`632a451da2f62928404a2408826747aa6fc55b20`**.
- One commit on local and remote **`feature/apple-native-host-determinism`**.
- Remote candidate: [feature branch](https://github.com/hmcneill46/celeste-ios/tree/feature/apple-native-host-determinism).
- Original HOST-A stays on `feature/apple-everest-host-a-build46` at the exact
  accepted SHA; the separate vanilla checkout also stays unchanged.

The builds ran the modified feature working tree before its commit. Per-run
SHA-256 maps bind all six build/verification/inspection tools to the committed
files, and the final candidate's source/test/document digests were checked. These
are builds of the **new tooling candidate**, not an unmodified checkout of the
accepted baseline. Native dependency patches/revisions, compiler/optimization/
architecture/deployment settings, native locks and normalization rules are
unchanged. Game, runtime, Everest, mod/content/map/audio/persistence and app
version authorities are unchanged.

The supplied stdout patch was reviewed and its exact SHA-256 verified before
application in the new checkout only:
`c8b3c4b5ce20aebd9da009bbb0d1c02ef1f7036023829388953376a6942ec747`.
No patch was applied to HOST-A.

## Issue and change ledger

| Item | Correction and evidence | Effect on native products |
| --- | --- | --- |
| A: mixed symbol/diagnostic streams | Both verifiers parse nm stdout only; stderr remains visible and nonzero exits fail. Supplied six stream tests retained. | Verification only |
| B: host-default architecture | Explicit arm64 logical export fingerprint; actual required-symbol and stub checks run independently on every packaged architecture. | Verification only; no slice added/removed |
| C: uninitialized archive-index padding | A bounded standard-library producer validates the archive/index/object definitions and initializes only derived terminal index alignment padding before staged publication and XCFramework packaging. | Actual index padding changes; all Mach-O bytes preserved |
| Additional bounded iOS verification | Detect duplicate platform variants, check stub overlap, force unsigned probes and verify probe architecture/platform/minimum/absence of signature. | Probe/check behavior only; packaged native compilation unchanged |

No expected digest or normalization rule was changed. No archive member was
removed or excluded, and all referenced symbols are preserved. There is no component-specific offset patch, digest search, warning
filter, allocator workaround, installed-tool patch or global override.

## Architecture and index design

The existing normalized export representation now explicitly selects **arm64**
on both Intel and Apple silicon. iOS retains arm64 device and arm64 simulator.
tvOS retains arm64 device and arm64/x86_64 simulator for all six components, plus
MoltenVK's existing device arm64e slice. MoltenVK arm64e receives its own actual
component requirements; other components are not required to have arm64e.
XCFramework metadata must agree with the real architecture set. SDL may use only
same-architecture tvStubs exports, and overlaps are checked in each architecture
where stubs are packaged. Per-architecture evidence remains separate from the
unchanged normalized representation.

The archive producer supports BSD ar with a first little-endian 32-bit
`__.SYMDEF` or `__.SYMDEF SORTED` index and supported 64-bit MH_OBJECT members,
including a big-endian FAT_MAGIC multi-architecture wrapper. Format fields come
from the installed Apple ar/ranlib/fat/Mach-O headers. It checks lengths, bounds,
fat overlap/alignment, member names/order/references, object load commands and
symbol/string tables, terminated index strings and their contiguous referenced
prefix. Index `(symbol, member)` entries must equal the multiset of actual object
definitions, including common symbols. Empty architecture-guarded assembly
objects contribute no symbols. Unsupported formats or ambiguous layouts fail.

For this supported Apple producer layout, the declared string allocation must be
the referenced length rounded to eight bytes. Only its terminal zero-to-seven
alignment bytes are initialized. Every other byte is compared before/after,
including raw archive/fat headers, object names/order, ranlib entries, referenced
strings and complete Mach-O payloads. The output is reparsed before atomic
publication from a flushed temporary file inside the owned build root. Input,
output and report are distinct; escapes, symlinks, hardlinks, foreign ownership
and missing parents are rejected. This assumes a private single-writer build
root, not an adversarial concurrent filesystem service.

The finalizer receives no acceptance hash. It constructs the actual library
before staging and packaging, including a final pass after the existing tvStubs
HUD addition. The unchanged complete hash interpretation and staged-versus-
packaged checks consume those actual bytes. No retained HOST-A/M1 archive is
modified or used as a fresh compiled input.

## Tests and exact reproducibility

**29 portable test methods PASS.** Project-owned byte
fixtures need no Apple tools or third-party binaries. Coverage includes normal
symbols; warnings between partial stdout writes; missing symbols; stderr-only
symbols; nonzero nm exits; preserved warning-free inventories; missing arm64
without fallback; missing x86_64 despite a valid arm64; cross-slice stub failures;
architecture-specific nm failures; overlap rejection; and MoltenVK arm64e's real
required-symbol gate. The tests invoke the production gate functions, not merely
a replacement set-difference assertion.

Producer coverage includes already-zero, all 256 byte patterns, mixed padding,
scribble values, idempotence, complete non-padding preservation, thin/fat/arm64e
paths, empty assembly objects, truncated/invalid tables, invalid offsets/member
references, unterminated or overlapping strings, unsupported formats and safe
output failures. Shortened, removed or changed referenced symbols cannot be
classified as padding. A legitimate changed definition stays changed.

Two further **full-verifier integration negatives PASS**: a copied iOS foundation
and a copied tvOS foundation each receive one deliberately changed Mach-O byte.
Finalization preserves that change, all preceding structural/symbol/link checks
complete, and the actual final accepted identity gate rejects it. The supplied
verified products were rehashed and preserved. Shell syntax, whitespace and
candidate privacy checks pass. Source review was by the root agent; user review
and fresh M1 regression are pending.

| Identity | Required and observed SHA-256 | Fresh runs |
| --- | --- | --- |
| iOS complete set | `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2` | 3/3 EXACT MATCH |
| iOS Theorafile | `f24cdd931a5ae337d681075d6f093afc7ca303c56652d79bc1510276772d8d21` | 3/3 EXACT MATCH |
| tvOS complete set | `6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc` | 3/3 EXACT MATCH |

Each run compares complete normalized JSON to the validated accepted reference.
An independent field/member comparison covers 524 iOS members (514 Mach-O) and
800 tvOS members (781 Mach-O) per pair: **3,972 member comparisons,
including 3,885 identical complete Mach-O payloads and 87 matching index
metadata members** across 87 slices. All member fields and the complete arm64
export inventories match. Only the already accepted terminal source-path suffix
normalization is used for names; no new exclusions exist.

All six builds use the corrected normal fetch/build/verify entry points and
separate unused work/output roots. The cache contains validated public Git
objects for 19 locked revisions, not prior compiled products. Sources, native
objects, archives and XCFrameworks are freshly produced. The source-only cache
was copied into HOST-B's private area and validated before reuse, preserving
HOST-A's cache. No M1 binary, game/FMOD material, old closure or PASS receipt is a
build input.

All archive/member/platform/minimum/architecture, per-slice symbol/stub,
staging/package equality and complete identity checks pass. Twelve fresh device/
simulator force-load probes pass and are unsigned. Minimums remain iOS 15 and
tvOS 16; SDKs remain 26.5. Each tvOS run freshly regenerates its 26-record license
inventory and link-probe interfaces, matching the accepted full-set fields.

| Run | Process-local constructor stress | iOS initialized nonzero bytes | tvOS initialized nonzero bytes | Accepted results |
| --- | --- | ---: | ---: | --- |
| 1 | `MallocNanoZone=0; MallocScribble absent` | 2 | 7 | iOS and tvOS EXACT MATCH |
| 2 | `MallocNanoZone=0; MallocScribble=1` | 24 | 56 | iOS and tvOS EXACT MATCH |
| 3 | `MallocNanoZone=1; MallocScribble=1` | 24 | 56 | iOS and tvOS EXACT MATCH |

The counts are actual nonzero bytes initialized across construction reports,
including intermediate/final tvStubs construction where applicable. All 72
construction receipts were independently re-derived from retained raw fresh
products; stress-run index padding contains actual `aa` scribble bytes before
finalization, and every constructed output digest matches its receipt. Stress
controls are process-local tests, not system changes, required production
settings or worker-performance tuning. Both stress environments pass complete
locks; this is not selection of an occasional matching allocator outcome.

## Host, timings and resource evidence

Intel x86_64 VM; macOS 26.6.2 (25G83); 8 physical/16 logical guest CPUs; 48 GiB
RAM. Guest CPUID reports Intel Core Processor (Skylake); the underlying i9-13900K
P-core allocation is user-supplied. Effective Xcode is **26.6 / 17F113**, selected
process-locally. Both Xcode applications and global Command Line Tools selection
remain unchanged. iOS/tvOS device/simulator SDKs are 26.5; Apple clang is 21.0.0
(`clang-2100.1.1.101`). .NET 10.0.302 and workload set 10.0.302.0, the retained
8.0.424/9.0.317 SDKs, and pinned host tools remain installed and unchanged.

| Phase | Run | Real s | User s | System s | Reported RSS MiB | Free disk GiB, before → after |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| iOS source acquisition | 1 | 10.02 | 0.95 | 0.83 | 114.0 | 1143.66 → 1143.54 |
| iOS compile + packaging | 1 | 30.82 | 11.26 | 4.11 | 132.7 | 1143.50 → 1143.26 |
| iOS complete verification | 1 | 3.90 | 7.39 | 7.18 | 110.5 | 1143.26 → 1143.24 |
| tvOS source acquisition | 1 | 27.74 | 2.79 | 3.11 | 114.7 | 1143.24 → 1142.96 |
| tvOS compile + packaging | 1 | 308.72 | 31.64 | 9.89 | 131.7 | 1142.96 → 1141.65 |
| tvOS complete verification | 1 | 6.28 | 11.50 | 9.99 | 147.6 | 1141.65 → 1141.60 |
| iOS source acquisition | 2 | 5.64 | 1.02 | 0.87 | 122.4 | 1141.61 → 1141.50 |
| iOS compile + packaging | 2 | 35.00 | 12.78 | 4.32 | 269.8 | 1141.50 → 1141.31 |
| iOS complete verification | 2 | 4.07 | 7.52 | 6.71 | 119.4 | 1141.31 → 1141.30 |
| tvOS source acquisition | 2 | 17.87 | 3.00 | 3.19 | 117.2 | 1141.30 → 1140.99 |
| tvOS compile + packaging | 2 | 356.07 | 40.62 | 11.79 | 361.4 | 1140.99 → 1139.67 |
| tvOS complete verification | 2 | 7.25 | 12.24 | 10.89 | 150.1 | 1139.67 → 1139.64 |
| iOS source acquisition | 3 | 12.82 | 1.02 | 0.90 | 123.9 | 1139.64 → 1139.54 |
| iOS compile + packaging | 3 | 34.54 | 12.49 | 4.24 | 279.9 | 1139.54 → 1139.37 |
| iOS complete verification | 3 | 3.97 | 7.44 | 6.52 | 117.3 | 1139.37 → 1139.36 |
| tvOS source acquisition | 3 | 17.43 | 2.92 | 3.10 | 124.1 | 1139.36 → 1139.10 |
| tvOS compile + packaging | 3 | 335.53 | 40.05 | 11.51 | 354.2 | 1139.10 → 1138.00 |
| tvOS complete verification | 3 | 7.04 | 11.90 | 10.34 | 148.9 | 1138.00 → 1137.97 |

Timings use `/usr/bin/time -lp` and retain real exit status. Compilation and
packaging are combined. RSS is the command's reported figure, not whole-VM or
aggregate Xcode build-service peak memory. All 36 successful-phase
before/after swap checkpoints show **0 MiB used**. Disk is volume-wide free
space. File caches were not flushed; network bytes were not separately metered.
Small review/negative checks overlapped parts of native compilation; this is not
an isolated speed benchmark. No M1 speedup or app AOT performance claim is made.
Bootstrap/tool installation was not rerun; inventory, cache-copy and initial
submodule setup were not separately timed.

Two setup/review issues are retained explicitly. Legacy recursive `git://` URLs
timed out; a process-local HTTPS transport rewrite completed the pinned
submodules without changing tracked URLs or global configuration. An initial
new SDL mirror download was stopped after 298.12 seconds (exit 1) in favor of the
validated source-only cache; its incomplete cache/log remains private. Retried
acquisition passed. A portable fixture initially used macOS's noncanonical temp
alias; correcting the fixture to use the canonical owned root made its intended
positive case pass. No native lock mismatch or compiler-output correction was
needed after the three production fixes.

## Reference provenance, preservation and scope

Transferred ZIP SHA-256 remains
`50271049e4897b3348cac544fc31422503fdcdd3f0418673ac8e54a27f9053f3`.
All 109 SHA inventory entries and all 108 content-file sizes/digests validate.
The metadata reference binds all 11 component identities to the accepted native
locks. Its README describes freshly inspected retained accepted M1 host-staging
archives, not a new build or physical PASS. Original compiler command logs,
response files/source-state receipts, pre-staging roots and historical detailed
tvOS probe/license evidence remain unavailable. Current M1 inspection tools do
not establish original compiler invocations. HOST-B executes its own complete
checks and does not transfer those historical claims to VM products.

Preservation audit PASS: **74,930 original evidence/native files,
7,618,148,806 bytes**, unchanged SHA-256, size, modification time and
mode. This includes original/continuation/diagnosis reports, the validated M1
reference metadata, closure evidence and both retained Intel iOS runs plus tvOS.
The original reports and HOST-A's historical result are unchanged. New raw logs,
archives, XCFrameworks and diagnostic evidence are private and verified ignored
or outside Git. Only project-owned tooling/tests and sanitized documentation
are committed.

Original HOST-A/vanilla HEADs, branches, local refs, tracked files and recursive
submodules are unchanged. The feature checkout and its eight pinned recursive
submodules are clean after commit. Diagnostic K-G/K-I/K-K commit objects remain
absent. All pre-existing remote refs are unchanged; only the requested feature
branch was added. Protected refs remain:

| Ref | Value |
| --- | --- |
| `ios-v0.1.1-rc.1^{}` | `27e16b4724d94d3991b99c4795f680fcb0e5830c` |
| `v1.0.0-rc.1^{}` | `ee52b0868df091746f134d95d4f020f94f23d4fb` |
| `v1.0.0-rc.2^{}` | `641e86e4ed164cdf93f602ce2f11436449654d6e` |
| `release/v1.0.0-rc.3` | `c8134c8ca7924cf12f48527e714b5242c6024927` |
| `v1.0.0-rc.3` tag | ABSENT |
| `origin/tvos-port` | `be8546d4ae411cb491a0c3bbc4e5343ebf9c9651` |

Push behavior was inspected: the candidate/default baseline have no workflow
files, GitHub reports zero workflows, and no executable push hooks or alternate
push destination are configured. The push was limited to the user's feature
branch with tag following and submodule pushes disabled. Actions settings were
not changed. No Actions, PR, merge, force-push, upstream push, tag or release was
performed. App AOT, signing, pairing/device operations, installation, physical
testing, worker tuning, gameplay expansion and K-M: **NOT RUN**.

## Handoff and remaining work

The reproducible M1 native-only procedure is committed in
`docs/APPLE_NATIVE_REPRODUCIBILITY.md`: separate checkout at the delivered feature
SHA; exact existing tools; portable tests; three fresh serial platform pairs with
the recorded allocator stress conditions; complete reference/lock comparisons;
and full-verifier changed-payload negatives. No private input is required for
that native-only procedure beyond the retained validated reference metadata.

The VM has demonstrated **native foundation construction with this candidate**.
Fresh M1 native regression and user review remain pending. It has not yet
qualified as a full game AOT build host. The M1 remains the signing/install
fallback. No HOST-B VM native blocker remains; M1 regression, review/manual
integration and later complete unsigned app qualification remain separate work.
Later app qualification must record this new tooling revision separately from
accepted build 46 and revalidate the retained shared/managed/content/registry
identities and gates A–D. HOST-A's retained three-run closure evidence is preserved,
not rerun or relabeled as new product evidence here.

**Exact next recommended action:** review commit `632a451da2f62928404a2408826747aa6fc55b20` and run the documented
M1 native-only regression in an isolated checkout of that exact SHA. Keep
`tvos-port` unchanged until the user's review/manual-fast-forward decision.
Do not start app AOT yet. This next stage has not been started.

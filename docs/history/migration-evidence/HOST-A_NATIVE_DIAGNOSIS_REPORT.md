**HOST-A native-equivalence diagnosis — HOST_YELLOW**

Date: 2026-09-09. Diagnostic observation began 2026-09-09T07:52:17.854928+00:00; final preservation audit 2026-09-09T08:22:20.134226+00:00. Repository `hmcneill46/celeste-ios`, branch `feature/apple-everest-host-a-build46`, exact frozen HEAD **`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`**. No app AOT was launched.

The transferred reference validates against the accepted native locks. **Every one of the 1,295 Mach-O member payload hashes matches the corresponding M1 reference** across the 22 first-run iOS/tvOS archives. The complete manifests expose three distinct problems: stderr corrupting symbol parsing, host-dependent simulator architecture selection by `nm`, and allocator-dependent padding in archive indexes. The iOS difference is exactly one unreferenced index-padding byte per Theorafile variant, reproduced in both retained Intel runs. No differing compiled code/data/relocation member was found.

The minimal stdout-only patch and tests are ready for separate review. It lets tvOS reach all remaining native checks and produce a complete manifest, but **the final native-set identity still fails**. Neither corrected diagnostic nor an occasional matching repack is baseline acceptance. HOST_YELLOW remains in force until actual full-product qualification completes.

**Reference inventory, provenance and boundaries**

The supplied Downloads ZIP matches the user-provided SHA-256:

`50271049e4897b3348cac544fc31422503fdcdd3f0418673ac8e54a27f9053f3`

The requested unpacked Downloads directory was absent. The verified ZIP was safely extracted into a new private outside-tree diagnostic directory. There are **110 files**: 109 entries covered by `SHA256SUMS.txt`, with all 108 content-file sizes/hashes in `inventory.json` also matching. Names are unique and safely contained; the transfer contains only Markdown, JSON and text. No M1 native binary was supplied or copied into VM build inputs.

The reference README states that genuine accepted host-staged archives survive and were freshly inspected at build 46, without reconstruction. I treated that as provenance evidence, not an instruction or an independent physical/build PASS. Its ordered archive/member and export records bind back to all **11 recomputed component hashes** and these exact identities:

| Reference | Recomputed and accepted SHA-256 |
| --- | --- |
| iOS full native set | `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2` |
| iOS Theorafile component | `f24cdd931a5ae337d681075d6f093afc7ca303c56652d79bc1510276772d8d21` |
| tvOS full native set | `6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc` |

All 11 recorded inspection/recipe/lock file digests match the frozen checkout. Four transferred locks are structurally identical to the tracked locks; three were reformatted during export and therefore have different file serialization, while the symbol-expectation file is byte-identical. This formatting distinction was recorded rather than treating exported bytes as the tracked-file digests. All component canonical hashes, member ordering and export fingerprints match the accepted values without changing canonicalization.

The M1 export describes 22 archives, 29 slices and 1,324 members. Its current inspection environment is Apple M1, macOS 26.3 (25D5087f), Xcode 26.6 / 17F113, SDKs 26.5, clang 21.0.0 (`clang-2100.1.1.101`). These are **current inspection tools**, not recovered original compiler invocations. The README explicitly lacks original compiler logs/response files/source-state receipts, original pre-staging artifact roots, the full historical tvOS link/license reports, and an independently recorded original clang build number.

The preserved tvOS `licenseInventorySha256` and `linkProbeInterfaces` are historical fields in the accepted set hash. The reference does not claim fresh execution of those old probes. The VM diagnostic below freshly regenerated those fields and executed its own probes. The supplied evidence is sufficient for the archive/component comparison; no additional M1 file is needed to identify the iOS byte difference. It does not establish new physical acceptance or reconstruct missing historical execution evidence.

**Exact iOS difference, both Intel runs**

Both complete normalized Intel manifests are identical. Against the M1, each differs in precisely four fields: the two following member payload digests, the derived Theorafile component digest, and the derived full-set digest. Every other normalized field matches.

| Exact field | M1 | Both Intel runs |
| --- | --- | --- |
| `components.Theorafile.variants.device.memberFingerprint[0][2]` | `645352bd09e1f1507637715978dbe07647696f12de9a86f3421c8476719eebab` | `c25d1ae467cb710eebf5ff593e6b3711abcaddde07f1969f29bd3f628b00dd2f` |
| `components.Theorafile.variants.simulator.memberFingerprint[0][2]` | `a1435eb1672d71d970073b25eeaba24479c91e21abbc6faefb79d7757ce6ec4c` | `3ab4a0ece80b99e283655965dac72e257e6cb2f19dc0207357750b28338a7761` |
| `components.Theorafile.logicalSha256` | `f24cdd931a5ae337d681075d6f093afc7ca303c56652d79bc1510276772d8d21` | `f43ee25b42262cc8b7ce7d998d3456ce374c59e91b4afdf0f4ecd81fad48c1c9` |
| `logicalSetSha256` | `9fb302d221180e39f270ea5ebf48e18433b67bd0a40943c042a227fe0f8ad6a2` | `b5fbcf54aded9d72e9a16c549fdb5c11bccb6ed7a1e03a059cbefa6b83d1a56d` |

The differing member is **member 1, `__.SYMDEF SORTED`**, type `archive-metadata`, in both arm64 device/simulator variants. All 37 Mach-O members per variant match in both Intel runs: 148 individual Intel Theorafile member comparisons against the two M1 variants. There are no differing Mach-O members requiring a section-level explanation. Complete payload hashes cover their code, data, load commands, symbol/string tables and relocation bytes; the supplied supplemental fingerprints were not substituted for the stronger whole-payload comparison.

The supplemental archive comparison also matches all member names/order, one-based indexes, header offsets/hashes, extended-name storage, alignment padding, stored/payload sizes, every ranlib entry, referenced-member resolution, and export count/fingerprint. Each Theorafile variant exports 293 symbols with fingerprint `4f2c0738fea9d7d1fdc733d7c4c0c7f101db159bd6a8a6e3a3182d8862c45c23`. SDK 26.5, platform and minimum iOS 15 are identical.

The index has 293 entries, a 2,344-byte ranlib table and a 5,608-byte string table. Referenced strings, including terminators, cover exactly 5,607 bytes. **Only its final, unreferenced byte differs:**

| Location, zero-based byte offset | M1 | Both Intel runs, both variants |
| --- | --- | --- |
| String table offset **5607** | `00` | `bf` |
| Index member payload offset **7959** | `00` | `bf` |
| Thin archive offset **8047** | `00` | `bf` |

All referenced string offsets and name hashes match. A bounded hash-only reconstruction over the one unknown byte uniquely identifies M1's value as `00` and explains the complete accepted metadata digest in each variant. **No archive was edited, stripped, normalized differently or accepted by this calculation.** Exact difference records remain in `ios-complete-manifest-differences.json`, `ios-archive-field-differences.json` and `ios-exact-byte-diagnosis.json`.

Theorafile's 139 pinned source files match the M1 source inventory in both iOS input roots and the tvOS input root, at `0c5504658a3108919e53b625287786a87529de42`. Build-46 recipes/settings match; original M1 per-command flags remain unavailable. The matching complete Mach-O payloads provide no evidence of differing emitted compilation, generated configuration, embedded object metadata, code, data or relocations.

**Established archive-writer cause and bounded controls**

The VM uses Xcode's **`cctools_ld-1267` libtool**. The diagnostic process inherits `MallocNanoZone=0`. Replaying the exact retained iOS device libtool command, with only output/dependency-report destinations moved outside the tree, reproduced the original `bf` byte. No source was compiled and every original Mach-O member was retained unchanged.

| Separately labeled control | Observed index-padding result | Interpretation |
| --- | --- | --- |
| Exact retained recipe replay | `bf`, original rejected digest | Reproduces the issue at archive construction |
| Same replay with `MallocScribble=1` | `aa`, different rejected digest | Padding follows the allocator's allocated-memory scribble marker |
| Same replay with `MallocNanoZone=1` | `00`, accepted metadata digest | This particular allocator control matches |
| Same replay with inherited `MallocNanoZone` removed | `00`, accepted metadata digest | This particular clean-environment replay matches |
| `MallocPreScribble=1` or `MallocZeroOnFree=1` controls | Original `bf` persists | Neither establishes a correction |
| Repack retained objects directly, varying allocator environment | Mixed matches/failures | Removing the environment override is not a reliable general solution |
| Existing `ar rcs` constructor, normal and scribbling controls | Theorafile still fails; scribbling changes padding | Replacing libtool with this installed ar is not an established fix |

These controls establish **allocator-dependent, uninitialized padding being serialized into the archive index**. They do not identify a source line inside the installed Apple tool or establish original M1 allocator settings. All experiment outputs are separate; no matching experiment was selected as baseline evidence. No compiler flags or native machine-code payloads changed in the experiments.

This is an archive-bookkeeping difference with matching compiled payload hashes. It still violates the unchanged lock. A reliable producer must initialize the complete index string-table allocation, including its alignment padding, when constructing the archive. No tested environment workaround or existing alternate ar invocation proved reliable. A producer/build-recipe correction remains necessary; there is **no review-ready deterministic producer patch** in this task. Ignoring that member, rewriting expected hashes, stripping padding after the fact, or an allowlist would change acceptance policy and was not done.

**Symbol-stream correction and tests**

The original iOS `exports()` and tvOS `exported_symbols()` pass `nm` through helpers merging stderr into stdout. A warning can split a symbol name before the parser sees a complete line. Filtering warning lines after the merge cannot recover that symbol reliably.

The minimal outside-tree proposal changes only those two capture sites to `subprocess.check_output(..., text=True)`: stdout alone supplies symbols, stderr is inherited for diagnostic logging, and nonzero command failures remain `CalledProcessError`. SDK/vtool helpers, symbol rules, architecture selection, native construction, expected hashes and acceptance checks remain unchanged.

| Required regression | Corrected candidate, both verifiers |
| --- | --- |
| Normal valid symbol output | PASS |
| Warning between partial stdout writes | PASS; symbol preserved, warning retained on stderr |
| Genuinely missing required symbol | PASS; requirement remains unsatisfied and fails |
| Symbol appearing only on stderr | PASS; cannot satisfy the requirement |
| Nonzero nm exit | PASS; failure and stderr retained |
| Warning-free accepted inventory | PASS; unchanged symbol sets |

The six portable test methods run against both verifiers. The frozen originals produce six subtest failures in the interleaving/stderr/error-diagnostic cases; the candidate produces zero failures/errors. **All 22 full warning-free M1 inventories also remain identical under the original and corrected parser versions.** Two additional tests run through the actual full tvOS verifier with unchanged copied archives and controlled nm output: removing `SDL_EnclosePoints`, and emitting it only on stderr, both fail the real required-symbol gate with exit 1 before normalized-manifest generation. These are deliberate negative-test successes, not product failures.

`native-symbol-streams.patch` includes the two minimal changes and `tests/test_native_symbol_streams.py`. It passed `git apply --check` at frozen HEAD and was **not applied**. Patch SHA-256:

`c8b3c4b5ce20aebd9da009bbb0d1c02ef1f7036023829388953376a6942ec747`

Portable review command, on a separately authorized feature branch: `python3 -m unittest discover -s tests -p test_native_symbol_streams.py -v`. The tests require no Apple toolchain. Private integration/reference-dependent evidence remains outside the proposed patch.

**Complete tvOS diagnostic results and newly exposed differences**

The original unmodified verifier was reproduced using copied retained archives. It exited 1 for a corrupted SDL2 simulator symbol inventory; in this copied-root run it reported `SDL_SetTextureAlphaMod` missing. The earlier retained failure named `SDL_EnclosePoints` and `SDL_RenderFillRectF`. The particular name affected changes with mixed-stream output; original archive bytes remain unchanged.

The stdout-only candidate then completed all remaining checks on unchanged compiled archives: all six XCFrameworks, 12 archive variants/19 architecture slices and all members; staged/archive equality; architecture/platform/minimum checks; required exports and stub overlap; both complete force-load link probes; unsigned probe checks; and all 26 license-inventory records. Fresh device and simulator probes are arm64, TVOS/TVOSSIMULATOR, minimum 16, and unsigned. Complete detailed and normalized manifests were generated. Dependencies, symbol-expectation hash, license inventory hash and link-probe interface fields equal the accepted M1 manifest.

| Run | Full native-set result |
| --- | --- |
| Required accepted identity | `6286e0545b32e9c56732955d4cf816ed8f5dc0d816ab610dd9fe1752090a01fc` |
| Stdout-only candidate, unchanged archives | **`86700a778b04c897044abd255734ddd279014ed1e94c14fb924089796d61315c` — MISMATCH, exit 1** |
| Separate explicit-arm64 symbol-query control, unchanged archives | **`d4c8f30c69892cd433055e397bc86c134ca30774608a54593a89b27d61a7cc2d` — MISMATCH, exit 1** |

The initial candidate differs in FAudio, SDL2, MoltenVK and Theorafile. FNA3D and tvStubs match. Exact component fields are retained in `tvos-complete-component-differences.json` and full-set fields in `tvos-complete-manifest-differences.json`.

Three simulator export fingerprints are host-dependent because `nm` has no explicit architecture argument. The VM's default inventory matches its x86_64 slice, while every M1 exported inventory matches explicit arm64:

| Component | Accepted/explicit arm64 symbol count | Intel default/x86_64 count |
| --- | ---: | ---: |
| SDL2 | 1449 | 1487 |
| FAudio | 353 | 354 |
| MoltenVK | 508 | 536 |

An independently labeled explicit-arm64 diagnostic removes precisely these inventory differences. It leaves only the archive-index discrepancies, and still fails the exact native-set comparison. **No archive architecture/member was removed.** Independently, all 19 component/variant/architecture combinations pass the required-symbol checks, including both simulator architectures and MoltenVK arm64e, with no stub overlap. This control establishes the measurement cause; it is not silently included in the minimal production proposal. A future reviewed verifier should make the accepted fingerprint architecture explicit and preserve required-symbol validation for every packaged slice.

Remaining tvOS index differences are:

| Component / variant / slice | Differing index bytes | Other payload result |
| --- | --- | --- |
| Theorafile device arm64 | String-table offset 5607: `bf` versus accepted `00` | All 37 Mach-O members, every ranlib entry and reference match |
| Theorafile simulator x86_64 | Same one-byte padding difference | All 37 Mach-O members and index references match |
| Theorafile simulator arm64 | Same one-byte padding difference | All 37 Mach-O members and index references match |
| MoltenVK simulator x86_64 | Unreferenced string-table padding range 17425–17431 contains nonzero bytes; accepted digest is explained by all-zero padding | Its complete Mach-O payload hash matches |

The MoltenVK arm64 simulator index already matches. Hash-only padding analysis explains each full rejected metadata digest against the accepted reference, without modifying an archive. The broader comparison covers **1,295 identical Mach-O payloads and 29 metadata members, with exactly six metadata-payload differences** across iOS/tvOS. Fifty raw path-derived object-name suffixes differ in other archives; the already accepted terminal-path-suffix canonicalization makes their normalized names identical. That existing rule was retained exactly, with no new ignored fields or members.

**Proposed changes, remaining blockers and scope**

| Item | Readiness / effect |
| --- | --- |
| Stdout-only capture patch with six portable tests | Review-ready; verification only; changes no emitted native code or archives |
| Explicit target architecture for logical export fingerprints, with all-slice validation | Cause established and separately tested; needs dedicated feature-branch design/review; verification only |
| Deterministic archive-index construction | Required to retain the exact locks; producer padding must be initialized; no reliable recipe patch established. All bounded repacks retained compiled payloads unchanged. |
| Full app AOT/product qualification | NOT RUN; remains blocked by native acceptance and unreviewed corrections |

The reference's missing historical compiler/probe records are documented provenance limits. The remaining actionable blockers are host-independent symbol measurement and deterministic index production, followed by actual fresh iOS/tvOS app AOT and final product checks. No source change, expected-hash adjustment or selective PASS can substitute for those results.

The VM remained x86_64 macOS 26.6.2 (25G83), 8 physical/16 logical guest CPUs, 48 GiB RAM. Effective Xcode remained 26.6 / 17F113 using `DEVELOPER_DIR=/Applications/Xcode-26.6.app/Contents/Developer`; all four Apple SDKs remain 26.5, with .NET SDK 10.0.302/workload set 10.0.302.0 unchanged. For this diagnostic task, global developer selection was Command Line Tools at entry and exit; it was never changed. Both Xcode applications retain their recorded Info.plist digests/timestamps/inodes. The current selector observation is separate from the historical continuation report.

| Instrumented command | Exit | Real s | User s | System s | Reported RSS MiB |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original verifier, copied retained archives | 1 | 5.82 | 10.65 | 10.20 | 97.6 |
| Stdout-only candidate, complete tvOS checks + exact comparison | 1 | 6.18 | 11.00 | 10.12 | 141.6 |
| Separate explicit-arm64 diagnostic, complete checks + exact comparison | 1 | 6.66 | 11.62 | 11.10 | 144.5 |
| Preservation audit, selector probe conflated process/global | 1 | 18.64 | 13.60 | 4.28 | 211.3 |
| Preservation audit, corrected read-only global-selector probe | 0 | 18.77 | 13.62 | 4.32 | 214.7 |

These command timings use `/usr/bin/time -lp`, with actual exits retained in private logs. RSS is reported command-level RSS, not whole-VM peak RAM. Other small diagnostic controls were not separately benchmarked. Final free volume space is approximately **1144.56 GiB**, with swap **0 MiB used**. This is not an AOT performance comparison or worker-tuning result.

The first final-audit failure was an outside-tree measurement error: `xcode-select -p` honored process-local `DEVELOPER_DIR`, so it was compared incorrectly with the global baseline. The corrected read-only global probe unsets that variable only for its query and confirms the unchanged Command Line Tools selection. No global selector or repository verifier was changed.

Preservation audit PASS: **72,601 original evidence/native-output files, 7,029,873,736 bytes**, with identical content hashes, sizes, modification times and modes. The read-only external FMOD mount was not traversed as an evidence output. Original HOST-A reports and diagnostic JSON files remain intact; the old report hashes are `64eb4df76759bf19ad1f812d6480113bc0593ebff3b4629a90903593239074c6` and `8f2cafeb259193d17f66a9556538a2a1b2b3789227954c19b3a3f414a244fe43`. The retained four closure identities, three-run determinism and gates A–D remain intact with their preserved evidence.

HEAD/branch, tracked files, all eight recursive submodules, local/public refs, protected release refs, and the existing vanilla checkout are unchanged and clean. Diagnostic K-G/K-I/K-K objects were not imported. New reference metadata, copied diagnostic outputs, experiments, patch/tests and logs live privately outside Git. This report contains no private absolute paths, credentials, device/team identities, game/FMOD material, native binaries, raw disassembly or generated game source.

Commits, merges, pushes, tags, Actions, signing, device operations, worker/performance tuning, Stage 25K-M and app AOT: **NOT RUN**. Process-local allocator controls were labeled diagnostic experiments, not baseline tuning. No proposed patch was applied to HOST-A. No historical physical PASS was transferred. The M1 remains the signing/install fallback.

**Exact next recommended action:** Start a dedicated feature-branch review of `native-symbol-streams.patch`, with separate reviewed work for explicit architecture validation and deterministic archive-index production. Use the normal review/manual-fast-forward workflow; keep HOST-A frozen and its locks unchanged until those corrections have an approved path and fresh native identities pass. Do not resume app AOT yet. This next task has not been started.

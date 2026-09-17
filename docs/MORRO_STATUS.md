# Morro progress and evidence

Morro is the standalone [Celeste Apple full-AOT project](https://github.com/hmcneill46/morro-celeste).
It contains both vanilla products and the finite static-Everest pipeline.
The companion [Cabrillo](https://github.com/hmcneill46/cabrillo-celeste) owns its
separate JIT/mod-loader work.

**The Squeeze iPhone stage is closed within the owner's tested scope on
17 September 2026.** The [closeout report](testing/MORRO_SQUEEZE_CLOSEOUT.md)
and [sanitized results](testing/MORRO_SQUEEZE_CLOSEOUT.json) separate each product's
evidence and retain the untested scope. Apple TV and static-Everest iPad remain
deferred; this is not the historical combined iPhone/tvOS GREEN verdict.

The [build-50 deployment renewal](testing/MORRO_SQUEEZE_BUILD50.md), at
`483bca5be2967cf12041d24bd8b875a34d634dba`, passed signed-product checks and
reported iPhone play: The Squeeze route, ordinary and silver berries, saved
completion, cold resumes, Bing and touch/controller input. The 73/73 factory
lifecycle sweep passed its dispatch scope. The owner found the lobby card
remaining above Pause. The [build-51 correction](testing/MORRO_LOBBY_PAUSE_BUILD51.md),
at `28a336f4db6bbdd32af52b830005e19f167ea1d2`, passed fresh signed-product checks,
preserved saved data through installation, and received the focused physical
confirmation: Pause closes the card, Resume/reopening works, and silver remains
visible. Build-50 route and factory results are not transferred to build 51.

The four unsigned migration products at
`6dc6427d1bdeab1a92987a4a1936235e48b7266b` retain their original build/product
PASS. Original build-49 products remain physically unaccepted historical
products. Exact IPA byte reproduction remains unresolved. Source publication
does not grant release approval.

## What a clone contains

- Modern iOS/tvOS host source, shared controls/persistence, native producers,
  source-generation tools, static-Everest runtime/compatibility and verification.
- Exact dependency/package/native/content authorities and pinned recursive FNA
  submodules, plus acquisition tooling for the public dependencies.
- Owned regression/negative tests, local builders, the private vanilla tvOS cloud
  template, build/install guidance and the complete modern development history.
- Original Git ancestry and authorship, license and credits, including RoootTheFox.
- Sanitized stage and migration reports with their actual source/product hashes.

A clone does not redistribute Celeste, FMOD, mod ZIP/DLL/map/bank binaries, Apple
SDKs, local .NET installations, generated source, apps/IPAs, saves, credentials or
signing material. Builders supply the documented toolchain and their own lawful
inputs; exact public downloads are validated again. Nothing requires an old
private project checkout. See [Building Morro](MORRO_BUILDING.md).

## Milestones and their authorities

| Milestone | Source | Scope and evidence |
| --- | --- | --- |
| Accepted build 46 / K-L | `be8546d4ae411cb491a0c3bbc4e5343ebf9c9651` | Original Beginner lobby + Bing, physically accepted on iPhone/Apple TV; [K-L report](history/stages/APPLE_EVEREST_FIRST_SJ_SLICE_OUTCOME_STAGE25KL_REPORT.md) and [HOST-A handoff](history/migration-evidence/HOST-A_FINAL_REPORT.md). |
| HOST-A diagnosis | Original build-46 source | Preserved [continuation](history/migration-evidence/HOST-A_CONTINUATION_REPORT.md) and [native diagnosis](history/migration-evidence/HOST-A_NATIVE_DIAGNOSIS_REPORT.md); historical YELLOW is unchanged. |
| HOST-B | `632a451da2f62928404a2408826747aa6fc55b20` | Native symbol-stream, architecture and deterministic-index corrections; [report](history/migration-evidence/HOST-B_FINAL_REPORT.md). M1 regression is external reviewed evidence. |
| HOST-C | `b4997fa49f4f8b82f6f2984fe9047396e401ea48` | Qualified Intel serial unsigned full-app path and strict host/signing provenance; [report](history/migration-evidence/HOST-C_FINAL_REPORT.md). |
| K-M | `b65bedd20016dc3482d7702d7f0a9707bc2b1479` | Complete Beginner expansion audit and decision; [report](history/migration-evidence/STAGE25KM_FINAL_REPORT.md). Audit readiness is not gameplay acceptance. |
| K-N build 49 | `34c0b933a4cf2252780ec84e5630847809793eab` | Adds unchanged The Squeeze, bounded mechanisms and expanded proofs; [report](history/migration-evidence/STAGE25KN_FINAL_REPORT.md). [Build-47 failure](history/migration-evidence/STAGE25KN_BUILD47_PHYSICAL_FAILURE_REPORT.md) remains separately recorded; original build-49 physical acceptance was never completed. |
| Morro migration | `6dc6427d1bdeab1a92987a4a1936235e48b7266b` | All four fresh unsigned products and applicable product gates PASS; [full report](history/morro-migration/MORRO_MIGRATION_REPORT.md), [results](history/morro-migration/MORRO_MIGRATION_RESULTS.json). Exact IPA equality was not achieved. |
| Source publication | Documentation/ignore-rule descendant of `6dc6427` | Adds this handoff and tracked sanitized evidence. Production code, toolchain/acceptance locks and build number remain unchanged. Existing AOT receipts still name `6dc6427`; they are not rebound to this later commit. |
| Morro build 50 | `483bca5be2967cf12041d24bd8b875a34d634dba` | Renewed iPhone deployment and scoped physical acceptance; [closeout](testing/MORRO_SQUEEZE_CLOSEOUT.md). Pause overlap found; Apple TV deferred. |
| Morro build 51 | `28a336f4db6bbdd32af52b830005e19f167ea1d2` | Fresh signed iPhone product, save-preserving install and owner-confirmed pause fix; [correction](testing/MORRO_LOBBY_PAUSE_BUILD51.md). Later closeout documentation does not change this tested source. |

## Current static-Everest selection

Exactly these unchanged Strawberry Jam 1.0.12 SIDs are selected:

- `StrawberryJam2021/0-Lobbies/1-Beginner`
- `StrawberryJam2021/1-Beginner/Bing_Over_Google`
- `StrawberryJam2021/1-Beginner/snas` — The Squeeze

The 18 existing regression maps remain: 21 custom/regression maps total, with
125 other original SJ maps excluded. The 15-bank order and separate vanilla,
module and AEVPSV1 persistence authorities remain unchanged. No general mod
loader, whole-Beginner or full-SJ support is claimed.

The current iPhone preparation revalidated A 1,309/1,309 occurrences, B 83/83 compiled
registrations (77 selected profiles plus six separate legacy proofs), C 83/83
semantic closures and D real composition, all with zero blocked/unknown/missing
requirements. Actual linked assemblies, AOT objects, native images and packaged
content were verified. The [frozen K-N identities](../apple-everest/sj-snas-identities-stage25kn.json)
bind the current build-51 product. Its reviewed pause correction changes only
the managed, shared, semantic and composition logical identities; content,
registry, progression, collab and audio identities remain unchanged. Historical
products retain the authorities recorded at their own source revisions.

The preserved migration comparison has the same content and inspected linked method bodies
as fresh original-source reference builds, but differing IPA bytes. Build
metadata and generated native differences are documented; unresolved native
differences are not dismissed as harmless metadata. No hash normalization or
acceptance rule was relaxed.

## Remaining work and operating boundaries

- Continue Beginner development in playable batches by grouping shared missing
  mechanics and exact authored profiles from the K-M audit. Only Bing and The
  Squeeze are currently enabled; no other map was added during this closeout.
- Use exploratory play and focused bug/save/restart checks for new candidates.
  The owner discontinued the exhaustive manual fixture checklist. Preserve
  untested scope without asking for that checklist again. Apple TV remains
  deferred until the owner resumes it.
- Resolve byte-level compiler/metadata reproducibility if exact IPA byte equality
  is required. All existing logical/native locks continue to apply.
- The private vanilla tvOS cloud workflow passed local syntax, orchestration,
  input/privacy and clean-source tests. A hosted Morro Actions run has not been
  performed; publication does not claim it has.

`IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY` remains in effect for the
static-Everest candidate. Universal metadata remains validated; modern-device
quality is not reduced for the old iPad. The M1 remains the signing/install
fallback. The closeout adds documentation and evidence only: no runtime or
content change, rebuild, device operation, merge, push, Actions run or release.

The public `main` branch is for the new Morro source. Old protected release refs
remain in the original repository and are not pushed as Morro tags/releases.
Generated/private roots and binary package/signing extensions are ignored.
Historical raw logs and products stay private; their sanitized outcomes and
hashes remain in this repository.

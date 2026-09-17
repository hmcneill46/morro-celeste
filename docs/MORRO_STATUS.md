# Morro progress and evidence

Morro is the standalone [Celeste Apple full-AOT project](https://github.com/hmcneill46/morro-celeste).
It contains both vanilla products and the finite static-Everest build-49 pipeline.
The companion [Cabrillo](https://github.com/hmcneill46/cabrillo-celeste) owns its
separate JIT/mod-loader work.

**Four fresh unsigned migration products passed their build/product checks:**
vanilla iOS, vanilla tvOS, Everest build-49 iOS and Everest build-49 tvOS.
The exact tested source is `6dc6427d1bdeab1a92987a4a1936235e48b7266b`.
**Exact IPA byte reproduction remains unresolved, and build-49 physical acceptance
is still pending.** Publishing source grants neither physical PASS nor release
approval.

The pending acceptance is continuing with the [build-50 deployment renewal
candidate](testing/MORRO_SQUEEZE_BUILD50.md). The original build-49 profiles
expired on 16 September 2026. Build 50 requires fresh signed products and its
own physical matrix; the historical products and observations remain separate.

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
| K-N build 49 | `34c0b933a4cf2252780ec84e5630847809793eab` | Adds unchanged The Squeeze, bounded mechanisms and expanded proofs; [report](history/migration-evidence/STAGE25KN_FINAL_REPORT.md). [Build-47 failure](history/migration-evidence/STAGE25KN_BUILD47_PHYSICAL_FAILURE_REPORT.md) remains separately recorded; build 49 still needs its exact device matrix. |
| Morro migration | `6dc6427d1bdeab1a92987a4a1936235e48b7266b` | All four fresh unsigned products and applicable product gates PASS; [full report](history/morro-migration/MORRO_MIGRATION_REPORT.md), [results](history/morro-migration/MORRO_MIGRATION_RESULTS.json). Exact IPA equality was not achieved. |
| Source publication | Documentation/ignore-rule descendant of `6dc6427` | Adds this handoff and tracked sanitized evidence. Production code, toolchain/acceptance locks and build number remain unchanged. Existing AOT receipts still name `6dc6427`; they are not rebound to this later commit. |

## Current static-Everest selection

Exactly these unchanged Strawberry Jam 1.0.12 SIDs are selected:

- `StrawberryJam2021/0-Lobbies/1-Beginner`
- `StrawberryJam2021/1-Beginner/Bing_Over_Google`
- `StrawberryJam2021/1-Beginner/snas` — The Squeeze

The 18 existing regression maps remain: 21 custom/regression maps total, with
125 other original SJ maps excluded. The 15-bank order and separate vanilla,
module and AEVPSV1 persistence authorities remain unchanged. No general mod
loader, whole-Beginner or full-SJ support is claimed.

Both fresh Morro products revalidated A 1,309/1,309 occurrences, B 83/83 compiled
registrations (77 selected profiles plus six separate legacy proofs), C 83/83
semantic closures and D real composition, all with zero blocked/unknown/missing
requirements. Actual linked assemblies, AOT objects, native images and packaged
content were verified. The [frozen K-N identities](../apple-everest/sj-snas-identities-stage25kn.json)
remain the unchanged acceptance authority.

The current migration has the same content and inspected linked method bodies
as fresh original-source reference builds, but differing IPA bytes. Build
metadata and generated native differences are documented; unresolved native
differences are not dismissed as harmless metadata. No hash normalization or
acceptance rule was relaxed.

## Remaining work and operating boundaries

- Complete the exact build-49 iPhone/Apple TV physical matrix: real lobby route,
  death/retry, bubble/switch/gate/audio behavior, completion, save/cold resume,
  cross-map restoration and lifecycle. Human observations remain separate from
  automated host/product checks.
- Resolve byte-level compiler/metadata reproducibility if exact IPA byte equality
  is required. All existing logical/native locks continue to apply.
- The private vanilla tvOS cloud workflow passed local syntax, orchestration,
  input/privacy and clean-source tests. A hosted Morro Actions run has not been
  performed; publication does not claim it has.

`IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY` remains in effect for the
static-Everest candidate. Universal metadata remains validated; modern-device
quality is not reduced for the old iPad. The M1 remains the signing/install
fallback. No new signing, installation, gameplay testing, worker tuning, map
expansion or release is part of source publication.

The public `main` branch is for the new Morro source. Old protected release refs
remain in the original repository and are not pushed as Morro tags/releases.
Generated/private roots and binary package/signing extensions are ignored.
Historical raw logs and products stay private; their sanitized outcomes and
hashes remain in this repository.

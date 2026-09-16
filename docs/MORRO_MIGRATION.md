# Morro source migration

The source baseline is the K-N build-49 candidate
`34c0b933a4cf2252780ec84e5630847809793eab`. It descends from accepted build 46
`be8546d4ae411cb491a0c3bbc4e5343ebf9c9651`, HOST-B native tooling
`632a451da2f62928404a2408826747aa6fc55b20`, HOST-C tooling
`b4997fa49f4f8b82f6f2984fe9047396e401ea48`, and K-M
`b65bedd20016dc3482d7702d7f0a9707bc2b1479`.

This migration preserves all 158 ancestor commits without rewriting author,
date, message or hash. The working tree removes the unused Xamarin application,
solution, legacy build script, DLL config, unused legacy patches and unused
native-builder gitlink. Git history necessarily still contains those historical
files. The source icon is moved byte-for-byte to `modern-ios/Assets/AppIcon`;
the still-used `patches/crash-fixes.patch` remains an exact canonical input.
FNA and its pinned recursive submodules remain. The modern native builder still
fetches its separately locked source from the original native-builder project;
removing an unused checkout does not remove that dependency or its credit.

`MORRO_FILE_INVENTORY.json` records each retained file, purpose, source hash and
disposition. The scope includes modern native/source generation, both vanilla
hosts, shared controls/persistence, static Everest code and authorities, current
and historical regression tooling, all modern architecture/build/test/release
documentation and cloud-template source. Historical stage evidence is retained
as evidence, not relabeled as a current PASS. No JIT project is imported. The [preserved local reports](history/migration-evidence/README.md)
include the separate HOST-A/B/C, K-M and K-N outcomes with original hashes.

Bounded tooling adjustments are the icon source location, the vanilla iOS host
doctor's host-architecture/OS policy, explicit serial vanilla iOS publishing,
the cloud verifier's reference to the actual modern iOS graph, a standalone
unsigned convenience wrapper and an explicit cloud-source export pin. The
desktop HookGen regression now restores its host builder before running it;
the old `--no-restore` assumption failed on a fresh checkout. The vanilla tvOS
soft-reload verifier now recognizes the existing canonical Apple version import
and exact property bindings, rather than requiring obsolete stage-version
literals. Missing, duplicate, conditional or changed bindings still fail; the
actual soft-reload behavior and product-token checks remain unchanged. Current FMOD,
Stage 6 and cloud aggregate callers also replace checks for unused Xamarin
prebuilt archives with the complete Morro source inventory check. All retained
prior native/host isolation paths, native hashes, generated-source locks and
product checks remain enforced against the accepted build-49 source foundation.
The public documentation check recognizes Morro's heading and additionally
requires its building, migration and credits documents. The original archive checksum record and exact
historical verifier versions remain in Git history; old manual stage commands
retain historical scope. Game,
runtime, helper/native code, transforms, dependency versions, normalization,
content, audio, persistence, native identities and canonical build 49 are not
changed to make migration pass.

## Reproduction and comparison

Fresh unsigned vanilla and Everest products must be built from committed source
with private inputs validated again. Retained build-49 signed IPAs are immutable
historical products, not unsigned ZIP comparison targets. A separate clean
baseline checkout produces the unsigned reference without modifying old working
trees. The migration report records complete IPA hashes, exact payload comparison
and any differing fields. A new source revision is reported truthfully in its
receipts; no old SHA, copied generated closure, signed product or PASS is relabeled.
Byte-identical ZIPs are claimed only if the bytes actually agree. Product and
logical-identity verification remain independent of ZIP equality.

Final results and private logs are outside Git. The migrated products do not
constitute physical acceptance of The Squeeze. Build 49's mandatory iPhone/tvOS
matrix remains pending, and iPad's static-AOT status remains
`IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY`.

## Publishing later and ancestry

The local working branch is `codex/morro-modern-aot-migration`. There is no
`origin`; inherited `upstream` is fetch-only with an intentionally disabled push
URL and `push.default=nothing`. No existing repository is renamed or published.
Choose the GitHub destination before adding `origin`, then push only the intended
branch explicitly. Never use a mirror or force push for this migration.

Morro is prepared for a standalone repository, following the owner's preference.
The ancestry, original licence and explicit README credit are preserved locally.
There is no requirement to force GitHub fork metadata. GitHub's “forked from”
relationship is repository metadata, not something a local `.git` directory can
set. The existing GitHub fork can be renamed later, or a separately authorized
fork/destination can be chosen where GitHub permits it. An independently created
repository retains Git ancestry but does not automatically acquire that badge.
See GitHub's [fork documentation](https://docs.github.com/en/pull-requests/reference/forks)
and [rename procedure](https://docs.github.com/en/repositories/creating-and-managing-repositories/renaming-a-repository).

The searchable names are **Morro — Celeste for Apple platforms (static AOT)**
and **Cabrillo — Celeste Mod Loader for Apple Platforms (JIT)**. Morro links to
the existing Cabrillo repository. Cabrillo already identifies the planned Morro
companion; its active, dirty working tree is not changed during this migration.
Update its public link once Morro has an actual published address.

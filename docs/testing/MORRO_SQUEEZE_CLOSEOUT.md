# The Squeeze — Morro iPhone stage closeout

Closed at the owner's request on **17 September 2026**. The iPhone continuation
completed its agreed playtesting and corrected the reported lobby card/Pause
overlap. Apple TV remains owner-deferred and static-Everest iPad acceptance
remains deferred. This closes the current stage; it does not claim the historical
combined iPhone/tvOS GREEN matrix or authorize a release.

## Exact products and source

| Product | Frozen source | Recorded outcome |
| --- | --- | --- |
| iPhone 0.1.1 (50) | `483bca5be2967cf12041d24bd8b875a34d634dba` | Signed product verified; The Squeeze and Bing play/persistence checks passed at the scope below; lobby card/Pause overlap reported. |
| iPhone 0.1.1 (51) | `28a336f4db6bbdd32af52b830005e19f167ea1d2` | Fresh signed product verified; in-place installation preserved saved data; owner confirmed the pause fix and retained silver display. |

| Product | IPA SHA-256 | Native executable SHA-256 |
| --- | --- | --- |
| 50 | `33fb412a522ffec89c1b5eec40f6552f949989ab23a9b10d0041878a8183e4e7` | `0eae020faabddb737407ab75820c9d3a9c76e0eabb565057b90a379d982579b8` |
| 51 | `5524f252fc1288153e9e358461a1392531f9132ecbf09b9cd65820952649e4cd` | `55907f3e052f3ea12cc570aa907b9e89ba1f5f229cb19900f185fcf0680e049b` |

The [sanitized results](MORRO_SQUEEZE_CLOSEOUT.json) bind these outcomes to their
private receipt digests. No generated game source, signed app, save, raw device
log, signing identifier or personal path is tracked. This later documentation
commit does not replace the source revision recorded in either product.

## Physical observations

| Product | Check | Result and boundary |
| --- | --- | --- |
| 50 | Real Beginner lobby and The Squeeze | Owner passed the unchanged six-room route, room-1 deaths/retries and completion/heart. |
| 50 | Room mechanics | Room-3 bubble and switch/gate, gate persistence through death/cold resume, room-4 berry and room-5 directional platform checks passed. |
| 50 | Collectibles and saves | Ordinary berries, silver pickup/death reset, successful silver completion, journal display, numbered Save and Quit and completion/silver cold persistence passed. The silver contributes to the displayed 3/2 count; the denominator counts two ordinary berries. |
| 50 | Cold restart | Scoped saved-state checks, actual full process exits and ordinary new launches accompanied the owner's room/gate/bubble, completion, Bing and silver confirmations. |
| 50 | Lobby/Bing and device lifecycle | Return to Lobby, Bing play/death/save/cold resume, iPhone Home/reopen recovery, touch and controller input passed in the requested scope. |
| 50 | Availability | Only Bing and The Squeeze entered playable Beginner levels. Other destinations, gym and heartside stayed unavailable; the authored 21-heart threshold remained. |
| 50 | Factory lifecycle | Owner reported 73/73; the retained application console confirms 73 distinct selected factories and the final PASS. This proves lifecycle dispatch, not every representative gameplay interaction. |
| 51 | Reported pause overlap | Owner confirmed the jar card closes on Pause, Resume/reopening works and the saved silver remains visible. No six-room or factory replay was requested for this UI correction. |

Before replacing build 50, the owner confirmed the title screen. A fresh backup
verified the saved lobby state, completion, heart and all three Squeeze berries.
The selected process was stopped and build 51 installed in place without
changing the app's save domain. All **20 files / 158690 bytes** matched before
and after installation. The ordinary build-51 launch used no injected debug
route, arguments or device environment. No further device action was required
to close this stage.

## Correction and automated evidence

The pinned CollabUtils2 implementation closes its wrapped Overworld after
`Level.Pause`. Morro omitted that cleanup dispatch, allowing the chapter card to
remain over the pause menu. Build 51 wraps the unchanged canonical Pause body
and invokes the existing cleanup afterward, including the early Quick Restart
path. The [correction report](MORRO_LOBBY_PAUSE_BUILD51.md) records the reference
and exact implementation scope.

Build 51 passed 48 focused host cases, a failure control that removes the cleanup
dispatch, 75 portable tests and 103 K-N contract controls. The actual generated
iOS Pause and cleanup methods matched those tested. Fresh mandatory preparation
and signed full/static LLVM AOT completed; final verification checked the exact
IPA/app payload, signatures/profile/device eligibility, managed/native/content
bindings, 15 platform controls and 17 AOT negative controls. These automated
results remain distinct from the owner's physical observations.

The first build-51 preparation correctly rejected the old frozen build-50
authority and was retained. The reviewed transition changed managed, shared,
semantic and composition identities; the final wrapper regenerated and matched
the new authority. Map/content, factory registry, collab, progression, audio,
dependency and native-library authorities stayed unchanged. The new app's
native executable has its own hash above. The four gates still cover 1,309
occurrences, 83 registrations/closures and the real composition, including
77 selected profiles plus six separate legacy proofs.

Build 50 also retains its fresh clean-source reproduction, signed product checks
and 21-step host regression run. Historical build-49 products, HOST/K-M/K-N
evidence and unsuccessful attempts remain preserved. No historical PASS is
transferred to a different product; build-50 IPA/native preservation was checked
again after producing build 51.

## Deferred scope and next development direction

Apple TV has no build-50/51 product or physical acceptance in this continuation.
Static-Everest iPad remains under
`IPADOS_PHYSICAL_DEFERRED_LEGACY_COMPATIBILITY_POLICY`. Separate physical journal
Pause, detailed authored sound timing/reference comparison and every manual
interaction across the 18 regression maps were not completed. Normal music and
controls passed the reported play checks. Host persistence fault controls do
not imply device save-corruption experiments; the owner's older slot-history
investigation was explicitly deferred.

The owner discontinued the exhaustive manual checklist in favor of exploratory
play and focused checks for reported bugs and persistence. Do not reopen that
checklist as a condition of continuing development. The next direction is to
reassess the retained K-M audit against current implementations, group shared
missing mechanics and authored profiles, and deliver playable Beginner batches
for feedback. The audit's earlier one-map recommendation is historical.

Only Bing and The Squeeze are enabled among ordinary Beginner maps; **19 other
ordinary maps** still need implementation/closure review, with gym and heartside
separate. No additional map was enabled or promised by this closeout. Existing
family-name coverage alone does not establish that a map's authored profiles,
assets, audio and persistence work.

This closeout changes documentation and its inventory only. No app rebuild,
device operation, runtime/content change, merge, push, hosted Actions run, tag or
release is part of it. Original migration byte-reproducibility uncertainty stays
recorded separately.

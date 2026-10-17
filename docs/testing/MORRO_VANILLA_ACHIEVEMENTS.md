# Vanilla Morro achievements

Status: **IMPLEMENTED — full-session backup integration under current validation; physical backup/restore acceptance pending.**

This report describes the new vanilla iOS/iPadOS and tvOS feature. Build and
host-fixture results establish no hardware or physical gameplay PASS. The initial
products and later viewer refinements have separate validation records below.
Historical vanilla, HOST, and static-Everest results remain evidence for their
original products only.

## Player experience

Open **Options → Achievements** to browse all 32 original Celeste Steam
achievements. The page uses Achievement Helper's illustrated rows, purple
panels, outlined text, selected-row borders, and an unlocked count over the
existing Celeste scene. Use the existing menu controls to move through the
list, page through it, reveal a selected secret, or go back. The same game UI is
used by the vanilla iPhone/iPad and Apple TV builds.

Unlocked achievements appear first, followed by locked achievements. Each group
keeps the original catalog order, so an entirely locked or unlocked list has the
same original order. A new unlock moves into its original position within the
unlocked group. Selection stays on the same achievement if the list changes
while open. Holding up or down stops at the end of the list; releasing and
pressing again wraps to the opposite end. Left/right paging stops at the ends.

Steam marks only **Celeste** and **1UP!** as public before unlocking; the other
30 achievements are hidden. Locked hidden entries remain in the list while
concealing their name, description, and artwork. **Reveal details**
is an explicit spoiler choice for the selected entry. It does not award an
achievement, and the reveal choice lasts only while that page is open. Earned
achievements always show their details.

This policy was checked against Valve's current presentation on 2026-10-17.
The [Steamworks spoiler-achievement guide](https://partner.steamgames.com/doc/features/achievements/ach_guide?l=english#spoiler_achievements)
says names and descriptions stay hidden until unlock or explicit reveal. The
[current Celeste web page](https://steamcommunity.com/achievements/504230?l=english)
also substitutes a question mark for hidden artwork; Valve's delivered row
renderer reveals all three fields together. That web page is marked beta; this
check does not claim every historical or stable native client uses the same
presentation. Morro keeps its existing per-entry reveal choice. The button glyph
and reveal/hide label are measured and centered together below the list.

New awards appear as queued, animated notifications, one at a time. A repeated
event for an already earned achievement does not create another notification.
The presentation waits while the achievements page or a blocking game menu is
open. New awards record their unlock date; achievements recovered from old
saves have no invented historical date. The initial achievement text and new UI
labels are English.

The feature is fully in game. It requires no Steam sign-in, Game Center,
server, or runtime internet connection, and it does not synchronize with a
Steam account.

## Original unlock rules

Celeste's existing `Achievements.Register` calls remain the authority for new
awards. Morro supplies a local implementation for the original game's otherwise
inactive achievement boundary; it does not replace the gameplay conditions.
The catalog contains exactly the 32 existing enum identifiers, with no extra
Morro challenges or awards. Assist Mode receives no new exclusion.

The original events cover chapter completion, strawberry totals, cassettes,
crystal hearts and B-sides, all C-sides, Farewell, the moon berry, a 1UP, and
PICO-8 completion. Several details matter when checking this integration:

- Core is area ID 9; its awards are `HEART8` and `BSIDE8`.
- B-side awards occur when the corresponding heart is registered.
- The C-side award uses the original total of at least 24 heart gems.
- Strawberry thresholds use Celeste's total strawberry count, including golden
  strawberries.
- The 1UP event is the original strawberry-chain event, at index five or higher
  in the zero-based sequence; no replacement chain counter is introduced.

## Existing saves

Before an explicit achievement reset or full-session restore, opening Achievements reads the three existing slots through the normal storage
authority, or uses the live save when playing. Selecting or loading a save also
recovers achievements that its original progress can establish. Recovery never
selects or initializes a save just to inspect it. This covers recorded chapter completion,
cassettes, strawberry totals, the relevant hearts, all C-sides, and Farewell.
Moon-berry recovery matches the original map's moon-berry entity to its exact
saved room/entity ID. A Farewell golden berry or an arbitrary berry count is
not evidence for that award.

Recovery adds earned achievements to the installation's record without changing
the vanilla save. It sends no burst of old notifications and assigns no
historical unlock dates. **1UP!** and **Real Gamer** have no reliable vanilla
save completion flag; they require their real game event after this feature is
installed. Merely opening the PICO-8 game does not earn its completion award.

## Storage and save compatibility

Achievement progress belongs to the app installation, across its normal save
slots. It uses a separate, versioned pair of app-private `NSUserDefaults`
records. Each record contains only bounded achievement IDs, unlock times,
generation metadata, and a checksum. The fixed 32-entry payload occupies less
than 16 KiB across its A/B replicas. Restore validates the records, retains
valid earned progress, and tolerates an invalid replica. Failed writes retain
pending progress in memory for retry. A failed synchronization cannot be
acknowledged merely because the preferences cache returns the new value. Both
replicas must confirm their writes. Historical recovery batches all awards into
one pair of writes; live awards are persisted immediately.

This storage does not add fields to Celeste's `SaveData` or `Settings`, change
those serializers, or expand the tvOS compressed vanilla-save envelope beyond
its original four files. Individual slot import/delete and Settings reset retain
achievement progress. On installations that have never explicitly restored or
reset achievements, selecting a save or opening the achievements page can still
recover awards supported by that save.

### Full-session backups

Vanilla Morro now has a **Full Backup** ZIP alongside the existing individual
`.celeste` operations. It contains all three slots (including explicit absence),
Settings, the achievement record and unlock dates, and a bounded allowlist of
device preferences. It does not contain credentials, pairing tokens, device or
team identifiers, app binaries, or game content.

- **iOS/iPadOS:** open **Options → Data & Files → Export Full Backup...** and
  save the single ZIP with the existing Files sheet. Rename it in Files, for
  example `Harry's Celeste.zip` or `Test Session.zip`. Use **Restore Full Backup...**
  to choose a ZIP and confirm replacement in Celeste.
- **tvOS:** open and pair the existing Save Manager, enter a backup name, and
  download its full ZIP. Upload a full ZIP and confirm replacement, then confirm
  the existing in-game reload to resume. Export is unavailable while a restore
  or reload is pending.
- **Reset Achievements** clears only achievements. Back up first to make that
  reset reversible. It does not delete saves or reset Settings.

A full restore replaces the session instead of adding to it: a missing slot is
removed, achievement progress can go down, and neither the old save fallback
nor an older achievement replica can bring the previous session back. Restoring
A, then B, then A restores each archive's own state. Explicit restore/reset
persistently disables automatic achievement recovery from saves. New original
gameplay achievement events still work normally; merely opening the viewer or
loading an old save does not undo the restored/reset state.

Full backups can move between vanilla iOS and tvOS subject to the destination's
existing save-size limits. Settings and achievements travel across platforms;
device preferences apply only when source and destination platforms match.
iOS includes touch preferences/layouts, grab modes, haptics, and controller
prompts. tvOS includes controller prompts and the performance HUD preference.
Foreign platform preferences are not copied into arbitrary native keys. Files
renaming on iOS changes the archive filename, not its internal creation label.

The ZIP has a versioned manifest and per-component SHA-256 hashes. Parsing
accepts only the fixed entry set, validates all files with the existing Celeste
serializers before mutation, rejects unknown/duplicate entries, and bounds
compressed and expanded data. It never extracts archive-supplied paths.
Legacy four-file tvOS ZIPs are not complete-session archives; import their
`.celeste` files with the existing individual-file actions.

The iOS restore stages one atomic private target journal, blocks stale writes,
replaces both primary and previous-good copies under the original file-store
lock, replaces achievements/preferences, and reloads a fresh main menu. The
journal is removed only after completion; an interrupted process replays the
validated target before gameplay. tvOS keeps the existing compressed A/B save
authority and stages a small intent containing old/target hashes and sidecars.
Cold recovery follows whichever base generation persisted, completes matching
sidecars, and mirrors both base replicas before clearing the intent. Both
platforms require storage acknowledgements rather than trusting a preferences
cache. An unresolved journal blocks gameplay mutations until recovery succeeds.

Local achievement replicas now include a session epoch. Only replicas within
the highest epoch can merge; explicit replacement creates a new epoch and
writes both replicas. Portable achievement data contains IDs, timestamps,
catalog identity, and recovery policy, but no local preference keys or epochs.
Existing version-1 local records remain readable without changing their
original recovery behavior.

Ordinary `.celeste` exports still contain only the original save or Settings
data. Full backups are user-controlled files, with no automatic cloud sync or
uninstall-survival guarantee for the app's local storage. Keep exported ZIPs
outside the app before deleting it.

## Artwork and provenance

[`managed/morro-achievements.json`](../../managed/morro-achievements.json)
records the original English display text, hidden flags, and icon URLs from
[Steam's Celeste achievement schema](https://api.steampowered.com/IPlayerService/GetGameAchievements/v1/?appid=504230&language=english).
The metadata snapshot was retrieved on 2026-10-16, with Steam schema version 9
and schema hash `2268056439`. The [Steam achievement page](https://steamcommunity.com/achievements/504230)
also exposes the count and hidden-achievement presentation.

The catalog pins both unlocked and locked icon URLs and SHA-256 checksums.
Original Steam JPEGs remain proprietary artwork, stored only in ignored local
build caches and staged private products. They are not checked into this
repository. Build-time artwork staging verifies all 64 pinned files. The first
build needs an internet connection to populate its private cache; a populated
cache allows subsequent offline preparation. The game loads only bundled
content, with a `*` text badge as a defensive fallback if an image cannot be
loaded. Hidden entries use the game's existing locked-chapter icon, with `?`
as its fallback. It never downloads icons on device.
The supplied game atlases contain useful chapter and collectible artwork, but
not the complete set of these exact Steam achievement composites.

[Achievement Helper](https://gamebanana.com/mods/392184) by Brokemia supplies
the adapted viewer and notification presentation. Its
[source at `6f8bb7807b077195c149f2336be2fb166a0e483b`](https://github.com/Brokemia/AchievementHelper/tree/6f8bb7807b077195c149f2336be2fb166a0e483b)
is MIT-licensed, copyright 2023 Brokemia. The
[complete upstream license](../licenses/ACHIEVEMENT_HELPER_LICENSE.txt) is
preserved verbatim, credited in [CREDITS.md](../../CREDITS.md), and bundled at
`Licenses/AchievementHelper.txt` in both vanilla Apple products. Package
verification rejects a missing or changed notice.

The adapted routines are `AchievementMenuItem.Render` (icon, name, description,
slate-blue cards, double borders, and locked-entry dimming),
`AchievementGameComponent.Draw`, `Update`, and `ShowNext` (notification sizing,
80-pixel icons, text proportions, five-second hold, and 0.3-second entry/exit),
and `OuiAchievementMenu.Render` (the translucent backdrop over the current
scene). Morro moves notifications to the upper right to avoid the Apple touch
controls and uses five visible rows with touch controls or six with a
controller. The original helper's Everest-only lock artwork is replaced with
the game's own lock artwork.

The presentation is adapted to use the original game's achievement events,
Morro's fixed Steam catalog, separate local storage, and the shared Apple input
and rendering path. No Achievement Helper assembly, condition evaluator,
Everest runtime, or dynamic hook is loaded. The helper's existing historical
static-Everest acceptance remains a separate feature and evidence record.
Its MIT license covers the reused code, not the Steam achievement artwork.

## Build isolation

The feature is applied as a final vanilla-only derived-source stage after the
existing locked source generation. Original staged source, upstream inputs,
and static-Everest generation paths are preserved. The iOS and tvOS vanilla
hosts use the same achievement catalog, rules adapter, presentation, and
bounded storage model.

There is no JIT, interpreter, dynamic mod loading, runtime hook installation,
new FMOD Studio system, or change to the native dependency authority. The normal
pinned toolchain, serial app AOT, full trimming, and disabled shared
compilation/build-server policies still apply.

## Validation record

The following results describe the initial implementation and its products. A
source review, unit test, or successful build cannot establish physical gameplay
acceptance.

| Gate | Initial implementation result |
| --- | --- |
| Catalog count, enum order, hidden flags, and artwork checksums | PASS — 32 exact IDs, 30 hidden entries, 64 pinned icons |
| Bounded ledger, duplicate awards, corruption recovery, and failed-write retry | PASS — 33 ledger checks |
| Existing-save recovery and exact moon-berry matching | PASS — 52 bridge checks plus one 30-award batch recovery check |
| Vanilla save/Settings and static-Everest isolation | PASS — strict derived-source/content verification and 19 tooling regressions |
| Existing Stage 22B source gate | PASS — 91 source checks |
| iOS unsigned full-AOT compilation | PASS — 329.331 seconds, exit code 0 |
| iOS final package and achievement checks | PASS — sanitized final app, full AOT/full trim, canonical original content, achievement content/notice, and IPA ZIP integrity |
| tvOS unsigned full-AOT compilation | PASS — 244.54 seconds, exit code 0 |
| tvOS final package and achievement checks | PASS — sanitized final app passed Stage 8A and achievement package verifiers, plus final IPA ZIP integrity |
| Adapted UI compilation against actual Celeste/FNA references | PASS — 0 warnings and 0 errors |
| Host UI navigation, secret reveal, queue concurrency, modal suspension | PASS — current adapted presentation fixture |
| Controller and phone/tablet default control geometry | PASS — source-backed host previews; no device or GPU result |
| iPhone/iPad touch and controller interaction | PENDING — physical acceptance |
| Apple TV controller interaction and visual layout | PENDING — physical acceptance |
| Cold relaunch, soft reload, import/delete, and Settings-reset continuity | PENDING — exact-product acceptance |

Both unsigned build receipts record frozen source
`316ae922f8db7f5bfe46021cf3ac07acd111c402`. No installation or device execution
was performed during those unsigned build checks. The verified iOS package retains the canonical original-content
SHA-256 `30a1c147d1a3ab0aa45762094e393ed7fd69951dd66e5af063447641e0699c46`;
the 64 achievement images and exact bundled license are checked separately.
The final copied tvOS app passed `scripts/verify-celeste-tvos-stage8a.py` and
`scripts/verify-morro-achievements.py --app`, with its report stored privately
at `artifacts/morro-achievements/tvos/package-verification.json`. Sanitization
fixed five CodeView records, and no local repository paths remained in the app.

Both builds used the pinned .NET SDK 10.0.302/runtime 10.0.10, Xcode 26.6
(17F113), and Apple SDKs 26.5, with process-local Xcode selection and serial app
AOT. They used unsigned test bundle identities `io.example.morro.ios` and
`io.example.morro.tvos`; those products were unsigned. The existing port
version 0.1.1/build 49 remains unchanged and does not transfer historical
build-49 device evidence to these new products. Original build outputs were
preserved; sanitization and final repackaging used separate copied apps.

The verified private archives are
`artifacts/morro-achievements/ios/Morro-Achievements-iOS-unsigned.ipa` and
`artifacts/morro-achievements/tvos/Morro-Achievements-tvOS-unsigned.ipa`.
The private `artifacts/morro-achievements/verification-receipt.json` records
their byte sizes, SHA-256 hashes, source revision, and verification results.

The adapted UI fixture uses the iOS compile definition and verifies hidden
names/descriptions, explicit reveal without unlocking, navigation through all
32 entries, touch/controller reflow, close/reset behavior, 1,000 concurrent
queue calls with duplicate suppression, and modal suspension. The inspected
controller, phone, tablet, and popup previews use the actual presentation
source, the supplied game's Renogare bitmap font, and the pinned original
Steam icons. Directional glyph placeholders appear only in that host fixture.
These previews establish source-backed layout evidence, not rendering or
input acceptance on a GPU or an Apple device.

### Viewer refinements, 2026-10-17

The later viewer changes center the full reveal/hide prompt, partition the list
by unlock state while retaining catalog order, and use Celeste's native
fresh-press versus repeat behavior for wrapping. The existing hidden flags,
gameplay unlock rules, storage, and original catalog order are unchanged.

The 19 achievement preparation and verification regressions and 51 source-backed
UI assertions pass for this revision. The UI fixture covers all-locked, all-unlocked,
and mixed order; selected-achievement preservation across unlocks; both held
boundary stops and fresh-press wraps; clamped page jumps; and measured centering
of both prompt labels. Both platform preprocessing paths compile against the
actual Celeste/FNA references with zero errors. The iOS host fixture reports
three warnings from its intentional Apple API stubs; the tvOS path reports none.
The refreshed controller, phone, and tablet previews use the real font and icons.
Host compilation and previews are evidence for the presentation source only.
The initial full-AOT and package results above do not
transfer to these refinements; a fresh build and physical interaction check are
still required for this revision.

Physical acceptance must check the page from supported Options routes, scrolling
and paging, reveal/hide behavior, return focus, touch/controller prompts, and
readable layout. It must also exercise a real new award, duplicate suppression,
multiple queued notifications, save recovery without notification spam, and
continuity after a cold relaunch. Save/Settings mutation checks belong to an
explicitly isolated acceptance environment, preserving the owner's production
data. Record the actual source revision and product identities with those
results; do not inherit a prior build's PASS.

Portable checks (use the pinned .NET SDK with shared compilation, build servers
and node reuse disabled):

```sh
PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover -s tests
dotnet run --project tests/MorroAchievementTests/MorroAchievementTests.csproj -p:UseSharedCompilation=false
dotnet run --project tests/MorroAchievementBridgeTests/MorroAchievementBridgeTests.csproj -p:UseSharedCompilation=false
dotnet run --no-build --project tests/MorroAchievementBridgeTests/MorroAchievementBridgeTests.csproj -- --batch-recovery
```

The full portable Python suite passed 117 tests, including the 19 feature
regressions. The C# ledger and bridge suites passed 86 checks in total, including
the separate batch-recovery process. Source verification checks the entire
derived tree against a fresh
application of the exact transform and verifies all original content bytes,
separately from the 64 new private artwork files. The optional iOS package flag
keeps the original canonical content hash/count/size requirement intact.

### Full-backup validation, 2026-10-17

This integration has separate validation from the initial achievement products
and viewer-refinement products. Host tests use owned data and isolated storage;
no restore, reset, or fault injection runs against the owner's device data.
Current host checks:

| Check | Result |
| --- | --- |
| Portable Python suite | PASS — 119 tests, including 21 final-stage preparation/verifier regressions |
| Achievement ledger and bridge | PASS — 116 checks including batch recovery |
| Shared ZIP format | PASS — 53 checks including malformed directory counts, payload limits, and exact A/B/A round trips |
| iOS actual-template operation flow | PASS — 29 assertions including cancellation, journal staging uncertainty, retry, and durable achievement-only reset |
| iOS durable file authority | PASS — 202 assertions including eight interrupted-write boundaries and previous-good isolation |
| iOS platform foundation | PASS — 255 assertions including preference validation |
| tvOS actual transaction with isolated platform fixture | PASS — 70 checks including all eight synchronization failure points with cached/durable uncertainty |
| tvOS full-backup HTTP protocol | PASS — 79 checks including authentication, stale revision, limits, and blocked export after failed restore |
| tvOS pairing / continuity | PASS — 31 / 57 checks |
| Apple API managed compilation | PASS — both platforms, zero errors; host fixtures are not device evidence |
| Fresh full-AOT products | PENDING — current source must be committed before serial product compilation |

Reproduce the new portable suites with the pinned SDK and flags below. The
Foundation and durability projects are under `modern-ios`; the ledger and
bridge projects are under `tests` as listed earlier.

```sh
dotnet run --project tests/MorroIOSBackupTests --disable-build-servers -m:1 -nr:false -p:BuildInParallel=false -p:UseSharedCompilation=false
dotnet run --project tests/MorroBackupTests --disable-build-servers -m:1 -nr:false -p:BuildInParallel=false -p:UseSharedCompilation=false
dotnet run --project tvos/MorroBackupTests --disable-build-servers -m:1 -nr:false -p:BuildInParallel=false -p:UseSharedCompilation=false
dotnet run --project tvos/SaveManagerProtocolTests --disable-build-servers -m:1 -nr:false -p:BuildInParallel=false -p:UseSharedCompilation=false -p:MorroBackupProtocolTests=true
```

Physical acceptance remains **NOT_RUN** for the backup changes. On each platform,
check export through its real file-transfer UI, switch two distinguishable
archives in both directions, confirm empty slots and achievement counts/dates,
confirm Settings and platform preferences, cold-launch, and verify a new award.
Use an isolated test namespace/data set and preserve production backups first.
Historical build-49 and HOST/K-M/K-N evidence is unchanged.

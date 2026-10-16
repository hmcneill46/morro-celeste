# Vanilla Morro achievements

Status: **IMPLEMENTED — portable logic verification passed; adapted UI, full-AOT, and physical acceptance pending.**

This report describes the new vanilla iOS/iPadOS and tvOS feature. It records no
new product or hardware PASS. Historical vanilla, HOST, and static-Everest
results remain evidence for their original products only.

## Player experience

Open **Options → Achievements** to browse all 32 original Celeste Steam
achievements. The page uses Achievement Helper's illustrated rows, purple
panels, outlined text, selected-row borders, and an unlocked count over the
existing Celeste scene. Use the existing menu controls to move through the
list, page through it, reveal a selected secret, or go back. The same game UI is
used by the vanilla iPhone/iPad and Apple TV builds.

Steam marks only **Celeste** and **1UP!** as public before unlocking; the other
30 achievements are hidden. Locked hidden entries retain their place in the
list while concealing their name, description, and artwork. **Reveal details**
is an explicit spoiler choice for the selected entry. It does not award an
achievement, and the reveal choice lasts only while that page is open. Earned
achievements always show their details.

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

Opening Achievements reads the three existing slots through the normal storage
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

This storage does not add fields to Celeste's `SaveData` or `Settings`, does not
change their serializers, and does not enter the tvOS compressed vanilla-save
generations. The Save Manager's fixed four-file boundary remains intact.
Achievement progress therefore remains when a numbered slot is imported,
replaced, or deleted, when Settings is reset, and through the existing soft
reload. Importing a save can add recoverable achievements when that save is
selected or the achievements page is reopened; deleting a save never revokes an
earned achievement.

Ordinary `.celeste` exports contain only the original save or Settings data.
They do not back up this separate achievement record. Achievement state has no
cross-device sync or uninstall-survival guarantee: uninstalling the app removes
its local progress. Reinstalling and importing an old save can recover only
the achievements evidenced by that save, as described above. There is no
achievement-reset action in this initial feature.

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
content, with a simple text badge as a defensive fallback if an image cannot
be loaded. Hidden entries use the game's existing locked-chapter icon. It never downloads icons on device.
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

The following results describe this implementation and its new products. A
source review, unit test, or successful build cannot establish physical gameplay
acceptance.

| Gate | Current result |
| --- | --- |
| Catalog count, enum order, hidden flags, and artwork checksums | PASS — 32 exact IDs, 30 hidden entries, 64 pinned icons |
| Bounded ledger, duplicate awards, corruption recovery, and failed-write retry | PASS — 33 ledger checks |
| Existing-save recovery and exact moon-berry matching | PASS — 52 bridge checks plus one 30-award batch recovery check |
| Vanilla save/Settings and static-Everest isolation | PASS — strict derived-source/content verification and 19 tooling regressions |
| iOS and tvOS full-AOT compilation and package checks | PENDING — actual new products |
| Host UI compilation, navigation, secret reveal, queue concurrency, modal suspension | PENDING — revalidation after adapting Achievement Helper presentation |
| Phone/tablet default control geometry | PENDING — new presentation requires layout validation |
| iPhone/iPad touch and controller interaction | PENDING — physical acceptance |
| Apple TV controller interaction and visual layout | PENDING — physical acceptance |
| Cold relaunch, soft reload, import/delete, and Settings-reset continuity | PENDING — exact-product acceptance |

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
regressions. Source verification checks the entire derived tree against a fresh
application of the exact transform and verifies all original content bytes,
separately from the 64 new private artwork files. The optional iOS package flag
keeps the original canonical content hash/count/size requirement intact.

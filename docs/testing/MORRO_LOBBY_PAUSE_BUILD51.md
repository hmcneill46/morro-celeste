# Morro build 51 — lobby card pause cleanup

On iPhone build 50, opening a lobby level card and pressing Pause opens the
pause menu underneath the card. CollabUtils2 1.13.4 handles Everest's post-pause
event by closing its wrapped Overworld and restoring the player. Morro lowered
the chapter UI and pause-menu buttons but omitted this cleanup event.

The correction keeps the complete canonical pause body behind a static wrapper
and calls the Collab cleanup afterward, including the early Quick Restart path.
The existing close operation removes and flushes the chapter panel before
removing its scene wrapper, restores the previous area and audio, clears forced
chapter/journal metadata, and releases the player's dummy state at frame end.
It also handles the journal, which uses the same wrapped scene.

The reference is the exact pinned CollabUtils2 DLL
`ce7d646252a2fc51f1f513071d97eaa1acedf7ddb0871276c688914e37319f60`,
`InGameOverworldHelper.OnPause` and `Close`, plus Everest commit
`4bbde91b8dbaaddef2ceec75ca0cd6d59b3b8d00`'s `Level.Pause` event dispatch.
No new event bus, dynamic hooks or runtime assembly loading is introduced.

`test-morro-collab-pause.py` runs the actual generated dispatch and runtime
cleanup against owned UI handles. It covers chapter/journal, ordinary/minimal
pause, Quick Restart, player-state restoration, missing save data, repeat pause,
and cleanup ordering. It compares the complete original pause body against the
build-50 generated body. Removing the dispatch must reproduce a retained card.
This host check does not certify GPU rendering or physical controls.

Only runtime pause dispatch/cleanup, its exact semantic/generator bindings,
canonical build 51 and freshly regenerated product identities change. Map,
asset, audio-bank, profile, native, dependency and persistence authorities stay
unchanged. The source inventory records exact successor hashes, preserving its
historical migration checks. Builds 49 and 50 and their evidence remain separate.

The current owner-directed playtest approach supersedes the older exhaustive
manual checklist for this continuation. After the replacement is verified and
installed, the focused check is: open a jar's card, Pause, Resume, then reopen
the card. Check the journal/Pause path and that saved silver remains visible.
Further play can be exploratory, with a map/room and reproduction steps for
issues. Do not repeat the six-room route and 73-factory sweep merely for this UI
correction. Unknown or untested behavior remains explicitly untested; no whole
matrix GREEN or Apple TV acceptance is implied.

Broader Beginner support is the development direction. The existing K-M audit
lists real missing mechanics/content bindings, so making every jar available is
not sufficient. Reassess that ledger against the now-implemented Squeeze
features, group shared implementation work, and deliver playable batches for
owner feedback. The audit's earlier one-map recommendation is historical, not
a standing requirement to repeat one-map-at-a-time manual certification.

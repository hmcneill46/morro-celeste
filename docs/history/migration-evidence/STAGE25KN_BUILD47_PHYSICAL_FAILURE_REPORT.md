# Stage 25K-N — build 47 physical failure

Status: YELLOW — iPhone death/reload fails; correction and fresh products required.

Source: `06c6fe0a5578888cf332cd5ed323aeb1065f1b34`, version 0.1.1 (47). All prior closure, signed product and negative checks remain historical results for these exact products. They did not establish gameplay acceptance.

After explicit user approval to remove only the experimental apps, both replacements installed successfully with unchanged bundle IDs. iPhone install took 40.709 seconds; Apple TV install took 725.018 seconds. The user-configured new signing team required replacement after both original in-place attempts were rejected. Vanilla was untouched. Supported restore and copy-back checks preserved all 20 iOS persistence files (163,835 bytes) and the tvOS preferences file (30,071 bytes) exactly. Actual use of restored progression still requires observation.

The user trusted the iPhone developer profile. Normal launch then succeeded, and its live process was bound to the exact installed build-47 app. The user reports correct The Squeeze title, credits, icon, initial room/spawn and initially normal play through the real lobby panel. After a death, probably in the first room, the screen became black and touch controls stopped responding; the cause of death was not recalled. The same app process remained alive. The selected app crash inventory contains no new matching crash report; an in-app error log records `System.NullReferenceException` in `Celeste.Audio.SetMusic`, called by `AudioState.Apply` during `Level.Reload`. This is an observed technical failure, not a physical PASS.

The private runtime log binds the transition to the exact snas SID and room 1, creates the snas music and flourish events through the ordinary audio path, then stops before the reported reload. Source inspection shows that current music is populated from an unchecked FMOD `EventDescription.getPath` result. The pinned managed FMOD binding returns a null path on nonzero results. The pinned Everest source supplies event names through its GUID/path cache, whereas this static closure only implements forward GUID lookup. Further host tests will establish the finite correction; no bank, map, dependency or native acceptance lock may change.

A preliminary host-native diagnostic stopped at an exact-version assertion before loading banks: the lawful macOS game bundles FMOD 1.10.20, while the device SDK is pinned to 1.10.09. It is not counted as a pinned native test or substituted for the device runtime.

Apple TV build 47 is installed and its preferences restored; gameplay is on hold while the shared audio defect is corrected. No title/route/death/audio/physical PASS is transferred between devices or products.

Preserve both build-47 products, original receipts and raw device logs. A correction requires a new source commit/build number, three fresh closures, clean-checkout reproduction, both fresh serial signed full-AOT products, and renewed physical acceptance. No integration approval, new map or next stage is implied.

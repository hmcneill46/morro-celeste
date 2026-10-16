# Morro credits and provenance

Morro continues the modern Apple work in
[hmcneill46/celeste-ios](https://github.com/hmcneill46/celeste-ios), descended from
[RoootTheFox/celeste-ios](https://github.com/RoootTheFox/celeste-ios).
The original FNA/iOS port and native foundation are credited to RoootTheFox and
the upstream contributors. The inherited MIT [LICENSE](LICENSE), including its
original copyright notice, is preserved verbatim.

All 158 ancestor commits through
`34c0b933a4cf2252780ec84e5630847809793eab` retain their original hashes,
authorship and messages. The recorded authors are hmcneill46, rooot, Toshit
Chawda and Kyrill. `git shortlog -sn` and `git log --follow -- PATH` retain the
detailed attribution; this summary does not replace source notices.

Celeste is by Extremely OK Games / Maddy Makes Games and its credited team.
Game code, maps, artwork, music and other assets remain their creators' work.
The user supplies lawful original game files; the repository's license does
not license Celeste or grant redistribution of generated game source or IPAs.

The runtime and native foundations use FNA, SDL, FNA3D, FAudio, Theorafile,
MoltenVK, Microsoft's .NET/Mono Apple tooling, FMOD and the FMOD-SDL bridge.
Dependency locks retain their exact source revisions and licenses. Native
packaging retains the required component license files. The external FMOD SDK
remains private and subject to its own terms.

The vanilla achievements viewer and notification presentation adapt code from
[Achievement Helper by Brokemia](https://gamebanana.com/mods/392184), using its
[MIT-licensed source at `6f8bb7807b077195c149f2336be2fb166a0e483b`](https://github.com/Brokemia/AchievementHelper/tree/6f8bb7807b077195c149f2336be2fb166a0e483b).
Copyright 2023 Brokemia. The [complete upstream notice](docs/licenses/ACHIEVEMENT_HELPER_LICENSE.txt)
is preserved verbatim and bundled as `Licenses/AchievementHelper.txt` in both
vanilla Apple products. Morro adapts that presentation for static compilation,
the original game's achievement events, and separate local storage; it does
not load Achievement Helper or Everest on device. Steam achievement artwork
remains Celeste artwork and is not covered by the helper's MIT license.

Everest, MonoMod, Strawberry Jam, its map authors (including The Squeeze's
source-bound credits), and each helper/asset/audio package retain their own
credits and licenses. See [the exact third-party inventory](docs/APPLE_EVEREST_THIRD_PARTY.md)
and the package authorities in `apple-everest/`; finite compatibility code is
not a claim of authorship of the original mod or map.

Morro is unofficial and is not endorsed by the game creators, Apple or FMOD.
The source-project name does not replace any in-game credit or attribution.

# Building Morro

Morro contains both the modern vanilla iOS/tvOS port and the finite static
Everest build-49 lane. Its source layout is independent of the old checkout.
The product names, bundle identities, save domains and canonical version remain
unchanged during migration. Unsigned artifacts cannot be installed directly.

## Host and private inputs

Use Xcode **26.6 / 17F113**, the four iOS/tvOS device/simulator SDKs **26.5**,
.NET SDK **10.0.302**, workload set **10.0.302.0**, and runtime/compiler packs
**10.0.10**. The existing host bootstrap pins .NET **8.0.424 / 9.0.317**; the
locked ILSpy tool also needs its pinned .NET 6 runtime. GNU Make and Mono's
`monodis` remain prerequisites. The Intel HOST-C environment is macOS 26.6.2;
the earlier Apple-silicon host used 26.3. The host doctor accepts those host
architectures on macOS 26.3+ in the 26.x family and still checks every exact
Xcode, SDK and workload pin. This does not change device deployment targets.

Set `DEVELOPER_DIR` for the current process, for example:

```sh
export DEVELOPER_DIR=/Applications/Xcode-26.6.app/Contents/Developer
git -c url.https://github.com/.insteadOf=git://github.com/ submodule update --init --recursive
```

The local migration includes verified private input copies and pinned host
SDKs. A source-only Git clone intentionally does not include them. Supply:

| Local path | Required content and reason |
| --- | --- |
| `.private/inputs/Celeste` | Exact original managed assemblies, complete Content, original icon and the locked package markers; validated by `managed/celeste-input-profiles.json` |
| `.private/inputs/FMOD` | Exact 1.10.09 build 97915 headers, revision and iOS/tvOS device archives; never redistributed |
| `.private/inputs/packages` | The 26 exact K-N helper/root/asset/audio ZIPs, including CommunalHelper 1.25.5 and ChronoHelper 1.3.3 |
| `.build/apple-everest/toolchain/dotnet10` | Optional local pinned .NET 10 SDK/workloads/runtime tools; otherwise use the correctly configured host `dotnet` |
| `.build/apple-everest/toolchain/dotnet8`, `dotnet9` | Pinned host-only generator SDKs; existing bootstrap installs them if absent |

`Backups/settings.celeste` in the supplied itch Mac profile is a hash-locked
shipped package marker. Only that exact marker is retained, not personal saves.
Desktop runtime binaries and unused Mac launcher files are not build inputs.
All package ZIPs are rehashed by the unchanged production preflight. Do not
replace helpers with latest versions, import M1 binaries or copy old app objects.

## Unsigned builds

Commit the intended source before final AOT. Use a new run name for each run.
Pass the intended bundle identifier explicitly; it is not a signing credential.
The examples are unsigned development examples, not replacement-install IDs.

```sh
python3 scripts/build-morro-unsigned.py --product vanilla --platform ios --run vanilla-ios-1 --bundle-id io.example.morro.ios
python3 scripts/build-morro-unsigned.py --product vanilla --platform tvos --run vanilla-tvos-1 --bundle-id io.example.morro.tvos
python3 scripts/build-morro-unsigned.py --product everest49 --platform ios --run everest-ios-1 --bundle-id io.example.morro.everest.ios
python3 scripts/build-morro-unsigned.py --product everest49 --platform tvos --run everest-tvos-1 --bundle-id io.example.morro.everest.tvos
```

Run these sequentially. The wrapper selects local host tools when present and
uses the existing native/source preparation and product builders. It never signs
or installs. Native inputs are built if absent and verified before staging;
Everest closures and app roots are fresh. Canonical source is regenerated from
the original inputs. Native construction is shared within this checkout, not
transplanted from an old `.build` tree. Phase logs and real exit codes are under
`.build/morro/runs/<run>`. The original `build-ios.sh`, `build-tvos.sh` and K-N
wrapper remain available with their documented options.

Vanilla output remains in `artifacts/ios` or `dist`; Everest output is under
`artifacts/apple-everest/morro/<run>/<platform>`. The K-N lane requires all four
expanded gates, all original logical identities and the actual linked/AOT/native
and package checks. Full trimming, static LLVM device AOT, arm64 targets,
iOS families [1,2]/minimum 15 and tvOS family [3]/minimum 16 remain unchanged.

## Tests and cloud compilation

Run portable tests with `PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover -s tests`.
The existing `scripts/regress-apple-everest-stage25kn.py` runs the complete
current host suite with an owned fresh output root. It does not replace final
linked-product verification or device observations.

The existing private **vanilla tvOS** GitHub builder and its cleanup/privacy
controls remain under `cloud-builder-template/`. Export for a published Morro
revision using `scripts/export-morro-cloud-builder.py`; it binds an explicit
public repository and immutable source SHA instead of silently building the old
repository. Its exact private inputs, permissions, Xcode/workload checks,
safe-cache restrictions and product verification are preserved. See
[cloud building](CLOUD_BUILDING.md). The template does not claim an Everest/iOS
cloud lane. Those products have the tested local commands above.

No hosted Actions run is performed by local migration testing. Workflow syntax,
export parity, orchestration and negative controls can be tested locally; the
first remote end-to-end run needs publication and private input setup separately.

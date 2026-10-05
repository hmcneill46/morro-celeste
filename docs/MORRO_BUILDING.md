# Building Morro

Morro contains both the modern vanilla iOS/tvOS port and the finite static
Everest build-49 lane. Its source layout is independent of the old checkout.
The product names, bundle identities, save domains and canonical version remain
unchanged during migration. Unsigned artifacts cannot be installed directly.

## Fresh source checkout

```sh
git clone https://github.com/hmcneill46/morro-celeste.git Morro-Celeste
cd Morro-Celeste
git -c url.https://github.com/.insteadOf=git://github.com/ submodule update --init --recursive
```

The repository contains all owned build/generation/verification code and project
history. Recursive public dependencies and exact package downloads remain pinned;
Celeste, FMOD and Xcode must be supplied separately. No historical local checkout,
generated closure, IPA or copied PASS is required. See [current progress](MORRO_STATUS.md)
for the distinction between tested source and later documentation commits.

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

## Exact build-49 public packages

For a **new** empty package destination, the existing pinned fetcher acquires 24
K-J/K-L packages including SJ assets/audio. K-N adds exactly CommunalHelper and
ChronoHelper. This shell block stops on a failed download or identity check:

```sh
(
  set -e
  python3 scripts/fetch-apple-everest-stage25kj-inputs.py \
    --include-presentation --output .private/inputs/packages
  curl --fail --location --retry 3 https://gamebanana.com/mmdl/1775162 \
    --output .private/inputs/packages/CommunalHelper.zip
  curl --fail --location --retry 3 https://gamebanana.com/mmdl/1778580 \
    --output .private/inputs/packages/ChronoHelper.zip
  printf '%s  %s\n' \
    44f4fb0b277a4900fd2a555e1a73e661140aa7b2455d3c7776cf420193349e3c .private/inputs/packages/CommunalHelper.zip \
    af46039437fbed52e72941d07e0d9ce6657dacc838516459f7e9e995fea07a18 .private/inputs/packages/ChronoHelper.zip \
    | shasum -a 256 -c -
)
```

The fetcher refuses an existing destination, so it cannot replace an established
input cache. K-N preparation independently validates all ZIP/DLL/version/profile
identities again. Exact pins come from `strawberry-jam-dependency-graph-stage25kc.json`,
`sj-beginner-expansion-inputs-stage25km.json` and the selected K-N contracts in
`apple-everest/`. EeveeHelper and the other audit-only packages are not inputs.
If a pinned public download disappears or differs, stop; do not use latest.

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

The private **vanilla iOS/tvOS** GitHub builder and its cleanup/privacy controls
live under `cloud-builder-template/`. Choose `ios`, `tvos`, or `both`; `both`
builds serially and publishes only after both products pass verification.
Export for another published Morro revision with
`scripts/export-morro-cloud-builder.py`, which binds the explicit public
repository and immutable source SHA in both locations. See
[cloud building](CLOUD_BUILDING.md) for inputs, output names, cleanup and signing.
Everest continues to use the local commands above.

Run `python3 scripts/test-cloud-builder-template.py` for the retained input,
privacy, cleanup and cache controls, and the `tests/test_cloud_products.py`
suite for platform routing and publication failures. The historical Stage 18C
verifier retains its original tvOS/source scope. Use actionlint for current
workflow syntax and the exporter's `--check` mode for exact template parity.
Local controls do not establish a successful hosted build or physical acceptance.

The [Apple cloud-builder acceptance report](testing/MORRO_CLOUD_APPLE_BUILDER.md)
records the successful hosted `both` run, tested source and exact output identities.

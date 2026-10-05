# Vanilla Apple cloud builder

Status: **PASS — hosted combined build and output verification (2026-10-05).**

## Result and scope

The private cloud builder offers `tvos` (the default), `ios` (one universal
iPhone/iPad app), or `both`. Combined builds download the two inputs once and
invoke the existing vanilla tvOS builder, then the existing vanilla iOS builder
serially. Each actual IPA must pass its platform verifiers before the complete
output is uploaded to a private draft Release. The draft is published only
after every expected asset passes size/digest checks.

The fixed tags remain `celeste-tvos-inputs` and `celeste-tvos-output`, preserving
existing private repositories and cleanup. The six-file export includes
`scripts/build-products.py`. The two open-source tvOS cache paths retain
independent verification; iOS has no cross-run cloud cache.

The workflow selects Xcode 26.6 through `DEVELOPER_DIR`, .NET SDK 10.0.302
before workload installation, workload set 10.0.302.0 and Apple SDK 26.5.
Build servers, shared compilation and node reuse are disabled, and app AOT is
serial. The job timeout is 180 minutes. Game/runtime, native dependency,
package, content and persistence authorities remain unchanged.

## Tested source

| Authority | Immutable commit |
| --- | --- |
| Published Morro vanilla product source | `cc02b0cc697bd620c0dc864d4af3c60d79a95e85` |
| Morro cloud implementation | `98dde8f1036e1e264637faa3d03873706fc3010f` |
| Matching public template feature source | `017778c05b695f666793c2bd9288e4bd962e21db` |
| Matching private acceptance template | `4ec06536b60844b1297b2a7f917ee45ea13177dd` |

The public and private copies match the canonical six-file template exactly.
Report/documentation commits do not change that tested implementation or the
product source pin.

## Local validation

Passed: 84 portable tests, 32 retained cloud input/privacy controls, actionlint
1.7.12, shell syntax for all 17 workflow run blocks and both changed shell
helpers, exact six-file export parity, repository checks and source inventory
verification.

Owned fixtures cover all three platform selections, serial build/verification,
second-build failure, unsigned platform identity, archive containment,
staged/output integrity, privacy and overwrite barriers, and draft asset
verification before publication. The historical Stage 18C verifier retains its
original tvOS/source scope.

## Hosted acceptance

Private [run 37297260328](https://github.com/hmcneill46/celeste-tvos-cloud-builder-rc3-acceptance/actions/runs/37297260328)
completed successfully with `platform=both` on ARM64 `macos-26`, image
`20260907.0351.1`. The exact toolchain and both host doctors passed. Input
validation accepted `itch-macos-fna-1.4.0.0`, canonical class
`celeste-1.4.0.0-a`, and the unchanged official FMOD Engine 1.10.09 build 97915
DMG through both platform validators.

The run performed a tvOS native cache miss and a fresh iOS native build. It
built Release `tvos-arm64`, then Release `ios-arm64`, with full AOT, full
trimming and `UseInterpreter=false`. The actual tvOS IPA passed Stage 14/15/16B
verification. The actual iOS IPA was extracted and its app passed the existing
device/vanilla/unsigned package verifier, including iPhone/iPad families [1,2].

The workflow took **23m53s**, including **20m50s** for serial builds and product
verification, **51s** for publication, and **15s** for final runner cleanup.
These are observations from one run, not predicted build times.

| IPA | Bytes | SHA-256 |
| --- | ---: | --- |
| `Celeste-tvOS-unsigned.ipa` | 895,793,019 | `dce93fb541e81b56c4f83efdcd10e489c3976b0f0fc5b9e269f94503e025f6ba` |
| `Celeste-iOS-unsigned.ipa` | 881,287,784 | `def59eaaac1f769c834d9321d45030fbda7c08f31e04d2cecae787337d908133` |

The published private output contains exactly these two IPAs and their two
build records. Both records were downloaded and their byte counts/checksums
verified against GitHub. Each IPA's server-reported size and SHA-256 matches
its record. This upload check does not claim a separate local download or
physical execution of either IPA. The runner's unconditional cleanup passed;
remote inputs/outputs remain private until explicit cleanup.

Single-platform routing is covered by local fixtures; the live hosted
acceptance exercised the combined selection. Existing cleanup scope and
historical cache-repeat evidence retain their original scope.

## Acceptance boundary

Cloud product verification is separate from signing, installation and physical
gameplay acceptance. Both products are unsigned and require separate signing.
Historical HOST, vanilla, Stage 18C and build-49 physical evidence does not
transfer to these new products. Stage 26 physical testing continues on its own
frozen checkout and installed app.

See [cloud setup and download instructions](../CLOUD_BUILDING.md).

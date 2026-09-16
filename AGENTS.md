# Morro development

Read `README.md`, `docs/MORRO_BUILDING.md`, `docs/MORRO_MIGRATION.md`, the static
Everest architecture/compatibility documents, and the relevant stage report.

- This is the modern fully AOT Apple project. Cabrillo is a separate JIT project.
- Preserve original checkouts, inputs, build-49 signed products and all historical
  HOST/K-M/K-N evidence. Their physical results never transfer to new products.
- Keep the native, game, package, content, semantic and persistence authorities.
  No JIT/interpreter, dynamic device mod loading, or second FMOD Studio system.
- Use process-local Xcode selection and the pinned SDK/workload/compiler set.
  Run app AOT serially; keep node reuse/server/shared compilation disabled.
- `.private`, `.build`, `artifacts` and `dist` are private inputs/output, not source.
  Never track proprietary game/mod/native binaries, saves, credentials, device or
  team identifiers, private logs or absolute personal paths.
- Local migration commits preserve ancestry and freeze the actual tested source.
  Publishing/creating/renaming GitHub repositories, pushes, Actions, signing and
  device operations need the owner's request; the migration authorizes none of them.
- The inherited `upstream` remote has a disabled local push URL. No `origin` is
  configured until the owner chooses the publication destination. Do not repair
  that intentional state by pointing push at the old repository.
- Every tracked file has a reason in `docs/MORRO_FILE_INVENTORY.json`. Update the
  inventory when changing the source layout. Historical verifiers retain their
  original scope; passing current product checks is not physical gameplay GREEN.

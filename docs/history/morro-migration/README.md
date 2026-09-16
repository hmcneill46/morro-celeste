# Morro migration evidence

These sanitized files preserve the completed local migration report and results.
The Markdown publication copy removes one surplus blank line at EOF; its body is
unchanged. The JSON is byte-identical. `inventory.json` binds original and copy
hashes; the original outside-tree files remain untouched. They describe the four fresh unsigned products built from
`6dc6427d1bdeab1a92987a4a1936235e48b7266b`, before publication. Their statements
that publication had not occurred describe that historical run.

- [Migration report](MORRO_MIGRATION_REPORT.md)
- [Machine-readable results](MORRO_MIGRATION_RESULTS.json)

All four product checks passed. Exact IPA byte equality with fresh original-source
builds was **not achieved**; remaining native differences are unresolved. Physical
acceptance of build 49 is still pending. These results are never rebound to a later
source commit merely because a documentation or publication commit is added.

Raw logs, proprietary inputs, generated source and IPAs remain private. See the
[current progress and evidence index](../../MORRO_STATUS.md) for the publication
handoff and earlier stage reports.

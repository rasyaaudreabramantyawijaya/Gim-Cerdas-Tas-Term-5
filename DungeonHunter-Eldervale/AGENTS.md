# Eldervale project guidance

Before making changes to this Unity project, read the portable project skill at
`.agents/skills/eldervale-unity/SKILL.md`, then read `README.md`,
`docs/2026-10-21-eldervale-design.md`, `docs/IMPLEMENTATION-STATUS.md`, and
`docs/ASSET-SOURCES.md`.

This directory is a standalone Unity project. Keep changes inside this project;
do not modify sibling game projects. Preserve Unity `.meta` files and existing
save data. Do not commit generated `Library`, `Temp`, `Builds`, `Logs`, or
`UserSettings` folders. Historical files containing `red` in their name are
earlier failed checks, not the latest result; use timestamps and the status
ledger to determine current verification.

Use Unity Editor `7000.0.0a7` with the package versions pinned in
`Packages/manifest.json`. Useful editor commands are **Eldervale → Play preview**,
**Eldervale → Run full Play checks**, and **Eldervale → Build macOS player**.
Inspect the editor scripts before using scene-generation commands, since they
may replace generated scene contents.

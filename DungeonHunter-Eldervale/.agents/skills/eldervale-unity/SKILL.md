---
name: eldervale-unity
description: Work on, debug, verify, and package the Eldervale Unity game. Use when opening or changing this project, its gameplay, presentation, assets, or builds.
---

# Eldervale Unity Project

## Instructions

### 1. Establish project context

- Work only in this project directory. It is a standalone Unity project in a repository that contains other unrelated student projects.
- Read `AGENTS.md`, `README.md`, `docs/2026-10-21-eldervale-design.md`, `docs/IMPLEMENTATION-STATUS.md`, and `docs/ASSET-SOURCES.md` before substantial changes.
- The project targets Unity `7000.0.0a7` and URP `17.7.0`; retain the versions pinned in `Packages/manifest.json` unless a compatibility change is necessary.
- Treat the status ledger and timestamped evidence as claims about particular runs, not as proof that the current checkout or player has been verified.

### 2. Preserve Unity project integrity

- Keep every existing `.meta` file and its GUID. Do not rename or move imported assets without updating references safely.
- Preserve user saves and settings. Do not reset the project, delete scenes, or overwrite generated assets without checking how the editor builder uses them.
- `Library`, `Temp`, `Obj`, `Builds`, `Logs`, and `UserSettings` are local/generated; do not add them to commits. Imported runtime content belongs in `Assets/`.
- The project has runtime/editor C# code, scenes, URP settings, and visual/audio resources. Inspect relevant scripts and scene dependencies before broad edits.

### 3. Build and test deliberately

- Open the project root (the directory containing `Assets`, `Packages`, and `ProjectSettings`) in Unity Hub.
- Use **Eldervale → Play preview** for a manual play session and **Eldervale → Run full Play checks** for the project's integrated checks. These checks may create temporary save data; inspect their cleanup/restore behavior before changing it.
- Use **Eldervale → Build macOS player** for the configured standalone build. Check `Logs/Editor.log` and the latest `docs/evidence/build-result.txt`; confirm timestamps and success markers correspond to the build just run.
- After gameplay or visual changes, inspect the actual Game view/player. Automated checks do not establish that character clothing, terrain shaders, lighting, map layout, controls, or spatial audio look and feel correct.
- Update `docs/IMPLEMENTATION-STATUS.md` and evidence only with results actually observed in the current run. Keep known limitations visible.

### 4. Respect asset provenance

- Consult `docs/ASSET-SOURCES.md` before adding, replacing, or redistributing assets. Preserve license notices such as `docs/licenses/LICENSE.txt` for Inter.
- Imported Quaternius assets are identified there as CC0; confirm the source and terms for any new asset rather than assuming all web downloads are free to redistribute.
- System-generated temporary character VO is a prototype placeholder; do not claim it is a licensed recording pack. The project does not claim Dolby or HRTF support unless such middleware is actually added and verified.

### 5. Keep the prototype scope clear

- This is a compact coursework prototype, not an AAA production. Keep the overworld and dungeon count small and performance-conscious.
- Preserve the intended exploration/combat flow, readable enemy behavior, responsive controls, and working UI/audio feedback. Avoid adding distracting NPC behavior or effects that interfere with play.
- For feature work, make the smallest coherent change, run the relevant checks, and report what remains unverified instead of presenting intended behavior as completed behavior.

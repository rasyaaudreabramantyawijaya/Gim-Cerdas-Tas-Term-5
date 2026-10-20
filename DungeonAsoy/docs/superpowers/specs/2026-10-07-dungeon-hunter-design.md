# Dungeon Hunter - Prototype Design

> Revisi open map yang diminta pengguna pada 7 Oktober 2026 tersedia di `docs/DUNGEON-OPEN-ESTATE.md`. Dokumen ini mempertahankan rancangan crypt awal untuk riwayat; hasil terbaru ada di `docs/DUNGEON-PROGRESS.md`.

## Goal

Create a third-person Unity dungeon game prototype that can be demonstrated in weekly progress reviews. The player explores a fantasy dungeon, finds a key or artifact, fights enemy archetypes, and reaches a Guardian encounter.

User update on October 7: include a friendly NPC that can be spoken to. Implement a human gatekeeper in the entrance room for the first-week slice; animals are optional future scenery. The user requested the written implementation plan and documentation now, and explicitly authorized execution of stages 1-2 at 04:30 WIB on October 7 if no correction arrives before then. Execution method is direct work in the open Unity project. This authorization does not request Git publication.

The attached 12-page PDF is a visual gameplay reference. Its pages show third-person exploration, combat, and interaction screenshots; they do not add written implementation requirements. The ZIP contains three character screenshots. The character direction is an original anime-inspired fantasy adventurer using the references' pale hair, blue/violet costume, cape-like silhouette, and staff. Do not reproduce a named game character exactly.

## Project Constraints

- Unity project: the root folder of this project checkout.
- Current Unity editor: Unity 7.0 Alpha `7000.0.0a7`, URP project, currently open at `Assets/Scenes/SampleScene.unity`.
- Keep the existing sample scene and the user's current project setting changes intact. Build this prototype in a separate scene and new asset folders.
- Target a desktop keyboard-and-mouse prototype using Unity's installed Input System and built-in Unity components. Avoid introducing packages during the first playable slice.
- Use a third-person follow camera with an over-the-shoulder view and basic orbit control.
- Build a readable greybox dungeon first. Character art should be a custom, stylized prototype model that can be replaced by a properly rigged asset later.

## Core Loop

```text
Explore the dungeon
  -> locate a key or artifact
  -> fight or avoid enemy groups
  -> unlock the sealed route
  -> enter the Guardian chamber
  -> defeat the Guardian and finish the run
```

The task brief calls for melee AI, ranged AI, distance-keeping, flee behavior, and Guardian AI. These will be introduced incrementally across the four stages below.

## Four-Stage Sprint

Assumption: Week 1 is October 5-11, 2026; Week 2 is October 12-18, 2026. Since work starts on October 7, stages 1 and 2 are the current week's deliverables.

### Week 1 - Stage 1: Dungeon Foundation and Player Controller

Deliverables:

- Dedicated `DungeonHunterPrototype` scene; leave `SampleScene` untouched.
- Greybox crypt layout with connected rooms, corridors, walls, floor, and a clearly readable route.
- Third-person camera that follows the player and supports camera orbit.
- Player movement with collision, turn-to-movement, and a simple walk/idle animation.
- Original anime-inspired adventurer prototype silhouette based on the supplied visual direction.
- A short presenter-ready progress note with what works and what is still a placeholder.

Acceptance checks:

- The player can traverse every intended corridor without passing through walls.
- The camera remains behind/around the player and does not drift independently.
- The scene opens and enters Play Mode without compile errors.

### Week 1 - Stage 2: Exploration and First Combat Slice

Deliverables:

- Collectible key or artifact with a visible pickup response.
- Locked exit/door that opens only after the required item is collected.
- Basic health and a melee attack interaction.
- A first melee enemy with a small state flow: idle/patrol, detect/chase, attack, take damage, defeated.
- A friendly human NPC with proximity/line-of-sight interaction and dialogue that changes after the artifact is collected. Dialogue briefly pauses gameplay and restores it when closed.
- HUD for health and objective/item state.
- A playable start-to-exit route for the first progress presentation.

Acceptance checks:

- The objective is understandable without verbal instructions.
- The player can collect the item, fight the enemy, unlock the exit, and finish the slice.
- The player can speak to the gatekeeper near the entrance, receive an artifact hint, and hear a different response after retrieval. Interaction and damage cannot pass through walls.
- Player and enemy collision works against the dungeon geometry.

### Week 2 - Stage 3: Enemy Archetypes and Combat AI

Deliverables:

- Ranged enemy attack with projectile or line-of-sight attack.
- Ranged behavior maintains preferred distance and flees when the player is too close.
- Melee and ranged behavior use explicit, inspector-visible tuning values.
- Enemy detection, attack range, cooldown, damage, and recovery are tested in multiple room layouts.

Acceptance checks:

- Melee enemies close distance before attacking.
- Ranged enemies attack from range, reposition when approached, and do not flee through walls.
- Multiple enemies can engage without clipping through the dungeon walls.

### Week 2 - Stage 4: Guardian, Integration, and Presentation

Deliverables:

- Guardian chamber and a Guardian with at least two readable attack/behavior phases.
- Full objective flow from entry through item retrieval to Guardian defeat.
- Feedback polish for attacks, damage, pickups, locked door, and victory state.
- Playtest notes, known limitations, final screenshots/gameplay capture, and a concise demo script.

Acceptance checks:

- The complete run can be played from a fresh start to a clear win state.
- Guardian states and transitions are observable during the demo.
- The project has no blocking console errors and the final scene is saved separately.

## Stage 1-2 Runtime Structure

Proposed new scripts, grouped by responsibility:

- `DungeonPlayerController.cs`: keyboard input, movement, facing, CharacterController collision.
- `ThirdPersonCameraRig.cs`: follow target, orbit, pitch limits, camera collision handling.
- `DungeonPlayerCombat.cs`: health and melee attack timing/hit detection.
- `DungeonHealth.cs`: shared health and damage state for player/enemies.
- `DungeonSession.cs`: artifact, completion, respawn/restart, and dialogue pause state.
- `DungeonPlayerInteraction.cs`: nearest visible interactable and interaction input.
- `DungeonInteractable.cs`: small shared interface for pickup, door, and NPC.
- `DungeonNpcDialogue.cs`: friendly NPC speech and artifact-dependent responses.
- `DungeonEnemyBrain.cs`: first enemy state flow and shared damage interface.
- `DungeonPickup.cs`: item collection event and item identity.
- `DungeonExitDoor.cs`: locked state and unlock response.
- `DungeonHUD.cs`: health, objective, and pickup feedback.
- `DungeonPrototypeSceneBuilder.cs`: editor-only construction of the greybox scene, materials, and simple prototype character/enemy geometry.

The builder is a convenience for producing repeatable blockout geometry; gameplay behavior remains in runtime scripts. Avoid putting game state and AI logic into editor-only code.

## Risks and Scope Boundaries

- Supplied character references are screenshots rather than a rigged model. Stages 1-2 use an original modular prototype character; professional mesh, facial rigging, and authored animation may need a later art pass.
- Local obstacle avoidance alone may not route enemies around a complete dungeon. Stage 3 should use Unity navigation for global path planning if the installed package supports it; enemy states decide how to behave while navigation handles traversable routes.
- Guardian complexity is reserved for Stage 4 so the first-week presentation can show a complete small gameplay loop.

## Review Checklist

- [x] Matches the requested third-person dungeon exploration premise.
- [x] Includes key/artifact, multiple enemy types, and Guardian in the staged scope.
- [x] Uses the supplied images as style and camera references without claiming they contain hidden written requirements.
- [x] Separates first-week demonstrable progress from next week's planned AI and Guardian work.
- [x] Keeps the existing Unity sample scene and current local project changes out of the prototype scope.

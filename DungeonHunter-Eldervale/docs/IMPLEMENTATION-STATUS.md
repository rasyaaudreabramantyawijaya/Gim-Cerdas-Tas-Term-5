# Execution ledger — plan: docs/superpowers/plans/2026-10-21-eldervale-unity.md

21 October 2026: written spec and implementation plan approved in chat. Native execution selected. New project only; existing UnityRecovery project untouched.

Ruling: this is a fresh standalone directory with no Git repository; no worktree/commits are created. Evidence and task progress are recorded here because the user's local-only constraint takes precedence over skill Git scaffolding.

Pre-flight: Task 1 travel/objective/threat rules are consumed by Tasks 2/4/5; signatures pinned by rules tests. GameFlow owns progress and transitions, presentation sends validated requests, audio reads aggregate AI threat. No duplicate persistence owner.

## Implementasi aktual — 21 Oktober, 15:37 WIB

- Task 1: implemented. Red rules run recorded in docs/rules-red.log; baseline rules passed. Unity 7 requires URP: package 17.7.0 and PC pipeline installed and assigned after actual black-screen failure. Native rendering verified.
- Task 2: implemented. Frontend, compact overworld, three dungeon scenes, four discoverable towers, gated travel, async loading, checkpoints and local atomic save. Integration checks pass.
- Task 3: implemented prototype. Three profiles, actual Quaternius Humanoid mesh/retargeted animation, WASD/arrows/Enter, height-gated glide, melee/projectiles, character/weapon swap, separate party HP, potions and temporary VO. Input feel/animation quality still needs visual rehearsal; live portrait being added.
- Task 4: implemented prototype. LOS/FOV, hearing, last-known memory, NavMesh movement, melee telegraph, ranged maintain-distance/retreat, Guardian phase attack, outdoor Sentinel, 10 wandering/greeting citizens, training duel, shops/upgrades, XP/city reward claim and magnetic loot. Integrated completion path passes; expanded AI sensor checks pending.
- Task 5: implemented prototype. Explore/Alert/Combat/Boss crossfade, pooled positional world cues and centered UI/music, 48 kHz generated effects, wall low-pass, chamber reverb, voice cooldown, direction-test button; wind transforms for grass/pines/hair/capes. No HRTF plugin, certified Dolby, or physical cloth simulation claimed. Recorded source audio retains its native quality.
- Task 6: in progress. Latest whole-run check: **45 PASS / 0 FAIL**, no runtime errors in that run (docs/evidence/play-checks-45-green.txt). Standalone build and final visual review pending.

## Decisions / rulings

Ruling: native Unity screenshot showed missing render pipeline; installed URP rather than relying on removed built-in fallback. PC template settings are copied into this new project; old project was read only.

Ruling: preserve active HP across travel and separate HP for all three party members. Grand Tower/retry restores party. A new integration test failed on unintended teleport healing, then passed after correction.

Ruling: title/menu must be real 3D, not a pasted picture. The generated mountain-waterfall image is only art direction under docs/art-direction; it is not loaded by gameplay. ScenicBackdrop builds grounded cliffs/gate, flowing water mesh, mist particles, pine trees, moving camera and color grade. Inter fonts are bundled under their license. Visual acceptance remains subject to native screenshot review.

Ruling: no paid assets or Adobe login required. Quaternius base characters replace unacquired Mixamo; current characters are stylized humans, not photorealistic scans. VO uses locally generated temporary macOS speech, not voice actors or copied game dialogue.

Ruling: user additions include party setup, coast, level rewards and outdoor boss. Map stays one small 200m overworld plus three interiors. Keyboard mapping follows latest request: Enter jump, arrows camera, high-altitude double Enter glides.

Ruling: tests run inside the already-open editor via editor-only mailbox, preserving and restoring the user's save. Old expected failing-test logs are retained as evidence, not counted as current runtime errors. The initial Unity Search editor exception did not recur in the latest full run.

Remaining verification: final typography/composition, live portrait, actual keyboard play feel, subjective headphone direction/audio mix, standalone startup/build and independent code review. Do not claim a finished AAA/open-world production game.

## Review / polish — 16:05 WIB

Independent read-only review completed. Seven findings reproduced (49 pass / 7 fail) and then fixed with a full **56 pass / 0 fail** run, including isolated editor save slot, fresh combat memory travel guard, pre-intro Guardian protection, persistent Guardian completion, animation restart, trunk navigation footprint and native material disposal. See docs/evidence/review-regressions-green.txt and independent-review.md.

Native visual inspection found pine foliage meshes parenting at world origin; corrected local parenting and imported a small official Quaternius textured-nature subset. Native title now visibly contains grounded gate + two animated waterfalls, Inter hierarchy and minimal menu. A further test caught non-looping walk/idle clips; importer now sets looping explicitly and exact Jump_Start / Spell_Simple_Shoot clips. Camera inversion now affects only input delta, covered by green rules run (73 checks, including reflection implementation guards).

Presentation assets imported, streaming music/mono positional SFX configured, 1K normal maps set, runtime shader material references and a clean sanctuary volume profile retained for standalone stripping. Fresh post-import full checks and build are next.

## Pemulihan dan perbaikan visual — 17:41 WIB

Laptop restart memutus build pertama sebelum laporan selesai; folder .app parsial tidak dihitung sebagai build berhasil. Project dan sumber tetap utuh. Unity dibuka kembali pada project baru yang sama.

Perbaikan tambahan: kanopi bertiang dan display dagangan, gable/frame/jendela rumah, pedestal waystone, penyangga tangga spiral, portal dengan permukaan bergerak dan rune, tekstur jalan sesuai skala, jubah mesh yang mengikuti bahu, siluet pedang dengan guard/grip/pommel, display plinth pada preview karakter, peta dari kamera medan aktual, status tombol party dan satuan pengaturan yang sesuai. Tombol Keluar juga berhenti dari Play Mode saat dipakai di Editor. Boss luar sekarang punya dialog pembuka.

Inspeksi gambar menemukan dua cacat yang tidak tercakup tes awal: material pohon ikut dihancurkan bersama scene sebelumnya, dan reset rotasi menimpa sumbu asli FBX sehingga pohon rebah. Cache material kini milik environment setiap scene; rotasi impor dipertahankan. Kedua regresi tercakup pemeriksaan runtime baru dan sudah lulus. Koreksi tiling sempat mengakses properti tekstur yang tidak ada di shader air; hasil gagal disimpan di texture-tiling-red.txt dan diperbaiki dengan pengecekan properti.

Terakhir tercatat 59 PASS / 0 FAIL setelah koreksi tersebut. Pemeriksaan diulang untuk animasi air/angin unscaled di menu, lalu build standalone dan pemeriksaan UI native. Status final harus merujuk hasil build dan capture aplikasi, bukan keberadaan folder output saja.

## Build profile dan scene preview — 22 Oktober 2026

Pada Unity 7000.0.0a11, shared build scene list kosong walau lima scene game tersedia. Daftar Frontend, EldervaleOverworld, AshenCrypt, WhisperingGrotto, dan VeyrSanctum kini dikonfigurasi lewat EditorBuildSettings API; perpindahan ke EldervaleOverworld tidak lagi gagal karena daftar scene. Scene `Untitled` pada screenshot adalah scene default Unity; ia tidak memiliki WorldBuilder, sehingga menampilkan horizon sederhana tanpa scenery judul. Render pipeline PC URP kini ditetapkan melalui GraphicsSettings dan API Editor aktif. Preview Play native menampilkan menu Frontend dengan scenery pegunungan dan air terjun; scene overworld juga ter-render dengan terrain dan karakter. Pemeriksaan mailbox editor menormalkan pemisah path Windows sebelum memilih assembly editor/runtime.
## Character lighting polish - 22 October 2026

Live preview review of character select found washed-out skin from roughness maps being assigned as URP metallic maps, a close face fill, and portrait lights stacking with the world sun. Character skin is now non-metallic, low-smoothness, and non-emissive; the hero fill is weaker and farther from the face; the portrait studio uses softened key and rim levels; and the portrait layer is excluded from the world sun. Native character-select preview shows legible facial detail without clipped highlights (docs/evidence/character-lighting-review.png). No full Play checks were run for this visual adjustment.

## Overworld environment review - 22 October 2026

Native Play inspection found trees and grass could intersect the river, riverbank spheres used the brick masonry material and floated at a fixed height, and the basin overlapped the town boulevard. The waterfall rocks also floated above the flat valley floor, while the slate roof tint rendered almost black. Vegetation now excludes river/coast footprints; river rocks use a mossy-rock material and sit at sampled terrain height; water and basin positions align with the valley floor and the basin sits west of the boulevard; waterfall crags are grounded and the cascade meets the pool; and slate roofs use a lighter tint. The updated `EldervaleOverworld` loaded in the native editor preview; the captured runtime view is `docs/evidence/native-game-view.png`. The integrated Play check stopped at its earlier character-portrait assertion after 9 passing checks, before reaching the overworld checks; it does not validate this environment change. The user's normal save remained byte-for-byte unchanged during the isolated review run.

## Bridge and river channel review - 22 October 2026

The river was a water plane laid over near-flat terrain, while the short bridge deck sat above the banks without approaches. The overworld heightmap now carves a recessed riverbed, gradual banks, and shallow basins using the same river path as the water meshes. Terrain heights outside those water footprints retain their previous world levels. The bridge now follows the river crossing, spans from bank to bank, includes solid sloped stone approaches and has abutments embedded below the waterline. Unity 7000.0.0a11 compiled the changes, and a native overworld preview was visually inspected; the capture is `docs/evidence/bridge-review.png`. The full Play suite was not rerun because its current portrait assertion stops before the overworld stage.

## Cape torso clearance - 22 October 2026

The procedural cape had only a shallow stand-off near the shoulder line, so its upper sheet could intersect the torso as the rig posed. Increased the shoulder bow and added clearance to the upper cloth that fades toward the hem. Unity 7000.0.0a11 recompiled the scripts successfully; the full Play suite and a post-change visual capture remain pending.

## Overworld castle silhouette - 22 October 2026

The courtyard approach showed a plain curtain wall and rectangular gate. The runtime-built compound now has crenellated walls, four taller corner turrets, a bannered gatehouse with flanking towers, and a roofed keep rising behind the rear wall. The Warden interaction point sits in front of the keep entrance. Visual Play inspection is pending. The project is configured for Unity 7000.0.0a11; an external `dotnet build` could not compile the Unity project because the .NET Framework 4.7.1 reference assemblies are not installed, and no Play checks were run.

## Run animation alignment - 23 October 2026

The high-speed movement state was playing the jog cycle even though the animation asset includes a dedicated sprint cycle. The Run state now uses `Sprint_Loop` in both the Animator controller and the editor setup that wires animation clips. Play preview is pending; no Play checks were run.

## Character select portrait facing - 23 October 2026

The live portrait preview rotated the avatar 180 degrees away from its camera. Removed the extra half-turn while retaining the slow idle turn. Source reviewed; Unity Play preview remains pending.

## Orin lighting balance - 23 October 2026

Orin's pale base texture clipped under the outdoor key light and left a harsh bright-to-shadow split. His body now uses a muted, matte tint and a modest neutral fill light. Source reviewed; Unity Play preview remains pending.

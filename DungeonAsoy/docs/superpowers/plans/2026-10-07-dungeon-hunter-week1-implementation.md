# Dungeon Hunter Week 1 Implementation Plan

> Plan crypt ini adalah rancangan awal. Revisi permintaan pengguna 7 Oktober 2026 mengubah scene menjadi open estate dengan terrain, mansion, checkpoint dan pemanah. Implementasi/verifikasi terbaru mengikuti `docs/DUNGEON-OPEN-ESTATE.md` dan `docs/DUNGEON-PROGRESS.md`; catatan pending di bagian lama adalah riwayat pengerjaan awal.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking. The user selected direct execution in the currently open Unity project.

**Goal:** Menyelesaikan tahap 1 dan 2: dungeon third-person yang dapat dimainkan, karakter stylized, NPC yang bisa diajak ngobrol, artifact, gerbang, dan pertarungan melee dasar.

**Architecture:** CharacterController menangani gerak dan collision. Komponen runtime terpisah menangani kamera, health, combat, interaksi, dialogue, dan objective; satu editor builder membuat scene dan material. Musuh tahap 2 dibatasi pada ruang tempur dengan deteksi line-of-sight; navigasi rute dungeon serta ranged AI diteruskan pada tahap 3.

**Tech Stack:** Unity 7.0 Alpha 7000.0.0a7, URP, C#, Input System yang sudah terpasang, UGUI yang sudah terpasang.

**Spec:** `docs/superpowers/specs/2026-10-07-dungeon-hunter-design.md`

## Global Constraints

- Project: the root folder of this project checkout.
- Tambahkan aset game ke `Assets/DungeonHunter/`; scene baru: `Assets/DungeonHunter/Scenes/DungeonHunterPrototype.unity`.
- Kondisi awal project: `SampleScene` kosong; beberapa ProjectSettings berubah dan file `Assets/Editor/HubForceResolve.cs` beserta metadata telah dihapus oleh pengguna. Hormati kondisi ini.
- Gunakan paket yang tersedia. Jangan mengubah versi Unity atau paket hanya untuk prototype ini.
- Referensi ZIP berisi screenshot, bukan FBX/rig. Buat model prototype humanoid anime dengan rambut terang, pakaian biru/violet, cloak dan staff; nyatakan kualitas prototype secara jujur.
- Dialogue NPC masuk tahap 2. Ranged, maintain distance, flee, dan Guardian aktif tetap minggu kedua.
- Mulai implementasi pukul 04:30 WIB, 7 Oktober 2026, sesuai izin pengguna bila tidak ada instruksi baru yang mengubah arah sebelum waktu tersebut. Tidak perlu meminta approval yang sama lagi.
- Kerjakan langsung di project ini. Jangan commit atau push tanpa permintaan khusus untuk game ini.
- Pemeriksaan utama: compile, scene construction, dan skenario Play Mode nyata. Hasil belum diperiksa harus ditulis sebagai belum diverifikasi.

## Review Focus

1. Kamera dekat dinding: hindari clipping dan tetap tampilkan player.
2. Artifact diambil berulang atau door dipakai sebelum pickup: inventory tidak terduplikasi, door tetap terkunci sebelum syaratnya terpenuhi.
3. NPC/item di balik dinding: proximity saja tidak cukup; cek line-of-sight.
4. Dialogue dibuka saat combat atau ditutup dengan Escape: movement/combat pause dan time scale dipulihkan.
5. Player/enemy mati atau restart setelah kemenangan: objective, health, input dan pause state kembali konsisten.

## Struktur File dan Kontrak

Runtime ada di `Assets/DungeonHunter/Scripts/`, editor builder di `Assets/DungeonHunter/Editor/`.

| File | Kontrak utama |
|---|---|
| DungeonHealth.cs | `Current`, `Maximum`, `IsDead`, `TakeDamage(float amount)`, event `Changed`, event `Died` |
| DungeonSession.cs | `HasArtifact`, `IsComplete`, `IsDialogueOpen`, `DialogueSpeaker`, `DialogueText`, `CollectArtifact()`, `CompleteRun()`, `OpenDialogue(string speaker, string text)`, `SetDialogueOpen(bool open)`, `RestartRun()` |
| DungeonPlayerController.cs | `Configure(Transform cameraTransform, DungeonSession session)`, `PlanarSpeed`; movement + gravity + rotation |
| ThirdPersonCameraRig.cs | `Configure(Transform target, LayerMask geometryMask)`; follow/orbit/pitch/camera SphereCast |
| DungeonCharacterVisual.cs | `Configure(DungeonPlayerController controller)`; walk/idle/staff gesture pada pivot model |
| DungeonPlayerCombat.cs | `Configure(DungeonHealth health, DungeonSession session, LayerMask enemyMask, LayerMask geometryMask)`, `TryAttack()` |
| DungeonEnemyBrain.cs | `Configure(Transform target, DungeonHealth targetHealth, DungeonSession session, LayerMask geometryMask)`; state dan parameter Inspector |
| DungeonInteractable.cs | abstract `GetPrompt(DungeonSession session): string`, `Interact(DungeonSession session)` |
| DungeonPlayerInteraction.cs | `Configure(DungeonSession session, LayerMask interactionMask, LayerMask geometryMask)`; `CurrentTarget`, `CurrentPrompt` |
| DungeonPickup.cs | subclass interactable, satu artifact, visual feedback, panggil `CollectArtifact()` |
| DungeonExitDoor.cs | subclass interactable, cek artifact, buka collider, trigger completion setelah melewati gerbang |
| DungeonNpcDialogue.cs | subclass interactable; pilih teks berdasarkan `HasArtifact`; human gatekeeper ramah |
| DungeonHUD.cs | `Configure(DungeonSession session, DungeonHealth playerHealth, DungeonPlayerInteraction interaction)`; HP/objective/dialogue/death/completion |
| DungeonPrototypeSceneBuilder.cs | editor menu `Dungeon Hunter/Build Week 1 Prototype`; buat scene, model primitive, material, dan referensi komponen |

Jika sebuah interface harus berubah saat coding, perbarui konsumen dan dokumen pada tugas yang sama agar tidak ada kontrak yang tertinggal.

## Task 1 - Scene, Character, Movement, Camera (Tahap 1)

**Files:** `DungeonPlayerController.cs`, `ThirdPersonCameraRig.cs`, `DungeonCharacterVisual.cs`, `DungeonPrototypeSceneBuilder.cs`, scene baru dan material di `Assets/DungeonHunter/`.

**Interfaces:** Menghasilkan player root ber-CharacterController, `VisualRoot` dengan shoulder/hip pivots, dan CameraRig yang bisa dipasang ke target. Session sementara boleh dibuat bersamaan sebagai data bootstrap dan diselesaikan pada task 2.

- [x] Buat scene builder yang meminta konfirmasi sebelum menimpa scene prototype yang sudah diubah. Pembuatan awal boleh berlangsung langsung pada scene baru.
- [x] Bangun crypt blockout: entrance 8x8, hall 18x14, artifact alcove 8x8 di barat, gate corridor di utara; koridor minimal 3.5 unit, wall height 3.2 unit.
- [x] Buat player prototype dengan kepala/wajah, rambut terang, tubuh berpakaian, cloak, tangan, kaki, dan staff. Mesh capsule untuk collision tidak ditampilkan sebagai karakter.
- [x] Movement: WASD relatif kamera, Shift berlari, gravity, CharacterController collision; facing mengikuti arah gerak. Nilai awal walk 4, run 6, turn 540 derajat/detik.
- [x] Camera: jarak awal 4.5, target offset setinggi 1.5, orbit mouse, pitch dibatasi; SphereCast kamera terhadap layer geometry. Hindari asumsi resolusi jendela.
- [x] Walk animation pada pivot tangan/kaki, body bob kecil dan cloak sway; idle mereda saat velocity nol.
- [x] Jalankan builder di editor yang terbuka, simpan scene baru, periksa compile log. Tambahkan scene prototype sebagai entry enabled pada build settings jika belum ada; pertahankan semua entry pengguna agar `RestartRun()` dapat reload scene pada runtime. Jangan meluncurkan editor kedua pada folder yang sedang terkunci.
- [ ] Play Mode: jalan/berlari, bergerak diagonal, berhenti, menghadap, tabrak tiap tipe wall, orbit dekat sudut. Simpan screenshot player dan dungeon yang terlihat jelas; catat PASS/FAIL di progress log.

## Task 2 - Health dan Objective State (Tahap 2)

**Files:** `DungeonHealth.cs`, `DungeonSession.cs`, sambungkan ke player dan builder.

**Interfaces:** Public health/session mengikuti tabel kontrak; session menyediakan data bagi combat, interaction, NPC, HUD dan exit.

- [x] Health player awal 100; clamp damage nonnegatif dan health minimal 0; event `Died` hanya sekali.
- [x] Session mulai tanpa artifact dan belum complete. `CollectArtifact()` idempotent; completion baru sah setelah artifact dan player lewat exit.
- [x] Implementasikan dialogue pause yang menyimpan time scale sebelumnya, dipulihkan saat close, restart, dan component disable.
- [x] Restart mengulang scene prototype dengan health/objective bersih serta time scale normal.
- [ ] Play Mode: damage berulang sampai mati, percobaan damage negatif, dua kali pickup, restart saat dialogue/death/completion; semua status harus konsisten.

## Task 3 - Interaksi, Artifact, Gate, NPC Bicara (Tahap 2)

**Files:** `DungeonInteractable.cs`, `DungeonPlayerInteraction.cs`, `DungeonPickup.cs`, `DungeonExitDoor.cs`, `DungeonNpcDialogue.cs`; update builder.

**Interfaces:** Semua target interaksi memakai abstract base yang sama; input berada pada player, game state berada pada session.

- [x] Deteksi target terdekat maksimal 2.5 unit, abaikan target di balik wall melalui ray/linecast geometry. E mengaktifkan satu target saja; jangan mengaktifkan interaction saat mati/complete/modal terbuka.
- [x] Buat artifact di alcove barat dengan emissive sederhana dan feedback pickup; artifact hilang setelah berhasil dikoleksi.
- [x] Gate menampilkan respons terkunci sebelum artifact, membuka geometri/collider setelah artifact, dan menyelesaikan slice hanya setelah player lewat.
- [x] Buat human gatekeeper di entrance, visual tunik dan idle sederhana, berbeda dari musuh. Player tidak bisa menyerang friendly NPC.
- [x] Dialogue sebelum pickup: "Segel gerbang hanya bisa dibuka dengan artifact di ruang barat. Waspadai penjaganya." Sesudah pickup: "Kamu menemukan artifact. Gerbang utara sekarang bisa dibuka." NPC memanggil `session.OpenDialogue(nama, teks)`; HUD membaca `DialogueSpeaker` dan `DialogueText`.
- [x] Dialogue memiliki tombol Tutup dan Escape; tampilkan nama NPC dan teks yang terbaca. Freeze gameplay saat modal aktif dan restore saat ditutup.
- [ ] Play Mode: bicara dekat NPC; coba interaksi dari balik wall; bicara setelah artifact; tutup dengan tombol/Escape; buka/tutup berulang. Kamera/input dan time scale tetap benar.

## Task 4 - First Melee Combat Slice (Tahap 2)

**Files:** `DungeonPlayerCombat.cs`, `DungeonEnemyBrain.cs`; sambungkan health, visual, geometry masks.

**Interfaces:** Player attack hanya mengenali enemy health. Enemy brain menerima target/player health/session dan mengekspos state di Inspector.

- [x] Player attack dari klik kiri, cooldown awal 0.65 detik, range 1.8, damage 25; satu target hanya terkena sekali per swing. Cek arah dan wall occlusion, sinkronkan staff gesture dengan hit window.
- [x] Enemy di combat hall: HP 60, deteksi 7, attack range 1.6, cooldown 1.2 detik, damage 12, speed 2.4.
- [x] State `Idle`, `Chase`, `Attack`, `Dead`; deteksi membutuhkan LOS, attack punya windup yang terbaca, health nol menonaktifkan combat.
- [x] Movement enemy memakai CharacterController dan obstacle sensing; enemy tidak boleh berjalan atau memberi damage melewati wall. Tahap 2 belum mengklaim global dungeon pathfinding.
- [ ] Play Mode: engage/chase/attack, player swing berhasil, musuh mati, tidak ada attack/damage menembus wall, NPC friendly tetap aman. Uji pause dialogue saat musuh dekat.

## Task 5 - HUD dan Alur Start-to-Exit (Tahap 2)

**Files:** `DungeonHUD.cs`, builder dan pengaitan referensi pada scene.

**Interfaces:** HUD membaca health/session/interaction; tombol dialogue/restart memanggil session tanpa menyimpan salinan objective.

- [x] HUD UGUI: HP, objective "Temukan artifact", status pickup, nama/prompt target interaksi, dialogue panel, defeat dan completion panel.
- [x] Sesuaikan CanvasScaler agar teks tidak terpotong pada jendela Game kecil dan layar desktop. Utamakan informasi gameplay dengan kontras yang cukup.
- [ ] Mainkan satu run penuh: bicara ke NPC, jelajahi hall, lawan enemy, ambil artifact, dengar dialogue baru, buka gate, lewati exit, restart.
- [ ] Rekam bukti aktual berupa screenshot dan ringkasan hasil; bukti yang belum direkam diberi status pending.

## Task 6 - Dokumentasi dan Handoff Minggu Pertama

**Files:** `docs/DUNGEON-SPRINT.md`, `docs/DUNGEON-PROGRESS.md`, `docs/DUNGEON-DEMO-MINGGU1.md`, `README-DungeonHunter.md`.

- [x] Perbarui status tahap berdasarkan status implementasi dan bukti yang tersedia; gameplay belum dinyatakan teruji.
- [x] Isi daftar script/metode nyata dan alur input -> gerak/interaksi -> session -> HUD.
- [x] Isi control map yang sesuai kode dan scene path yang benar.
- [x] Buat naskah presentasi sekitar 2-3 menit yang menandai pemeriksaan runtime pending, termasuk batasan prototype model dan enemy navigation.
- [x] Catat backlog minggu kedua: ranged + maintain distance/flee; Guardian + complete run + polish.
- [x] Laporkan hasil yang terverifikasi, lokasi scene, cara membuka, bukti pemeriksaan, dan kendala yang masih tersisa kepada pengguna.

### Execution Evidence - 7 October 2026

- Unity reloaded the changed scripts, generated `Assets/DungeonHunter/Scenes/DungeonHunterPrototype.unity`, opened the saved scene, and logged builder completion. Static scene scan confirms the required components exist.
- Build Settings contains the existing SampleScene and the Dungeon Hunter prototype scene with its correct asset GUID. The builder now saves the scene before updating build settings so future rebuilds retain that GUID.
- Play Mode, NUnit Test Runner, visual inspection, and end-to-end user input were not verified because the Mac session was locked and the Test Runner result UI was inaccessible. Do not represent them as passing checks.

## Self-Review Plan

- [x] Tahap 1-2 dan tambahan NPC punya tugas serta acceptance scenario.
- [x] Semua interface yang dikonsumsi tercantum dalam tabel kontrak.
- [x] Review Focus punya skenario pemeriksaan pada task terkait.
- [x] Native execution sudah dipilih melalui instruksi pengguna untuk mengerjakan langsung di Unity.
- [x] Waktu izin eksekusi tercatat; koreksi terbaru pengguna tetap menjadi arahan utama.

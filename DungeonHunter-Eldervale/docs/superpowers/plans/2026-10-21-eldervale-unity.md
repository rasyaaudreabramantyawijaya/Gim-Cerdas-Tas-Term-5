# Eldervale Unity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Project Unity baru yang dapat dimainkan dengan tiga dungeon, map/teleport, loading bergerak, AI, audio contextual, karakter pilihan dan angin.

**Architecture:** Bootstrap memelihara progress, audio dan transisi; scene overworld dan tiga interior berdiri sendiri. Runtime components dipisah antara player, AI, presentation, navigation, wind, serta rules yang dapat diuji tanpa scene.

**Tech Stack:** Unity lokal 7000.0.0a7, C#, Unity UI, NavMesh, AudioSource; URP diprioritaskan bila paket editor lokal mendukung pembuatan pipeline.

**Spec:** ../../2026-10-21-eldervale-design.md

## Global Constraints

- Engine wajib Unity; project baru terpisah Documents/DungeonHunter-Eldervale.
- Satu overworld kecil dan tiga interior; third-person single-player.
- Tidak mengubah project UnityRecovery/DungeonHunter.
- Aset hanya sumber gratis resmi yang sudah diperiksa; jangan menyatakan rig/voice/final visual telah diterima sebelum diperiksa di Unity.
- Perubahan lokal, tanpa commit/push.
- Tambahan disetujui pengguna: loading bergerak bergaya perjalanan menuju gerbang; semua tombol/action menu, ganti karakter/senjata, masuk dungeon memiliki cue.
- Referensi video Celestia Door dipakai sebagai arah animasi perjalanan/gerbang; komposisi, logo, musik dan aset dibuat sendiri.

## Review Focus

- Teleport ketika combat/death/loading harus ditolak; panggilan berulang tidak menggandakan transisi.
- Dua enemy berbeda state tidak boleh membuat musik turun ke Explore ketika salah satunya masih engaged.
- Pause/map/settings harus memblokir gerak/serangan sambil membiarkan UI dan SFX berfungsi.
- Kembali dari dungeon atau Continue harus memulihkan spawn/progress/karakter dan mencegah reward ganda.
- Teleport tidak boleh melontarkan rambut/kain; audio dan jumlah object persistensi tidak bertambah tiap scene.

## Task 1: Fondasi project dan rules

Files: Assets/Eldervale/Scripts/RunRules.cs, Assets/Eldervale/Editor/EldervaleVerification.cs, Packages/manifest.json, ProjectSettings, Assets/Eldervale/Scenes.
Interfaces: RunRules.CanTravel(known, combat, alive, busy, inWorld), RunRules.CanOpenSanctum, ThreatMusic.Resolve.
- [ ] Tulis pemeriksaan rules untuk tujuan terkunci, combat, death, busy, sigil/Guardian dan threat agregat.
- [ ] Jalankan red checks yang membuktikan rule belum tersedia.
- [ ] Buat fondasi project dan implementasikan rules serta state save berversi.
- [ ] Jalankan checks dan compiler Unity; simpan output aktual.

## Task 2: Map, portal dan loading bergerak

Files: Scripts/GameFlow.cs, Scripts/WorldPortal.cs, Scripts/Presentation.cs, Editor/EldervaleSceneBuilder.cs.
Interfaces: GameFlow.Travel(sceneId, spawnId), GameFlow.InteractPortal(dungeonId), Presentation.ShowMap.
- [ ] Tambah checks tujuan terkunci, permintaan ganda, save/return point dan input saat overlay.
- [ ] Buat Frontend/Overworld/tiga interior dengan waypoint, UI map dan async SceneManager loading.
- [ ] Loading memakai kabut/partikel, gerbang berlapis dan kamera/parallax bergerak secara orisinal; actual progress dilabeli.
- [ ] Setiap tombol terpusat memainkan UI hover/click/back/error; character/weapon/dungeon/teleport memiliki cue berbeda.
- [ ] Periksa alur start → character select → load → portal → return, dan transisi berulang.

## Task 3: Player, karakter, combat dan VO

Files: Scripts/HunterPlayer.cs, Scripts/HumanoidView.cs, Scripts/HunterProjectile.cs, Scripts/CharacterVoice.cs.
Interfaces: HunterPlayer.Damage, Cast, ChangeWeapon; CharacterVoice.Play(context).
- [ ] Tambah checks HP/death, cooldown, hit tunggal dan ganti karakter/weapon cue.
- [ ] Import subset model Humanoid dari asset resmi; tiga profil visual/voice, locomotion, jump/melee/cast/death.
- [ ] Hubungkan gerakan, gravity/collision, melee/projectile dan waypoint/NPC/pickup interaction.
- [ ] Pasang voice profile, subtitle, anti-repeat/cooldown; catat voice clips yang belum tersedia.
- [ ] Jalankan Play Mode checks untuk input/physics/combat dan periksa render karakter.

## Task 4: AI, tactical dan objectives

Files: Scripts/EnemyAgent.cs, Scripts/WorldObjective.cs, Scripts/WorldNavigation.cs.
Interfaces: EnemyAgent.Threat, LastKnown, State, Damage; WorldObjective.Collect/Complete.
- [ ] Checks LOS terhalang, memory decay, path valid, maintain-distance/retreat, boss arena leash dan win gate.
- [ ] Buat sensor+FSM, runtime/baked NavMesh, melee/ranged dengan telegraph dan Guardian dua fase.
- [ ] Dua sigil membuka Sanctum; boss mati lalu artifact dan extraction memberi kemenangan.
- [ ] F3 menampilkan sensor/memory/state/path; verifikasi kematian, retry/checkpoint dan reward idempotent.

## Task 5: Audio dan angin

Files: Scripts/GameAudio.cs, Scripts/WindMotion.cs, Scripts/CharacterVoice.cs.
Interfaces: GameAudio.Cue(id,pos), GameAudio.SetThreat; WindMotion.ResetAfterTravel.
- [ ] Checks Explore/Alert/Combat priority dan hysteresis, voice cooldown, footstep hanya grounded/bergerak.
- [ ] Import musik/SFX terpilih, AudioSource pool dan mixer volume/settings; cue teleport/skill/damage/death.
- [ ] Implementasikan gust bersama untuk rumput/pohon dan secondary motion rambut/kain sesuai aset.
- [ ] Periksa reset secondary motion, crossfade dan source count pada beberapa transisi.

## Task 6: Verification dan handoff

Files: docs/evidence, README.md, docs/IMPLEMENTATION-STATUS.md.
- [ ] Jalankan compiler, rules suite, scene/Play Mode checks dan build standalone.
- [ ] Periksa capture menu/loading/map, tiga dungeon serta karakter/AI/wind.
- [ ] Catat requirement mana terverifikasi otomatis, mana memerlukan pemeriksaan audio/feel manual, serta aset yang masih sementara.
- [ ] Serahkan path project, scene start, controls, build dan batasan yang terbukti.


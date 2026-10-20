# Demo Minggu Pertama - Open Estate

Prototype telah melewati pemeriksaan programatik dalam Play Mode. Lakukan rehearsal kontrol mouse/WASD dan urutan di bawah sebelum presentasi; tes otomatis tidak mengukur kenyamanan bermain atau kualitas animasi final. Bukti ada di `docs/evidence/` dan rincian pada `DUNGEON-PROGRESS.md`.

## Urutan Demo 2-3 Menit

1. Buka `Assets/DungeonHunter/Scenes/DungeonHunterPrototype.unity`, tekan Play dan tampilkan vista mansion/terrain.
2. WASD dan Shift untuk menuju NPC dekat camp; E membuka dialogue.
3. Menuju menara kiri; E pada beacon menara mengaktifkan checkpoint.
4. Dekati pemanah courtyard: tunjukkan panah dan bersembunyi di balik pilar atau wall mansion.
5. Masuk mansion melalui pintu tengah; artifact ada di ruangan/sayap kiri di belakang.
6. E mengambil artifact. Buka gate di tengah bagian belakang menggunakan E, lalu berjalan melewatinya sampai exit.
7. Jelaskan bahwa R ketika mati mengembalikan player ke checkpoint; R setelah menang memulai run baru.
8. Tutup dengan rencana navigasi, variasi enemy melee, Guardian dan polishing pada minggu kedua.

## Naskah Target

"Proyek saya adalah Dungeon Hunter, dengan lokasi perkebunan tua dan mansion terbengkalai. Player masuk dari camp, mencari artifact di dalam mansion dan membuka gerbang belakang. Di luar mansion ada tanah terbuka, bukit dan dua menara checkpoint.

Tahap pertama menghubungkan input, gerak dan lingkungan. `DungeonPlayerController` membaca WASD, mengambil arah kamera lalu memanggil `CharacterController.Move`. TerrainCollider menjadi pijakan dan collider bangunan menahan player. Saya menemukan bug ketika collider lantai dihapus oleh builder; setelah diperbaiki, tes grounding dalam Play Mode memastikan player tidak lagi jatuh menembus tanah.

Tahap kedua menyatukan interaksi dan progres. Menekan E memilih objek terdekat yang tidak terhalang wall. NPC memberi petunjuk, checkpoint menyimpan posisi respawn dan memulihkan HP, sementara artifact mengubah state pada `DungeonSession`. State itu dipakai kembali oleh dialogue, HUD dan gate agar semuanya konsisten.

Enemy saat ini adalah pemanah. `DungeonEnemyBrain.UpdateArcher` memeriksa jarak dan line-of-sight, mendekat ketika player terlalu jauh, mundur jika terlalu dekat, lalu meluncurkan panah setelah windup. `DungeonArrow.Update` melakukan SphereCast sepanjang perpindahan frame, sehingga panah berhenti pada tembok dan HP hanya berkurang jika panah benar-benar mengenai player.

Untuk alur akhir, gate tetap terkunci sebelum artifact diperoleh. Membuka gate tidak langsung memenangkan game; player harus melewati exit trigger. Sistem checkpoint memulihkan player di dalam run yang sama, sedangkan restart sesudah menang mengulang seluruh scene.

Hasil minggu pertama masih menggunakan model procedural prototype. Pemeriksaan Play Mode membuktikan grounding, wall collision, dialogue, checkpoint, panah, artifact, gate dan exit. Minggu kedua berfokus pada navigasi enemy yang lebih baik, variasi melee, Guardian dan pemolesan visual serta animasi."

## Kode yang Ditunjukkan

| Bagian | File/metode yang dituju | Yang dijelaskan |
|---|---|---|
| Input dan movement | `DungeonPlayerController.Update` | Input relatif kamera -> gravity -> CharacterController -> facing |
| Terrain/map | `BuildEstateTerrain`, `BuildMansion` | TerrainData/TerrainCollider, mesh dan collider bangunan |
| Kamera | `ThirdPersonCameraRig.LateUpdate` | Follow, orbit dan SphereCast kamera |
| Enemy | `DungeonEnemyBrain.UpdateArcher` | Jarak, LOS, windup dan projectile |
| Projectile | `DungeonArrow.Update` | SphereCast per-frame, wall occlusion dan damage |
| Checkpoint | `DungeonCheckpoint.Interact`, `DungeonPlayerRespawn.Respawn` | Spawn tersimpan, teleport aman, HP dipulihkan |
| NPC | `DungeonNpcDialogue.Interact` | Dialogue berdasarkan HasArtifact |
| Objective | `DungeonSession`, `DungeonExitDoor.Interact`, `DungeonExitTrigger.OnTriggerEnter` | Artifact -> gate terbuka -> exit |
| Interaksi | `DungeonPlayerInteraction.Update`, `HasLineOfSight` | Objek terdekat terlihat; E melakukan satu interaksi |

Saat menjawab pertanyaan dosen, tunjukkan hubungan antarmetode lalu demonstrasikan perilakunya; jangan hanya membacakan daftar script. Local avoidance saat ini dapat berhenti di sudut dan belum memilih rute global.

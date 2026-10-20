# Dungeon Hunter - Open Estate Revision

## Brief Perubahan

Permintaan 7 Oktober 2026: player tetap memakai visual humanoid sekarang; perbaiki jatuh melalui lantai; ubah lokasi menjadi open map terbatas dengan terrain bervariasi, mansion terbengkalai, menara checkpoint, dan musuh pemanah.

Interpretasi menara/turret: menara batu sebagai landmark checkpoint, bukan meriam otomatis. Terrain berukuran 140 x 140 unit dengan meadow datar di tengah serta bukit di tepi. Mansion dapat dimasuki dari depan, artifact berada di sayap barat, dan gate berada di bagian belakang.

## Alur Main

1. Spawn di camp selatan dan bicara dengan NPC penjaga.
2. Jelajahi jalan masuk; aktifkan salah satu dari dua menara menggunakan E.
3. Hindari panah pemanah courtyard dan masuk mansion.
4. Ambil artifact di sayap barat; gunakan staff melee untuk melawan pemanah.
5. Buka gate belakang mansion dan lewati exit di utara.
6. Jika mati, R mengembalikan player ke checkpoint terakhir dan memulihkan HP. Setelah kemenangan, R memulai run baru.

## Implementasi

| Bagian | Kode |
|---|---|
| Terrain dan mansion | `DungeonOpenEstateBuilder.BuildOpenEstate`, `BuildEstateTerrain`, `BuildMansion` |
| Collision | TerrainCollider, BoxCollider pada fondasi/lantai dan `CharacterController.Move` |
| Checkpoint | `DungeonCheckpoint.Interact` menetapkan spawn pada `DungeonPlayerRespawn` |
| Respawn | `DungeonPlayerRespawn.Respawn` memulihkan HP dan memanggil `DungeonPlayerController.Teleport` |
| Enemy panah | `DungeonEnemyBrain.UpdateArcher`: jangkauan, jarak aman, windup, projectile |
| Panah | `DungeonArrow.Update`: swept SphereCast setiap frame, hentikan panah pada collider pertama |
| Dialogue/HUD | Subscriber event dipasang saat runtime melalui Awake; Canvas membaca state session dan health |
| Objective | `DungeonPickup`, `DungeonExitDoor.IsOpen`, `DungeonExitTrigger` dan `DungeonSession` |

## Penyebab Bug Lantai

Builder crypt sebelumnya memakai `Box(..., geometry: false)` untuk lima lantai. Parameter itu menghapus collider. Tes raycast di bawah player pada scene lama menghasilkan FAIL dua kali. Kode lantai crypt dikoreksi agar mempertahankan collider; map baru menggunakan TerrainCollider serta fondasi mansion yang solid.

## Pemeriksaan

Menu `Dungeon Hunter > Verify Ground Collision` menguji adanya permukaan solid di bawah player.

Menu `Dungeon Hunter > Run Open Estate Play Mode Checks` memasuki Play Mode untuk mengecek grounding setelah gravity, collision mansion, event HUD, dialogue pause/close, checkpoint/respawn, projectile terhadap player dan wall, artifact, gate dan exit. Pemeriksaan menggerakkan player secara programatik; pemeriksaan kenyamanan kontrol manual tetap dilakukan terpisah.

Hasil tersimpan di `docs/evidence/ground-regression.txt` dan `docs/evidence/estate-playmode-checks.txt`; capture di `docs/evidence/estate-playmode-start.png` dan `estate-camera.png`.

Hasil terakhir: 17 PASS, mencakup input W dan animasi kaki selain alur komponen. Panorama terrain/mansion tersedia di `docs/evidence/estate-overview.png`. Verifier memakai InputSettings clone selama simulasi keyboard agar fokus editor tidak menghilangkan input uji; pengaturan/perangkat asli dipulihkan saat selesai.

Scene lama disimpan sebagai `Assets/DungeonHunter/Scenes/DungeonHunterBeforeEstate-20261007-052029.unity`. Scene aktif tetap `DungeonHunterPrototype.unity`.

## Batas Prototype

Bangunan, pohon, batu dan karakter merupakan model procedural prototype. Checkpoint mempertahankan progres artifact pada respawn dalam run yang sama. Musuh memakai movement lokal dengan collision; global pathfinding, variasi melee lengkap, Guardian dan polishing animasi tetap untuk tahap berikutnya. Pemanah serta maintain-distance dimajukan dari minggu kedua sesuai permintaan revisi.

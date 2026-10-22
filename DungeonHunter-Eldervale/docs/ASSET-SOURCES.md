# Asset sources dan Unity intake

Diperiksa: 21 Oktober 2026. Download dan import adalah dua status berbeda; lihat AssetStaging/intake-manifest.json untuk file yang benar-benar tersedia.

| Kebutuhan | Sumber primer | Lisensi pada halaman | Keputusan |
|---|---|---|---|
| Humanoid manusia dan animasi | https://www.mixamo.com/ ; https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html | Adobe mengizinkan royalty-free pada personal/commercial/nonprofit game | Utama; memerlukan Adobe ID dan pemilihan model. Belum diunduh |
| Humanoid yang dipakai | https://quaternius.com/packs/universalbasecharacters.html | CC0; Standard dan Source berbeda | Male/female FullBody dan tiga rambut FBX diimpor; Humanoid dan controller lolos Play checks |
| Outfit fantasy | https://quaternius.com/packs/modularcharacteroutfitsfantasy.html | CC0; Standard subset gratis | Kandidat outfit; daftar yang tersedia perlu diperiksa dari ZIP |
| Terrain PBR | https://polyhaven.com/a/rocky_terrain | CC0, Amal Kumar | Intake 1K diffuse, OpenGL normal, roughness, AO |
| Dinding dungeon PBR | https://polyhaven.com/a/stone_brick_wall_001 | CC0, Dimitrios Savva/Rico Cilliers | Intake 1K diffuse, OpenGL normal, roughness, AO |
| Impact | https://kenney.nl/assets/impact-sounds | CC0 | Intake ZIP; audition sebelum pilih hit/landing |
| Gerak dan weapon | https://kenney.nl/assets/rpg-audio | CC0 | Intake ZIP; pilih sample berdasarkan nama dan dengarkan |
| UI | https://kenney.nl/assets/interface-sounds | CC0 | Intake ZIP |
| Musik explore/shrine | https://kmontesdev.itch.io/7-fantasy-music-tracks | CC0 pada halaman kreator | Kandidat, ambil file gratis resmi bila endpoint tersedia |
| Musik combat | https://opengameart.org/content/battle-theme-b-for-rpg | CC0, cynicmusic | Kandidat combat; perlu audition dan loop check |
| Layout dungeon cadangan | https://rgsdev.itch.io/free-modular-low-poly-dungeon-pack-by-rgsdev | CC0, RGS_Dev | Low-poly, tidak mewakili target realisme; staged jika tersedia |
| Retarget animation | https://quaternius.com/packs/universalanimationlibrary.html | CC0, Quaternius | UAL1 Standard FBX diimpor untuk idle/walk/jump/attack/cast/death |
| Mossy cliffs | https://polyhaven.com/a/mossy_rock | CC0 | Diffuse dan OpenGL normal 1K diimpor untuk geometri tebing |
| Forest floor | https://polyhaven.com/a/forest_ground_04 | CC0 | Diffuse dan OpenGL normal 1K untuk terrain dan title valley |
| Sky lighting | https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky | CC0 | HDR 1K; hanya langit/lighting, bukan gambar landscape pengganti geometri |
| Typography | https://github.com/rsms/inter/releases/tag/v4.1 | SIL OFL 1.1 | Static Inter/Inter Display TTF dibundel, lisensi docs/licenses/LICENSE.txt |
| Textured nature | https://quaternius.com/packs/ultimatestylizednature.html | CC0, Quaternius | BirchTree_1, NormalTree_1, Grass_Small FBX + bark/leaf/grass textures, dari folder publik Google Drive yang ditautkan kreator. Import subset saja |

## Status aktual import

Kenney SFX OGG dan musik kmontesdev/cynicmusic sudah diimpor. Mixamo tidak diunduh dan tidak diperlukan untuk menjalankan prototype. Fantasy-outfit pack belum berhasil diambil; jangan mengklaim outfit final tersedia. RGS dungeon ZIP hanya staged, bukan environment runtime. Seluruh geometry title/loading dibuat di Unity; gambar konsep berada di docs/art-direction dan tidak diimpor ke Resources.

VO temporary dibuat lokal memakai macOS `say` dengan suara sistem Daniel, Samantha, Rishi dan Damayanti, memakai kalimat orisinal. Ini placeholder untuk evaluasi, bukan paket aktor/rekaman final; verifikasi hak distribusi suara sistem sebelum rilis publik atau ganti dengan rekaman berlisensi. Project/build lokal untuk review tidak mengklaim izin publikasi suara komersial.

Link yang dikirim pengguna https://github.com/itchio/itch adalah source aplikasi desktop itch, bukan perpustakaan model dungeon. Tidak perlu clone repository itu untuk mengambil aset.

## Import Unity

Gunakan FBX untuk karakter/model bila tersedia; inspect rig Humanoid dan Avatar, scale manusia sekitar 1,7–1,9 meter, feet grounded, normals, materials serta retarget animation. Mesh rambut/baju perlu tulang atau topology tambahan untuk secondary motion; download humanoid tidak otomatis menyediakannya.
Pilih clip in-place bila movement digerakkan controller. Periksa event langkah/hit/cast, locomotion blend dan root motion agar tidak terjadi double movement.

Untuk PBR URP: diffuse sRGB aktif, normal sebagai Normal Map, AO/roughness sRGB mati. Roughness tidak langsung dipasang ke slot smoothness; gunakan 1 − roughness dan packing channel yang benar bila memakai metallic/smoothness map. JPG 1K adalah starter ringan; source normal PNG lebih baik untuk final bila artifact kompresi terlihat. Tiling dunia memakai satuan meter; jangan meregangkan tekstur seluruh terrain.
Shader/material format engine lain tidak otomatis cocok dengan URP. Gunakan hanya file model/audio/texture yang dibutuhkan, jangan impor seluruh contoh project lain.

Audio: musik panjang compressed streaming, SFX pendek preload/kompresi sesuai profil; test spatial attenuation. Sample bank belum membuktikan tersedia semua teleport, skill atau VO. Karakter memerlukan voice recording/licensed bank terpisah; naskah desain bukan audio.

## Visual orisinal

eldervale-map.svg adalah rancangan peta untuk review, bukan screenshot runtime. Portal/waypoint/sigil dan loading art dapat dibuat orisinal. Model manusia rigged final tidak dapat dijanjikan hanya dari screenshot.

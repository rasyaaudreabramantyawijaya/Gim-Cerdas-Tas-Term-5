# Dungeon Hunter: Relic of Veyr — spesifikasi Unity

Tanggal: 21 Oktober 2026, Asia/Jakarta.
Status: review desain; gameplay belum diimplementasikan.
Project baru terpisah: Documents/DungeonHunter-Eldervale.
Keputusan pengguna: gunakan Unity, beberapa gerbang dungeon, open map kecil, peta yang bisa dibuka, teleport, loading, humanoid menyerupai manusia, settings, audio adaptif, suara karakter, serta angin pada karakter dan lingkungan.

## 1. Tujuan dan scope

Game tugas Intelligent Enemy Dungeon. Pemain memilih seorang pemburu artifact, menjelajahi satu wilayah kecil bernama Eldervale, mengaktifkan waypoint, membuka tiga dungeon, mengumpulkan dua sigil, lalu mengalahkan Guardian dan membawa artifact pulang. Target durasi satu run: 15–25 menit; ini target desain, belum pengukuran.

Pilihan pendekatan:
1. Satu overworld kecil + tiga interior pendek terpisah: dipilih; mendukung perjalanan terbuka, teleport, dan loading yang jelas.
2. Semua ruangan berada di satu scene: perpindahan cepat, tetapi isolasi audio, lighting, NavMesh, dan reset encounter lebih sulit.
3. Beberapa wilayah besar: ditunda karena pengguna meminta jumlah map sedikit.

Asumsi kerja: third-person, satu pemain, keyboard/mouse, tiga karakter pilihan dengan moveset dasar yang sama. Tidak ada multiplayer. Target visual dark fantasy semi-realistis; proporsi manusia, rig, material dan animasi harus diperiksa di game.

## 2. Dunia, gerbang dan alur

Overworld: satu terrain sekitar 200 × 200 meter. Ukuran dapat turun bila gameplay terasa kosong. Bukit, pohon, batu dan reruntuhan membingkai jalur pendek; landmark bisa dikenali dari tempat tinggi. Dungeon masuk ke gunung melalui portal fisik; interior scene terpisah memungkinkan loading.

| Lokasi | Isi dan objective | Gerbang/waypoint | Target durasi |
|---|---|---|---|
| Hunter Camp | Spawn, pilih tujuan, NPC penjaga, tempat kembali | Waypoint aktif awal | 1–2 menit |
| Ashen Crypt | Dua ruangan, melee patrol dan investigasi; ambil Sigil Bara | Gerbang di bukit barat | 3–5 menit |
| Whispering Grotto | Dua ruangan, ranged maintain-distance dan retreat; ambil Sigil Kabut | Gerbang di tebing timur | 3–5 menit |
| Veyr Sanctum | Ruang pendek dan arena Guardian; ambil Relic of Veyr | Gerbang utara terbuka setelah dua sigil | 5–8 menit |

Ashen Crypt dan Whispering Grotto dapat diselesaikan dalam urutan apa pun. Veyr Sanctum terlihat di peta setelah ditemukan, tetapi alasan terkunci ditampilkan. Tidak perlu membunuh semua enemy luar agar dapat masuk; interaksi portal diblokir selama combat aktif.

Win: dua sigil terkumpul, Guardian mati, artifact diperoleh, kembali ke Hunter Camp dan berinteraksi pada altar.
Lose: HP player mencapai nol. Layar kalah memberi Retry Checkpoint atau Main Menu. Tidak ada timer wajib.
Checkpoint dipicu setelah memasuki dungeon dan setelah kembali ke overworld; kematian mereset encounter dungeon aktif, bukan menggandakan reward.

## 3. Map dan teleport

M membuka/menutup peta. UI menampilkan player, arah utara, objective, nama dungeon, marker belum ditemukan/ditemukan/selesai, serta status gerbang terkunci. Peta memakai bounds overworld dan posisi ternormalisasi, sehingga ukuran world tidak bergantung pada piksel UI.

Empat waypoint: camp dan satu di luar masing-masing gerbang. Waypoint dungeon harus diaktifkan dengan E sebelum muncul sebagai tujuan teleport. Setiap marker punya label serta ikon selain warna.

Teleport dimulai dari peta saat di overworld, di luar combat, player hidup, dan tujuan sudah diaktifkan. Di dalam dungeon, gunakan exit portal untuk kembali ke waypoint gerbang; teleport peta diblokir. Permintaan ganda diblokir selama transisi.
Validasi tujuan dan spawn point dilakukan sebelum memindah player. Jika gagal, tampilkan pesan dan pulihkan kendali; jangan meninggalkan black screen.

## 4. Unity scene dan state

Scene rencana: Bootstrap, Frontend, EldervaleOverworld, AshenCrypt, WhisperingGrotto, VeyrSanctum. Bootstrap membawa service persistensi dan UI loading; hanya satu service instance boleh aktif. Ini satu overworld dengan tiga interior, bukan beberapa open world besar.

Alur: Main Menu → Character Select → Loading → Camp → eksplorasi/dungeon → Guardian → extraction → Victory.
Continue menggunakan checkpoint valid; New Game mereset run dengan konfirmasi UI.
Scene lama di UnityRecovery tidak diubah.

Komponen utama:
- GameFlow: alur menu, pause, hidup/mati/menang dan kontrol input.
- RunProgress: sigil, artifact, dungeon completion, waypoint, checkpoint, karakter.
- SceneTransition: fade, async loading, spawn resolver dan recovery error.
- WorldMap dan Waypoint: peta, discovery dan permintaan teleport yang tervalidasi.
- PlayerMotor, PlayerCombat, PlayerSkills, PlayerHealth: gerakan, damage, cooldown, resource.
- EnemyPerception, EnemyMemory, EnemyBrain, EnemyNavigation: data sensor, memory dan perilaku.
- AudioDirector, CharacterVoice, FootstepSurface: musik dan suara contextual.
- WindDirector dan adapters: angin yang sama untuk terrain, pohon dan aksesori karakter.

Data karakter, enemy, dungeon, skill, voice dan audio cue diletakkan di ScriptableObject. Transisi memakai ID scene/spawn stabil; save menyimpan ID, tidak menyimpan object reference runtime. Save lokal bertanda versi, ditulis secara atomik dan dicek saat Continue. Versi save tidak cocok memberi pesan dan pilihan New Game.

## 5. Player dan karakter pilihan

Tiga pilihan rancangan:
- Kael, hunter pedang: suara rendah, tenang, celetukan observasional.
- Lyra, hunter ranger: suara ringan, waspada, menyebut arah ancaman.
- Orin, hunter rune: suara berat, sedikit humor, komentar artifact.

Gender/penampilan/nama ini asumsi desain yang dapat direvisi. Semua memakai body manusia berproporsi wajar dan Humanoid rig; outfit medieval, hood/cape ringan. Core gerak/melee/projectile sama agar scope terkendali. Perbedaan awal visual, voice bank dan efek skill; animasi harus cocok pada seluruh avatar.

Kontrol rencana: WASD bergerak, Shift sprint, Space lompat, mouse kamera, LMB melee, RMB block/aim, Q skill, E interaksi, M map, Esc pause, F3 AI debug. Kontrol project lama tidak diwarisi diam-diam karena ini project baru.

Animasi minimal: idle, walk, run, jump start/air/land, melee windup/hit/recovery, cast, hit reaction, death. Damage/cast/footstep dipicu animation event terverifikasi, bukan suara tiap frame. Satu aksi hanya boleh memicu satu hit yang dimaksud.

## 6. Enemy dan Guardian

Minimum demo: dua melee Ravager, dua ranged Arcanist dan satu Guardian. Masing-masing memiliki rig, collider, animasi dan feedback damage.
- Perception: radius, sudut pandang, raycast penghalang, stimulus suara langkah/serangan. Enemy tidak tahu posisi player menembus dinding.
- Memory: last seen/heard position beserta timestamp; setelah kehilangan target lakukan investigate → search terbatas → kembali patrol.
- Navigation: NavMeshAgent, rute patrol, validasi path, dan sampling tujuan aman. Jalur spawn/portal tidak terhalang dekorasi.
- Decision: FSM dengan Patrol, Suspicious, Investigate, Chase, Attack, Reposition, Retreat, Return, Dead. Dead terminal; status tak relevan dilewati.
- Melee: mendekat, telegraph lalu serang dengan cooldown; beri ruang recovery.
- Ranged: jarak nyaman sekitar 7–12 m, cari line of sight; mundur di bawah 5 m dan saat HP rendah; memilih posisi yang dapat dicapai.
- Guardian humanoid besar: fase 1 melee + ranged rune ketika player jauh, fase 2 di HP ≤ 50% menambah shockwave. Telegraph bisa dibaca dan dihindari. Arena leash mengembalikan boss dan mereset encounter jika player kabur lewat batas; map teleport diblokir selama encounter.

Musik/teks/player reaction menggunakan enemy yang benar-benar aware, bukan hanya jarak geometris.
F3 menampilkan state, HP, target, last-known position dan umur memory, vision cone/ray, path, tactical destination, serta event hearing terbaru. Debug dapat dimatikan dari settings/build.

## 7. Musik adaptif

Tiga state yang diminta pengguna harus terdengar berbeda:
- Explore: tidak ada ancaman aktif; ambience angin dan musik santai.
- Alert: enemy curiga, menginvestigasi atau mendeteksi; motif tegang sebelum kontak.
- Combat: serangan dimulai, damage masuk/keluar, atau enemy engaged dan memiliki memory ancaman yang masih valid.
Boss adalah varian Combat dengan prioritas musik tertinggi saat encounter Guardian aktif.
Death/Victory memberi stinger; UI/loading memiliki ambience sendiri.

Explore → Alert memakai deteksi enemy; Alert → Combat memakai engage/serangan. Combat bertahan minimal 4 detik dan baru turun setelah 6 detik tanpa enemy engaged. Alert turun setelah 8 detik tanpa stimulus. Nilai dapat diubah di Inspector; hysteresis mencegah musik bolak-balik.

AudioDirector menghitung state agregat semua enemy hidup. Satu enemy mati tidak mengakhiri musik jika enemy lain masih bertarung. Pause tidak membuat threat timer berakhir. Transisi memakai dua AudioSource dan crossfade 1–2 detik; state tetap benar walau clip hilang, tetapi hilangnya clip dicatat sebagai masalah konten.

Pengaturan AudioMixer: Master, Music, SFX, Ambience, Voice. Dialog/voice menurunkan musik sementara. Headphone/spatial audio untuk enemy dan lingkungan; musik/UI bersifat 2D. Loading progress berasal dari operasi load sebenarnya, dengan tahap terpisah untuk aktivasi scene; bukan angka acak.

## 8. Efek suara

Cue minimum:
- footstep per kaki dan permukaan tanah/batu/kayu; volume/rate sprint berbeda;
- takeoff, landing normal, heavy landing;
- melee swing, shield block, blade hit flesh/stone, hurt, death;
- skill windup, cast, projectile flight/impact;
- waypoint activate, teleport charge, departure, arrival;
- pickup artifact/sigil, pintu terbuka/terkunci, objective complete;
- UI hover, select, back, error, confirm.

SFX memakai variation kecil pada pitch/gain dan pool AudioSource untuk mencegah repetisi serta lonjakan alokasi. Footstep hanya ketika grounded dan bergerak; tidak berbunyi saat player diam/di udara. Teleport memainkan departure di asal dan arrival di tujuan, dengan transisi scene tetap menyelesaikan cue secara benar. Atur max voices dan audio priority agar cue penting tidak hilang.

Suara enemy mengikuti animasi serang/hurt/death; bunyi langkah tidak bergantung pada frame rate. Audio source jarak jauh dicull dan diprioritaskan.

## 9. Suara bicara per karakter

Setiap pilihan karakter memiliki bank suara sendiri: idle, discovery, alert, combat attack/skill, low health, victory, death, dan interaksi NPC. Tidak menyalin identitas suara manusia tertentu. VO final perlu rekaman sendiri, voice asset berlisensi, atau suara sintetik yang disepakati; teks belum merupakan file audio.

Idle bark dipicu setelah tidak bergerak/bertarung selama acak 25–45 detik, lalu cooldown minimal 45 detik. Setelah serangan bark memiliki peluang kecil dan cooldown ≥ 8 detik. Hindari baris sama berturut-turut.
Prioritas: death > critical warning > dialog > combat bark > idle. Satu voice line player aktif pada satu waktu; tidak tumpang tindih dengan dialog penting.
Subtitles aktif default, menyebut nama pembicara; toggle voice/idle chatter terpisah. Ganti karakter harus mengganti VoiceProfile dengan benar, termasuk setelah Continue.

Contoh naskah orisinal (belum VO):
Kael idle: "Angin membawa debu dari reruntuhan itu."
Lyra idle: "Jejaknya masih baru. Aku tidak sendirian."
Orin idle: "Kalau runenya menyala, biasanya ada masalah."
NPC camp: "Dua sigil akan membuka Sanctum. Aktifkan batu penunjuk sebelum masuk."
Guardian: "Relik itu tidak akan meninggalkan tempat ini."

## 10. Angin, rambut, pakaian, pepohonan dan rumput

WindDirector menghasilkan arah, strength dan gust berdasarkan waktu yang sama. Di luar dungeon gunakan angin lembut dengan hembusan sesekali; di dungeon kurangi kekuatan dan pakai arus lokal dekat pintu. VFX partikel debu/daun mengikuti arah angin.

- Rumput: shader vertex sway di pangkal-terkunci, amplitude mengikuti tinggi, variasi phase per instance; instancing/LOD untuk efisiensi.
- Pohon: trunk sway kecil dan branch/leaf flutter lebih besar; material/mesh harus mendukungnya. WindZone sendiri tidak menggerakkan semua mesh/material.
- Rambut: secondary bones/spring bones pada beberapa untaian; capai collider kepala/bahu agar tidak tembus wajah.
- Kain: cape/ujung rok/jubah memakai Cloth dengan pin di bahu/pinggang dan collider tubuh, atau rantai tulang untuk outfit yang tidak mendukung cloth. CPU simulation dibatasi pada player/karakter dekat.
- Teleport/respawn: reset simulasi secondary motion agar tidak melontarkan kain/rambut akibat perubahan posisi.
- Death: hilangkan gaya berlebihan dan pertahankan pose/ragdoll yang aman bila ada.

Tidak semua model gratis memiliki bone rambut/kain atau weight yang sesuai. Pemilihan outfit harus menandai kebutuhan authoring; rig Humanoid saja tidak menjamin rambut/baju bergerak. Wind quality setting Off/Low/High; particle/grass masih memberi atmosfer pada Low. Visual harus dicek pada idle, run, jump, combat dan teleport.

## 11. Loading, menu, UI dan feedback

Main Menu: New Game, Continue, Settings, Credits, Quit. Character Select menampilkan model, nama, preview animasi dan contoh voice. Loading memakai art/silhouette orisinal, nama destination, bar progress dan tips; opening game, masuk/keluar dungeon dan teleport cross-scene semuanya memakai transisi yang konsisten.

HUD: HP, stamina/skill cooldown, crosshair ringan, prompt E, objective, compass dan boss bar saat relevan. Hit flash, hit sound, camera response ringan dan indikator damage; screen shake dapat dimatikan. Warna altar/portal cyan, sigil emas, serangan berbahaya amber/merah dengan bentuk/animasi yang jelas.

Settings: difficulty, master/music/SFX/ambience/voice volume, sensitivity, FOV, invert Y, subtitles, idle chatter, objective marker, quality/wind, screen shake dan debug. Nilai disimpan. Difficulty Easy mengubah damage/reaction window; musuh tetap melakukan AI yang dapat didemokan.

## 12. Intake aset dan arah visual

Utama: model manusia rigged + animasi Mixamo, PBR terrain/dinding Poly Haven, suara Kenney serta musik fantasy berlisensi. Mixamo butuh Adobe login; jangan menyatakan model telah diunduh jika belum.
Quaternius adalah opsi humanoid stylized; perlu preview untuk menentukan apakah mendekati harapan manusia pengguna. Kit dungeon low-poly RGS hanya cadangan layout, bukan tolok ukur realisme.
Portal, sigil, artifact, map dan UI dapat dibuat orisinal dari geometri/material sederhana; kualitasnya harus dinilai pada render Unity.

Aset hasil intake ada di AssetStaging dengan sumber, lisensi, ukuran dan checksum. Belum ada import/rig/material/audio acceptance di Unity. Folder staging tidak berada di Assets agar ZIP, contoh engine lain dan file berlebih tidak diimpor otomatis.

## 13. Verifikasi requirement

| Requirement | Bukti penerimaan yang harus diambil |
|---|---|
| Playable level/arena | Run baru dari menu sampai kemenangan, semua gerbang dapat dimasuki |
| Player controller | Gerak/lompat/camera/serang/skill tidak bocor saat map/pause/loading |
| ≥ 3 NPC/enemy | Minimal melee, ranged dan Guardian hidup, bergerak dan berinteraksi |
| Objective/win/lose | Kunci sigil, pickup, boss, extraction, death/retry terbukti |
| Perception/memory | Dinding menghalangi sight, suara diselidiki, memory kadaluarsa |
| Navigation | Enemy melewati rute berhalang dan tidak berhenti di obstacle |
| Decision/tactical | Transisi FSM, ranged menjaga jarak dan retreat benar |
| Asset/environment | Humanoid rigged, material URP sesuai, gerbang/terrain terbaca |
| UI/audio/visual | Tiga state musik, SFX tiap event, subtitles, damage/teleport feedback |
| AI debug | F3 menampilkan sensor/state/path/memory yang sesuai keadaan |
| Console | Compile, import, run dan build tanpa error merah; simpan log nyata |

Kasus regresi penting: double-click teleport, tujuan belum ditemukan, teleport saat combat/death, keluar dungeon kembali ke gerbang tepat, Continue karakter/VO benar, dua enemy dengan state berbeda, boss tetap mengunci teleport, source audio tidak bertambah tanpa batas, pause tidak melewatkan cooldown, reset kain setelah teleport.

Rehearsal manual wajib untuk kualitas musik/crossfade, rasa kontrol, animasi/rig, keterbacaan peta serta angin. Ukur frame time pada perangkat target; FPS belum diukur. Belum ada klaim requirement lolos.

## 14. Urutan pengerjaan setelah review desain

1. Tetapkan versi Unity, buat project URP baru, fondasi data/input/scene transition dan smoke test.
2. Overworld pendek, tiga portal, empat waypoint, map dan save/checkpoint; verifikasi transisi.
3. Player dan karakter rigged, locomotion/combat/skill dan variasi voice profile.
4. AI perception/memory/navigation, melee/ranged tactics dan Guardian.
5. Objective lengkap, menu/settings/loading, audio adaptif/SFX/VO/subtitles.
6. Wind/secondary motion, lighting/environment, profiling, playthrough dan standalone build.

Catatan skill: forward engineer belum ditemukan pada katalog serta pencarian SKILL.md lokal. Alur desain memakai brainstorming. Tidak ada pemasangan skill atau tool baru yang dilakukan.

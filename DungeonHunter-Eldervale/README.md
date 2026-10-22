# Dungeon Hunter: Relic of Veyr

Target: game Unity third-person single-player PC/Mac untuk tugas Intelligent Enemy Dungeon.

Project Unity baru terpisah dari UnityRecovery/DungeonHunter. Prototype gameplay sudah diimplementasikan. Editor: **Unity 7000.0.0a7**, URP 17.7.0. Pemeriksaan terakhir yang tercatat adalah **59 PASS / 0 FAIL** pada 21 Oktober 2026 pukul 17:41 (`docs/evidence/polish-59-green.txt`). Verifikasi build macOS standalone dan review visual final masih perlu dikonfirmasi; lihat ledger untuk batasan dan status terakhir. File build lama bukan bukti build dari perubahan terbaru.

## Mainkan di Unity

1. Buka folder project ini melalui Unity Hub / Add project from disk.
2. Buka `Assets/Eldervale/Scenes/Frontend.unity` dan tekan Play, atau gunakan menu **Eldervale → Play preview**.
3. Mulai ekspedisi, pilih pemburu, lalu masuki Eldervale.

Environment dibangun oleh komponen WorldBuilder saat Play. Karena itu, Game View di luar Play dapat bertuliskan “No cameras rendering”; itu bukan kegagalan render pipeline. Saat Play, Hierarchy berisi geometry yang dapat diinspeksi. URP harus tetap terpasang di Project Settings / Graphics.

## Kontrol

| Tombol | Fungsi |
|---|---|
| WASD / Shift | Bergerak / berlari |
| Panah / mouse | Arah kamera |
| Enter | Lompat |
| Enter dua kali saat airborne ≥3 meter | Melayang, bukan terbang dari tanah |
| Klik kiri / Q | Serang / skill |
| E / H | Interaksi / minum pemulih |
| Tab | Ganti senjata |
| 1, 2, 3 / P | Ganti anggota aktif / party setup |
| M / Escape | Peta / jeda |
| F3 | AI debug |

## Alur ekspedisi

Aktifkan menara → jelajahi Ashen Crypt dan Whispering Grotto → temukan dua sigil → buka Veyr Sanctum → hadapi dialog dan Guardian → ambil Relic of Veyr → kembalikan ke altar camp. Grand Tower memulihkan party; reward kenaikan level diklaim di Warden camp. Loot musuh otomatis mendekat saat berada dalam jangkauan.

Satu overworld sekitar 200 × 200 meter mencakup kota kecil, istana, pantai kecil, perbukitan, empat menara dan tiga gerbang. Tiga interior adalah scene terpisah. Ini prototype perkuliahan berskala kecil, bukan game open world produksi AAA.

## Audio / visual

Musik Explore–Alert–Combat–Boss, SFX posisi 3D, voice/subtitle, UI terpusat, reverb dungeon, wall filtering, angin pada pines/grass/hair/capes. Pengaturan memiliki uji kiri/depan/kanan/belakang. Gunakan headphone. Belum memakai plugin HRTF atau Dolby; sample sumber tidak otomatis menjadi hi-res hanya karena output 48 kHz. VO masih temporary macOS TTS.

Latar judul berupa scene pegunungan/air terjun 3D, bukan image backdrop. Ilustrasi pada docs/art-direction hanya acuan visual. Font Inter dibundel dengan lisensi SIL OFL.

## Verifikasi dan build

- Menu **Eldervale → Run full Play checks** menjalankan simulasi alur dengan backup/restore save.
- Menu **Eldervale → Build macOS player** menghasilkan `Builds/Mac/DungeonHunter-Eldervale.app` bila berhasil.
- Bukti aktual: `docs/evidence/play-checks.txt`, `rules-checks.txt`, `build-result.txt` dan capture PNG. File berlabel `red` adalah kegagalan uji sebelum perbaikan, bukan hasil terakhir.
- Uji otomatis tidak menggantikan rehearsal kontrol, penilaian kualitas animasi, atau audition headphone.

- [Desain game dan acceptance criteria](docs/2026-10-21-eldervale-design.md)
- [Peta rancangan](docs/eldervale-map.svg)
- [Sumber aset dan catatan import Unity](docs/ASSET-SOURCES.md)
- Asset sumber yang berhasil diambil tersimpan di AssetStaging/, dengan hasil intake dan SHA-256 pada AssetStaging/intake-manifest.json setelah intake selesai.

- [Status implementasi dan keputusan](docs/IMPLEMENTATION-STATUS.md)
- [Arah visual dan penggunaan gambar konsep](docs/art-direction/README.md)

## Kontribusi dengan coding agent

Panduan project untuk agent dan kontributor ada di [AGENTS.md](AGENTS.md). Skill
project portable tersimpan di
[`.agents/skills/eldervale-unity/SKILL.md`](.agents/skills/eldervale-unity/SKILL.md) dan mengarahkan agent ke desain,
batasan aset, cara menjalankan pemeriksaan, serta verifikasi build. Baca panduan
itu sebelum mengubah project.

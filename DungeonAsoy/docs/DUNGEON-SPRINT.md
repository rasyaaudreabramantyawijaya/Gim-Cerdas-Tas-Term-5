# Sprint Dungeon Hunter

Nama proyek: Dungeon Hunter - Intelligent Enemy Dungeon.

Tujuan: player menjelajahi dungeon dari kamera third-person, mencari artifact, bertarung, membuka gerbang dan menghadapi Guardian. NPC manusia di entrance dapat diajak bicara untuk memperoleh petunjuk.

Pembagian tanggal adalah asumsi kalender: minggu 1 pada 5-11 Oktober 2026; minggu 2 pada 12-18 Oktober 2026.

| Minggu | Tahap | Hasil untuk demo | Status awal |
|---|---|---|---|
| 1 | 1. Fondasi | Terrain berbukit, mansion berinterior, karakter, kamera, movement dan collision | Dibuat dan diperiksa dalam Play Mode |
| 1 | 2. Playable slice | NPC, dua checkpoint, pemanah, panah, artifact, gate, HP, HUD dan exit | Dibuat; alur komponen diperiksa dalam Play Mode |
| 2 | 3. Enemy AI | Navigasi, variasi melee/flee serta tuning pemanah | Backlog; ranged/maintain-distance dimajukan sesuai revisi |
| 2 | 4. Guardian | Guardian phases, alur penuh, feedback, polish, capture dan demo akhir | Backlog |

## Syarat Tahap 1 Selesai

- Scene tersimpan, masuk Build Settings dan Play Mode diperiksa.
- Player dapat bergerak, kamera mengikuti, tangan/kaki bergerak saat jalan.
- Player tidak menembus wall.
- Karakter tampil sebagai model prototype humanoid dengan arah gaya anime dari referensi.

## Syarat Tahap 2 Selesai

- NPC gatekeeper bisa diajak bicara dari jarak dekat dan respons berubah setelah pickup.
- Artifact hanya dapat dikoleksi sekali dan diperlukan untuk membuka gate.
- Pemanah meluncurkan projectile yang memberi damage pada player; HP/death dan checkpoint bekerja.
- Attack/interaksi terhalang wall dan NPC friendly aman.
- Run kecil dapat diselesaikan sampai exit lalu diulang.

## Bukti Progress

Status implementasi dan verifikasi dicatat pada `DUNGEON-PROGRESS.md`; hasil pemeriksaan dan capture runtime tersedia pada `docs/evidence/`. Rehearsal run penuh dengan kontrol manual masih perlu dilakukan sebelum presentasi.

Minggu pertama mempresentasikan Open Estate exploration/combat slice. Guardian, navigasi lengkap dan polishing masih untuk minggu kedua.

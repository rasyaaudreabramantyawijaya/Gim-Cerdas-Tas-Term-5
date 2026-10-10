# Praktikum 3: Autonomous Delivery Bot Training Yard

## Tujuan

Membuat NPC robot pengantar yang bergerak mandiri dengan steering behavior. NPC
harus dapat mendekati Player secara halus, melambat sebelum berhenti, bergerak
acak ketika target dimatikan, dan menghindari obstacle secara lokal.

Keberhasilan demo ditunjukkan oleh alur berikut:

1. Player bergerak dengan keyboard.
2. Dengan `Use Target` aktif, NPC memakai Arrive untuk mendekati Player.
3. NPC melambat dalam `Slow Radius` dan berhenti dalam `Stop Radius`.
4. Obstacle memicu SphereCast dan perubahan arah yang terlihat.
5. Dengan `Use Target` nonaktif, NPC memakai Wander sambil tetap menghindari obstacle.
6. Tiga eksperimen parameter dapat dilakukan melalui Inspector.

## Arena

Scene baru bernama `Praktikum03_AutonomousSteering` memakai arena latihan terbuka,
bukan maze rapat. Isinya:

- ground datar berukuran cukup luas untuk melihat lintasan NPC;
- Player di satu sisi arena;
- NPC robot kuning di bagian tengah;
- beberapa crate atau pilar pada layer `Obstacle`;
- satu konfigurasi obstacle berbentuk U untuk menunjukkan batas local avoidance.

Arena mempertahankan layer `Obstacle`, material dasar, dan gaya visual NPC dari
Praktikum 2. NavMesh dan NavMeshAgent tidak digunakan untuk movement NPC.

## Komponen

### SimplePlayerController

- Memindahkan Player dengan WASD dan tombol panah.
- Menggunakan `CharacterController` agar Player tidak menembus obstacle.

### SteeringSensor

- Menggunakan SphereCast dari posisi depan NPC.
- Mengekspos `Sensor Distance`, `Sensor Radius`, dan `Obstacle Mask`.
- Mengembalikan arah avoidance berdasarkan normal obstacle atau arah samping yang aman.
- Menggambar SphereCast dan titik benturan melalui Gizmos.

### SteeringAgent

- Menyimpan `velocity`, `desiredVelocity`, dan acceleration.
- Mengekspos seluruh parameter wajib: `Max Speed`, `Max Acceleration`, `Turn Speed`,
  `Use Target`, `Slow Radius`, `Stop Radius`, `Wander Speed`,
  `Wander Change Interval`, `Wander Angle Change`, dan `Avoidance Weight`.
- `Use Target = true`: menghitung Arrive. Kecepatan target proporsional terhadap
  jarak dalam Slow Radius dan nol pada Stop Radius.
- `Use Target = false`: menghitung Wander dengan perubahan arah berkala.
- Menambahkan avoidance ke desired velocity sebelum membatasi acceleration dan velocity.
- Memindahkan transform dari velocity dan merotasi NPC menuju arah gerak.
- Mengubah warna NPC: hijau untuk Arrive, biru untuk Wander, dan oranye/merah
  ketika avoidance aktif.
- Menggambar Stop Radius, Slow Radius, velocity, desired velocity, arah target,
  dan status avoidance melalui Gizmos.

## Urutan Steering

```text
Arrive atau Wander
        -> Desired Velocity
        -> + Obstacle Avoidance
        -> Desired Acceleration
        -> Velocity
        -> Movement
        -> Rotation
```

Acceleration dibatasi oleh `Max Acceleration`; velocity dibatasi oleh `Max Speed`.
Dengan demikian perpindahan arah dan perubahan kecepatan tidak mendadak.

## Eksperimen Wajib

| Parameter | Nilai | Observasi yang dicatat |
| --- | --- | --- |
| Max Speed | 2, 4, 8 | Kecepatan tertinggi dan respons movement. |
| Slow Radius | 2, 5, 10 | Jarak saat NPC mulai melambat. |
| Avoidance Weight | 0.5, 2, 5 | Kekuatan perubahan arah ketika sensor mendeteksi obstacle. |

Eksperimen dilakukan dengan posisi Player dan obstacle yang sama agar perbandingan
adil. Pengamatan direkam dengan screenshot atau video singkat dari Game View.

## Batasan yang Akan Dijelaskan Saat Demo

Obstacle avoidance adalah local movement. NPC membaca obstacle di depan lewat
SphereCast dan memilih arah baru, tetapi tidak menghitung rute global. Karena itu
NPC dapat buntu pada obstacle berbentuk U atau konfigurasi sempit; solusi untuk
masalah tersebut adalah pathfinding/navigation, bukan menaikkan Avoidance Weight
secara terus-menerus.

## Verifikasi

Sebelum dinyatakan selesai, scene harus diuji di Play Mode untuk membuktikan:

- Arrive melambat dan berhenti pada radius yang benar;
- Wander berlangsung saat target nonaktif;
- SphereCast mencegah benturan pada mode Arrive dan Wander;
- NPC menghadap arah velocity;
- perubahan warna dan Gizmos muncul;
- tiga eksperimen bisa diatur lewat Inspector.

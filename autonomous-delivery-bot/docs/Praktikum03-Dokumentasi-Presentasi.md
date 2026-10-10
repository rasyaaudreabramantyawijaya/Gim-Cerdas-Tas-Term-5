# Praktikum 3 - Autonomous Steering Agent

## Alur Sistem

```text
Arrive/Wander -> base velocity -> obstacle avoidance -> desired velocity
-> acceleration -> velocity -> CharacterController.Move -> rotation/animation
```

NPC menggunakan local steering. Sistem tidak menghitung rute global seperti NavMesh.

## File dan Bagian Kode yang Dijelaskan

### `Assets/Scripts/SteeringAgent.cs`

Ini adalah otak NPC.

**Pemilihan behavior:**

```csharp
Vector3 baseVelocity = useTarget && target != null
    ? CalculateArriveVelocity(transform.position, target.position,
        maxSpeed, slowRadius, stopRadius)
    : CalculateWanderVelocity();
```

`Use Target` aktif berarti Arrive. Jika nonaktif berarti Wander.

**Arrive:**

```csharp
if (distance <= stopRadiusValue) return Vector3.zero;
float speed = distance >= safeSlowRadius
    ? maxSpeedValue
    : maxSpeedValue * distance / safeSlowRadius;
return offset.normalized * speed;
```

Saat jauh dari target, NPC memakai `Max Speed`. Saat masuk `Slow Radius`, kecepatan turun bertahap. Saat masuk `Stop Radius`, desired velocity menjadi nol.

**Wander:**

```csharp
if (Time.time >= nextWanderChangeTime)
{
    Vector3 baseDirection = velocity.sqrMagnitude > 0.01f
        ? velocity.normalized : transform.forward;
    wanderHeading = Quaternion.Euler(
        0f, Random.Range(-wanderAngleChange, wanderAngleChange), 0f)
        * baseDirection;
    nextWanderChangeTime = Time.time + wanderChangeInterval;
}
return wanderHeading.normalized * wanderSpeed;
```

Arah tidak diacak setiap frame, tetapi hanya ketika interval tercapai agar gerak tidak bergetar.

**Acceleration, movement, dan rotation:**

```csharp
Vector3 acceleration = CalculateAcceleration(
    velocity, desiredVelocity, maxAcceleration);
velocity += acceleration * Time.deltaTime;
velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
characterController.Move(velocity * Time.deltaTime);
```

`desiredVelocity` tidak langsung menjadi velocity. Perubahannya dibatasi `Max Acceleration`, kemudian NPC digerakkan dengan `CharacterController`. Rotasi mengikuti arah velocity melalui `Quaternion.LookRotation` dan `Quaternion.RotateTowards`.

**Visual state:**

NPC berwarna hijau saat Arrive, biru saat Wander, dan oranye saat menghindari obstacle. Fungsi `AnimateWalk()` menggerakkan tangan dan kaki berdasarkan nilai velocity.

### `Assets/Scripts/SteeringSensor.cs`

Script ini mendeteksi obstacle di depan NPC.

```csharp
Physics.SphereCast(lastOrigin, sensorRadius, transform.forward,
    out lastHit, sensorDistance, obstacleMask,
    QueryTriggerInteraction.Ignore)
```

SphereCast dipilih karena memiliki radius. Raycast hanya berupa garis, sedangkan SphereCast lebih sesuai dengan ukuran tubuh NPC. Jika obstacle ditemukan, normal permukaan dan arah lateral digabungkan:

```csharp
avoidanceDirection = (normal + lateral).normalized;
```

Gizmos menggambar sensor kuning saat aman dan merah saat obstacle terdeteksi.

### `Assets/Scripts/SimplePlayerController.cs`

Script ini membaca `WASD` atau tombol panah, menggerakkan Player dengan `CharacterController.Move`, memutar Player ke arah gerak, dan menjalankan animasi tangan-kaki.

```csharp
Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
controller.Move(direction * moveSpeed * Time.deltaTime);
```

### `Assets/Editor/AutonomousSteeringSceneBuilder.cs`

Builder hanya menyiapkan scene: arena, dinding, obstacle dengan layer `Obstacle`, Player, DeliveryBot, material, kamera, `CharacterController`, `SteeringAgent`, dan `SteeringSensor`. Logika AI utama tetap berada di dua script runtime di atas.

## Urutan Demonstrasi

1. Jalankan `Praktikum03_AutonomousSteering`.
2. Gerakkan Player dengan `WASD`.
3. Tunjukkan NPC mengikuti Player.
4. Dekatkan Player perlahan untuk menunjukkan `Slow Radius` dan `Stop Radius`.
5. Matikan `Use Target` pada NPC untuk menunjukkan Wander.
6. Arahkan NPC ke obstacle dan tunjukkan perubahan warna oranye serta SphereCast Gizmos.
7. Ubah `Max Speed`, `Slow Radius`, dan `Avoidance Weight` dari Inspector.

## Eksperimen

| Parameter | Nilai | Pengamatan |
|---|---:|---|
| Max Speed | 2, 4, 8 | Kecepatan NPC dan waktu mencapai target |
| Slow Radius | 2, 5, 10 | Jarak NPC mulai melambat |
| Avoidance Weight | 0.5, 2, 5 | Kekuatan NPC membelok menghindari obstacle |

## Naskah Presentasi

Selamat pagi/siang. Pada praktikum ini saya membuat Autonomous Steering Agent menggunakan Unity dan C#. Fokus implementasinya adalah Arrive, Wander, dan Obstacle Avoidance.

Alur sistem dimulai dari pemilihan behavior. Jika `Use Target` aktif, NPC menghitung Arrive berdasarkan posisi Player. Jika nonaktif, NPC menggunakan Wander dan mengubah arah secara berkala.

Pada Arrive, saya menghitung jarak NPC ke target. Saat jaraknya masih lebih besar dari `Slow Radius`, NPC bergerak dengan `Max Speed`. Saat masuk `Slow Radius`, kecepatannya dikurangi secara proporsional. Saat masuk `Stop Radius`, desired velocity menjadi nol sehingga NPC berhenti. Ini membedakan Arrive dari Seek: Seek hanya mengejar target dan tidak memiliki fase perlambatan.

Setelah base velocity diperoleh, `SteeringSensor` menjalankan SphereCast ke arah depan NPC. Saya memilih SphereCast karena sensor memiliki radius dan lebih sesuai dengan ukuran tubuh NPC dibanding Raycast yang hanya berupa garis. Jika obstacle terdeteksi, sistem menghasilkan arah avoidance lalu menggabungkannya dengan desired velocity menggunakan `Avoidance Weight`.

Desired velocity kemudian dibandingkan dengan velocity saat ini. Selisihnya dibatasi oleh `Max Acceleration`, lalu velocity diperbarui. NPC digerakkan menggunakan `CharacterController.Move` dan diputar mengikuti arah velocity. Jadi urutannya adalah desired velocity, acceleration, velocity, movement, lalu rotation.

`SimplePlayerController` menangani input dan pergerakan Player. `SteeringAgent` menangani keputusan dan gerakan NPC. `SteeringSensor` menangani deteksi obstacle. `AutonomousSteeringSceneBuilder` digunakan untuk membangun arena dan memasang komponen secara otomatis.

Untuk debugging, saya menggunakan Gizmos. Garis dan lingkaran menunjukkan sensor, slow radius, stop radius, velocity, dan arah target. Warna NPC menunjukkan status: hijau untuk Arrive, biru untuk Wander, dan oranye saat menghindari obstacle.

Keterbatasan sistem ini adalah obstacle avoidance yang digunakan bersifat local movement. NPC hanya bereaksi terhadap obstacle yang terdeteksi di depannya dan belum menghitung rute global. Pada sudut sempit atau obstacle berbentuk U, NPC masih mungkin kesulitan menemukan jalan keluar. Penyelesaian global membutuhkan pathfinding atau NavMesh.

Kesimpulannya, praktikum ini menunjukkan bagaimana keputusan behavior AI diterjemahkan menjadi gerakan melalui rangkaian desired velocity, acceleration, velocity, movement, dan rotation.

## Jawaban Cepat Saat Defense

- **Seek vs Arrive:** Seek mengejar dengan kecepatan penuh; Arrive memperlambat dan berhenti pada jarak tertentu.
- **Slow Radius:** Jarak mulai perlambatan.
- **Stop Radius:** Jarak minimum NPC dari target ketika berhenti.
- **Max Speed vs Max Acceleration:** Max Speed membatasi kecepatan; Max Acceleration membatasi perubahan kecepatan.
- **Raycast vs SphereCast:** Raycast berupa garis; SphereCast memiliki volume/radius.
- **Avoidance Weight:** Bobot pengaruh arah menghindar terhadap desired velocity.
- **Obstacle Avoidance vs Pathfinding:** Obstacle Avoidance bereaksi lokal; Pathfinding mencari rute global.

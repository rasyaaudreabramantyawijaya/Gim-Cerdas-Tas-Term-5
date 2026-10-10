using UnityEngine;

// Tempel di GameObject enemy yang bentuknya Cube.
// Klik ikon titik-3 di komponen ini di Inspector > "Build Cube Cluster".
// Ini akan MENYEMBUNYIKAN mesh cube utama (BoxCollider tetap aktif)
// dan menggantinya secara visual dengan kumpulan kubus kecil berwarna neon
// yang berputar mengelilingi titik pusat, lengkap dengan efek listrik/spark.
public class CubeClusterG : MonoBehaviour
{
    [Header("Bentuk Kluster")]
    [SerializeField] private int cubeCount = 8;
    [SerializeField] private float clusterRadius = 0.55f;
    [SerializeField] private float cubeSize = 0.4f;

    [Header("Warna")]
    [SerializeField] private Color colorA = new Color(0.2f, 0.6f, 1f);    // electric blue
    [SerializeField] private Color colorB = new Color(0.7f, 0.2f, 1f);    // neon purple
    [SerializeField] private Color coreColor = new Color(1f, 0.9f, 0.3f); // kuning energi
    [SerializeField] private float emissionIntensity = 2f;

    [Header("Animasi Putar")]
    [SerializeField] private float clusterSpinSpeed = 20f;
    [SerializeField] private float cubeSelfSpinSpeed = 60f;

    [Header("Efek Listrik")]
    [SerializeField] private int sparkCount = 30;
    [SerializeField] private float sparkLifetime = 0.4f;
    [SerializeField] private float sparkSpeed = 3f;
    [SerializeField] private Color sparkColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Transform clusterRoot;

    [ContextMenu("Build Cube Cluster")]
    public void BuildCluster()
    {
        // Hapus CapsuleCollider kalau masih ada (berjaga-jaga dari setup lama)
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null) DestroyImmediate(capsule);

        // Pastikan BoxCollider ada (karena enemy sekarang cube)
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) gameObject.AddComponent<BoxCollider>();

        // Sembunyikan mesh cube utama (collider BoxCollider tetap aktif)
        MeshRenderer cubeRenderer = GetComponent<MeshRenderer>();
        if (cubeRenderer != null) cubeRenderer.enabled = false;

        Transform old = transform.Find("CubeCluster");
        if (old != null) DestroyImmediate(old.gameObject);

        GameObject clusterObj = new GameObject("CubeCluster");
        clusterObj.transform.SetParent(transform, false);
        clusterObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        clusterRoot = clusterObj.transform;

        // --- Spawn kubus-kubus kluster dengan warna gradient ---
        for (int i = 0; i < cubeCount; i++)
        {
            float y = 1f - (i / (float)(cubeCount - 1)) * 2f;
            float radiusAtY = Mathf.Sqrt(1f - y * y);
            float theta = i * 2.399963f; // golden angle
            Vector3 dir = new Vector3(Mathf.Cos(theta) * radiusAtY, y, Mathf.Sin(theta) * radiusAtY);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"CoreCube_{i}";
            cube.transform.SetParent(clusterRoot, false);
            cube.transform.localPosition = dir * clusterRadius;
            cube.transform.localScale = Vector3.one * cubeSize;
            cube.transform.localRotation = Random.rotation;
            DestroyImmediate(cube.GetComponent<Collider>());

            // Warna gradient dari colorA ke colorB + emission glow
            float t = i / (float)(cubeCount - 1);
            Color cubeColor = Color.Lerp(colorA, colorB, t);
            ApplyEmissiveMaterial(cube, cubeColor);

            // Tambah efek hover (naik-turun pelan) + spin
            CubeFloat floater = cube.AddComponent<CubeFloat>();
            floater.floatAmplitude = Random.Range(0.03f, 0.08f);
            floater.floatSpeed = Random.Range(1.5f, 3f);
            floater.phaseOffset = Random.Range(0f, Mathf.PI * 2f);

            cube.AddComponent<SelfSpin>().speed = cubeSelfSpinSpeed * (Random.value > 0.5f ? 1f : -1f);
        }

        // --- Core glow di tengah (berdenyut) ---
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "CoreGlow";
        core.transform.SetParent(clusterRoot, false);
        core.transform.localScale = Vector3.one * (cubeSize * 0.6f);
        DestroyImmediate(core.GetComponent<Collider>());
        ApplyEmissiveMaterial(core, coreColor);
        CorePulse pulse = core.AddComponent<CorePulse>();
        pulse.baseScale = cubeSize * 0.6f;
        pulse.pulseColor = coreColor;
        pulse.emissionIntensity = emissionIntensity;

        // --- Efek partikel listrik / spark ---
        BuildSparkEffect(clusterObj);

        // Spin keseluruhan kluster
        clusterObj.AddComponent<SelfSpin>().speed = clusterSpinSpeed;
    }

    private void ApplyEmissiveMaterial(GameObject obj, Color color)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * emissionIntensity);
        rend.material = mat;
    }

    private void BuildSparkEffect(GameObject parent)
    {
        GameObject sparkObj = new GameObject("ElectricSparks");
        sparkObj.transform.SetParent(parent.transform, false);

        ParticleSystem ps = sparkObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = sparkLifetime;
        main.startSpeed = sparkSpeed;
        main.startSize = 0.06f;
        main.maxParticles = sparkCount * 3;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startColor = sparkColor;
        main.loop = true;
        main.playOnAwake = true;
        main.gravityModifier = 0f;

        // Emisi
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = sparkCount;

        // Bentuk: sphere kecil sesuai radius kluster
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = clusterRadius * 0.8f;

        // Ukuran mengecil seiring waktu (menghilang)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Warna: fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(sparkColor, 0f),
                new GradientColorKey(Color.white, 0.3f),
                new GradientColorKey(sparkColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        // Noise biar gerakannya zig-zag kayak listrik
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1.5f;
        noise.frequency = 5f;
        noise.scrollSpeed = 3f;
        noise.damping = true;
        noise.octaveCount = 2;

        // Trail mini biar ada garis-garis listrik
        var trails = ps.trails;
        trails.enabled = true;
        trails.lifetime = 0.15f;
        trails.dieWithParticles = true;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));

        // Material partikel (additive glow)
        ParticleSystemRenderer psRenderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        Material sparkMat = new Material(Shader.Find("Particles/Standard Unlit"));
        sparkMat.color = sparkColor;
        sparkMat.renderQueue = 3000;
        psRenderer.material = sparkMat;
        psRenderer.trailMaterial = sparkMat;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}

// ─── Komponen pendukung ───

// Muter objek pelan-pelan
public class SelfSpin : MonoBehaviour
{
    public float speed = 30f;
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
    }
}

// Efek naik-turun (hover/float) untuk tiap kubus
public class CubeFloat : MonoBehaviour
{
    public float floatAmplitude = 0.05f;
    public float floatSpeed = 2f;
    public float phaseOffset = 0f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin((Time.time * floatSpeed) + phaseOffset) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0f, offset, 0f);
    }
}

// Efek denyut (pulse) untuk core glow di tengah
public class CorePulse : MonoBehaviour
{
    public float baseScale = 0.24f;
    public float pulseAmount = 0.3f;
    public float pulseSpeed = 3f;
    public Color pulseColor = Color.yellow;
    public float emissionIntensity = 2f;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // Scale naik-turun
        float scale = baseScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
        transform.localScale = Vector3.one * scale;

        // Emission intensity naik-turun
        if (rend != null)
        {
            float glow = emissionIntensity * (1f + Mathf.Sin(Time.time * pulseSpeed * 1.5f) * 0.5f);
            rend.material.SetColor("_EmissionColor", pulseColor * glow);
        }
    }
}

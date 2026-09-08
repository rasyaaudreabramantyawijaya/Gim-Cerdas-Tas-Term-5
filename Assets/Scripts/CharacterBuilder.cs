using UnityEngine;

// Tempel di GameObject Player (Capsule).
// Klik ikon titik-3 di komponen ini di Inspector > "Build Humanoid Character".
// Menghasilkan karakter humanoid low-poly procedural bergaya anime/RPG.
// Animasi jalan dihandle oleh PlayerController (bukan script ini).
public class HumanoidBuilder : MonoBehaviour
{
    [Header("Skala & Proporsi")]
    [SerializeField] private float bodyScale = 1f;

    [Header("Warna Utama")]
    [SerializeField] private Color skinColor = new Color(0.93f, 0.80f, 0.70f);
    [SerializeField] private Color hairColor = new Color(0.65f, 0.68f, 0.72f);
    [SerializeField] private Color outfitPrimary = new Color(0.10f, 0.30f, 0.35f);
    [SerializeField] private Color outfitSecondary = new Color(0.05f, 0.12f, 0.15f);
    [SerializeField] private Color accentGold = new Color(0.85f, 0.75f, 0.30f);
    [SerializeField] private Color capeColor = new Color(0.06f, 0.14f, 0.18f);
    [SerializeField] private Color bootColor = new Color(0.08f, 0.08f, 0.10f);
    [SerializeField] private Color weaponColor = new Color(0.75f, 0.75f, 0.78f);
    [SerializeField] private float emissionIntensity = 0.8f;

    [Header("Cape")]
    [SerializeField] private bool showCape = true;
    [SerializeField] private float capeLength = 1.2f;

    [Header("Senjata")]
    [SerializeField] private bool showWeapon = true;
    [SerializeField] private float weaponLength = 2.5f;

    [ContextMenu("Build Humanoid Character")]
    public void BuildCharacter()
    {
        ClearOldParts();

        // Sembunyikan mesh capsule utama
        MeshRenderer mainRend = GetComponent<MeshRenderer>();
        if (mainRend != null) mainRend.enabled = false;

        float s = bodyScale;

        // Root container untuk seluruh body
        GameObject bodyRoot = new GameObject("BodyRoot");
        bodyRoot.transform.SetParent(transform, false);
        bodyRoot.transform.localPosition = Vector3.zero;
        Transform root = bodyRoot.transform;

        // ══════════════════════════════════════════
        //  KEPALA & RAMBUT
        // ══════════════════════════════════════════
        MakePart("Head", PrimitiveType.Sphere,
            new Vector3(0f, 1.55f, 0f) * s,
            new Vector3(0.32f, 0.36f, 0.32f) * s, skinColor, root);

        MakePart("HairBack", PrimitiveType.Cube,
            new Vector3(0f, 1.65f, -0.08f) * s,
            new Vector3(0.38f, 0.30f, 0.30f) * s, hairColor, root);

        MakePart("HairFront", PrimitiveType.Cube,
            new Vector3(0f, 1.72f, 0.10f) * s,
            new Vector3(0.34f, 0.12f, 0.12f) * s, hairColor, root);

        float[] spikeAngles = { -40f, -15f, 10f, 35f, 55f, -55f };
        for (int i = 0; i < spikeAngles.Length; i++)
        {
            GameObject spike = MakePart($"HairSpike_{i}", PrimitiveType.Cube,
                new Vector3(
                    Mathf.Sin(spikeAngles[i] * Mathf.Deg2Rad) * 0.18f,
                    1.80f + Random.Range(0f, 0.06f),
                    -0.05f + Mathf.Cos(spikeAngles[i] * Mathf.Deg2Rad) * 0.05f
                ) * s,
                new Vector3(0.06f, 0.18f + Random.Range(0f, 0.08f), 0.06f) * s,
                hairColor, root);
            spike.transform.localRotation = Quaternion.Euler(
                Random.Range(-15f, 15f), 0f, spikeAngles[i] * 0.5f);
        }

        MakePart("EyeL", PrimitiveType.Sphere,
            new Vector3(-0.08f, 1.56f, 0.14f) * s,
            new Vector3(0.05f, 0.04f, 0.03f) * s, outfitPrimary, root);
        MakePart("EyeR", PrimitiveType.Sphere,
            new Vector3(0.08f, 1.56f, 0.14f) * s,
            new Vector3(0.05f, 0.04f, 0.03f) * s, outfitPrimary, root);

        MakePart("Neck", PrimitiveType.Cube,
            new Vector3(0f, 1.38f, 0f) * s,
            new Vector3(0.10f, 0.10f, 0.10f) * s, skinColor, root);

        // ══════════════════════════════════════════
        //  TORSO
        // ══════════════════════════════════════════
        MakePart("ChestMain", PrimitiveType.Cube,
            new Vector3(0f, 1.12f, 0f) * s,
            new Vector3(0.42f, 0.45f, 0.22f) * s, outfitPrimary, root);

        MakePart("JacketFront", PrimitiveType.Cube,
            new Vector3(0f, 1.14f, 0.02f) * s,
            new Vector3(0.38f, 0.40f, 0.18f) * s, outfitSecondary, root);

        MakePart("CollarL", PrimitiveType.Cube,
            new Vector3(-0.12f, 1.32f, 0.06f) * s,
            new Vector3(0.12f, 0.08f, 0.08f) * s, outfitPrimary, root);
        MakePart("CollarR", PrimitiveType.Cube,
            new Vector3(0.12f, 1.32f, 0.06f) * s,
            new Vector3(0.12f, 0.08f, 0.08f) * s, outfitPrimary, root);

        MakeGlowPart("ChestTrimV", PrimitiveType.Cube,
            new Vector3(0f, 1.15f, 0.12f) * s,
            new Vector3(0.03f, 0.30f, 0.02f) * s, accentGold, root);
        MakeGlowPart("ChestTrimH", PrimitiveType.Cube,
            new Vector3(0f, 1.22f, 0.12f) * s,
            new Vector3(0.20f, 0.02f, 0.02f) * s, accentGold, root);
        MakeGlowPart("ChestEmblem", PrimitiveType.Sphere,
            new Vector3(0f, 1.18f, 0.13f) * s,
            new Vector3(0.05f, 0.05f, 0.02f) * s, new Color(0.3f, 1f, 0.5f), root);

        MakePart("ShoulderPadL", PrimitiveType.Cube,
            new Vector3(-0.28f, 1.28f, 0f) * s,
            new Vector3(0.14f, 0.08f, 0.16f) * s, outfitPrimary, root);
        MakePart("ShoulderPadR", PrimitiveType.Cube,
            new Vector3(0.28f, 1.28f, 0f) * s,
            new Vector3(0.14f, 0.08f, 0.16f) * s, outfitPrimary, root);

        // Pinggang
        MakePart("Waist", PrimitiveType.Cube,
            new Vector3(0f, 0.88f, 0f) * s,
            new Vector3(0.36f, 0.10f, 0.20f) * s, outfitSecondary, root);
        MakeGlowPart("Belt", PrimitiveType.Cube,
            new Vector3(0f, 0.88f, 0.02f) * s,
            new Vector3(0.38f, 0.04f, 0.18f) * s, accentGold, root);
        MakeGlowPart("BeltBuckle", PrimitiveType.Sphere,
            new Vector3(0f, 0.88f, 0.11f) * s,
            new Vector3(0.06f, 0.06f, 0.03f) * s, new Color(0.3f, 0.9f, 0.6f), root);

        MakePart("TassetFrontL", PrimitiveType.Cube,
            new Vector3(-0.10f, 0.74f, 0.06f) * s,
            new Vector3(0.14f, 0.22f, 0.03f) * s, outfitPrimary, root);
        MakePart("TassetFrontR", PrimitiveType.Cube,
            new Vector3(0.10f, 0.74f, 0.06f) * s,
            new Vector3(0.14f, 0.22f, 0.03f) * s, outfitPrimary, root);

        // ══════════════════════════════════════════
        //  LENGAN KIRI (pivot di bahu)
        // ══════════════════════════════════════════
        GameObject armPivotL = new GameObject("ArmPivotL");
        armPivotL.transform.SetParent(root, false);
        armPivotL.transform.localPosition = new Vector3(-0.28f, 1.22f, 0f) * s;

        MakePart("UpperArmL", PrimitiveType.Cube,
            new Vector3(0f, -0.14f, 0f) * s,
            new Vector3(0.12f, 0.28f, 0.12f) * s, outfitSecondary, armPivotL.transform);
        MakePart("ForearmL", PrimitiveType.Cube,
            new Vector3(0f, -0.38f, 0f) * s,
            new Vector3(0.10f, 0.22f, 0.10f) * s, outfitPrimary, armPivotL.transform);
        MakeGlowPart("ArmTrimL", PrimitiveType.Cube,
            new Vector3(0f, -0.28f, 0.05f) * s,
            new Vector3(0.11f, 0.02f, 0.02f) * s, accentGold, armPivotL.transform);
        MakePart("HandL", PrimitiveType.Cube,
            new Vector3(0f, -0.52f, 0f) * s,
            new Vector3(0.08f, 0.08f, 0.06f) * s, skinColor, armPivotL.transform);

        // ══════════════════════════════════════════
        //  LENGAN KANAN (pivot di bahu)
        // ══════════════════════════════════════════
        GameObject armPivotR = new GameObject("ArmPivotR");
        armPivotR.transform.SetParent(root, false);
        armPivotR.transform.localPosition = new Vector3(0.28f, 1.22f, 0f) * s;

        MakePart("UpperArmR", PrimitiveType.Cube,
            new Vector3(0f, -0.14f, 0f) * s,
            new Vector3(0.12f, 0.28f, 0.12f) * s, outfitSecondary, armPivotR.transform);
        MakePart("ForearmR", PrimitiveType.Cube,
            new Vector3(0f, -0.38f, 0f) * s,
            new Vector3(0.10f, 0.22f, 0.10f) * s, outfitPrimary, armPivotR.transform);
        MakeGlowPart("ArmTrimR", PrimitiveType.Cube,
            new Vector3(0f, -0.28f, 0.05f) * s,
            new Vector3(0.11f, 0.02f, 0.02f) * s, accentGold, armPivotR.transform);
        MakePart("HandR", PrimitiveType.Cube,
            new Vector3(0f, -0.52f, 0f) * s,
            new Vector3(0.08f, 0.08f, 0.06f) * s, skinColor, armPivotR.transform);

        // ══════════════════════════════════════════
        //  KAKI KIRI (pivot di pinggul)
        // ══════════════════════════════════════════
        GameObject legPivotL = new GameObject("LegPivotL");
        legPivotL.transform.SetParent(root, false);
        legPivotL.transform.localPosition = new Vector3(-0.10f, 0.82f, 0f) * s;

        MakePart("ThighL", PrimitiveType.Cube,
            new Vector3(0f, -0.20f, 0f) * s,
            new Vector3(0.14f, 0.35f, 0.14f) * s, outfitSecondary, legPivotL.transform);
        MakePart("ShinL", PrimitiveType.Cube,
            new Vector3(0f, -0.50f, 0f) * s,
            new Vector3(0.13f, 0.30f, 0.13f) * s, bootColor, legPivotL.transform);
        MakeGlowPart("BootTrimL", PrimitiveType.Cube,
            new Vector3(0f, -0.37f, 0.02f) * s,
            new Vector3(0.14f, 0.03f, 0.02f) * s, accentGold, legPivotL.transform);
        MakeGlowPart("BootGlowL", PrimitiveType.Sphere,
            new Vector3(0f, -0.44f, 0.06f) * s,
            new Vector3(0.04f, 0.04f, 0.02f) * s, new Color(0.3f, 1f, 0.5f), legPivotL.transform);
        MakePart("FootL", PrimitiveType.Cube,
            new Vector3(0f, -0.68f, 0.03f) * s,
            new Vector3(0.12f, 0.10f, 0.18f) * s, bootColor, legPivotL.transform);

        // ══════════════════════════════════════════
        //  KAKI KANAN (pivot di pinggul)
        // ══════════════════════════════════════════
        GameObject legPivotR = new GameObject("LegPivotR");
        legPivotR.transform.SetParent(root, false);
        legPivotR.transform.localPosition = new Vector3(0.10f, 0.82f, 0f) * s;

        MakePart("ThighR", PrimitiveType.Cube,
            new Vector3(0f, -0.20f, 0f) * s,
            new Vector3(0.14f, 0.35f, 0.14f) * s, outfitSecondary, legPivotR.transform);
        MakePart("ShinR", PrimitiveType.Cube,
            new Vector3(0f, -0.50f, 0f) * s,
            new Vector3(0.13f, 0.30f, 0.13f) * s, bootColor, legPivotR.transform);
        MakeGlowPart("BootTrimR", PrimitiveType.Cube,
            new Vector3(0f, -0.37f, 0.02f) * s,
            new Vector3(0.14f, 0.03f, 0.02f) * s, accentGold, legPivotR.transform);
        MakeGlowPart("BootGlowR", PrimitiveType.Sphere,
            new Vector3(0f, -0.44f, 0.06f) * s,
            new Vector3(0.04f, 0.04f, 0.02f) * s, new Color(0.3f, 1f, 0.5f), legPivotR.transform);
        MakePart("FootR", PrimitiveType.Cube,
            new Vector3(0f, -0.68f, 0.03f) * s,
            new Vector3(0.12f, 0.10f, 0.18f) * s, bootColor, legPivotR.transform);

        // ══════════════════════════════════════════
        //  CAPE
        // ══════════════════════════════════════════
        if (showCape)
        {
            GameObject cape = MakePart("CapeMain", PrimitiveType.Cube,
                new Vector3(0f, 1.05f - capeLength * 0.3f, -0.16f) * s,
                new Vector3(0.40f, capeLength, 0.03f) * s, capeColor, root);
            cape.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

            MakePart("CapeCollar", PrimitiveType.Cube,
                new Vector3(0f, 1.30f, -0.14f) * s,
                new Vector3(0.44f, 0.10f, 0.06f) * s, capeColor, root);

            MakeGlowPart("CapeTrimBottom", PrimitiveType.Cube,
                new Vector3(0f, 1.05f - capeLength * 0.3f - capeLength * 0.48f, -0.16f) * s,
                new Vector3(0.42f, 0.02f, 0.04f) * s, accentGold, root);

            cape.AddComponent<CapeWave>();
        }

        // ══════════════════════════════════════════
        //  SENJATA (Polearm — di tangan kanan, ikut swing)
        // ══════════════════════════════════════════
        if (showWeapon)
        {
            GameObject shaft = MakePart("WeaponShaft", PrimitiveType.Cube,
                new Vector3(0f, 0.20f, -0.05f) * s,
                new Vector3(0.03f, weaponLength, 0.03f) * s, weaponColor, armPivotR.transform);

            MakePart("WeaponBlade", PrimitiveType.Cube,
                new Vector3(0f, weaponLength * 0.48f, 0f),
                new Vector3(0.08f, 0.30f, 0.02f), weaponColor, shaft.transform);

            MakeGlowPart("WeaponGuard", PrimitiveType.Cube,
                new Vector3(0f, weaponLength * 0.38f, 0f),
                new Vector3(0.14f, 0.04f, 0.06f), accentGold, shaft.transform);

            MakeGlowPart("WeaponOrnL", PrimitiveType.Sphere,
                new Vector3(-0.08f, weaponLength * 0.38f, 0f),
                new Vector3(0.04f, 0.04f, 0.04f), accentGold, shaft.transform);
            MakeGlowPart("WeaponOrnR", PrimitiveType.Sphere,
                new Vector3(0.08f, weaponLength * 0.38f, 0f),
                new Vector3(0.04f, 0.04f, 0.04f), accentGold, shaft.transform);

            MakePart("WeaponPommel", PrimitiveType.Sphere,
                new Vector3(0f, -weaponLength * 0.48f, 0f),
                new Vector3(0.05f, 0.05f, 0.05f), accentGold, shaft.transform);
        }
    }

    private GameObject MakePart(string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;
        DestroyImmediate(obj.GetComponent<Collider>());

        Renderer rend = obj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.material = mat;
        return obj;
    }

    private GameObject MakeGlowPart(string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color, Transform parent)
    {
        GameObject obj = MakePart(name, type, localPos, localScale, color, parent);
        Renderer rend = obj.GetComponent<Renderer>();
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", color * emissionIntensity);
        return obj;
    }

    [ContextMenu("Clear Character Parts")]
    public void ClearOldParts()
    {
        Transform br = transform.Find("BodyRoot");
        if (br != null) DestroyImmediate(br.gameObject);

        // Legacy cleanup
        foreach (string n in new[] { "Head", "ArmLeft", "ArmRight", "AuraEffect", "CubeCluster" })
        {
            Transform t = transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }

        MeshRenderer mainRend = GetComponent<MeshRenderer>();
        if (mainRend != null) mainRend.enabled = true;
    }
}

// Cape sway
public class CapeWave : MonoBehaviour
{
    [SerializeField] private float swayAmount = 3f;
    [SerializeField] private float swaySpeed = 2f;
    private Quaternion baseRot;

    void Start() { baseRot = transform.localRotation; }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.localRotation = baseRot * Quaternion.Euler(angle, 0f, angle * 0.3f);
    }
}

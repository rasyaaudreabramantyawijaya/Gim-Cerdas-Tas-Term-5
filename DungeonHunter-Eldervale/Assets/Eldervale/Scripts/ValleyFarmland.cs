using UnityEngine;

public static class ValleyFarmland
{
    public static void Build(Transform parent, Material wood, Material stone, Material path, Material grass)
    {
        var root = new GameObject("Valley Farmland and Sawah").transform;
        root.SetParent(parent);

        var cropSoil = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        cropSoil.color = new Color(0.28f, 0.22f, 0.14f);
        cropSoil.SetFloat("_Smoothness", 0.45f);

        var wheatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wheatMat.color = new Color(0.82f, 0.72f, 0.28f);
        wheatMat.SetFloat("_Smoothness", 0.1f);

        var greenCrop = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        greenCrop.color = new Color(0.25f, 0.55f, 0.2f);
        greenCrop.SetFloat("_Smoothness", 0.15f);

        // --- 1. Terraced Sawah / Farmland Plots ---
        Vector3 farmBase = new Vector3(-18, 0.03f, 38);
        for (int plot = 0; plot < 3; plot++)
        {
            Vector3 ppos = farmBase + new Vector3(plot * 9f, plot * 0.25f, 0);

            // Soil Bed
            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "Sawah crop bed " + plot;
            bed.transform.SetParent(root);
            bed.transform.position = ppos + Vector3.up * 0.1f;
            bed.transform.localScale = new Vector3(7.5f, 0.22f, 14f);
            bed.GetComponent<Renderer>().sharedMaterial = cropSoil;

            // Wooden perimeter fence
            for (int z = -6; z <= 6; z += 4)
            {
                for (int x = -1; x <= 1; x += 2)
                {
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    post.transform.SetParent(root);
                    post.transform.position = ppos + new Vector3(x * 4.1f, 0.6f, z);
                    post.transform.localScale = new Vector3(0.18f, 1.1f, 0.18f);
                    post.GetComponent<Renderer>().sharedMaterial = wood;
                    Object.Destroy(post.GetComponent<Collider>());
                }
            }

            // Crop rows (Wheat and Cabbages)
            for (int r = -2; r <= 2; r++)
            {
                float xOffset = r * 1.3f;
                if (plot == 0 || plot == 2)
                {
                    // Golden wheat rows
                    for (int w = -5; w <= 5; w++)
                    {
                        var stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        stalk.name = "Wheat stalk";
                        stalk.transform.SetParent(root);
                        stalk.transform.position = ppos + new Vector3(xOffset, 0.65f, w * 1.1f);
                        stalk.transform.localScale = new Vector3(0.12f, 0.45f, 0.12f);
                        stalk.GetComponent<Renderer>().sharedMaterial = wheatMat;
                        Object.Destroy(stalk.GetComponent<Collider>());
                    }
                }
                else
                {
                    // Green cabbage heads
                    for (int c = -5; c <= 5; c++)
                    {
                        var cab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cab.name = "Cabbage";
                        cab.transform.SetParent(root);
                        cab.transform.position = ppos + new Vector3(xOffset, 0.35f, c * 1.2f);
                        cab.transform.localScale = new Vector3(0.45f, 0.35f, 0.45f);
                        cab.GetComponent<Renderer>().sharedMaterial = greenCrop;
                        Object.Destroy(cab.GetComponent<Collider>());
                    }
                }
            }
        }

        // --- 2. Traditional Tall Windmill (Kincir Angin Lembah) ---
        Vector3 millPos = new Vector3(-24, 0, 60);
        var millTower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        millTower.name = "Windmill Stone Tower";
        millTower.transform.SetParent(root);
        millTower.transform.position = millPos + Vector3.up * 6f;
        millTower.transform.localScale = new Vector3(6f, 6f, 6f);
        millTower.GetComponent<Renderer>().sharedMaterial = stone;

        var millCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        millCap.name = "Windmill Wooden Cap";
        millCap.transform.SetParent(root);
        millCap.transform.position = millPos + Vector3.up * 13f;
        millCap.transform.localScale = new Vector3(4.8f, 1.2f, 4.8f);
        millCap.GetComponent<Renderer>().sharedMaterial = wood;

        // Rotating Windmill Sails
        var sailsHub = new GameObject("Windmill Sails");
        sailsHub.transform.SetParent(root);
        sailsHub.transform.position = millPos + new Vector3(0, 13f, -2.6f);
        sailsHub.AddComponent<WindmillSailRotator>();

        for (int i = 0; i < 4; i++)
        {
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Sail Arm " + i;
            arm.transform.SetParent(sailsHub.transform, false);
            arm.transform.localRotation = Quaternion.Euler(0, 0, i * 90f);
            arm.transform.localPosition = arm.transform.localRotation * new Vector3(0, 3.8f, 0);
            arm.transform.localScale = new Vector3(0.2f, 7.5f, 0.15f);
            arm.GetComponent<Renderer>().sharedMaterial = wood;
            Object.Destroy(arm.GetComponent<Collider>());

            var cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "Sail Cloth";
            cloth.transform.SetParent(arm.transform, false);
            cloth.transform.localPosition = new Vector3(0.55f, 0, 0.05f);
            cloth.transform.localScale = new Vector3(5f, 0.95f, 0.1f);
            cloth.GetComponent<Renderer>().sharedMaterial = wheatMat;
            Object.Destroy(cloth.GetComponent<Collider>());
        }

        // --- 3. Haystacks & Farmer's Cart ---
        for (int h = 0; h < 4; h++)
        {
            var hay = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hay.name = "Haystack " + h;
            hay.transform.SetParent(root);
            hay.transform.position = farmBase + new Vector3(Random.Range(-5f, 22f), 1f, Random.Range(-10f, -4f));
            hay.transform.localScale = new Vector3(2.2f, 1f, 2.2f);
            hay.GetComponent<Renderer>().sharedMaterial = wheatMat;
        }

        // --- 4. Archery Practice Range (Lapangan Panahan Hiburan) ---
        Vector3 rangePos = new Vector3(14, 0, 38);
        var rangeGroup = new GameObject("Archery Range").transform;
        rangeGroup.SetParent(root);
        rangeGroup.position = rangePos;

        // Shooting Stand Line
        var shootStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shootStand.transform.SetParent(rangeGroup, false);
        shootStand.transform.localPosition = new Vector3(0, 0.1f, -8f);
        shootStand.transform.localScale = new Vector3(7f, 0.2f, 1.4f);
        shootStand.GetComponent<Renderer>().sharedMaterial = wood;

        // 3 Bullseye Targets
        for (int t = -1; t <= 1; t++)
        {
            Vector3 tpos = new Vector3(t * 2.4f, 1.5f, 6f);
            var targetDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            targetDisc.name = "Archery Target";
            targetDisc.transform.SetParent(rangeGroup, false);
            targetDisc.transform.localPosition = tpos;
            targetDisc.transform.localRotation = Quaternion.Euler(90, 0, 0);
            targetDisc.transform.localScale = new Vector3(1.4f, 0.15f, 1.4f);
            targetDisc.GetComponent<Renderer>().sharedMaterial = wheatMat;

            // Red center bullseye
            var bullseye = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bullseye.transform.SetParent(targetDisc.transform, false);
            bullseye.transform.localPosition = new Vector3(0, 1.1f, 0);
            bullseye.transform.localScale = new Vector3(0.45f, 0.1f, 0.45f);
            var redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            redMat.color = new Color(0.85f, 0.18f, 0.15f);
            bullseye.GetComponent<Renderer>().sharedMaterial = redMat;
            Object.Destroy(bullseye.GetComponent<Collider>());

            // Wooden tripod legs
            for (int leg = -1; leg <= 1; leg += 2)
            {
                var legObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                legObj.transform.SetParent(rangeGroup, false);
                legObj.transform.localPosition = new Vector3(t * 2.4f + leg * 0.4f, 0.75f, 6f);
                legObj.transform.localRotation = Quaternion.Euler(0, 0, leg * 15f);
                legObj.transform.localScale = new Vector3(0.12f, 1.7f, 0.12f);
                legObj.GetComponent<Renderer>().sharedMaterial = wood;
                Object.Destroy(legObj.GetComponent<Collider>());
            }
        }
    }
}

public sealed class WindmillSailRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.forward * (22f * Time.deltaTime), Space.Self);
    }
}

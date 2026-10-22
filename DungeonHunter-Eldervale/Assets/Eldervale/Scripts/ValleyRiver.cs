using System.Collections.Generic;
using UnityEngine;

public static class ValleyRiver
{
    public static void Build(Transform parent, Material stone, Material wood, Material rock)
    {
        var root = new GameObject("Valley River and Waterfall").transform;
        root.SetParent(parent);

        var waterFlow = ScenicBackdrop.WaterMaterial(false);
        var waterFall = ScenicBackdrop.WaterMaterial(true);

        // --- 1. Mountain Waterfall (Air Terjun di Bukit) ---
        Vector3 fallPos = new Vector3(-48, 16, 26);
        // Cliff rocks around waterfall
        for (int i = 0; i < 7; i++)
        {
            float ox = Random.Range(-6f, 6f), oz = Random.Range(-3f, 4f), oy = Random.Range(-4f, 2f);
            ScenicBackdrop.Crag(root, fallPos + new Vector3(ox, oy, oz), new Vector3(7, 12, 6), rock);
        }
        // Falling water sheet
        var fallSheet = new GameObject("Waterfall Cascade");
        fallSheet.transform.SetParent(root);
        fallSheet.transform.position = new Vector3(-48, 8.5f, 25);
        fallSheet.transform.rotation = Quaternion.Euler(68, 160, 0);
        var fmesh = new Mesh();
        fmesh.vertices = new[] { new Vector3(-3.5f, -8, 0), new Vector3(3.5f, -8, 0), new Vector3(3.5f, 8, 0), new Vector3(-3.5f, 8, 0) };
        fmesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        fmesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
        fmesh.RecalculateNormals();
        fallSheet.AddComponent<MeshFilter>().sharedMesh = fmesh;
        fallSheet.AddComponent<MeshRenderer>().sharedMaterial = waterFall;
        RuntimeResources.Own(fallSheet, fmesh);

        // --- 2. Mountain Waterfall Pool (Kolam Sumber Air) ---
        Vector3 poolPos = new Vector3(-46, 1.1f, 20);
        ScenicBackdrop.WaterPlane(root, "Mountain Waterfall Pool", poolPos, new Vector2(16, 14), waterFlow);
        WaterVolume.Register(root.gameObject, new Bounds(poolPos, new Vector3(17, 3, 15)), 1.15f);

        // --- 3. Flowing River Winding to Town (Sungai Mengalir) ---
        Vector3[] waypoints = new[] {
            new Vector3(-45, 0.9f, 18),
            new Vector3(-36, 0.55f, 6),
            new Vector3(-28, 0.35f, -8),
            new Vector3(-18, 0.22f, -20),
            new Vector3(-9, 0.18f, -32)
        };

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 a = waypoints[i];
            Vector3 b = waypoints[i + 1];
            Vector3 mid = (a + b) * 0.5f;
            float length = Vector3.Distance(a, b);
            float angle = Mathf.Atan2(b.x - a.x, b.z - a.z) * Mathf.Rad2Deg;

            var rseg = new GameObject("River segment " + i);
            rseg.transform.SetParent(root);
            rseg.transform.position = mid;
            rseg.transform.rotation = Quaternion.Euler(0, angle, 0);

            var rmesh = new Mesh();
            float halfW = 4.2f;
            float halfL = length * 0.55f;
            rmesh.vertices = new[] {
                new Vector3(-halfW, 0, -halfL),
                new Vector3(halfW, 0, -halfL),
                new Vector3(halfW, 0, halfL),
                new Vector3(-halfW, 0, halfL)
            };
            rmesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            rmesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
            rmesh.RecalculateNormals();
            rseg.AddComponent<MeshFilter>().sharedMesh = rmesh;
            rseg.AddComponent<MeshRenderer>().sharedMaterial = waterFlow;
            RuntimeResources.Own(rseg, rmesh);

            // Water volume registration for splashing
            WaterVolume.Register(rseg, new Bounds(mid, new Vector3(halfW * 2f, 2.5f, length + 2f)), mid.y + 0.05f);

            // Riverbanks - smooth river stones along edges
            for (int s = -1; s <= 1; s += 2)
            {
                for (int st = -2; st <= 2; st++)
                {
                    Vector3 rockPos = mid + Quaternion.Euler(0, angle, 0) * new Vector3(s * 4.6f, -0.05f, st * (length * 0.22f));
                    var stoneObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    stoneObj.name = "River stone";
                    stoneObj.transform.SetParent(root);
                    stoneObj.transform.position = rockPos;
                    stoneObj.transform.localScale = new Vector3(Random.Range(0.8f, 1.4f), Random.Range(0.4f, 0.7f), Random.Range(0.8f, 1.5f));
                    stoneObj.GetComponent<Renderer>().sharedMaterial = stone;
                    Object.Destroy(stoneObj.GetComponent<Collider>());
                }
            }
        }

        // Townside Lake / Basin (tempat muara sungai)
        Vector3 basinPos = new Vector3(-8, 0.16f, -34);
        ScenicBackdrop.WaterPlane(root, "Riverside Lake Basin", basinPos, new Vector2(14, 12), waterFlow);
        WaterVolume.Register(root.gameObject, new Bounds(basinPos, new Vector3(15, 2.5f, 13)), 0.2f);

        // --- 4. Arched Stone Bridge over River (Jembatan Batu Desa) ---
        Vector3 bridgePos = new Vector3(-28, 0.45f, -8);
        var bridge = new GameObject("Eldervale Stone Bridge");
        bridge.transform.SetParent(root);
        bridge.transform.position = bridgePos;

        // Bridge road deck
        var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deck.name = "Bridge Deck";
        deck.transform.SetParent(bridge.transform, false);
        deck.transform.localPosition = new Vector3(0, 0.35f, 0);
        deck.transform.localScale = new Vector3(9f, 0.45f, 4.5f);
        deck.GetComponent<Renderer>().sharedMaterial = stone;

        // Stone railings
        for (int side = -1; side <= 1; side += 2)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Bridge Parapet";
            rail.transform.SetParent(bridge.transform, false);
            rail.transform.localPosition = new Vector3(0, 0.85f, side * 2.35f);
            rail.transform.localScale = new Vector3(9.2f, 0.85f, 0.35f);
            rail.GetComponent<Renderer>().sharedMaterial = stone;

            // Bridge Lanterns on stone pillars
            for (int end = -1; end <= 1; end += 2)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.transform.SetParent(bridge.transform, false);
                pillar.transform.localPosition = new Vector3(end * 4.4f, 1.1f, side * 2.35f);
                pillar.transform.localScale = new Vector3(0.55f, 1.3f, 0.55f);
                pillar.GetComponent<Renderer>().sharedMaterial = stone;

                var blantern = new GameObject("Bridge Lantern").AddComponent<Light>();
                blantern.transform.SetParent(bridge.transform, false);
                blantern.transform.localPosition = new Vector3(end * 4.4f, 1.9f, side * 2.35f);
                blantern.type = LightType.Point;
                blantern.color = new Color(1f, 0.85f, 0.55f);
                blantern.range = 7f;
                blantern.intensity = 1.4f;
            }
        }

        // --- 5. Rustic Watermill (Kincir Air Desa) ---
        Vector3 millPos = new Vector3(-32, 0.5f, 4);
        var millHouse = GameObject.CreatePrimitive(PrimitiveType.Cube);
        millHouse.name = "Watermill House";
        millHouse.transform.SetParent(root);
        millHouse.transform.position = millPos + new Vector3(0, 2f, 0);
        millHouse.transform.localScale = new Vector3(5, 4, 6);
        millHouse.GetComponent<Renderer>().sharedMaterial = wood;

        // Rotating Water Wheel
        var wheelObj = new GameObject("Watermill Wheel");
        wheelObj.transform.SetParent(root);
        wheelObj.transform.position = millPos + new Vector3(3.2f, 1.2f, 0);
        wheelObj.AddComponent<WaterWheelRotator>();
        for (int p = 0; p < 8; p++)
        {
            float a = p / 8f * Mathf.PI * 2f;
            var paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paddle.transform.SetParent(wheelObj.transform, false);
            paddle.transform.localPosition = new Vector3(0, Mathf.Sin(a) * 1.8f, Mathf.Cos(a) * 1.8f);
            paddle.transform.localRotation = Quaternion.Euler(p * 45f, 0, 0);
            paddle.transform.localScale = new Vector3(1.2f, 0.1f, 0.8f);
            paddle.GetComponent<Renderer>().sharedMaterial = wood;
            Object.Destroy(paddle.GetComponent<Collider>());
        }
        var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hub.transform.SetParent(wheelObj.transform, false);
        hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
        hub.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
        hub.GetComponent<Renderer>().sharedMaterial = stone;
        Object.Destroy(hub.GetComponent<Collider>());
    }
}

public sealed class WaterWheelRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.right * (28f * Time.deltaTime), Space.Self);
    }
}

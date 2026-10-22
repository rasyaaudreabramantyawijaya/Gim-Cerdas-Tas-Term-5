using System.Collections.Generic;
using UnityEngine;

public static class ValleyRiver
{
    const float RiverHalfWidth = 4.2f;
    const float ChannelDepth = 0.85f;
    const float RiverBankWidth = 3.2f;
    static readonly Vector3[] RiverWaypoints = {
        new Vector3(-45, 0.22f, 18),
        new Vector3(-36, 0.19f, 6),
        new Vector3(-28, 0.15f, -8),
        new Vector3(-18, 0.12f, -20),
        new Vector3(-38, 0.1f, -34)
    };

    public static bool IsNearWater(Vector3 point,float margin=0f)
    {
        if(Mathf.Abs(point.x+46f)<=8f+margin&&Mathf.Abs(point.z-20f)<=7f+margin)return true;
        if(Mathf.Abs(point.x+38f)<=7f+margin&&Mathf.Abs(point.z+34f)<=6f+margin)return true;
        var p=new Vector2(point.x,point.z);
        for(int i=0;i<RiverWaypoints.Length-1;i++)
        {
            var a=new Vector2(RiverWaypoints[i].x,RiverWaypoints[i].z);
            var b=new Vector2(RiverWaypoints[i+1].x,RiverWaypoints[i+1].z);
            var segment=b-a;
            float t=Mathf.Clamp01(Vector2.Dot(p-a,segment)/segment.sqrMagnitude);
            if(Vector2.Distance(p,a+segment*t)<=RiverHalfWidth+margin)return true;
        }
        return false;
    }

    // Terrain uses this offset to carve a real bed below each water surface, with
    // a gradual bank rather than laying a blue sheet over flat ground.
    public static float GroundOffsetAt(float x, float z)
    {
        var p = new Vector2(x, z);
        float distance = DistanceToRiver(p);
        float bank = 1f - Mathf.SmoothStep(RiverHalfWidth, RiverHalfWidth + RiverBankWidth, distance);
        float depth = -ChannelDepth * bank;
        depth = Mathf.Min(depth, EllipseBed(p, new Vector2(-46f, 20f), new Vector2(8f, 7f), .68f));
        depth = Mathf.Min(depth, NaturalEllipseBed(p, new Vector2(-38f, -34f), new Vector2(7f, 6f), .62f));
        return depth;
    }

    static float DistanceToRiver(Vector2 p)
    {
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < RiverWaypoints.Length - 1; i++)
        {
            var a = new Vector2(RiverWaypoints[i].x, RiverWaypoints[i].z);
            var b = new Vector2(RiverWaypoints[i + 1].x, RiverWaypoints[i + 1].z);
            var segment = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, segment) / segment.sqrMagnitude);
            nearest = Mathf.Min(nearest, Vector2.Distance(p, a + segment * t));
        }
        return nearest;
    }

    static float EllipseBed(Vector2 p, Vector2 center, Vector2 halfSize, float maxDepth)
    {
        var delta = p - center;
        float radius = Mathf.Sqrt(delta.x * delta.x / (halfSize.x * halfSize.x) + delta.y * delta.y / (halfSize.y * halfSize.y));
        return -maxDepth * (1f - Mathf.SmoothStep(.72f, 1f, radius));
    }

    static float NaturalEllipseBed(Vector2 p, Vector2 center, Vector2 halfSize, float maxDepth)
    {
        var delta = p - center;
        float x = delta.x / halfSize.x, z = delta.y / halfSize.y;
        float angle = Mathf.Atan2(z, x);
        float radius = Mathf.Sqrt(x * x + z * z) / ScenicBackdrop.NaturalBasinRadius(angle);
        return -maxDepth * (1f - Mathf.SmoothStep(.72f, 1f, radius));
    }

    static float TerrainWorldHeight(Vector3 worldPosition)
    {
        var terrain = Terrain.activeTerrain;
        return terrain != null ? terrain.transform.position.y + terrain.SampleHeight(worldPosition) : 0f;
    }

    public static void Build(Transform parent, Material stone, Material wood, Material rock)
    {
        var root = new GameObject("Valley River and Waterfall").transform;
        root.SetParent(parent);

        var waterFlow = ScenicBackdrop.WaterMaterial(false);
        var waterFall = ScenicBackdrop.WaterMaterial(true);

        // --- 1. Mountain Waterfall (Air Terjun di Bukit) ---
        Vector3 fallPos = new Vector3(-48, 4, 26);
        // Cliff rocks around waterfall
        for (int i = 0; i < 7; i++)
        {
            float ox = Random.Range(-6f, 6f), oz = Random.Range(-3f, 4f);
            var size = new Vector3(Random.Range(7f, 10f), Random.Range(8f, 10f), Random.Range(6f, 8f));
            var rockPosition = fallPos + new Vector3(ox, 0, oz);
            float ground = TerrainWorldHeight(rockPosition);
            rockPosition.y = ground + size.y * 0.4f;
            ScenicBackdrop.Crag(root, rockPosition, size, rock);
        }
        // Falling water sheet
        var fallSheet = new GameObject("Waterfall Cascade");
        fallSheet.transform.SetParent(root);
        fallSheet.transform.position = new Vector3(-48, 4.2f, 25);
        fallSheet.transform.rotation = Quaternion.Euler(5, 160, 0);
        var fmesh = new Mesh();
        fmesh.vertices = new[] { new Vector3(-3.5f, -4, 0), new Vector3(3.5f, -4, 0), new Vector3(3.5f, 4, 0), new Vector3(-3.5f, 4, 0) };
        fmesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        fmesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
        fmesh.RecalculateNormals();
        fallSheet.AddComponent<MeshFilter>().sharedMesh = fmesh;
        fallSheet.AddComponent<MeshRenderer>().sharedMaterial = waterFall;
        RuntimeResources.Own(fallSheet, fmesh);

        // --- 2. Mountain Waterfall Pool (Kolam Sumber Air) ---
        Vector3 poolPos = new Vector3(-46, 0.22f, 20);
        ScenicBackdrop.WaterPlane(root, "Mountain Waterfall Pool", poolPos, new Vector2(16, 14), waterFlow);
        WaterVolume.Register(root.gameObject, new Bounds(poolPos, new Vector3(17, 1.5f, 15)), 0.25f);

        // --- 3. Flowing River Winding to Town (Sungai Mengalir) ---
        for (int i = 0; i < RiverWaypoints.Length - 1; i++)
        {
            Vector3 a = RiverWaypoints[i];
            Vector3 b = RiverWaypoints[i + 1];
            Vector3 mid = (a + b) * 0.5f;
            float length = Vector3.Distance(a, b);
            float angle = Mathf.Atan2(b.x - a.x, b.z - a.z) * Mathf.Rad2Deg;

            var rseg = new GameObject("River segment " + i);
            rseg.transform.SetParent(root);
            rseg.transform.position = mid;
            rseg.transform.rotation = Quaternion.Euler(0, angle, 0);

            var rmesh = new Mesh();
            float halfW = RiverHalfWidth;
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
                    Vector3 rockPos = mid + Quaternion.Euler(0, angle, 0) * new Vector3(s * 6.3f, -0.05f, st * (length * 0.22f));
                    var stoneObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    stoneObj.name = "River rock";
                    stoneObj.transform.SetParent(root);
                    var size = new Vector3(Random.Range(0.65f, 1.15f), Random.Range(0.28f, 0.52f), Random.Range(0.65f, 1.15f));
                    float ground = TerrainWorldHeight(rockPos);
                    rockPos.y = ground + size.y * 0.32f;
                    stoneObj.transform.position = rockPos;
                    stoneObj.transform.localScale = size;
                    stoneObj.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
                    stoneObj.GetComponent<Renderer>().sharedMaterial = rock;
                    Object.Destroy(stoneObj.GetComponent<Collider>());
                }
            }
        }

        // Townside Lake / Basin (tempat muara sungai)
        Vector3 basinPos = new Vector3(-38, 0.1f, -34);
        var basinFootprint = ScenicBackdrop.NaturalWaterPlane(root, "Riverside Lake Basin", basinPos, new Vector2(14, 12), waterFlow);
        WaterVolume.Register(root.gameObject, new Bounds(basinPos, new Vector3(17, 1.5f, 15)), 0.13f, basinFootprint);

        // --- 4. Arched Stone Bridge over River (Jembatan Batu Desa) ---
        Vector3 bridgePos = new Vector3(-28, 0f, -8);
        var bridge = new GameObject("Eldervale Stone Bridge");
        bridge.transform.SetParent(root);
        bridge.transform.position = bridgePos;
        var flow = (new Vector3(RiverWaypoints[3].x - RiverWaypoints[1].x, 0f, RiverWaypoints[3].z - RiverWaypoints[1].z)).normalized;
        bridge.transform.rotation = Quaternion.LookRotation(flow, Vector3.up);

        // The deck crosses the river at right angles; the old short east-west
        // slab missed the diagonal banks and had no walkable connection to land.
        var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deck.name = "Bridge Deck";
        deck.transform.SetParent(bridge.transform, false);
        deck.transform.localPosition = new Vector3(0, 0.35f, 0);
        deck.transform.localScale = new Vector3(14.5f, 0.5f, 6.5f);
        deck.GetComponent<Renderer>().sharedMaterial = stone;

        // Solid stone approaches rise from the level ground into both deck ends.
        // Their lower faces embed into the carved bank so there is no visible gap.
        for (int side = -1; side <= 1; side += 2)
            BuildApproach(bridge.transform, side, stone, 14.5f, 6.5f, .6f, -.9f);

        // Abutments reach below the waterline and support the bridge at the banks.
        for (int side = -1; side <= 1; side += 2)
        {
            var abutment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            abutment.name = "Bridge bank abutment";
            abutment.transform.SetParent(bridge.transform, false);
            abutment.transform.localPosition = new Vector3(side * 6.35f, -.4f, 0f);
            abutment.transform.localScale = new Vector3(1.05f, 1f, 2.4f);
            abutment.GetComponent<Renderer>().sharedMaterial = stone;
        }

        // Stone railings
        for (int side = -1; side <= 1; side += 2)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Bridge Parapet";
            rail.transform.SetParent(bridge.transform, false);
            rail.transform.localPosition = new Vector3(0, 1.03f, side * 3.15f);
            rail.transform.localScale = new Vector3(14.5f, 0.72f, 0.35f);
            rail.GetComponent<Renderer>().sharedMaterial = stone;

            // Bridge Lanterns on stone pillars
            for (int end = -1; end <= 1; end += 2)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.transform.SetParent(bridge.transform, false);
                pillar.transform.localPosition = new Vector3(end * 6.9f, 1.15f, side * 3.15f);
                pillar.transform.localScale = new Vector3(0.55f, 1.3f, 0.55f);
                pillar.GetComponent<Renderer>().sharedMaterial = stone;

                var blantern = new GameObject("Bridge Lantern").AddComponent<Light>();
                blantern.transform.SetParent(bridge.transform, false);
                blantern.transform.localPosition = new Vector3(end * 6.9f, 1.95f, side * 3.15f);
                blantern.type = LightType.Point;
                blantern.color = new Color(1f, 0.85f, 0.55f);
                blantern.range = 7f;
                blantern.intensity = 1.4f;
            }
        }

        // --- 5. Rustic Watermill (Kincir Air Desa) ---
        Vector3 millFlow = (RiverWaypoints[2] - RiverWaypoints[1]).normalized;
        Vector3 riverNormal = Vector3.Cross(millFlow, Vector3.up).normalized;
        Vector3 millBase = (RiverWaypoints[1] + RiverWaypoints[2]) * 0.5f - riverNormal * 7f;
        millBase.y = TerrainWorldHeight(millBase);
        Quaternion millRotation = Quaternion.LookRotation(-millFlow, Vector3.up);
        var mill = new GameObject("Watermill Assembly");
        mill.transform.SetParent(root, false);
        mill.transform.SetPositionAndRotation(millBase, millRotation);

        var millCollider = mill.AddComponent<BoxCollider>();
        millCollider.center = new Vector3(0, 2.35f, 0);
        millCollider.size = new Vector3(5.5f, 4.8f, 6.5f);

        Material millTimber = WatermillMaterial(mill, "Watermill oak frame", new Color(.22f, .13f, .075f), .2f);
        Material millSiding = WatermillMaterial(mill, "Watermill warm timber siding", new Color(.48f, .34f, .2f), .24f);
        Material millRoof = WatermillMaterial(mill, "Watermill slate roof", new Color(.22f, .25f, .22f), .18f);
        Material millGlass = WatermillMaterial(mill, "Watermill dark glass", new Color(.09f, .16f, .16f), .58f);

        // A low stone plinth keeps the timber structure grounded above the bank.
        WatermillBlock(mill.transform, "Watermill stone foundation", new Vector3(0, .31f, 0), new Vector3(5.5f, .62f, 6.5f), stone);

        // Timber sill and eaves beams form the visible skeleton of the mill.
        foreach (int side in new[] { -1, 1 })
        {
            float x = side * 2.4f;
            WatermillBlock(mill.transform, "Watermill side sill", new Vector3(x, .78f, 0), new Vector3(.24f, .2f, 5.85f), millTimber);
            WatermillBlock(mill.transform, "Watermill side plate", new Vector3(x, 3.3f, 0), new Vector3(.24f, .22f, 5.85f), millTimber);
            foreach (int end in new[] { -1, 1 })
                WatermillBlock(mill.transform, "Watermill corner post", new Vector3(x, 2.02f, end * 2.78f), new Vector3(.3f, 2.62f, .3f), millTimber);
        }
        foreach (int end in new[] { -1, 1 })
        {
            float z = end * 2.78f;
            WatermillBlock(mill.transform, "Watermill gable sill", new Vector3(0, .78f, z), new Vector3(4.9f, .2f, .24f), millTimber);
            WatermillBlock(mill.transform, "Watermill gable plate", new Vector3(0, 3.3f, z), new Vector3(4.9f, .22f, .24f), millTimber);
            WatermillBlock(mill.transform, "Watermill gable wall", new Vector3(0, 2.02f, end * 2.84f), new Vector3(4.55f, 2.38f, .12f), millSiding);
            WatermillGable(mill.transform, "Watermill timber gable", end * 2.84f, millSiding);

            float windowZ = end * 2.93f;
            WatermillBlock(mill.transform, "Watermill window glass", new Vector3(0, 2.25f, windowZ), new Vector3(.78f, .62f, .045f), millGlass);
            foreach (int side in new[] { -1, 1 })
                WatermillBlock(mill.transform, "Watermill window upright", new Vector3(side * .48f, 2.25f, windowZ), new Vector3(.12f, .82f, .12f), millTimber);
            foreach (int row in new[] { -1, 1 })
                WatermillBlock(mill.transform, "Watermill window rail", new Vector3(0, 2.25f + row * .4f, windowZ), new Vector3(1.08f, .12f, .12f), millTimber);
            WatermillBlock(mill.transform, "Watermill window crossbar", new Vector3(0, 2.25f, windowZ), new Vector3(.1f, .62f, .12f), millTimber);
        }

        // Warm plank walls leave a framed opening on the river side for the wheel.
        WatermillBlock(mill.transform, "Watermill bank wall front", new Vector3(-2.4f, 1.97f, -1.73f), new Vector3(.12f, 2.34f, 2.05f), millSiding);
        WatermillBlock(mill.transform, "Watermill bank wall rear", new Vector3(-2.4f, 1.97f, 1.73f), new Vector3(.12f, 2.34f, 2.05f), millSiding);
        WatermillBlock(mill.transform, "Watermill door", new Vector3(-2.49f, 1.62f, 0), new Vector3(.09f, 1.78f, .94f), millTimber);
        foreach (int side in new[] { -1, 1 })
            WatermillBlock(mill.transform, "Watermill door jamb", new Vector3(-2.53f, 1.67f, side * .55f), new Vector3(.15f, 1.9f, .13f), millTimber);
        WatermillBlock(mill.transform, "Watermill door lintel", new Vector3(-2.53f, 2.64f, 0), new Vector3(.15f, .14f, 1.23f), millTimber);
        WatermillBlock(mill.transform, "Watermill door handle", new Vector3(-2.56f, 1.63f, -.3f), new Vector3(.06f, .1f, .06f), stone);

        // The wheel aperture is wider than the paddles so the spinning wheel reads clearly.
        foreach (int side in new[] { -1, 1 })
            WatermillBlock(mill.transform, "Watermill river wall panel", new Vector3(2.4f, 1.97f, side * 2.55f), new Vector3(.12f, 2.34f, .48f), millSiding);
        WatermillBlock(mill.transform, "Watermill river wall upper panel", new Vector3(2.4f, 3.19f, 0), new Vector3(.12f, .18f, 5.55f), millSiding);
        WatermillBlock(mill.transform, "Watermill river wall lower panel", new Vector3(2.4f, .86f, 0), new Vector3(.12f, .24f, 5.55f), millSiding);

        // Two pitched roof panels, with exposed rafters and a stout ridge cap.
        float roofAngle = Mathf.Atan2(1.48f, 2.68f) * Mathf.Rad2Deg;
        float roofLength = Mathf.Sqrt(1.48f * 1.48f + 2.68f * 2.68f) + .2f;
        foreach (int side in new[] { -1, 1 })
        {
            Quaternion slope = Quaternion.Euler(0, 0, -side * roofAngle);
            Vector3 roofCenter = new Vector3(side * 1.34f, 4.05f, 0);
            WatermillBlock(mill.transform, "Watermill pitched roof", roofCenter, new Vector3(roofLength, .18f, 6.75f), millRoof, slope);
            foreach (int rafter in new[] { -2, -1, 0, 1, 2 })
                WatermillBlock(mill.transform, "Watermill roof rafter", roofCenter + new Vector3(0, 0, rafter * 1.3f), new Vector3(roofLength, .12f, .14f), millTimber, slope);
            WatermillBlock(mill.transform, "Watermill eave beam", new Vector3(side * 2.68f, 3.32f, 0), new Vector3(.2f, .22f, 6.75f), millTimber);
        }
        WatermillBlock(mill.transform, "Watermill ridge beam", new Vector3(0, 4.82f, 0), new Vector3(.28f, .25f, 6.85f), millTimber);

        // Rotating Water Wheel
        var wheelObj = new GameObject("Watermill Wheel");
        wheelObj.transform.SetParent(mill.transform, false);
        wheelObj.transform.localPosition = new Vector3(3.25f, 1.2f, 0);
        wheelObj.AddComponent<WaterWheelRotator>();
        const int wheelSections = 8;
        const float wheelRadius = 2f;
        foreach (int side in new[] { -1, 1 })
        {
            float axleSide = side * .46f;
            for (int p = 0; p < wheelSections; p++)
            {
                float a0 = p * Mathf.PI * 2f / wheelSections;
                float a1 = (p + 1) * Mathf.PI * 2f / wheelSections;
                Vector3 point0 = new Vector3(0, Mathf.Sin(a0) * wheelRadius, Mathf.Cos(a0) * wheelRadius);
                Vector3 point1 = new Vector3(0, Mathf.Sin(a1) * wheelRadius, Mathf.Cos(a1) * wheelRadius);
                Vector3 tangent = (point1 - point0).normalized;
                Vector3 middle = (point0 + point1) * .5f;
                WatermillBlock(wheelObj.transform, "Waterwheel outer rim", new Vector3(axleSide, middle.y, middle.z), new Vector3(Vector3.Distance(point0, point1) + .08f, .15f, .15f), millTimber, Quaternion.FromToRotation(Vector3.right, tangent));

                Vector3 radial = new Vector3(0, Mathf.Sin(a0), Mathf.Cos(a0));
                Vector3 spokeCenter = radial * 1.02f;
                WatermillBlock(wheelObj.transform, "Waterwheel spoke", new Vector3(axleSide, spokeCenter.y, spokeCenter.z), new Vector3(1.95f, .14f, .14f), millTimber, Quaternion.FromToRotation(Vector3.right, radial));
            }
        }
        for (int p = 0; p < 8; p++)
        {
            float a = p / 8f * Mathf.PI * 2f;
            WatermillBlock(wheelObj.transform, "Waterwheel paddle", new Vector3(0, Mathf.Sin(a) * 1.76f, Mathf.Cos(a) * 1.76f), new Vector3(1.12f, .14f, .72f), millSiding, Quaternion.Euler(p * 45f, 0, 0));
        }
        var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hub.transform.SetParent(wheelObj.transform, false);
        hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
        hub.transform.localScale = new Vector3(.42f, .72f, .42f);
        hub.GetComponent<Renderer>().sharedMaterial = millTimber;
        Object.Destroy(hub.GetComponent<Collider>());
    }

    static Material WatermillMaterial(GameObject owner, string name, Color color, float smoothness)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        material.SetFloat("_Smoothness", smoothness);
        return RuntimeResources.Own(owner, material);
    }

    static GameObject WatermillBlock(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        => WatermillBlock(parent, name, position, size, material, Quaternion.identity);

    static GameObject WatermillBlock(Transform parent, string name, Vector3 position, Vector3 size, Material material, Quaternion rotation)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localRotation = rotation;
        block.transform.localScale = size;
        block.GetComponent<Renderer>().sharedMaterial = material;
        Object.Destroy(block.GetComponent<Collider>());
        return block;
    }

    static void WatermillGable(Transform parent, string name, float z, Material material)
    {
        var mesh = new Mesh { name = name + " mesh" };
        mesh.vertices = new[] { new Vector3(-2.28f, 3.17f, 0), new Vector3(2.28f, 3.17f, 0), new Vector3(0, 4.82f, 0) };
        mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
        mesh.triangles = new[] { 0, 1, 2, 2, 1, 0 };
        mesh.RecalculateNormals();
        var gable = new GameObject(name);
        gable.transform.SetParent(parent, false);
        gable.transform.localPosition = new Vector3(0, 0, z);
        gable.AddComponent<MeshFilter>().sharedMesh = RuntimeResources.Own(parent.gameObject, mesh);
        gable.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    static void BuildApproach(Transform parent, int side, Material stone, float deckWidth, float deckLength, float deckTop, float bottom)
    {
        const float run = 3.4f;
        var ramp = new GameObject("Bridge grounded stone approach");
        ramp.transform.SetParent(parent, false);
        ramp.transform.localPosition = new Vector3(side * deckWidth * .5f, 0f, 0f);
        ramp.transform.localRotation = side > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

        float halfDepth = deckLength * .5f;
        var mesh = new Mesh { name = "Grounded bridge approach mesh" };
        mesh.vertices = new[] {
            new Vector3(0f, deckTop, -halfDepth), new Vector3(run, 0f, -halfDepth),
            new Vector3(run, bottom, -halfDepth), new Vector3(0f, bottom, -halfDepth),
            new Vector3(0f, deckTop, halfDepth), new Vector3(run, 0f, halfDepth),
            new Vector3(run, bottom, halfDepth), new Vector3(0f, bottom, halfDepth)
        };
        mesh.triangles = new[] {
            0,1,2, 0,2,3, 4,6,5, 4,7,6,
            0,4,5, 0,5,1, 3,2,6, 3,6,7,
            0,3,7, 0,7,4, 1,5,6, 1,6,2
        };
        mesh.RecalculateNormals();
        ramp.AddComponent<MeshFilter>().sharedMesh = mesh;
        ramp.AddComponent<MeshRenderer>().sharedMaterial = stone;
        ramp.AddComponent<MeshCollider>().sharedMesh = mesh;
        RuntimeResources.Own(ramp, mesh);
    }
}

public sealed class WaterWheelRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.right * (28f * Time.deltaTime), Space.Self);
    }
}

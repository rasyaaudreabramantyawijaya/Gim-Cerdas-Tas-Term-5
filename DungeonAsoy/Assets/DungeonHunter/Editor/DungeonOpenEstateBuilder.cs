using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class DungeonPrototypeSceneBuilder
{
    [MenuItem("Dungeon Hunter/Build Open Estate (Preserve Current Scene)")]
    public static void BuildOpenEstate()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before rebuilding."); return; }
        EnsureFolders();
        if (!AssetDatabase.IsValidFolder(Root + "/Environment")) AssetDatabase.CreateFolder(Root, "Environment");
        var active = SceneManager.GetActiveScene();
        string backup = Root + "/Scenes/DungeonHunterBeforeEstate-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity";
        if (active.IsValid() && active.isLoaded) EditorSceneManager.SaveScene(active, backup, true);
        CreateMaterials();
        var wood = MakeMaterial("Estate Timber", new Color(0.26f, 0.17f, 0.12f));
        var plaster = MakeMaterial("Weathered Mansion", new Color(0.51f, 0.52f, 0.47f));
        var roofing = MakeMaterial("Weathered Red Roof", new Color(0.31f, 0.12f, 0.15f));
        var leaves = MakeMaterial("Estate Pine", new Color(0.13f, 0.31f, 0.22f));
        var moss = MakeMaterial("Estate Moss", new Color(0.27f, 0.39f, 0.22f));
        var glass = MakeMaterial("Dark Window", new Color(0.08f, 0.14f, 0.17f), 0.1f, 0.65f);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.56f, 0.66f, 0.76f);
        RenderSettings.ambientEquatorColor = new Color(0.36f, 0.42f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.23f, 0.27f, 0.22f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.63f, 0.72f, 0.74f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 170f;
        var session = new GameObject("Dungeon Session").AddComponent<DungeonSession>();
        var world = new GameObject("Abandoned Estate - Open Map").transform;
        Terrain terrain = BuildEstateTerrain(world);
        BuildEstateLandscape(world, terrain, wood, leaves, moss);
        BuildMansion(world, plaster, wood, roofing, moss, glass);
        BuildCheckpointTower(world, new Vector3(-17, 0, 1), "South watchtower", plaster, wood);
        BuildCheckpointTower(world, new Vector3(20, 0, 24), "Mansion watchtower", plaster, wood);

        var player = BuildPlayer(session);
        player.transform.position = new Vector3(0, 0.4f, -25f);
        player.AddComponent<DungeonPlayerRespawn>();
        var camera = Object.FindFirstObjectByType<Camera>();
        camera.farClipPlane = 230f;
        camera.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 2.8f, -4.4f), Quaternion.Euler(14, 0, 0));
        var fringe = player.transform.Find("Original Anime-inspired Prototype Visual/Body Motion Pivot/Hair Fringe");
        fringe.localPosition = new Vector3(0, 0.76f, 0.13f);
        fringe.localScale = new Vector3(0.33f, 0.13f, 0.15f);
        BuildNpc(world, session);
        var npc = world.Find("Gatekeeper - Friendly NPC");
        npc.position = new Vector3(2.4f, 0.08f, -19f);
        BuildArtifact(world, session);
        world.Find("Artifact - west alcove").position = new Vector3(-9f, 1f, 39f);
        world.Find("Artifact Pedestal").position = new Vector3(-9f, 0.56f, 39f);
        world.Find("Pedestal Brass Band").position = new Vector3(-9f, 0.8f, 39f);
        BuildGate(world, session);
        world.Find("North Sealed Gate").position = new Vector3(0, 0.2f, 48f);
        world.Find("Exit Trigger - slice complete").position = new Vector3(0, 1f, 55f);
        BuildArcher(world, player, session, new Vector3(9, 0.4f, 13), "Courtyard Archer", wood);
        BuildArcher(world, player, session, new Vector3(4, 0.4f, 36), "Mansion Archer", wood);
        BuildHud(session, player.GetComponent<DungeonHealth>(), player.GetComponent<DungeonPlayerInteraction>());
        var label = GameObject.Find("Expedition Label").GetComponent<UnityEngine.UI.Text>();
        label.text = "DUNGEON HUNTER / ABANDONED ESTATE";
        label.fontSize = 16;
        GameObject.Find("Dungeon HUD").transform.Find("Run End Panel/Run End Hint").GetComponent<UnityEngine.UI.Text>().text = "R: checkpoint / new run after victory";
        var sun = new GameObject("Estate late-afternoon sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.6f;
        sun.color = new Color(1f, 0.91f, 0.77f);
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
        PointLight(world, "Mansion artifact light", new Vector3(-9, 2.8f, 39), new Color(0.2f, 0.8f, 1f), 7f, 2f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Physics.SyncTransforms();
        DungeonSceneVerification.CheckGround();
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0, 1, 26), Quaternion.Euler(37, -28, 0), 65);
        Debug.Log("Dungeon Open Estate generated and saved. Previous scene: " + backup);
    }

    private static Terrain BuildEstateTerrain(Transform parent)
    {
        const int resolution = 257;
        string path = Root + "/Environment/EstateTerrain.asset";
        var data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
        if (data == null) { data = new TerrainData(); AssetDatabase.CreateAsset(data, path); }
        data.heightmapResolution = resolution;
        data.size = new Vector3(140, 18, 140);
        var heights = new float[resolution, resolution];
        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            float wx = x / (float)(resolution - 1) * 140f - 70f;
            float wz = z / (float)(resolution - 1) * 140f - 45f;
            float edge = Mathf.Max(Mathf.Abs(wx) - 24f, Mathf.Max(-wz - 31f, wz - 61f));
            float blend = Mathf.SmoothStep(0, 1, Mathf.Clamp01(edge / 24f));
            float hill = 2f + Mathf.PerlinNoise(x * 0.022f + 1.4f, z * 0.022f + 5.1f) * 8f;
            heights[z, x] = (1.05f + blend * hill) / 18f;
        }
        data.SetHeights(0, 0, heights);
        data.alphamapResolution = 128;
        data.terrainLayers = new[] { EstateLayer("Grass", new Color(0.33f, 0.43f, 0.26f)), EstateLayer("Road", new Color(0.43f, 0.39f, 0.3f)), EstateLayer("Rock", new Color(0.43f, 0.45f, 0.43f)) };
        var alpha = new float[128, 128, 3];
        for (int z = 0; z < 128; z++)
        for (int x = 0; x < 128; x++)
        {
            float wx = x / 127f * 140f - 70;
            float wz = z / 127f * 140f - 45;
            float road = Mathf.Clamp01((4.8f - Mathf.Abs(wx - Mathf.Sin(wz * 0.1f) * 1.4f)) / 2f);
            if (wz > 58 || wz < -29) road = 0;
            float rock = Mathf.Clamp01((Mathf.Abs(wx) - 42) / 20f) * 0.6f;
            alpha[z, x, 1] = road;
            alpha[z, x, 2] = rock * (1 - road);
            alpha[z, x, 0] = 1 - alpha[z, x, 1] - alpha[z, x, 2];
        }
        data.SetAlphamaps(0, 0, alpha);
        var obj = Terrain.CreateTerrainGameObject(data);
        obj.name = "Estate Terrain - hills and meadow (solid TerrainCollider)";
        obj.transform.SetParent(parent, false);
        obj.transform.position = new Vector3(-70, -1, -45);
        obj.AddComponent<DungeonGeometry>();
        var terrain = obj.GetComponent<Terrain>();
        terrain.heightmapPixelError = 4;
        var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
        if (shader != null)
        {
            string materialPath = Root + "/Materials/Estate Terrain.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath); }
            terrain.materialTemplate = material;
        }
        return terrain;
    }

    private static TerrainLayer EstateLayer(string name, Color color)
    {
        string prefix = Root + "/Environment/" + name;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "Texture.asset");
        if (texture == null)
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, true);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                texture.SetPixel(x, y, color * (0.88f + 0.22f * Mathf.PerlinNoise(x * 0.4f, y * 0.4f)));
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            AssetDatabase.CreateAsset(texture, prefix + "Texture.asset");
        }
        var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(prefix + "Layer.asset");
        if (layer == null) { layer = new TerrainLayer(); AssetDatabase.CreateAsset(layer, prefix + "Layer.asset"); }
        layer.diffuseTexture = texture;
        layer.tileSize = new Vector2(7, 7);
        layer.smoothness = 0.05f;
        EditorUtility.SetDirty(layer);
        return layer;
    }

    private static void BuildEstateLandscape(Transform root, Terrain terrain, Material wood, Material leaves, Material moss)
    {
        var random = new System.Random(4241);
        Mesh crown = EstateCone();
        for (int i = 0; i < 90; i++)
        {
            float x = (float)random.NextDouble() * 122 - 61;
            float z = (float)random.NextDouble() * 121 - 36;
            if (Mathf.Abs(x) < 25 && z > -31 && z < 62) continue;
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
            float size = 0.75f + (float)random.NextDouble() * 0.8f;
            var tree = new GameObject("Estate pine " + i).transform;
            tree.SetParent(root, false);
            tree.position = new Vector3(x, y, z);
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Tree trunk - solid";
            trunk.transform.SetParent(tree, false);
            trunk.transform.localPosition = Vector3.up * 2.4f * size;
            trunk.transform.localScale = new Vector3(0.45f * size, 2.4f * size, 0.45f * size);
            trunk.GetComponent<Renderer>().sharedMaterial = wood;
            trunk.AddComponent<DungeonGeometry>();
            for (int tier = 0; tier < 3; tier++)
            {
                var canopy = new GameObject("Pine canopy", typeof(MeshFilter), typeof(MeshRenderer));
                canopy.transform.SetParent(tree, false);
                canopy.transform.localPosition = Vector3.up * (2.7f + tier * 1.45f) * size;
                canopy.transform.localScale = new Vector3((2.3f - tier * 0.45f) * size, 3f * size, (2.3f - tier * 0.45f) * size);
                canopy.GetComponent<MeshFilter>().sharedMesh = crown;
                canopy.GetComponent<Renderer>().sharedMaterial = leaves;
            }
        }
        for (int i = 0; i < 30; i++)
        {
            float x = (i % 2 == 0 ? -1 : 1) * (25 + (float)random.NextDouble() * 34);
            float z = (float)random.NextDouble() * 110 - 28;
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Weathered boulder " + i;
            rock.transform.SetParent(root, false);
            rock.transform.position = new Vector3(x, y + 0.5f, z);
            rock.transform.localScale = new Vector3(2 + (float)random.NextDouble() * 3, 2.2f, 2.8f);
            rock.GetComponent<Renderer>().sharedMaterial = i % 3 == 0 ? moss : stone;
            rock.AddComponent<DungeonGeometry>();
        }
        for (int z = -22; z < 20; z += 6)
        {
            var paving = Box(root, "Broken pathway slab", new Vector3(Mathf.Sin(z * 0.1f), 0.03f, z), new Vector3(3.4f, 0.08f, 2.8f), floor, true);
            paving.transform.rotation = Quaternion.Euler(0, z * 0.8f, 0);
        }
        Wall(root, "Estate boundary west", new Vector3(-69, 6, 25), new Vector3(1, 16, 140));
        Wall(root, "Estate boundary east", new Vector3(69, 6, 25), new Vector3(1, 16, 140));
        Wall(root, "Estate boundary south", new Vector3(0, 6, -44), new Vector3(140, 16, 1));
        Wall(root, "Estate boundary north", new Vector3(0, 6, 94), new Vector3(140, 16, 1));
        foreach (string edge in new[] { "west", "east", "south", "north" })
            root.Find("Estate boundary " + edge).GetComponent<Renderer>().enabled = false;
        for (int i = 0; i < 6; i++)
        {
            float x = i < 3 ? -7 : 7;
            float z = -10 + (i % 3) * 10;
            Wall(root, "Ruined estate boundary pillar", new Vector3(x, 1.1f, z), new Vector3(1.1f, 2.2f, 1.1f), floor);
            Box(root, "Pillar cap", new Vector3(x, 2.2f, z), new Vector3(1.35f, 0.24f, 1.35f), floor, false);
        }
    }

    private static void BuildMansion(Transform root, Material plaster, Material wood, Material roofing, Material moss, Material glass)
    {
        var house = new GameObject("Abandoned Mansion - explorable interior").transform;
        house.SetParent(root, false);
        Box(house, "Mansion solid foundation floor", new Vector3(0, -0.05f, 35), new Vector3(28, 0.5f, 28), floor, true);
        Wall(house, "Front wall left", new Vector3(-8.5f, 3, 21), new Vector3(11, 6, 0.6f), plaster);
        Wall(house, "Front wall right", new Vector3(8.5f, 3, 21), new Vector3(11, 6, 0.6f), plaster);
        Wall(house, "Entrance lintel", new Vector3(0, 5, 21), new Vector3(6, 2, 0.7f), plaster);
        Wall(house, "Mansion west wall", new Vector3(-14, 3, 35), new Vector3(0.6f, 6, 28), plaster);
        Wall(house, "Mansion east wall", new Vector3(14, 3, 35), new Vector3(0.6f, 6, 28), plaster);
        Wall(house, "Rear wall left", new Vector3(-8, 3, 49), new Vector3(12, 6, 0.6f), plaster);
        Wall(house, "Rear wall right", new Vector3(8, 3, 49), new Vector3(12, 6, 0.6f), plaster);
        Wall(house, "Rear lintel", new Vector3(0, 5, 49), new Vector3(4, 2, 0.6f), plaster);
        Box(house, "Rear exit path floor", new Vector3(0, -0.05f, 53), new Vector3(5, 0.5f, 8), floor, true);
        for (int side = -1; side <= 1; side += 2)
        {
            Wall(house, "Wing divider front", new Vector3(side * 5, 2, 28), new Vector3(0.4f, 4, 8), plaster);
            Wall(house, "Wing divider rear", new Vector3(side * 5, 2, 44), new Vector3(0.4f, 4, 8), plaster);
            for (int w = 0; w < 2; w++)
            {
                float x = side * (7.5f + w * 4f);
                Box(house, "Window stone surround", new Vector3(x, 3.1f, 20.6f), new Vector3(2.3f, 3, 0.25f), stone, false);
                Box(house, "Abandoned dark window", new Vector3(x, 3.1f, 20.4f), new Vector3(1.8f, 2.5f, 0.08f), glass, false);
                for (int b = 0; b < 2; b++)
                {
                    var board = Box(house, "Broken boarded window", new Vector3(x, 2.7f + b * 0.8f, 20.3f), new Vector3(2.2f, 0.2f, 0.1f), wood, false);
                    board.transform.rotation = Quaternion.Euler(0, 0, b == 0 ? 13 : -16);
                }
            }
            Wall(house, "Entrance column", new Vector3(side * 3.1f, 2, 19.8f), new Vector3(0.8f, 4, 0.8f), stone);
            Box(house, "Entrance column capital", new Vector3(side * 3.1f, 4, 19.8f), new Vector3(1.1f, 0.3f, 1.1f), trim, false);
            Wall(house, "Wing chimney", new Vector3(side * 11, 7.5f, 39), new Vector3(1.2f, 5, 1.3f), stone);
            for (int i = 0; i < 9; i++)
                Part(house, "Moss on facade", PrimitiveType.Sphere, new Vector3(side * (5.4f + i * 0.7f), 0.7f + (i % 3) * 0.35f, 20.6f), new Vector3(0.8f, 0.6f, 0.18f), moss);
        }
        Roof(house, "Mansion pitched roof", new Vector3(0, 6, 35), new Vector3(29.5f, 4.5f, 30f), roofing);
        Box(house, "Front porch", new Vector3(0, -0.03f, 19.2f), new Vector3(8, 0.3f, 3), floor, true);
        for (int i = 0; i < 6; i++)
            Box(house, "Fallen timber", new Vector3(-11 + i * 4.2f, 0.4f, 44 + i % 2), new Vector3(2.8f, 0.35f, 0.4f), wood, true).transform.rotation = Quaternion.Euler(0, i * 24, 0);
    }

    private static void BuildCheckpointTower(Transform root, Vector3 position, string name, Material masonry, Material wood)
    {
        var tower = new GameObject(name + " - checkpoint turret").transform;
        tower.SetParent(root, false);
        tower.position = position;
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Solid stone tower";
        cylinder.transform.SetParent(tower, false);
        cylinder.transform.localPosition = Vector3.up * 3.4f;
        cylinder.transform.localScale = new Vector3(5, 3.4f, 5);
        cylinder.GetComponent<Renderer>().sharedMaterial = masonry;
        cylinder.AddComponent<DungeonGeometry>();
        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI * 2 / 10;
            Box(tower, "Turret crenellation", new Vector3(Mathf.Sin(angle) * 2.25f, 7.1f, Mathf.Cos(angle) * 2.25f), new Vector3(0.7f, 1.1f, 0.7f), stone, true);
        }
        Box(tower, "Tower wooden doorway", new Vector3(0, 1.2f, -2.55f), new Vector3(1.6f, 2.4f, 0.12f), wood, false);
        var checkpoint = new GameObject(name + " Beacon");
        checkpoint.transform.SetParent(tower, false);
        checkpoint.transform.localPosition = new Vector3(0, 0, -4.2f);
        var sensor = checkpoint.AddComponent<SphereCollider>();
        sensor.isTrigger = true;
        sensor.center = Vector3.up;
        sensor.radius = 1.1f;
        Box(checkpoint.transform, "Checkpoint plinth", new Vector3(0, 0.25f, 0), new Vector3(1, 0.5f, 1), stone, true);
        var beacon = Part(checkpoint.transform, "Checkpoint crystal", PrimitiveType.Sphere, new Vector3(0, 1.1f, 0), new Vector3(0.4f, 0.7f, 0.4f), cyan);
        checkpoint.AddComponent<DungeonCheckpoint>().Configure(name, position + new Vector3(0, 0.4f, -6.5f), beacon.GetComponent<Renderer>());
        PointLight(tower, "Checkpoint light", position + new Vector3(0, 2, -4.2f), new Color(0.3f, 0.9f, 0.8f), 6f, 2f);
    }

    private static void BuildArcher(Transform root, GameObject player, DungeonSession session, Vector3 position, string name, Material wood)
    {
        var enemy = BuildEnemy(root, player.transform, session);
        enemy.name = name;
        enemy.transform.position = position;
        enemy.GetComponent<DungeonEnemyBrain>().Configure(player.transform, player.GetComponent<DungeonHealth>(), session, true, trim);
        var visual = enemy.transform.Find("Warden Visual");
        Object.DestroyImmediate(visual.Find("Rust Blade").gameObject);
        var bow = new GameObject("Longbow").transform;
        bow.SetParent(visual, false);
        bow.localPosition = new Vector3(0.55f, 1.2f, 0.4f);
        var upper = Part(bow, "Bow upper limb", PrimitiveType.Cube, new Vector3(-0.08f, 0.32f, 0), new Vector3(0.09f, 0.72f, 0.1f), wood);
        upper.transform.localRotation = Quaternion.Euler(0, 0, -23);
        var lower = Part(bow, "Bow lower limb", PrimitiveType.Cube, new Vector3(-0.08f, -0.32f, 0), new Vector3(0.09f, 0.72f, 0.1f), wood);
        lower.transform.localRotation = Quaternion.Euler(0, 0, 23);
        Part(bow, "Bow string", PrimitiveType.Cube, new Vector3(0.09f, 0, 0), new Vector3(0.015f, 1.3f, 0.015f), silver);
    }

    private static Mesh EstateCone()
    {
        string path = Root + "/Environment/PineCrown.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;
        mesh = new Mesh { name = "Low-poly pine crown" };
        var vertices = new Vector3[9];
        vertices[0] = Vector3.up;
        vertices[8] = Vector3.zero;
        for (int i = 0; i < 7; i++) vertices[i + 1] = new Vector3(Mathf.Cos(i * Mathf.PI * 2 / 7), 0, Mathf.Sin(i * Mathf.PI * 2 / 7));
        var triangles = new int[42];
        for (int i = 0; i < 7; i++)
        {
            triangles[i * 6] = 0; triangles[i * 6 + 1] = (i + 1) % 7 + 1; triangles[i * 6 + 2] = i + 1;
            triangles[i * 6 + 3] = 8; triangles[i * 6 + 4] = i + 1; triangles[i * 6 + 5] = (i + 1) % 7 + 1;
        }
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static void Roof(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        string path = Root + "/Environment/MansionRoof.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = new Mesh { name = "Pitched roof" };
            mesh.vertices = new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(0, 1, -0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0, 1, 0.5f), new Vector3(0.5f, 0, 0.5f) };
            mesh.triangles = new[] { 0, 1, 2, 3, 5, 4, 0, 3, 4, 0, 4, 1, 1, 4, 5, 1, 5, 2 };
            mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, path);
        }
        var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        obj.AddComponent<DungeonGeometry>();
    }
}

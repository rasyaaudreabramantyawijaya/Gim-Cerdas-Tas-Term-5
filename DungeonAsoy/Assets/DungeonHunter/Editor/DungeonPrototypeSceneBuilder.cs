using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class DungeonPrototypeSceneBuilder
{
    public const string ScenePath = "Assets/DungeonHunter/Scenes/DungeonHunterPrototype.unity";
    private const string Root = "Assets/DungeonHunter";
    private const string BuildGuardKey = "DungeonHunter.Week1.BuildInProgress";
    private static Material stone, floor, trim, blue, violet, silver, skin, gold, cyan, enemyMat, cloth, uiDark, uiAccent;

    [MenuItem("Dungeon Hunter/Build Week 1 Prototype")]
    public static void BuildFromMenu()
    {
        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Build Dungeon Hunter",
                "This replaces the DungeonHunterPrototype scene. Continue?", "Replace prototype", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildScene();
    }

    public static void BuildIfMissing()
    {
        if (File.Exists(ScenePath) || EditorPrefs.GetBool(BuildGuardKey)) return;
        EditorPrefs.SetBool(BuildGuardKey, true);
        try
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) BuildScene();
        }
        finally { EditorPrefs.SetBool(BuildGuardKey, false); }
    }

    private static void BuildScene()
    {
        EnsureFolders();
        CreateMaterials();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "DungeonHunterPrototype";
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.31f, 0.34f, 0.42f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.09f, 0.12f, 0.18f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 25f;
        RenderSettings.fogEndDistance = 75f;

        var sessionObject = new GameObject("Dungeon Session");
        var session = sessionObject.AddComponent<DungeonSession>();
        var world = new GameObject("Dungeon Geometry").transform;
        BuildCrypt(world);
        BuildDecor(world);
        var player = BuildPlayer(session);
        BuildNpc(world, session);
        BuildArtifact(world, session);
        BuildGate(world, session);
        BuildEnemy(world, player.transform, session);
        BuildHud(session, player.GetComponent<DungeonHealth>(), player.GetComponent<DungeonPlayerInteraction>());
        BuildLighting(world);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Dungeon Hunter Week 1 prototype generated: " + ScenePath);
    }

    private static void EnsureFolders()
    {
        string[] folders = { Root, Root + "/Scripts", Root + "/Editor", Root + "/Scenes", Root + "/Materials", Root + "/Prefabs", Root + "/Tests", Root + "/Tests/EditMode" };
        foreach (var folder in folders)
        {
            if (AssetDatabase.IsValidFolder(folder)) continue;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            var name = Path.GetFileName(folder);
            if (AssetDatabase.IsValidFolder(parent)) AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static Material MakeMaterial(string name, Color color, float metallic = 0f, float smoothness = 0.25f, bool emission = false)
    {
        string path = Root + "/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            if (existing.HasProperty("_BaseColor")) existing.SetColor("_BaseColor", color);
            return existing;
        }
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.4f);
        }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreateMaterials()
    {
        stone = MakeMaterial("Crypt Stone", new Color(0.20f, 0.24f, 0.31f), 0.18f);
        floor = MakeMaterial("Crypt Floor", new Color(0.30f, 0.34f, 0.39f), 0.06f);
        trim = MakeMaterial("Aged Brass", new Color(0.64f, 0.42f, 0.17f), 0.62f, 0.45f);
        blue = MakeMaterial("Adventurer Blue", new Color(0.10f, 0.25f, 0.54f), 0.04f);
        violet = MakeMaterial("Cloak Violet", new Color(0.28f, 0.13f, 0.43f), 0.02f);
        silver = MakeMaterial("Silver Hair", new Color(0.78f, 0.83f, 0.92f), 0.14f, 0.42f);
        skin = MakeMaterial("Warm Skin", new Color(0.91f, 0.66f, 0.54f));
        gold = MakeMaterial("Artifact Gold", new Color(1f, 0.68f, 0.14f), 0.55f, 0.65f, true);
        cyan = MakeMaterial("Artifact Light", new Color(0.22f, 0.88f, 0.96f), 0.18f, 0.5f, true);
        enemyMat = MakeMaterial("Crypt Warden", new Color(0.47f, 0.18f, 0.16f), 0.24f);
        cloth = MakeMaterial("Keeper Teal", new Color(0.15f, 0.43f, 0.43f), 0.02f);
        uiDark = MakeMaterial("UI Dark", new Color(0.055f, 0.075f, 0.12f, 0.94f));
        uiAccent = MakeMaterial("UI Accent", new Color(0.9f, 0.62f, 0.19f));
    }

    private static void BuildCrypt(Transform root)
    {
        Box(root, "Entrance Floor", new Vector3(0, -0.25f, 0), new Vector3(8, 0.5f, 8), floor, true);
        Box(root, "North Passage Floor", new Vector3(0, -0.25f, 5.5f), new Vector3(4, 0.5f, 3), floor, true);
        Box(root, "War Hall Floor", new Vector3(0, -0.25f, 14), new Vector3(18, 0.5f, 14), floor, true);
        Box(root, "Artifact Alcove Floor", new Vector3(-13, -0.25f, 14), new Vector3(8, 0.5f, 8), floor, true);
        Box(root, "Exit Passage Floor", new Vector3(0, -0.25f, 25.5f), new Vector3(4, 0.5f, 9), floor, true);

        Wall(root, "Entrance West", new Vector3(-4.15f, 1.6f, 0), new Vector3(0.3f, 3.2f, 8.3f));
        Wall(root, "Entrance East", new Vector3(4.15f, 1.6f, 0), new Vector3(0.3f, 3.2f, 8.3f));
        Wall(root, "Entrance South", new Vector3(0, 1.6f, -4.15f), new Vector3(8.3f, 3.2f, 0.3f));
        Wall(root, "Entrance North West", new Vector3(-3f, 1.6f, 4.15f), new Vector3(2f, 3.2f, 0.3f));
        Wall(root, "Entrance North East", new Vector3(3f, 1.6f, 4.15f), new Vector3(2f, 3.2f, 0.3f));

        Wall(root, "Hall West", new Vector3(-9.15f, 1.6f, 14), new Vector3(0.3f, 3.2f, 14.3f));
        Wall(root, "Hall East", new Vector3(9.15f, 1.6f, 14), new Vector3(0.3f, 3.2f, 14.3f));
        Wall(root, "Hall South West", new Vector3(-5.5f, 1.6f, 6.85f), new Vector3(7f, 3.2f, 0.3f));
        Wall(root, "Hall South East", new Vector3(5.5f, 1.6f, 6.85f), new Vector3(7f, 3.2f, 0.3f));
        Wall(root, "Hall North West", new Vector3(-5.5f, 1.6f, 21.15f), new Vector3(7f, 3.2f, 0.3f));
        Wall(root, "Hall North East", new Vector3(5.5f, 1.6f, 21.15f), new Vector3(7f, 3.2f, 0.3f));

        Wall(root, "Alcove West", new Vector3(-17.15f, 1.6f, 14), new Vector3(0.3f, 3.2f, 8.3f));
        Wall(root, "Alcove North", new Vector3(-13f, 1.6f, 18.15f), new Vector3(8.3f, 3.2f, 0.3f));
        Wall(root, "Alcove South", new Vector3(-13f, 1.6f, 9.85f), new Vector3(8.3f, 3.2f, 0.3f));
        Wall(root, "Alcove East North", new Vector3(-8.85f, 1.6f, 17f), new Vector3(0.3f, 2.2f, 2f));
        Wall(root, "Alcove East South", new Vector3(-8.85f, 1.6f, 11f), new Vector3(0.3f, 2.2f, 2f));

        Wall(root, "Exit Passage West", new Vector3(-2.15f, 1.6f, 25.5f), new Vector3(0.3f, 3.2f, 9.3f));
        Wall(root, "Exit Passage East", new Vector3(2.15f, 1.6f, 25.5f), new Vector3(0.3f, 3.2f, 9.3f));

        for (int z = -3; z <= 28; z += 4)
        {
            if (z < 3 || z > 21)
            {
                Pillar(root, new Vector3(-3.65f, 0, z), 0.42f);
                Pillar(root, new Vector3(3.65f, 0, z), 0.42f);
            }
        }
    }

    private static void BuildDecor(Transform root)
    {
        for (int i = 0; i < 5; i++)
        {
            float z = 8.5f + i * 2.7f;
            Wall(root, "Hall Floor Inlay " + i, new Vector3(-2.4f, 0.015f, z), new Vector3(0.06f, 0.035f, 1.3f), trim, false);
            Wall(root, "Hall Floor Inlay " + i + "b", new Vector3(2.4f, 0.015f, z), new Vector3(0.06f, 0.035f, 1.3f), trim, false);
        }
        for (int i = 0; i < 3; i++)
        {
            var banner = Box(root, "Crypt Banner " + i, new Vector3(0, 2.65f, 11f + i * 6f), new Vector3(1.15f, 0.9f, 0.08f), violet, false);
            banner.transform.localRotation = Quaternion.Euler(0, 0, 45f);
        }
    }

    private static GameObject BuildPlayer(DungeonSession session)
    {
        var player = new GameObject("Player - Silver-haired Adventurer");
        player.transform.position = new Vector3(0, 0.05f, -1.2f);
        player.layer = 2;
        var character = player.AddComponent<CharacterController>();
        character.height = 1.72f;
        character.radius = 0.32f;
        character.center = new Vector3(0, 0.86f, 0);
        character.skinWidth = 0.055f;
        player.AddComponent<DungeonHealth>().Configure(100f);
        var controller = player.AddComponent<DungeonPlayerController>();
        var interaction = player.AddComponent<DungeonPlayerInteraction>();
        interaction.Configure(session, player.GetComponent<DungeonHealth>());
        player.AddComponent<DungeonPlayerCombat>().Configure(player.GetComponent<DungeonHealth>(), session);

        var visualRoot = new GameObject("Original Anime-inspired Prototype Visual").transform;
        visualRoot.SetParent(player.transform, false);
        SetLayerRecursively(visualRoot.gameObject, 2);
        var body = new GameObject("Body Motion Pivot").transform;
        body.SetParent(visualRoot, false);
        body.localPosition = new Vector3(0, 1.02f, 0);
        Part(body, "Blue Tunic", PrimitiveType.Capsule, Vector3.zero, new Vector3(0.47f, 0.57f, 0.30f), blue);
        Part(body, "Violet Waist Sash", PrimitiveType.Cube, new Vector3(0, -0.36f, 0), new Vector3(0.48f, 0.13f, 0.32f), violet);
        Part(body, "Gold Chest Clasp", PrimitiveType.Sphere, new Vector3(0, 0.13f, 0.166f), new Vector3(0.1f, 0.1f, 0.045f), gold);

        Part(body, "Face", PrimitiveType.Sphere, new Vector3(0, 0.62f, 0.015f), new Vector3(0.37f, 0.39f, 0.33f), skin);
        Part(body, "Hair Crown", PrimitiveType.Sphere, new Vector3(0, 0.78f, -0.045f), new Vector3(0.43f, 0.33f, 0.39f), silver);
        Part(body, "Side Hair L", PrimitiveType.Capsule, new Vector3(-0.2f, 0.54f, -0.02f), new Vector3(0.13f, 0.34f, 0.16f), silver);
        Part(body, "Side Hair R", PrimitiveType.Capsule, new Vector3(0.2f, 0.54f, -0.02f), new Vector3(0.13f, 0.34f, 0.16f), silver);
        Part(body, "Hair Fringe", PrimitiveType.Sphere, new Vector3(0, 0.66f, 0.16f), new Vector3(0.33f, 0.16f, 0.18f), silver);
        Part(body, "Eye L", PrimitiveType.Sphere, new Vector3(-0.09f, 0.63f, 0.167f), new Vector3(0.045f, 0.065f, 0.025f), violet);
        Part(body, "Eye R", PrimitiveType.Sphere, new Vector3(0.09f, 0.63f, 0.167f), new Vector3(0.045f, 0.065f, 0.025f), violet);

        var cloak = Part(body, "Short Flowing Cloak", PrimitiveType.Capsule, new Vector3(0, -0.08f, -0.24f), new Vector3(0.67f, 0.54f, 0.18f), violet).transform;
        var leftArm = LimbPivot(visualRoot, "Left Arm Swing", new Vector3(-0.3f, 1.19f, 0));
        Part(leftArm, "Blue Sleeve L", PrimitiveType.Capsule, new Vector3(0, -0.28f, 0), new Vector3(0.18f, 0.38f, 0.18f), blue);
        Part(leftArm, "Hand L", PrimitiveType.Sphere, new Vector3(0, -0.51f, 0.01f), new Vector3(0.12f, 0.13f, 0.12f), skin);
        var rightArm = LimbPivot(visualRoot, "Right Arm Swing", new Vector3(0.3f, 1.19f, 0));
        Part(rightArm, "Blue Sleeve R", PrimitiveType.Capsule, new Vector3(0, -0.28f, 0), new Vector3(0.18f, 0.38f, 0.18f), blue);
        Part(rightArm, "Hand R", PrimitiveType.Sphere, new Vector3(0, -0.51f, 0.01f), new Vector3(0.12f, 0.13f, 0.12f), skin);
        var leftLeg = LimbPivot(visualRoot, "Left Leg Swing", new Vector3(-0.16f, 0.63f, 0));
        Part(leftLeg, "Dark Leg L", PrimitiveType.Capsule, new Vector3(0, -0.25f, 0), new Vector3(0.18f, 0.33f, 0.19f), violet);
        Part(leftLeg, "Boot L", PrimitiveType.Cube, new Vector3(0, -0.48f, 0.06f), new Vector3(0.22f, 0.13f, 0.31f), stone);
        var rightLeg = LimbPivot(visualRoot, "Right Leg Swing", new Vector3(0.16f, 0.63f, 0));
        Part(rightLeg, "Dark Leg R", PrimitiveType.Capsule, new Vector3(0, -0.25f, 0), new Vector3(0.18f, 0.33f, 0.19f), violet);
        Part(rightLeg, "Boot R", PrimitiveType.Cube, new Vector3(0, -0.48f, 0.06f), new Vector3(0.22f, 0.13f, 0.31f), stone);

        var staff = new GameObject("Short Staff").transform;
        staff.SetParent(rightArm, false);
        staff.localPosition = new Vector3(0.13f, -0.48f, 0.07f);
        Part(staff, "Wooden Shaft", PrimitiveType.Cylinder, new Vector3(0, 0.39f, 0), new Vector3(0.035f, 0.42f, 0.035f), trim);
        Part(staff, "Crystal", PrimitiveType.Sphere, new Vector3(0, 0.82f, 0), new Vector3(0.16f, 0.19f, 0.16f), cyan);
        player.AddComponent<DungeonCharacterVisual>().Configure(controller, body, leftArm, rightArm, leftLeg, rightLeg, cloak, staff);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 58f;
        camera.nearClipPlane = 0.08f;
        camera.farClipPlane = 100f;
        camera.backgroundColor = new Color(0.08f, 0.11f, 0.16f);
        cameraObject.AddComponent<AudioListener>();
        var cameraRig = cameraObject.AddComponent<ThirdPersonCameraRig>();
        cameraRig.Configure(player.transform, ~0);
        controller.Configure(cameraObject.transform, session);
        return player;
    }

    private static void BuildNpc(Transform root, DungeonSession session)
    {
        var npc = new GameObject("Gatekeeper - Friendly NPC");
        npc.transform.SetParent(root, false);
        npc.transform.position = new Vector3(1.7f, 0, 1.9f);
        npc.transform.rotation = Quaternion.Euler(0, 210, 0);
        var collider = npc.AddComponent<CapsuleCollider>();
        collider.height = 1.8f;
        collider.radius = 0.38f;
        collider.center = new Vector3(0, 0.9f, 0);
        collider.isTrigger = true;
        npc.AddComponent<DungeonNpcDialogue>();
        var body = Part(npc.transform, "Keeper Tunic", PrimitiveType.Capsule, new Vector3(0, 0.95f, 0), new Vector3(0.43f, 0.53f, 0.31f), cloth);
        Part(npc.transform, "Keeper Face", PrimitiveType.Sphere, new Vector3(0, 1.56f, 0), new Vector3(0.32f, 0.36f, 0.3f), skin);
        Part(npc.transform, "Keeper Hair", PrimitiveType.Sphere, new Vector3(0, 1.74f, -0.04f), new Vector3(0.34f, 0.22f, 0.34f), stone);
        Part(npc.transform, "Keeper Belt", PrimitiveType.Cube, new Vector3(0, 0.59f, 0), new Vector3(0.42f, 0.12f, 0.3f), trim);
        var sign = Part(npc.transform, "Keeper Badge", PrimitiveType.Sphere, new Vector3(0.12f, 1.02f, 0.15f), Vector3.one * 0.1f, gold);
        sign.name = "Keeper Badge - talk to me";
    }

    private static void BuildArtifact(Transform root, DungeonSession session)
    {
        var pickup = new GameObject("Artifact - west alcove");
        pickup.transform.SetParent(root, false);
        pickup.transform.position = new Vector3(-13f, 0.75f, 14f);
        var trigger = pickup.AddComponent<SphereCollider>();
        trigger.radius = 1.05f;
        trigger.isTrigger = true;
        pickup.AddComponent<DungeonPickup>();
        var pedestal = Box(root, "Artifact Pedestal", new Vector3(-13f, 0.38f, 14f), new Vector3(1.45f, 0.76f, 1.45f), stone, false);
        Box(root, "Pedestal Brass Band", new Vector3(-13f, 0.62f, 14f), new Vector3(1.48f, 0.08f, 1.48f), trim, false);
        var artifactCore = Part(pickup.transform, "Artifact Core", PrimitiveType.Cube, new Vector3(0, 0.35f, 0), new Vector3(0.43f, 0.76f, 0.43f), cyan);
        artifactCore.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
        Part(pickup.transform, "Artifact Halo", PrimitiveType.Sphere, new Vector3(0, 0.35f, 0), Vector3.one * 0.18f, gold);
        pedestal.transform.SetParent(root, false);
    }

    private static void BuildGate(Transform root, DungeonSession session)
    {
        var gate = new GameObject("North Sealed Gate");
        gate.transform.SetParent(root, false);
        gate.transform.position = new Vector3(0, 0, 23.1f);
        var solid = gate.AddComponent<BoxCollider>();
        solid.center = new Vector3(0, 1.55f, 0);
        solid.size = new Vector3(3.9f, 3.1f, 0.45f);
        var trigger = gate.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 1f, -0.55f);
        trigger.size = new Vector3(4.7f, 2.2f, 1.8f);
        var visual = Box(gate.transform, "Sliding Gate Panel", new Vector3(0, 1.55f, 0), new Vector3(3.75f, 2.95f, 0.28f), stone, false).transform;
        for (int x = -1; x <= 1; x++)
            Box(visual, "Gate Brass Rib", new Vector3(x * 1.15f, 0, 0.16f), new Vector3(0.1f, 2.9f, 0.06f), trim, false);
        var door = gate.AddComponent<DungeonExitDoor>();
        door.Configure(session, visual, solid);
        var exit = new GameObject("Exit Trigger - slice complete");
        exit.transform.SetParent(root, false);
        exit.transform.position = new Vector3(0, 0.9f, 29.4f);
        var exitCollider = exit.AddComponent<BoxCollider>();
        exitCollider.isTrigger = true;
        exitCollider.size = new Vector3(3.3f, 2.4f, 0.9f);
        exit.AddComponent<DungeonExitTrigger>().Configure(session, door);
    }

    private static GameObject BuildEnemy(Transform root, Transform player, DungeonSession session)
    {
        var enemy = new GameObject("Crypt Warden - Melee Enemy");
        enemy.transform.SetParent(root, false);
        enemy.transform.position = new Vector3(4.5f, 0.05f, 14f);
        var character = enemy.AddComponent<CharacterController>();
        character.height = 1.8f;
        character.radius = 0.38f;
        character.center = new Vector3(0, 0.9f, 0);
        enemy.AddComponent<DungeonHealth>().Configure(60f);
        enemy.AddComponent<DungeonEnemyBrain>().Configure(player, player.GetComponent<DungeonHealth>(), session);
        var visual = new GameObject("Warden Visual").transform;
        visual.SetParent(enemy.transform, false);
        Part(visual, "Warden Body", PrimitiveType.Capsule, new Vector3(0, 0.96f, 0), new Vector3(0.7f, 0.78f, 0.52f), enemyMat);
        Part(visual, "Warden Helmet", PrimitiveType.Sphere, new Vector3(0, 1.67f, 0), new Vector3(0.48f, 0.4f, 0.44f), stone);
        Part(visual, "Helmet Crest", PrimitiveType.Cube, new Vector3(0, 1.96f, -0.02f), new Vector3(0.16f, 0.35f, 0.12f), trim);
        Part(visual, "Eye L", PrimitiveType.Sphere, new Vector3(-0.12f, 1.68f, 0.2f), new Vector3(0.08f, 0.07f, 0.035f), gold);
        Part(visual, "Eye R", PrimitiveType.Sphere, new Vector3(0.12f, 1.68f, 0.2f), new Vector3(0.08f, 0.07f, 0.035f), gold);
        Part(visual, "Left Shoulder", PrimitiveType.Sphere, new Vector3(-0.42f, 1.25f, 0), Vector3.one * 0.36f, enemyMat);
        Part(visual, "Right Shoulder", PrimitiveType.Sphere, new Vector3(0.42f, 1.25f, 0), Vector3.one * 0.36f, enemyMat);
        Part(visual, "Rust Blade", PrimitiveType.Cube, new Vector3(0.6f, 0.77f, 0.15f), new Vector3(0.13f, 0.95f, 0.12f), trim);
        Part(visual, "Warden Leg L", PrimitiveType.Capsule, new Vector3(-0.19f, 0.28f, 0), new Vector3(0.25f, 0.38f, 0.28f), stone);
        Part(visual, "Warden Leg R", PrimitiveType.Capsule, new Vector3(0.19f, 0.28f, 0), new Vector3(0.25f, 0.38f, 0.28f), stone);
        return enemy;
    }

    private static void BuildHud(DungeonSession session, DungeonHealth health, DungeonPlayerInteraction interaction)
    {
        var canvasObject = new GameObject("Dungeon HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var eventObject = new GameObject("UI Event System", typeof(EventSystem));
        var eventSystem = eventObject.GetComponent<EventSystem>();
        canvasObject.AddComponent<DungeonHudPointer>().Configure(canvasObject.GetComponent<GraphicRaycaster>(), eventSystem);

        var panel = UiPanel(canvasObject.transform, "Status Panel", new Vector2(24, -24), new Vector2(410, 142), new Vector2(0, 1), new Vector2(0, 1), uiDark);
        var title = UiText(panel.transform, "Expedition Label", "DUNGEON HUNTER  /  FIRST DEPTH", 19, TextAnchor.MiddleLeft, new Vector2(14, -14), new Vector2(382, 28), new Vector2(0, 1), new Vector2(0, 1), gold.color);
        var healthText = UiText(panel.transform, "Health Text", "VITALITY  100 / 100", 18, TextAnchor.MiddleLeft, new Vector2(14, -52), new Vector2(360, 25), new Vector2(0, 1), new Vector2(0, 1), Color.white);
        var barBack = UiImage(panel.transform, "Health Bar Background", new Vector2(14, -87), new Vector2(376, 14), new Vector2(0, 1), new Vector2(0, 1), new Color(0.18f, 0.12f, 0.14f));
        var healthFill = UiImage(barBack.transform, "Health Bar Fill", Vector2.zero, new Vector2(1, 1), Vector2.zero, Vector2.one, new Color(0.77f, 0.2f, 0.22f));
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillAmount = 1f;
        var objective = UiPanel(canvasObject.transform, "Objective Panel", new Vector2(24, -178), new Vector2(410, 64), new Vector2(0, 1), new Vector2(0, 1), uiDark);
        var objectiveText = UiText(objective.transform, "Objective Text", "Objective: find the artifact west", 19, TextAnchor.MiddleLeft, new Vector2(14, 0), new Vector2(382, 54), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Color.white);
        UiText(canvasObject.transform, "Control Hint", "WASD Move    Shift Run    Mouse Look    LMB Attack", 18, TextAnchor.MiddleCenter,
            new Vector2(0, 26), new Vector2(730, 40), new Vector2(0.5f, 0), new Vector2(0.5f, 0), Color.white);
        var prompt = UiText(canvasObject.transform, "Interaction Prompt", "", 23, TextAnchor.MiddleCenter,
            new Vector2(0, 78), new Vector2(720, 48), new Vector2(0.5f, 0), new Vector2(0.5f, 0), gold.color);
        var feedback = UiText(canvasObject.transform, "Feedback", "", 22, TextAnchor.MiddleCenter,
            new Vector2(0, -42), new Vector2(800, 48), new Vector2(0.5f, 1), new Vector2(0.5f, 1), Color.white);

        var dialogue = UiPanel(canvasObject.transform, "Dialogue Panel", Vector2.zero, new Vector2(700, 320), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), uiDark);
        UiText(dialogue.transform, "Dialogue Eyebrow", "FIELD NOTES  /  GATEKEEPER", 17, TextAnchor.MiddleLeft,
            new Vector2(28, -24), new Vector2(640, 30), new Vector2(0, 1), new Vector2(0, 1), gold.color);
        var speaker = UiText(dialogue.transform, "Dialogue Speaker", "", 28, TextAnchor.MiddleLeft,
            new Vector2(28, -66), new Vector2(640, 44), new Vector2(0, 1), new Vector2(0, 1), Color.white);
        var body = UiText(dialogue.transform, "Dialogue Body", "", 22, TextAnchor.UpperLeft,
            new Vector2(28, -122), new Vector2(640, 104), new Vector2(0, 1), new Vector2(0, 1), new Color(0.84f, 0.87f, 0.92f));
        var closeButton = UiButton(dialogue.transform, "Close Dialogue", "Continue  /  Esc", new Vector2(28, 26), new Vector2(190, 48), new Vector2(0, 0), new Vector2(0, 0), uiAccent);
        var hud = canvasObject.AddComponent<DungeonHUD>();

        var endPanel = UiPanel(canvasObject.transform, "Run End Panel", Vector2.zero, new Vector2(540, 270), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), uiDark);
        var endTitle = UiText(endPanel.transform, "Run End Title", "DUNGEON CLEARED", 31, TextAnchor.MiddleCenter,
            new Vector2(0, -60), new Vector2(490, 70), new Vector2(0.5f, 1), new Vector2(0.5f, 1), gold.color);
        UiText(endPanel.transform, "Run End Hint", "Press R or choose restart", 20, TextAnchor.MiddleCenter,
            new Vector2(0, 46), new Vector2(470, 42), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);
        var restart = UiButton(endPanel.transform, "Restart Button", "Restart Run", new Vector2(0, 25), new Vector2(210, 50), new Vector2(0.5f, 0), new Vector2(0.5f, 0), uiAccent);

        var serialized = new SerializedObject(hud);
        serialized.FindProperty("healthText").objectReferenceValue = healthText;
        serialized.FindProperty("healthFill").objectReferenceValue = healthFill;
        serialized.FindProperty("objectiveText").objectReferenceValue = objectiveText;
        serialized.FindProperty("promptText").objectReferenceValue = prompt;
        serialized.FindProperty("feedbackText").objectReferenceValue = feedback;
        serialized.FindProperty("dialoguePanel").objectReferenceValue = dialogue;
        serialized.FindProperty("dialogueSpeakerText").objectReferenceValue = speaker;
        serialized.FindProperty("dialogueBodyText").objectReferenceValue = body;
        serialized.FindProperty("endPanel").objectReferenceValue = endPanel;
        serialized.FindProperty("endTitleText").objectReferenceValue = endTitle;
        serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
        serialized.FindProperty("restartButton").objectReferenceValue = restart;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        dialogue.SetActive(false);
        endPanel.SetActive(false);
        hud.Configure(session, health, interaction);
    }

    private static void BuildLighting(Transform root)
    {
        var lightObject = new GameObject("Crypt Key Light");
        lightObject.transform.SetParent(root, false);
        lightObject.transform.position = new Vector3(-3, 6, 8);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.72f, 0.80f, 1f);
        light.intensity = 0.8f;
        lightObject.transform.rotation = Quaternion.Euler(48, -32, 0);
        PointLight(root, "Warm Entrance Lantern", new Vector3(0, 2.6f, 1.3f), new Color(1f, 0.62f, 0.28f), 5.5f, 2.2f);
        PointLight(root, "Artifact Glow", new Vector3(-13, 1.5f, 14), new Color(0.19f, 0.75f, 1f), 4.5f, 1.7f);
        PointLight(root, "War Hall Brazier", new Vector3(0, 2.6f, 15), new Color(1f, 0.48f, 0.22f), 8f, 2.4f);
        PointLight(root, "North Gate Light", new Vector3(0, 2.4f, 24), new Color(0.38f, 0.66f, 1f), 5f, 1.6f);
    }

    private static void PointLight(Transform root, string name, Vector3 position, Color color, float range, float intensity)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(root, false);
        obj.transform.position = position;
        var light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
    }

    private static void AddSceneToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes;
        var expectedGuid = AssetDatabase.AssetPathToGUID(ScenePath);
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path != ScenePath) continue;
            if (scenes[i].enabled && scenes[i].guid.ToString() == expectedGuid) return;
            var corrected = new EditorBuildSettingsScene[scenes.Length];
            scenes.CopyTo(corrected, 0);
            corrected[i] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = corrected;
            return;
        }
        var updated = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(updated, 0);
        updated[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = updated;
    }

    private static Transform LimbPivot(Transform parent, string name, Vector3 position)
    {
        var pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        return pivot;
    }

    private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool geometry)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        if (!geometry)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }
        else obj.AddComponent<DungeonGeometry>();
        return obj;
    }

    private static void Wall(Transform parent, string name, Vector3 position, Vector3 size, Material material = null, bool geometry = true)
    {
        Box(parent, name, position, size, material == null ? stone : material, geometry);
    }

    private static void Pillar(Transform parent, Vector3 position, float width)
    {
        Wall(parent, "Crypt Pillar", new Vector3(position.x, 1.6f, position.z), new Vector3(width, 3.2f, width));
        Box(parent, "Pillar Capital", new Vector3(position.x, 3.15f, position.z), new Vector3(width * 1.45f, 0.22f, width * 1.45f), trim, false);
    }

    private static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        var renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        var collider = obj.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return obj;
    }

    private static GameObject UiPanel(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Material material)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        obj.GetComponent<Image>().color = material.color;
        return obj;
    }

    private static Image UiImage(Transform parent, string name, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        obj.GetComponent<Image>().color = color;
        return obj.GetComponent<Image>();
    }

    private static Text UiText(Transform parent, string name, string value, int size, TextAnchor alignment,
        Vector2 position, Vector2 dimensions, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f, anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = dimensions;
        var text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value; text.fontSize = size; text.alignment = alignment; text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button UiButton(Transform parent, string name, string label, Vector2 position, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Material material)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        var image = obj.GetComponent<Image>(); image.color = material.color;
        var button = obj.GetComponent<Button>(); button.targetGraphic = image;
        var text = UiText(obj.transform, "Label", label, 19, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, Color.white);
        text.raycastTarget = false;
        return button;
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, layer);
    }
}

[InitializeOnLoad]
public static class DungeonPrototypeBootstrap
{
    static DungeonPrototypeBootstrap()
    {
        EditorApplication.delayCall += DungeonPrototypeSceneBuilder.BuildIfMissing;
    }
}

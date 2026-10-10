using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class NPCGuardSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Praktikum02_NPCGuard.unity";

    [MenuItem("Game Cerdas/Build Praktikum 02 NPC Guard Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureLayer("Player");
        EnsureLayer("Obstacle");

        Material groundMaterial = CreateMaterial("GroundMaterial", new Color(0.26f, 0.45f, 0.30f));
        Material npcMaterial = CreateMaterial("NPCMaterial", new Color(0.86f, 0.16f, 0.20f));
        Material wallMaterial = CreateMaterial("WallMaterial", new Color(0.32f, 0.35f, 0.40f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

        CreateMaze(wallMaterial);

        GameObject navigation = new GameObject("Navigation");
        NavMeshSurface navMeshSurface = navigation.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.All;
        navMeshSurface.layerMask = LayerMask.GetMask("Default", "Obstacle");
        navMeshSurface.BuildNavMesh();

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(-7f, 1f, -7f);
        player.AddComponent<PlayerController>();
        StylePlayerCharacter(player);

        GameObject patrolRoot = new GameObject("PatrolPoints");
        Vector3[] positions =
        {
            new Vector3(-7f, 0f, 7f), new Vector3(-2f, 0f, 7f),
            new Vector3(-2f, 0f, 2f), new Vector3(2f, 0f, 2f),
            new Vector3(7f, 0f, 7f), new Vector3(7f, 0f, -7f),
            new Vector3(2f, 0f, -2f), new Vector3(-2f, 0f, -2f),
            new Vector3(-7f, 0f, -7f)
        };
        Transform[] patrolPoints = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = new GameObject($"Point{i + 1}");
            point.transform.SetParent(patrolRoot.transform);
            point.transform.position = positions[i];
            patrolPoints[i] = point.transform;
        }

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "NPC_Guard";
        npc.transform.position = new Vector3(-7f, 1f, 7f);
        npc.GetComponent<Renderer>().sharedMaterial = npcMaterial;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "DirectionMarker";
        marker.transform.SetParent(npc.transform);
        marker.transform.localPosition = new Vector3(0f, 0.5f, 0.65f);
        marker.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);
        marker.GetComponent<Renderer>().sharedMaterial = npcMaterial;

        NavMeshAgent agent = npc.AddComponent<NavMeshAgent>();
        agent.speed = 3f;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.3f;

        NPCSensor sensor = npc.AddComponent<NPCSensor>();
        sensor.Configure(player.transform, 8f, 90f, LayerMask.GetMask("Obstacle"));
        NPCBrain brain = npc.AddComponent<NPCBrain>();
        brain.Configure(sensor, agent, patrolPoints);
        StyleNpcGuard(npc);

        ConfigureCamera();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = npc;
        Debug.Log("Praktikum02_NPCGuard dibuat. Tekan Play untuk menguji NPC.");
    }

    private static void StylePlayerCharacter(GameObject player)
    {
        Transform oldVisual = player.transform.Find("BlockCharacterVisual");
        if (oldVisual != null) Object.DestroyImmediate(oldVisual.gameObject);

        Renderer capsuleRenderer = player.GetComponent<Renderer>();
        if (capsuleRenderer != null) capsuleRenderer.enabled = false;

        CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null) Object.DestroyImmediate(capsuleCollider);

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null) controller = player.AddComponent<CharacterController>();
        controller.center = Vector3.zero;
        controller.height = 2f;
        controller.radius = 0.38f;

        Material skin = CreateMaterial("PlayerSkin", new Color(0.55f, 0.33f, 0.22f));
        Material hair = CreateMaterial("PlayerHair", new Color(0.13f, 0.08f, 0.04f));
        Material shirt = CreateMaterial("PlayerShirt", new Color(0.02f, 0.63f, 0.67f));
        Material pants = CreateMaterial("PlayerPants", new Color(0.25f, 0.17f, 0.62f));
        Material white = CreateMaterial("PlayerWhite", Color.white);
        Material dark = CreateMaterial("PlayerDark", new Color(0.10f, 0.06f, 0.04f));

        GameObject visual = new GameObject("BlockCharacterVisual");
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        CreatePart("Torso", PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.23f, 0f), Vector3.zero, new Vector3(1.00f, 0.78f, 0.46f), shirt);
        CreatePart("Head", PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.95f, 0f), Vector3.zero, new Vector3(0.82f, 0.62f, 0.56f), skin);
        CreatePart("Hair", PrimitiveType.Cube, visual.transform, new Vector3(0f, 1.22f, -0.01f), Vector3.zero, new Vector3(0.84f, 0.16f, 0.58f), hair);
        CreatePart("LeftArm", PrimitiveType.Cube, visual.transform, new Vector3(-0.68f, 0.23f, 0f), Vector3.zero, new Vector3(0.34f, 0.76f, 0.42f), skin);
        CreatePart("RightArm", PrimitiveType.Cube, visual.transform, new Vector3(0.68f, 0.23f, 0f), Vector3.zero, new Vector3(0.34f, 0.76f, 0.42f), skin);
        CreatePart("LeftLeg", PrimitiveType.Cube, visual.transform, new Vector3(-0.27f, -0.54f, 0f), Vector3.zero, new Vector3(0.42f, 0.78f, 0.44f), pants);
        CreatePart("RightLeg", PrimitiveType.Cube, visual.transform, new Vector3(0.27f, -0.54f, 0f), Vector3.zero, new Vector3(0.42f, 0.78f, 0.44f), pants);
        CreatePart("LeftEye", PrimitiveType.Cube, visual.transform, new Vector3(-0.19f, 1.02f, 0.30f), Vector3.zero, new Vector3(0.18f, 0.12f, 0.03f), white);
        CreatePart("RightEye", PrimitiveType.Cube, visual.transform, new Vector3(0.19f, 1.02f, 0.30f), Vector3.zero, new Vector3(0.18f, 0.12f, 0.03f), white);
        CreatePart("Beard", PrimitiveType.Cube, visual.transform, new Vector3(0f, 0.73f, 0.30f), Vector3.zero, new Vector3(0.38f, 0.18f, 0.04f), dark);
    }

    private static void CreateMaze(Material wallMaterial)
    {
        GameObject maze = new GameObject("MazeWalls");

        // Compact loop layout: every branch reconnects to another corridor.
        CreateMazeWall("NorthWall", maze.transform, new Vector3(0f, 1.5f, 10f), new Vector3(20.6f, 3f, 0.6f), wallMaterial);
        CreateMazeWall("SouthWall", maze.transform, new Vector3(0f, 1.5f, -10f), new Vector3(20.6f, 3f, 0.6f), wallMaterial);
        CreateMazeWall("WestWall", maze.transform, new Vector3(-10f, 1.5f, 0f), new Vector3(0.6f, 3f, 20.6f), wallMaterial);
        CreateMazeWall("EastWall", maze.transform, new Vector3(10f, 1.5f, 0f), new Vector3(0.6f, 3f, 20.6f), wallMaterial);

        CreateMazeWall("InnerWallA", maze.transform, new Vector3(-5f, 1.5f, 3.5f), new Vector3(0.6f, 3f, 9f), wallMaterial);
        CreateMazeWall("InnerWallB", maze.transform, new Vector3(0f, 1.5f, -3.5f), new Vector3(0.6f, 3f, 9f), wallMaterial);
        CreateMazeWall("InnerWallC", maze.transform, new Vector3(5f, 1.5f, 3.5f), new Vector3(0.6f, 3f, 9f), wallMaterial);
        CreateMazeWall("InnerWallD", maze.transform, new Vector3(-2.5f, 1.5f, 5f), new Vector3(5f, 3f, 0.6f), wallMaterial);
        CreateMazeWall("InnerWallE", maze.transform, new Vector3(2.5f, 1.5f, -5f), new Vector3(5f, 3f, 0.6f), wallMaterial);
        CreateMazeWall("InnerWallF", maze.transform, new Vector3(-7.5f, 1.5f, -2f), new Vector3(4f, 3f, 0.6f), wallMaterial);
        CreateMazeWall("InnerWallG", maze.transform, new Vector3(7.5f, 1.5f, 2f), new Vector3(4f, 3f, 0.6f), wallMaterial);
    }

    private static void CreateMazeWall(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.SetPositionAndRotation(position, Quaternion.identity);
        wall.transform.localScale = scale;
        wall.layer = LayerMask.NameToLayer("Obstacle");
        wall.GetComponent<Renderer>().sharedMaterial = material;
    }

    [MenuItem("Game Cerdas/Style NPC Guard as Yellow Character")]
    public static void StyleCurrentNpcGuard()
    {
        GameObject npc = GameObject.Find("NPC_Guard");
        if (npc == null)
        {
            Debug.LogWarning("NPC_Guard tidak ditemukan di scene aktif.");
            return;
        }

        StyleNpcGuard(npc);
        EditorSceneManager.MarkSceneDirty(npc.scene);
        Selection.activeGameObject = npc;
        Debug.Log("Tampilan NPC_Guard diubah menjadi karakter kuning. Sensor dan Gizmos tetap aktif.");
    }

    private static void StyleNpcGuard(GameObject npc)
    {
        Transform oldVisual = npc.transform.Find("CharacterVisual");
        if (oldVisual != null)
        {
            Object.DestroyImmediate(oldVisual.gameObject);
        }

        Renderer capsuleRenderer = npc.GetComponent<Renderer>();
        if (capsuleRenderer != null)
        {
            capsuleRenderer.enabled = false;
        }

        Transform marker = npc.transform.Find("DirectionMarker");
        if (marker != null)
        {
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer != null) markerRenderer.enabled = false;
        }

        Material yellow = CreateMaterial("CharacterYellow", new Color(1f, 0.67f, 0.08f));
        Material cream = CreateMaterial("CharacterCream", new Color(1f, 0.88f, 0.53f));
        Material white = CreateMaterial("CharacterWhite", Color.white);
        Material black = CreateMaterial("CharacterBlack", new Color(0.08f, 0.06f, 0.05f));
        Material pink = CreateMaterial("CharacterPink", new Color(1f, 0.47f, 0.55f));

        GameObject visual = new GameObject("CharacterVisual");
        visual.transform.SetParent(npc.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        CreatePart("Body", PrimitiveType.Sphere, visual.transform, new Vector3(0f, 0.20f, 0f), Vector3.zero, new Vector3(1.25f, 1.22f, 0.96f), yellow);
        CreatePart("Head", PrimitiveType.Sphere, visual.transform, new Vector3(0f, 1.15f, 0.02f), Vector3.zero, new Vector3(1.28f, 1.18f, 1.10f), yellow);
        CreatePart("Belly", PrimitiveType.Sphere, visual.transform, new Vector3(0f, 0.18f, 0.50f), Vector3.zero, new Vector3(0.68f, 0.82f, 0.12f), cream);

        CreatePart("LeftArm", PrimitiveType.Sphere, visual.transform, new Vector3(-0.66f, 0.36f, 0.02f), new Vector3(0f, 0f, -32f), new Vector3(0.62f, 0.88f, 0.62f), yellow);
        CreatePart("RightArm", PrimitiveType.Sphere, visual.transform, new Vector3(0.66f, 0.36f, 0.02f), new Vector3(0f, 0f, 32f), new Vector3(0.62f, 0.88f, 0.62f), yellow);
        CreatePart("LeftFoot", PrimitiveType.Sphere, visual.transform, new Vector3(-0.35f, -0.37f, 0.24f), new Vector3(0f, 0f, -18f), new Vector3(0.62f, 0.38f, 0.74f), yellow);
        CreatePart("RightFoot", PrimitiveType.Sphere, visual.transform, new Vector3(0.35f, -0.37f, 0.24f), new Vector3(0f, 0f, 18f), new Vector3(0.62f, 0.38f, 0.74f), yellow);

        CreatePart("LeftEye", PrimitiveType.Sphere, visual.transform, new Vector3(-0.34f, 1.34f, 0.57f), Vector3.zero, new Vector3(0.33f, 0.42f, 0.10f), white);
        CreatePart("RightEye", PrimitiveType.Sphere, visual.transform, new Vector3(0.34f, 1.34f, 0.57f), Vector3.zero, new Vector3(0.33f, 0.42f, 0.10f), white);
        CreatePart("LeftPupil", PrimitiveType.Sphere, visual.transform, new Vector3(-0.34f, 1.34f, 0.66f), Vector3.zero, new Vector3(0.16f, 0.22f, 0.05f), black);
        CreatePart("RightPupil", PrimitiveType.Sphere, visual.transform, new Vector3(0.34f, 1.34f, 0.66f), Vector3.zero, new Vector3(0.16f, 0.22f, 0.05f), black);
        CreatePart("Mouth", PrimitiveType.Sphere, visual.transform, new Vector3(0f, 0.93f, 0.59f), Vector3.zero, new Vector3(0.48f, 0.20f, 0.06f), black);
        CreatePart("Tongue", PrimitiveType.Sphere, visual.transform, new Vector3(0f, 0.87f, 0.65f), Vector3.zero, new Vector3(0.25f, 0.07f, 0.03f), pink);

        if (npc.GetComponent<NPCCharacterAnimator>() == null)
        {
            npc.AddComponent<NPCCharacterAnimator>();
        }
    }

    private static GameObject CreatePart(
        string name,
        PrimitiveType type,
        Transform parent,
        Vector3 localPosition,
        Vector3 localEulerAngles,
        Vector3 localScale,
        Material material
    )
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localEulerAngles);
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return part;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        const string folder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Materials");

        string path = $"{folder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureLayer(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) >= 0) return;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue)) continue;
            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return;
        }
    }

    private static void ConfigureCamera()
    {
        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null) return;
        camera.transform.SetPositionAndRotation(new Vector3(0f, 20f, -20f), Quaternion.Euler(48f, 0f, 0f));
    }
}

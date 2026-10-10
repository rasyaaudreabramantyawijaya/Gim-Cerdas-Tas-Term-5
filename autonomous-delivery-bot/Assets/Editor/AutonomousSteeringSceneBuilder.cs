using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AutonomousSteeringSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Praktikum03_AutonomousSteering.unity";

    [MenuItem("Game Cerdas/Build Praktikum 03 Autonomous Steering Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureLayer("Player");
        EnsureLayer("Obstacle");

        Material ground = CreateMaterial("SteeringGround", new Color(0.18f, 0.32f, 0.25f));
        Material obstacle = CreateMaterial("SteeringObstacle", new Color(0.26f, 0.31f, 0.40f));
        Material playerMaterial = CreateMaterial("SteeringPlayer", new Color(0.08f, 0.65f, 0.82f));
        Material playerSkin = CreateMaterial("SteeringPlayerSkin", new Color(0.55f, 0.33f, 0.22f));
        Material playerHair = CreateMaterial("SteeringPlayerHair", new Color(0.13f, 0.08f, 0.04f));
        Material playerPants = CreateMaterial("SteeringPlayerPants", new Color(0.25f, 0.17f, 0.62f));
        Material botMaterial = CreateMaterial("SteeringBot", Color.green);
        Material white = CreateMaterial("SteeringWhite", Color.white);
        Material black = CreateMaterial("SteeringBlack", new Color(0.06f, 0.05f, 0.04f));

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "TrainingGround";
        plane.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
        plane.GetComponent<Renderer>().sharedMaterial = ground;

        GameObject obstacleRoot = new GameObject("ObstacleCourse");
        CreateWall("NorthBoundary", obstacleRoot.transform, new Vector3(0f, 1f, 14.8f), new Vector3(30f, 2f, 0.4f), obstacle);
        CreateWall("SouthBoundary", obstacleRoot.transform, new Vector3(0f, 1f, -14.8f), new Vector3(30f, 2f, 0.4f), obstacle);
        CreateWall("WestBoundary", obstacleRoot.transform, new Vector3(-14.8f, 1f, 0f), new Vector3(0.4f, 2f, 30f), obstacle);
        CreateWall("EastBoundary", obstacleRoot.transform, new Vector3(14.8f, 1f, 0f), new Vector3(0.4f, 2f, 30f), obstacle);
        CreateWall("CrateA", obstacleRoot.transform, new Vector3(0f, 1f, 5f), new Vector3(2.3f, 2f, 2.3f), obstacle);
        CreateWall("CrateB", obstacleRoot.transform, new Vector3(4f, 1f, -1f), new Vector3(2f, 2f, 2f), obstacle);
        CreateWall("CrateC", obstacleRoot.transform, new Vector3(-4f, 1f, 2f), new Vector3(2f, 2f, 2f), obstacle);

        GameObject uShape = new GameObject("UShapeLimitation");
        CreateWall("UBack", uShape.transform, new Vector3(7f, 1f, 8f), new Vector3(5f, 2f, 0.5f), obstacle);
        CreateWall("ULeft", uShape.transform, new Vector3(4.75f, 1f, 6f), new Vector3(0.5f, 2f, 4.5f), obstacle);
        CreateWall("URight", uShape.transform, new Vector3(9.25f, 1f, 6f), new Vector3(0.5f, 2f, 4.5f), obstacle);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(-8f, 0f, -6f);
        StylePlayerVisual(player, playerMaterial, playerSkin, playerHair, playerPants, white, black);
        player.AddComponent<SimplePlayerController>();

        GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bot.name = "DeliveryBot";
        bot.transform.position = new Vector3(0f, 0.9f, 0f);
        bot.GetComponent<Renderer>().enabled = false;
        Collider rootCollider = bot.GetComponent<Collider>();
        if (rootCollider != null) Object.DestroyImmediate(rootCollider);
        CharacterController botController = bot.AddComponent<CharacterController>();
        botController.center = Vector3.zero;
        botController.height = 1.8f;
        botController.radius = 0.48f;
        botController.stepOffset = 0.1f;
        botController.skinWidth = 0.03f;
        Renderer[] colouredParts = CreateBotVisual(bot.transform, botMaterial, white, black);

        SteeringSensor sensor = bot.AddComponent<SteeringSensor>();
        sensor.Configure(4.5f, 0.45f, LayerMask.GetMask("Obstacle"));
        SteeringAgent agent = bot.AddComponent<SteeringAgent>();
        agent.Configure(player.transform, sensor, colouredParts);

        ConfigureCamera();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = bot;
        Debug.Log("Praktikum03_AutonomousSteering dibuat. Tekan Play untuk menguji Arrive, Wander, dan SphereCast.");
    }

    private static void StylePlayerVisual(GameObject player, Material shirt, Material skin, Material hair,
        Material pants, Material white, Material dark)
    {
        Renderer capsuleRenderer = player.GetComponent<Renderer>();
        if (capsuleRenderer != null) capsuleRenderer.enabled = false;
        Collider capsuleCollider = player.GetComponent<Collider>();
        if (capsuleCollider != null) Object.DestroyImmediate(capsuleCollider);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.stepOffset = 0.1f;

        Transform visual = new GameObject("BlockCharacterVisual").transform;
        visual.SetParent(player.transform);
        visual.localPosition = Vector3.zero;
        AddPart("Torso", PrimitiveType.Cube, visual, new Vector3(0f, 0.23f, 0f), new Vector3(1f, 0.78f, 0.46f), shirt, null);
        AddPart("Head", PrimitiveType.Cube, visual, new Vector3(0f, 0.95f, 0f), new Vector3(0.82f, 0.62f, 0.56f), skin, null);
        AddPart("Hair", PrimitiveType.Cube, visual, new Vector3(0f, 1.22f, -0.01f), new Vector3(0.84f, 0.16f, 0.58f), hair, null);
        AddPart("LeftArm", PrimitiveType.Cube, visual, new Vector3(-0.68f, 0.23f, 0f), new Vector3(0.34f, 0.76f, 0.42f), skin, null);
        AddPart("RightArm", PrimitiveType.Cube, visual, new Vector3(0.68f, 0.23f, 0f), new Vector3(0.34f, 0.76f, 0.42f), skin, null);
        AddPart("LeftLeg", PrimitiveType.Cube, visual, new Vector3(-0.27f, -0.54f, 0f), new Vector3(0.42f, 0.78f, 0.44f), pants, null);
        AddPart("RightLeg", PrimitiveType.Cube, visual, new Vector3(0.27f, -0.54f, 0f), new Vector3(0.42f, 0.78f, 0.44f), pants, null);
        AddPart("LeftEye", PrimitiveType.Cube, visual, new Vector3(-0.19f, 1.02f, 0.30f), new Vector3(0.18f, 0.12f, 0.03f), white, null);
        AddPart("RightEye", PrimitiveType.Cube, visual, new Vector3(0.19f, 1.02f, 0.30f), new Vector3(0.18f, 0.12f, 0.03f), white, null);
        AddPart("Beard", PrimitiveType.Cube, visual, new Vector3(0f, 0.73f, 0.30f), new Vector3(0.38f, 0.18f, 0.04f), dark, null);
    }

    private static Renderer[] CreateBotVisual(Transform parent, Material colour, Material white, Material black)
    {
        List<Renderer> coloured = new List<Renderer>();
        Transform visual = new GameObject("CharacterVisual").transform;
        visual.SetParent(parent);
        visual.localPosition = Vector3.zero;
        AddPart("Body", PrimitiveType.Sphere, visual, new Vector3(0f, 0f, 0f), new Vector3(1.25f, 1.2f, 1f), colour, coloured);
        AddPart("Head", PrimitiveType.Sphere, visual, new Vector3(0f, 0.85f, 0f), new Vector3(1.2f, 1.1f, 1f), colour, coloured);
        AddPart("LeftArm", PrimitiveType.Sphere, visual, new Vector3(-0.68f, 0.15f, 0f), new Vector3(0.55f, 0.8f, 0.55f), colour, coloured);
        AddPart("RightArm", PrimitiveType.Sphere, visual, new Vector3(0.68f, 0.15f, 0f), new Vector3(0.55f, 0.8f, 0.55f), colour, coloured);
        AddPart("LeftFoot", PrimitiveType.Sphere, visual, new Vector3(-0.32f, -0.55f, 0.14f), new Vector3(0.55f, 0.35f, 0.65f), colour, coloured);
        AddPart("RightFoot", PrimitiveType.Sphere, visual, new Vector3(0.32f, -0.55f, 0.14f), new Vector3(0.55f, 0.35f, 0.65f), colour, coloured);
        AddPart("LeftEye", PrimitiveType.Sphere, visual, new Vector3(-0.28f, 1.02f, 0.48f), new Vector3(0.25f, 0.32f, 0.08f), white, null);
        AddPart("RightEye", PrimitiveType.Sphere, visual, new Vector3(0.28f, 1.02f, 0.48f), new Vector3(0.25f, 0.32f, 0.08f), white, null);
        AddPart("Mouth", PrimitiveType.Sphere, visual, new Vector3(0f, 0.66f, 0.5f), new Vector3(0.38f, 0.14f, 0.05f), black, null);
        return coloured.ToArray();
    }

    private static void AddPart(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, List<Renderer> coloured)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        if (coloured != null) coloured.Add(renderer);
        Object.DestroyImmediate(part.GetComponent<Collider>());
    }

    private static void CreateWall(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.layer = LayerMask.NameToLayer("Obstacle");
        wall.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Material CreateMaterial(string name, Color colour)
    {
        const string folder = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Materials");
        string path = $"{folder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = colour;
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
        camera.transform.SetPositionAndRotation(new Vector3(0f, 20f, -22f), Quaternion.Euler(50f, 0f, 0f));
    }
}

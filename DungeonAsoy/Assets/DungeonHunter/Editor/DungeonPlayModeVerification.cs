using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class DungeonPlayModeVerification
{
    private const string Key = "DungeonHunter.PlayChecks";
    private static double start;
    private static int stage;
    private static List<string> results;
    private static DungeonPlayerController player;
    private static DungeonSession session;
    private static DungeonHealth health;
    private static float beforeArrow;
    private static DungeonEnemyBrain archer;
    private static Keyboard verificationKeyboard;
    private static bool captured;
    private static bool movementChecked;
    private static Vector3 movementStart;
    private static string firstDialogue;
    private static InputSettings originalInputSettings;
    private static InputSettings.BackgroundBehavior previousBackgroundBehavior;
    private static InputSettings.EditorInputBehaviorInPlayMode previousFocusBehavior;
    private static Keyboard originalKeyboard;
    private static bool originalBackground;
    private static int initialFrames;

    [InitializeOnLoadMethod]
    private static void Restore()
    {
        if (!EditorApplication.isPlaying) EnsureLiveSettings();
        EditorApplication.playModeStateChanged -= OnMode;
        EditorApplication.playModeStateChanged += OnMode;
        if (SessionState.GetBool(Key, false) && EditorApplication.isPlaying) Begin();
    }

    [MenuItem("Dungeon Hunter/Run Open Estate Play Mode Checks")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode first."); return; }
        EnsureLiveSettings();
        Directory.CreateDirectory("docs/evidence");
        SessionState.SetBool(Key, true);
        EditorApplication.isPaused = false;
        EditorApplication.playModeStateChanged -= OnMode;
        EditorApplication.playModeStateChanged += OnMode;
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.isPlaying = true;
    }

    private static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
        {
            try { Begin(); }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                File.WriteAllText("docs/evidence/estate-playmode-checks.txt", "FAIL verifier setup: " + exception);
                SessionState.SetBool(Key, false);
                RestoreInput();
                EditorApplication.isPlaying = false;
            }
        }
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= Tick;
            RestoreInput();
        }
        if (state == PlayModeStateChange.EnteredEditMode) EnsureLiveSettings();
    }

    private static void Begin()
    {
        start = EditorApplication.timeSinceStartup;
        stage = 0;
        results = new List<string>();
        player = UnityEngine.Object.FindFirstObjectByType<DungeonPlayerController>();
        session = UnityEngine.Object.FindFirstObjectByType<DungeonSession>();
        health = player.GetComponent<DungeonHealth>();
        captured = false;
        movementChecked = false;
        originalBackground = Application.runInBackground;
        Application.runInBackground = true;
        originalKeyboard = Keyboard.current;
        EnsureLiveSettings();
        originalInputSettings = InputSystem.settings;
        previousBackgroundBehavior = originalInputSettings.backgroundBehavior;
        previousFocusBehavior = originalInputSettings.editorInputBehaviorInPlayMode;
        originalInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        originalInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        verificationKeyboard = InputSystem.AddDevice<Keyboard>();
        verificationKeyboard.MakeCurrent();
        initialFrames = Time.frameCount;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Check(bool pass, string label) => results.Add((pass ? "PASS " : "FAIL ") + label);

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) return;
        double elapsed = EditorApplication.timeSinceStartup - start;
        try
        {
            if (stage == 0 && elapsed > 2 && !captured)
            {
                ScreenCapture.CaptureScreenshot("docs/evidence/estate-playmode-start.png");
                CaptureCamera("docs/evidence/estate-camera.png");
                var camera = Camera.main;
                Vector3 cameraPosition = camera.transform.position;
                Quaternion cameraRotation = camera.transform.rotation;
                camera.transform.position = new Vector3(47, 43, -48);
                camera.transform.LookAt(new Vector3(0, 0, 22));
                CaptureCamera("docs/evidence/estate-overview.png");
                camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
                captured = true;
            }
            if (stage == 0 && elapsed > 3)
            {
                results.Add("INFO initial: paused=" + EditorApplication.isPaused + " scale=" + Time.timeScale + " frames=" + (Time.frameCount - initialFrames) + " focus=" + Application.isFocused + " player=" + player.transform.position + " controller=" + player.enabled + " complete=" + session.IsComplete + " dialogue=" + session.IsDialogueOpen + " keyboard=" + (Keyboard.current != null));
                Check(player.transform.position.y > -0.2f && player.transform.position.y < 0.8f && player.GetComponent<CharacterController>().isGrounded, "player remains grounded after three seconds of actual gravity");
                Check(UnityEngine.Object.FindFirstObjectByType<TerrainCollider>() != null, "terrain collider exists");
                foreach (var brain in UnityEngine.Object.FindObjectsByType<DungeonEnemyBrain>(FindObjectsSortMode.None)) brain.enabled = false;
                health.TakeDamage(20);
                Check(GameObject.Find("Health Text").GetComponent<Text>().text.Contains("80"), "health event updates visible HUD at runtime");
                UnityEngine.Object.FindFirstObjectByType<DungeonNpcDialogue>().Interact(session);
                firstDialogue = session.DialogueText;
                Check(session.IsDialogueOpen && Time.timeScale == 0 && GameObject.Find("Dialogue Panel") != null, "NPC opens visible dialogue and pauses gameplay");
                session.SetDialogueOpen(false);
                Check(Time.timeScale > 0 && !session.IsDialogueOpen, "closing dialogue restores time");
                var checkpoint = UnityEngine.Object.FindFirstObjectByType<DungeonCheckpoint>();
                checkpoint.Interact(session);
                health.TakeDamage(200);
                session.RestartRun();
                var respawn = player.GetComponent<DungeonPlayerRespawn>();
                Check(checkpoint.Activated && !health.IsDead && Vector3.Distance(player.transform.position, respawn.SpawnPosition) < 0.1f, "checkpoint activation restores health and death restart respawns there");
                player.enabled = false;
                player.Teleport(new Vector3(8, 0.4f, 18));
                player.GetComponent<CharacterController>().Move(Vector3.forward * 8);
                Check(player.transform.position.z < 20.8f, "CharacterController cannot cross mansion front wall");
                player.Teleport(new Vector3(0, 0.4f, 8));
                player.enabled = true;
                beforeArrow = health.Current;
                archer = GameObject.Find("Courtyard Archer").GetComponent<DungeonEnemyBrain>();
                archer.enabled = true;
                Check(archer.UsesBow, "courtyard enemy uses ranged mode");
                movementStart = player.transform.position;
                InputSystem.QueueStateEvent(verificationKeyboard, new KeyboardState(UnityEngine.InputSystem.Key.W));
                stage = 1;
                start = EditorApplication.timeSinceStartup;
            }
            else if (stage == 1 && elapsed > 0.65 && !movementChecked)
            {
                results.Add("INFO input: paused=" + EditorApplication.isPaused + " scale=" + Time.timeScale + " speed=" + player.PlanarSpeed + " keyboardCurrent=" + (Keyboard.current == verificationKeyboard) + " W=" + (Keyboard.current != null && Keyboard.current.wKey.isPressed));
                Check(player.PlanarSpeed > 0 && Vector3.Distance(player.transform.position, movementStart) > 1, "W input moves player across terrain");
                var leg = player.transform.Find("Original Anime-inspired Prototype Visual/Left Leg Swing");
                Check(Quaternion.Angle(leg.localRotation, Quaternion.identity) > 0.1f, "walking input drives leg swing animation");
                InputSystem.QueueStateEvent(verificationKeyboard, new KeyboardState());
                movementChecked = true;
            }
            else if (stage == 1 && elapsed > 4)
            {
                Check(health.Current < beforeArrow, "archer fires projectile and damages stationary player");
                archer.enabled = false;
                foreach (var arrow in UnityEngine.Object.FindObjectsByType<DungeonArrow>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(arrow.gameObject);
                player.Teleport(new Vector3(8, 0.4f, 24));
                beforeArrow = health.Current;
                var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/DungeonHunter/Materials/Aged Brass.mat");
                DungeonArrow.Launch(new Vector3(8, 1.2f, 18), Vector3.forward, archer.transform, health, 12, material);
                stage = 2;
                start = EditorApplication.timeSinceStartup;
            }
            else if (stage == 2 && elapsed > 1)
            {
                Check(health.Current == beforeArrow, "arrow stops at mansion wall instead of damaging player behind it");
                var gate = UnityEngine.Object.FindFirstObjectByType<DungeonExitDoor>();
                gate.Interact(session);
                Check(!gate.IsOpen && !session.IsComplete, "gate stays locked before artifact");
                var pickup = UnityEngine.Object.FindFirstObjectByType<DungeonPickup>();
                pickup.Interact(session);
                pickup.Interact(session);
                Check(session.HasArtifact && !pickup.GetComponentInChildren<Renderer>().enabled, "artifact pickup is idempotent and hides visual");
                UnityEngine.Object.FindFirstObjectByType<DungeonNpcDialogue>().Interact(session);
                Check(session.IsDialogueOpen && session.DialogueText != firstDialogue, "NPC dialogue changes after collecting the artifact");
                session.SetDialogueOpen(false);
                gate.Interact(session);
                Check(gate.IsOpen && !session.IsComplete, "artifact unlocks gate without prematurely ending the run");
                player.Teleport(new Vector3(0, 0.4f, 55));
                stage = 3;
                start = EditorApplication.timeSinceStartup;
            }
            else if (stage == 3 && elapsed > 1)
            {
                Check(session.IsComplete, "crossing the unlocked exit completes run");
                File.WriteAllLines("docs/evidence/estate-playmode-checks.txt", results);
                Debug.Log("Dungeon estate Play Mode checks:\n" + string.Join("\n", results));
                SessionState.SetBool(Key, false);
                EditorApplication.update -= Tick;
                RestoreInput();
                EditorApplication.isPlaying = false;
            }
        }
        catch (Exception exception)
        {
            results.Add("FAIL exception: " + exception);
            File.WriteAllLines("docs/evidence/estate-playmode-checks.txt", results);
            Debug.LogException(exception);
            SessionState.SetBool(Key, false);
            EditorApplication.update -= Tick;
            RestoreInput();
            Time.timeScale = 1;
            EditorApplication.isPlaying = false;
        }
    }

    private static void RestoreInput()
    {
        if (verificationKeyboard != null && verificationKeyboard.added) InputSystem.RemoveDevice(verificationKeyboard);
        verificationKeyboard = null;
        if (originalKeyboard != null && originalKeyboard.added) originalKeyboard.MakeCurrent();
        originalKeyboard = null;
        if (originalInputSettings != null)
        {
            originalInputSettings.backgroundBehavior = previousBackgroundBehavior;
            originalInputSettings.editorInputBehaviorInPlayMode = previousFocusBehavior;
            originalInputSettings = null;
            Application.runInBackground = originalBackground;
        }
    }

    private static void EnsureLiveSettings()
    {
        if (InputSystem.settings != null) return;
        if (EditorBuildSettings.TryGetConfigObject("com.unity.input.settings", out InputSettings asset) && asset != null)
            InputSystem.settings = asset;
        else
        {
            var settings = ScriptableObject.CreateInstance<InputSettings>();
            settings.hideFlags = HideFlags.HideAndDontSave;
            InputSystem.settings = settings;
        }
        Debug.Log("Dungeon verifier recovered the configured/default InputSettings native object.");
    }

    private static void CaptureCamera(string path)
    {
        var camera = Camera.main;
        var texture = new RenderTexture(1280, 720, 24);
        var old = camera.targetTexture;
        var previous = RenderTexture.active;
        var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = texture };
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
        RenderTexture.active = texture;
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        pixels.Apply();
        File.WriteAllBytes(path, pixels.EncodeToPNG());
        camera.targetTexture = old;
        RenderTexture.active = previous;
        UnityEngine.Object.Destroy(pixels);
        texture.Release();
        UnityEngine.Object.Destroy(texture);
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

public static class DungeonSceneVerification
{
    [InitializeOnLoadMethod]
    private static void Schedule() => EditorApplication.delayCall += CheckRequest;

    private static void CheckRequest()
    {
        const string request = "Temp/DungeonHunterGroundCheck.request";
        if (!File.Exists(request)) return;
        File.Delete(request);
        CheckGround();
    }

    [MenuItem("Dungeon Hunter/Verify Ground Collision")]
    public static void CheckGround()
    {
        var player = Object.FindFirstObjectByType<DungeonPlayerController>();
        Physics.SyncTransforms();
        bool exists = player != null;
        bool grounded = exists && Physics.Raycast(player.transform.position + Vector3.up * 2f,
            Vector3.down, out _, 10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        string result = grounded ? "PASS" : "FAIL";
        Directory.CreateDirectory("docs/evidence");
        File.AppendAllText("docs/evidence/ground-regression.txt", result + " ground raycast below player\n");
        Debug.Log("Dungeon ground regression: " + result);
    }
}

using UnityEngine;

[RequireComponent(typeof(DungeonPlayerController), typeof(DungeonHealth))]
public sealed class DungeonPlayerRespawn : MonoBehaviour
{
    private Vector3 spawn;
    public string CheckpointName { get; private set; } = "Camp entrance";
    public Vector3 SpawnPosition => spawn;

    private void Awake() => spawn = transform.position;

    public void SetCheckpoint(Vector3 position, string name)
    {
        spawn = position;
        CheckpointName = name;
    }

    public void Respawn()
    {
        GetComponent<DungeonPlayerController>().Teleport(spawn);
        var health = GetComponent<DungeonHealth>();
        health.Configure(health.Maximum);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

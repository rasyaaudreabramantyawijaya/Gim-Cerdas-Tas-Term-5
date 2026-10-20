using UnityEngine;

public sealed class DungeonCheckpoint : DungeonInteractable
{
    [SerializeField] private string checkpointName = "Watchtower";
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Renderer beacon;
    public bool Activated { get; private set; }

    public void Configure(string name, Vector3 spawn, Renderer lamp)
    {
        checkpointName = name;
        spawnPosition = spawn;
        beacon = lamp;
    }

    public override string GetPrompt(DungeonSession session) => "E  " + (Activated ? "Pulihkan HP di " : "Aktifkan checkpoint ") + checkpointName;

    public override void Interact(DungeonSession session)
    {
        var player = FindFirstObjectByType<DungeonPlayerRespawn>();
        if (player == null || session == null) return;
        player.SetCheckpoint(spawnPosition, checkpointName);
        var health = player.GetComponent<DungeonHealth>();
        health.Configure(health.Maximum);
        Activated = true;
        if (beacon != null) beacon.material.color = new Color(0.2f, 1f, 0.75f);
        session.SetFeedback("Checkpoint aktif: " + checkpointName);
    }
}

using UnityEngine;

public sealed class DungeonPickup : DungeonInteractable
{
    [SerializeField] private string itemName = "Ancient Artifact";
    private bool collected;
    public override string GetPrompt(DungeonSession session) => collected ? string.Empty : $"E  Ambil {itemName}";

    public override void Interact(DungeonSession session)
    {
        if (collected || session == null || !session.CollectArtifact()) return;
        collected = true;
        foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
    }
}

using UnityEngine;

public abstract class DungeonInteractable : MonoBehaviour
{
    public abstract string GetPrompt(DungeonSession session);
    public abstract void Interact(DungeonSession session);
}

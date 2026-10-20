using UnityEngine;
using UnityEngine.InputSystem;

public sealed class DungeonPlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2.5f;
    public DungeonInteractable CurrentTarget { get; private set; }
    public string CurrentPrompt { get; private set; }
    [SerializeField] private DungeonSession session;
    [SerializeField] private DungeonHealth health;

    public void Configure(DungeonSession gameSession, DungeonHealth playerHealth)
    {
        session = gameSession;
        health = playerHealth;
    }

    private void Update()
    {
        CurrentTarget = null;
        CurrentPrompt = string.Empty;
        if (session == null || health == null || health.IsDead || session.IsDialogueOpen || session.IsComplete) return;
        var candidates = Physics.OverlapSphere(transform.position + Vector3.up, interactionDistance,
            ~0, QueryTriggerInteraction.Collide);
        float closest = float.MaxValue;
        foreach (var candidate in candidates)
        {
            var interactable = candidate.GetComponentInParent<DungeonInteractable>();
            if (interactable == null) continue;
            Vector3 point = candidate.ClosestPoint(transform.position + Vector3.up);
            float distance = Vector3.Distance(transform.position + Vector3.up, point);
            if (distance > interactionDistance || distance >= closest || !HasLineOfSight(interactable, point)) continue;
            closest = distance;
            CurrentTarget = interactable;
        }
        if (CurrentTarget != null)
        {
            CurrentPrompt = CurrentTarget.GetPrompt(session);
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                CurrentTarget.Interact(session);
        }
    }

    private bool HasLineOfSight(DungeonInteractable target, Vector3 point)
    {
        Vector3 origin = transform.position + Vector3.up * 1.25f;
        Vector3 delta = point - origin;
        if (delta.sqrMagnitude < 0.01f) return true;
        var hits = Physics.RaycastAll(origin, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponentInParent<DungeonGeometry>() != null) return false;
            if (hit.collider.GetComponentInParent<DungeonInteractable>() == target) return true;
        }
        return true;
    }
}

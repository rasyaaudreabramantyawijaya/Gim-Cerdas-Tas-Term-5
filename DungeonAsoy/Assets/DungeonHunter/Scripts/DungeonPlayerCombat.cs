using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class DungeonPlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float cooldown = 0.65f;
    [SerializeField] private float attackArc = 0.15f;
    [SerializeField] private DungeonHealth health;
    [SerializeField] private DungeonSession session;
    [SerializeField] private DungeonPlayerController controller;
    private float readyAt;
    private readonly HashSet<DungeonHealth> struckThisSwing = new HashSet<DungeonHealth>();

    public void Configure(DungeonHealth playerHealth, DungeonSession gameSession)
    {
        health = playerHealth;
        session = gameSession;
        controller = GetComponent<DungeonPlayerController>();
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (TryAttack())
        {
            controller?.PulseAttack();
            struckThisSwing.Clear();
            foreach (var candidate in Physics.OverlapSphere(transform.position + Vector3.up, attackRange,
                         ~0, QueryTriggerInteraction.Ignore))
            {
                var target = candidate.GetComponentInParent<DungeonHealth>();
                if (target == null || target == health || target.IsDead || struckThisSwing.Contains(target)) continue;
                Vector3 targetPoint = target.transform.position + Vector3.up;
                Vector3 direction = Vector3.ProjectOnPlane(targetPoint - transform.position, Vector3.up).normalized;
                if (Vector3.Dot(transform.forward, direction) < attackArc) continue;
                if (BlockedByWall(targetPoint, target.transform)) continue;
                struckThisSwing.Add(target);
                target.TakeDamage(damage);
            }
        }
    }

    public bool TryAttack()
    {
        if (health == null || session == null || health.IsDead || session.IsDialogueOpen || session.IsComplete || Time.time < readyAt)
            return false;
        readyAt = Time.time + cooldown;
        return true;
    }

    private bool BlockedByWall(Vector3 point, Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * 1.15f;
        Vector3 delta = point - origin;
        var hits = Physics.RaycastAll(origin, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(target) || hit.collider.transform == target) return false;
            if (hit.collider.GetComponentInParent<DungeonGeometry>() != null) return true;
        }
        return false;
    }
}

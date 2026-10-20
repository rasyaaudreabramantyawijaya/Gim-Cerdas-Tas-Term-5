using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class DungeonEnemyBrain : MonoBehaviour
{
    public enum BrainState { Idle, Chase, MaintainDistance, Attack, Dead }
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackWindup = 0.45f;
    public BrainState State { get; private set; } = BrainState.Idle;
    [SerializeField] private Transform target;
    [SerializeField] private DungeonHealth targetHealth;
    [SerializeField] private DungeonSession session;
    private DungeonHealth ownHealth;
    private CharacterController characterController;
    private float nextAttack;
    private float attackAt;
    private bool windingUp;
    private float verticalSpeed;
    [SerializeField] private bool useBow;
    [SerializeField] private float shootingRange = 15f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private Material arrowMaterial;
    public bool UsesBow => useBow;

    public void Configure(Transform player, DungeonHealth playerHealth, DungeonSession gameSession, bool ranged = false, Material projectileMaterial = null)
    {
        target = player;
        targetHealth = playerHealth;
        session = gameSession;
        useBow = ranged;
        arrowMaterial = projectileMaterial;
        if (ranged) { detectionRange = 23f; attackCooldown = 2.1f; attackWindup = 0.7f; }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        ownHealth = GetComponent<DungeonHealth>();
        if (ownHealth != null) ownHealth.Died += OnDeath;
    }

    private void Update()
    {
        if (ownHealth == null || ownHealth.IsDead || target == null || targetHealth == null || targetHealth.IsDead || session == null || session.IsDialogueOpen || session.IsComplete)
        {
            if (ownHealth != null && ownHealth.IsDead) State = BrainState.Dead;
            return;
        }
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        if (distance > detectionRange || !HasLineOfSight())
        {
            State = BrainState.Idle;
            windingUp = false;
            ApplyGravity();
            return;
        }
        if (useBow)
        {
            UpdateArcher(toTarget, distance);
            return;
        }
        if (distance > attackRange)
        {
            State = BrainState.Chase;
            windingUp = false;
            Vector3 direction = toTarget.normalized;
            if (Physics.SphereCast(transform.position + Vector3.up * 0.7f, 0.32f, direction, out RaycastHit hit, 0.85f)
                && hit.collider.GetComponentInParent<DungeonGeometry>() != null)
                direction = Quaternion.Euler(0f, hit.normal.y >= 0.5f ? 70f : -70f, 0f) * direction;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 360f * Time.deltaTime);
            Vector3 step = direction * moveSpeed;
            verticalSpeed = characterController.isGrounded ? -2f : verticalSpeed - 22f * Time.deltaTime;
            characterController.Move((step + Vector3.up * verticalSpeed) * Time.deltaTime);
            return;
        }
        State = BrainState.Attack;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget.normalized), 420f * Time.deltaTime);
        ApplyGravity();
        if (!windingUp && Time.time >= nextAttack)
        {
            windingUp = true;
            attackAt = Time.time + attackWindup;
        }
        if (windingUp && Time.time >= attackAt)
        {
            windingUp = false;
            nextAttack = Time.time + attackCooldown;
            if (HasLineOfSight() && Vector3.Distance(transform.position, target.position) <= attackRange + 0.2f)
                targetHealth.TakeDamage(attackDamage);
        }
    }

    private void UpdateArcher(Vector3 toTarget, float distance)
    {
        Vector3 direction = toTarget.normalized;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 300f * Time.deltaTime);
        float move = distance > shootingRange ? 1f : distance < preferredDistance - 2f ? -1f : 0f;
        if (move != 0f)
        {
            State = move > 0f ? BrainState.Chase : BrainState.MaintainDistance;
            characterController.Move(direction * (move * moveSpeed * Time.deltaTime));
        }
        else State = BrainState.Attack;
        ApplyGravity();
        if (distance > shootingRange) { windingUp = false; return; }
        if (!windingUp && Time.time >= nextAttack)
        {
            windingUp = true;
            attackAt = Time.time + attackWindup;
        }
        if (windingUp && Time.time >= attackAt)
        {
            windingUp = false;
            nextAttack = Time.time + attackCooldown;
            Vector3 origin = transform.position + Vector3.up * 1.25f + transform.forward * 0.65f;
            Vector3 aim = target.position + Vector3.up * 0.95f;
            if (HasLineOfSight()) DungeonArrow.Launch(origin, aim - origin, transform, targetHealth, attackDamage, arrowMaterial);
        }
    }

    private void ApplyGravity()
    {
        if (characterController == null) return;
        verticalSpeed = characterController.isGrounded ? -2f : verticalSpeed - 22f * Time.deltaTime;
        characterController.Move(Vector3.up * verticalSpeed * Time.deltaTime);
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.1f;
        Vector3 end = target.position + Vector3.up * 1.1f;
        Vector3 delta = end - origin;
        foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform.IsChildOf(target)) continue;
            if (hit.collider.GetComponentInParent<DungeonGeometry>() != null) return false;
        }
        return true;
    }

    private void OnDeath()
    {
        State = BrainState.Dead;
        characterController.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
        foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.material.color *= 0.55f;
    }
}

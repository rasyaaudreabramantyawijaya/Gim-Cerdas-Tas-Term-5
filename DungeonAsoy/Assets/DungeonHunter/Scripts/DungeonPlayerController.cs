using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class DungeonPlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float gravity = -22f;
    public float PlanarSpeed { get; private set; }
    public Vector3 Velocity { get; private set; }
    public bool IsRunning { get; private set; }
    public float AttackPulse { get; private set; }
    private CharacterController controller;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private DungeonSession session;
    private float verticalSpeed;

    public void Configure(Transform cameraPivot, DungeonSession gameSession)
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = cameraPivot;
        session = gameSession;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (transform.position.y < -15f)
        {
            var respawn = GetComponent<DungeonPlayerRespawn>();
            if (respawn != null) respawn.Respawn();
            return;
        }
        if (session == null || session.IsDialogueOpen || session.IsComplete || GetComponent<DungeonHealth>().IsDead)
        {
            PlanarSpeed = 0f;
            return;
        }
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !session.IsDialogueOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Vector2 input = Vector2.zero;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
        }
        input = Vector2.ClampMagnitude(input, 1f);
        IsRunning = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        Vector3 forward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : transform.forward;
        Vector3 right = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : transform.right;
        Vector3 desired = forward * input.y + right * input.x;
        PlanarSpeed = input.sqrMagnitude > 0f ? (IsRunning ? runSpeed : walkSpeed) : 0f;
        Vector3 planar = desired * PlanarSpeed;
        if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
        verticalSpeed += gravity * Time.deltaTime;
        Velocity = planar + Vector3.up * verticalSpeed;
        controller.Move(Velocity * Time.deltaTime);
        if (desired.sqrMagnitude > 0.001f)
        {
            var target = Quaternion.LookRotation(desired, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
        AttackPulse = Mathf.MoveTowards(AttackPulse, 0f, Time.deltaTime * 2.8f);
    }

    public void PulseAttack() => AttackPulse = 1f;

    public void Teleport(Vector3 position)
    {
        controller = GetComponent<CharacterController>();
        controller.enabled = false;
        transform.position = position;
        verticalSpeed = 0f;
        Velocity = Vector3.zero;
        PlanarSpeed = 0f;
        controller.enabled = true;
        Physics.SyncTransforms();
    }
}

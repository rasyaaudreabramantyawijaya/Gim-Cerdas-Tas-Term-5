using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ThirdPersonCameraRig : MonoBehaviour
{
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float sensitivity = 0.12f;
    [SerializeField] private float minimumPitch = -18f;
    [SerializeField] private float maximumPitch = 62f;
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask geometryMask;
    private float yaw;
    private float pitch = 14f;

    public void Configure(Transform followTarget, LayerMask collisionMask)
    {
        target = followTarget;
        geometryMask = collisionMask;
        if (target != null) yaw = target.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minimumPitch, maximumPitch);
        }
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * 1.5f;
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        float safeDistance = distance;
        foreach (var hit in Physics.SphereCastAll(focus, 0.22f, offset.normalized, distance, geometryMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<DungeonGeometry>() == null) continue;
            safeDistance = Mathf.Min(safeDistance, Mathf.Max(0.55f, hit.distance - 0.12f));
        }
        transform.position = focus + offset.normalized * safeDistance;
        transform.rotation = rotation;
    }
}

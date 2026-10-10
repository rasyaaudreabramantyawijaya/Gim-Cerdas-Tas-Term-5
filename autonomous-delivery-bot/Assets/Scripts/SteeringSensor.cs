using UnityEngine;

public class SteeringSensor : MonoBehaviour
{
    [Header("SphereCast Sensor")]
    [SerializeField] private float sensorDistance = 4.5f;
    [SerializeField] private float sensorRadius = 0.45f;
    [SerializeField] private float sensorHeight = 0.65f;
    [SerializeField] private LayerMask obstacleMask;

    private Vector3 lastOrigin;
    private Vector3 lastEndPoint;
    private RaycastHit lastHit;

    public bool IsAvoiding { get; private set; }
    public RaycastHit LastHit => lastHit;

    public void Configure(float distance, float radius, LayerMask obstacles)
    {
        sensorDistance = distance;
        sensorRadius = radius;
        obstacleMask = obstacles;
    }

    public bool TryGetAvoidance(out Vector3 avoidanceDirection)
    {
        lastOrigin = transform.position + Vector3.up * sensorHeight;
        lastEndPoint = lastOrigin + transform.forward * sensorDistance;

        if (!Physics.SphereCast(lastOrigin, sensorRadius, transform.forward, out lastHit,
                sensorDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            IsAvoiding = false;
            avoidanceDirection = Vector3.zero;
            return false;
        }

        Vector3 normal = Vector3.ProjectOnPlane(lastHit.normal, Vector3.up).normalized;
        Vector3 lateral = Vector3.Cross(Vector3.up, transform.forward).normalized;
        if (Vector3.Dot(normal, lateral) < 0f) lateral = -lateral;

        avoidanceDirection = (normal + lateral).normalized;
        IsAvoiding = true;
        return true;
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? lastOrigin : transform.position + Vector3.up * sensorHeight;
        Vector3 direction = transform.forward;
        float distance = Mathf.Max(0f, sensorDistance);
        Vector3 endPoint = Application.isPlaying ? lastEndPoint : origin + direction * distance;

        Gizmos.color = IsAvoiding ? Color.red : Color.yellow;
        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawWireSphere(origin, sensorRadius);
        Gizmos.DrawWireSphere(endPoint, sensorRadius);

        if (IsAvoiding)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastHit.point, 0.1f);
            Gizmos.DrawLine(origin, lastHit.point);
        }
    }
}

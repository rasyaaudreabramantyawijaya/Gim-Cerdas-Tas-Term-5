using UnityEngine;

public class NPCSensor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 8f;
    [Range(0f, 360f)] [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Eye Settings")]
    [SerializeField] private float eyeHeight = 1.2f;

    public bool CanSeePlayer { get; private set; }
    public Transform Player => player;

    public void Configure(Transform target, float radius, float angle, LayerMask obstacles)
    {
        player = target;
        viewRadius = radius;
        viewAngle = angle;
        obstacleMask = obstacles;
    }

    private void Update() => DetectPlayer();

    private void DetectPlayer()
    {
        CanSeePlayer = false;
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        if (directionToPlayer.magnitude > viewRadius) return;

        Vector3 flatDirection = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;
        if (Vector3.Angle(transform.forward, flatDirection) > viewAngle * 0.5f) return;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = player.position + Vector3.up * eyeHeight;
        Vector3 rayDirection = (targetPosition - eyePosition).normalized;
        float rayDistance = Vector3.Distance(eyePosition, targetPosition);
        if (Physics.Raycast(eyePosition, rayDirection, rayDistance, obstacleMask)) return;

        CanSeePlayer = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + DirectionFromAngle(-viewAngle * 0.5f) * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + DirectionFromAngle(viewAngle * 0.5f) * viewRadius);

        if (player != null && CanSeePlayer)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * eyeHeight, player.position + Vector3.up * eyeHeight);
        }
    }

    private Vector3 DirectionFromAngle(float angle)
    {
        float finalAngle = transform.eulerAngles.y + angle;
        return new Vector3(Mathf.Sin(finalAngle * Mathf.Deg2Rad), 0f, Mathf.Cos(finalAngle * Mathf.Deg2Rad));
    }
}

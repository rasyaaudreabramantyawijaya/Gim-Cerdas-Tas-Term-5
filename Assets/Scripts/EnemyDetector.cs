using UnityEngine;
 
public class EnemyDetector : MonoBehaviour
{
    public enum EnemyState { Idle, Suspicious, Alert }
 
    [Header("Target")]
    [SerializeField] private Transform player;
 
    [Header("AI Parameters")]
    [SerializeField][Min(0f)] private float suspiciousRadius = 8f;
    [SerializeField][Min(0f)] private float alertRadius = 4f;
    [SerializeField] private float chaseSpeed = 2f; // belum dipakai di praktikum 1, disiapkan untuk lanjutan
 
    [Header("Visual")]
    [SerializeField] private Color idleColor = Color.blue;
    [SerializeField] private Color suspiciousColor = Color.yellow;
    [SerializeField] private Color alertColor = Color.red;
 
    [Header("Debug")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    [SerializeField] private float currentDistance;
 
    private Renderer rend;
 
    void Awake()
    {
        rend = GetComponent<Renderer>();
    }
 
    void Update()
    {
        if (player == null) return;
 
        // 1) PERCEPTION -> baca informasi lingkungan: jarak ke player
        currentDistance = Vector3.Distance(transform.position, player.position);
 
        // 2) DECISION -> bandingkan jarak dengan parameter deteksi
        if (currentDistance <= alertRadius)
        {
            currentState = EnemyState.Alert;
        }
        else if (currentDistance <= suspiciousRadius)
        {
            currentState = EnemyState.Suspicious;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
 
        // 3) ACTION -> respon hasil keputusan (di sini: ubah warna)
        ApplyVisual();
    }
 
    private void ApplyVisual()
    {
        if (rend == null) return;
 
        switch (currentState)
        {
            case EnemyState.Alert:
                rend.material.color = alertColor;
                break;
            case EnemyState.Suspicious:
                rend.material.color = suspiciousColor;
                break;
            default:
                rend.material.color = idleColor;
                break;
        }
    }
 
    // Debug visual di Scene view: lingkaran radius deteksi
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, suspiciousRadius);
 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}

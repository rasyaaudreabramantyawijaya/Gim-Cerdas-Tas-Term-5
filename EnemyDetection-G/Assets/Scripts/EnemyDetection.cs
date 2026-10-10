using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public enum EnemyState
    {
        Detection,
        Alert,
        Outside
    }

    [Header("Target")]
    public Transform player;

    [Header("Detection Area")]
    public float detectionRange = 5f;

    [Header("Alert Area")]
    public float alertRange = 10f;

    [Header("Enemy Colors")]
    public Color detectionColor = Color.red;
    public Color alertColor = Color.yellow;
    public Color outsideColor = Color.green;

    private Renderer enemyRenderer;

    public EnemyState currentState;

    void Start()
    {
        enemyRenderer = GetComponent<Renderer>();

        ChangeState(EnemyState.Outside);
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= detectionRange)
        {
            ChangeState(EnemyState.Detection);
        }
        else if (distance <= alertRange)
        {
            ChangeState(EnemyState.Alert);
        }
        else
        {
            ChangeState(EnemyState.Outside);
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Detection:
                SetColor(detectionColor);
                Debug.Log("ENEMY: DETECTION");
                break;

            case EnemyState.Alert:
                SetColor(alertColor);
                Debug.Log("ENEMY: ALERT");
                break;

            case EnemyState.Outside:
                SetColor(outsideColor);
                Debug.Log("ENEMY: OUTSIDE");
                break;
        }
    }

    void SetColor(Color color)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = color;
        }
    }

    private void OnDrawGizmos()
   {
    // Detection Area
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(
        transform.position,
        detectionRange
    );

    // Alert Area
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(
        transform.position,
        alertRange
    );
    
    }
}
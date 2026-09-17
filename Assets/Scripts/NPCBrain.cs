using UnityEngine;
using UnityEngine.AI;

public class NPCBrain : MonoBehaviour
{
    public enum NPCState { Patrol, Chase, Search }

    [Header("References")]
    [SerializeField] private NPCSensor sensor;
    [SerializeField] private NavMeshAgent agent;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 0.7f;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Search Settings")]
    [SerializeField] private float searchDuration = 4f;
    [SerializeField] private float searchTolerance = 0.8f;

    [Header("Debug")]
    [SerializeField] private NPCState currentState = NPCState.Patrol;

    private NPCState previousState;
    private int patrolIndex;
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition;
    private float searchTimer;
    private bool arrivedAtSearchPosition;

    public void Configure(NPCSensor npcSensor, NavMeshAgent navAgent, Transform[] points)
    {
        sensor = npcSensor;
        agent = navAgent;
        patrolPoints = points;
    }

    private void Start()
    {
        previousState = currentState;
        ApplyCharacterVisualLayout();
        if (GetComponent<NPCCharacterAnimator>() == null)
        {
            gameObject.AddComponent<NPCCharacterAnimator>();
        }
        GoToCurrentPatrolPoint();
    }

    private void OnValidate()
    {
        ApplyCharacterVisualLayout();
    }

    private void Update()
    {
        if (sensor == null || agent == null) return;
        UpdateMemory();
        MakeDecision();
        ExecuteCurrentState();
    }

    private void UpdateMemory()
    {
        if (!sensor.CanSeePlayer || sensor.Player == null) return;
        lastKnownPosition = sensor.Player.position;
        hasLastKnownPosition = true;
    }

    private void MakeDecision()
    {
        if (sensor.CanSeePlayer)
        {
            ChangeState(NPCState.Chase);
            return;
        }

        if (currentState == NPCState.Chase && hasLastKnownPosition)
        {
            ChangeState(NPCState.Search);
            return;
        }

        if (currentState == NPCState.Search && arrivedAtSearchPosition && searchTimer >= searchDuration)
        {
            hasLastKnownPosition = false;
            ChangeState(NPCState.Patrol);
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case NPCState.Patrol: Patrol(); break;
            case NPCState.Chase: Chase(); break;
            case NPCState.Search: Search(); break;
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            GoToCurrentPatrolPoint();
        }
    }

    private void GoToCurrentPatrolPoint()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return;
        agent.isStopped = false;
        SetReachableDestination(patrolPoints[patrolIndex].position);
    }

    private void Chase()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        if (sensor.Player != null) SetReachableDestination(sensor.Player.position);
    }

    private void Search()
    {
        agent.speed = patrolSpeed;
        if (!arrivedAtSearchPosition)
        {
            agent.isStopped = false;
            SetReachableDestination(lastKnownPosition);
            if (!agent.pathPending && agent.remainingDistance <= searchTolerance)
            {
                arrivedAtSearchPosition = true;
                agent.isStopped = true;
            }
            return;
        }

        searchTimer += Time.deltaTime;
    }

    private void ChangeState(NPCState newState)
    {
        if (currentState == newState) return;
        previousState = currentState;
        currentState = newState;
        Debug.Log($"{gameObject.name}: {previousState} -> {currentState}");

        if (currentState == NPCState.Search)
        {
            searchTimer = 0f;
            arrivedAtSearchPosition = false;
        }
        else if (currentState == NPCState.Patrol)
        {
            GoToCurrentPatrolPoint();
        }
    }

    private void SetReachableDestination(Vector3 desiredPosition)
    {
        if (agent == null) return;

        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 1.5f, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = currentState == NPCState.Patrol ? Color.green : currentState == NPCState.Chase ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.8f);
        if (!hasLastKnownPosition) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(lastKnownPosition, 0.3f);
        Gizmos.DrawLine(transform.position, lastKnownPosition);
    }

    private void ApplyCharacterVisualLayout()
    {
        Transform visual = transform.Find("CharacterVisual");
        if (visual == null) return;

        SetPart(visual, "Body", new Vector3(0f, 0.20f, 0f), Vector3.zero, new Vector3(1.25f, 1.22f, 0.96f));
        SetPart(visual, "Head", new Vector3(0f, 1.15f, 0.02f), Vector3.zero, new Vector3(1.28f, 1.18f, 1.10f));
        SetPart(visual, "Belly", new Vector3(0f, 0.18f, 0.50f), Vector3.zero, new Vector3(0.68f, 0.82f, 0.12f));
        SetPart(visual, "LeftArm", new Vector3(-0.66f, 0.36f, 0.02f), new Vector3(0f, 0f, -32f), new Vector3(0.62f, 0.88f, 0.62f));
        SetPart(visual, "RightArm", new Vector3(0.66f, 0.36f, 0.02f), new Vector3(0f, 0f, 32f), new Vector3(0.62f, 0.88f, 0.62f));
        SetPart(visual, "LeftFoot", new Vector3(-0.35f, -0.37f, 0.24f), new Vector3(0f, 0f, -18f), new Vector3(0.62f, 0.38f, 0.74f));
        SetPart(visual, "RightFoot", new Vector3(0.35f, -0.37f, 0.24f), new Vector3(0f, 0f, 18f), new Vector3(0.62f, 0.38f, 0.74f));
        SetPart(visual, "LeftEye", new Vector3(-0.34f, 1.34f, 0.57f), Vector3.zero, new Vector3(0.33f, 0.42f, 0.10f));
        SetPart(visual, "RightEye", new Vector3(0.34f, 1.34f, 0.57f), Vector3.zero, new Vector3(0.33f, 0.42f, 0.10f));
        SetPart(visual, "LeftPupil", new Vector3(-0.34f, 1.34f, 0.66f), Vector3.zero, new Vector3(0.16f, 0.22f, 0.05f));
        SetPart(visual, "RightPupil", new Vector3(0.34f, 1.34f, 0.66f), Vector3.zero, new Vector3(0.16f, 0.22f, 0.05f));
        SetPart(visual, "Mouth", new Vector3(0f, 0.93f, 0.59f), Vector3.zero, new Vector3(0.48f, 0.20f, 0.06f));
        SetPart(visual, "Tongue", new Vector3(0f, 0.87f, 0.65f), Vector3.zero, new Vector3(0.25f, 0.07f, 0.03f));
    }

    private static void SetPart(Transform visual, string partName, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Transform part = visual.Find(partName);
        if (part == null) return;

        part.localPosition = position;
        part.localRotation = Quaternion.Euler(rotation);
        part.localScale = scale;
    }
}

using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool useTarget = true;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxAcceleration = 8f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Arrive")]
    [SerializeField] private float slowRadius = 5f;
    [SerializeField] private float stopRadius = 1f;

    [Header("Wander")]
    [SerializeField] private float wanderSpeed = 2.5f;
    [SerializeField] private float wanderChangeInterval = 1.2f;
    [SerializeField] private float wanderAngleChange = 65f;

    [Header("Avoidance")]
    [SerializeField] private SteeringSensor sensor;
    [SerializeField] private float avoidanceWeight = 2f;

    [Header("Visual State")]
    [SerializeField] private Renderer[] colourRenderers;

    private Vector3 velocity;
    private Vector3 desiredVelocity;
    private Vector3 wanderHeading;
    private float nextWanderChangeTime;
    private CharacterController characterController;
    private Transform visual;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftFoot;
    private Transform rightFoot;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftFootRest;
    private Quaternion rightFootRest;
    private float walkTime;

    public Vector3 Velocity => velocity;
    public bool IsUsingTarget => useTarget;
    public bool IsAvoiding { get; private set; }

    public void Configure(Transform targetTransform, SteeringSensor steeringSensor, Renderer[] renderers)
    {
        target = targetTransform;
        sensor = steeringSensor;
        colourRenderers = renderers;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null) characterController = gameObject.AddComponent<CharacterController>();
        characterController.center = Vector3.zero;
        characterController.height = 1.8f;
        characterController.radius = 0.48f;
        characterController.stepOffset = 0.1f;
        characterController.skinWidth = 0.03f;

        if (sensor == null) sensor = GetComponent<SteeringSensor>();
        if (colourRenderers == null || colourRenderers.Length == 0)
            colourRenderers = GetComponentsInChildren<Renderer>();
        visual = transform.Find("CharacterVisual");
        leftArm = visual == null ? null : visual.Find("LeftArm");
        rightArm = visual == null ? null : visual.Find("RightArm");
        leftFoot = visual == null ? null : visual.Find("LeftFoot");
        rightFoot = visual == null ? null : visual.Find("RightFoot");
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        if (leftFoot != null) leftFootRest = leftFoot.localRotation;
        if (rightFoot != null) rightFootRest = rightFoot.localRotation;
        wanderHeading = transform.forward;
    }

    private void Update()
    {
        Vector3 baseVelocity = useTarget && target != null
            ? CalculateArriveVelocity(transform.position, target.position, maxSpeed, slowRadius, stopRadius)
            : CalculateWanderVelocity();

        Vector3 avoidanceDirection = Vector3.zero;
        IsAvoiding = sensor != null && sensor.TryGetAvoidance(out avoidanceDirection);
        desiredVelocity = baseVelocity;
        if (IsAvoiding) desiredVelocity += avoidanceDirection * avoidanceWeight;
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, Mathf.Max(0f, maxSpeed));

        Vector3 acceleration = CalculateAcceleration(velocity, desiredVelocity, maxAcceleration);
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, Mathf.Max(0f, maxSpeed));
        if (characterController != null)
            characterController.Move(velocity * Time.deltaTime);
        else
            transform.position += velocity * Time.deltaTime;
        RotateTowardVelocity();
        AnimateWalk();
        UpdateColour();
    }

    public static Vector3 CalculateArriveVelocity(
        Vector3 agentPosition, Vector3 targetPosition, float maxSpeedValue,
        float slowRadiusValue, float stopRadiusValue)
    {
        Vector3 offset = Vector3.ProjectOnPlane(targetPosition - agentPosition, Vector3.up);
        float distance = offset.magnitude;
        if (distance <= Mathf.Max(0f, stopRadiusValue) || distance <= 0.0001f) return Vector3.zero;

        float safeSlowRadius = Mathf.Max(slowRadiusValue, stopRadiusValue + 0.0001f);
        float speed = distance >= safeSlowRadius
            ? Mathf.Max(0f, maxSpeedValue)
            : Mathf.Max(0f, maxSpeedValue) * distance / safeSlowRadius;
        return offset.normalized * speed;
    }

    public static Vector3 CalculateAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, float accelerationLimit)
    {
        return Vector3.ClampMagnitude(targetVelocity - currentVelocity, Mathf.Max(0f, accelerationLimit));
    }

    private Vector3 CalculateWanderVelocity()
    {
        if (Time.time >= nextWanderChangeTime)
        {
            Vector3 baseDirection = velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward;
            wanderHeading = Quaternion.Euler(0f, Random.Range(-wanderAngleChange, wanderAngleChange), 0f) * baseDirection;
            nextWanderChangeTime = Time.time + Mathf.Max(0.05f, wanderChangeInterval);
        }

        return wanderHeading.normalized * Mathf.Min(Mathf.Max(0f, wanderSpeed), Mathf.Max(0f, maxSpeed));
    }

    private void RotateTowardVelocity()
    {
        if (velocity.sqrMagnitude <= 0.0001f) return;
        Quaternion desiredRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
    }

    private void AnimateWalk()
    {
        if (visual == null || leftArm == null || rightArm == null || leftFoot == null || rightFoot == null) return;

        float movementAmount = Mathf.Clamp01(velocity.magnitude / Mathf.Max(0.01f, maxSpeed));
        walkTime += Time.deltaTime * Mathf.Lerp(2.5f, 9f, movementAmount);
        float swing = Mathf.Sin(walkTime) * movementAmount * 34f;
        float bob = Mathf.Abs(Mathf.Sin(walkTime * 2f)) * 0.045f * movementAmount;

        leftArm.localRotation = leftArmRest * Quaternion.Euler(swing, 0f, 0f);
        rightArm.localRotation = rightArmRest * Quaternion.Euler(-swing, 0f, 0f);
        leftFoot.localRotation = leftFootRest * Quaternion.Euler(-swing, 0f, 0f);
        rightFoot.localRotation = rightFootRest * Quaternion.Euler(swing, 0f, 0f);
        visual.localPosition = Vector3.up * bob;
        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(walkTime) * 2.5f * movementAmount);
    }

    private void UpdateColour()
    {
        Color stateColour = IsAvoiding ? new Color(1f, 0.25f, 0.08f) : useTarget ? Color.green : Color.blue;
        foreach (Renderer renderer in colourRenderers)
        {
            if (renderer != null) renderer.material.color = stateColour;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, stopRadius));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(stopRadius, slowRadius));
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);

        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}

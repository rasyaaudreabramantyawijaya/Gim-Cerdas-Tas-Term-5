using UnityEngine;
using UnityEngine.AI;

public class NPCCharacterAnimator : MonoBehaviour
{
    [Header("Walk Motion")]
    [SerializeField] private float walkCycleSpeed = 8f;
    [SerializeField] private float armSwingAngle = 32f;
    [SerializeField] private float footSwingAngle = 24f;

    private NavMeshAgent agent;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftFoot;
    private Transform rightFoot;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftFootRest;
    private Quaternion rightFootRest;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        CacheBodyParts();
    }

    private void Update()
    {
        if (leftArm == null)
        {
            CacheBodyParts();
            if (leftArm == null) return;
        }

        float movement = agent == null || agent.speed <= 0f
            ? 0f
            : Mathf.Clamp01(agent.velocity.magnitude / agent.speed);
        float swing = Mathf.Sin(Time.time * walkCycleSpeed) * movement;

        leftArm.localRotation = leftArmRest * Quaternion.Euler(swing * armSwingAngle, 0f, 0f);
        rightArm.localRotation = rightArmRest * Quaternion.Euler(-swing * armSwingAngle, 0f, 0f);
        leftFoot.localRotation = leftFootRest * Quaternion.Euler(-swing * footSwingAngle, 0f, 0f);
        rightFoot.localRotation = rightFootRest * Quaternion.Euler(swing * footSwingAngle, 0f, 0f);
    }

    private void CacheBodyParts()
    {
        Transform visual = transform.Find("CharacterVisual");
        if (visual == null) return;

        leftArm = visual.Find("LeftArm");
        rightArm = visual.Find("RightArm");
        leftFoot = visual.Find("LeftFoot");
        rightFoot = visual.Find("RightFoot");
        if (leftArm == null || rightArm == null || leftFoot == null || rightFoot == null) return;

        leftArmRest = leftArm.localRotation;
        rightArmRest = rightArm.localRotation;
        leftFootRest = leftFoot.localRotation;
        rightFootRest = rightFoot.localRotation;
    }
}

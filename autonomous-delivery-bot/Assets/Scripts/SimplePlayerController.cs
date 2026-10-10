using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 540f;
    private CharacterController controller;
    private Transform visual;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftLeg;
    private Transform rightLeg;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;
    private float walkTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) controller = gameObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        visual = transform.Find("BlockCharacterVisual");
        if (visual != null)
        {
            leftArm = visual.Find("LeftArm");
            rightArm = visual.Find("RightArm");
            leftLeg = visual.Find("LeftLeg");
            rightLeg = visual.Find("RightLeg");
            if (leftArm != null) leftArmRest = leftArm.localRotation;
            if (rightArm != null) rightArmRest = rightArm.localRotation;
            if (leftLeg != null) leftLegRest = leftLeg.localRotation;
            if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        float horizontal = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                         - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
        float vertical = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                       - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        controller.Move(direction * moveSpeed * Time.deltaTime);
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        AnimateWalk(direction.magnitude);
    }

    private void AnimateWalk(float movementAmount)
    {
        if (visual == null || leftArm == null || rightArm == null || leftLeg == null || rightLeg == null) return;
        float intensity = Mathf.Clamp01(movementAmount);
        walkTime += Time.deltaTime * Mathf.Lerp(2.5f, 9f, intensity);
        float swing = Mathf.Sin(walkTime) * intensity * 34f;
        float bob = Mathf.Abs(Mathf.Sin(walkTime * 2f)) * 0.045f * intensity;
        leftArm.localRotation = leftArmRest * Quaternion.Euler(swing, 0f, 0f);
        rightArm.localRotation = rightArmRest * Quaternion.Euler(-swing, 0f, 0f);
        leftLeg.localRotation = leftLegRest * Quaternion.Euler(-swing, 0f, 0f);
        rightLeg.localRotation = rightLegRest * Quaternion.Euler(swing, 0f, 0f);
        visual.localPosition = Vector3.up * bob;
        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(walkTime) * 2.5f * intensity);
    }
}

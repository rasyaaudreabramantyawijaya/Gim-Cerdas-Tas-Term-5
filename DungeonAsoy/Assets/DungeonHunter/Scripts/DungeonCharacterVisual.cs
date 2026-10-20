using UnityEngine;

public sealed class DungeonCharacterVisual : MonoBehaviour
{
    [SerializeField] private DungeonPlayerController controller;
    [SerializeField] private Transform body;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    [SerializeField] private Transform cloak;
    [SerializeField] private Transform staff;
    private float phase;

    public void Configure(DungeonPlayerController player, Transform bodyRoot, Transform leftArmPivot,
        Transform rightArmPivot, Transform leftLegPivot, Transform rightLegPivot, Transform cloakRoot, Transform staffRoot)
    {
        controller = player;
        body = bodyRoot;
        leftArm = leftArmPivot;
        rightArm = rightArmPivot;
        leftLeg = leftLegPivot;
        rightLeg = rightLegPivot;
        cloak = cloakRoot;
        staff = staffRoot;
    }

    private void Update()
    {
        if (controller == null) return;
        float speed = controller.PlanarSpeed;
        float blend = Mathf.Clamp01(speed / 4f);
        phase += Time.deltaTime * Mathf.Lerp(2f, controller.IsRunning ? 12f : 9f, blend);
        float swing = Mathf.Sin(phase) * 28f * blend;
        if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
        if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-swing * 0.72f, 0f, 0f);
        if (rightArm != null) rightArm.localRotation = Quaternion.Euler(8f + swing * 0.2f + controller.AttackPulse * -58f, 0f, 4f);
        if (body != null) body.localPosition = new Vector3(0f, 1.02f + Mathf.Abs(Mathf.Sin(phase * 2f)) * 0.045f * blend, 0f);
        if (cloak != null) cloak.localRotation = Quaternion.Euler(8f + Mathf.Sin(phase) * 9f * blend, Mathf.Sin(phase * 0.5f) * 3f, 0f);
        if (staff != null) staff.localRotation = Quaternion.Euler(controller.AttackPulse * -55f, 0f, 0f);
    }
}

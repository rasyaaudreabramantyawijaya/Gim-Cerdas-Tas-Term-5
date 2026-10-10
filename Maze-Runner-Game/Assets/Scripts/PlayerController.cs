using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float walkCycleSpeed = 9f;
    [SerializeField] private float limbSwingAngle = 34f;

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
    private CharacterController characterController;

    private void Start()
    {
        EnsureBlockCharacter();
        CacheBodyParts();
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.center = Vector3.zero;
            characterController.height = 2f;
            characterController.radius = 0.38f;
        }
    }

    private void Update()
    {
        Vector2 input = ReadInput();
        Vector3 movement = new Vector3(input.x, 0f, input.y).normalized;
        // CharacterController resolves wall collisions while keeping the player on maze paths.
        characterController.Move(movement * moveSpeed * Time.deltaTime);

        if (movement.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(movement, Vector3.up),
                rotationSpeed * Time.deltaTime
            );
        }

        AnimateWalk(movement.magnitude);
    }

    private static Vector2 ReadInput()
    {
        if (Keyboard.current == null) return Vector2.zero;

        float horizontal = 0f;
        float vertical = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
        return new Vector2(horizontal, vertical);
    }

    private void EnsureBlockCharacter()
    {
        visual = transform.Find("BlockCharacterVisual");
        if (visual != null) return;

        Renderer rootRenderer = GetComponent<Renderer>();
        if (rootRenderer != null) rootRenderer.enabled = false;

        visual = new GameObject("BlockCharacterVisual").transform;
        visual.SetParent(transform);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;

        Material skin = CreateMaterial(new Color(0.55f, 0.33f, 0.22f));
        Material hair = CreateMaterial(new Color(0.13f, 0.08f, 0.04f));
        Material shirt = CreateMaterial(new Color(0.02f, 0.63f, 0.67f));
        Material pants = CreateMaterial(new Color(0.25f, 0.17f, 0.62f));
        Material white = CreateMaterial(Color.white);
        Material dark = CreateMaterial(new Color(0.10f, 0.06f, 0.04f));

        CreateBlock("Torso", visual, new Vector3(0f, 0.23f, 0f), Vector3.zero, new Vector3(1.00f, 0.78f, 0.46f), shirt);
        CreateBlock("Head", visual, new Vector3(0f, 0.95f, 0f), Vector3.zero, new Vector3(0.82f, 0.62f, 0.56f), skin);
        CreateBlock("Hair", visual, new Vector3(0f, 1.22f, -0.01f), Vector3.zero, new Vector3(0.84f, 0.16f, 0.58f), hair);
        CreateBlock("LeftArm", visual, new Vector3(-0.68f, 0.23f, 0f), Vector3.zero, new Vector3(0.34f, 0.76f, 0.42f), skin);
        CreateBlock("RightArm", visual, new Vector3(0.68f, 0.23f, 0f), Vector3.zero, new Vector3(0.34f, 0.76f, 0.42f), skin);
        CreateBlock("LeftLeg", visual, new Vector3(-0.27f, -0.54f, 0f), Vector3.zero, new Vector3(0.42f, 0.78f, 0.44f), pants);
        CreateBlock("RightLeg", visual, new Vector3(0.27f, -0.54f, 0f), Vector3.zero, new Vector3(0.42f, 0.78f, 0.44f), pants);

        CreateBlock("LeftEye", visual, new Vector3(-0.19f, 1.02f, 0.30f), Vector3.zero, new Vector3(0.18f, 0.12f, 0.03f), white);
        CreateBlock("RightEye", visual, new Vector3(0.19f, 1.02f, 0.30f), Vector3.zero, new Vector3(0.18f, 0.12f, 0.03f), white);
        CreateBlock("Beard", visual, new Vector3(0f, 0.73f, 0.30f), Vector3.zero, new Vector3(0.38f, 0.18f, 0.04f), dark);
    }

    private void CacheBodyParts()
    {
        if (visual == null) visual = transform.Find("BlockCharacterVisual");
        if (visual == null) return;

        leftArm = visual.Find("LeftArm");
        rightArm = visual.Find("RightArm");
        leftLeg = visual.Find("LeftLeg");
        rightLeg = visual.Find("RightLeg");
        if (leftArm == null || rightArm == null || leftLeg == null || rightLeg == null) return;

        leftArmRest = leftArm.localRotation;
        rightArmRest = rightArm.localRotation;
        leftLegRest = leftLeg.localRotation;
        rightLegRest = rightLeg.localRotation;
    }

    private void AnimateWalk(float movementAmount)
    {
        if (leftArm == null) return;

        float intensity = Mathf.Clamp01(movementAmount);
        walkTime += Time.deltaTime * walkCycleSpeed * Mathf.Lerp(0.35f, 1f, intensity);
        float swing = Mathf.Sin(walkTime) * intensity;
        float bob = Mathf.Abs(Mathf.Sin(walkTime * 2f)) * 0.045f * intensity;

        leftArm.localRotation = leftArmRest * Quaternion.Euler(swing * limbSwingAngle, 0f, 0f);
        rightArm.localRotation = rightArmRest * Quaternion.Euler(-swing * limbSwingAngle, 0f, 0f);
        leftLeg.localRotation = leftLegRest * Quaternion.Euler(-swing * limbSwingAngle, 0f, 0f);
        rightLeg.localRotation = rightLegRest * Quaternion.Euler(swing * limbSwingAngle, 0f, 0f);
        visual.localPosition = Vector3.up * bob;
        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(walkTime) * 2.5f * intensity);
    }

    private static GameObject CreateBlock(
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localEulerAngles,
        Vector3 localScale,
        Material material
    )
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent);
        block.transform.localPosition = localPosition;
        block.transform.localRotation = Quaternion.Euler(localEulerAngles);
        block.transform.localScale = localScale;
        block.GetComponent<Renderer>().sharedMaterial = material;

        Collider collider = block.GetComponent<Collider>();
        if (collider != null) Object.Destroy(collider);
        return block;
    }

    private static Material CreateMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        return material;
    }
}

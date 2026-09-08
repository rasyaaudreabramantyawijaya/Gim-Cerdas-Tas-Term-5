using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;  // kecepatan putar menghadap arah gerak

    [Header("Walk Animation")]
    [SerializeField] private float swingAngle = 30f;
    [SerializeField] private float swingSpeed = 8f;
    [SerializeField] private float bobAmount = 0.05f;

    private Transform armPivotL, armPivotR;
    private Transform legPivotL, legPivotR;
    private Transform bodyRoot;

    private float currentSwing = 0f;
    private float currentSwingIntensity = 0f;
    private Vector3 bodyRootBasePos;

    void Start()
    {
        // Cari pivot-pivot by name di dalam BodyRoot
        bodyRoot = transform.Find("BodyRoot");
        if (bodyRoot != null)
        {
            armPivotL = bodyRoot.Find("ArmPivotL");
            armPivotR = bodyRoot.Find("ArmPivotR");
            legPivotL = bodyRoot.Find("LegPivotL");
            legPivotR = bodyRoot.Find("LegPivotR");
            bodyRootBasePos = bodyRoot.localPosition;
        }
    }

    void Update()
    {
        // ── Input gerak ──
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0f, v);
        float speed = direction.magnitude;

        // Gerakkan player
        if (speed > 0.01f)
        {
            transform.Translate(direction.normalized * moveSpeed * Time.deltaTime, Space.World);

            // Putar karakter menghadap arah gerak
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // ── Animasi jalan ──
        AnimateWalk(speed);
    }

    private void AnimateWalk(float speed)
    {
        if (bodyRoot == null) return;

        // Target intensitas: 1 saat bergerak, 0 saat diam
        float targetIntensity = speed > 0.05f ? 1f : 0f;
        currentSwingIntensity = Mathf.Lerp(currentSwingIntensity, targetIntensity, Time.deltaTime * 8f);

        // Hitung sudut swing
        if (currentSwingIntensity > 0.01f)
            currentSwing += Time.deltaTime * swingSpeed;
        else
            currentSwing = 0f; // reset saat diam supaya posisi kembali netral

        float swing = Mathf.Sin(currentSwing) * swingAngle * currentSwingIntensity;

        // Kaki: kiri & kanan bergantian (berlawanan arah)
        if (legPivotL != null)
            legPivotL.localRotation = Quaternion.Euler(swing, 0f, 0f);
        if (legPivotR != null)
            legPivotR.localRotation = Quaternion.Euler(-swing, 0f, 0f);

        // Tangan: berlawanan arah dengan kaki (natural walk)
        if (armPivotL != null)
            armPivotL.localRotation = Quaternion.Euler(-swing * 0.7f, 0f, 0f);
        if (armPivotR != null)
            armPivotR.localRotation = Quaternion.Euler(swing * 0.7f, 0f, 0f);

        // Body bob (naik-turun sedikit)
        float bob = Mathf.Abs(Mathf.Sin(currentSwing * 2f)) * bobAmount * currentSwingIntensity;
        bodyRoot.localPosition = bodyRootBasePos + new Vector3(0f, bob, 0f);
    }
}

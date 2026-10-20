using UnityEngine;

public sealed class DungeonArrow : MonoBehaviour
{
    private Vector3 velocity;
    private Transform owner;
    private DungeonHealth target;
    private float damage;
    private float expires;
    public static int ShotsFired { get; private set; }
    public static int TargetHits { get; private set; }

    public static DungeonArrow Launch(Vector3 origin, Vector3 direction, Transform shooter, DungeonHealth victim, float amount, Material material)
    {
        var obj = new GameObject("Flying Arrow");
        obj.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(obj.transform, false);
        shaft.transform.localScale = new Vector3(0.055f, 0.055f, 0.85f);
        shaft.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(shaft.GetComponent<Collider>());
        var feather = GameObject.CreatePrimitive(PrimitiveType.Cube);
        feather.transform.SetParent(obj.transform, false);
        feather.transform.localPosition = new Vector3(0, 0, -0.3f);
        feather.transform.localScale = new Vector3(0.22f, 0.04f, 0.15f);
        feather.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(feather.GetComponent<Collider>());
        var arrow = obj.AddComponent<DungeonArrow>();
        arrow.velocity = direction.normalized * 18f;
        arrow.owner = shooter;
        arrow.target = victim;
        arrow.damage = amount;
        arrow.expires = Time.time + 5f;
        ShotsFired++;
        return arrow;
    }

    private void Update()
    {
        float step = velocity.magnitude * Time.deltaTime;
        var hits = Physics.SphereCastAll(transform.position, 0.08f, velocity.normalized, step,
            ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.transform == owner || hit.collider.transform.IsChildOf(owner) || hit.collider.transform.IsChildOf(transform)) continue;
            var health = hit.collider.GetComponentInParent<DungeonHealth>();
            if (health == target && !health.IsDead) { health.TakeDamage(damage); TargetHits++; }
            Destroy(gameObject);
            return;
        }
        transform.position += velocity * Time.deltaTime;
        if (Time.time >= expires) Destroy(gameObject);
    }
}

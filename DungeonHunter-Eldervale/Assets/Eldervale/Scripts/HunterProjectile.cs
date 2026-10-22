using UnityEngine;
public sealed class HunterProjectile : MonoBehaviour
{
    public bool Friendly;
    public float Damage=20;
    Vector3 velocity;
    float until;
    public static void Create(Vector3 pos,Vector3 direction,bool friendly,float damage)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=friendly?"Player Rune":"Enemy Rune";g.transform.position=pos;g.transform.localScale=Vector3.one*.22f;
        Destroy(g.GetComponent<Collider>());
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=friendly?Color.cyan:new Color(1,.3f,.08f);mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",mat.color*3);g.GetComponent<Renderer>().material=mat;
        var p=g.AddComponent<HunterProjectile>();p.Friendly=friendly;p.Damage=damage;p.velocity=direction*18;p.until=Time.time+4;
        RuntimeResources.Own(g,mat);
        var light=g.AddComponent<Light>();light.color=mat.color;light.intensity=2;light.range=3;
        var trail=g.AddComponent<TrailRenderer>();trail.time=.15f;trail.startWidth=.15f;trail.endWidth=0;trail.material=mat;
    }
    void Update()
    {
        if(GameFlow.I==null||GameFlow.I.Screen!=GameScreen.Playing)return;
        Vector3 delta=velocity*Time.deltaTime;
        if(Physics.SphereCast(transform.position,.13f,delta.normalized,out var hit,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
        {
            var enemy=hit.collider.GetComponentInParent<EnemyAgent>();
            var player=hit.collider.GetComponentInParent<HunterPlayer>();
            if(Friendly && player!=null || !Friendly && enemy!=null) { transform.position+=delta;return; }
            if(Friendly && enemy!=null && !enemy.Civilian && (!enemy.Trainer||enemy.Dueling))enemy.Damage(Damage);
            if(!Friendly && player!=null)player.Damage(Damage);
            GameAudio.I?.Cue("impact",transform.position);Destroy(gameObject);return;
        }
        transform.position+=delta;if(Time.time>until)Destroy(gameObject);
    }
}

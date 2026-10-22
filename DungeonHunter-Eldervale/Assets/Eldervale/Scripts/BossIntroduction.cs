using UnityEngine;
public sealed class BossIntroduction : MonoBehaviour
{
    bool triggered;
    EnemyAgent boss;
    public string Speaker="Guardian of Veyr";
    public string Story="Dua sigil membawamu ke sini, pemburu. Relik ini menyimpan sumpah Eldervale. Jika engkau ingin membawanya pergi, buktikan bahwa engkau sanggup menjaganya.";
    public float Distance=23;
    void Start(){boss=GetComponent<EnemyAgent>();boss.IntroBlocked=true;}
    void Update()
    {
        var f=GameFlow.I;
        if(f==null||f.Player==null||triggered||f.Screen!=GameScreen.Playing)return;
        if(Vector3.Distance(f.Player.transform.position,transform.position)>Distance)return;
        triggered=true;f.Player.Cinema(3f,16);
        f.Talk(Speaker,Story);
        f.PendingBoss=boss;
        GameAudio.I?.Cue("bossAwake",transform.position);
        VisualEffects.Ring(transform.position,.8f,Color.cyan,3);
    }
}
public sealed class VisualEffects : MonoBehaviour
{
    float until,start,size;
    Color color;
    LineRenderer line;
    public static void EarthSlam(Vector3 pos)
    {
        Ring(pos, 5.5f, new Color(1f, .55f, .15f), 1.2f);
        var g = new GameObject("Earth Slam Fissure");
        g.transform.position = pos;
        var rockMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        rockMat.color = new Color(.45f, .38f, .28f);
        rockMat.SetFloat("_Smoothness", .1f);
        RuntimeResources.Own(g, rockMat);

        for(int i = 0; i < 8; i++)
        {
            float a = i / 8f * Mathf.PI * 2f;
            float r = Random.Range(2.2f, 4.2f);
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = "Earth Spike";
            spike.transform.SetParent(g.transform);
            spike.transform.position = pos + new Vector3(Mathf.Cos(a) * r, .8f, Mathf.Sin(a) * r);
            spike.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            spike.transform.localScale = new Vector3(.6f, Random.Range(1.6f, 2.8f), .6f);
            spike.GetComponent<Renderer>().sharedMaterial = rockMat;
            var col = spike.GetComponent<Collider>();if(col)Destroy(col);
        }
        var flash = g.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = new Color(1f, .5f, .1f);
        flash.intensity = 4.5f;
        flash.range = 10f;
        Destroy(g, 2.5f);
        GameAudio.I?.Cue("hit", pos);
        GameAudio.I?.Cue("land", pos);
    }
    public static void ArcaneVortex(Vector3 pos)
    {
        Ring(pos, 4.8f, new Color(.6f, .2f, 1f), 1.8f);
        var g = new GameObject("Arcane Vortex");
        g.transform.position = pos + Vector3.up * 1f;
        var light = g.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(.7f, .3f, 1f);
        light.intensity = 4f;
        light.range = 9f;
        Destroy(g, 2f);
        GameAudio.I?.Cue("skill", pos);
    }
    public static void WaterSplash(Vector3 pos)
    {
        var g=new GameObject("Water Splash Effect");g.transform.position=pos+Vector3.up*.05f;
        var e=g.AddComponent<VisualEffects>();e.start=Time.time;e.until=Time.time+.75f;e.size=1.8f;e.color=new Color(.55f,.88f,.95f,.85f);
        e.line=g.AddComponent<LineRenderer>();e.line.positionCount=33;e.line.loop=true;e.line.useWorldSpace=false;e.line.widthMultiplier=.14f;
        e.line.material=new Material(Shader.Find("Sprites/Default"));e.line.startColor=new Color(.75f,.95f,1f,.9f);e.line.endColor=new Color(.2f,.6f,.8f,0f);
        RuntimeResources.Own(g,e.line.sharedMaterial);
        for(int n=0;n<33;n++){float a=n/32f*Mathf.PI*2;e.line.SetPosition(n,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));}
        for(int i=0;i<8;i++)
        {
            var drop=GameObject.CreatePrimitive(PrimitiveType.Sphere);drop.name="Splash droplet";
            drop.transform.SetParent(g.transform);drop.transform.position=pos+Vector3.up*.15f;drop.transform.localScale=Vector3.one*Random.Range(.08f,.15f);
            var col=drop.GetComponent<Collider>();if(col)Destroy(col);
            var dm=new Material(Shader.Find("Universal Render Pipeline/Lit"));dm.color=new Color(.8f,.95f,1f,.9f);dm.SetFloat("_Smoothness",.95f);
            drop.GetComponent<Renderer>().sharedMaterial=dm;RuntimeResources.Own(drop,dm);
            var rb=drop.AddComponent<Rigidbody>();
            float angle=Random.Range(0,Mathf.PI*2);float outward=Random.Range(.8f,2.4f);
            rb.linearVelocity=new Vector3(Mathf.Cos(angle)*outward,Random.Range(2.8f,5.2f),Mathf.Sin(angle)*outward);
        }
        GameAudio.I?.Cue("splash",pos);
    }
    public static void Ring(Vector3 pos,float radius,Color color,float seconds)
    {
        var g=new GameObject("Rune telegraph");g.transform.position=pos+Vector3.up*.07f;
        var e=g.AddComponent<VisualEffects>();e.start=Time.time;e.until=Time.time+seconds;e.size=radius;e.color=color;
        e.line=g.AddComponent<LineRenderer>();e.line.positionCount=65;e.line.loop=true;e.line.useWorldSpace=false;e.line.widthMultiplier=.12f;
        e.line.material=new Material(Shader.Find("Sprites/Default"));e.line.startColor=e.line.endColor=color;
        RuntimeResources.Own(g,e.line.sharedMaterial);
        for(int n=0;n<65;n++){float a=n/64f*Mathf.PI*2;e.line.SetPosition(n,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
        var l=g.AddComponent<Light>();l.color=color;l.intensity=3;l.range=radius+2;
    }
    void Update()
    {
        float u=Mathf.InverseLerp(start,until,Time.time);
        if(line!=null && name.Contains("Water Splash"))
        {
            transform.localScale=new Vector3(Mathf.Lerp(.3f,size,u),1,Mathf.Lerp(.3f,size,u));
            Color c=Color.Lerp(color,new Color(color.r,color.g,color.b,0),u);
            line.startColor=line.endColor=c;
        }
        else
        {
            transform.localScale=Vector3.one*(.9f+Mathf.Sin(u*Mathf.PI)*.1f);
        }
        if(Time.time>until)Destroy(gameObject);
    }
}


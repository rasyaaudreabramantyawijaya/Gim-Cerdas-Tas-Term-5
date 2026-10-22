using UnityEngine;
public sealed class LootDrop : MonoBehaviour
{
    int coins;
    bool taken;
    Vector3 resting;
    float born;
    public static void Create(Vector3 point,int value)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Auto Loot — "+value+" coins";g.transform.position=point+Vector3.up*.6f;g.transform.localScale=Vector3.one*.32f;
        var collider=g.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=new Color(1,.78f,.25f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(1,.64f,.1f)*2);g.GetComponent<Renderer>().material=m;
        var loot=g.AddComponent<LootDrop>();loot.coins=value;loot.resting=g.transform.position;loot.born=Time.time;
        RuntimeResources.Own(g,m);
        VisualEffects.Ring(point,.45f,new Color(1,.75f,.2f),1);
    }
    void Update()
    {
        var f=GameFlow.I;if(f==null||f.Player==null||taken||f.Player.HP<=0||f.Screen!=GameScreen.Playing)return;
        var target=f.Player.transform.position+Vector3.up;
        float distance=Vector3.Distance(transform.position,target);
        if(distance<5)transform.position=Vector3.MoveTowards(transform.position,target,Time.deltaTime*(8+Time.time-born));
        else transform.position=resting+Vector3.up*Mathf.Sin((Time.time-born)*3)*.12f;
        transform.Rotate(0,90*Time.deltaTime,0);
        if(distance<.7f)
        {
            taken=true;f.State.coins+=coins;GameAudio.I?.Cue("pickup",transform.position);f.Note("Auto-loot: +"+coins+" koin");f.Save();Destroy(gameObject);
        }
    }
}


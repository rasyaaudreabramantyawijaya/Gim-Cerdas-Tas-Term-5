using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyAgent : MonoBehaviour
{
    static readonly List<EnemyAgent> all=new List<EnemyAgent>();
    public static int CombatCount { get { int n=0;foreach(var e in all)if(e!=null&&e.Engaged)n++;return n;} }
    public static int AlertCount { get { int n=0;foreach(var e in all)if(e!=null&&!e.Dead&&(e.State=="Investigate"||e.State=="Search"))n++;return n;} }
    public static bool BossActive { get { foreach(var e in all)if(e!=null&&e.Boss&&e.Engaged)return true;return false;} }
    public bool Ranged,Boss,Civilian,Trainer,Dueling;
    public int Role; // 0 = Sword, 1 = Archer, 2 = Mage
    public bool IntroBlocked,SanctumGuardian;
    public float HP=80, MaxHP=80;
    public bool Dead=>HP<=0;
    public string State="Patrol";
    public Vector3 LastKnown;
    public float MemoryAge=>Time.time-lastSeen;
    public bool Engaged=>!Dead&&!Civilian&&!IntroBlocked&&(!Trainer||Dueling)&&Time.time-lastSeen<6f&&(State=="Chase"||State=="Attack"||State=="Retreat"||State=="Reposition"||State=="Investigate");
    public NavMeshAgent Agent;
    public HumanoidView Visual;
    Vector3 home;
    float lastSeen=-100,attackAt,nextPatrol,nextShock;
    bool winding;
    float greetAt;
    float greetUntil;
    void OnEnable(){all.Add(this);}
    void OnDisable(){all.Remove(this);}
    void Start()
    {
        home=transform.position;LastKnown=home;
        Agent=gameObject.AddComponent<NavMeshAgent>();Agent.height=1.8f;Agent.radius=.35f;Agent.speed=Civilian?1.4f:Ranged?3.3f:3.8f;Agent.stoppingDistance=.3f;
        if(NavMesh.SamplePosition(home,out var p,5,NavMesh.AllAreas))Agent.Warp(p.position);
        if(Ranged && Role==0) Role=1;
        Ranged = Role != 0;
        Visual=gameObject.AddComponent<HumanoidView>();
        int charVisual = Civilian ? Random.Range(0,3) : Boss ? 2 : Role == 1 ? 1 : 0;
        Visual.Build(charVisual, !Civilian);
        if(!Civilian) Visual.SetWeapon(Role);
        if(Boss){if(MaxHP==80)MaxHP=HP=260;transform.localScale=Vector3.one*1.35f;}
    }
    public static void Hear(Vector3 position,float range)
    {
        foreach(var e in all)
        {
            if(e==null||e.Dead||e.Civilian||e.Trainer&&!e.Dueling||Vector3.Distance(e.transform.position,position)>range)continue;
            if(e.Engaged)continue;
            e.LastKnown=position;e.lastSeen=Time.time-3.5f;e.State="Investigate";
        }
    }
    bool CanSee(HunterPlayer player)
    {
        Vector3 delta=player.transform.position-transform.position;
        if(delta.magnitude>19 || Vector3.Angle(transform.forward,delta)>65)return false;
        Vector3 from=transform.position+Vector3.up*1.5f,to=player.transform.position+Vector3.up*1.1f;
        if(Physics.Linecast(from,to,out var hit,(1<<0)|(1<<8),QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<HunterPlayer>()==player;
        return true;
    }
    void Move(Vector3 point)
    {
        if(Agent==null||!Agent.enabled||!Agent.isOnNavMesh)return;
        if(NavMesh.SamplePosition(point,out var destination,4,NavMesh.AllAreas)) Agent.SetDestination(destination.position);
    }
    void Update()
    {
        var f=GameFlow.I;
        if(f==null||Dead)return;
        if(Agent!=null&&Agent.isOnNavMesh)Agent.isStopped=f.Screen!=GameScreen.Playing||winding;
        if(Visual!=null)Visual.Speed=Agent!=null&&Agent.enabled?Agent.velocity.magnitude:0;
        if(f.Screen!=GameScreen.Playing||f.Player==null||IntroBlocked)return;
        var player=f.Player;
        if(Civilian || Trainer&&!Dueling)
        {
            State="Patrol";
            if(Time.time<greetUntil)
            {
                State="Greet";if(Agent.isOnNavMesh)Agent.isStopped=true;
                Vector3 towards=player.transform.position-transform.position;towards.y=0;
                if(towards.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(towards),Time.deltaTime*4);
                return;
            }
            if(Civilian && Vector3.Distance(player.transform.position,transform.position)<4 && Time.time>greetAt)
            {
                greetAt=Time.time+50;greetUntil=Time.time+1.7f; State="Greet";
                Vector3 look=player.transform.position-transform.position;look.y=0;
                if(look.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look),Time.deltaTime*4);
                GameAudio.I?.SpeakNPC("Warga", "Selamat datang di Eldervale, pemburu.",false,transform.position+Vector3.up*1.5f);
                nextPatrol=Time.time+2;
            }
            if(Time.time>nextPatrol){nextPatrol=Time.time+Random.Range(5,11);Move(home+new Vector3(Random.Range(-8f,8f),0,Random.Range(-8f,8f)));}
            return;
        }
        if(Trainer&&Dueling&&player.HP<=12){Dueling=false;HP=MaxHP;player.Heal(100);State="Patrol";f.Note("Latihan selesai. Warden memulihkan HP.");return;}
        bool seen=CanSee(player)&&!player.Cinematic;
        if(seen||Trainer&&Dueling)
        {
            if(Time.time-lastSeen>6)GameAudio.I?.Speak(f.State.character,"alert");
            LastKnown=player.transform.position;lastSeen=Time.time;
        }
        float distance=Vector3.Distance(transform.position,player.transform.position);
        if(Boss&&Vector3.Distance(player.transform.position,home)>32)
        {HP=MaxHP;lastSeen=-100;State="Return";Move(home);return;}
        if(Time.time-lastSeen<6)
        {
            if(!seen&&!Dueling) {State="Investigate";Move(LastKnown);return;}
            if(Ranged)
            {
                if(distance<6 || HP<MaxHP*.28f&&distance<12)
                {State="Retreat";Move(transform.position+(transform.position-player.transform.position).normalized*6);}
                else if(distance>13) {State="Chase";Move(LastKnown);}
                else {State="Reposition";if(Agent.isOnNavMesh)Agent.ResetPath();}
                if(seen&&distance<19&&Time.time>attackAt&&!winding)StartCoroutine(RangedAttack(player));
            }
            else if(distance>2.3f) {State="Chase";Move(LastKnown);}
            else if(Time.time>attackAt&&!winding)StartCoroutine(MeleeAttack(player));
            else if(!winding&&Agent.isOnNavMesh)Agent.ResetPath();
            if(Boss&&HP<MaxHP*.5f&&distance<9&&Time.time>nextShock&&!winding)StartCoroutine(Shock(player));
        }
        else if(Time.time-lastSeen<10) {State="Search";Move(LastKnown);}
        else {State="Patrol";if(Time.time>nextPatrol){nextPatrol=Time.time+6;Move(home+new Vector3(Random.Range(-4f,4f),0,Random.Range(-4f,4f)));}}
        if(winding || Ranged&&seen)
        {
            Vector3 facing=player.transform.position-transform.position;facing.y=0;
            if(facing.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(facing),Time.deltaTime*8);
        }
    }
    IEnumerator MeleeAttack(HunterPlayer player)
    {
        winding=true;State="Attack";attackAt=Time.time+1.7f;Visual.Action("Attack");GameAudio.I?.Cue("enemyWindup",transform.position);
        yield return new WaitForSeconds(.55f);
        if(!Dead&&player!=null&&Vector3.Distance(transform.position,player.transform.position)<3.2f&&CanSee(player))
        { if(Trainer)player.HP=Mathf.Max(1,player.HP-8);else player.Damage(Boss?22:10);GameAudio.I?.Cue("hit",player.transform.position); }
        winding=false;
    }
    IEnumerator RangedAttack(HunterPlayer player)
    {
        winding=true;State="Attack";
        attackAt=Time.time+(Role==1?2.2f:2.8f);
        if(Role==1) // Archer
        {
            Visual.Action("Attack");
            GameAudio.I?.Cue("swing",transform.position);
            yield return new WaitForSeconds(.45f);
            if(!Dead&&player!=null&&CanSee(player))
            {
                Vector3 origin=transform.position+Vector3.up*1.3f+transform.forward*.6f;
                Vector3 aim=(player.transform.position+Vector3.up*.9f-origin).normalized;
                HunterProjectile.Create(origin,aim,false,14);
            }
        }
        else // Mage
        {
            Visual.Action("Cast");
            VisualEffects.Ring(transform.position,1.5f,new Color(.65f,.25f,1f),1.2f);
            GameAudio.I?.Cue("skill",transform.position);
            yield return new WaitForSeconds(.65f);
            if(!Dead&&player!=null&&CanSee(player))
            {
                Vector3 origin=transform.position+Vector3.up*1.4f+transform.forward*.7f;
                Vector3 aim=(player.transform.position+Vector3.up*1.1f-origin).normalized;
                HunterProjectile.Create(origin,aim,false,18);
            }
        }
        winding=false;
    }
    IEnumerator Shock(HunterPlayer player)
    {
        winding=true;State="Attack";nextShock=Time.time+8;GameFlow.I.Note("Guardian: hindari lingkaran rune!");
        GameAudio.I?.Cue("shockwave",transform.position);
        VisualEffects.Ring(transform.position,7,new Color(1,.35f,.12f),1.2f);
        yield return new WaitForSeconds(1f);
        if(!Dead&&player!=null&&Vector3.Distance(transform.position,player.transform.position)<7&&player.transform.position.y<transform.position.y+1.2f)player.Damage(25);
        winding=false;
    }
    public void Damage(float value)
    {
        if(Dead||Civilian||IntroBlocked||Trainer&&!Dueling)return;
        HP=Mathf.Max(0,HP-value);lastSeen=Time.time;State="Chase";
        if(GameFlow.I.Player!=null)LastKnown=GameFlow.I.Player.transform.position;
        if(Dead)
        {
            GameAudio.I?.Cue("enemyDeath",transform.position);
            if(Trainer){Dueling=false;HP=MaxHP;GameFlow.I.State.coins+=5;GameFlow.I.Player.Heal(100);GameFlow.I.Note("Menang duel latihan. +5 koin");State="Patrol";lastSeen=-100;return;}
            State="Dead";Visual?.Action("Dead");
            if(Agent!=null)Agent.enabled=false;
            var c=GetComponent<Collider>();if(c!=null)c.enabled=false;
            if(Boss){if(SanctumGuardian)GameFlow.I.State.guardian=true;LootDrop.Create(transform.position,60);GameFlow.I.Note(SanctumGuardian?"Guardian dikalahkan. Ambil Relic of Veyr.":"Sentinel dikalahkan. Jalur perbukitan aman.");GameFlow.I.AwardXP(120);GameFlow.I.Save();}
            else {LootDrop.Create(transform.position,10);GameFlow.I.AwardXP(25);GameFlow.I.Note("Enemy dikalahkan. +25 EXP");}
        }
    }
    void OnDrawGizmos()
    {
        if(GameFlow.I==null||!GameFlow.I.DebugAI||Civilian)return;
        Gizmos.color=Engaged?Color.red:Color.yellow;Gizmos.DrawWireSphere(transform.position,19);
        Gizmos.color=Color.cyan;Gizmos.DrawSphere(LastKnown,.2f);Gizmos.DrawLine(transform.position,LastKnown);
        if(Agent!=null&&Agent.hasPath){Gizmos.color=Color.green;var p=Agent.path.corners;for(int i=1;i<p.Length;i++)Gizmos.DrawLine(p[i-1],p[i]);}
    }
}

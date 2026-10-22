using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public sealed class HunterPlayer : MonoBehaviour
{
    public float HP=100, Speed;
    public bool Gliding;
    public string Prompt="";
    public Camera ViewCamera;
    public HumanoidView Visual;
    public float CinemaUntil;
    CharacterController motor;
    float vertical,yaw=25,pitch=18,lastEnter=-10,lastAttack=-10,lastSkill=-10,step;
    float cameraDistance=5.5f,cinemaDistance;
    Interactable nearest;
    float lastForwardPress=-10;
    bool sprintLatched;
    bool inWater;
    float nextWaterStep;
    public bool Cinematic => Time.time < CinemaUntil;
    float flameBladeUntil;
    public bool FlameBladeActive => Time.time < flameBladeUntil;
    public bool SkillReady => Time.time - lastSkill > (GameFlow.I!=null&&GameFlow.I.State.character==1?5f:6f);
    public float DamageAmount => 18+GameFlow.I.State.upgrade*5;

    void Awake()
    {
        var progress=GameFlow.I.State;
        if(progress.health!=null&&progress.health.Length==3)HP=progress.health[progress.character];
        motor=GetComponent<CharacterController>();motor.height=1.85f;motor.radius=.32f;motor.center=new Vector3(0,.94f,0);motor.stepOffset=.38f;
        var cameraObject=new GameObject("Hunter Camera");
        ViewCamera=cameraObject.AddComponent<Camera>(); ViewCamera.nearClipPlane=.15f; ViewCamera.farClipPlane=400;
        ViewCamera.allowHDR=true;ViewCamera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        ViewCamera.transform.position=transform.position+new Vector3(-3,3,-5);ViewCamera.transform.LookAt(transform.position+Vector3.up*1.4f);
        cameraObject.AddComponent<AudioListener>();
        Visual=gameObject.AddComponent<HumanoidView>();
        Visual.Build(GameFlow.I.State.character,false);
    }
    void Start() { GameFlow.I.Player=this; RefreshWeapon(); }
    public void RefreshWeapon() { int ch=GameFlow.I!=null?GameFlow.I.State.character:0;Visual?.SetWeapon(ch);GameAudio.I?.Cue("weapon"); }
    public void SwitchProfile()
    {
        foreach(Transform child in transform)Destroy(child.gameObject);
        if(Visual!=null)Destroy(Visual);
        Visual=gameObject.AddComponent<HumanoidView>();Visual.Build(GameFlow.I.State.character,false);RefreshWeapon();
        VisualEffects.Ring(transform.position,.7f,Color.cyan,.7f);
    }
    void Update()
    {
        var f=GameFlow.I;
        if(f==null)return;
        if(f.Controls && !Cinematic)
        {
            yaw+=(Input.GetAxis("Mouse X")*2.2f*f.Sensitivity+(Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.LeftArrow)?1:0))* (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.RightArrow)?75*Time.deltaTime:1);
            float k=(Input.GetKey(KeyCode.DownArrow)?1:0)-(Input.GetKey(KeyCode.UpArrow)?1:0);
            pitch=RunRules.CameraPitch(pitch,Input.GetAxis("Mouse Y")*-1.5f*f.Sensitivity+k*50*Time.deltaTime,f.InvertY);
            float h=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0);
            float v=(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0);
            Vector3 direction=Quaternion.Euler(0,yaw,0)*new Vector3(h,0,v).normalized;
            if(Input.GetKeyDown(KeyCode.W)){sprintLatched=Time.unscaledTime-lastForwardPress<=.3f;lastForwardPress=Time.unscaledTime;}
            if(!Input.GetKey(KeyCode.W))sprintLatched=false;
            Speed=direction.magnitude*((Input.GetKey(KeyCode.LeftShift)||sprintLatched)?7f:4.3f);
            if(direction.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),Time.deltaTime*12f);
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                float altitude=Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out var hit,100f,~0,QueryTriggerInteraction.Ignore)?Mathf.Max(0,hit.distance-.2f):100f;
                if(Time.time-lastEnter<.4f && RunRules.CanGlide(altitude,!motor.isGrounded,HP>0))
                { Gliding=true;GameAudio.I?.Cue("glide"); }
                else if(motor.isGrounded)
                {
                    vertical=7.3f;GameAudio.I?.Cue("jump",transform.position);Visual.Action("Jump");
                    if(inWater){ VisualEffects.WaterSplash(transform.position);GameAudio.I?.Cue("splash",transform.position); }
                }
                lastEnter=Time.time;
            }
            if(motor.isGrounded && vertical<0) { vertical=-2;Gliding=false; }
            vertical+= (Gliding?-3f:-22f)*Time.deltaTime;
            if(Gliding)vertical=Mathf.Max(vertical,-2f);
            bool wasGrounded=motor.isGrounded;
            motor.Move((direction*(Gliding?6f:Speed)+Vector3.up*vertical)*Time.deltaTime);
            CheckWater();
            if(!wasGrounded && motor.isGrounded) GameAudio.I?.Cue("land",transform.position);
            if(motor.isGrounded && Speed>.2f && Time.time>step)
            { GameAudio.I?.Cue(f.InTown?"step":"stoneStep",transform.position);step=Time.time+(Speed>5?.29f:.43f);EnemyAgent.Hear(transform.position,Speed>5?12:5); }
            if(Input.GetMouseButtonDown(0)) Attack();
            if(Input.GetKeyDown(KeyCode.Q)) Cast();
            if(Input.GetKeyDown(KeyCode.Tab)) {f.State.weapon=(f.State.weapon+1)%3;RefreshWeapon();f.Save();}
            if(Input.GetKeyDown(KeyCode.H) && f.State.potions>0 && HP<100) { f.State.potions--;Heal(45);GameAudio.I?.Cue("heal");f.Save();}
            if(Input.GetKeyDown(KeyCode.E) && nearest!=null)nearest.Use(this);
        }
        else {Speed=0;sprintLatched=false;lastForwardPress=-10;}
        FindInteraction();
        if(Visual!=null) Visual.Speed=Speed;
        if(f.Controls && motor.enabled && !motor.isGrounded && Cinematic)
        { vertical-=22*Time.deltaTime;motor.Move(Vector3.up*vertical*Time.deltaTime); }
        if(flameBladeUntil>0 && Time.time>flameBladeUntil){flameBladeUntil=0;Visual?.FlameBlade(false);}
        if(transform.position.y < -20) Damage(200);
    }
    void CheckWater()
    {
        float wy;
        bool touching = WaterVolume.Check(transform.position, out wy);
        if(touching && !inWater)
        {
            inWater=true;
            VisualEffects.WaterSplash(new Vector3(transform.position.x,wy,transform.position.z));
            GameAudio.I?.Cue("splash",transform.position);
        }
        else if(!touching && inWater)
        {
            inWater=false;
        }
        if(inWater && Speed>.5f && Time.time>nextWaterStep)
        {
            nextWaterStep=Time.time+.32f;
            VisualEffects.WaterSplash(new Vector3(transform.position.x,wy,transform.position.z));
        }
    }
    void FindInteraction()
    {
        nearest=null;float best=3.4f;
        foreach(var item in FindObjectsByType<Interactable>())
        { float d=Vector3.Distance(item.transform.position,transform.position);if(d<best && item.Available){nearest=item;best=d;} }
        Prompt=nearest!=null?"[E] "+nearest.Label:"";
    }
    void LateUpdate()
    {
        if(ViewCamera==null || GameFlow.I==null)return;
        float targetDistance=Cinematic?cinemaDistance:cameraDistance;
        float usedYaw=Cinematic?yaw+Mathf.Sin(Time.time*.6f)*18:yaw;
        Vector3 focus=transform.position+Vector3.up*1.45f;
        Quaternion rotation=Quaternion.Euler(Cinematic?28:pitch,usedYaw,0);
        Vector3 desired=focus-rotation*Vector3.forward*targetDistance;
        if(Physics.Linecast(focus,desired,out var obstruction,1<<0,QueryTriggerInteraction.Ignore))
            desired=obstruction.point+obstruction.normal*.25f;
        ViewCamera.transform.position=Vector3.Lerp(ViewCamera.transform.position,desired,Time.unscaledDeltaTime*9);
        ViewCamera.transform.LookAt(focus);
        ViewCamera.fieldOfView=Mathf.Lerp(ViewCamera.fieldOfView,GameFlow.I.Fov,Time.unscaledDeltaTime*3);
    }
    public void Cinema(float seconds,float distance) { CinemaUntil=Time.time+seconds;cinemaDistance=distance; }
    public void Attack()
    {
        if(Time.time-lastAttack<.55f || HP<=0)return;
        lastAttack=Time.time;
        int ch=GameFlow.I.State.character;
        Visual.Action("Attack");
        EnemyAgent.Hear(transform.position,18);

        if(ch==0) // Sword - Kael
        {
            motor.Move(transform.forward * 2.2f * Time.deltaTime);
            GameAudio.I?.Cue(FlameBladeActive ? "hit" : "swing", transform.position);
            GameAudio.I?.Speak(ch,"combat");
            StartCoroutine(SwordHit());
        }
        else if(ch==1) // Bow - Lyra
        {
            GameAudio.I?.Cue("swing", transform.position);
            GameAudio.I?.Speak(ch,"combat");
            var aim=ViewCamera.transform.forward;aim.y=Mathf.Clamp(aim.y,-.15f,.25f);
            HunterProjectile.Create(transform.position+Vector3.up*1.3f+transform.forward*.6f,aim.normalized,true,24+GameFlow.I.State.upgrade*5);
        }
        else // Grimoire - Garrick
        {
            GameAudio.I?.Cue("skill", transform.position);
            GameAudio.I?.Speak(ch,"combat");
            var aim=ViewCamera.transform.forward;aim.y=Mathf.Clamp(aim.y,-.15f,.25f);
            HunterProjectile.Create(transform.position+Vector3.up*1.3f+transform.forward*.6f,aim.normalized,true,28+GameFlow.I.State.upgrade*6);
        }
    }
    IEnumerator SwordHit()
    {
        yield return new WaitForSeconds(.18f);
        EnemyAgent target=null;float best=3.0f;
        foreach(var enemy in FindObjectsByType<EnemyAgent>())
        {
            if(enemy.Dead || enemy.Civilian || enemy.Trainer&&!enemy.Dueling)continue;
            var delta=enemy.transform.position-transform.position;
            if(delta.magnitude<best && Vector3.Dot(transform.forward,delta.normalized)>-.1f) { target=enemy;best=delta.magnitude; }
        }
        if(target!=null)
        {
            float dmg=DamageAmount + (FlameBladeActive ? 16 : 0);
            target.Damage(dmg);
            GameAudio.I?.Cue("hit",target.transform.position);
        }
    }
    public void Cast()
    {
        if(!SkillReady||HP<=0)return;
        lastSkill=Time.time;
        int ch=GameFlow.I.State.character;
        EnemyAgent.Hear(transform.position,22);

        if(ch==0) // Sword - Earth Slam & Flame Blade
        {
            Visual.Action("Jump");
            vertical=9.5f;
            StartCoroutine(ExecuteEarthSlam());
        }
        else if(ch==1) // Bow - Rain of Arrows / Windstorm Volley
        {
            Visual.Action("Cast");
            GameAudio.I?.Cue("skill", transform.position);
            motor.Move(-transform.forward * 3.5f * Time.deltaTime);
            StartCoroutine(ExecuteArrowRain());
        }
        else // Grimoire - Arcane Ruin Singularity
        {
            Visual.Action("Cast");
            StartCoroutine(ExecuteArcaneVortex());
        }
    }
    IEnumerator ExecuteEarthSlam()
    {
        yield return new WaitForSeconds(.35f);
        vertical=-20f;
        while(!motor.isGrounded) yield return null;
        VisualEffects.EarthSlam(transform.position);
        flameBladeUntil=Time.time+6f;
        Visual?.FlameBlade(true);
        GameFlow.I?.Note("Earth Slam! Pedang membara api suci (+16 Damage) selama 6 detik.");
        foreach(var enemy in FindObjectsByType<EnemyAgent>())
        {
            if(enemy.Dead || enemy.Civilian || enemy.Trainer&&!enemy.Dueling)continue;
            if(Vector3.Distance(transform.position, enemy.transform.position) <= 6.5f)
            {
                enemy.Damage(68 + GameFlow.I.State.upgrade * 8);
                GameAudio.I?.Cue("hit", enemy.transform.position);
            }
        }
    }
    IEnumerator ExecuteArrowRain()
    {
        Vector3 aim=ViewCamera.transform.forward;aim.y=Mathf.Clamp(aim.y,-.1f,.15f);
        for(int i=-2;i<=2;i++)
        {
            Vector3 spreadAim=Quaternion.Euler(0,i*9f,0)*aim;
            HunterProjectile.Create(transform.position+Vector3.up*1.3f+transform.forward*.7f,spreadAim.normalized,true,30+GameFlow.I.State.upgrade*5);
        }
        yield return new WaitForSeconds(.25f);
        Vector3 groundTarget=transform.position+aim*12f;
        groundTarget.y=Physics.Raycast(groundTarget+Vector3.up*10f,Vector3.down,out var hit,30f)?hit.point.y:transform.position.y;
        VisualEffects.Ring(groundTarget,4.5f,new Color(.4f,.9f,1f),1.5f);
        GameFlow.I?.Note("Hujan Panah Badai Angin menyapu area target!");
        for(int a=0;a<6;a++)
        {
            Vector3 dropPos=groundTarget+new Vector3(Random.Range(-3f,3f),14f,Random.Range(-3f,3f));
            HunterProjectile.Create(dropPos,Vector3.down,true,32+GameFlow.I.State.upgrade*5);
            yield return new WaitForSeconds(.06f);
        }
        foreach(var enemy in FindObjectsByType<EnemyAgent>())
        {
            if(enemy.Dead || enemy.Civilian || enemy.Trainer&&!enemy.Dueling)continue;
            if(Vector3.Distance(groundTarget, enemy.transform.position) <= 4.5f)
            {
                enemy.Damage(55 + GameFlow.I.State.upgrade * 6);
            }
        }
    }
    IEnumerator ExecuteArcaneVortex()
    {
        Vector3 aim=ViewCamera.transform.forward;aim.y=Mathf.Clamp(aim.y,-.1f,.15f);
        Vector3 vortexPos=transform.position+aim*10f;
        vortexPos.y=Physics.Raycast(vortexPos+Vector3.up*10f,Vector3.down,out var hit,30f)?hit.point.y:transform.position.y;
        VisualEffects.ArcaneVortex(vortexPos);
        GameFlow.I?.Note("Singularitas Gaib meledak dari Grimoire!");
        yield return new WaitForSeconds(.5f);
        foreach(var enemy in FindObjectsByType<EnemyAgent>())
        {
            if(enemy.Dead || enemy.Civilian || enemy.Trainer&&!enemy.Dueling)continue;
            float dist=Vector3.Distance(vortexPos, enemy.transform.position);
            if(dist <= 5.5f)
            {
                enemy.Damage(75 + GameFlow.I.State.upgrade * 8);
                GameAudio.I?.Cue("hit", enemy.transform.position);
            }
        }
    }
    public void Heal(float value) { HP=Mathf.Clamp(HP+value,0,100);SyncHealth(); }
    void SyncHealth(){var f=GameFlow.I;if(f!=null&&f.State.health!=null&&f.State.health.Length==3)f.State.health[f.State.character]=HP;}
    public void Damage(float value)
    {
        if(HP<=0 || Cinematic || GameFlow.I.Screen!=GameScreen.Playing)return;
        HP=Mathf.Max(0,HP-value);
        SyncHealth();
        GameAudio.I?.Cue(HP<=0?"death":"hurt",transform.position);
        if(HP<=0) {Visual.Action("Dead");GameAudio.I?.Speak(GameFlow.I.State.character,"death");GameFlow.I.SetScreen(GameScreen.Dead);}
    }
}

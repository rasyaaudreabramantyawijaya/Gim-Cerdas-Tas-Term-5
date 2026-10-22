using UnityEngine;

public sealed class HumanoidView : MonoBehaviour
{
    public float Speed;
    Animator animator;
    Transform body,hand,weapon,head,grimoire,quiver;
    GameObject flameAura;
    float actionUntil, lastY, currentRoll;
    string state="";

    public void Build(int character,bool enemy)
    {
        string asset=character==1?"Superhero_Female_FullBody":"Superhero_Male_FullBody";
        var model=Resources.Load<GameObject>("Humanoid/"+asset);
        if(model==null) { Debug.LogError("Missing rigged humanoid "+asset);return; }
        body=Instantiate(model,transform).transform;body.name="Rigged "+asset;
        body.localPosition=Vector3.zero;
        animator=body.GetComponentInChildren<Animator>();
        if(animator==null)animator=body.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController=Resources.Load<RuntimeAnimatorController>("HunterAnimator");
        animator.applyRootMotion=false;
        if(animator.isHuman)
        {
            hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            head=animator.GetBoneTransform(HumanBodyBones.Head);
        }
        if(hand==null)hand=body;

        // --- Hair ---
        var hairSource=Resources.Load<GameObject>("Humanoid/"+(character==1?"Hair_Long":character==2?"Hair_Beard":"Hair_SimpleParted"));
        var hairTex=Resources.Load<Texture2D>(character==1?"Humanoid/T_Hair_2_BaseColor":"Humanoid/T_Hair_1_BaseColor");
        var hairNorm=Resources.Load<Texture2D>(character==1?"Humanoid/T_Hair_2_Normal":"Humanoid/T_Hair_1_Normal");
        var hairMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if(hairTex!=null){hairMat.mainTexture=hairTex;hairMat.SetTexture("_BaseMap",hairTex);}
        if(hairNorm!=null){hairMat.SetTexture("_BumpMap",hairNorm);hairMat.EnableKeyword("_NORMALMAP");}
        hairMat.color=character==1?new Color(.75f,.42f,.25f):character==2?new Color(.35f,.25f,.18f):new Color(.15f,.12f,.1f);
        hairMat.SetFloat("_Smoothness",.3f);
        RuntimeResources.Own(gameObject,hairMat);

        if(hairSource!=null)
        {
            var hair=Instantiate(hairSource,body).transform;hair.localPosition=Vector3.zero;hair.localRotation=Quaternion.identity;
            var skeleton=body.GetComponentsInChildren<Transform>();
            foreach(var skin in hair.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var bones=skin.bones;
                for(int i=0;i<bones.Length;i++)if(bones[i]!=null)foreach(var candidate in skeleton)if(candidate.name==bones[i].name&&!candidate.IsChildOf(hair)){bones[i]=candidate;break;}
                skin.bones=bones;if(head!=null)skin.rootBone=head;
                var mesh=RuntimeResources.Own(hair.gameObject,new Mesh());skin.BakeMesh(mesh);
                if(mesh!=null)
                {
                    var strand=new GameObject("Secondary hair motion");strand.transform.SetParent(head!=null?head:body,false);
                    Transform pivot=head!=null?head:body;
                    var vertices=mesh.vertices;
                    for(int n=0;n<vertices.Length;n++)vertices[n]=pivot.InverseTransformPoint(skin.transform.TransformPoint(vertices[n]));
                    mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                    strand.AddComponent<MeshFilter>().sharedMesh=mesh;strand.AddComponent<MeshRenderer>().sharedMaterial=hairMat;
                    strand.AddComponent<WindMotion>().kind=2;skin.enabled=false;
                }
            }
        }

        // --- Body Textures & Lighting ---
        Texture2D baseTex=null,normTex=null;
        if(character==1)
        {
            baseTex=Resources.Load<Texture2D>("Humanoid/T_Superhero_Female_Dark_BaseColor");
            normTex=Resources.Load<Texture2D>("Humanoid/T_Superhero_Female_Normal");
        }
        else
        {
            baseTex=Resources.Load<Texture2D>(character==2?"Humanoid/T_Superhero_Male_Ligh":"Humanoid/T_Superhero_Male_Dark");
            normTex=Resources.Load<Texture2D>("Humanoid/T_Superhero_Male_Normal");
        }

        foreach(var renderer in body.GetComponentsInChildren<Renderer>())
        {
            foreach(var material in renderer.materials)
            {
                RuntimeResources.Own(body.gameObject,material);
                material.shader=Shader.Find("Universal Render Pipeline/Lit");
                if(baseTex!=null){material.mainTexture=baseTex;material.SetTexture("_BaseMap",baseTex);}
                if(normTex!=null){material.SetTexture("_BumpMap",normTex);material.EnableKeyword("_NORMALMAP");}
                material.color=enemy?new Color(.85f,.65f,.65f):character==2?new Color(.74f,.74f,.74f):Color.white;
                // The source texture stores roughness, while URP's metallic map expects metallic in R
                // and smoothness in A. Treating roughness as metallic made skin glossy and blew out faces.
                material.SetTexture("_MetallicGlossMap",null);
                material.DisableKeyword("_METALLICSPECGLOSSMAP");
                material.SetFloat("_Metallic",0f);
                material.SetFloat("_Smoothness",character==2?.12f:.2f);
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",Color.black);
            }
        }

        // --- Gentle character fill to retain face detail without a hard hot spot ---
        if(!enemy)
        {
            var heroLight=new GameObject("Character Soft Fill").AddComponent<Light>();
            heroLight.transform.SetParent(transform,false);
            heroLight.transform.localPosition=new Vector3(0,2.1f,1.1f);
            heroLight.type=LightType.Point;
            heroLight.range=3.8f;
            heroLight.intensity=character==2?.34f:.08f;
            heroLight.color=character==2?Color.white:new Color(.86f,.92f,1f);
            heroLight.shadows=LightShadows.None;
        }

        // --- Flowing Cape ---
        var cloak=new GameObject("Wind Cape");
        var chest=animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Chest):null;
        var leftShoulder=animator.isHuman?animator.GetBoneTransform(HumanBodyBones.LeftShoulder):null;
        var rightShoulder=animator.isHuman?animator.GetBoneTransform(HumanBodyBones.RightShoulder):null;
        if(leftShoulder==null&&animator.isHuman)leftShoulder=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        if(rightShoulder==null&&animator.isHuman)rightShoulder=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        cloak.transform.SetParent(transform,false);
        cloak.transform.position=transform.TransformPoint(new Vector3(0,1.46f,-.2f));cloak.transform.rotation=transform.rotation;
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color=enemy?new Color(.35f,.12f,.12f):character==1?new Color(.12f,.32f,.28f):character==2?new Color(.42f,.32f,.18f):new Color(.14f,.22f,.35f);
        mat.SetFloat("_Cull",0);mat.SetFloat("_Smoothness",.15f);
        RuntimeResources.Own(cloak,mat);
        var flowingCape=cloak.AddComponent<FlowingCape>();
        flowingCape.Shoulder=chest;flowingCape.LeftShoulder=leftShoulder;flowingCape.RightShoulder=rightShoulder;
        flowingCape.Hips=animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Hips):null;flowingCape.Head=head;
        flowingCape.Avatar=transform;flowingCape.Build(mat);

        // --- Fitted Adventurer Accents ---
        var outfit=new GameObject("Adventurer clothing");outfit.transform.SetParent(transform,false);
        outfit.AddComponent<AdventurerOutfit>().Build(animator,character,enemy);

        SetWeapon(0);
    }

    public void SetWeapon(int value)
    {
        if(weapon!=null)Destroy(weapon.gameObject);
        if(grimoire!=null)Destroy(grimoire.gameObject);
        if(quiver!=null)Destroy(quiver.gameObject);
        weapon=null;grimoire=null;quiver=null;flameAura=null;

        var metalMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        metalMat.color=new Color(.78f,.75f,.68f);metalMat.SetFloat("_Metallic",.85f);metalMat.SetFloat("_Smoothness",.75f);
        RuntimeResources.Own(gameObject,metalMat);

        var woodMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodMat.color=new Color(.35f,.22f,.12f);woodMat.SetFloat("_Smoothness",.25f);
        RuntimeResources.Own(gameObject,woodMat);

        if(value==0)
        {
            // === 1. Forged Steel & Flame Broadsword ===
            var w=GameObject.CreatePrimitive(PrimitiveType.Cube);w.name="Steel Broadsword";
            Destroy(w.GetComponent<Collider>());w.transform.SetParent(hand!=null?hand:transform,false);
            w.transform.localPosition=new Vector3(.02f,-.1f,.25f);
            w.transform.localRotation=Quaternion.Euler(90,0,0);
            w.transform.localScale=new Vector3(.065f,.55f,.025f);
            var blade=RuntimeResources.Own(w,new Mesh{name="Forged tapered blade"});
            blade.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.3f,0),new Vector3(0,.67f,0),new Vector3(-.5f,.3f,0),new Vector3(0,-.5f,.5f),new Vector3(0,.3f,.5f),new Vector3(0,-.5f,-.5f),new Vector3(0,.3f,-.5f)};
            blade.triangles=new[]{0,5,6,0,6,4,4,6,3,5,1,2,5,2,6,6,2,3,0,4,8,0,8,7,4,3,8,7,8,2,7,2,1,8,3,2};
            blade.RecalculateNormals();w.GetComponent<MeshFilter>().sharedMesh=blade;
            w.GetComponent<Renderer>().sharedMaterial=metalMat;
            WeaponPart(w.transform,"Crossguard",new Vector3(0,-.5f,0),new Vector3(3,.1f,2),metalMat);
            WeaponPart(w.transform,"Grip",new Vector3(0,-.68f,0),new Vector3(.75f,.27f,1.3f),woodMat);
            WeaponPart(w.transform,"Pommel",new Vector3(0,-.84f,0),new Vector3(1.1f,.07f,1.8f),metalMat);

            // Fiery Aura for Flame Blade skill
            flameAura=new GameObject("Flame Aura Light");flameAura.transform.SetParent(w.transform,false);
            flameAura.transform.localPosition=new Vector3(0,.2f,0);
            var fl=flameAura.AddComponent<Light>();fl.type=LightType.Point;fl.range=3.5f;fl.color=new Color(1f,.45f,.1f);fl.intensity=2.8f;
            flameAura.SetActive(false);
            weapon=w.transform;
        }
        else if(value==1)
        {
            // === 2. Elven Recurve Longbow & Back Quiver ===
            var bow=new GameObject("Recurve Bow");bow.transform.SetParent(hand!=null?hand:transform,false);
            bow.transform.localPosition=new Vector3(-.02f,.05f,.05f);
            bow.transform.localRotation=Quaternion.Euler(15,80,-30);
            // Center Grip
            var grip=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Destroy(grip.GetComponent<Collider>());
            grip.transform.SetParent(bow.transform,false);grip.transform.localScale=new Vector3(.04f,.12f,.04f);
            grip.GetComponent<Renderer>().sharedMaterial=woodMat;
            // Upper and lower curved limbs
            for(int s=-1;s<=1;s+=2)
            {
                var limb=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(limb.GetComponent<Collider>());
                limb.transform.SetParent(bow.transform,false);
                limb.transform.localPosition=new Vector3(0,s*.32f,-.06f);
                limb.transform.localRotation=Quaternion.Euler(s*28f,0,0);
                limb.transform.localScale=new Vector3(.03f,.45f,.02f);
                limb.GetComponent<Renderer>().sharedMaterial=woodMat;
            }
            // Bowstring
            var strObj=new GameObject("Bowstring");strObj.transform.SetParent(bow.transform,false);
            var lr=strObj.AddComponent<LineRenderer>();lr.positionCount=3;lr.useWorldSpace=false;lr.widthMultiplier=.008f;
            lr.material=new Material(Shader.Find("Sprites/Default"));lr.startColor=lr.endColor=new Color(.9f,.9f,.85f,.9f);
            lr.SetPosition(0,new Vector3(0,.55f,-.15f));lr.SetPosition(1,new Vector3(0,0,-.1f));lr.SetPosition(2,new Vector3(0,-.55f,-.15f));

            // Back Quiver with arrows
            var spine=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Chest):transform;
            quiver=new GameObject("Leather Quiver").transform;quiver.transform.SetParent(spine!=null?spine:transform,false);
            quiver.transform.localPosition=new Vector3(.18f,.15f,-.18f);
            quiver.transform.localRotation=Quaternion.Euler(25,-25,-20);
            var qbody=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Destroy(qbody.GetComponent<Collider>());
            qbody.transform.SetParent(quiver.transform,false);qbody.transform.localScale=new Vector3(.12f,.4f,.12f);
            qbody.GetComponent<Renderer>().sharedMaterial=woodMat;
            // Arrow shafts sticking out
            for(int a=0;a<3;a++)
            {
                var arr=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Destroy(arr.GetComponent<Collider>());
                arr.transform.SetParent(quiver.transform,false);arr.transform.localPosition=new Vector3(a*.03f-.03f,.35f,a*.02f);
                arr.transform.localScale=new Vector3(.015f,.35f,.015f);arr.GetComponent<Renderer>().sharedMaterial=woodMat;
            }
            weapon=bow.transform;
        }
        else if(value==2)
        {
            // === 3. Floating Arcane Grimoire ===
            grimoire=new GameObject("Floating Grimoire").transform;grimoire.transform.SetParent(transform,false);
            grimoire.transform.localPosition=new Vector3(-.42f,1.22f,.35f);
            var bookCover=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(bookCover.GetComponent<Collider>());
            bookCover.transform.SetParent(grimoire.transform,false);bookCover.transform.localScale=new Vector3(.28f,.38f,.07f);
            var leatherCover=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            leatherCover.color=new Color(.24f,.12f,.32f);leatherCover.SetFloat("_Smoothness",.4f);
            bookCover.GetComponent<Renderer>().sharedMaterial=leatherCover;
            // Gold corner mounts
            for(int cx=-1;cx<=1;cx+=2)for(int cy=-1;cy<=1;cy+=2)
            {
                var c=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(c.GetComponent<Collider>());
                c.transform.SetParent(bookCover.transform,false);c.transform.localPosition=new Vector3(cx*.42f,cy*.44f,0);
                c.transform.localScale=new Vector3(.2f,.15f,1.15f);c.GetComponent<Renderer>().sharedMaterial=metalMat;
            }
            // Glowing pages & rune light
            var pages=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(pages.GetComponent<Collider>());
            pages.transform.SetParent(grimoire.transform,false);pages.transform.localScale=new Vector3(.24f,.34f,.055f);
            var pageMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pageMat.color=new Color(.95f,.9f,.75f);pageMat.EnableKeyword("_EMISSION");pageMat.SetColor("_EmissionColor",new Color(.3f,.15f,.5f));
            pages.GetComponent<Renderer>().sharedMaterial=pageMat;
            var gLight=grimoire.gameObject.AddComponent<Light>();gLight.type=LightType.Point;gLight.range=3.2f;gLight.color=new Color(.65f,.25f,1f);gLight.intensity=2.2f;
            weapon=grimoire.transform;
        }
    }

    public void FlameBlade(bool active)
    {
        if(flameAura!=null) flameAura.SetActive(active);
    }

    static void WeaponPart(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;Destroy(g.GetComponent<Collider>());
    }

    public void Action(string name)
    {
        actionUntil=Time.time+(name=="Dead"?10000:name=="Jump"?.6f:.65f);
        if(animator!=null&&animator.runtimeAnimatorController!=null&&animator.HasState(0,Animator.StringToHash(name)))
        {animator.CrossFadeInFixedTime(name,.1f,0,0);state=name;}
    }

    void Play(string name)
    {
        if(animator==null || animator.runtimeAnimatorController==null||name==state)return;
        if(animator.HasState(0,Animator.StringToHash(name))) {animator.CrossFadeInFixedTime(name,.16f);state=name;}
    }

    void Update()
    {
        // Smooth turning lean / banking (eliminates stiffness)
        float deltaY = Mathf.DeltaAngle(lastY, transform.eulerAngles.y);
        lastY = transform.eulerAngles.y;
        float targetRoll = Speed > .4f ? Mathf.Clamp(deltaY * -1.8f, -12f, 12f) : 0f;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * 8f);
        if(body != null) body.localRotation = Quaternion.Euler(0, 0, currentRoll);

        // Floating grimoire dynamic hovering & bobbing
        if(grimoire != null)
        {
            float bob = Mathf.Sin(Time.time * 2.8f) * .045f;
            grimoire.transform.localPosition = new Vector3(-.42f, 1.22f + bob, .35f);
            grimoire.transform.localRotation = Quaternion.Euler(18f, Mathf.Sin(Time.time * 2f) * 14f, -currentRoll * .6f);
        }

        if(Time.time>=actionUntil) Play(Speed>5f?"Run":Speed>.1f?"Walk":"Idle");
    }
}

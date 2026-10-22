using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// All title scenery is actual 3D geometry. No background photo or sky card.
public sealed class ScenicBackdrop : MonoBehaviour
{
    Material rock,earth,stone,bronze,leaves,trunk,water,fall,runes;
    Transform root;
    public void Build()
    {
        root=new GameObject("Eldervale — mountain sanctuary").transform;root.SetParent(transform);
        rock=Surface("Moss-covered cliff",new Color(.8f,.86f,.8f),"mossy_rock",3);
        earth=Surface("Forest floor",new Color(.6f,.75f,.5f),"forest_ground_04",9);
        stone=Surface("Ancient masonry",new Color(.55f,.59f,.57f),"stone_brick_wall_001",2);
        bronze=Surface("Weathered bronze",new Color(.42f,.32f,.17f));bronze.SetFloat("_Metallic",.65f);
        leaves=Surface("Pine needles",new Color(.075f,.18f,.12f));trunk=Surface("Pine bark",new Color(.18f,.14f,.1f));
        water=WaterMaterial(false);fall=WaterMaterial(true);
        runes=Surface("Gate turquoise runes",new Color(.11f,.4f,.37f));runes.EnableKeyword("_EMISSION");runes.SetColor("_EmissionColor",new Color(.06f,.75f,.66f)*2.5f);
        foreach(var material in new[]{rock,earth,stone,bronze,leaves,trunk,water,fall,runes})RuntimeResources.Own(gameObject,material);
        var state=Random.state;Random.InitState(71);
        Ground();
        // Crags overlap and bury their roots; silhouettes vary rather than floating pillars.
        for(int n=0;n<26;n++)
        {
            float x=n<17?Random.Range(31,69):Random.Range(-70,-36),z=Random.Range(61,115),h=Random.Range(20,55);
            Crag(root,new Vector3(x,h*.36f-3,z),new Vector3(Random.Range(13,24),h,Random.Range(16,27)),rock);
        }
        for(int n=0;n<11;n++)
        {float h=Random.Range(35,68);Crag(root,new Vector3(-70+n*17,h*.32f-3,125+Random.Range(-5,28)),new Vector3(36,h,42),rock);}
        for(int n=0;n<40;n++)
        {
            float x=Random.Range(-64,62),z=Random.Range(-17,100);
            if(x>-20&&x<55&&z<60||Mathf.Abs(x-8)<13)continue;
            Pine(root,new Vector3(x,-.5f,z),Random.Range(5,13),leaves,trunk);
        }
        for(int n=0;n<34;n++)
        {var p=new Vector3(Random.Range(-45,60),0,Random.Range(-18,70));if(Mathf.Abs(p.x-8)>7)Crag(root,p,new Vector3(Random.Range(1,4),Random.Range(1,3),Random.Range(1,4)),rock);}
        Pine(root,new Vector3(-16,-1,-13),17,leaves,trunk);
        Pine(root,new Vector3(-24,-1,-2),20,leaves,trunk);
        WaterPlane(root,"Mountain pool",new Vector3(30,-.45f,13),new Vector2(48,75),water);
        Crag(root,new Vector3(43,15,56),new Vector3(22,44,20),rock);
        Waterfall(new Vector3(40,32,44),6,32);
        Waterfall(new Vector3(49,20,43),3.8f,20);
        PathAndGate();
        Atmosphere();Random.state=state;
        var camera=new GameObject("Sanctuary cinematic camera").AddComponent<Camera>();camera.tag="MainCamera";
        camera.nearClipPlane=.15f;camera.farClipPlane=400;camera.fieldOfView=54;
        camera.allowHDR=true;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        camera.gameObject.AddComponent<AudioListener>();camera.gameObject.AddComponent<SanctuaryCamera>();
        var probe=new GameObject("Sanctuary reflection").AddComponent<ReflectionProbe>();probe.transform.position=new Vector3(16,5,12);probe.size=new Vector3(150,70,180);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.resolution=128;
    }
    void Ground()
    {
        const int n=72;var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var tris=new int[n*n*6];int ti=0;
        for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)
        {
            float wx=x/(float)n*180-90,wz=z/(float)n*210-55;
            float y=-1+Mathf.PerlinNoise(wx*.027f+50,wz*.027f+50)*1.5f;
            y+=Mathf.SmoothStep(0,6,Mathf.InverseLerp(20,85,Mathf.Abs(wx)));
            if(wx>14&&wx<55&&wz<50)y=-2;
            int i=z*(n+1)+x;vertices[i]=new Vector3(wx,y,wz);uv[i]=new Vector2(x/(float)n,z/(float)n);
            if(x<n&&z<n){tris[ti++]=i;tris[ti++]=i+n+1;tris[ti++]=i+1;tris[ti++]=i+1;tris[ti++]=i+n+1;tris[ti++]=i+n+2;}
        }
        var mesh=new Mesh{name="Sculpted valley terrain"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
        MeshObject(root,"Grounded valley",mesh,earth);
    }
    void PathAndGate()
    {
        for(int n=0;n<18;n++)
        {
            float z=-22+n*3.2f,x=8+Mathf.Sin(n*.18f)*3;
            Block("Stone approach",new Vector3(x,-1.5f,z),new Vector3(8,3.14f,3.25f),stone);
            Block("Low parapet",new Vector3(x-4.2f,-.5f,z),new Vector3(.65f,2.2f,3.25f),stone);
        }
        Vector3 p=new Vector3(10,.1f,36);
        Crag(root,p+new Vector3(-10,9,3),new Vector3(14,27,20),rock);
        Crag(root,p+new Vector3(12,16,4),new Vector3(15,38,23),rock);
        Crag(root,p+new Vector3(1,19,8),new Vector3(25,18,20),rock);
        for(int side=-1;side<=1;side+=2)
        {
            Block("Gate foundation",p+new Vector3(side*4,.35f,0),new Vector3(2.4f,.7f,3),stone);
            for(int n=0;n<7;n++)Block("Gate jamb stone",p+new Vector3(side*4,1+n*1.2f,0),new Vector3(1.5f,1.15f,1.8f),stone);
            Block("Bronze inset",p+new Vector3(side*4,4,-.94f),new Vector3(.14f,6,.1f),bronze);
            var torch=Block("Gate lantern",p+new Vector3(side*5,2,-1),new Vector3(.45f,.75f,.45f),bronze);
            Block("Lantern pedestal",p+new Vector3(side*5,.8f,-1),new Vector3(.65f,1.6f,.65f),stone);
            var light=torch.AddComponent<Light>();light.color=new Color(1,.63f,.26f);light.range=11;light.intensity=3;
        }
        for(int n=0;n<13;n++)
        {
            float a=n/12f*Mathf.PI;var arch=Block("Grounded gate arch",p+new Vector3(Mathf.Cos(a)*4,8.2f+Mathf.Sin(a)*4,0),new Vector3(1.18f,1.65f,1.8f),stone);
            arch.transform.rotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90);
        }
        var doorMat=Surface("Deep dungeon",new Color(.03f,.04f,.045f));
        var leftPivot=new GameObject("Gate Left Door Pivot");leftPivot.transform.SetParent(root);leftPivot.transform.position=p+new Vector3(-3.3f,4.6f,.55f);
        var leftDoor=Block("Left Door Wing",leftPivot.transform.position+new Vector3(1.65f,0,0),new Vector3(3.3f,9.3f,.4f),doorMat);leftDoor.transform.SetParent(leftPivot.transform);
        var rightPivot=new GameObject("Gate Right Door Pivot");rightPivot.transform.SetParent(root);rightPivot.transform.position=p+new Vector3(3.3f,4.6f,.55f);
        var rightDoor=Block("Right Door Wing",rightPivot.transform.position+new Vector3(-1.65f,0,0),new Vector3(3.3f,9.3f,.4f),doorMat);rightDoor.transform.SetParent(rightPivot.transform);
        Block("Gate Abyssal Void",p+new Vector3(0,4.6f,3f),new Vector3(6.8f,9.5f,5f),Surface("Void Dark",Color.black));
        SanctuaryCamera.LeftDoor=leftPivot.transform;SanctuaryCamera.RightDoor=rightPivot.transform;

        for(int n=0;n<7;n++)
        {Block("Gate carved seam",p+new Vector3(-2.8f+n*.93f,4.4f,.28f),new Vector3(.04f,8,.055f),bronze);}
        var emblem=new GameObject("Gate rune seal").AddComponent<LineRenderer>();emblem.transform.SetParent(root);emblem.positionCount=65;emblem.loop=true;emblem.widthMultiplier=.07f;emblem.material=runes;
        for(int n=0;n<65;n++){float a=n/64f*Mathf.PI*2;emblem.SetPosition(n,p+new Vector3(Mathf.Cos(a)*1.5f,5+Mathf.Sin(a)*1.5f,-.02f));}
        var glow=new GameObject("Rune light").AddComponent<Light>();glow.transform.position=p+new Vector3(0,5,-2);glow.color=new Color(.1f,.9f,.8f);glow.intensity=2;glow.range=12;
        for(int i=0;i<8;i++)Block("Gate stairs",p+new Vector3(0,-.12f+i*.1f,-4+i*.4f),new Vector3(8,.3f,1.6f),stone);
    }
    void Waterfall(Vector3 top,float width,float height)
    {
        int count=30;var v=new Vector3[(count+1)*2];var uv=new Vector2[v.Length];var tr=new int[count*6];
        for(int n=0;n<=count;n++)
        {
            float u=n/(float)count;float spread=width*(1+u*.5f);float z=-u*u*4;
            v[n*2]=new Vector3(-spread/2,-u*height,z);v[n*2+1]=new Vector3(spread/2,-u*height,z);
            uv[n*2]=new Vector2(0,1-u);uv[n*2+1]=new Vector2(1,1-u);
            if(n<count){int k=n*6,j=n*2;tr[k]=j;tr[k+1]=j+1;tr[k+2]=j+2;tr[k+3]=j+1;tr[k+4]=j+3;tr[k+5]=j+2;}
        }
        var mesh=new Mesh{name="Curved falling water mesh"};mesh.vertices=v;mesh.uv=uv;mesh.triangles=tr;mesh.RecalculateNormals();
        var waterfall=MeshObject(root,"Mountain waterfall",mesh,fall);waterfall.transform.position=top;
        var spray=new GameObject("Waterfall spray");spray.transform.SetParent(root);spray.transform.position=top+new Vector3(0,-height,-4);
        var particles=spray.AddComponent<ParticleSystem>();var main=particles.main;main.startLifetime=3;main.startSpeed=1.2f;main.startSize=1.2f;main.startColor=new Color(.7f,.86f,.85f,.18f);main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=particles.emission;emission.rateOverTime=18;var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=width*.65f;
        var mist=SoftMistMaterial();particles.GetComponent<ParticleSystemRenderer>().material=mist;RuntimeResources.Own(spray,mist);RuntimeResources.Own(spray,mist.GetTexture("_BaseMap"));
    }
    void Atmosphere()
    {
        var sky=Resources.Load<Texture>("Sky/MountainSky");
        if(sky!=null){var material=RuntimeResources.Own(gameObject,new Material(Shader.Find("Skybox/Panoramic")));material.SetTexture("_MainTex",sky);material.SetFloat("_Exposure",.8f);material.SetFloat("_Rotation",110);RenderSettings.skybox=material;}
        RenderSettings.fogColor=new Color(.39f,.53f,.52f);RenderSettings.fogDensity=.0045f;
        RenderSettings.ambientSkyColor=new Color(.42f,.53f,.57f);RenderSettings.ambientEquatorColor=new Color(.28f,.36f,.31f);
        var volume=new GameObject("Sanctuary color grade").AddComponent<Volume>();volume.isGlobal=true;volume.profile=RuntimeResources.Own(gameObject,ScriptableObject.CreateInstance<VolumeProfile>());
        var bloom=volume.profile.Add<Bloom>();bloom.intensity.Override(.35f);bloom.threshold.Override(1.1f);bloom.scatter.Override(.6f);
        volume.profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
        var vignette=volume.profile.Add<Vignette>();vignette.intensity.Override(.2f);vignette.smoothness.Override(.65f);
        DynamicGI.UpdateEnvironment();
    }
    GameObject Block(string name,Vector3 p,Vector3 size,Material material)
    {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(root);o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;Destroy(o.GetComponent<Collider>());return o;}
    static GameObject MeshObject(Transform parent,string name,Mesh mesh,Material material)
    {var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=material;RuntimeResources.Own(o,mesh);return o;}
    public static Material Surface(string name,Color tint,string texture=null,float tiling=1)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=tint};m.SetFloat("_Smoothness",.22f);
        if(texture!=null){m.mainTexture=Resources.Load<Texture2D>("Textures/"+texture+"/"+texture+"_diff_1k");m.mainTextureScale=Vector2.one*tiling;var normal=Resources.Load<Texture2D>("Textures/"+texture+"/"+texture+"_nor_gl_1k");if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}}
        return m;
    }
    public static Material WaterMaterial(bool waterfall)
    {var m=new Material(Resources.Load<Shader>("Scenery/FlowingWater"));m.SetColor("_BaseColor",waterfall?new Color(.42f,.65f,.64f,.85f):new Color(.055f,.24f,.25f,.94f));m.SetFloat("_Vertical",waterfall?1:0);return m;}
    public static void WaterPlane(Transform parent,string name,Vector3 p,Vector2 size,Material material)
    {
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-size.x/2,0,-size.y/2),new Vector3(-size.x/2,0,size.y/2),new Vector3(size.x/2,0,size.y/2),new Vector3(size.x/2,0,-size.y/2)};mesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();var o=MeshObject(parent,name,mesh,material);o.transform.position=p;
    }
    public static void Crag(Transform parent,Vector3 p,Vector3 size,Material material)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Sphere);o.name="Grounded cliff rock";o.transform.SetParent(parent);Destroy(o.GetComponent<Collider>());
        var mesh=RuntimeResources.Own(o,Instantiate(o.GetComponent<MeshFilter>().sharedMesh));var v=mesh.vertices;float seed=Random.Range(0,90);
        for(int n=0;n<v.Length;n++){var q=v[n];float k=.83f+Mathf.PerlinNoise(q.x*4+seed,q.z*4+q.y*3+seed)*.38f;v[n]=q*k;}
        mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();o.GetComponent<MeshFilter>().sharedMesh=mesh;o.GetComponent<Renderer>().sharedMaterial=material;
        o.transform.position=p;o.transform.localScale=size;o.transform.rotation=Quaternion.Euler(Random.Range(-7,7),Random.Range(0,360),Random.Range(-7,7));
    }
    public static void Pine(Transform parent,Vector3 p,float height,Material needles,Material bark)
    {
        if(NatureAssets.Tree(parent,p,height))return;
        var tree=new GameObject("Wind pine tree");tree.transform.SetParent(parent);tree.transform.position=p;
        var stem=GameObject.CreatePrimitive(PrimitiveType.Cylinder);stem.name="Pine trunk";stem.transform.SetParent(tree.transform,false);stem.transform.localPosition=Vector3.up*height*.3f;stem.transform.localScale=new Vector3(height*.045f,height*.3f,height*.045f);stem.GetComponent<Renderer>().sharedMaterial=bark;
        var verts=new List<Vector3>();var tris=new List<int>();
        for(int tier=0;tier<7;tier++)
        {
            float y=height*(.17f+tier*.1f),radius=height*(.23f-tier*.026f);int tip=verts.Count;verts.Add(new Vector3(0,y+height*.33f,0));
            for(int n=0;n<12;n++){float a=n/12f*Mathf.PI*2;float r=radius*Random.Range(.87f,1.1f);verts.Add(new Vector3(Mathf.Cos(a)*r,y+Random.Range(-.2f,.2f),Mathf.Sin(a)*r));}
            for(int n=0;n<12;n++){tris.Add(tip);tris.Add(tip+1+(n+1)%12);tris.Add(tip+1+n);}
        }
        var mesh=new Mesh{name="Layered pine boughs"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
        var crown=MeshObject(tree.transform,"Wind pine boughs",mesh,needles);crown.AddComponent<WindMotion>().kind=1;
    }
    static Material SoftMistMaterial()
    {
        var texture=new Texture2D(32,32);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float a=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f);texture.SetPixel(x,y,new Color(1,1,1,a*a));}texture.Apply();
        var material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.SetTexture("_BaseMap",texture);material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;return material;
    }
}
public sealed class SanctuaryCamera : MonoBehaviour
{
    public static SanctuaryCamera I;
    public static Transform LeftDoor, RightDoor;
    public float BlackoutAlpha = 0f;
    public bool Zooming;
    float zoomStart;
    Vector3 startPos;
    System.Action onFinished;
    bool gateSoundPlayed;

    void Awake() { I = this; }
    void OnDestroy() { if(I == this) I = null; }

    void LateUpdate()
    {
        if(!Zooming)
        {
            float t = Time.unscaledTime;
            transform.position = new Vector3(-5 + Mathf.Sin(t * .055f) * 1.8f, 6 + Mathf.Sin(t * .1f) * .25f, -24 + Mathf.Cos(t * .04f) * 1.3f);
            transform.LookAt(new Vector3(12, 9, 35));
            return;
        }

        float progress = Mathf.Clamp01((Time.unscaledTime - zoomStart) / 3.6f);
        float tC = Mathf.SmoothStep(0f, 1f, progress);

        // Spline through stone pathway straight into the gate
        Vector3 p0 = startPos;
        Vector3 p1 = new Vector3(5f, 3.6f, 5f);
        Vector3 p2 = new Vector3(10f, 2.4f, 27f);
        Vector3 p3 = new Vector3(10f, 2.4f, 39f);

        Vector3 curPos;
        if(tC < 0.5f)
        {
            float u = tC * 2f;
            curPos = Vector3.Lerp(Vector3.Lerp(p0, p1, u), Vector3.Lerp(p1, p2, u), u);
        }
        else
        {
            float u = (tC - 0.5f) * 2f;
            curPos = Vector3.Lerp(Vector3.Lerp(p1, p2, u), Vector3.Lerp(p2, p3, u), u);
        }
        transform.position = curPos;
        Vector3 target = Vector3.Lerp(new Vector3(12, 9, 35), new Vector3(10f, 4.2f, 44f), tC);
        transform.LookAt(target);

        // Smooth gate doors swinging open
        if(progress > 0.42f)
        {
            if(!gateSoundPlayed)
            {
                gateSoundPlayed = true;
                GameAudio.I?.Cue("dungeon", new Vector3(10f, 4.5f, 36f));
            }
            float openT = Mathf.SmoothStep(0f, 1f, (progress - 0.42f) / 0.38f);
            if(LeftDoor != null) LeftDoor.localRotation = Quaternion.Euler(0, -openT * 88f, 0);
            if(RightDoor != null) RightDoor.localRotation = Quaternion.Euler(0, openT * 88f, 0);
        }

        // Blackout fade when entering gate portal void
        if(progress > 0.72f)
        {
            BlackoutAlpha = Mathf.Clamp01((progress - 0.72f) / 0.22f);
        }

        if(progress >= 1f && onFinished != null)
        {
            var cb = onFinished;
            onFinished = null;
            cb();
        }
    }

    public void ZoomIntoGate(System.Action callback)
    {
        Zooming = true;
        zoomStart = Time.unscaledTime;
        startPos = transform.position;
        onFinished = callback;
    }
}

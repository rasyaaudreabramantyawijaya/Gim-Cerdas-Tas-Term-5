using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class WorldBuilder : MonoBehaviour
{
    public string World="Frontend";
    Material stone,wood,roof,path,grass,metal,rune,water;
    Transform environment;
    void Awake()
    {
        Random.InitState(42);
        stone=Material("Stone",new Color(.6f,.58f,.51f),"stone_brick_wall_001");
        wood=Material("Timber",new Color(.27f,.18f,.12f));
        wood.mainTexture=Resources.Load<Texture2D>("Nature/NormalTree_Bark");wood.mainTextureScale=new Vector2(1,3);
        roof=Material("Slate roofs",new Color(.36f,.39f,.37f));
        path=Material("Path",new Color(.38f,.34f,.27f),"rocky_terrain");
        grass=Material("Grass",new Color(.21f,.35f,.18f));
        metal=Material("Aged gold",new Color(.65f,.53f,.31f));metal.SetFloat("_Metallic",.7f);metal.SetFloat("_Smoothness",.7f);
        rune=Material("Waystone",new Color(.18f,.6f,.63f));rune.EnableKeyword("_EMISSION");rune.SetColor("_EmissionColor",new Color(.1f,.75f,.8f)*2);
        water=Material("Reflective basin",new Color(.13f,.28f,.31f));water.SetFloat("_Metallic",.65f);water.SetFloat("_Smoothness",.96f);
        environment=new GameObject("Environment — "+World).transform;
        Lighting(World=="Frontend"||World=="EldervaleOverworld");
        if(World=="Frontend") { Frontend();return; }
        if(World=="EldervaleOverworld") Overworld();else Dungeon();
        gameObject.AddComponent<WorldNavigation>().Build();
        if(World=="EldervaleOverworld")TownPopulation();else DungeonEnemies();
        SpawnPlayer();
        var probe=new GameObject("Sun and sky reflection probe").AddComponent<ReflectionProbe>();
        probe.transform.position=new Vector3(0,6,-25);probe.size=new Vector3(180,50,180);
        probe.mode=UnityEngine.Rendering.ReflectionProbeMode.Realtime;probe.refreshMode=UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
        probe.resolution=128;probe.intensity=.45f;
    }
    Material Material(string name,Color color,string texture=null)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        m.SetFloat("_Smoothness",.28f);
        if(texture!=null)
        {
            m.mainTexture=Resources.Load<Texture2D>("Textures/"+texture+"/"+texture+"_diff_1k");
            var normal=Resources.Load<Texture2D>("Textures/"+texture+"/"+texture+"_nor_gl_1k");
            if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
            var ao=Resources.Load<Texture2D>("Textures/"+texture+"/"+texture+"_ao_1k");
            if(ao!=null)m.SetTexture("_OcclusionMap",ao);
            m.mainTextureScale=Vector2.one*2;
        }
        RuntimeResources.Own(gameObject,m);return m;
    }
    GameObject Box(string name,Vector3 pos,Vector3 scale,Material mat,bool collider=true,PrimitiveType type=PrimitiveType.Cube)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(environment);g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;
        if(type==PrimitiveType.Cube&&mat.HasProperty("_BaseMap")&&mat.GetTexture("_BaseMap")!=null){var properties=new MaterialPropertyBlock();properties.SetVector("_BaseMap_ST",new Vector4(Mathf.Max(1,scale.x/2),Mathf.Max(1,(scale.y<.6f?scale.z:scale.y)/2),0,0));g.GetComponent<Renderer>().SetPropertyBlock(properties);}
        if(!collider){var c=g.GetComponent<Collider>();c.enabled=false;Destroy(c);}
        return g;
    }
    void Lighting(bool outside)
    {
        var light=new GameObject("Sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=outside?1.45f:.25f;
        // Portraits use their own studio lights on layer 29; avoid stacking the world sun on them.
        light.cullingMask&=~(1<<29);
        light.color=new Color(1,.9f,.75f);light.transform.rotation=Quaternion.Euler(38,-42,0);light.shadows=LightShadows.Soft;light.shadowStrength=0.65f;RenderSettings.sun=light;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=outside?new Color(.68f,.78f,.88f):new Color(.35f,.42f,.45f);
        RenderSettings.ambientEquatorColor=outside?new Color(.52f,.58f,.55f):new Color(.26f,.30f,.30f);
        RenderSettings.ambientGroundColor=outside?new Color(.38f,.42f,.38f):new Color(.18f,.20f,.18f);
        RenderSettings.reflectionIntensity=.55f;RenderSettings.fog=true;RenderSettings.fogColor=outside?new Color(.55f,.68f,.7f):new Color(.035f,.075f,.085f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=outside?.004f:.015f;
        var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.4f,.58f,.7f));sky.SetFloat("_SunSize",.04f);sky.SetFloat("_AtmosphereThickness",1.1f);RenderSettings.skybox=sky;
        RuntimeResources.Own(gameObject,sky);
        if(outside&&World!="Frontend")
        {
            var texture=Resources.Load<Texture>("Sky/MountainSky");if(texture!=null){var hdr=RuntimeResources.Own(gameObject,new Material(Shader.Find("Skybox/Panoramic")));hdr.SetTexture("_MainTex",texture);hdr.SetFloat("_Exposure",.8f);RenderSettings.skybox=hdr;}
            var volume=new GameObject("Eldervale natural color grade").AddComponent<Volume>();volume.isGlobal=true;volume.profile=RuntimeResources.Own(volume.gameObject,ScriptableObject.CreateInstance<VolumeProfile>());
            volume.profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
            var colorAdjust=volume.profile.Add<ColorAdjustments>();colorAdjust.postExposure.Override(.35f);
            volume.profile.Add<Bloom>().intensity.Override(0);
        }
        QualitySettings.shadowDistance=100;QualitySettings.shadows=UnityEngine.ShadowQuality.All;QualitySettings.antiAliasing=4;
        DynamicGI.UpdateEnvironment();
    }
    void Terrain()
    {
        var data=new TerrainData{heightmapResolution=129,size=new Vector3(200,24,200)};
        var heights=new float[129,129];
        for(int z=0;z<129;z++)for(int x=0;x<129;x++)
        {
            float wx=x/128f*200-100,wz=z/128f*200-100;
            float edge=Mathf.Max(Mathf.Abs(wx),Mathf.Abs(wz));
            float originalHeight=Mathf.SmoothStep(0,.7f,Mathf.InverseLerp(73,100,edge))*(.55f+Mathf.PerlinNoise(x*.1f,z*.1f)*.45f)*24f;
            float worldHeight=originalHeight+ValleyRiver.GroundOffsetAt(wx,wz);
            // A one metre terrain offset preserves every existing ground height
            // while giving the riverbed room to sit below the surrounding land.
            heights[z,x]=Mathf.Clamp01((worldHeight+1f)/24f);
        }
        data.SetHeights(0,0,heights);
        var tl=new TerrainLayer{diffuseTexture=Resources.Load<Texture2D>("Textures/forest_ground_04/forest_ground_04_diff_1k"),tileSize=new Vector2(7,7),diffuseRemapMin=Vector4.zero,diffuseRemapMax=new Vector4(.62f,.88f,.51f,1)};
        data.terrainLayers=new[]{tl};
        var t=UnityEngine.Terrain.CreateTerrainGameObject(data);t.name="Eldervale Hills";t.transform.SetParent(environment);t.transform.position=new Vector3(-100,-1,-100);
        var terrain=t.GetComponent<UnityEngine.Terrain>();terrain.materialTemplate=Resources.Load<Material>("Scenery/RetainedTerrain");terrain.drawInstanced=false;
    }
    void Overworld()
    {
        Terrain();
        var sand=Material("Coastal sand",new Color(.63f,.58f,.4f));
        Box("Small eastern beach",new Vector3(75,.025f,-35),new Vector3(30,.05f,35),sand,false);
        ScenicBackdrop.WaterPlane(environment,"Eastern coast",new Vector3(89,.03f,-38),new Vector2(25,43),ScenicBackdrop.WaterMaterial(false));
        Box("Town boulevard",new Vector3(0,.02f,-30),new Vector3(10,.04f,100),path,false);
        Box("East-west ancient road",new Vector3(0,.02f,0),new Vector3(130,.04f,8),path,false);
        Box("Northern path",new Vector3(0,.03f,33),new Vector3(8,.04f,70),path,false);
        for(int i=0;i<6;i++) House(new Vector3(i%2==0?-20:20,0,-65+(i/2)*16),i);
        Castle(new Vector3(0,0,-15));
        for(int i=0;i<4;i++)Tower(GameFlow.TowerPositions[i],i,i==0);
        for(int i=1;i<4;i++)Gate(GameFlow.TowerPositions[i]+new Vector3(i==1?-7:i==2?7:0,0,i==3?7:0),i,false);
        for(int i=0;i<130;i++)
        {
            float x=Random.Range(-94f,94f),z=Random.Range(-94f,94f);
            if(Mathf.Abs(x)<38&&z<12 || Mathf.Abs(z)<9 || Mathf.Abs(x)<10)continue;
            var treeSite=new Vector3(x,0,z);
            if(ValleyRiver.IsNearWater(treeSite,5f)||IsCoastalWater(treeSite,3f))continue;
            Tree(new Vector3(x,0,z),Random.Range(3.8f,7));
        }
        for(int i=0;i<240;i++)
        {
            Vector3 p=new Vector3(Random.Range(-85,85),0,Random.Range(-80,80));
            if(Mathf.Abs(p.x)<34&&p.z<12||Mathf.Abs(p.z)<6||Mathf.Abs(p.x)<6)continue;
            if(ValleyRiver.IsNearWater(p,.35f)||IsCoastalWater(p,.35f))continue;
            p.y=TerrainWorldHeight(p);
            if(!NatureAssets.Grass(environment,p,.65f)){var tuft=Box("Wind grass",p+Vector3.up*.25f,new Vector3(.12f,.5f,.5f),grass,false);tuft.AddComponent<WindMotion>();}
        }
        var merchant=Box("Market canopy",new Vector3(9,3,-53),new Vector3(5,.2f,4),roof);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Market timber post",new Vector3(9+x*2.25f,1.5f,-53+z*1.75f),new Vector3(.2f,3,.2f),wood);
        Box("Merchant counter",new Vector3(9,.65f,-52),new Vector3(4,1.3f,1),wood);
        for(int n=0;n<5;n++){Box("Potion display",new Vector3(7.6f+n*.5f,1.52f,-52),new Vector3(.15f,.22f,.15f),n%2==0?rune:metal,false,PrimitiveType.Cylinder);}
        Use(UseKind.Shop,0,"Beli senjata, minuman atau upgrade",new Vector3(9,0,-54));
        Use(UseKind.Altar,0,"Serahkan Relic of Veyr",new Vector3(-5,0,-53));
        Use(UseKind.Reward,0,"Klaim hadiah level — Warden",new Vector3(-7,0,-53));
        Box("Camp altar",new Vector3(-5,.5f,-53),new Vector3(1.7f,1,1.7f),metal);
        Box("Reflective fountain basin",new Vector3(0,.18f,-34),new Vector3(5,.25f,4),water);
        for(int i=0;i<4;i++)Box("Fountain rim",new Vector3(i<2?(i==0?-2.6f:2.6f):0,.28f,i<2?-34:(i==2?-36.1f:-31.9f)),new Vector3(i<2?.25f:5.4f,.5f,i<2?4.5f:.25f),stone);

        // Landmark Features
        var riverRock=Material("River rock",new Color(.62f,.62f,.56f),"mossy_rock");
        ValleyRiver.Build(environment, stone, wood, riverRock);
        ValleyRailway.Build(environment, wood, metal, stone);
        ValleyFarmland.Build(environment, wood, stone, path, grass);

        // --- Street Food and Vegetable Merchants ---
        // Bibi Elen's Fresh Produce Stall
        Box("Produce stall canopy", new Vector3(-9, 3, -53), new Vector3(5, .2f, 4), grass);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Produce timber post",new Vector3(-9+x*2.25f,1.5f,-53+z*1.75f),new Vector3(.2f,3,.2f),wood);
        Box("Produce counter",new Vector3(-9,.65f,-52),new Vector3(4,1.3f,1),wood);
        var vegMat=Material("Produce Veg",new Color(.85f,.42f,.12f));
        for(int n=0;n<4;n++) Box("Produce crate",new Vector3(-10.2f+n*.85f,1.48f,-52),new Vector3(.35f,.3f,.35f),vegMat,false,PrimitiveType.Sphere);
        var vegUse=Use(UseKind.Drink,10,"Beli buah & sayur segar dari Bibi Elen",new Vector3(-9,0,-54));
        vegUse.DrinkName="Buah Segar";

        // Paman Tomo's Street Snack Cart
        Box("Snack cart body",new Vector3(0,.6f,-48),new Vector3(3.2f,1.1f,1.8f),wood);
        Box("Cart wheel L",new Vector3(-1.65f,.45f,-48),new Vector3(.1f,.9f,.9f),metal,false,PrimitiveType.Cylinder);
        Box("Cart wheel R",new Vector3(1.65f,.45f,-48),new Vector3(.1f,.9f,.9f),metal,false,PrimitiveType.Cylinder);
        Box("Steaming broth pot",new Vector3(-.6f,1.3f,-48),new Vector3(.45f,.35f,.45f),metal,false,PrimitiveType.Cylinder);
        Box("Steamer basket",new Vector3(.6f,1.3f,-48),new Vector3(.5f,.3f,.5f),wood,false,PrimitiveType.Cylinder);
        var snackUse=Use(UseKind.Drink,11,"Beli teh manis hangat & bakpao dari Paman Tomo",new Vector3(0,0,-50));
        snackUse.DrinkName="Teh Manis & Bakpao";
    }
    void House(Vector3 p,int id)
    {
        Box("House "+id+" floor",p+new Vector3(0,.1f,0),new Vector3(10,.2f,10),stone);
        Box("House back",p+new Vector3(0,2,4.8f),new Vector3(10,4,.3f),stone);
        Box("House side",p+new Vector3(-4.8f,2,0),new Vector3(.3f,4,10),stone);
        Box("House side",p+new Vector3(4.8f,2,0),new Vector3(.3f,4,10),stone);
        Box("House entrance left",p+new Vector3(-3.2f,2,-4.8f),new Vector3(3.6f,4,.3f),stone);
        Box("House entrance right",p+new Vector3(3.2f,2,-4.8f),new Vector3(3.6f,4,.3f),stone);
        var a=Box("Pitched roof",p+new Vector3(-2.6f,4.6f,0),new Vector3(6,.25f,11),roof);a.transform.rotation=Quaternion.Euler(0,0,25);
        var b=Box("Pitched roof",p+new Vector3(2.6f,4.6f,0),new Vector3(6,.25f,11),roof);b.transform.rotation=Quaternion.Euler(0,0,-25);
        // Close the gables and join roof edges to wall frames; no hovering roof plates.
        for(int side=-1;side<=1;side+=2)
        {
            var g=new GameObject("House closed gable");g.transform.SetParent(environment);g.transform.position=p+new Vector3(0,0,side*4.8f);
            var mesh=RuntimeResources.Own(g,new Mesh());mesh.vertices=new[]{new Vector3(-5,3.45f,0),new Vector3(5,3.45f,0),new Vector3(0,5.85f,0)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up};mesh.triangles=new[]{0,1,2,2,1,0};mesh.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=stone;
            Box("House timber eave",p+new Vector3(0,3.8f,side*4.95f),new Vector3(10,.24f,.22f),wood);
            for(int x=-1;x<=1;x+=2)Box("House corner frame",p+new Vector3(x*4.8f,1.95f,side*4.95f),new Vector3(.22f,3.9f,.22f),wood);
        }
        Box("Chimney",p+new Vector3(3.3f,4.5f,2.2f),new Vector3(1.1f,3,1.1f),stone);
        for(int side=-1;side<=1;side+=2)
        {
            Box("Window recess",p+new Vector3(side*4.99f,2.2f,0),new Vector3(.08f,1.3f,1.6f),wood,false);
            Box("Window warm glass",p+new Vector3(side*5.04f,2.2f,0),new Vector3(.04f,1.05f,1.35f),metal,false);
            Box("Window mullion",p+new Vector3(side*5.07f,2.2f,0),new Vector3(.08f,1.3f,.08f),wood,false);
        }
        // --- Interior lighting: warm fireplace glow + ceiling lantern ---
        var fireGlow=new GameObject("Fireplace warm glow").AddComponent<Light>();
        fireGlow.transform.SetParent(environment);fireGlow.transform.position=p+new Vector3(3.1f,1.2f,2.2f);
        fireGlow.type=LightType.Point;fireGlow.color=new Color(1f,.7f,.36f);fireGlow.range=9f;fireGlow.intensity=1.8f;fireGlow.shadows=LightShadows.Soft;
        var ceilingLamp=new GameObject("Ceiling lantern").AddComponent<Light>();
        ceilingLamp.transform.SetParent(environment);ceilingLamp.transform.position=p+new Vector3(0,3.3f,0);
        ceilingLamp.type=LightType.Point;ceilingLamp.color=new Color(1f,.85f,.6f);ceilingLamp.range=11f;ceilingLamp.intensity=1.2f;
        // Lantern fixture visual
        Box("Lantern chain",p+new Vector3(0,3.7f,0),new Vector3(.06f,.8f,.06f),metal,false);
        Box("Lantern body",p+new Vector3(0,3.35f,0),new Vector3(.35f,.3f,.35f),metal,false);
        // Fireplace visual (stone hearth with emissive coals)
        Box("Fireplace hearth",p+new Vector3(3.1f,.35f,2.2f),new Vector3(1.8f,.7f,1.2f),stone);
        Box("Fireplace back",p+new Vector3(3.1f,1.2f,2.85f),new Vector3(1.8f,1.7f,.15f),stone);
        var coals=Box("Glowing coals",p+new Vector3(3.1f,.5f,2.2f),new Vector3(1.2f,.15f,.7f),rune,false);
        // --- Furniture: Bed on left side ---
        var fabric=RuntimeResources.Own(gameObject,new Material(Shader.Find("Universal Render Pipeline/Lit")));
        fabric.color=id%2==0?new Color(.35f,.18f,.16f):new Color(.2f,.25f,.38f);fabric.SetFloat("_Smoothness",.15f);
        Box("Bed frame",p+new Vector3(-3.2f,.45f,2.8f),new Vector3(2.2f,.9f,3.2f),wood);
        Box("Mattress",p+new Vector3(-3.2f,.72f,2.8f),new Vector3(1.9f,.35f,2.9f),fabric,false);
        Box("Pillow",p+new Vector3(-3.2f,.98f,4.0f),new Vector3(1.2f,.22f,.6f),fabric,false);
        Box("Blanket",p+new Vector3(-3.2f,.84f,2.0f),new Vector3(1.85f,.12f,1.8f),fabric,false);
        // Bedside table
        Box("Nightstand",p+new Vector3(-1.7f,.42f,3.8f),new Vector3(.7f,.84f,.7f),wood);
        Box("Water jug",p+new Vector3(-1.7f,1.0f,3.8f),new Vector3(.18f,.32f,.18f),stone,false,PrimitiveType.Cylinder);
        // --- Furniture: Coffee/dining table (center) ---
        Box("Coffee table",p+new Vector3(0,.5f,0),new Vector3(2.6f,1f,1.4f),wood);
        for(int leg=-1;leg<=1;leg+=2) Box("Table leg",p+new Vector3(leg*1.0f,.27f,0),new Vector3(.14f,.54f,.14f),wood,false);
        // Coffee cups and teapot
        Box("Teapot",p+new Vector3(.3f,1.12f,0),new Vector3(.26f,.2f,.26f),stone,false,PrimitiveType.Cylinder);
        Box("Coffee cup 1",p+new Vector3(-.6f,1.08f,-.3f),new Vector3(.12f,.14f,.12f),stone,false,PrimitiveType.Cylinder);
        Box("Coffee cup 2",p+new Vector3(-.6f,1.08f,.3f),new Vector3(.12f,.14f,.12f),stone,false,PrimitiveType.Cylinder);
        // Chair left
        Box("Chair seat",p+new Vector3(0,.42f,-1.3f),new Vector3(.8f,.08f,.8f),wood);
        for(int lx=-1;lx<=1;lx+=2)for(int lz=-1;lz<=1;lz+=2)Box("Chair leg",p+new Vector3(lx*.3f,.21f,-1.3f+lz*.3f),new Vector3(.08f,.42f,.08f),wood,false);
        Box("Chair back",p+new Vector3(0,.7f,-1.65f),new Vector3(.8f,.56f,.08f),wood);
        // Chair right
        Box("Chair seat",p+new Vector3(0,.42f,1.3f),new Vector3(.8f,.08f,.8f),wood);
        for(int lx=-1;lx<=1;lx+=2)for(int lz=-1;lz<=1;lz+=2)Box("Chair leg",p+new Vector3(lx*.3f,.21f,1.3f+lz*.3f),new Vector3(.08f,.42f,.08f),wood,false);
        Box("Chair back",p+new Vector3(0,.7f,1.65f),new Vector3(.8f,.56f,.08f),wood);
        // --- Furniture: Bookshelf on back wall ---
        Box("Bookshelf frame",p+new Vector3(-1.5f,1.5f,4.3f),new Vector3(2.2f,3,.5f),wood);
        for(int shelf=0;shelf<3;shelf++)Box("Shelf plank",p+new Vector3(-1.5f,.6f+shelf*1.0f,4.3f),new Vector3(2.1f,.1f,.48f),wood,false);
        var bookColor=new Color[]{new Color(.55f,.15f,.12f),new Color(.12f,.35f,.2f),new Color(.2f,.18f,.4f),new Color(.6f,.5f,.15f)};
        for(int bk=0;bk<8;bk++){var bc=RuntimeResources.Own(gameObject,new Material(Shader.Find("Universal Render Pipeline/Lit")));bc.color=bookColor[bk%4];bc.SetFloat("_Smoothness",.1f);
        Box("Book",p+new Vector3(-2.3f+bk*.25f,.85f+(bk<4?0:1.0f),4.3f),new Vector3(.12f,.55f,.35f),bc,false);}
        // --- Furniture: Storage chest ---
        Box("Storage chest",p+new Vector3(3.5f,.42f,-2.5f),new Vector3(1.6f,.84f,1),wood);
        Box("Chest lid",p+new Vector3(3.5f,.87f,-2.5f),new Vector3(1.65f,.12f,1.05f),wood);
        Box("Chest iron band",p+new Vector3(3.5f,.42f,-2.5f),new Vector3(1.62f,.1f,1.02f),metal,false);
        // --- Rug under coffee table ---
        var rugMat=RuntimeResources.Own(gameObject,new Material(Shader.Find("Universal Render Pipeline/Lit")));
        rugMat.color=id%2==0?new Color(.45f,.22f,.15f):new Color(.2f,.3f,.18f);rugMat.SetFloat("_Smoothness",.08f);
        Box("Floor rug",p+new Vector3(0,.22f,0),new Vector3(4,.02f,3),rugMat,false);
        // --- Drink and rest interaction points ---
        var coffeeUse=Use(UseKind.Drink,id,"Minum kopi hangat",p+new Vector3(-.5f,0,-.8f));coffeeUse.DrinkName="Kopi";
        var waterUse=Use(UseKind.Drink,id,"Minum air segar",p+new Vector3(-1.7f,0,3.2f));waterUse.DrinkName="Air";
        Use(UseKind.Rest,id,"Beristirahat di kasur",p+new Vector3(-3.2f,0,2.2f));
        Use(UseKind.Talk,id,"Sapa penghuni rumah",p+new Vector3(0,0,1));
    }
    void Castle(Vector3 p)
    {
        Box("Castle courtyard",p+new Vector3(0,.04f,0),new Vector3(29,.08f,24),path);
        Box("Castle west wall",p+new Vector3(-15,3,0),new Vector3(1,6,26),stone);
        Box("Castle east wall",p+new Vector3(15,3,0),new Vector3(1,6,26),stone);
        Box("Castle north wall",p+new Vector3(0,3,13),new Vector3(31,6,1),stone);
        Box("Castle gate left",p+new Vector3(-10,3,-13),new Vector3(10,6,1),stone);
        Box("Castle gate right",p+new Vector3(10,3,-13),new Vector3(10,6,1),stone);
        Box("Castle arch",p+new Vector3(0,6,-13),new Vector3(12,1,1.2f),stone);

        // Crenellations make the curtain walls read as a fortress from the approach.
        for(int i=0;i<13;i++)
        {
            float offset=-13f+i*(26f/12f);
            Box("Castle north battlement",p+new Vector3(offset,6.55f,13),new Vector3(1.05f,1.1f,.9f),stone);
            if(i<4)
            {
                float gateOffset=-13f+i*2.2f;
                Box("Castle west gate battlement",p+new Vector3(gateOffset,6.55f,-13),new Vector3(1.05f,1.1f,.9f),stone);
                Box("Castle east gate battlement",p+new Vector3(-gateOffset,6.55f,-13),new Vector3(1.05f,1.1f,.9f),stone);
            }
        }
        for(int i=0;i<11;i++)
        {
            float z=-11f+i*2.2f;
            Box("Castle west battlement",p+new Vector3(-15,6.55f,z),new Vector3(.9f,1.1f,1.05f),stone);
            Box("Castle east battlement",p+new Vector3(15,6.55f,z),new Vector3(.9f,1.1f,1.05f),stone);
        }

        // Corner turrets and the taller gatehouse frame the entrance.
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            CastleTower(p+new Vector3(x*15,0,z*13),"Castle corner tower",5.6f,10.5f);
        CastleTower(p+new Vector3(-5.7f,0,-13),"Castle gatehouse west tower",3.8f,11.5f);
        CastleTower(p+new Vector3(5.7f,0,-13),"Castle gatehouse east tower",3.8f,11.5f);
        Box("Castle gatehouse bridge",p+new Vector3(0,8.4f,-13),new Vector3(7.8f,4.4f,4),stone);
        var banner=Material("Castle banners",new Color(.48f,.12f,.08f));
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*5.7f;
            Box("Castle gatehouse banner pole",p+new Vector3(x,9.2f,-15.05f),new Vector3(.12f,3.6f,.12f),metal,false);
            Box("Castle gatehouse banner",p+new Vector3(x,7.5f,-15.08f),new Vector3(1.45f,2.4f,.12f),banner,false);
        }

        // A roofed keep rises behind the rear curtain wall, visible through the gate.
        Box("Castle keep foundation",p+new Vector3(0,.4f,9),new Vector3(13,.8f,14),stone);
        Box("Castle keep",p+new Vector3(0,6.25f,9),new Vector3(11.5f,12.5f,12),stone);
        Box("Castle keep entrance",p+new Vector3(0,2.05f,2.88f),new Vector3(2.3f,3.9f,.14f),wood,false);
        for(int side=-1;side<=1;side+=2)
        {
            Box("Castle keep doorway pier",p+new Vector3(side*1.42f,2.1f,2.72f),new Vector3(.42f,4.2f,.4f),stone,false);
            for(int row=0;row<2;row++)
                Box("Castle keep arrow slit",p+new Vector3(side*3.65f,7.5f+row*2.6f,2.9f),new Vector3(.48f,1.55f,.12f),wood,false);
        }
        Box("Castle keep doorway lintel",p+new Vector3(0,4.25f,2.72f),new Vector3(3.2f,.48f,.4f),stone,false);
        var leftRoof=Box("Castle keep west roof",p+new Vector3(-3.15f,13.35f,9),new Vector3(7.5f,.65f,14),roof,false);
        leftRoof.transform.rotation=Quaternion.Euler(0,0,35);
        var rightRoof=Box("Castle keep east roof",p+new Vector3(3.15f,13.35f,9),new Vector3(7.5f,.65f,14),roof,false);
        rightRoof.transform.rotation=Quaternion.Euler(0,0,-35);
        Box("Castle keep ridge cap",p+new Vector3(0,15.35f,9),new Vector3(.5f,.45f,14.2f),roof,false);
        Box("Castle keep flag pole",p+new Vector3(0,17.1f,9),new Vector3(.12f,3.2f,.12f),metal,false);
        Box("Castle keep flag",p+new Vector3(.8f,17.55f,9),new Vector3(1.55f,1.8f,.12f),banner,false);
        Use(UseKind.Talk,0,"Bicara dengan Warden istana",p+new Vector3(0,0,1.5f));
    }
    void CastleTower(Vector3 p,string name,float width,float height)
    {
        Box(name+" footing",p+Vector3.up*.35f,new Vector3(width+1,.7f,width+1),stone);
        Box(name,p+Vector3.up*(height*.5f),new Vector3(width*.86f,height,width*.86f),stone);
        Box(name+" crown",p+Vector3.up*(height+.25f),new Vector3(width+.45f,.5f,width+.45f),stone);
        float edge=(width+.45f)*.46f;
        float offset=width*.25f;
        float size=width*.22f;
        for(int i=-1;i<=1;i++)
        {
            Box(name+" merlon",p+new Vector3(i*offset,height+1.05f,edge),new Vector3(size,1.1f,size),stone);
            Box(name+" merlon",p+new Vector3(i*offset,height+1.05f,-edge),new Vector3(size,1.1f,size),stone);
            if(i!=-1&&i!=1)
            {
                Box(name+" merlon",p+new Vector3(edge,height+1.05f,i*offset),new Vector3(size,1.1f,size),stone);
                Box(name+" merlon",p+new Vector3(-edge,height+1.05f,i*offset),new Vector3(size,1.1f,size),stone);
            }
        }
    }
    void Tower(Vector3 p,int id,bool grand)
    {
        Box("Waytower "+id,p+Vector3.up*4,new Vector3(3,8,3),stone);
        Box("Waytower terrace",p+Vector3.up*8.2f,new Vector3(6,.4f,6),stone);
        Box("Waystone crystal",p+new Vector3(0,1.3f,-2.4f),new Vector3(.7f,1.7f,.7f),rune,false,PrimitiveType.Cylinder);
        Box("Waystone plinth",p+new Vector3(0,.3f,-2.4f),new Vector3(1.25f,.6f,1.25f),stone);
        Use(UseKind.Tower,id,grand?"Grand Tower: pulihkan HP & aktifkan":"Aktifkan menara "+GameFlow.Places[id],p+new Vector3(0,0,-2.8f));
        for(int n=0;n<24;n++)
        {
            float angle=n*15*Mathf.Deg2Rad;
            var step=Box("Tower spiral step",p+new Vector3(Mathf.Cos(angle)*2.6f,.16f+n*.34f,Mathf.Sin(angle)*2.6f),new Vector3(1.9f,.32f,1.05f),stone);
            step.transform.rotation=Quaternion.Euler(0,-n*15,0);
            var beam=Box("Stair cantilever beam",p+new Vector3(Mathf.Cos(angle)*1.6f,n*.34f-.12f,Mathf.Sin(angle)*1.6f),new Vector3(3,.18f,.3f),wood,false);beam.transform.rotation=Quaternion.Euler(0,-n*15,0);
            float height=.16f+n*.34f;
            if(n%3==0)Box("Stair structural pier",p+new Vector3(Mathf.Cos(angle)*2.6f,height*.5f,Mathf.Sin(angle)*2.6f),new Vector3(.28f,Mathf.Max(.16f,height),.28f),stone);
        }
        var l=new GameObject("Waystone glow").AddComponent<Light>();l.transform.position=p+Vector3.up*3;l.color=Color.cyan;l.range=8;l.intensity=1.4f;
    }
    void Gate(Vector3 p,int id,bool exit)
    {
        Box("Gate pillar left",p+new Vector3(-2.4f,3,0),new Vector3(1.2f,6,1.3f),stone);
        Box("Gate pillar right",p+new Vector3(2.4f,3,0),new Vector3(1.2f,6,1.3f),stone);
        Box("Gate lintel",p+new Vector3(0,6,0),new Vector3(6,1.1f,1.5f),stone);
        Box("Gate threshold",p+new Vector3(0,.12f,0),new Vector3(6,.24f,2.4f),stone);
        for(int side=-1;side<=1;side+=2)Box("Gate buttress",p+new Vector3(side*3,.9f,.5f),new Vector3(1.4f,1.8f,2.3f),stone);
        var portalMaterial=RuntimeResources.Own(gameObject,ScenicBackdrop.WaterMaterial(true));portalMaterial.SetColor("_BaseColor",new Color(.08f,.45f,.48f,.75f));
        Box("Living portal",p+new Vector3(0,2.5f,.1f),new Vector3(3.5f,5,.08f),portalMaterial,false);
        var seal=new GameObject("Carved portal seal");seal.transform.SetParent(environment);var line=seal.AddComponent<LineRenderer>();line.sharedMaterial=rune;line.positionCount=65;line.widthMultiplier=.045f;
        for(int n=0;n<65;n++){float a=n/64f*Mathf.PI*2;line.SetPosition(n,p+new Vector3(Mathf.Cos(a)*1.25f,2.7f+Mathf.Sin(a)*1.25f,-.02f));}
        Use(exit?UseKind.Exit:UseKind.Gate,id,exit?"Kembali ke Eldervale":"Masuki "+GameFlow.Places[id],p+new Vector3(0,0,-1.5f));
    }
    void Tree(Vector3 p,float height)
    {
        p.y=TerrainWorldHeight(p);
        ScenicBackdrop.Pine(environment,p,height*1.35f,grass,wood);
    }
    static float TerrainWorldHeight(Vector3 worldPosition)
    {
        var terrain=UnityEngine.Terrain.activeTerrain;
        return terrain!=null?terrain.transform.position.y+terrain.SampleHeight(worldPosition):0f;
    }
    static bool IsCoastalWater(Vector3 p,float margin)
        =>Mathf.Abs(p.x-89f)<=12.5f+margin&&Mathf.Abs(p.z+38f)<=21.5f+margin;
    void Dungeon()
    {
        var acoustics=new GameObject("Stone chamber acoustics").AddComponent<AudioReverbZone>();
        acoustics.transform.position=new Vector3(0,3,7);acoustics.minDistance=42;acoustics.maxDistance=65;acoustics.reverbPreset=AudioReverbPreset.StoneCorridor;
        int id=World=="AshenCrypt"?1:World=="WhisperingGrotto"?2:3;
        Box("Dungeon floor",new Vector3(0,-.3f,7),new Vector3(36,.6f,58),stone);
        Box("West dungeon wall",new Vector3(-18,4,7),new Vector3(1,8,58),stone);
        Box("East dungeon wall",new Vector3(18,4,7),new Vector3(1,8,58),stone);
        Box("Far dungeon wall",new Vector3(0,4,36),new Vector3(36,8,1),stone);
        Box("Near dungeon wall",new Vector3(0,4,-22),new Vector3(36,8,1),stone);
        for(int n=0;n<8;n++)
        {
            Vector3 p=new Vector3(n%2==0?-13:13,0,-10+n/2*12);
            Box("Dungeon pillar",p+Vector3.up*3,new Vector3(1.2f,6,1.2f),stone);
            Box("Pillar capital",p+Vector3.up*6,new Vector3(2,1,2),stone);
            Box("Pillar foot",p+Vector3.up*.2f,new Vector3(1.8f,.4f,1.8f),stone);
            Box("Torch iron bracket",p+new Vector3(n%2==0?.8f:-.8f,2.7f,0),new Vector3(.75f,.12f,.12f),metal,false);
            var torch=Box("Rune torch",p+new Vector3(n%2==0?1:-1,3,0),Vector3.one*.3f,rune,false,PrimitiveType.Sphere);
            var l=torch.AddComponent<Light>();l.range=13;l.intensity=2;l.color=id==1?new Color(1,.55f,.24f):new Color(.25f,.6f,1);
        }
        if(id!=3)
        {
            Box("Room divider left",new Vector3(-11,2,8),new Vector3(14,4,1),stone);
            Box("Room divider right",new Vector3(11,2,8),new Vector3(14,4,1),stone);
        }
        Gate(new Vector3(0,0,-19),id,true);
        var relic=Box(id==3?"Relic of Veyr":"Ancient Sigil",new Vector3(0,1.65f,29),new Vector3(.45f,.65f,.45f),metal,false,PrimitiveType.Cylinder);
        Use(id==3?UseKind.Artifact:UseKind.Sigil,id,id==3?"Ambil Relic of Veyr":"Ambil sigil",new Vector3(0,0,28));
        Box("Relic altar",new Vector3(0,.5f,29),new Vector3(3,1,3),stone);
    }
    EnemyAgent Person(string name,Vector3 p,bool civilian,bool ranged=false,bool boss=false,bool trainer=false)
    {
        var g=new GameObject(name);g.transform.position=p;g.layer=9;
        var c=g.AddComponent<CapsuleCollider>();c.height=1.8f;c.radius=.35f;c.center=Vector3.up*.9f;
        var e=g.AddComponent<EnemyAgent>();e.Civilian=civilian;e.Ranged=ranged;e.Boss=boss;e.Trainer=trainer;
        g.transform.forward=Vector3.back;
        return e;
    }
    void TownPopulation()
    {
        for(int i=0;i<10;i++)Person("Warga "+(i+1),new Vector3((i%3-1)*5,0,-65+i*4),true);
        Person("Mira sang pedagang",new Vector3(9,0,-50.7f),true);
        Person("Warden hadiah",new Vector3(-7,0,-51),true);
        var trainer=Person("Warden Duel",new Vector3(-8,0,-10),false,false,false,true);
        var use=trainer.gameObject.AddComponent<Interactable>();use.Kind=UseKind.Duel;use.Label="Mulai / hentikan latihan duel";
        var outsideBoss=Person("Sentinel of the Hollow",new Vector3(36,0,38),false,false,true);
        outsideBoss.HP=180;outsideBoss.MaxHP=180;
        var introduction=outsideBoss.gameObject.AddComponent<BossIntroduction>();introduction.Speaker="Sentinel of the Hollow";introduction.Distance=12;introduction.Story="Jalur pegunungan ini telah lama kututup. Kau membawa sumpah para pemburu? Perlihatkan keberanianmu, maka hutan akan mengingat namamu.";
        Person("Ravager luar",new Vector3(-39,0,20),false);
        Person("Arcanist luar",new Vector3(42,0,19),false,true);
    }
    void DungeonEnemies()
    {
        if(World=="VeyrSanctum")
        {
            if(GameFlow.I.State.guardian)return;
            var boss=Person("Guardian of Veyr",new Vector3(0,0,18),false,false,true);
            boss.SanctumGuardian=true;
            boss.gameObject.AddComponent<BossIntroduction>();
        }
        else
        {
            Person("Ravager",new Vector3(-5,0,0),false,World=="WhisperingGrotto");
            Person("Arcanist",new Vector3(6,0,18),false,true);
        }
    }
    Interactable Use(UseKind kind,int id,string label,Vector3 p)
    {
        var o=new GameObject(label);o.transform.position=p;var use=o.AddComponent<Interactable>();use.Kind=kind;use.Id=id;use.Label=label;return use;
    }
    void SpawnPlayer()
    {
        Vector3 p=World=="EldervaleOverworld"?GameFlow.TowerPositions[Mathf.Clamp(GameFlow.I.SpawnId,0,3)]+new Vector3(0,1,-6):new Vector3(0,1,-13);
        var g=new GameObject("Hunter");g.layer=8;g.transform.position=p;g.AddComponent<HunterPlayer>();
    }
    void Frontend()
    {
        gameObject.AddComponent<ScenicBackdrop>().Build();
    }
}


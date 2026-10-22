using System;
using UnityEngine;

public sealed class Presentation : MonoBehaviour
{
    GUIStyle title,heading,body,small,button;
    Texture2D white,glow,menuShade;
    int selected,hover=-1;
    float sliderAt;
    CharacterPortrait portrait;
    WorldMapPreview terrainMap;
    public int SelectedCharacter=>selected;
    void Awake(){portrait=gameObject.AddComponent<CharacterPortrait>();terrainMap=gameObject.AddComponent<WorldMapPreview>();}
    readonly Color ink=new Color(.035f,.065f,.08f,.94f),gold=new Color(.88f,.73f,.47f),cyan=new Color(.35f,.81f,.8f);
    void Styles()
    {
        if(title!=null)return;
        white=Texture2D.whiteTexture;
        glow=new Texture2D(64,64);
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float a=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/32);glow.SetPixel(x,y,new Color(1,1,1,a*a));}glow.Apply();
        var font=Resources.Load<Font>("Fonts/Inter-Regular");
        menuShade=new Texture2D(128,1);menuShade.wrapMode=TextureWrapMode.Clamp;
        for(int x=0;x<128;x++)menuShade.SetPixel(x,0,new Color(.015f,.035f,.034f,Mathf.Pow(1-x/127f,.7f)*.93f));menuShade.Apply();
        title=new GUIStyle{font=Resources.Load<Font>("Fonts/InterDisplay-Light"),fontSize=76,fontStyle=FontStyle.Normal,normal={textColor=new Color(.96f,.96f,.91f)},wordWrap=true};
        heading=new GUIStyle(title){font=Resources.Load<Font>("Fonts/Inter-SemiBold"),fontSize=23};body=new GUIStyle(title){font=font,fontSize=18};
        small=new GUIStyle(body){fontSize=15,normal={textColor=new Color(.76f,.81f,.77f)}};
        button=new GUIStyle(body){alignment=TextAnchor.MiddleCenter,normal={textColor=Color.white},wordWrap=true};
    }
    void Rect(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,white);GUI.color=old;}
    void Text(string text,float x,float y,float w,float h,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),text,style??body);}
    bool Button(string text,float x,float y,float w=300,float h=50,bool available=true)
    {
        var r=new Rect(x,y,w,h);bool over=r.Contains(Event.current.mousePosition)&&available;
        int id=(int)(x+y*2000);
        if(over&&hover!=id){hover=id;GameAudio.I?.Cue("hover");}
        Rect(r,available?over?new Color(.26f,.37f,.33f,.88f):new Color(.065f,.11f,.1f,.72f):new Color(.08f,.1f,.09f,.55f));
        Rect(new Rect(x,y+h-1,w,1),available?new Color(gold.r,gold.g,gold.b,over?.85f:.28f):new Color(.3f,.33f,.3f));
        GUI.enabled=available;
        bool clicked=GUI.Button(r,text,button);GUI.enabled=true;
        if(clicked)GameAudio.I?.Cue("click");
        return clicked;
    }
    void Panel(float x,float y,float w,float h,string label)
    {Rect(new Rect(x,y,w,h),ink);Rect(new Rect(x,y,w,1),new Color(gold.r,gold.g,gold.b,.65f));Text(label,x+28,y+22,w-56,60,heading);}
    void OnGUI()
    {
        var f=GameFlow.I;if(f==null)return;Styles();
        float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
        if(SanctuaryCamera.I != null && SanctuaryCamera.I.BlackoutAlpha > 0f)
        {
            Rect(new Rect(0,0,1600,900), new Color(0,0,0,SanctuaryCamera.I.BlackoutAlpha));
        }
        if(f.Screen==GameScreen.Loading){Loading(f);return;}
        if(f.Screen==GameScreen.Menu||f.Screen==GameScreen.Select)Frontend(f);
        else HUD(f);
        switch(f.Screen)
        {
            case GameScreen.Map:Map(f);break;
            case GameScreen.Party:
                Panel(160,130,1280,650,"PARTY SETUP");
                Text("1 / 2 / 3 beralih saat bermain. HP setiap karakter terpisah.",205,212,900,60,small);
                for(int slot=0;slot<3;slot++)
                {
                    int ch=f.State.party[slot];float x=205+slot*255;
                    Text("SLOT 0"+(slot+1),x,315,220,40,small);
                    Text(GameFlow.Names[ch],x,362,220,60,heading);
                    Text(Mathf.CeilToInt(ch==f.State.character?f.Player.HP:f.State.health[ch])+" / 100 HP"+(ch==f.State.character?"  ·  AKTIF":""),x,418,220,35,small);
                    if(Button("Ganti anggota",x,490,220))f.CyclePartySlot(slot);
                    if(Button(ch==f.State.character?"Sedang digunakan":"Gunakan",x,555,220,50,ch!=f.State.character))f.SwitchCharacter(slot);
                }
                if(portrait!=null&&portrait.Texture!=null)GUI.DrawTexture(new Rect(1045,230,355,450),portrait.Texture,ScaleMode.ScaleToFit,true);
                if(Button("Kembali",640,690))f.SetScreen(GameScreen.Playing);
                break;
            case GameScreen.Pause:
                Panel(520,190,560,510,"EKSPEDISI DIJEDA");
                if(Button("Lanjutkan",650,300))f.SetScreen(GameScreen.Playing);
                if(Button("Pengaturan",650,370))f.OpenSettings();
                if(Button("Simpan dan menu",650,440)){f.Save();f.Menu();}
                Text("WASD gerak · Panah kamera · Enter lompat\nEnter dua kali di ketinggian: melayang\nKlik kiri serang · Q skill · Tab senjata · H minum",565,540,490,120,small);
                break;
            case GameScreen.Settings:Settings(f);break;
            case GameScreen.Shop:
                Panel(430,160,740,560,"PASAR ELDERVALE");
                Text("Koin: "+f.State.coins+"     Upgrade: "+f.State.upgrade+"/5",470,245,650,50);
                if(Button("Minuman pemulih — 25 koin",540,320,520))f.Buy(0);
                if(Button("Senjata berikutnya — 40 koin",540,390,520))f.Buy(1);
                if(Button("Upgrade damage — "+(60+f.State.upgrade*25)+" koin",540,460,520))f.Buy(2);
                if(Button("Kembali",650,580))f.SetScreen(GameScreen.Playing);
                break;
            case GameScreen.Dialogue:
                Panel(280,605,1040,230,f.Speaker);Text(f.Dialogue,315,680,960,70);
                if(Button("Tutup",1080,765,190,43))f.SetScreen(GameScreen.Playing);break;
            case GameScreen.Dead:case GameScreen.Victory:
                Panel(420,230,760,430,f.Screen==GameScreen.Victory?"RELIC SECURED":"EKSPEDISI TERHENTI");
                Text(f.Screen==GameScreen.Victory?"Kamu membawa Relic of Veyr kembali ke Eldervale.":"Perjalananmu berlanjut dari menara checkpoint.",470,325,660,80);
                if(Button("Kembali ke checkpoint",550,440,500))f.Retry();
                if(Button("Main menu",650,520))f.Menu();break;
        }
        if(f.Subtitles&&GameAudio.I!=null&&GameAudio.I.Subtitle!="")
        {Rect(new Rect(340,790,920,55),ink);Text(GameAudio.I.Subtitle,360,805,880,48);}
        if(Time.unscaledTime<f.NoticeUntil){Rect(new Rect(420,105,760,68),ink);Text(f.Notice,445,120,710,55,small);}
        if(f.DebugAI)DebugView(f);
    }
    void Frontend(GameFlow f)
    {
        GUI.DrawTexture(new Rect(0,0,1000,900),menuShade,ScaleMode.StretchToFill);
        Text("E L D E R V A L E   /   A N   E X P E D I T I O N",102,92,700,40,small);
        Text("DUNGEON\nHUNTER",96,165,580,195,title);
        Text("R E L I C   O F   V E Y R",103,377,540,45,small);
        Rect(new Rect(104,440,58,2),gold);
        if(f.Screen==GameScreen.Menu)
        {
            Text("Di balik pegunungan, sebuah sumpah menunggu.\nTemukan sigil. Hadapi penjaga. Bawa pulang reliknya.",104,468,490,68,small);
            if(Button("Mulai ekspedisi   →",104,567,375,52))f.SetScreen(GameScreen.Select);
            if(Button("Lanjutkan perjalanan",104,632,375,48,f.HasSave()))f.Continue();
            if(Button("Pengaturan",104,693,375,48))f.OpenSettings();
            if(Button("Keluar",104,754,375,48)){
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying=false;
#else
                Application.Quit();
#endif
            }
            Text("01   /   THE MOUNTAIN SANCTUARY",1090,816,450,30,small);
            Text("SINGLE PLAYER    ·    HEADPHONES RECOMMENDED",104,848,780,30,small);
        }
        else
        {
            Text("PILIH PEMBURU",104,470,480,40,heading);
            for(int n=0;n<3;n++)
            { if(Button((n==selected?"—  ":"")+GameFlow.Names[n],104,530+n*60,375)) {selected=n;GameAudio.I?.Cue("character");GameAudio.I?.Speak(n,"idle");} }
            if(portrait!=null&&portrait.Texture!=null)GUI.DrawTexture(new Rect(805,130,470,590),portrait.Texture,ScaleMode.ScaleToFit,true);
            Panel(680,693,800,113,GameFlow.Names[selected].ToUpper()+"  /  ORIGIN");
            Text(selected==0?"Mencari jejak saudaranya yang hilang. Relik Veyr mungkin menyimpan jawabannya.":selected==1?"Mengikuti peta warisan ibunya untuk membuka kembali jalur yang disegel para Sentinel.":"Murid penjaga rune yang memburu relik demi membebaskan Eldervale dari sumpah lama.",710,747,730,50,small);
            if(Button("Masuki Eldervale   →",104,735,375))f.NewRun(selected);
            if(Button("Kembali",104,802,190,40))f.SetScreen(GameScreen.Menu);
        }
    }
    void HUD(GameFlow f)
    {
        if(f.Player==null)return;
        Panel(30,28,350,109,GameFlow.Names[f.State.character].ToUpper());
        Rect(new Rect(57,100,290,13),new Color(.2f,.22f,.23f));
        Rect(new Rect(57,100,290*Mathf.Clamp01(f.Player.HP/100),13),cyan);
        Text(Mathf.CeilToInt(f.Player.HP)+" / 100 HP",226,65,150,30,small);
        Text("Lv "+f.State.level+" · EXP "+f.State.xp+"\nKoin "+f.State.coins+"   Minuman "+f.State.potions,50,150,340,65,small);
        string objective=!f.State.sigilA||!f.State.sigilB?"Temukan dua sigil: Bara "+(f.State.sigilA?"✓":"○")+" · Kabut "+(f.State.sigilB?"✓":"○"):!f.State.guardian?"Masuki Veyr Sanctum. Kalahkan Guardian.":!f.State.artifact?"Ambil Relic of Veyr.":"Kembali ke altar Hunter Camp.";
        Panel(620,28,620,90,"");Text(objective,645,54,565,55,small);
        Text("M Peta · P Party\n1/2/3 Ganti · F3 AI",1255,45,320,65,small);
        if(f.Screen==GameScreen.Playing)
        {
            Text("+",788,434,40,35,body);
            if(f.Player.Prompt!=""){Panel(500,680,600,65,"");Text(f.Player.Prompt,530,696,540,45);}
            string weapon=f.State.weapon==0?"Iron Sword":f.State.weapon==1?"Rune Blade":"Rune Staff";
            Text("LMB Serang   Q Skill "+(f.Player.SkillReady?"siap":"cooldown")+"   Tab "+weapon+"   H Minum",395,846,1120,42,small);
            if(f.Player.Gliding)Text("MELAYANG",700,745,400,40,heading);
            if(f.Player.Cinematic)Text("ELDERVALE  /  "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,620,190,850,50,heading);
        }
        foreach(var e in FindObjectsByType<EnemyAgent>())
        {
            if(e.Boss&&!e.Dead&&e.Engaged)
            {Text(e.name.ToUpperInvariant(),565,750,800,40,heading);Rect(new Rect(450,797,700,13),Color.black);Rect(new Rect(450,797,700*e.HP/e.MaxHP,13),gold);}
        }
    }
    void Map(GameFlow f)
    {
        Panel(180,140,1240,625,"ELDERVALE — PETA PERJALANAN");
        Rect(new Rect(220,232,725,475),new Color(.06f,.13f,.13f));
        if(terrainMap!=null&&terrainMap.Texture!=null)GUI.DrawTexture(new Rect(220,232,725,475),terrainMap.Texture);
        Rect(new Rect(220,232,725,475),new Color(.025f,.04f,.04f,.2f));
        Vector2 MapPoint(Vector3 p)=>new Vector2(582.5f+p.x*725f/315f,469.5f-p.z*475f/210f);
        var camp=MapPoint(GameFlow.TowerPositions[0]);
        foreach(var p in GameFlow.TowerPositions)
        {
            var v=MapPoint(p);DrawLine(camp,v,new Color(.7f,.58f,.38f),3);
        }
        for(int i=0;i<4;i++)
        {
            var pos=MapPoint(GameFlow.TowerPositions[i]);
            var save=GUI.matrix;GUI.matrix=save*Matrix4x4.TRS(new Vector3(pos.x,pos.y,0),Quaternion.Euler(0,0,45),Vector3.one);Rect(new Rect(-8,-8,16,16),f.State.waypoints[i]?cyan:Color.gray);GUI.matrix=save;
            Rect(new Rect(pos.x-3,pos.y-3,6,6),ink);
            Rect(new Rect(pos.x-54,pos.y+17,200,29),new Color(.02f,.045f,.045f,.85f));
            Text(GameFlow.Places[i],pos.x-50,pos.y+16,210,30,small);
            bool can=f.State.waypoints[i]&&f.InTown&&!f.Engaged;
            if(Button((f.State.waypoints[i]?"Teleport: ":"Terkunci: ")+GameFlow.Places[i],990,270+i*78,370,56,can))f.Travel(i);
        }
        if(f.Player!=null&&f.InTown){var player=MapPoint(f.Player.transform.position);Rect(new Rect(player.x-5,player.y-5,10,10),Color.white);}
        Text("N ↑",900,245,45,30,small);
        Text(!f.InTown?"Kembali ke overworld untuk teleport.":f.Engaged?"Teleport ditahan: musuh masih mengingatmu.":"Aktifkan menara dengan E.\nGrand Tower memulihkan seluruh party.",985,613,390,105,small);
        if(Button("Tutup peta",1110,700,250,40))f.SetScreen(GameScreen.Playing);
    }
    void DrawLine(Vector2 a,Vector2 b,Color c,float width)
    {
        var save=GUI.matrix;var d=b-a;GUI.matrix=save*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg),Vector3.one);
        Rect(new Rect(0,0,d.magnitude,width),c);GUI.matrix=save;
    }
    void Settings(GameFlow f)
    {
        Panel(300,110,1000,700,"PENGATURAN");
        f.Master=Slider("Volume utama",f.Master,210);f.Music=Slider("Musik",f.Music,270);f.Sfx=Slider("Efek suara",f.Sfx,330);f.Voice=Slider("Suara karakter",f.Voice,390);
        f.Sensitivity=.15f+Slider("Sensitivitas kamera",(f.Sensitivity-.15f)/2.85f,450,2.85f,.15f,"×")*2.85f;
        f.Fov=45+Slider("Sudut pandang", (f.Fov-45)/45,510,45,45,"°")*45;
        f.Wind=Slider("Kekuatan angin",f.Wind,570);
        if(Button("Subtitle: "+(f.Subtitles?"Aktif":"Mati"),350,640,250,45))f.Subtitles=!f.Subtitles;
        if(Button("Idle voice: "+(f.IdleChatter?"Aktif":"Mati"),675,640,250,45))f.IdleChatter=!f.IdleChatter;
        if(Button("Invert Y: "+(f.InvertY?"Aktif":"Mati"),985,640,250,45))f.InvertY=!f.InvertY;
        if(Button("Uji audio kiri / tengah / kanan",350,710,500,44))GameAudio.I?.TestDirections();
        Text("Audio 3D · "+AudioSettings.outputSampleRate/1000+" kHz · "+AudioSettings.speakerMode,870,720,390,35,small);
        if(Button("Simpan dan kembali",600,764,400,42)){f.SaveSettings();f.SetScreen(f.ReturnScreen);}
    }
    float Slider(string label,float value,float y,float displayScale=100,float displayOffset=0,string unit="%")
    {
        Text(label,350,y,400,45);float next=GUI.HorizontalSlider(new Rect(800,y+14,360,30),value,0,1);
        Text((next*displayScale+displayOffset).ToString(unit=="×"?"F1":"F0")+unit,1180,y,100,35,small);
        if(Mathf.Abs(next-value)>.001f && Time.unscaledTime>sliderAt){GameAudio.I?.Cue("hover");sliderAt=Time.unscaledTime+.09f;}
        return next;
    }
    static readonly string[] DungeonTips = new[] {
        "Dua sigil kuno: Bara dan Kabut, mengunci pintu menuju Sanctum Veyr di puncak gunung.",
        "Kael (Pedang): Tekan Q untuk Earth Slam yang menghantam tanah dan menyalakan pedang api suci.",
        "Lyra (Panah): Tembakkan panah beruntun dan panggil hujan panah dari langit dengan skill Q.",
        "Garrick (Grimoire): Buku sihir kuno melayang di sampingmu dan memanggil ledakan singularitas gaib.",
        "Kunjungi kedai dan rumah warga di Eldervale untuk menikmati kopi, teh, dan makanan pemulih HP.",
        "Kereta uap wisata lembah berputar di padang timur—kamu dapat menaikinya dari peron stasiun."
    };

    void Loading(GameFlow f)
    {
        // 100% Solid Deep Dark Fantasy Dungeon Blackout
        Rect(new Rect(0,0,1600,900), new Color(.015f,.018f,.022f, 1f));
        float t = Time.unscaledTime;

        // Floating mystic amber and cyan embers
        for(int i=0;i<40;i++)
        {
            float x=Mathf.Repeat(i*57.3f+t*(12+i%7),1680)-40;
            float y=Mathf.Repeat(i*39.1f-t*(15+i%8),940)-20;
            float pulse = .3f + Mathf.Sin(t*2f+i)*.25f;
            Color emberCol = (i%2==0) ? new Color(1f,.7f,.25f,pulse) : new Color(.25f,.85f,.95f,pulse);
            Rect(new Rect(x,y,2.5f+i%2,2.5f+i%2), emberCol);
        }

        // Central Rotating Ancient Runic Sigil Seal
        Vector2 center = new Vector2(800, 410);
        float pulseScale = 1f + Mathf.Sin(t * 2.2f) * 0.04f;
        var oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(t * 18f, center);
        var oldColor = GUI.color;
        GUI.color = new Color(.18f, .85f, .82f, .75f);
        GUI.DrawTexture(new Rect(center.x - 160*pulseScale, center.y - 160*pulseScale, 320*pulseScale, 320*pulseScale), glow);
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;

        // Concentric geometric ritual rings
        for(int ring=1; ring<=3; ring++)
        {
            float r = ring * 45f * pulseScale;
            float angleOff = (ring % 2 == 0 ? 1 : -1) * t * 12f;
            for(int k=0; k<8; k++)
            {
                float a = (k / 8f * 360f + angleOff) * Mathf.Deg2Rad;
                float px = center.x + Mathf.Cos(a) * r;
                float py = center.y + Mathf.Sin(a) * r;
                Rect(new Rect(px - 3, py - 3, 6, 6), new Color(.85f, .75f, .4f, .65f));
            }
        }

        // Header Title
        Text("D U N G E O N   H U N T E R", 100, 75, 1400, 50, small);
        Text("MEMASUKI DUNIA ELDERVALE", 100, 115, 1400, 65, heading);

        // Progress bar with gold & runic turquoise fill
        Rect(new Rect(250, 740, 1100, 4), new Color(.12f, .18f, .22f, 1f));
        Rect(new Rect(250, 740, 1100 * f.Progress, 4), gold);

        // Progress percentage & phase
        string phase = f.Progress < .9f ? "MENYIAPKAN LEMBAH PEGUNUNGAN   ·   " + Mathf.RoundToInt(f.Progress * 100) + "%" : "GERBANG TERBUKA — MEMASUKI DUNIA…";
        Text(phase, 250, 755, 1100, 35, small);

        // Dungeon Lore & Gameplay Tips
        int tipIdx = (Mathf.FloorToInt(t / 4.5f)) % DungeonTips.Length;
        Text(DungeonTips[tipIdx], 250, 810, 1100, 60, small);
    }

    void DebugView(GameFlow f)
    {
        Panel(20,250,390,365,"AI DEBUG");
        Text("Music: "+GameAudio.I?.MusicState+"\nCombat: "+EnemyAgent.CombatCount+"  Alert: "+EnemyAgent.AlertCount+"\nScene: "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"\nF3 untuk menutup",40,320,350,120,small);
        int row=0;
        foreach(var e in FindObjectsByType<EnemyAgent>())
        {
            if(e.Civilian||e.Dead)continue;
            Text(e.name+": "+e.State+"\nMemory "+e.MemoryAge.ToString("F1")+"s · HP "+e.HP,40,435+row*57,350,58,small);
            if(++row>=3)break;
        }
    }
}


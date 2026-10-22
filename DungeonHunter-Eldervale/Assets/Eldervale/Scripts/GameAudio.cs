using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameAudio : MonoBehaviour
{
    public static GameAudio I;
    readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
    readonly List<AudioSource> pool=new List<AudioSource>();
    AudioSource[] music=new AudioSource[2];
    AudioSource voice;
    readonly List<AudioLowPassFilter> filters=new List<AudioLowPassFilter>();
    float nextAcoustics;
    bool testingDirections;
    string current="", previousBark="", subtitle="";
    float lastThreat, changed, voiceUntil, lastVoice=-100f, nextIdle;
    public string MusicState => current;
    public string Subtitle => Time.unscaledTime<voiceUntil?subtitle:"";
    public int PlayedCues { get; private set; }
    int active, pointer;
    readonly string[,] Lines={
        {"The ruins are quiet. Too quiet.","Something is watching us.","Stay back!","The relic. At last.","I need to recover."},
        {"Listen. The wind has changed.","I can hear someone ahead.","I will not miss.","We found what we came for.","I need a moment."},
        {"I hope this place has a way out.","That does not sound friendly.","Let the runes answer!","An ancient power, finally.","Not my best day."}
    };
    void Awake()
    {
        I=this;
        foreach(var clip in Resources.LoadAll<AudioClip>("Sounds")) if(!sounds.ContainsKey(clip.name)) sounds[clip.name]=clip;
        for(int n=0;n<24;n++)
        {
            var child=new GameObject("SFX voice "+n);child.SetActive(false);child.transform.SetParent(transform);
            var source=child.AddComponent<AudioSource>();source.playOnAwake=false;source.dopplerLevel=0;source.spread=0;
            pool.Add(source);var filter=child.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=22000;filters.Add(filter);
            child.SetActive(true);
        }
        for(int n=0;n<2;n++) { music[n]=gameObject.AddComponent<AudioSource>(); music[n].loop=true;music[n].playOnAwake=false;music[n].priority=32;music[n].bypassReverbZones=true; }
        var speaker=new GameObject("Character voice");speaker.transform.SetParent(transform);
        voice=speaker.AddComponent<AudioSource>();voice.playOnAwake=false;voice.priority=24;voice.dopplerLevel=0;
        nextIdle=Time.unscaledTime+30;
    }
    void OnDestroy(){if(I==this)I=null;}
    void Update()
    {
        var f=GameFlow.I;if(f==null)return;
        string desired=RunRules.SelectMusic(EnemyAgent.AlertCount,EnemyAgent.CombatCount,EnemyAgent.BossActive);
        if(EnemyAgent.CombatCount>0 || EnemyAgent.AlertCount>0) lastThreat=Time.unscaledTime;
        if(f.Screen==GameScreen.Menu || f.Screen==GameScreen.Select) desired="Explore";
        if((current=="Combat" || current=="Boss") && desired!="Boss" && desired!="Combat" && Time.unscaledTime-lastThreat<6f) desired=current;
        if(desired!=current) SwitchMusic(desired);
        float mix=Mathf.Clamp01((Time.unscaledTime-changed)/1.7f);
        float duck=voice.isPlaying?.55f:1f;
        music[active].volume=f.Master*f.Music*mix*duck;
        music[1-active].volume=f.Master*f.Music*(1-mix)*duck;
        voice.volume=f.Master*f.Voice;
        foreach(var s in pool) if(s.isPlaying) s.volume=f.Master*f.Sfx*.55f;
        if(Time.unscaledTime>nextAcoustics && f.Player!=null)
        {
            nextAcoustics=Time.unscaledTime+.12f;
            var listener=f.Player.ViewCamera.transform.position;
            for(int i=0;i<pool.Count;i++)
            {
                var s=pool[i];if(!s.isPlaying||s.spatialBlend==0)continue;
                bool blocked=Physics.Linecast(listener,s.transform.position,1<<0,QueryTriggerInteraction.Ignore);
                filters[i].cutoffFrequency=blocked?1800:22000;
            }
        }
        if(f.Controls && f.IdleChatter && f.Player.Speed<.1f && !f.Engaged && Time.unscaledTime>nextIdle)
        { Speak(f.State.character,"idle"); nextIdle=Time.unscaledTime+UnityEngine.Random.Range(40,60); }
    }
    void SwitchMusic(string state)
    {
        current=state;active=1-active;changed=Time.unscaledTime;
        string clip=state=="Combat"||state=="Boss"?"cynicmusic_battleThemeB":state=="Alert"?"kmontesdev_akash-chamber":"kmontesdev_shores-of-yore";
        music[active].clip=Resources.Load<AudioClip>("Music/"+clip);
        music[active].pitch=state=="Boss"?.92f:1f;
        music[active].volume=0;music[active].Play();
    }
    public void Cue(string id,Vector3? position=null)
    {
        var f=GameFlow.I;
        AudioClip clip=null;
        string name=id=="click"||id=="confirm"?"click_001":id=="back"?"back_001":id=="hover"?"switch_001":
            id=="step"?"footstep_grass_000":id=="stoneStep"?"footstep_concrete_000":id=="swing"?"knifeSlice":
            id=="weapon"?"metalClick":id=="hit"||id=="hurt"?"impactPunch_heavy_000":
            id=="jump"||id=="glide"?"cloth4":id=="land"?"footstep_concrete_003":id=="purchase"||id=="pickup"?"handleCoins":
            id=="enemyWindup"?"knifeSlice2":id=="dungeon"?"doorOpen_2":id=="splash"?"drop_001":"";
        if(name!="") sounds.TryGetValue(name,out clip);
        if(clip==null) clip=Tone(id);
        var source=pool[pointer++%pool.Count];
        source.Stop();source.clip=clip;source.loop=false;
        source.spatialBlend=position.HasValue?1f:0f;source.minDistance=2f;source.maxDistance=42f;
        source.bypassReverbZones=!position.HasValue;source.reverbZoneMix=position.HasValue?.65f:0;
        source.rolloffMode=AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff,new AnimationCurve(new Keyframe(0,1),new Keyframe(.08f,1),new Keyframe(.4f,.25f),new Keyframe(1,0)));
        filters[(pointer-1)%pool.Count].cutoffFrequency=22000;
        source.transform.position=position??(f!=null&&f.Player!=null?f.Player.transform.position:Vector3.zero);
        source.volume=(f==null?.8f:f.Master*f.Sfx)*.55f;
        source.pitch=id=="hover"||id=="click"?1f:UnityEngine.Random.Range(.94f,1.06f);
        source.Play();PlayedCues++;
    }
    AudioClip Tone(string id)
    {
        string key="generated_"+id;if(sounds.TryGetValue(key,out var cached))return cached;
        int rate=48000;
        if(id=="splash")
        {
            float duration=.38f;int count=(int)(duration*rate);var data=new float[count];
            for(int n=0;n<count;n++)
            {
                float t=(float)n/rate,u=t/duration;
                float noise=(Mathf.PerlinNoise(t*900f,0.5f)*2f-1f)*(1-u);
                float plop=Mathf.Sin(2*Mathf.PI*(160f+280f*(1-u))*t)*Mathf.Exp(-u*7f);
                data[n]=Mathf.Clamp(noise*.35f+plop*.45f,-1f,1f);
            }
            var sc=AudioClip.Create(key,count,1,rate,false);sc.SetData(data,0);sounds[key]=sc;return sc;
        }
        if(id=="whistle"||id=="train")
        {
            float duration=1.1f;int count=(int)(duration*rate);var data=new float[count];
            for(int n=0;n<count;n++)
            {
                float t=(float)n/rate,u=t/duration;
                float env=Mathf.Sin(Mathf.PI*Mathf.Clamp01(u*1.2f));
                data[n]=Mathf.Clamp((Mathf.Sin(2*Mathf.PI*587f*t)+Mathf.Sin(2*Mathf.PI*880f*t))*.22f*env,-1f,1f);
            }
            var sc=AudioClip.Create(key,count,1,rate,false);sc.SetData(data,0);sounds[key]=sc;return sc;
        }
        float durationBase=id=="teleport"||id=="departure"||id=="arrival"?1f:id=="death"?1.2f:.18f;
        int countBase=(int)(durationBase*rate);var dataBase=new float[countBase];
        float frequency=id=="error"||id=="locked"?145:id=="heal"?650:id=="skill"?380:id=="jump"?300:id=="death"?125:id=="pickup"?880:500;
        for(int n=0;n<countBase;n++)
        { float t=(float)n/rate,u=t/durationBase;float sweep=(id=="departure"||id=="skill"||id=="jump")?1+u*3:id=="death"?1-u*.65f:1; dataBase[n]=Mathf.Sin(2*Mathf.PI*frequency*sweep*t)*Mathf.Sin(Mathf.PI*u)*.24f+Mathf.Sin(2*Mathf.PI*frequency*1.5f*t)*Mathf.Sin(Mathf.PI*u)*.08f; }
        var c=AudioClip.Create(key,countBase,1,rate,false);c.SetData(dataBase,0);sounds[key]=c;return c;
    }
    public void Speak(int character,string context,string text=null)
    {
        if(GameFlow.I==null || character<0 || character>2)return;
        if(Time.unscaledTime-lastVoice<8f && context!="death" && text==null)return;
        int index=context=="idle"?0:context=="alert"?1:context=="combat"?2:context=="death"?4:3;
        string line=text??Lines[character,index];
        if(line==previousBark && text==null && context=="idle")return;
        var clip=Resources.Load<AudioClip>("Voice/"+GameFlow.Names[character]+"/"+index);
        voice.spatialBlend=0;voice.bypassReverbZones=true;
        voice.Stop();voice.clip=clip;
        if(clip!=null)voice.Play();
        previousBark=line;lastVoice=Time.unscaledTime;
        subtitle=GameFlow.Names[character]+": "+line;voiceUntil=Time.unscaledTime+(clip!=null?clip.length:4f);
    }
    public void SpeakNPC(string speaker,string text,bool force=false,Vector3? position=null)
    {
        if(!force&&(Time.unscaledTime-lastVoice<10||voice.isPlaying))return;
        lastVoice=Time.unscaledTime;subtitle=speaker+": "+text;voiceUntil=Time.unscaledTime+4;
        string id=speaker.StartsWith("Guardian")?"boss":speaker=="Warden"?"warden":text.StartsWith("Dulu")?"story":"greeting";
        var clip=Resources.Load<AudioClip>("Voice/NPC/"+id);
        voice.transform.position=position??Vector3.zero;voice.spatialBlend=position.HasValue?1:0;
        voice.minDistance=3;voice.maxDistance=24;voice.rolloffMode=AudioRolloffMode.Linear;voice.bypassReverbZones=!position.HasValue;
        if(clip!=null){voice.Stop();voice.clip=clip;voice.Play();voiceUntil=Time.unscaledTime+clip.length;}
    }
    public void TestDirections(){if(!testingDirections)StartCoroutine(DirectionCheck());}
    IEnumerator DirectionCheck()
    {
        testingDirections=true;
        string[] labels={"KIRI","TENGAH / DEPAN","KANAN","BELAKANG"};
        Vector3[] offsets={Vector3.left,Vector3.forward,Vector3.right,Vector3.back};
        foreach(int index in new[]{0,1,2,3})
        {
            var camera=Camera.main;
            if(GameFlow.I.Player!=null)camera=GameFlow.I.Player.ViewCamera;
            if(camera==null)camera=FindAnyObjectByType<Camera>();
            if(camera!=null){GameFlow.I.Note("Uji audio 3D: "+labels[index]+" — gunakan headphone");Cue("audioProbe",camera.transform.position+camera.transform.TransformDirection(offsets[index])*5);}
            yield return new WaitForSecondsRealtime(1.2f);
        }
        testingDirections=false;
    }
}


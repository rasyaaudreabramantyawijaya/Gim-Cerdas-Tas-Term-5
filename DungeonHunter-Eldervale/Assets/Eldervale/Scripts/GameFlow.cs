using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameScreen { Menu, Select, Playing, Map, Party, Pause, Settings, Shop, Dialogue, Loading, Dead, Victory, Cinematic }
public sealed class GameFlow : MonoBehaviour
{
    public static GameFlow I { get; private set; }
    public static readonly string[] Names = { "Kael", "Lyra", "Orin" };
    public static readonly string[] Places = { "Hunter Camp", "Ashen Crypt", "Whispering Grotto", "Veyr Sanctum" };
    public static readonly Vector3[] TowerPositions = { new Vector3(0,0,-52),new Vector3(-58,0,0),new Vector3(58,0,0),new Vector3(0,0,62) };
    public RunState State = new RunState();
    public GameScreen Screen = GameScreen.Menu;
    public GameScreen ReturnScreen = GameScreen.Menu;
    public HunterPlayer Player;
    public EnemyAgent PendingBoss;
    public string Notice = "", Dialogue = "", Speaker = "", Destination = "";
    public float NoticeUntil, Progress;
    float switchedAt=-10;
    public int SpawnId;
    public bool DebugAI;
    public bool InTown => SceneManager.GetActiveScene().name == "EldervaleOverworld";
    public bool Busy => Screen == GameScreen.Loading;
    public bool Controls => Screen == GameScreen.Playing && Player != null && Player.HP > 0 && !Busy;
    public bool Engaged => EnemyAgent.CombatCount > 0;
    public float Master=0.8f, Music=0.55f, Sfx=0.8f, Voice=0.8f, Sensitivity=1f, Fov=65f, Wind=1f;
    public bool InvertY, Subtitles=true, IdleChatter=true, Marker=true;
    [SerializeField] string saveFileName="eldervale-run.json";
    string SavePath => Path.Combine(Application.persistentDataPath,Application.isBatchMode?"eldervale-verification-run.json":saveFileName);
    string SettingsPath => Path.Combine(Application.persistentDataPath,"eldervale-settings.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (I != null) return;
        new GameObject("Eldervale Services").AddComponent<GameFlow>();
    }
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I=this; DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<GameAudio>();
        gameObject.AddComponent<Presentation>();
        LoadSettings();
        SceneManager.sceneLoaded += OnScene;
    }
    void OnDestroy() { SceneManager.sceneLoaded -= OnScene; if(I==this) I=null; }
    void OnScene(Scene scene, LoadSceneMode mode)
    {
        Player = FindAnyObjectByType<HunterPlayer>();
        if (scene.name == "Frontend") SetScreen(GameScreen.Menu);
    }
    void Update()
    {
        Shader.SetGlobalFloat("_EldervaleTime",Time.unscaledTime);
        if(Input.GetKeyDown(KeyCode.F3)) DebugAI=!DebugAI;
        if(Input.GetKeyDown(KeyCode.Escape) && !Busy)
        {
            if(Screen==GameScreen.Playing) SetScreen(GameScreen.Pause);
            else if(Screen==GameScreen.Menu) { }
            else if(Screen==GameScreen.Select) SetScreen(GameScreen.Menu);
            else if(Screen==GameScreen.Settings) SetScreen(ReturnScreen);
            else if(Screen!=GameScreen.Dead && Screen!=GameScreen.Victory) SetScreen(GameScreen.Playing);
            GameAudio.I?.Cue("back");
        }
        if(Input.GetKeyDown(KeyCode.M) && !Busy && (Screen==GameScreen.Playing || Screen==GameScreen.Map))
        { SetScreen(Screen==GameScreen.Map?GameScreen.Playing:GameScreen.Map); GameAudio.I?.Cue("map"); }
        if(Input.GetKeyDown(KeyCode.P) && !Busy && (Screen==GameScreen.Playing || Screen==GameScreen.Party))
        {SetScreen(Screen==GameScreen.Party?GameScreen.Playing:GameScreen.Party);GameAudio.I?.Cue("party");}
        if(Controls)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))SwitchCharacter(0);
            if(Input.GetKeyDown(KeyCode.Alpha2))SwitchCharacter(1);
            if(Input.GetKeyDown(KeyCode.Alpha3))SwitchCharacter(2);
        }
    }
    public void SetScreen(GameScreen s)
    {
        if(Screen==GameScreen.Dialogue && s==GameScreen.Playing && PendingBoss!=null)
        { PendingBoss.IntroBlocked=false; PendingBoss=null; Player?.Cinema(1f,8); GameAudio.I?.Cue("combatStart"); }
        Screen=s;
        Time.timeScale = s==GameScreen.Playing || s==GameScreen.Loading ? 1f : 0f;
        Cursor.lockState = Controls ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !Controls;
    }
    public void Note(string text) { Notice=text; NoticeUntil=Time.unscaledTime+4.5f; }
    public bool HasSave() { try { return File.Exists(SavePath) && JsonUtility.FromJson<RunState>(File.ReadAllText(SavePath)).Valid; } catch { return false; } }
    public void Save()
    {
        if(Player!=null&&State.health!=null&&State.health.Length==3)State.health[State.character]=Player.HP;
        try { Directory.CreateDirectory(Application.persistentDataPath); var temp=SavePath+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(State,true)); if(File.Exists(SavePath))File.Replace(temp,SavePath,null);else File.Move(temp,SavePath); }
        catch(Exception e) { Note("Save gagal: "+e.Message); Debug.LogException(e); }
    }
    public void NewRun(int character)
    {
        if(Busy) return;
        State=new RunState {character=character};
        State.party=new[]{character,(character+1)%3,(character+2)%3};
        SpawnId=0; GameAudio.I?.Cue("character");
        if(SanctuaryCamera.I != null && !SanctuaryCamera.I.Zooming)
        {
            SetScreen(GameScreen.Cinematic);
            SanctuaryCamera.I.ZoomIntoGate(() => {
                StartCoroutine(Load("EldervaleOverworld",0,"Eldervale"));
            });
        }
        else StartCoroutine(Load("EldervaleOverworld",0,"Eldervale"));
    }
    public void Continue()
    {
        if(!HasSave()) { Note("Belum ada save yang valid."); GameAudio.I?.Cue("error"); return; }
        State=JsonUtility.FromJson<RunState>(File.ReadAllText(SavePath));
        if(SanctuaryCamera.I != null && !SanctuaryCamera.I.Zooming)
        {
            SetScreen(GameScreen.Cinematic);
            SanctuaryCamera.I.ZoomIntoGate(() => {
                StartCoroutine(Load("EldervaleOverworld",State.checkpoint,"Lanjut ekspedisi"));
            });
        }
        else StartCoroutine(Load("EldervaleOverworld",State.checkpoint,"Lanjut ekspedisi"));
    }
    public void OpenSettings() { ReturnScreen=Screen; SetScreen(GameScreen.Settings); }
    public void Talk(string speaker,string text) { Speaker=speaker; Dialogue=text; SetScreen(GameScreen.Dialogue); GameAudio.I?.SpeakNPC(speaker,text,true); }
    public void Activate(int id)
    {
        State.waypoints[id]=true; State.checkpoint=id;
        GameAudio.I?.Cue("activate",Player.transform.position);
        Player.Cinema(2.6f,11f);
        if(id==0) { State.health=new float[]{100,100,100};Player.Heal(100); Note("Grand Tower: seluruh party pulih penuh. Waypoint disimpan."); GameAudio.I?.Cue("heal"); }
        else Note("Menara "+Places[id]+" aktif. Buka peta M untuk teleport.");
        Save();
    }
    public bool Travel(int id)
    {
        if(id<0 || id>=4 || !RunRules.CanTravel(State.waypoints[id],Engaged,Player!=null&&Player.HP>0,Busy,InTown))
        { Note("Teleport memerlukan waypoint aktif, di overworld dan di luar combat."); GameAudio.I?.Cue("error"); return false; }
        GameAudio.I?.Cue("teleport",Player.transform.position);
        State.checkpoint=id;
        StartCoroutine(Load("EldervaleOverworld",id,"Teleport — "+Places[id]));
        return true;
    }
    public void EnterDungeon(int id)
    {
        if(Busy || Engaged) { Note("Akhiri combat sebelum memasuki gerbang."); GameAudio.I?.Cue("error"); return; }
        if(id==3 && !RunRules.CanOpenSanctum(State.sigilA,State.sigilB)) { Note("Veyr Sanctum memerlukan Sigil Bara dan Sigil Kabut."); GameAudio.I?.Cue("locked"); return; }
        State.checkpoint=id; Save();
        GameAudio.I?.Cue("dungeon");
        StartCoroutine(Load(id==1?"AshenCrypt":id==2?"WhisperingGrotto":"VeyrSanctum",-id,Places[id]));
    }
    public void ExitDungeon(int id)
    {
        if(Busy || Engaged) { Note("Musuh masih bertarung. Cari posisi aman."); GameAudio.I?.Cue("error"); return; }
        StartCoroutine(Load("EldervaleOverworld",id,"Kembali — "+Places[id]));
    }
    public void Collect(int id)
    {
        if(id==1) { if(State.sigilA) return; State.sigilA=true; State.coins+=35; Note("Sigil Bara ditemukan. +35 koin"); }
        if(id==2) { if(State.sigilB) return; State.sigilB=true; State.coins+=35; Note("Sigil Kabut ditemukan. +35 koin"); }
        if(id==3) { if(!State.guardian) { Note("Kalahkan Guardian sebelum mengambil relik."); return; } State.artifact=true; Note("Relic of Veyr diamankan. Kembali ke altar camp."); }
        Save(); GameAudio.I?.Cue("pickup"); GameAudio.I?.Speak(State.character,"discovery");
    }
    public void Extract()
    {
        if(RunRules.CanWin(State.sigilA,State.sigilB,State.guardian,State.artifact,true))
        { GameAudio.I?.Cue("victory"); SetScreen(GameScreen.Victory); }
        else Talk("Warden","Temukan dua sigil, kalahkan Guardian dan bawa Relic of Veyr ke altar ini.");
    }
    public void Buy(int item)
    {
        if(Player==null) return;
        int cost=item==0?25:item==1?40:60+State.upgrade*25;
        if(State.coins<cost || item==2&&State.upgrade>=5) { GameAudio.I?.Cue("error"); Note("Koin tidak cukup atau upgrade sudah maksimum."); return; }
        State.coins-=cost;
        if(item==0) State.potions++;
        if(item==1) State.weapon=(State.weapon+1)%3;
        if(item==2) State.upgrade++;
        Save(); GameAudio.I?.Cue("purchase"); Player.RefreshWeapon();
        Note(item==0?"Minuman pemulih ditambahkan. Tekan H.":item==1?"Senjata diganti.":"Senjata ditingkatkan.");
    }
    public void AwardXP(int xp)
    {
        State.xp+=Mathf.Max(0,xp);
        int before=State.level;
        while(State.xp>=State.level*(State.level+1)*50)State.level++;
        if(State.level>before){Note("Naik ke level "+State.level+"! Klaim hadiah di Warden kota.");GameAudio.I?.Cue("levelUp");}
        Save();
    }
    public void ClaimRewards()
    {
        if(!InTown||Player==null||Vector3.Distance(Player.transform.position,new Vector3(-5,0,-53))>7)
        {Note("Klaim hadiah hanya di Warden kota.");GameAudio.I?.Cue("error");return;}
        int levels=State.level-State.claimedLevel;
        if(levels<=0){Note("Tidak ada hadiah yang belum diklaim.");return;}
        State.coins+=levels*40;State.potions+=levels;State.claimedLevel=State.level;
        Note("Hadiah diklaim: "+levels*40+" koin dan "+levels+" minuman.");GameAudio.I?.Cue("pickup");Save();
    }
    public void Retry() { if(Busy) return;State.health=new float[]{100,100,100};if(Player!=null)Player.HP=100;StartCoroutine(Load("EldervaleOverworld",State.checkpoint,"Kembali ke checkpoint")); }
    public bool SwitchCharacter(int slot)
    {
        if(Player==null||Busy||Player.HP<=0||slot<0||slot>2||Time.unscaledTime-switchedAt<1.5f)return false;
        if(State.party==null||State.party.Length!=3)State.party=new[]{0,1,2};
        if(State.health==null||State.health.Length!=3)State.health=new float[]{100,100,100};
        int character=State.party[slot];if(character==State.character)return true;
        if(State.health[character]<=0){Note("Karakter ini memerlukan pemulihan di Grand Tower.");GameAudio.I?.Cue("error");return false;}
        State.health[State.character]=Player.HP;State.character=character;Player.HP=State.health[character];
        Player.SwitchProfile();switchedAt=Time.unscaledTime;GameAudio.I?.Cue("character");GameAudio.I?.Speak(character,"combat");
        Save();return true;
    }
    public void CyclePartySlot(int slot)
    {
        int next=(State.party[slot]+1)%3;int other=Array.IndexOf(State.party,next);
        if(other>=0)State.party[other]=State.party[slot];State.party[slot]=next;Save();GameAudio.I?.Cue("character");
    }
    public void Menu() { if(Busy) return; StartCoroutine(Load("Frontend",0,"Kembali ke menu")); }
    IEnumerator Load(string scene,int spawn,string title)
    {
        if(Busy) yield break;
        SetScreen(GameScreen.Loading); Destination=title; Progress=0f; SpawnId=spawn;
        if(Player!=null&&State.health!=null&&State.health.Length==3)State.health[State.character]=Player.HP;
        Player=null; GameAudio.I?.Cue("departure");
        yield return null;
        var operation=SceneManager.LoadSceneAsync(scene);
        if(operation==null) { Note("Scene tidak tersedia."); SetScreen(GameScreen.Menu); yield break; }
        operation.allowSceneActivation=false;
        float start=Time.unscaledTime;
        while(operation.progress<0.9f || Time.unscaledTime-start<2.8f)
        { Progress=Mathf.Clamp01(operation.progress/0.9f)*0.9f; yield return null; }
        Progress=.95f; operation.allowSceneActivation=true;
        while(!operation.isDone) yield return null;
        yield return null;
        Progress=1f;
        if(scene!="Frontend")
        {
            Player=FindAnyObjectByType<HunterPlayer>();
            SetScreen(GameScreen.Playing); Player?.Cinema(3.1f,13f);
            GameAudio.I?.Cue("arrival"); Save();
        }
        else SetScreen(GameScreen.Menu);
    }
    [Serializable] sealed class SettingsData { public float master,music,sfx,voice,sensitivity,fov,wind; public bool invert,subtitles,idle,marker; }
    public void SaveSettings()
    {
        var d=new SettingsData{master=Master,music=Music,sfx=Sfx,voice=Voice,sensitivity=Sensitivity,fov=Fov,wind=Wind,invert=InvertY,subtitles=Subtitles,idle=IdleChatter,marker=Marker};
        File.WriteAllText(SettingsPath,JsonUtility.ToJson(d)); GameAudio.I?.Cue("confirm");
    }
    void LoadSettings()
    {
        if(!File.Exists(SettingsPath)) return;
        try { var d=JsonUtility.FromJson<SettingsData>(File.ReadAllText(SettingsPath)); Master=Mathf.Clamp01(d.master); Music=Mathf.Clamp01(d.music); Sfx=Mathf.Clamp01(d.sfx); Voice=Mathf.Clamp01(d.voice); Sensitivity=Mathf.Clamp(d.sensitivity,.2f,3); Fov=Mathf.Clamp(d.fov,45,90); Wind=Mathf.Clamp01(d.wind); InvertY=d.invert; Subtitles=d.subtitles; IdleChatter=d.idle; Marker=d.marker; } catch { Note("Settings lama tidak terbaca; memakai default."); }
    }
}

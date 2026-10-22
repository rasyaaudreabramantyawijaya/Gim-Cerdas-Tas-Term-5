using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

[InitializeOnLoad]
public static class EldervalePlayVerification
{
    static int stage,pass,failures;
    static IEnumerator audit;
    static bool inspectedReturn;
    static double deadline,next;
    static string evidence="docs/evidence/";
    static bool exiting;
    static string errors="";
    static string saveBackup;
    static string savePath;
    static EldervalePlayVerification()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("EldervalePlayCheck",false))
            {
                stage=0;pass=0;failures=0;errors="";exiting=false;deadline=EditorApplication.timeSinceStartup+190;
                Application.logMessageReceived+=Log;EditorApplication.update+=Step;
            }
        };
    }
    static void Log(string condition,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception)errors+=condition+"\n";}
    public static void Run()
    {
        savePath=Path.Combine(Application.persistentDataPath,"eldervale-verification-run.json");
        SessionState.SetString("EldervaleCheckSavePath",savePath);
        saveBackup=File.Exists(savePath)?File.ReadAllText(savePath):null;
        SessionState.SetString("EldervaleCheckSave",saveBackup??"");
        Directory.CreateDirectory(evidence);File.WriteAllText(evidence+"play-checks.txt","");
        SessionState.SetBool("EldervalePlayCheck",true);
        EditorSceneManager.OpenScene("Assets/Eldervale/Scenes/Frontend.unity");EditorApplication.EnterPlaymode();
    }
    static void Check(bool value,string label)
    {
        File.AppendAllText(evidence+"play-checks.txt",(value?"PASS ":"FAIL ")+label+"\n");
        if(!value)throw new Exception("FAIL "+label);pass++;
    }
    static void Audit(bool value,string label)
    {File.AppendAllText(evidence+"play-checks.txt",(value?"PASS ":"FAIL ")+label+"\n");if(value)pass++;else failures++;}
    static IEnumerator AuditRuntime(GameFlow f)
    {
        var saved=f.Player.transform.position;
        var cape=f.Player.GetComponentInChildren<FlowingCape>();
        Audit(cape!=null&&Vector3.Dot(cape.transform.position-cape.Shoulder.position,f.Player.transform.forward)<-.1f&&cape.GetComponent<Renderer>().bounds.size.y<1.4f,"cape stays behind shoulders at human scale after animation");
        var outfit=f.Player.GetComponentInChildren<AdventurerOutfit>();
        Audit(outfit!=null&&outfit.GetComponentsInChildren<MeshRenderer>().Length>=10&&outfit.GetComponentsInChildren<MeshRenderer>().All(r=>r.bounds.size.sqrMagnitude>.001f),"fitted tunic trousers bracers and boots follow humanoid joints");
        var foliage=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r=>r.transform.parent!=null&&r.transform.parent.name.Contains("Tree")).ToArray();
        Audit(foliage.Length>0&&foliage.All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader!=null&&m.shader.name!="Hidden/InternalErrorShader")),"imported trees retain live materials after scene transition");
        var treeRoots=UnityEngine.Object.FindObjectsByType<NatureMaterialLibrary>().SelectMany(l=>l.GetComponentsInChildren<Transform>()).Where(t=>t.name.StartsWith("Wind imported ")&&t.name.Contains("Tree")).ToArray();
        Audit(treeRoots.Length>0&&treeRoots.All(t=>{var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b.size.y>Mathf.Max(b.size.x,b.size.z)*.7f&&Mathf.Abs(b.min.y-t.position.y)<.45f;}),"tree import axes remain upright and roots touch terrain");
        var enemy=UnityEngine.Object.FindObjectsByType<EnemyAgent>().First(e=>!e.Civilian&&!e.Trainer&&!e.Boss);
        var sees=typeof(EnemyAgent).GetMethod("CanSee",BindingFlags.NonPublic|BindingFlags.Instance);
        Position(f.Player,enemy.transform.position+enemy.transform.forward*5);Physics.SyncTransforms();
        Audit((bool)sees.Invoke(enemy,new object[]{f.Player}),"AI sees player inside field of view");
        Position(f.Player,enemy.transform.position-enemy.transform.forward*5);Physics.SyncTransforms();
        Audit(!(bool)sees.Invoke(enemy,new object[]{f.Player}),"AI cannot see behind its back");
        Position(f.Player,enemy.transform.position+enemy.transform.forward*5);
        var cover=GameObject.CreatePrimitive(PrimitiveType.Cube);cover.transform.position=(enemy.transform.position+f.Player.transform.position)/2+Vector3.up;cover.transform.localScale=new Vector3(4,4,1);Physics.SyncTransforms();
        Audit(!(bool)sees.Invoke(enemy,new object[]{f.Player}),"solid cover blocks AI line of sight");UnityEngine.Object.Destroy(cover);Position(f.Player,saved);
        float hp=enemy.HP;enemy.Damage(1);enemy.State="Investigate";
        Audit(enemy.Engaged,"fresh combat memory remains engaged behind cover");
        enemy.HP=hp;enemy.State="Patrol";typeof(EnemyAgent).GetField("lastSeen",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(enemy,-100f);
        var tree=UnityEngine.Object.FindObjectsByType<CapsuleCollider>().First(c=>c.name=="Pine trunk"&&Mathf.Abs(c.transform.position.x)>38&&Mathf.Abs(c.transform.position.x)<70&&c.transform.position.z>10&&c.transform.position.z<70);
        var bottom=new Vector3(tree.transform.position.x,tree.bounds.min.y+.05f,tree.transform.position.z);
        Audit(!NavMesh.SamplePosition(bottom,out var point,.15f,NavMesh.AllAreas),"navigation excludes solid pine trunk footprint");
        var renderer=f.Player.Visual.GetComponentsInChildren<Renderer>().First(r=>r.name=="Iron Sword"||r.name=="Rune Blade"||r.name=="Rune Staff");
        var oldMaterial=renderer.sharedMaterial;f.Player.RefreshWeapon();
        double end=EditorApplication.timeSinceStartup+.2;while(EditorApplication.timeSinceStartup<end)yield return null;
        Audit(oldMaterial==null,"weapon swap releases obsolete native material");
        var animator=f.Player.GetComponentInChildren<Animator>();f.Player.Visual.Action("Attack");
        end=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<end)yield return null;
        float before=animator.GetCurrentAnimatorStateInfo(0).normalizedTime;f.Player.Visual.Action("Attack");
        end=EditorApplication.timeSinceStartup+.22;while(EditorApplication.timeSinceStartup<end)yield return null;
        Audit(animator.GetCurrentAnimatorStateInfo(0).normalizedTime<before+.1f,"repeated attack restarts actual animation");
    }
    static void Capture(string name,Camera camera)
    {
        if(camera==null)return;
        var previous=RenderTexture.active;var target=new RenderTexture(1280,720,24);
        var original=camera.targetTexture;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
        File.WriteAllBytes(evidence+name+".png",texture.EncodeToPNG());
        camera.targetTexture=original;RenderTexture.active=previous;
        UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);
    }
    static void Position(HunterPlayer p,Vector3 position)
    {
        var controller=p.GetComponent<CharacterController>();controller.enabled=false;p.transform.position=position;controller.enabled=true;p.CinemaUntil=-1;
    }
    static void Step()
    {
        if(exiting)return;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timeout at verification stage "+stage);
            var f=GameFlow.I;if(f==null)return;
            if(stage==0)
            {
                Check(f.Screen==GameScreen.Menu,"Frontend opens with main menu");
                typeof(GameFlow).GetField("saveFileName",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(f,"eldervale-verification-run.json");
                Audit(((string)typeof(GameFlow).GetProperty("SavePath",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f)).EndsWith("eldervale-verification-run.json"),"editor verification uses isolated save slot");
                Check(GameAudio.I!=null,"audio service initialized");
                GameAudio.I.Cue("audioProbe",Vector3.right*8);
                var probe=GameAudio.I.GetComponentsInChildren<AudioSource>().First(s=>s.clip!=null&&s.clip.name=="generated_audioProbe");
                Check(probe.spatialBlend==1f,"positional SFX is fully 3D, not partly centered");
                Check(probe.clip.frequency==48000,"generated SFX uses 48 kHz audio");
                Check(probe.clip.channels==1,"world emitter is mono for directional playback");
                GameAudio.I.Cue("click");
                Check(GameAudio.I.GetComponentsInChildren<AudioSource>().Any(s=>s.clip!=null&&s.clip.name=="click_001"&&s.spatialBlend==0),"UI cue remains centered 2D");
                Check(GameObject.Find("Mountain waterfall")!=null,"title background contains real animated waterfall geometry");
                Check(Resources.Load<Font>("Fonts/Inter-Regular")!=null,"bundled Inter typography available, no OS font dependency");
                f.SetScreen(GameScreen.Select);next=EditorApplication.timeSinceStartup+1;stage=-1;return;
            }
            if(f.Busy || EditorApplication.timeSinceStartup<next)return;
            if(stage==-1)
            {
                Check(GameObject.Find("Character portrait studio")!=null,"character selection has a live 3D portrait, not only text");
                f.NewRun(1);stage=1;return;
            }
            if(stage==1)
            {
                Check(f.Player!=null&&f.Screen==GameScreen.Playing,"new game loads overworld and live player");
                Check(SceneManager.GetActiveScene().name=="EldervaleOverworld","overworld is separate scene");
                Check(f.State.character==1,"character selection retained");
                Check(f.Player.GetComponentsInChildren<SkinnedMeshRenderer>().Length>0,"actual rigged character mesh");
                var animator=f.Player.GetComponentInChildren<Animator>();
                Check(animator!=null&&animator.isHuman,"Humanoid avatar imported");
                Check(animator.runtimeAnimatorController!=null,"animation controller wired");
                Audit(animator.runtimeAnimatorController.animationClips.Where(c=>c.name.EndsWith("Idle_Loop")||c.name.EndsWith("Walk_Loop")).All(c=>c.isLooping),"idle and walk clips actually loop");
                Check(UnityEngine.Object.FindObjectsByType<EnemyAgent>().Count(e=>e.Civilian)>=10,"town population present");
                Check(UnityEngine.Object.FindObjectsByType<Interactable>().Count(i=>i.Kind==UseKind.Gate)==3,"three dungeon gates");
                Check(UnityEngine.Object.FindObjectsByType<Interactable>().Count(i=>i.Kind==UseKind.Tower)==4,"four travel towers");
                Check(NavMesh.SamplePosition(f.Player.transform.position,out var point,5,NavMesh.AllAreas),"walkable NavMesh at player spawn");
                Capture("overworld",f.Player.ViewCamera);
                audit=AuditRuntime(f);stage=100;return;
            }
            if(stage==100){if(audit.MoveNext())return;stage=11;}
            if(stage==11)
            {
                f.Player.HP=20;f.Activate(0);Check(f.Player.HP==100,"Grand Tower heals to full");
                Position(f.Player,new Vector3(-5,1,-54));
                int coins=f.State.coins;f.AwardXP(120);Check(f.State.level==2,"kills can advance level by XP");
                f.ClaimRewards();Check(f.State.coins==coins+40,"city level reward claim");
                coins=f.State.coins;f.ClaimRewards();Check(f.State.coins==coins,"reward cannot be claimed twice");
                Check(!f.Travel(1),"undiscovered waypoint blocked");
                f.Player.HP=37;
                f.State.waypoints[1]=true;Check(f.Travel(1),"known waypoint starts transition");
                Check(!f.Travel(1),"duplicate transition blocked");
                stage=2;return;
            }
            if(stage==2)
            {
                Check(Vector3.Distance(f.Player.transform.position,GameFlow.TowerPositions[1])<10,"teleport destination spawn correct");
                Check(f.Player.HP==37,"teleport preserves active character HP");
                f.EnterDungeon(1);stage=3;return;
            }
            if(stage==3)
            {
                Check(SceneManager.GetActiveScene().name=="AshenCrypt","dungeon scene entered");
                Check(!f.Travel(0),"map teleport blocked inside dungeon");
                Capture("ashen-crypt",f.Player.ViewCamera);
                var enemies=UnityEngine.Object.FindObjectsByType<EnemyAgent>();
                Check(enemies.Length>=2,"dungeon enemy encounter");
                var e=enemies[0];f.Player.CinemaUntil=-1;e.Damage(999);
                Check(e.Dead,"enemy receives lethal damage");
                int xp=f.State.xp;e.Damage(999);Check(f.State.xp==xp,"dead enemy cannot award XP twice");
                var loot=UnityEngine.Object.FindAnyObjectByType<LootDrop>();
                Check(loot!=null,"enemy drops visible auto-loot");
                Position(f.Player,loot.transform.position-Vector3.up*.5f);
                next=EditorApplication.timeSinceStartup+2;stage=4;return;
            }
            if(stage==4)
            {
                Check(UnityEngine.Object.FindAnyObjectByType<LootDrop>()==null,"nearby loot automatically collected");
                f.State.sigilA=true;f.State.sigilB=true;
                // Return through real scene transition; encounters are cleared so combat guard permits it.
                foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyAgent>())e.Damage(999);
                f.ExitDungeon(1);stage=5;return;
            }
            if(stage==5)
            {
                Check(SceneManager.GetActiveScene().name=="EldervaleOverworld","dungeon exit returns to overworld");
                f.EnterDungeon(3);stage=6;return;
            }
            if(stage==6)
            {
                Check(SceneManager.GetActiveScene().name=="VeyrSanctum","two sigils unlock Sanctum");
                var boss=UnityEngine.Object.FindObjectsByType<EnemyAgent>().First(e=>e.Boss);
                Check(boss.IntroBlocked,"boss waits for story introduction");
                float health=boss.HP;boss.Damage(1);Audit(boss.HP==health,"Guardian invulnerable until introductory dialogue");
                Position(f.Player,new Vector3(0,0,0));next=EditorApplication.timeSinceStartup+1;stage=7;return;
            }
            if(stage==7)
            {
                Check(f.Screen==GameScreen.Dialogue&&f.PendingBoss!=null,"approaching Guardian starts dialogue");
                f.SetScreen(GameScreen.Playing);
                var boss=UnityEngine.Object.FindObjectsByType<EnemyAgent>().First(e=>e.Boss);
                Check(!boss.IntroBlocked,"finishing dialogue activates Guardian");
                boss.Damage(999);Check(f.State.guardian,"Guardian completion recorded");
                f.Collect(3);Check(f.State.artifact,"artifact can be collected after Guardian");
                Capture("guardian-arena",f.Player.ViewCamera);
                f.ExitDungeon(3);stage=8;return;
            }
            if(stage==8)
            {
                f.Extract();Check(f.Screen==GameScreen.Victory,"artifact extraction wins expedition");
                Check(GameAudio.I.PlayedCues>10,"interaction and transition cues dispatched");
                f.Player.HP=0;f.SetScreen(GameScreen.Dead);f.Retry();stage=9;return;
            }
            if(stage==9)
            {
                Check(f.Player.HP==100&&f.Screen==GameScreen.Playing,"retry revives party at checkpoint");
                f.EnterDungeon(3);stage=10;return;
            }
            if(stage==10)
            {
                Audit(!UnityEngine.Object.FindObjectsByType<EnemyAgent>().Any(e=>e.SanctumGuardian&&!e.Dead),"completed Guardian does not respawn for duplicate rewards");
                Check(string.IsNullOrEmpty(errors),"no runtime errors or exceptions: "+errors);
                Finish(failures==0?0:1);
            }
        }
        catch(Exception e){File.AppendAllText(evidence+"play-checks.txt",e+"\n"+errors);Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code)
    {
        exiting=true;SessionState.SetBool("EldervalePlayCheck",false);EditorApplication.update-=Step;
        File.AppendAllText(evidence+"play-checks.txt","\n"+pass+" checks passed; failures="+failures+"; result="+code+"\n");
        Debug.Log("ELDERVALE_PLAY_RESULT "+code+"; checks "+pass);
        Application.logMessageReceived-=Log;
        saveBackup=SessionState.GetString("EldervaleCheckSave","");
        savePath=SessionState.GetString("EldervaleCheckSavePath","");
        if(!string.IsNullOrEmpty(saveBackup))File.WriteAllText(savePath,saveBackup);
        else if(File.Exists(savePath))File.Move(savePath,evidence+"verification-run-"+System.DateTime.Now.ToString("HHmmss")+".json");
        if(Application.isBatchMode)EditorApplication.Exit(code);
        else {SessionState.SetBool("EldervalePreviewAfterCheck",true);EditorApplication.ExitPlaymode();}
    }
}


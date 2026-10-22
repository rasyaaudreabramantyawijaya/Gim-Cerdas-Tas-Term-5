using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Local editor-only command mailbox; never included in the player build.
[InitializeOnLoad]
public static class EldervaleEditorControl
{
    const string Command="ProjectSettings/EldervaleCommand.txt";
    static string pending;
    static double previewAt;
    static EldervaleEditorControl()
    {
        pending=SessionState.GetString("EldervalePendingCommand","");if(pending=="")pending=null;
        EditorApplication.update+=Update;
        EditorApplication.playModeStateChanged+=state=>
        {if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("EldervalePreviewAfterCheck",false)){SessionState.SetBool("EldervalePreviewAfterCheck",false);pending="preview";}};
    }
    static void Update()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(previewAt>0&&EditorApplication.timeSinceStartup>previewAt&&!EditorApplication.isPlayingOrWillChangePlaymode){previewAt=0;EditorApplication.EnterPlaymode();return;}
        if(File.Exists(Command)&&Directory.GetFiles("Assets/Eldervale","*.cs",SearchOption.AllDirectories).Any(p=>File.GetLastWriteTimeUtc(p)>File.GetLastWriteTimeUtc(p.Replace('\\','/').Contains("/Editor/")?"Library/ScriptAssemblies/Assembly-CSharp-Editor.dll":"Library/ScriptAssemblies/Assembly-CSharp.dll")))
        {AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);return;}
        if(pending==null&&File.Exists(Command)){pending=File.ReadAllText(Command).Trim();SessionState.SetString("EldervalePendingCommand",pending);File.Delete(Command);}
        if(pending==null)return;
        if(EditorApplication.isPlaying&&pending.StartsWith("live-"))
        {
            var liveAction=pending;pending=null;SessionState.EraseString("EldervalePendingCommand");
            if(liveAction=="live-shot")ScreenCapture.CaptureScreenshot(Path.GetFullPath("docs/evidence/native-game-view.png"));
            if(liveAction=="live-state")
            {
                var cameras=Camera.allCameras;
                var cameraInfo=string.Join(" | ",System.Array.ConvertAll(cameras,c=>c.name+" enabled="+c.enabled+" active="+c.gameObject.activeInHierarchy+" target="+(c.targetTexture==null?"display":"texture")+" mask="+c.cullingMask));
                Debug.Log("ELDERVALE_RUNTIME_STATE scene="+SceneManager.GetActiveScene().name+" screen="+(GameFlow.I==null?"missing":GameFlow.I.Screen.ToString())+" timeScale="+Time.timeScale+" renderPipeline="+(GraphicsSettings.currentRenderPipeline==null?"missing":GraphicsSettings.currentRenderPipeline.name)+" cameras="+cameras.Length+" ["+cameraInfo+"]");
                foreach(var cape in UnityEngine.Object.FindObjectsByType<FlowingCape>(FindObjectsSortMode.None))
                {
                    var up=cape.Hips!=null&&cape.Head!=null?(cape.Head.position-cape.Hips.position).normalized:cape.Avatar.up;
                    var right=cape.LeftShoulder!=null&&cape.RightShoulder!=null?(cape.RightShoulder.position-cape.LeftShoulder.position).normalized:cape.Avatar.right;
                    var back=-Vector3.Cross(right,up).normalized;
                    var view=cameras.OrderBy(c=>Vector3.Distance(c.transform.position,cape.transform.position)).FirstOrDefault();
                    var dot=view==null?0:Vector3.Dot(back,(view.transform.position-cape.transform.position).normalized);
                    Debug.Log("ELDERVALE_CAPE position="+cape.transform.position+" shoulders="+cape.LeftShoulder.position+"/"+cape.RightShoulder.position+" back="+back+" cameraDot="+dot+" bounds="+cape.GetComponent<Renderer>().bounds.size);
                }
            }
            if(liveAction=="live-select")GameFlow.I.SetScreen(GameScreen.Select);
            if(liveAction=="live-cape-start")GameFlow.I.NewRun(0);
            if(liveAction=="live-cape-shot")ScreenCapture.CaptureScreenshot(Path.GetFullPath("Library/cape-review.png"));
            if(liveAction=="live-rules")EldervaleVerification.Rules();
            if(liveAction=="live-audio")GameAudio.I.TestDirections();
            return;
        }
        if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();return;}
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var action=pending;pending=null;SessionState.EraseString("EldervalePendingCommand");
        Debug.Log("ELDERVALE_EDITOR_COMMAND "+action);
        if(action=="verify")EldervalePlayVerification.Run();
        else if(action=="rules")EldervaleVerification.Rules();
        else if(action=="build")EldervaleSceneBuilder.BuildMac();
        else if(action=="preview")EldervaleSceneBuilder.Preview();
        else if(action=="scene-list")EldervaleSceneBuilder.ConfigureBuildSceneList();
        else if(action=="pipeline")EldervaleSceneBuilder.ConfigureRenderPipeline();
        else if(action=="scenes")EldervaleSceneBuilder.Build();
        else if(action=="assets")EldervaleSceneBuilder.ConfigurePresentationAssets();
    }
    [MenuItem("Eldervale/Play preview")]
    public static void Preview(){pending="preview";}
    [MenuItem("Eldervale/Run full Play checks")]
    public static void Verify(){pending="verify";}
    [MenuItem("Eldervale/Build macOS player")]
    public static void Build(){pending="build";}
    public static void SchedulePreview(){previewAt=EditorApplication.timeSinceStartup+1.5;}
}

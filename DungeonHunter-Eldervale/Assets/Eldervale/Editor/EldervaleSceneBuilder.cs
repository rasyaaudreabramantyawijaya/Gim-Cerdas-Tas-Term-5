using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EldervaleSceneBuilder
{
    public static void Preview()
    {
        SessionState.SetBool("EldervalePlayCheck",false);
        EditorSceneManager.OpenScene("Assets/Eldervale/Scenes/Frontend.unity");
        var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if(type!=null){var window=EditorWindow.GetWindow(type);window.Show();var gizmos=type.GetProperty("drawGizmos",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);if(gizmos!=null&&gizmos.CanWrite)gizmos.SetValue(window,false);}
        EldervaleEditorControl.SchedulePreview();
    }
    [MenuItem("Eldervale/Configure build scene list")]
    public static void ConfigureBuildSceneList()
    {
        var names=new[]{"Frontend","EldervaleOverworld","AshenCrypt","WhisperingGrotto","VeyrSanctum"};
        var scenes=names.Select(name=>"Assets/Eldervale/Scenes/"+name+".unity").ToArray();
        foreach(var scene in scenes)if(!File.Exists(scene))throw new FileNotFoundException("Required game scene is missing.",scene);
        EditorBuildSettings.scenes=scenes.Select(path=>new EditorBuildSettingsScene(path,true)).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("ELDERVALE_BUILD_SCENES_CONFIGURED "+string.Join(", ",names));
    }
    [MenuItem("Eldervale/Configure URP")]
    public static void ConfigureRenderPipeline()
    {
        var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
        if(pipeline==null)throw new FileNotFoundException("The PC URP asset is missing.","Assets/Settings/PC_RPAsset.asset");
        GraphicsSettings.defaultRenderPipeline=pipeline;
        QualitySettings.renderPipeline=pipeline;
        AssetDatabase.SaveAssets();
        Debug.Log("ELDERVALE_URP_CONFIGURED "+AssetDatabase.GetAssetPath(pipeline));
    }
    [MenuItem("Eldervale/Build project scenes")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Eldervale/Scenes");
        PlayerSettings.productName="Dungeon Hunter - Relic of Veyr";
        PlayerSettings.companyName="Eldervale Studio";
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.runInBackground=true;
        var ps=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=ps.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;ps.ApplyModifiedPropertiesWithoutUndo();}
        ConfigureModels();
        var names=new[]{"Frontend","EldervaleOverworld","AshenCrypt","WhisperingGrotto","VeyrSanctum"};
        foreach(var name in names)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("World — "+name);root.AddComponent<WorldBuilder>().World=name;
            EditorSceneManager.SaveScene(scene,"Assets/Eldervale/Scenes/"+name+".unity");
        }
        EditorBuildSettings.scenes=names.Select(name=>new EditorBuildSettingsScene("Assets/Eldervale/Scenes/"+name+".unity",true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Eldervale/Scenes/Frontend.unity");
        Debug.Log("ELDERVALE_SCENES_READY");
    }
    static void ConfigureModels()
    {
        string[] guids=AssetDatabase.FindAssets("t:Model",new[]{"Assets/Eldervale/Resources/Humanoid","Assets/Eldervale/Resources/Animations"});
        foreach(var guid in guids)
        {
            string p=AssetDatabase.GUIDToAssetPath(guid);
            if(!p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))continue;
            var importer=AssetImporter.GetAtPath(p) as ModelImporter;
            if(importer==null)continue;
            importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=p.Contains("Animations/");importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            if(p.Contains("Animations/"))
            {
                var animations=importer.clipAnimations.Length>0?importer.clipAnimations:importer.defaultClipAnimations;
                foreach(var clip in animations){clip.loopTime=clip.name.Contains("_Loop");clip.loopPose=clip.loopTime;clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;}
                importer.clipAnimations=animations;
            }
            if(p.Contains("Hair_")) {importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;}
            importer.SaveAndReimport();
        }
        foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Eldervale/Resources"}))
        {
            string p=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(p) as TextureImporter;
            if(importer==null)continue;
            importer.maxTextureSize=1024;
            if(p.Contains("_nor_gl_")||p.ToLower().Contains("_normal"))importer.textureType=TextureImporterType.NormalMap;
            if(p.Contains("_rough_")||p.Contains("_ao_"))importer.sRGBTexture=false;
            importer.SaveAndReimport();
        }
        string controllerPath="Assets/Eldervale/Resources/HunterAnimator.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var clips=AssetDatabase.LoadAllAssetsAtPath("Assets/Eldervale/Resources/Animations/UAL1_Standard.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
        Debug.Log("ELDERVALE_CLIPS "+string.Join(", ",clips.Select(c=>c.name)));
        var machine=controller.layers[0].stateMachine;
        foreach(var pair in new[]{new[]{"Idle","Idle_Loop"},new[]{"Walk","Walk_Loop"},new[]{"Run","Sprint_Loop"},new[]{"Jump","Jump_Start"},new[]{"Attack","Sword_Attack"},new[]{"Cast","Spell_Simple_Shoot"},new[]{"Dead","Death01"}})
        {
            var state=machine.states.Select(x=>x.state).FirstOrDefault(s=>s.name==pair[0])??machine.AddState(pair[0]);
            var clip=clips.FirstOrDefault(c=>c.name.EndsWith("|"+pair[1],StringComparison.OrdinalIgnoreCase))
                ??clips.FirstOrDefault(c=>c.name.IndexOf(pair[0],StringComparison.OrdinalIgnoreCase)>=0);
            if(clip==null && pair[0]=="Attack")clip=clips.FirstOrDefault(c=>c.name.Contains("Punch")||c.name.Contains("Melee"));
            if(clip==null && pair[0]=="Cast")clip=clips.FirstOrDefault(c=>c.name.Contains("Wave"));
            state.motion=clip;
            if(pair[0]=="Idle")machine.defaultState=state;
        }
        EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
    }
    public static void BuildMac()
    {
        EldervaleVerification.Rules();
        // Earlier red-test evidence stays in docs; show a fresh Console for the verified build.
        typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)?.Invoke(null,null);
        RetainRuntimeShaders();
        Directory.CreateDirectory("Builds/Mac");
        var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),"Builds/Mac/DungeonHunter-Eldervale.app",BuildTarget.StandaloneOSX,BuildOptions.Development);
        Directory.CreateDirectory("docs/evidence");
        File.WriteAllText("docs/evidence/build-result.txt",report.summary.result+"; errors="+report.summary.totalErrors+"; warnings="+report.summary.totalWarnings);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Standalone build failed.");
        Debug.Log("ELDERVALE_BUILD_SUCCESS");
    }
    public static void ConfigurePresentationAssets()
    {
        ConfigureModels();
        foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Eldervale/Resources"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(importer==null)continue;
            bool music=path.Contains("/Music/");var settings=importer.defaultSampleSettings;
            settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.quality=1;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings=settings;importer.forceToMono=!music;importer.loadInBackground=music;importer.SaveAndReimport();
        }
        foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Eldervale/Resources/Textures","Assets/Eldervale/Resources/Nature"}))
        {
            string p=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(p) as TextureImporter;if(importer==null)continue;
            importer.maxTextureSize=1024;if(p.Contains("_nor_gl_")||p.EndsWith("_Normal.png"))importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();
        }
        RetainRuntimeShaders();
        Debug.Log("ELDERVALE_PRESENTATION_ASSETS_READY");
    }
    static void RetainRuntimeShaders()
    {
        const string folder="Assets/Eldervale/Resources/Scenery";
        Directory.CreateDirectory(folder);
        foreach(string kind in new[]{"Lit","Normal","Emission","Leaves","Mist","Sky","Terrain"})
        {
            string path=folder+"/Retained"+kind+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null)
            {
                var shader=Shader.Find(kind=="Terrain"?"Universal Render Pipeline/Terrain/Lit":kind=="Sky"?"Skybox/Panoramic":kind=="Mist"?"Universal Render Pipeline/Particles/Unlit":"Universal Render Pipeline/Lit");
                if(shader==null)throw new Exception("Missing build shader: "+kind);
                material=new Material(shader);AssetDatabase.CreateAsset(material,path);
            }
            if(kind=="Normal")material.EnableKeyword("_NORMALMAP");
            if(kind=="Emission")material.EnableKeyword("_EMISSION");
            if(kind=="Leaves"){material.SetFloat("_AlphaClip",1);material.EnableKeyword("_ALPHATEST_ON");material.SetFloat("_Cull",0);}
            if(kind=="Sky")material.SetTexture("_MainTex",Resources.Load<Texture>("Sky/MountainSky"));
            if(kind=="Mist"){material.SetFloat("_Surface",1);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");}
            EditorUtility.SetDirty(material);
        }
        string volumePath=folder+"/SanctuaryVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumePath);
        if(profile==null)
        {
            profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,volumePath);
            var bloom=profile.Add<Bloom>();bloom.intensity.Override(.35f);bloom.threshold.Override(1.1f);
            profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
            profile.Add<Vignette>().intensity.Override(.2f);
            foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);
            EditorUtility.SetDirty(profile);
        }
        AssetDatabase.SaveAssets();
    }
}


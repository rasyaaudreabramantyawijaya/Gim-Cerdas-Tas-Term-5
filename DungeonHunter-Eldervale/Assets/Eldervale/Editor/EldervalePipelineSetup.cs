using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
[InitializeOnLoad]
public static class EldervalePipelineSetup
{
    const string Marker="ProjectSettings/EldervaleSetup.pending";
    static EldervalePipelineSetup()
    {
        if(File.Exists(Marker))EditorApplication.update+=Configure;
    }
    static void Configure()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
        var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
        if(pipeline==null)return;
        GraphicsSettings.defaultRenderPipeline=pipeline;
        QualitySettings.renderPipeline=pipeline;
        AssetDatabase.SaveAssets();
        File.Delete(Marker);EditorApplication.update-=Configure;
        Debug.Log("ELDERVALE_URP_CONFIGURED");
        if(!Application.isBatchMode)EldervaleSceneBuilder.Preview();
    }
}


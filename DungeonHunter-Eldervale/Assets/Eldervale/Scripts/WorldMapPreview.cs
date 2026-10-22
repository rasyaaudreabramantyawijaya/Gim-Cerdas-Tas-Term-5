using UnityEngine;

public sealed class WorldMapPreview:MonoBehaviour
{
    public RenderTexture Texture {get;private set;}
    Camera mapCamera;
    void Start()
    {
        Texture=new RenderTexture(768,512,24){name="Eldervale live terrain map"};Texture.Create();
        var g=new GameObject("Terrain map camera");g.transform.SetParent(transform,false);g.transform.position=new Vector3(0,145,0);g.transform.rotation=Quaternion.Euler(90,0,0);
        mapCamera=g.AddComponent<Camera>();mapCamera.orthographic=true;mapCamera.orthographicSize=105;mapCamera.nearClipPlane=.3f;mapCamera.farClipPlane=180;mapCamera.cullingMask=~(1<<29);mapCamera.targetTexture=Texture;mapCamera.clearFlags=CameraClearFlags.SolidColor;mapCamera.backgroundColor=new Color(.07f,.12f,.12f);mapCamera.enabled=false;
    }
    void LateUpdate(){if(mapCamera!=null)mapCamera.enabled=GameFlow.I!=null&&GameFlow.I.Screen==GameScreen.Map&&GameFlow.I.InTown;}
    void OnDestroy(){if(Texture!=null){Texture.Release();Destroy(Texture);}}
}

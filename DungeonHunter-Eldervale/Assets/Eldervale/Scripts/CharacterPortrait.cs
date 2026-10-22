using UnityEngine;

// Dedicated live portrait render, isolated from world navigation and audio.
public sealed class CharacterPortrait:MonoBehaviour
{
    public RenderTexture Texture {get;private set;}
    GameObject studio,avatar;
    Camera portraitCamera;
    int profile=-1;
    void Start()
    {
        studio=new GameObject("Character portrait studio");studio.transform.SetParent(transform);studio.transform.position=new Vector3(5000,0,0);
        Texture=new RenderTexture(560,700,24,RenderTextureFormat.ARGB32){name="Live character portrait"};Texture.Create();
        var cameraObject=new GameObject("Portrait camera");cameraObject.transform.SetParent(studio.transform,false);cameraObject.transform.localPosition=new Vector3(0,1.1f,3.25f);cameraObject.transform.LookAt(studio.transform.position+Vector3.up*.95f);
        portraitCamera=cameraObject.AddComponent<Camera>();portraitCamera.clearFlags=CameraClearFlags.SolidColor;portraitCamera.backgroundColor=new Color(.025f,.045f,.04f,0);portraitCamera.fieldOfView=37;portraitCamera.nearClipPlane=.1f;portraitCamera.farClipPlane=10;portraitCamera.cullingMask=1<<29;portraitCamera.targetTexture=Texture;
        AddLight(new Vector3(-2,3,3),new Color(1,.86f,.68f),12f);
        AddLight(new Vector3(2,2,-1),new Color(.32f,.77f,.78f),8f);
        var plinth=GameObject.CreatePrimitive(PrimitiveType.Cylinder);plinth.name="Character display plinth";plinth.transform.SetParent(studio.transform,false);plinth.transform.localPosition=new Vector3(0,-.05f,0);plinth.transform.localScale=new Vector3(1.25f,.05f,1.25f);plinth.layer=29;Destroy(plinth.GetComponent<Collider>());
        var stone=RuntimeResources.Own(plinth,new Material(Shader.Find("Universal Render Pipeline/Lit")));stone.color=new Color(.16f,.2f,.19f);stone.SetFloat("_Smoothness",.24f);plinth.GetComponent<Renderer>().sharedMaterial=stone;
    }
    void AddLight(Vector3 p,Color color,float intensity)
    {var o=new GameObject("Portrait softbox");o.transform.SetParent(studio.transform,false);o.transform.localPosition=p;var light=o.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=7;light.cullingMask=1<<29;}
    void Update()
    {
        var f=GameFlow.I;if(f==null||studio==null)return;
        bool visible=f.Screen==GameScreen.Select||f.Screen==GameScreen.Party;
        studio.SetActive(visible);if(!visible)return;
        int requested=f.Screen==GameScreen.Select?GetComponent<Presentation>().SelectedCharacter:f.State.character;
        if(profile!=requested)
        {
            profile=requested;if(avatar!=null)Destroy(avatar);
            avatar=new GameObject("Portrait — "+GameFlow.Names[profile]);avatar.transform.SetParent(studio.transform,false);
            avatar.AddComponent<HumanoidView>().Build(profile,false);
            foreach(var child in avatar.GetComponentsInChildren<Transform>(true))child.gameObject.layer=29;
            foreach(var animator in avatar.GetComponentsInChildren<Animator>())animator.updateMode=AnimatorUpdateMode.UnscaledTime;
            foreach(var collider in avatar.GetComponentsInChildren<Collider>())collider.enabled=false;
        }
        if(avatar!=null)avatar.transform.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.26f)*13,0);
    }
    void OnDestroy(){if(Texture!=null){Texture.Release();Destroy(Texture);}if(studio!=null)Destroy(studio);}
}

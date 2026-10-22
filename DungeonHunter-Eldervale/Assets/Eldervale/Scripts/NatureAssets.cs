using System.Collections.Generic;
using UnityEngine;

public static class NatureAssets
{
    public static bool Tree(Transform parent,Vector3 position,float height)
    {
        string kind=Random.value>.4f?"NormalTree":"BirchTree";
        var model=Resources.Load<GameObject>("Nature/"+kind+"_1");if(model==null)return false;
        var root=new GameObject("Wind imported "+kind);root.transform.SetParent(parent,false);root.transform.position=position;
        var graphic=Object.Instantiate(model,root.transform);graphic.transform.localPosition=Vector3.zero;
        var renderers=graphic.GetComponentsInChildren<Renderer>();if(renderers.Length==0){Object.Destroy(root);return false;}
        var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
        float scale=height/Mathf.Max(.1f,bounds.size.y);graphic.transform.localScale*=scale;
        graphic.transform.localPosition-=Vector3.up*(bounds.min.y-position.y)*scale;
        root.transform.localRotation=Quaternion.Euler(0,Random.Range(0,360),0);
        foreach(var renderer in renderers)
        {
            var slots=renderer.sharedMaterials;
            for(int i=0;i<slots.Length;i++)
            {
                bool leaf=slots[i].name.ToLower().Contains("lea");string texture=kind+(leaf?"_Leaves":"_Bark");
                slots[i]=GetMaterial(parent.root.gameObject,texture,leaf);
            }
            renderer.sharedMaterials=slots;
        }
        graphic.AddComponent<WindMotion>().kind=1;
        var solid=new GameObject("Pine trunk");solid.transform.SetParent(root.transform,false);
        var collider=solid.AddComponent<CapsuleCollider>();collider.radius=height*.023f;collider.height=height*.6f;collider.center=Vector3.up*height*.3f;
        return true;
    }
    public static bool Grass(Transform parent,Vector3 position,float height)
    {
        var model=Resources.Load<GameObject>("Nature/Grass_Small");if(model==null)return false;
        var graphic=Object.Instantiate(model,parent);graphic.name="Wind imported grass";graphic.transform.position=position;
        graphic.transform.localScale*=height;graphic.transform.rotation=Quaternion.Euler(0,Random.Range(0,360),0)*graphic.transform.rotation;
        foreach(var renderer in graphic.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=GetMaterial(parent.root.gameObject,"Grass",false);
        graphic.AddComponent<WindMotion>();return true;
    }
    static Material GetMaterial(GameObject owner,string texture,bool alpha)
    {
        // Assets from the outgoing scene can still be alive during the new scene's Awake.
        // Scope the material cache to this environment so teardown cannot invalidate new trees.
        var library=owner.GetComponent<NatureMaterialLibrary>();if(library==null)library=owner.AddComponent<NatureMaterialLibrary>();
        var materials=library.Materials;
        if(materials.TryGetValue(texture,out var cached)&&cached!=null)return cached;
        var m=RuntimeResources.Own(owner,new Material(Shader.Find("Universal Render Pipeline/Lit")));
        m.name="Nature — "+texture;m.mainTexture=Resources.Load<Texture2D>("Nature/"+texture);m.color=Color.white;m.SetFloat("_Smoothness",.12f);
        var normal=Resources.Load<Texture2D>("Nature/"+texture+"_Normal");if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
        if(alpha){m.SetFloat("_AlphaClip",1);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",.45f);m.SetFloat("_Cull",0);m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");}
        materials[texture]=m;return m;
    }
}
public sealed class NatureMaterialLibrary:MonoBehaviour
{
    public readonly Dictionary<string,Material> Materials=new Dictionary<string,Material>();
}

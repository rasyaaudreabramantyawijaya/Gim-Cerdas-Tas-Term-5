using System.Collections.Generic;
using UnityEngine;

// Fitted adventurer accents that complement the rigged humanoid model
// without swallowing the body in oversized geometric barrels.
public sealed class AdventurerOutfit : MonoBehaviour
{
    sealed class Section { public Transform a,b;public Mesh mesh;public Vector3[] vertices;public float r0,r1,depth,start,end; }
    readonly List<Section> sections=new List<Section>();
    Transform avatar;
    public void Build(Animator rig,int profile,bool enemy)
    {
        avatar=transform;
        if(rig==null||!rig.isHuman)return;
        var leather=Surface(new Color(.16f,.10f,.06f),.35f);
        var armour=Surface(enemy?new Color(.35f,.18f,.18f):profile==1?new Color(.2f,.45f,.42f):profile==2?new Color(.48f,.38f,.22f):new Color(.32f,.35f,.38f),.6f);
        armour.SetFloat("_Metallic",.6f);
        var hips=rig.GetBoneTransform(HumanBodyBones.Hips);
        
        // Sleek leather waist belt with brass buckle accent
        if(hips!=null)
        {
            Add("Leather waist belt",hips,hips,.195f,.195f,.62f,.02f,.08f,leather);
            var buckle=GameObject.CreatePrimitive(PrimitiveType.Cube);
            buckle.name="Belt buckle";buckle.transform.SetParent(transform,false);
            Destroy(buckle.GetComponent<Collider>());
            var bmat=Surface(new Color(.82f,.68f,.28f),.8f);bmat.SetFloat("_Metallic",.85f);
            buckle.GetComponent<Renderer>().sharedMaterial=bmat;
            buckle.transform.localScale=new Vector3(.09f,.06f,.03f);
            // Parent to hips if possible
            buckle.transform.SetParent(hips,false);
            buckle.transform.localPosition=new Vector3(0,.05f,.18f);
        }
        
        // Sleek shoulder pauldrons and vambraces (clean, fitted)
        foreach(bool left in new[]{true,false})
        {
            var arm=rig.GetBoneTransform(left?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm);
            var elbow=rig.GetBoneTransform(left?HumanBodyBones.LeftLowerArm:HumanBodyBones.RightLowerArm);
            var hand=rig.GetBoneTransform(left?HumanBodyBones.LeftHand:HumanBodyBones.RightHand);
            if(arm!=null&&elbow!=null)
            {
                Add("Shoulder pauldron",arm,elbow,.095f,.072f,.95f,0f,-.08f,armour);
            }
            if(elbow!=null&&hand!=null)
            {
                Add("Hunter bracer",elbow,hand,.068f,.055f,.95f,-.04f,-.04f,leather);
            }
        }
    }
    Material Surface(Color color,float smoothness)
    {
        var m=RuntimeResources.Own(gameObject,new Material(Shader.Find("Universal Render Pipeline/Lit")));
        m.color=color;m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Cull",0);
        return m;
    }
    void Add(string label,Transform a,Transform b,float r0,float r1,float depth,float start,float end,Material material)
    {
        if(a==null||b==null)return;
        const int sides=14,rings=4;
        var g=new GameObject(label);g.transform.SetParent(transform,false);
        var mesh=RuntimeResources.Own(g,new Mesh{name=label});mesh.MarkDynamic();
        var vertices=new Vector3[(rings+1)*sides];var triangles=new int[rings*sides*6];var uv=new Vector2[vertices.Length];int k=0;
        for(int j=0;j<=rings;j++)for(int i=0;i<sides;i++)
        {
            int n=j*sides+i;uv[n]=new Vector2(i/(float)sides,j/(float)rings);
            if(j<rings)
            {
                int next=j*sides+(i+1)%sides;
                triangles[k++]=n;triangles[k++]=next;triangles[k++]=n+sides;
                triangles[k++]=next;triangles[k++]=next+sides;triangles[k++]=n+sides;
            }
        }
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        sections.Add(new Section{a=a,b=b,mesh=mesh,vertices=vertices,r0=r0,r1=r1,depth=depth,start=start,end=end});
    }
    void LateUpdate()
    {
        foreach(var s in sections)
        {
            if(s.a==null||s.b==null)continue;
            Vector3 axis=s.a==s.b?avatar.up:(s.b.position-s.a.position).normalized;
            Vector3 side=Vector3.ProjectOnPlane(avatar.right,axis).normalized;
            if(side.sqrMagnitude<.05f)side=Vector3.ProjectOnPlane(avatar.forward,axis).normalized;
            var depth=Vector3.Cross(side,axis).normalized;
            var start=s.a.position+axis*s.start;var end=s.b.position+axis*s.end;
            for(int j=0;j<=4;j++)for(int i=0;i<14;i++)
            {
                float t=j/4f,angle=i/14f*Mathf.PI*2,r=Mathf.Lerp(s.r0,s.r1,t);
                var p=Vector3.Lerp(start,end,t)+side*(Mathf.Cos(angle)*r)+depth*(Mathf.Sin(angle)*r*s.depth);
                s.vertices[j*14+i]=transform.InverseTransformPoint(p);
            }
            s.mesh.vertices=s.vertices;s.mesh.RecalculateNormals();s.mesh.RecalculateBounds();
        }
    }
}

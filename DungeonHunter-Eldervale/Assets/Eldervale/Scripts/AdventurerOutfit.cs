using System.Collections.Generic;
using UnityEngine;

// Fitted adventurer accents that complement the rigged humanoid model
// without swallowing the body in oversized geometric barrels.
public sealed class AdventurerOutfit : MonoBehaviour
{
    sealed class Section { public Transform a,b;public Mesh mesh;public Vector3[] vertices;public float r0,r1,depth,start,end,frontOffset; }
    readonly List<Section> sections=new List<Section>();
    Transform avatar;
    public void Build(Animator rig,int profile,bool enemy)
    {
        avatar=transform;
        if(rig==null||!rig.isHuman)return;
        var cloth=Surface(enemy?new Color(.27f,.11f,.10f):profile==1?new Color(.20f,.31f,.23f):profile==2?new Color(.34f,.26f,.17f):new Color(.17f,.23f,.31f),.24f);
        var leather=Surface(enemy?new Color(.16f,.07f,.06f):new Color(.20f,.12f,.07f),.28f);
        var armour=Surface(enemy?new Color(.34f,.18f,.16f):profile==1?new Color(.28f,.34f,.27f):profile==2?new Color(.40f,.32f,.22f):new Color(.34f,.38f,.42f),.38f);
        armour.SetFloat("_Metallic",profile==0 && !enemy ? .28f : .12f);
        var brass=Surface(new Color(.65f,.47f,.20f),.52f);brass.SetFloat("_Metallic",.58f);
        var hips=rig.GetBoneTransform(HumanBodyBones.Hips);
        var chest=rig.GetBoneTransform(HumanBodyBones.UpperChest);
        if(chest==null)chest=rig.GetBoneTransform(HumanBodyBones.Chest);
        if(chest==null)chest=rig.GetBoneTransform(HumanBodyBones.Spine);

        // A fitted wool tunic gives the bare base body a grounded medieval silhouette.
        if(hips!=null&&chest!=null)
            Add("Wool hunter tunic",hips,chest,.19f,.225f,.92f,-.035f,-.12f,cloth);

        // Narrow leather harness straps sit on top of the tunic like practical field gear.
        var leftShoulder=rig.GetBoneTransform(HumanBodyBones.LeftShoulder);
        var rightShoulder=rig.GetBoneTransform(HumanBodyBones.RightShoulder);
        if(leftShoulder==null)leftShoulder=rig.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        if(rightShoulder==null)rightShoulder=rig.GetBoneTransform(HumanBodyBones.RightUpperArm);
        if(hips!=null&&leftShoulder!=null)
            Add("Cross-body leather harness",leftShoulder,hips,.025f,.025f,.42f,-.015f,.10f,leather,.13f);
        if(hips!=null&&rightShoulder!=null)
            Add("Cross-body leather harness",rightShoulder,hips,.025f,.025f,.42f,-.015f,.10f,leather,.13f);

        // A plain belt and small brass fastening break up the tunic at the waist.
        if(hips!=null)
        {
            Add("Leather waist belt",hips,hips,.176f,.176f,.72f,.005f,.065f,leather);
            AttachPrimitive(hips,"Brass belt buckle",PrimitiveType.Cube,new Vector3(0,.035f,.178f),new Vector3(.075f,.052f,.025f),brass,Quaternion.identity);
        }

        // Cloth sleeves, fitted bracers, and small shoulder fasteners follow the arm rig.
        foreach(bool left in new[]{true,false})
        {
            var arm=rig.GetBoneTransform(left?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm);
            var elbow=rig.GetBoneTransform(left?HumanBodyBones.LeftLowerArm:HumanBodyBones.RightLowerArm);
            var hand=rig.GetBoneTransform(left?HumanBodyBones.LeftHand:HumanBodyBones.RightHand);
            var shoulder=rig.GetBoneTransform(left?HumanBodyBones.LeftShoulder:HumanBodyBones.RightShoulder);
            if(shoulder==null)shoulder=arm;
            if(arm!=null&&elbow!=null)
            {
                Add("Tunic sleeve",arm,elbow,.075f,.061f,.82f,-.015f,-.015f,cloth);
                Add("Leather shoulder guard",arm,arm,.09f,.09f,.72f,.015f,.075f,armour);
            }
            if(elbow!=null&&hand!=null)
            {
                Add("Leather forearm bracer",elbow,hand,.061f,.048f,.82f,-.025f,-.035f,leather);
                Add("Brass bracer trim",elbow,elbow,.063f,.063f,.84f,.015f,.04f,brass);
            }
            if(shoulder!=null)
                AttachPrimitive(shoulder,"Cape shoulder clasp",PrimitiveType.Sphere,new Vector3(0,0,.045f),new Vector3(.058f,.048f,.025f),brass,Quaternion.identity);
        }

        // Trousers and tall riding boots cover the remaining exposed body mesh.
        foreach(bool left in new[]{true,false})
        {
            var upperLeg=rig.GetBoneTransform(left?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg);
            var lowerLeg=rig.GetBoneTransform(left?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg);
            var foot=rig.GetBoneTransform(left?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);
            if(upperLeg!=null&&lowerLeg!=null)
            {
                Add("Wool trousers",upperLeg,lowerLeg,.12f,.09f,.9f,-.015f,-.015f,cloth);
            }
            if(lowerLeg!=null&&foot!=null)
            {
                Add("Leather boot shaft",lowerLeg,foot,.078f,.082f,.86f,-.025f,-.005f,leather);
                var toePosition=foot.position+avatar.forward*.065f-avatar.up*.015f;
                var toeRotation=Quaternion.Inverse(foot.rotation)*avatar.rotation;
                AttachPrimitive(foot,"Leather boot toe",PrimitiveType.Sphere,foot.InverseTransformPoint(toePosition),new Vector3(.085f,.065f,.14f),leather,toeRotation);
            }
        }
    }
    void AttachPrimitive(Transform parent,string label,PrimitiveType type,Vector3 localPosition,Vector3 localScale,Material material,Quaternion localRotation)
    {
        if(parent==null)return;
        var piece=GameObject.CreatePrimitive(type);piece.name=label;piece.transform.SetParent(parent,false);
        piece.transform.localPosition=localPosition;piece.transform.localRotation=localRotation;piece.transform.localScale=localScale;
        piece.GetComponent<Renderer>().sharedMaterial=material;Destroy(piece.GetComponent<Collider>());
    }
    Material Surface(Color color,float smoothness)
    {
        var m=RuntimeResources.Own(gameObject,new Material(Shader.Find("Universal Render Pipeline/Lit")));
        m.color=color;m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Cull",0);
        return m;
    }
    void Add(string label,Transform a,Transform b,float r0,float r1,float depth,float start,float end,Material material,float frontOffset=0f)
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
                // Winding faces outward from the clothing shell; inward normals
                // made the garment shade as if it were buried inside the body.
                triangles[k++]=n;triangles[k++]=n+sides;triangles[k++]=next;
                triangles[k++]=next;triangles[k++]=n+sides;triangles[k++]=next+sides;
            }
        }
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        sections.Add(new Section{a=a,b=b,mesh=mesh,vertices=vertices,r0=r0,r1=r1,depth=depth,start=start,end=end,frontOffset=frontOffset});
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
            var start=s.a.position+axis*s.start+avatar.forward*s.frontOffset;var end=s.b.position+axis*s.end+avatar.forward*s.frontOffset;
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

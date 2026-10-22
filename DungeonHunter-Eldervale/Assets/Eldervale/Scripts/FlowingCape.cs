using UnityEngine;

// The shoulder edge stays attached; only the lower cloth follows the breeze.
public sealed class FlowingCape : MonoBehaviour
{
    Mesh mesh;
    Vector3[] rest,vertices;
    public Transform Shoulder,LeftShoulder,RightShoulder,Hips,Head,Avatar;
    public void Build(Material material)
    {
        const int cols=8,rows=12;
        rest=new Vector3[(cols+1)*(rows+1)];vertices=new Vector3[rest.Length];
        var uv=new Vector2[rest.Length];var triangles=new int[cols*rows*6];int k=0;
        for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++)
        {
            float v=y/(float)rows,u=x/(float)cols;int i=y*(cols+1)+x;
            // Keep the silhouette close to a practical cloak, with a shallow bow
            // away from the back so the cloth cannot slice through the torso.
            rest[i]=new Vector3((u-.5f)*Mathf.Lerp(.42f,.68f,v),-v*.94f,-.01f-v*.025f-Mathf.Sin(u*Mathf.PI)*.10f+Mathf.Cos(u*Mathf.PI*6)*.012f*v);
            uv[i]=new Vector2(u,v);
            if(x<cols&&y<rows){triangles[k++]=i;triangles[k++]=i+cols+1;triangles[k++]=i+1;triangles[k++]=i+1;triangles[k++]=i+cols+1;triangles[k++]=i+cols+2;}
        }
        mesh=RuntimeResources.Own(gameObject,new Mesh{name="Tailored flowing cape"});mesh.vertices=rest;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    void LateUpdate()
    {
        if(mesh==null)return;float wind=GameFlow.I!=null?GameFlow.I.Wind:.5f;
        if(Avatar!=null)
        {
            var up=Hips!=null&&Head!=null?(Head.position-Hips.position).normalized:Avatar.up;
            var right=LeftShoulder!=null&&RightShoulder!=null?(RightShoulder.position-LeftShoulder.position).normalized:Avatar.right;
            var forward=Vector3.Cross(right,up).normalized;
            if(forward.sqrMagnitude<.01f)forward=Avatar.forward;
            var back=-forward;
            var anchor=LeftShoulder!=null&&RightShoulder!=null?(LeftShoulder.position+RightShoulder.position)*.5f:Shoulder!=null?Shoulder.position:transform.position;
            transform.position=anchor+up*.015f+back*.08f;
            transform.rotation=Quaternion.LookRotation(forward,up);
        }
        const int cols=8;
        var bodyUp=Hips!=null&&Head!=null?(Head.position-Hips.position).normalized:Avatar!=null?Avatar.up:Vector3.up;
        var bodyRight=LeftShoulder!=null&&RightShoulder!=null?(RightShoulder.position-LeftShoulder.position).normalized:Avatar!=null?Avatar.right:Vector3.right;
        var bodyForward=Vector3.Cross(bodyRight,bodyUp).normalized;
        if(bodyForward.sqrMagnitude<.01f)bodyForward=Avatar!=null?Avatar.forward:Vector3.forward;
        var bodyBack=-bodyForward;
        for(int i=0;i<rest.Length;i++)
        {
            int row=i/(cols+1),column=i%(cols+1);
            if(row==0&&LeftShoulder!=null&&RightShoulder!=null)
            {
                float u=column/(float)cols;
                // Give the shoulder edge enough stand-off to clear the back of the
                // torso during upper-body animation, while keeping the hem close.
                float backBow=.14f+Mathf.Sin(u*Mathf.PI)*.12f;
                var world=Vector3.Lerp(LeftShoulder.position,RightShoulder.position,u)+bodyUp*.015f+bodyBack*backBow;
                vertices[i]=transform.InverseTransformPoint(world);
                continue;
            }
            var p=rest[i];float fall=Mathf.Clamp01(-p.y/.94f);
            p.z-=.10f*(1-fall);
            p.z+=Mathf.Sin(Time.unscaledTime*2.3f+p.x*7+fall*3)*.025f*fall*wind;
            p.x+=Mathf.Sin(Time.unscaledTime*1.7f+fall*2)*.02f*fall*wind;
            vertices[i]=p;
        }
        mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
    }
}

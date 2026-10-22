using UnityEngine;

// The shoulder edge stays attached; only the lower cloth follows the breeze.
public sealed class FlowingCape : MonoBehaviour
{
    Mesh mesh;
    Vector3[] rest,vertices;
    public Transform Shoulder,Avatar;
    public void Build(Material material)
    {
        const int cols=8,rows=12;
        rest=new Vector3[(cols+1)*(rows+1)];vertices=new Vector3[rest.Length];
        var uv=new Vector2[rest.Length];var triangles=new int[cols*rows*6];int k=0;
        for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++)
        {
            float v=y/(float)rows,u=x/(float)cols;int i=y*(cols+1)+x;
            rest[i]=new Vector3((u-.5f)*Mathf.Lerp(.46f,.82f,v),-v*.94f,-.05f-v*.18f+Mathf.Cos(u*Mathf.PI*6)*.025f*v);
            uv[i]=new Vector2(u,v);
            if(x<cols&&y<rows){triangles[k++]=i;triangles[k++]=i+cols+1;triangles[k++]=i+1;triangles[k++]=i+1;triangles[k++]=i+cols+1;triangles[k++]=i+cols+2;}
        }
        mesh=RuntimeResources.Own(gameObject,new Mesh{name="Tailored flowing cape"});mesh.vertices=rest;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    void LateUpdate()
    {
        if(mesh==null)return;float wind=GameFlow.I!=null?GameFlow.I.Wind:.5f;
        if(Shoulder!=null&&Avatar!=null){transform.position=Shoulder.position+Avatar.up*.13f-Avatar.forward*.2f;transform.rotation=Avatar.rotation;}
        for(int i=0;i<rest.Length;i++){var p=rest[i];float fall=Mathf.Clamp01(-p.y/.94f);p.z+=Mathf.Sin(Time.unscaledTime*2.3f+p.x*7+fall*3)*.075f*fall*wind;p.x+=Mathf.Sin(Time.unscaledTime*1.7f+fall*2)*.025f*fall*wind;vertices[i]=p;}
        mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
    }
}

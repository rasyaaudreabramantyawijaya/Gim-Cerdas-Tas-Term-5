using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public sealed class WorldNavigation : MonoBehaviour
{
    NavMeshData data;
    NavMeshDataInstance instance;
    public void Build()
    {
        var sources=new List<NavMeshBuildSource>();
        foreach(var collider in FindObjectsByType<BoxCollider>())
        {
            if(collider.gameObject.layer!=0||collider.isTrigger||!collider.enabled)continue;
            sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,transform=collider.transform.localToWorldMatrix*Matrix4x4.Translate(collider.center),size=collider.size,area=0});
        }
        foreach(var terrain in Terrain.activeTerrains)
            sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Terrain,transform=terrain.transform.localToWorldMatrix,sourceObject=terrain.terrainData,area=0});
        foreach(var collider in FindObjectsByType<CapsuleCollider>())
        {
            if(collider.gameObject.layer!=0||collider.isTrigger||!collider.enabled)continue;
            var rotation=collider.direction==0?Quaternion.Euler(0,0,90):collider.direction==2?Quaternion.Euler(90,0,0):Quaternion.identity;
            sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Capsule,transform=collider.transform.localToWorldMatrix*Matrix4x4.TRS(collider.center,rotation,Vector3.one),size=new Vector3(collider.radius*2,collider.height,collider.radius*2),area=0});
        }
        var settings=NavMesh.GetSettingsCount()>0?NavMesh.GetSettingsByIndex(0):NavMesh.CreateSettings();
        settings.agentRadius=.38f;settings.agentHeight=1.8f;settings.agentClimb=.45f;settings.agentSlope=45;
        data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(240,70,240)),Vector3.zero,Quaternion.identity);
        if(data!=null)instance=NavMesh.AddNavMeshData(data);else Debug.LogError("NavMesh generation failed.");
    }
    void OnDestroy(){if(instance.valid)instance.Remove();if(data!=null)Destroy(data);}
}

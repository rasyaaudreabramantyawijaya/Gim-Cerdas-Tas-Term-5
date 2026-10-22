using System.Collections.Generic;
using UnityEngine;

public sealed class WaterVolume : MonoBehaviour
{
    public static readonly List<WaterVolume> All = new List<WaterVolume>();
    public Bounds Bounds;
    public float SurfaceY;
    public Vector2[] Footprint;

    void OnEnable() { if(!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public static WaterVolume Register(GameObject go, Bounds bounds, float surfaceY, Vector2[] footprint = null)
    {
        var wv = go.AddComponent<WaterVolume>();
        wv.Bounds = bounds;
        wv.SurfaceY = surfaceY;
        wv.Footprint = footprint;
        return wv;
    }

    public static bool Check(Vector3 pos, out float waterY)
    {
        waterY = 0f;
        for(int i = 0; i < All.Count; i++)
        {
            var wv = All[i];
            // Bounds can overlap raised walkways, such as the stone bridge.
            // Treat the player as in water only when their feet reach its surface.
            if(wv != null && wv.Bounds.Contains(pos) && pos.y <= wv.SurfaceY && InsideFootprint(wv.Footprint, new Vector2(pos.x,pos.z)))
            {
                waterY = wv.SurfaceY;
                return true;
            }
        }
        // Fountain basin
        if(Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(0, -34)) <= 2.7f && pos.y <= 0.8f)
        {
            waterY = 0.35f; return true;
        }
        // Eastern coast
        if(pos.x >= 74f && pos.x <= 110f && pos.z >= -65f && pos.z <= -15f && pos.y <= 0.6f)
        {
            waterY = 0.05f; return true;
        }
        return false;
    }

    static bool InsideFootprint(Vector2[] footprint, Vector2 point)
    {
        if(footprint==null||footprint.Length<3)return true;
        bool inside=false;
        for(int i=0,j=footprint.Length-1;i<footprint.Length;j=i++)
        {
            Vector2 a=footprint[i],b=footprint[j];
            if((a.y>point.y)!=(b.y>point.y) && point.x<(b.x-a.x)*(point.y-a.y)/(b.y-a.y)+a.x)
                inside=!inside;
        }
        return inside;
    }
}

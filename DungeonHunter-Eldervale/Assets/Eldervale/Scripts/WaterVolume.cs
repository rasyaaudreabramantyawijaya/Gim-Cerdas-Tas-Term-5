using System.Collections.Generic;
using UnityEngine;

public sealed class WaterVolume : MonoBehaviour
{
    public static readonly List<WaterVolume> All = new List<WaterVolume>();
    public Bounds Bounds;
    public float SurfaceY;

    void OnEnable() { if(!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public static WaterVolume Register(GameObject go, Bounds bounds, float surfaceY)
    {
        var wv = go.AddComponent<WaterVolume>();
        wv.Bounds = bounds;
        wv.SurfaceY = surfaceY;
        return wv;
    }

    public static bool Check(Vector3 pos, out float waterY)
    {
        waterY = 0f;
        for(int i = 0; i < All.Count; i++)
        {
            var wv = All[i];
            if(wv != null && wv.Bounds.Contains(pos))
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
}

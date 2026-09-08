using UnityEngine;
using System.Collections.Generic;

// Tempel di GameObject kosong (Create Empty), kasih nama misal "TreeSpawner".
// Klik ikon titik-3 (⋮) di komponen ini di Inspector > "Spawn Low Poly Trees".
public class LowPolyTreeSpawner : MonoBehaviour
{
    [Header("Jumlah & Area Sebar")]
    [SerializeField] private int treeCount = 8;
    [SerializeField] private float areaWidth = 15f;
    [SerializeField] private float areaLength = 15f;

    [Header("Batang")]
    [SerializeField] private float trunkHeight = 1.4f;
    [SerializeField] private float trunkRadius = 0.12f;
    [SerializeField] private int trunkSides = 6; // makin kecil makin "kotak", makin besar makin bulat
    [SerializeField] private Color trunkColor = new Color(0.42f, 0.27f, 0.14f);

    [Header("Daun (kumpulan blob low-poly)")]
    [SerializeField] private int leafBlobCount = 4;
    [SerializeField] private float leafBlobRadius = 0.7f;
    [SerializeField] private Color leafColorA = new Color(0.30f, 0.50f, 0.25f);
    [SerializeField] private Color leafColorB = new Color(0.40f, 0.60f, 0.30f);

    private static Material _sharedLitMaterial;

    [ContextMenu("Spawn Low Poly Trees")]
    public void SpawnTrees()
    {
        ClearOldTrees();
        for (int i = 0; i < treeCount; i++)
        {
            float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float z = Random.Range(-areaLength / 2f, areaLength / 2f);
            BuildTree(new Vector3(x, 0f, z), i);
        }
    }

    [ContextMenu("Clear Trees")]
    public void ClearOldTrees()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    private void BuildTree(Vector3 position, int index)
    {
        GameObject tree = new GameObject($"LowPolyTree_{index}");
        tree.transform.SetParent(transform, false);
        tree.transform.localPosition = position;
        tree.transform.localScale = Vector3.one * Random.Range(0.85f, 1.25f);
        tree.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Batang
        GameObject trunk = new GameObject("Trunk");
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, trunkHeight / 2f, 0f);
        var trunkMf = trunk.AddComponent<MeshFilter>();
        var trunkMr = trunk.AddComponent<MeshRenderer>();
        trunkMf.mesh = LowPolyMeshUtils.CreateFacetedCylinder(trunkRadius, trunkHeight, trunkSides);
        trunkMr.material = GetMaterial(trunkColor);

        // Kumpulan blob daun (biar kelihatan seperti kluster, seperti referensi)
        for (int b = 0; b < leafBlobCount; b++)
        {
            GameObject leaf = new GameObject($"Leaf_{b}");
            leaf.transform.SetParent(tree.transform, false);

            Vector3 offset = Random.insideUnitSphere * (leafBlobRadius * 0.5f);
            offset.y = Mathf.Abs(offset.y); // jaga supaya kluster daun di atas, bukan di bawah tanah
            leaf.transform.localPosition = new Vector3(0f, trunkHeight, 0f) + offset;

            float sizeVar = Random.Range(0.7f, 1.1f);
            var leafMf = leaf.AddComponent<MeshFilter>();
            var leafMr = leaf.AddComponent<MeshRenderer>();
            leafMf.mesh = LowPolyMeshUtils.CreateIcosphere(leafBlobRadius * sizeVar, 1);
            leafMr.material = GetMaterial(Color.Lerp(leafColorA, leafColorB, Random.value));
        }
    }

    private Material GetMaterial(Color color)
    {
        // Pakai shader Standard biar tetap kompatibel dengan EnemyDetector.ApplyVisual() (rend.material.color)
        Material m = new Material(Shader.Find("Standard"));
        m.color = color;
        return m;
    }
}

// Utility pembuat mesh low-poly (flat shading: tiap segitiga punya vertex sendiri
// supaya bidangnya kelihatan tegas/faceted, bukan halus).
public static class LowPolyMeshUtils
{
    public static Mesh CreateIcosphere(float radius, int subdivisions)
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        List<Vector3> verts = new List<Vector3>
        {
            new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
            new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
            new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
        };
        List<int> tris = new List<int>
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };

        for (int s = 0; s < subdivisions; s++)
        {
            List<int> newTris = new List<int>();
            Dictionary<long, int> midCache = new Dictionary<long, int>();
            for (int i = 0; i < tris.Count; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                int ab = GetMidpoint(a, b, verts, midCache);
                int bc = GetMidpoint(b, c, verts, midCache);
                int ca = GetMidpoint(c, a, verts, midCache);
                newTris.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }
            tris = newTris;
        }

        // Flat shading: duplikat vertex per segitiga
        Vector3[] flatVerts = new Vector3[tris.Count];
        int[] flatTris = new int[tris.Count];
        for (int i = 0; i < tris.Count; i++)
        {
            flatVerts[i] = verts[tris[i]].normalized * radius;
            flatTris[i] = i;
        }

        Mesh mesh = new Mesh { vertices = flatVerts, triangles = flatTris };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static int GetMidpoint(int a, int b, List<Vector3> verts, Dictionary<long, int> cache)
    {
        long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
        if (cache.TryGetValue(key, out int existing)) return existing;

        Vector3 mid = ((verts[a] + verts[b]) / 2f).normalized;
        verts.Add(mid);
        int idx = verts.Count - 1;
        cache[key] = idx;
        return idx;
    }

    public static Mesh CreateFacetedCylinder(float radius, float height, int sides)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        float half = height / 2f;

        for (int i = 0; i < sides; i++)
        {
            float a0 = (i / (float)sides) * Mathf.PI * 2f;
            float a1 = ((i + 1) / (float)sides) * Mathf.PI * 2f;

            Vector3 p0Top = new Vector3(Mathf.Cos(a0) * radius, half, Mathf.Sin(a0) * radius);
            Vector3 p1Top = new Vector3(Mathf.Cos(a1) * radius, half, Mathf.Sin(a1) * radius);
            Vector3 p0Bot = new Vector3(Mathf.Cos(a0) * radius, -half, Mathf.Sin(a0) * radius);
            Vector3 p1Bot = new Vector3(Mathf.Cos(a1) * radius, -half, Mathf.Sin(a1) * radius);

            int baseIdx = verts.Count;
            verts.Add(p0Top); verts.Add(p1Top); verts.Add(p0Bot);
            verts.Add(p1Top); verts.Add(p1Bot); verts.Add(p0Bot);
            tris.AddRange(new[] { baseIdx, baseIdx + 1, baseIdx + 2, baseIdx + 3, baseIdx + 4, baseIdx + 5 });
        }

        Mesh mesh = new Mesh { vertices = verts.ToArray(), triangles = tris.ToArray() };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainHillyForestGenerator : MonoBehaviour
{
    [Header("Seed & Size")]
    public int seed = 12345;
    public Vector2Int heightmapResolution = new Vector2Int(513, 513); 
    public Vector3 terrainSize = new Vector3(200f, 40f, 200f);        

    [Header("Noise (fractal Perlin)")]
    public float noiseScale = 35f;
    public int octaves = 4;
    [Range(0f, 1f)] public float persistence = 0.5f;
    public float lacunarity = 2.0f;

    [Header("Flat Base Area")]
    public bool makeFlatBase = true;
    public Vector2 baseCenterWorld = new Vector2(100f, 100f); 
    public Vector2 baseSizeWorld = new Vector2(40f, 40f);     
    [Range(0f, 1f)] public float baseHeight01 = 0.08f;         
    public float edgeFalloffWorld = 8f;                       

    // ------------------ NEW: Base placement options ------------------
    public enum BaseHeightMode { SampleTerrain, UsePadHeight01 }

    [Header("Base Prefab Placement")]
    public bool placeBasePrefab = true;
    public GameObject basePrefab;        
    public float baseRotationY = 0f;
    public Vector3 basePrefabOffset = Vector3.zero;
    public bool clearTreesOnPad = true;

    [Tooltip("How to determine Y height for the base.")]
    public BaseHeightMode baseHeightMode = BaseHeightMode.SampleTerrain;

    [Tooltip("Force a large scale so it's visible the first time.")]
    public float baseForceScale = 10f;

    [Tooltip("If true, rescale the base uniformly to fit inside the pad.")]
    public bool fitPrefabToPad = true;

    [Tooltip("Multiply the fitted size to leave some margin (e.g., 0.95).")]
    [Range(0.1f, 8f)] public float fitPadding = 0.95f;

    [Tooltip("Parent the spawned base under the Terrain object once placed.")]
    public bool parentBaseUnderTerrain = false;

    [Tooltip("Log the final world position/scale after spawn.")]
    public bool logBaseSpawn = true;
    // -----------------------------------------------------------------

    [Header("Trees")]
    public GameObject[] treePrefabs;
    public float[] treeWeights;
    public int targetTreeCount = 1200;
    [Range(0f, 60f)] public float slopeLimit = 28f;
    public Vector2 heightRange01 = new Vector2(0.05f, 0.9f);
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);

    Terrain terrain;
    TerrainData data;
    System.Random rng;

    GameObject baseInstance;

    void OnValidate()
    {
        heightmapResolution.x = Mathf.Max(33, heightmapResolution.x | 1);
        heightmapResolution.y = Mathf.Max(33, heightmapResolution.y | 1);
        terrainSize.x = Mathf.Max(10f, terrainSize.x);
        terrainSize.y = Mathf.Max(1f, terrainSize.y);
        terrainSize.z = Mathf.Max(10f, terrainSize.z);

        if (treePrefabs != null && treeWeights != null && treeWeights.Length != treePrefabs.Length)
        {
            System.Array.Resize(ref treeWeights, treePrefabs.Length);
            for (int i = 0; i < treeWeights.Length; i++)
                if (treeWeights[i] <= 0f) treeWeights[i] = 1f;
        }
    }

    // Buttons in Inspector
    [ContextMenu("Generate Terrain + Trees")]
    public void GenerateAll()
    {
        PrepareTerrainAsset();
        GenerateHeights();

        if (placeBasePrefab) PlaceBasePrefab();

        SetupTreePrototypes();
        ScatterTrees();

        if (clearTreesOnPad) ClearTreesOnPad();
    }

    [ContextMenu("Place Base Only")]
    void PlaceBaseOnly()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        if (!data) data = terrain.terrainData;
        PlaceBasePrefab();
    }

    [ContextMenu("Regenerate Trees Only")]
    public void RegenerateTreesOnly()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        if (!data) data = terrain.terrainData;
        SetupTreePrototypes();
        ScatterTrees();
        if (clearTreesOnPad) ClearTreesOnPad();
    }

    [ContextMenu("Randomize Seed + Generate")]
    public void RandomizeAndGenerate()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        GenerateAll();
    }

    void PrepareTerrainAsset()
    {
        terrain = GetComponent<Terrain>();
        data = new TerrainData();
        data.heightmapResolution = Mathf.Max(heightmapResolution.x, heightmapResolution.y);
        data.size = terrainSize;
        terrain.terrainData = data;

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(data, GetUniquePath("Assets/GeneratedTerrain.asset"));
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

#if UNITY_EDITOR
    string GetUniquePath(string basePath)
    {
        string path = basePath;
        int i = 1;
        while (UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainData>(path) != null)
        {
            path = basePath.Replace(".asset", "_" + i + ".asset");
            i++;
        }
        return path;
    }
#endif

    void GenerateHeights()
    {
        rng = new System.Random(seed);
        float offX = (float)rng.NextDouble() * 9999f;
        float offZ = (float)rng.NextDouble() * 9999f;

        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float nx = (x / (float)(res - 1)) * (terrainSize.x / noiseScale) + offX;
                float nz = (z / (float)(res - 1)) * (terrainSize.z / noiseScale) + offZ;

                float amp = 1f;
                float freq = 1f;
                float h = 0f;
                for (int o = 0; o < octaves; o++)
                {
                    h += Mathf.PerlinNoise(nx * freq, nz * freq) * amp;
                    amp *= persistence;
                    freq *= lacunarity;
                }

                
                float norm = (1f - Mathf.Pow(persistence, octaves)) / (1f - Mathf.Max(0.0001f, persistence));
                h /= Mathf.Max(0.0001f, norm);

                heights[z, x] = Mathf.Clamp01(h);
            }
        }

        if (makeFlatBase)
            ApplyFlatRect(ref heights);

        data.SetHeights(0, 0, heights);
    }

    void ApplyFlatRect(ref float[,] heights)
    {
        int res = data.heightmapResolution;

        
        float nxCenter = Mathf.Clamp01(baseCenterWorld.x / terrainSize.x);
        float nzCenter = Mathf.Clamp01(baseCenterWorld.y / terrainSize.z);
        float nxHalf = Mathf.Clamp01(0.5f * baseSizeWorld.x / terrainSize.x);
        float nzHalf = Mathf.Clamp01(0.5f * baseSizeWorld.y / terrainSize.z);
        float nFallX = Mathf.Clamp01(edgeFalloffWorld / terrainSize.x);
        float nFallZ = Mathf.Clamp01(edgeFalloffWorld / terrainSize.z);

        int xMin = Mathf.Clamp(Mathf.RoundToInt((nxCenter - nxHalf - nFallX) * (res - 1)), 0, res - 1);
        int xMax = Mathf.Clamp(Mathf.RoundToInt((nxCenter + nxHalf + nFallX) * (res - 1)), 0, res - 1);
        int zMin = Mathf.Clamp(Mathf.RoundToInt((nzCenter - nzHalf - nFallZ) * (res - 1)), 0, res - 1);
        int zMax = Mathf.Clamp(Mathf.RoundToInt((nzCenter + nzHalf + nFallZ) * (res - 1)), 0, res - 1);

        float nxMinInner = nxCenter - nxHalf;
        float nxMaxInner = nxCenter + nxHalf;
        float nzMinInner = nzCenter - nzHalf;
        float nzMaxInner = nzCenter + nzHalf;

        for (int z = zMin; z <= zMax; z++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                float nx = x / (float)(res - 1);
                float nz = z / (float)(res - 1);

                float dx = Mathf.Max(0f, Mathf.Max(nxMinInner - nx, nx - nxMaxInner));
                float dz = Mathf.Max(0f, Mathf.Max(nzMinInner - nz, nz - nzMaxInner));

                float tx = nFallX > 0f ? Mathf.Clamp01(dx / nFallX) : (dx > 0f ? 1f : 0f);
                float tz = nFallZ > 0f ? Mathf.Clamp01(dz / nFallZ) : (dz > 0f ? 1f : 0f);
                float t = Mathf.Max(tx, tz);

                float original = heights[z, x];
                heights[z, x] = Mathf.Lerp(baseHeight01, original, t);
            }
        }
    }

    // ---------- Trees ----------
    void SetupTreePrototypes()
    {
        var prototypes = new List<TreePrototype>();
        if (treePrefabs != null)
        {
            foreach (var p in treePrefabs)
            {
                if (!p) continue;

                bool valid = false;
                if (p.TryGetComponent<MeshRenderer>(out _)) valid = true;
                else if (p.TryGetComponent<LODGroup>(out var lod))
                {
                    var lods = lod.GetLODs();
                    for (int i = 0; i < lods.Length && !valid; i++)
                        if (lods[i].renderers != null && lods[i].renderers.Length > 0) valid = true;
                }
                else if (p.GetComponentsInChildren<MeshRenderer>(true).Length > 0) valid = true;

                if (!valid)
                {
                    Debug.LogWarning($"[TerrainGen] Skipping tree prefab '{p.name}' — no valid MeshRenderer/LODGroup.");
                    continue;
                }

                prototypes.Add(new TreePrototype { prefab = p });
            }
        }
        data.treePrototypes = prototypes.ToArray();
    }

    void ScatterTrees()
    {
        if (data.treePrototypes == null || data.treePrototypes.Length == 0) return;

        float totalW = 0f;
        List<float> cum = new List<float>(treePrefabs.Length);
        for (int i = 0; i < treeWeights.Length; i++)
        {
            totalW += Mathf.Max(0.0001f, treeWeights[i]);
            cum.Add(totalW);
        }

        List<TreeInstance> trees = new List<TreeInstance>(targetTreeCount);
        int attempts = targetTreeCount * 4;
        int placed = 0;

        while (placed < targetTreeCount && attempts-- > 0)
        {
            float rx = (float)rng.NextDouble();
            float rz = (float)rng.NextDouble();

            if (makeFlatBase && IsInsidePad(rx, rz)) continue;

            float height01 = data.GetInterpolatedHeight(rx, rz) / terrainSize.y;
            if (height01 < heightRange01.x || height01 > heightRange01.y) continue;

            float slope = data.GetSteepness(rx, rz);
            if (slope > slopeLimit) continue;

            float r = (float)rng.NextDouble() * totalW;
            int protoIndex = 0;
            for (int i = 0; i < cum.Count; i++) { if (r <= cum[i]) { protoIndex = i; break; } }

            TreeInstance ti = new TreeInstance
            {
                prototypeIndex = protoIndex,
                position = new Vector3(rx, height01, rz),
                color = Color.white,
                lightmapColor = Color.white,
                heightScale = RandomRange(scaleRange),
                widthScale = RandomRange(scaleRange),
                rotation = (float)rng.NextDouble() * 360f
            };

            trees.Add(ti);
            placed++;
        }

        data.SetTreeInstances(trees.ToArray(), true);
        Debug.Log("Placed trees: " + placed);
    }

    bool IsInsidePad(float rx, float rz)
    {
        float nxCenter = Mathf.Clamp01(baseCenterWorld.x / terrainSize.x);
        float nzCenter = Mathf.Clamp01(baseCenterWorld.y / terrainSize.z);
        float nxHalf = Mathf.Clamp01(0.5f * baseSizeWorld.x / terrainSize.x);
        float nzHalf = Mathf.Clamp01(0.5f * baseSizeWorld.y / terrainSize.z);

        return (rx >= nxCenter - nxHalf && rx <= nxCenter + nxHalf &&
                rz >= nzCenter - nzHalf && rz <= nzCenter + nzHalf);
    }

    void ClearTreesOnPad()
    {
        var arr = data.treeInstances;
        if (arr == null || arr.Length == 0) return;

        List<TreeInstance> kept = new List<TreeInstance>(arr.Length);
        for (int i = 0; i < arr.Length; i++)
        {
            var t = arr[i];
            if (makeFlatBase && IsInsidePad(t.position.x, t.position.z)) continue;
            kept.Add(t);
        }
        data.SetTreeInstances(kept.ToArray(), true);
    }

    float RandomRange(Vector2 r)
    {
        if (r.x > r.y) { float t = r.x; r.x = r.y; r.y = t; }
        return (float)rng.NextDouble() * (r.y - r.x) + r.x;
    }

    // ---------- Base spawn (rewritten) ----------
    void PlaceBasePrefab()
    {
        if (!basePrefab)
        {
            Debug.LogWarning("[TerrainGen] placeBasePrefab is true but basePrefab is not assigned.");
            return;
        }

        if (!terrain) terrain = GetComponent<Terrain>();
        if (!data) data = terrain.terrainData;

       
        if (baseInstance)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(baseInstance);
            else Destroy(baseInstance);
#else
            Destroy(baseInstance);
#endif
            baseInstance = null;
        }

    
        Vector3 terrainOrigin = terrain.GetPosition();

       
        Vector3 padWorldXZ = terrainOrigin + new Vector3(baseCenterWorld.x, 0f, baseCenterWorld.y);

      
        float y;
        if (baseHeightMode == BaseHeightMode.SampleTerrain)
            y = terrain.SampleHeight(padWorldXZ) + terrainOrigin.y;
        else
            y = baseHeight01 * terrainSize.y + terrainOrigin.y;

        Vector3 worldPos = new Vector3(padWorldXZ.x, y, padWorldXZ.z) + basePrefabOffset;
        Quaternion rot = Quaternion.Euler(0f, baseRotationY, 0f);

  
        GameObject go = null;
#if UNITY_EDITOR
        bool isPrefabAsset = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(basePrefab);
        if (!Application.isPlaying && isPrefabAsset)
            go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(basePrefab);
        else
            go = Instantiate(basePrefab);
#else
        go = Instantiate(basePrefab);
#endif

        baseInstance = go;
        baseInstance.name = basePrefab.name + " (Clone)";
        baseInstance.transform.SetPositionAndRotation(worldPos, rot);
        baseInstance.layer = 0;
        baseInstance.SetActive(true);

       
        var lod = baseInstance.GetComponentInChildren<LODGroup>(true);
        if (lod) lod.ForceLOD(0);

      
        baseInstance.transform.localScale = Vector3.one * Mathf.Max(0.01f, baseForceScale);

       
        if (fitPrefabToPad)
        {
            var rs = baseInstance.GetComponentsInChildren<Renderer>(true);
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);

                float curX = Mathf.Max(0.01f, b.size.x);
                float curZ = Mathf.Max(0.01f, b.size.z);

                float targetX = Mathf.Max(0.01f, baseSizeWorld.x) * fitPadding;
                float targetZ = Mathf.Max(0.01f, baseSizeWorld.y) * fitPadding;

                float s = Mathf.Min(targetX / curX, targetZ / curZ);
                baseInstance.transform.localScale = Vector3.one * s;
            }
        }

        if (parentBaseUnderTerrain)
            baseInstance.transform.SetParent(this.transform, true);

        if (logBaseSpawn)
            Debug.Log($"[TerrainGen] Base spawned @ {worldPos}  scale={baseInstance.transform.localScale}  parented={parentBaseUnderTerrain}");
    }

  
    void OnDrawGizmosSelected()
    {
        if (!makeFlatBase) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 c = new Vector3(baseCenterWorld.x, baseHeight01 * terrainSize.y, baseCenterWorld.y);
        Vector3 sz = new Vector3(baseSizeWorld.x, 0.1f, baseSizeWorld.y);
        Gizmos.DrawCube(GetTerrainOrigin() + c, sz);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Vector3 szFall = sz + new Vector3(edgeFalloffWorld * 2f, 0.1f, edgeFalloffWorld * 2f);
        Gizmos.DrawWireCube(GetTerrainOrigin() + c, szFall);
    }

    Vector3 GetTerrainOrigin()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        return terrain ? terrain.GetPosition() : transform.position;
    }
}

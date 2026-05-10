using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;

    public enum DrawMode
    {
        noiseMap,
        colourMap,
        mesh,
        falloff,
        biomeMap
    }

    public DrawMode drawMode;

    [Header("Map")]
    public int mapChunkSize = 128;

    [Header("Height Noise")]
    public float noiseScale = 40f;
    public int octaves = 4;

    [Range(0,1)]
    public float persistance = 0.5f;

    public float lacunarity = 2f;

    [Header("Biome Noise")]
    public float biomeScale = 300f;

    [Header("General")]
    public int seed;
    public Vector2 offset;

    [Header("Falloff")]
    public bool useFalloffMap;
    public AnimationCurve falloffMapCurve;

    [Header("Mesh")]
    public float meshHeightMultiplier = 10f;
    public AnimationCurve meshHeightCurve;

    [Header("Texture")]
    [Range(0f, 0.2f)]
    public float blendAmount = 0.03f;

    public int textureResolution = 1024;

    [Header("Biomes")]
    public RegionSO[] biomes;

    public bool autoUpdate;

    private float[,] falloffMap;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void GenerateMap(int seedIn = 0)
    {
        // =====================================================
        // FALLOFF
        // =====================================================

        falloffMap = FalloffGenerator.GenerateFalloffMap(
            mapChunkSize,
            falloffMapCurve);

        // =====================================================
        // HEIGHT MAP
        // =====================================================

        float[,] heightMap = Noise.GenerateNoiseMap(
            mapChunkSize,
            mapChunkSize,
            seedIn,
            noiseScale,
            octaves,
            persistance,
            lacunarity,
            offset);

        // =====================================================
        // BIOME MAP
        // =====================================================

        float[,] biomeMap = BiomeGenerator.GenerateBiomeMap(
            mapChunkSize,
            mapChunkSize,
            biomeScale,
            seedIn + 999,
            offset);

        // =====================================================
        // APPLY FALLOFF
        // =====================================================

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloffMap)
                {
                    heightMap[x, y] = Mathf.Clamp01(
                        heightMap[x, y] - falloffMap[x, y]);
                }
            }
        }

        // =====================================================
        // GENERATE LOW RES COLOUR MAP
        // (Used for preview modes only)
        // =====================================================

        Color[] colourMap =
            new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float height = heightMap[x, y];
                float biomeValue = biomeMap[x, y];

                colourMap[y * mapChunkSize + x] =
                    EvaluateTerrainColour(
                        height,
                        biomeValue);
            }
        }

        // =====================================================
        // DISPLAY
        // =====================================================

        MapDisplay display = FindAnyObjectByType<MapDisplay>();

        if (drawMode == DrawMode.noiseMap)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.biomeMap)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(biomeMap));
        }
        else if (drawMode == DrawMode.colourMap)
        {
            display.DrawTexture(
                TextureGenerator.TexturefromColourMap(
                    colourMap,
                    mapChunkSize,
                    mapChunkSize));
        }
        else if (drawMode == DrawMode.mesh)
        {
            // =================================================
            // MESH
            // =================================================

            MeshData meshData =
                MeshGenerator.GenerateTerrainMesh(
                    heightMap,
                    meshHeightMultiplier * noiseScale,
                    meshHeightCurve);

            // =================================================
            // HIGH RES SMOOTH TEXTURE
            // =================================================

            Texture2D texture =
                TextureGenerator.GenerateBiomeTexture(
                    heightMap,
                    biomeMap,
                    biomes,
                    blendAmount,
                    textureResolution);

            display.DrawMesh(meshData, texture);
        }
        else if (drawMode == DrawMode.falloff)
        {
            display.DrawTexture(
                TextureGenerator.TextureFromHeightMap(
                    falloffMap));
        }
    }

    // =========================================================
    // TERRAIN COLOUR LOOKUP
    // =========================================================

    Color EvaluateTerrainColour(
        float height,
        float biomeValue)
    {
        RegionSO biomeA = GetBiome(
            biomeValue,
            out RegionSO biomeB,
            out float biomeBlend);

        Color colourA =
            EvaluateBiomeTerrainColour(
                biomeA,
                height);

        Color colourB =
            EvaluateBiomeTerrainColour(
                biomeB,
                height);

        biomeBlend = Mathf.SmoothStep(
            0f,
            1f,
            biomeBlend);

        return Color.Lerp(
            colourA,
            colourB,
            biomeBlend);
    }
    
    Color EvaluateBiomeTerrainColour(
        RegionSO biome,
        float height)
    {
        TerrainType[] regions = biome.regions;

        for (int i = 0; i < regions.Length; i++)
        {
            if (height <= regions[i].height)
            {
                if (i == 0)
                    return regions[i].colour;

                TerrainType previous =
                    regions[i - 1];

                TerrainType current =
                    regions[i];

                float blendStart =
                    current.height - blendAmount;

                if (height < blendStart)
                    return previous.colour;

                float t = Mathf.InverseLerp(
                    blendStart,
                    current.height,
                    height);

                t = Mathf.SmoothStep(0f, 1f, t);

                return Color.Lerp(
                    previous.colour,
                    current.colour,
                    t);
            }
        }

        return regions[regions.Length - 1].colour;
    }

    // =========================================================
    // BIOME LOOKUP
    // =========================================================

    RegionSO GetBiome(
        float biomeValue,
        out RegionSO nextBiome,
        out float biomeBlend)
    {
        float scaled =
            biomeValue * (biomes.Length - 1);

        int index =
            Mathf.FloorToInt(scaled);

        biomeBlend = scaled - index;

        index = Mathf.Clamp(
            index,
            0,
            biomes.Length - 1);

        int nextIndex = Mathf.Clamp(
            index + 1,
            0,
            biomes.Length - 1);

        nextBiome = biomes[nextIndex];

        return biomes[index];
    }

    private void OnValidate()
    {
        if (mapChunkSize < 1)
            mapChunkSize = 1;

        if (lacunarity < 1)
            lacunarity = 1;

        if (octaves < 0)
            octaves = 0;

        if (noiseScale < 0.1f)
            noiseScale = 0.1f;

        if (biomeScale < 1f)
            biomeScale = 1f;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;

    [Range(0,1)]
    public float height;

    public Color colour;
}

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(
        int size,
        AnimationCurve curve)
    {
        float[,] map = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx =
                    x / (float)(size - 1) * 2f - 1f;

                float ny =
                    y / (float)(size - 1) * 2f - 1f;

                float distance =
                    Mathf.Sqrt(nx * nx + ny * ny);

                distance =
                    Mathf.Clamp01(
                        distance / Mathf.Sqrt(2f));

                map[x, y] =
                    curve.Evaluate(distance);
            }
        }

        return map;
    }
}
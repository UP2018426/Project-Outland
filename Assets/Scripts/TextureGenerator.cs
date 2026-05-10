// ============================================
// TEXTURE GENERATOR
// Put this in: TextureGenerator.cs
// ============================================

using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D GenerateBiomeTexture(float[,] heightMap, float[,] biomeMap, RegionSO[] biomes, float blendAmount, int textureResolution)
    {
        Texture2D texture = new Texture2D(
            textureResolution,
            textureResolution,
            TextureFormat.RGBA32,
            true);

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 8;

        Color[] colours =
            new Color[textureResolution * textureResolution];

        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                float u = x / (float)(textureResolution - 1);
                float v = y / (float)(textureResolution - 1);

                float height =
                    SampleMapBilinear(heightMap, u, v);

                float biomeValue =
                    SampleMapBilinear(biomeMap, u, v);

                colours[y * textureResolution + x] =
                    EvaluateTerrainColour(
                        height,
                        biomeValue,
                        biomes,
                        blendAmount);
            }
        }

        texture.SetPixels(colours);
        texture.Apply();

        return texture;
    }
    
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colourMap = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (heightMap[x, y] == 1)
                {
                    colourMap[y * width + x] = Color.red;
                    continue;
                }
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TexturefromColourMap(colourMap, width, height);
    }

    public static Texture2D TexturefromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    static float SampleMapBilinear(
        float[,] map,
        float u,
        float v)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        float x = u * (width - 1);
        float y = v * (height - 1);

        int xMin = Mathf.FloorToInt(x);
        int yMin = Mathf.FloorToInt(y);

        int xMax = Mathf.Min(xMin + 1, width - 1);
        int yMax = Mathf.Min(yMin + 1, height - 1);

        float tx = x - xMin;
        float ty = y - yMin;

        float a = Mathf.Lerp(
            map[xMin, yMin],
            map[xMax, yMin],
            tx);

        float b = Mathf.Lerp(
            map[xMin, yMax],
            map[xMax, yMax],
            tx);

        return Mathf.Lerp(a, b, ty);
    }

    static Color EvaluateTerrainColour(
        float height,
        float biomeValue,
        RegionSO[] biomes,
        float blendAmount)
    {
        RegionSO biome =
            GetBiome(biomeValue, biomes);

        TerrainType[] regions =
            biome.regions;

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

    static RegionSO GetBiome(
        float biomeValue,
        RegionSO[] biomes)
    {
        int index = Mathf.FloorToInt(
            biomeValue * biomes.Length);

        index = Mathf.Clamp(
            index,
            0,
            biomes.Length - 1);

        return biomes[index];
    }
}
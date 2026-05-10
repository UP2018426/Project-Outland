using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TexturefromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
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
    
    static Color EvaluateBiomeColour(float height, TerrainType[] regions, float blendAmount)
    {
        for (int i = 0; i < regions.Length; i++)
        {
            if (height <= regions[i].height)
            {
                // first region
                if (i == 0)
                    return regions[i].colour;

                TerrainType previous = regions[i - 1];
                TerrainType current = regions[i];

                // start blending only near boundary
                float blendStart = current.height - blendAmount;

                // outside blend zone
                if (height < blendStart)
                    return previous.colour;

                // inside blend zone
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
    
    public static Texture2D GenerateBiomeTexture(float[,] noiseMap, TerrainType[] regions, int textureResolution, float blendAmount)
    {
        Texture2D texture = new Texture2D(
            textureResolution,
            textureResolution,
            TextureFormat.RGBA32,
            true);

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 8;

        Color[] colours = new Color[
            textureResolution * textureResolution];

        int noiseWidth = noiseMap.GetLength(0);
        int noiseHeight = noiseMap.GetLength(1);

        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                // normalized texture coordinate
                float u = x / (float)(textureResolution - 1);
                float v = y / (float)(textureResolution - 1);

                // sample noise map smoothly
                float height = SampleNoiseBilinear(
                    noiseMap,
                    u,
                    v);

                colours[y * textureResolution + x] =
                    EvaluateBiomeColour(
                        height,
                        regions,
                        blendAmount);
            }
        }

        texture.SetPixels(colours);
        texture.Apply();

        return texture;
    }
    
    static float SampleNoiseBilinear(float[,] noiseMap, float u, float v)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        float x = u * (width - 1);
        float y = v * (height - 1);

        int xMin = Mathf.FloorToInt(x);
        int yMin = Mathf.FloorToInt(y);

        int xMax = Mathf.Min(xMin + 1, width - 1);
        int yMax = Mathf.Min(yMin + 1, height - 1);

        float tx = x - xMin;
        float ty = y - yMin;

        float a = Mathf.Lerp(
            noiseMap[xMin, yMin],
            noiseMap[xMax, yMin],
            tx);

        float b = Mathf.Lerp(
            noiseMap[xMin, yMax],
            noiseMap[xMax, yMax],
            tx);

        return Mathf.Lerp(a, b, ty);
    }
}

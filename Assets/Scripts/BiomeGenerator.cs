// ============================================
// BiomeGenerator.cs
// ============================================

using UnityEngine;

public static class BiomeGenerator
{
    public static float[,] GenerateBiomeMap(int width, int height, float scale, int seed, Vector2 offset)
    {
        float[,] biomeMap = new float[width, height];

        System.Random prng =
            new System.Random(seed);

        float offsetX =
            prng.Next(-100000, 100000) + offset.x;

        float offsetY =
            prng.Next(-100000, 100000) + offset.y;

        if (scale <= 0)
            scale = 0.0001f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX =
                    (x + offsetX) / scale;

                float sampleY =
                    (y + offsetY) / scale;

                float noiseValue =
                    Mathf.PerlinNoise(sampleX, sampleY);

                biomeMap[x, y] = noiseValue;
            }
        }

        return biomeMap;
    }
}
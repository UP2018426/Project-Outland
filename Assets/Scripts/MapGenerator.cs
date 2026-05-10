using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        noiseMap,
        colourMap,
        mesh,
        falloff
    }

    public DrawMode drawMode;
    public int mapChunkSize;
    public float noiseScale;
    public int octaves;
    [Range(0,1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFalloffMap;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve, falloffMapCurve;
    public bool autoUpdate;

    public RegionSO selectedRegion;

    public float blendAmount = 0.05f;
    public int textureResolution = 1024;
    private float[,] falloffMap;

    /*private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }*/

    public void GenerateMap()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, falloffMapCurve);
        
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloffMap)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]); 
                }
                
                float currentHeight = noiseMap[x, y];
                /*for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }*/
                for (int i = 0; i < selectedRegion.regions.Length; i++)
                {
                    if (currentHeight <= selectedRegion.regions[i].height)
                    {
                        if (i == 0)
                        {
                            colourMap[y * mapChunkSize + x] = selectedRegion.regions[i].colour;
                        }
                        else
                        {
                            TerrainType previous = selectedRegion.regions[i - 1];
                            TerrainType current = selectedRegion.regions[i];

                            float blendStart = current.height - blendAmount;

                            if (currentHeight >= blendStart)
                            {
                                float t = Mathf.InverseLerp(blendStart, current.height, currentHeight);

                                colourMap[y * mapChunkSize + x] =
                                    Color.Lerp(previous.colour, current.colour, t);
                            }
                            else
                            {
                                colourMap[y * mapChunkSize + x] = previous.colour;
                            }
                        }

                        break;
                    }
                }
            }
        }
        
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.noiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.colourMap)
        {
            display.DrawTexture(TextureGenerator.TexturefromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier * noiseScale, meshHeightCurve), TextureGenerator.TexturefromColourMap(colourMap, mapChunkSize, mapChunkSize));
            display.DrawSmoothTexture(noiseMap, selectedRegion.regions, textureResolution, blendAmount);
        }
        else if (drawMode == DrawMode.falloff)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize, falloffMapCurve)));
        }
    }

    private void OnValidate()
    {
        if (mapChunkSize < 1)
        {
            mapChunkSize = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }

        if (noiseScale < 0.1f)
        {
            noiseScale = 0.1f;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size, AnimationCurve curve)
    {
        float[,] map = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)(size - 1) * 2f - 1f;
                float ny = y / (float)(size - 1) * 2f - 1f;

                // Radial distance from center
                float distance = Mathf.Sqrt(nx * nx + ny * ny);

                // Normalize so corners clamp to 1
                distance = Mathf.Clamp01(distance / Mathf.Sqrt(2f));

                map[x, y] = curve.Evaluate(distance);
            }
        }

        return map;
    }
}

using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public Terrain terrain;
    public List<NoiseSettings> noiseSettings;
    public bool normalize = false;
    public float[,] heights { get; private set; }
    public Texture2D noiseTexture;

    [Header("Steepness for Gradient Perlin")]
    public float steepnessMultiplier = 10.0f;
    public float steepnessExponent = 2.0f;

    void Start()
    {
        float[,] heights = GenerateHeightMap(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

        if (normalize) NormalizeHeightMap(heights);

        noiseTexture = HeightMapToTexture(heights);

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    [BurstCompile]
    private float[,] GenerateHeightMap(int width, int height)
    {
        float[,] heights = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heights[x, y] = GetHeightAverage(x, y);
            }
        }

        return heights;
    }

    [BurstCompile]
    private void NormalizeHeightMap(float[,] heightMap)
    {
        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        //get min - max
        for (int y = 0; y < heightMap.GetLength(1); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                float height = heightMap[x, y];
                if (height >= max) max = height;
                if (height < min) min = height;
            }
        }

        //normalize to -> 0 - 1
        for (int y = 0; y < heightMap.GetLength(1); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                float height = heightMap[x, y];
                heightMap[x, y] = Remap(height, min, max, 0f, 1f);
            }
        }

        float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
    }

    [BurstCompile]
    internal Texture2D HeightMapToTexture(float[,] heightMap)
    {
        Texture2D heightTexture = new Texture2D(heightMap.GetLength(0), heightMap.GetLength(1), TextureFormat.RGB24, false);
        heightTexture.wrapMode = TextureWrapMode.Clamp;
        heightTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < heightMap.GetLength(1); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                heightTexture.SetPixel(x, y, Color.white * heightMap[x, y]);
            }
        }

        heightTexture.Apply();

        return heightTexture;
    }

    [BurstCompile]
    private float GetHeightAverage(float x, float y)
    {
        float sumHeight = 0f;

        foreach (NoiseSettings noiseSettings in noiseSettings)
        {
            sumHeight += GetPerlinValue(x, y, noiseSettings);
        }

        return sumHeight / (float)noiseSettings.Count;
    }

    [BurstCompile]
    private float GetPerlinValue(float x, float y, NoiseSettings noiseSettings)
    {
        return GradientPerlin.GetGradientPerlin(x, y, noiseSettings.seedX, noiseSettings.seedY, terrain.terrainData.heightmapResolution, noiseSettings.scale, 1f, steepnessMultiplier, steepnessExponent);
    }
}

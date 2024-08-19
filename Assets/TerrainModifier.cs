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
    public float[,] steepnessMap { get; private set; }
    public Texture2D steepnessTexture;
    public RenderTexture terrainHeightMapTexture;

    [Header("Steepness for Gradient Perlin")]
    public float steepnessMultiplier = 10.0f;
    public float steepnessExponent = 2.0f;

    [Header("Height Modifiers")]
    public float heightMultiplier = 1f;
    public float heightAdd = 0f;

    void Start()
    {
        heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
        steepnessMap = new float[heights.GetLength(0), heights.GetLength(1)];

        GenerateHeightMap(heights.GetLength(0), heights.GetLength(1));

        //NormalizeHeightMap(heights);
        //NormalizeHeightMap(steepnessMap);

        noiseTexture = HeightMapToTexture(heights);
        steepnessTexture = HeightMapToTexture(steepnessMap);

        //terrain.terrainData.SetHeights(0, 0, heights);
        terrain.terrainData.SetHeights(0, 0, steepnessMap);
        terrainHeightMapTexture = terrain.terrainData.heightmapTexture;
    }

    private void OnDisable()
    {
        terrainHeightMapTexture.Release();
    }

    [BurstCompile]
    private void GenerateHeightMap(int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                (float height, float steepness) heightSteepness = GetHeightAverage(x, y);
                heights[x, y] = heightSteepness.height * heightMultiplier + heightAdd;
                steepnessMap[x, y] = heightSteepness.steepness * heightMultiplier + heightAdd;
            }
        }
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
    private (float height, float steepness) GetHeightAverage(float x, float y)
    {
        float height = 0f;
        float steepness = 0f;
        float comulativeIntensity = 0f;

        foreach (NoiseSettings noiseSetting in noiseSettings)
        {
            (float height, float steepness) heightSteepness = GetPerlinValue(x, y, noiseSetting);
            comulativeIntensity += noiseSetting.intensity;
            height += heightSteepness.height * noiseSetting.intensity;
            steepness += heightSteepness.steepness * noiseSetting.intensity;
        }

        return (height / comulativeIntensity, steepness / comulativeIntensity);
    }

    [BurstCompile]
    private (float height, float steepness) GetPerlinValue(float x, float y, NoiseSettings noiseSettings)
    {
        return GradientPerlin.GetGradientPerlin(x, y, noiseSettings.seedX, noiseSettings.seedY, terrain.terrainData.heightmapResolution, noiseSettings.scale, 1f, steepnessMultiplier, steepnessExponent);
    }
}

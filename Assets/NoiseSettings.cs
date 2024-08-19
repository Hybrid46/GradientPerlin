using System;
using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseSettings", menuName = "Scriptable Objects/NoiseSettings")]
public class NoiseSettings : ScriptableObject
{
    [Range(0, 1000000)] public int seedX;
    [Range(0, 1000000)] public int seedY;
    public float scale;
    [Range(0.01f, 1)] public float intensity = 1f;

    private Texture2D heightTexture;

    internal void UpdateHeightMap()
    {
        HeightMapToTexture();
    }

    [BurstCompile]
    private void HeightMapToTexture()
    {
        heightTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        heightTexture.wrapMode = TextureWrapMode.Clamp;
        float[,] heightMap = new float[256, 256];

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                heightMap[x, y] = GetHeight(new Vector3(x,0f, y)) * intensity;
                heightTexture.SetPixel(x, y, Color.white * heightMap[x, y]);
            }
        }

        heightTexture.Apply();
    }

    private float GetHeight(Vector3 worldPosition)
    {
        float xCoord = worldPosition.x / 256 * scale + seedX;
        float zCoord = worldPosition.z / 256 * scale + seedY;

        return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord));
        //For Job -> return noise.cnoise(new float2(xCoord, zCoord)) * noiseSettings.maxHeight + noiseSettings.minHeight;
    }

    internal Texture GetHeightTexture()
    {
        return heightTexture;
    }
}

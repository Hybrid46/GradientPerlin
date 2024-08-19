using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public Terrain terrain;
    public Texture2D texture;

    void Start()
    {
        float[,] heights = new float[texture.width, texture.height];

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                heights[x, y] = GradientPerlin.GetHeight(0f, 0f, 0f);
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
    }
}

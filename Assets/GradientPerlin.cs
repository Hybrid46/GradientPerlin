using Unity.Burst;
using UnityEngine;

public static class GradientPerlin
{
    [BurstCompile]
    public static float GetGradientPerlin(float x, float y, float seedX, float seedY, float resolution, float scale, float derivationDistance = 0.001f, float steepnessMultiplier = 2f, float steepnessExponent = 2f)
    {
        float perlin = GetPerlin(x, y, seedX, seedY, resolution, scale);

        float steepness = GetSteepness(perlin, x, y, seedX, seedY, resolution, scale, derivationDistance);
        //float steepness = GetSteepnessCentralDifference( x, y, seedX, seedY, resolution, scale, derivationDistance);

        steepness = Mathf.Pow(steepness * steepnessMultiplier, steepnessExponent);

        float gradientPerlin = Mathf.Lerp(perlin, 0f, steepness);

        return gradientPerlin;
    }

    //fastest
    [BurstCompile]
    public static float GetSteepness(float basePerlinValue, float x, float y, float seedX, float seedY, float resolution, float scale, float derivationDistance)
    {
        // Calculate the Perlin noise values at small offsets from the original point
        float perlinDX = GetPerlin(x + derivationDistance, y, seedX, seedY, resolution, scale);
        float perlinDY = GetPerlin(x, y + derivationDistance, seedX, seedY, resolution, scale);

        // Calculate the gradients in the X and Y directions
        float gradientX = (perlinDX - basePerlinValue) / derivationDistance;
        float gradientY = (perlinDY - basePerlinValue) / derivationDistance;

        // Calculate the magnitude of the gradient vector
        float steepness = Mathf.Sqrt(gradientX * gradientX + gradientY * gradientY);

        return steepness;
    }

    //+precision
    //-computations
    [BurstCompile]
    public static float GetSteepnessCentralDifference(float x, float y, float seedX, float seedY, float resolution, float scale, float derivationDistance)
    {
        // Calculate Perlin noise at slightly shifted positions in both directions for x and y
        float perlinLeft = GetPerlin(x - derivationDistance, y, seedX, seedY, resolution, scale);
        float perlinRight = GetPerlin(x + derivationDistance, y, seedX, seedY, resolution, scale);
        float perlinDown = GetPerlin(x, y - derivationDistance, seedX, seedY, resolution, scale);
        float perlinUp = GetPerlin(x, y + derivationDistance, seedX, seedY, resolution, scale);

        // Calculate the gradient in the X direction using central difference
        float gradientX = (perlinRight - perlinLeft) / (2 * derivationDistance);

        // Calculate the gradient in the Y direction using central difference
        float gradientY = (perlinUp - perlinDown) / (2 * derivationDistance);

        // Calculate the magnitude of the gradient vector
        float steepness = Mathf.Sqrt(gradientX * gradientX + gradientY * gradientY);

        return steepness;
    }

    [BurstCompile]
    public static float GetPerlin(float x, float y, float seedX, float seedY, float resolution, float scale)
    {
        float xCoord = x / resolution * scale + seedX;
        float zCoord = y / resolution * scale + seedY;

        //When normalized(only for pregen) no clamping needed!
        //Clamping causes losses on edge values! x > 1 && x < 0
        return Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoiseMethodType
{
    Random,
    Perlin2D,
    Perlin3D,
    RealWorldTest1,
    Simplex2D,
    Simplex3D
}

public class NoiseGenerator : MonoBehaviour
{
    public NoiseMethodType noiseMethod;

    [Header("Perlin 3D Noise")]
    public int mySeed = 0;

    [Header("Real World Noise")]
    public int worldSeed = 0;
    //First Layer
    public bool active1 = true;
    public float scale1 = 2.5f;
    [Range(-1, 1)]
    public float heightScale1 = 1;
    //Second Layer
    public bool active2 = false;
    public float scale2 = 4;
    [Range(-1, 1)]
    public float heightScale2 = 1;

    [Header("Simple Noise")]
    private SimplexNoise simplexNoise;


    public float Generate(Vector3 pos)
    {
        if (noiseMethod == NoiseMethodType.Perlin2D)
        {
            return Perlin(pos);
        }
        else if (noiseMethod == NoiseMethodType.Perlin3D)
        {
            return PerlinNoise3D(pos);
        }
        else if (noiseMethod == NoiseMethodType.RealWorldTest1)
        {
            return AvgNoise(pos);
        }
        else if (noiseMethod == NoiseMethodType.Simplex2D)
        {
            if (simplexNoise == null)
            {
                simplexNoise = new SimplexNoise();
            }
            return (float)simplexNoise.Evaluate(pos.x, pos.z) + pos.y;
        }
        else if (noiseMethod == NoiseMethodType.Simplex3D)
        {
            if (simplexNoise == null)
            {
                simplexNoise = new SimplexNoise();
            }
            return (float)simplexNoise.Evaluate(pos.x, pos.y, pos.z) + pos.y;
        }
        return RandomNoise();
    }

    //REAL WORLD
    private float AvgNoise(Vector3 pos)
    {
        float n1;
        float n2;
        if (active1)
        {
            n1 = Mathf.PerlinNoise((pos.x * scale1) + worldSeed, (pos.z * scale1) + worldSeed) - (heightScale1 * pos.y);
        }
        else
        {
            n1 = 1;
        }
        if (active2)
        {
            n2 = Mathf.PerlinNoise((pos.x * scale2) + (2 * worldSeed), (pos.z * scale2) + (2 * worldSeed)) - (heightScale2 * pos.y);
        }
        else
        {
            n2 = 1;
        }

        return (n1 + n2) / 2f;
    }

    //RANDOM
    private float RandomNoise()
    {
        return UnityEngine.Random.Range(0f, 1f);
    }

    //PERLIN 2D
    private float Perlin(Vector3 pos)
    {

        return Mathf.PerlinNoise(pos.x, pos.z) - pos.y;
    }

    //PERLIN 3D
    //returns a perlin noise value between 1 and 0 based on 3D coords
    private float PerlinNoise3D(Vector3 pos)
    {
        pos.y += 1;
        pos.z += 2;
        float xy = _perlin3DFixed(pos.x, pos.y);
        float xz = _perlin3DFixed(pos.x, pos.z);
        float yz = _perlin3DFixed(pos.y, pos.z);
        float yx = _perlin3DFixed(pos.y, pos.x);
        float zx = _perlin3DFixed(pos.z, pos.x);
        float zy = _perlin3DFixed(pos.z, pos.y);
        float val = xy * xz * yz * yx * zx * zy;
        return val;
    }

    //perlin helper
    private float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a + mySeed, b + mySeed));
    }
}
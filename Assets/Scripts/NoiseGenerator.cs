using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoiseMethodType
{
    Random,
    Perlin2D,
    Perlin3D,
    SimplexValue
}

public class NoiseGenerator : MonoBehaviour
{
    public NoiseMethodType noiseMethod;

    [Header("Perlin 3D Noise")]
    public int mySeed = 0;

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
        else if (noiseMethod == NoiseMethodType.SimplexValue)
        {
            //TODO
        }
        return RandomNoise();
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
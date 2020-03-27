using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    [Header ("Noise")]
    public int mySeed = 0;

    public float Generate(Vector3 pos){
        return PerlinNoise3D(pos);
    }

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    public Vector3 position, xEdgePosition, yEdgePosition, zEdgePosition;

    //1 - on, 0 - off
    public float noiseVal;

    public Voxel()
    {

    }

    public Voxel(float x, float y, float z, float noiseVal, float size)
    {
        position.x = x;
        position.y = y;
        position.z = z;
        this.noiseVal = noiseVal;

        xEdgePosition = position;
        xEdgePosition.x += size * 0.5f;
        yEdgePosition = position;
        yEdgePosition.y += size * 0.5f;
        zEdgePosition = position;
        zEdgePosition.z += size * 0.5f;
    }

    public Voxel Copy()
    {
        Voxel copy = new Voxel();
        copy.position = this.position;
        copy.xEdgePosition = this.xEdgePosition;
        copy.yEdgePosition = this.yEdgePosition;
        copy.zEdgePosition = this.zEdgePosition;
        copy.noiseVal = noiseVal;
        return copy;
    }
}
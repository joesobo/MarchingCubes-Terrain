﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ChunkGenerator : MonoBehaviour
{
    private int resolution = 2;
    private float isoLevel = 0.5f;
    private NoiseGenerator densityGenerator;
    public List<Voxel> voxelList = new List<Voxel>();
    private List<Triangle> triangles = new List<Triangle>();
    private VoxelTables vTables = new VoxelTables();
    public ChunkGenerator xNeighbor, yNeighbor, zNeighbor, xyNeighbor, yzNeighbor, xzNeighbor, xyzNeighbor;
    private MeshFilter meshFilter;
    private float voxelSize;
    private Vector3Int chunkPosition;
    private Vector3Int mapSize;

    public void Initialize(int resolution, float isoLevel, Vector3Int chunkPos, Vector3Int mapSize)
    {
        this.resolution = resolution;
        this.isoLevel = isoLevel;
        this.chunkPosition = chunkPos;
        this.mapSize = mapSize;

        densityGenerator = FindObjectOfType<NoiseGenerator>();

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
        }

        voxelSize = 1f / resolution;
        voxelList = new List<Voxel>();
        meshFilter.mesh = new Mesh();

        GenerateVoxelNoise();
    }

    public void ReDraw()
    {
        voxelSize = 1f / resolution;
        voxelList = new List<Voxel>();
        meshFilter.mesh = new Mesh();

        GenerateVoxelNoise();
        RecreateMesh();
    }

    public void RecreateMesh()
    {
        triangles.Clear();

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    March(new Vector3(x, y, z));
                }
            }
        }

        CreateFrontMesh();
        CreateBackMesh();
    }

    //Finds the index of the coordinate
    public int ConvertCoordToIndex(Vector3 pos)
    {
        Vector3Int intPos = new Vector3Int();
        intPos.x = (int)Mathf.Ceil(pos.x);
        intPos.y = (int)Mathf.Ceil(pos.y);
        intPos.z = (int)Mathf.Ceil(pos.z);
        return intPos.z + intPos.y * resolution + intPos.x * resolution * resolution;
    }

    //Calculates midpoint between voxel point based on noise value
    private Vector3 InterpolateVerts(Voxel v1, Voxel v2)
    {
        float t = (isoLevel - v1.noiseVal) / (v2.noiseVal - v1.noiseVal);
        return v1.position + t * (v2.position - v1.position);
    }

    private void CreateFrontMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[triangles.Count * 3];
        int[] meshTriangles = new int[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++)
        {
            meshTriangles[i * 3] = i * 3;
            meshTriangles[i * 3 + 1] = i * 3 + 1;
            meshTriangles[i * 3 + 2] = i * 3 + 2;

            vertices[i * 3] = triangles[i].posA;
            vertices[i * 3 + 1] = triangles[i].posB;
            vertices[i * 3 + 2] = triangles[i].posC;
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        Vector3 scale = GetComponent<Transform>().localScale;
        mesh.SetUVs(0, UvCalculator.CalculateUVs(vertices, scale.magnitude));
        NormalSolver.RecalculateNormals(mesh, 180);

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    //completely doubles the triangles, inverting them to be seen from the back
    private void CreateBackMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        Vector3[] normals = mesh.normals;
        int oldVerticesSize = vertices.Length;
        Vector3[] newVerts = new Vector3[oldVerticesSize * 2];
        Vector2[] newUv = new Vector2[oldVerticesSize * 2];
        Vector3[] newNorms = new Vector3[oldVerticesSize * 2];

        for (int j = 0; j < oldVerticesSize; j++)
        {
            // duplicate vertices and uvs:
            newVerts[j] = newVerts[j + oldVerticesSize] = vertices[j];
            newUv[j] = newUv[j + oldVerticesSize] = uv[j];
            // copy the original normals...
            newNorms[j] = normals[j];
            // and invert the new ones
            newNorms[j + oldVerticesSize] = -normals[j];
        }

        int[] triangles = mesh.triangles;
        int oldTrianglesSize = triangles.Length;
        int[] newTris = new int[oldTrianglesSize * 2]; // double the triangles

        for (int i = 0; i < oldTrianglesSize; i += 3)
        {
            // copy the original triangle
            newTris[i] = triangles[i];
            newTris[i + 1] = triangles[i + 1];
            newTris[i + 2] = triangles[i + 2];
            // save the new reversed triangle
            int j = i + oldTrianglesSize;
            newTris[j] = triangles[i] + oldVerticesSize;
            newTris[j + 2] = triangles[i + 1] + oldVerticesSize;
            newTris[j + 1] = triangles[i + 2] + oldVerticesSize;
        }
        mesh.vertices = newVerts;
        mesh.uv = newUv;
        mesh.normals = newNorms;
        mesh.triangles = newTris; // assign triangles last!
        meshCollider.sharedMesh = mesh;
    }

    private void GenerateVoxelNoise()
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 localPos = pos * voxelSize;
                    Vector3 worldPos = localPos + chunkPosition;
                    worldPos.x /= mapSize.x;
                    worldPos.y /= mapSize.y;
                    worldPos.z /= mapSize.z;
                    float noise = densityGenerator.Generate(worldPos);

                    Voxel voxel = new Voxel(x * voxelSize, y * voxelSize, z * voxelSize, noise, voxelSize);
                    voxelList.Add(voxel);
                }
            }
        }
    }

    private void March(Vector3 pos)
    {
        List<Voxel> cubeCorners = new List<Voxel>();
        Voxel a, b, c, d, e, f, g, h;

        //MarchEdges(pos);

        //corner and edge position
        if (pos.z >= resolution - 1 || pos.x >= resolution - 1 || pos.y >= resolution - 1)
        {
            return;
        }
        else
        {
            //find index of 8 corner coords
            a = voxelList[ConvertCoordToIndex(new Vector3(pos.x, pos.y, pos.z))].Copy();
            b = voxelList[ConvertCoordToIndex(new Vector3(pos.x, pos.y, pos.z + 1))].Copy();
            c = voxelList[ConvertCoordToIndex(new Vector3(pos.x + 1, pos.y, pos.z + 1))].Copy();
            d = voxelList[ConvertCoordToIndex(new Vector3(pos.x + 1, pos.y, pos.z))].Copy();
            e = voxelList[ConvertCoordToIndex(new Vector3(pos.x, pos.y + 1, pos.z))].Copy();
            f = voxelList[ConvertCoordToIndex(new Vector3(pos.x, pos.y + 1, pos.z + 1))].Copy();
            g = voxelList[ConvertCoordToIndex(new Vector3(pos.x + 1, pos.y + 1, pos.z + 1))].Copy();
            h = voxelList[ConvertCoordToIndex(new Vector3(pos.x + 1, pos.y + 1, pos.z))].Copy();

            cubeCorners = new List<Voxel>{
                a,b,c,d,e,f,g,h
            };

            //finds special combination of 256 using indices
            int cubeIndex = 0;
            if (cubeCorners[0].noiseVal > isoLevel) cubeIndex |= 1;
            if (cubeCorners[1].noiseVal > isoLevel) cubeIndex |= 2;
            if (cubeCorners[2].noiseVal > isoLevel) cubeIndex |= 4;
            if (cubeCorners[3].noiseVal > isoLevel) cubeIndex |= 8;
            if (cubeCorners[4].noiseVal > isoLevel) cubeIndex |= 16;
            if (cubeCorners[5].noiseVal > isoLevel) cubeIndex |= 32;
            if (cubeCorners[6].noiseVal > isoLevel) cubeIndex |= 64;
            if (cubeCorners[7].noiseVal > isoLevel) cubeIndex |= 128;

            //all corners are active or inactive
            if (cubeIndex == 255 || cubeIndex == 0)
            {
                return;
            }

            //look up combination in tri table decideing which edges to add
            for (int i = 0; vTables.triTable[cubeIndex, i] != -1; i += 3)
            {
                int a0 = vTables.cornerIndexAFromEdge[vTables.triTable[cubeIndex, i]];
                int b0 = vTables.cornerIndexBFromEdge[vTables.triTable[cubeIndex, i]];

                int a1 = vTables.cornerIndexAFromEdge[vTables.triTable[cubeIndex, i + 1]];
                int b1 = vTables.cornerIndexBFromEdge[vTables.triTable[cubeIndex, i + 1]];

                int a2 = vTables.cornerIndexAFromEdge[vTables.triTable[cubeIndex, i + 2]];
                int b2 = vTables.cornerIndexBFromEdge[vTables.triTable[cubeIndex, i + 2]];

                Triangle tri = new Triangle();
                tri.posA = InterpolateVerts(cubeCorners[a0], cubeCorners[b0]);
                tri.posB = InterpolateVerts(cubeCorners[a1], cubeCorners[b1]);
                tri.posC = InterpolateVerts(cubeCorners[a2], cubeCorners[b2]);
                triangles.Add(tri);
            }
        }
    }
}
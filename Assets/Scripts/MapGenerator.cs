using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Vector3Int mapSize;
    public int voxelResolution = 8;
    public float isoLevel = 0.5f;
    private List<ChunkGenerator> chunkList;

    private void Awake(){
        chunkList = new List<ChunkGenerator>();

        //GenerateChunks();

        //DrawChunks();
    }

    void Update()
    {
        DestroyChunks();
        chunkList = new List<ChunkGenerator>();

        GenerateChunks();

        DrawChunks();
    }

    private void GenerateChunks(){
        for (int i = 0, x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++, i++)
                {
                    Vector3Int chunkPosition = new Vector3Int(x, y, z);

                    //Create new chunk
                    GameObject chunk = Instantiate(chunkPrefab);
                    chunk.name = "Chunk (" + x + ", " + y + ", " + z + ")";
                    chunk.transform.localPosition = chunkPosition;
                    chunk.transform.localScale = Vector3.one;
                    chunk.transform.parent = transform;

                    ChunkGenerator chunkGenerator = chunk.GetComponent<ChunkGenerator>();
                    chunkGenerator.Initialize(voxelResolution, isoLevel, chunkPosition, mapSize);
                    chunkList.Add(chunkGenerator);

                    //Assign Neighbors
                    AssignNeighbors(chunkPosition, i, chunkGenerator);
                }
            }
        }
    }

    private void DrawChunks(){
        for (int i = 0, x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++, i++)
                {
                    chunkList[i].ReDraw();
                }
            }
        }
    }

    //assigns the neighbors of a chunk based on its location
    private void AssignNeighbors(Vector3Int chunkPos, int i, ChunkGenerator chunkGen){
        if(chunkPos.z > 0){
            chunkList[i-1].zNeighbor = chunkGen;
            if(chunkPos.y > 0){
                chunkList[i - mapSize.z - 1].yzNeighbor = chunkGen;
                if(chunkPos.x > 0){
                    chunkList[i - (mapSize.z * mapSize.y) - mapSize.z - 1].xyzNeighbor = chunkGen;
                }
            }
            if(chunkPos.x > 0){
                chunkList[i - (mapSize.z * mapSize.y) - 1].xzNeighbor = chunkGen;
            }
        }
        if(chunkPos.y > 0){
            chunkList[i - mapSize.z].yNeighbor = chunkGen;
            if(chunkPos.x > 0){
                chunkList[i - (mapSize.z * mapSize.y) - mapSize.z].xyNeighbor = chunkGen;
            }
        }
        if(chunkPos.x > 0){
            chunkList[i - (mapSize.z * mapSize.y)].xNeighbor = chunkGen;
        }
    }

    //takes in a coord and converts it to an index based on how it was spawned (z => y => x)
    public int ConvertCoordToIndex(Vector3 pos)
    {
        Vector3Int intPos = new Vector3Int();
        intPos.x = (int)pos.x;
        intPos.y = (int)pos.y;
        intPos.z = (int)pos.z;
        return intPos.z + intPos.y * mapSize.z + intPos.x * mapSize.z * mapSize.y;
    }

    private void DestroyChunks(){
        ChunkGenerator[] oldChunks = FindObjectsOfType<ChunkGenerator>();
        foreach (ChunkGenerator chunk in oldChunks)
        {
            DestroyImmediate(chunk.gameObject);
        }
    }
}
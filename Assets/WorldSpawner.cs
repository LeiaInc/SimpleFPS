using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    public Transform chunkPrefab;
    public Transform blockPrefab;
    public Transform treePrefab;

    public int worldWidth;
    public int worldLength;
    public int worldHeight;

    Transform[,] chunks;

    void Start()
    {
        chunks = new Transform[worldWidth / 10,worldLength / 10];

        
        for (int ix = 0; ix < worldWidth/10; ix++)
        {
            for (int iz = 0; iz < worldLength/10; iz++)
            {
                chunks[ix,iz] = Instantiate(chunkPrefab, new Vector3(ix * 10, 0, iz * 10), Quaternion.identity);
            }
        }


        for (int ix = 0; ix < worldWidth; ix++)
        {
            for (int iz = 0; iz < worldLength; iz++)
            {
                for (int iy = 0; iy < worldHeight; iy++)
                {
                    float holeSize = (Mathf.Sin(ix / 10f) * Mathf.Cos(iz / 10f) + 2) * 5;
                    if (iy == Mathf.Round((Mathf.Sin(ix / holeSize) * Mathf.Cos(iz / holeSize) + 1) * (worldHeight / 3f)))
                    {
                        Instantiate(blockPrefab, new Vector3(ix, iy, iz), Quaternion.identity, chunks[ix/10,iz/10]);
                        if (Random.value > .99f)
                        {
                            Instantiate(treePrefab, new Vector3(ix, iy + 1, iz), Quaternion.identity, chunks[ix/10,iz/10]);
                        }
                    }
                }
            }
        }
    }
}

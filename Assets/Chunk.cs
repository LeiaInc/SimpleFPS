using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        player = Camera.main.transform;
        Invoke("UpdateChunk",1f);
    }

    void UpdateChunk()
    {
        gameObject.SetActive(Vector3.Distance(transform.position, player.position + player.forward * 20) < 75);        
        Invoke("UpdateChunk",1f);
    }
}

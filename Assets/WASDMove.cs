using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMove : MonoBehaviour
{
    public float speed = .025f;

    void Update()
    {
        transform.position += transform.forward * Input.GetAxis("Vertical") * speed;
        transform.position += transform.right * Input.GetAxis("Horizontal") * speed;
        transform.position += transform.up * Input.GetAxis("Up") * speed;
    }
}

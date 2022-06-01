using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMove : MonoBehaviour
{
    float sensitivity = .1f;

    void Update()
    {
        transform.position += transform.forward * Input.GetAxis("Vertical") * sensitivity;
        transform.position += transform.right * Input.GetAxis("Horizontal") * sensitivity;
    }
}

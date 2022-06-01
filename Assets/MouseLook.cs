using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    float rx, ry, rz;
    Vector3 mousePositionPrev;
    float sensitivity = 4f;

    // Start is called before the first frame update
    void Start()
    {
        mousePositionPrev = Input.mousePosition;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Screen.width = "+Screen.width);
        Debug.Log("Screen.height = "+Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Input.mousePosition = "+Input.mousePosition);
        rx -= Input.GetAxis("Mouse Y") * sensitivity; //(Input.mousePosition.y - mousePositionPrev.y) * sensitivity;
        rx = Mathf.Clamp(rx,-89f,89f);
        ry += Input.GetAxis("Mouse X") * sensitivity; //(Input.mousePosition.x - mousePositionPrev.x) * sensitivity;
        transform.rotation = Quaternion.Euler(rx, ry, rz);

        mousePositionPrev = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

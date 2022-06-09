using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacer : MonoBehaviour
{
    public Transform blockPrefab;
    public Transform positionBlock;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit);

        if (hit.transform != null)
        {
            Vector3 closerPoint = hit.transform.position - Vector3.forward;

            Vector3 add = Vector3.zero;

            if (hit.point.x < hit.transform.position.x - .25f)
            {
                add.x = -1;
            }

            if (hit.point.x > hit.transform.position.x + .25f)
            {
                add.x = 1;
            }

            if (hit.point.y < hit.transform.position.y - .25f)
            {
                add.y = -1;
            }

            if (hit.point.y > hit.transform.position.y + .25f)
            {
                add.y = 1;
            }

            if (hit.point.z < hit.transform.position.z - .25f)
            {
                add.z = -1;
            }

            if (hit.point.z > hit.transform.position.z + .25f)
            {
                add.z = 1;
            }


            Vector3 placePoint = hit.transform.position + add;

            /*new Vector3(
            Mathf.Round(closerPoint.x),
            Mathf.Round(closerPoint.y),
            Mathf.Round(closerPoint.z)
            );*/

            positionBlock.position = placePoint;

            if (Input.GetMouseButtonDown(0))
            {
                Transform newCube = Instantiate(blockPrefab, placePoint, Quaternion.identity);
                //newCube.localScale = Vector3.one * .1f;
            }
        }
    }
}

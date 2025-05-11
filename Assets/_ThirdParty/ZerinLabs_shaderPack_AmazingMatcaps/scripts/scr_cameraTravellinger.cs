using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_cameraTravellinger : MonoBehaviour
{
    public float transitionSpeed = 5f;

    public GameObject cameraObj;

    public GameObject[] pointObjectsArray;

    private Vector3 target_pos;
    private Quaternion target_rot;

    private int index = 0;

    //----------------------------------------------------------------------------------------
    void Start()
    {
        /*index = 0;

        Vector3 p = pointObjectsArray[index].transform.position;
        Quaternion q = pointObjectsArray[index].transform.rotation;

        repositionObject(cameraObj, p, q);*/

        Vector3 p = pointObjectsArray[index].transform.position;
        Quaternion q = pointObjectsArray[index].transform.rotation;

        target_pos = p;
        target_rot = q;
    }

    //----------------------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (index < pointObjectsArray.Length-1)
            {
                index = index + 1;
            }
            else
            {
                index = 0;
            }

            Vector3 p = pointObjectsArray[index].transform.position;
            Quaternion q = pointObjectsArray[index].transform.rotation;

            target_pos = p;
            target_rot = q;

            //repositionObject(cameraObj, p, q);
        }

        cameraObj.transform.position = Vector3.MoveTowards(cameraObj.transform.position, target_pos, transitionSpeed * Time.deltaTime);
        cameraObj.transform.rotation = Quaternion.RotateTowards(cameraObj.transform.rotation, target_rot, transitionSpeed * Time.deltaTime);
    }

    //----------------------------------------------------------------------------------------
    void repositionObject(GameObject obj, Vector3 pos, Quaternion rot)
    {
        obj.transform.position = pos;
        obj.transform.rotation = rot;
    }
}

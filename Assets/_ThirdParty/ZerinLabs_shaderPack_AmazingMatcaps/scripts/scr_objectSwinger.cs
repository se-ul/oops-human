using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_objectSwinger : MonoBehaviour
{
    public Vector3 speed = new Vector3(1f, 1f, 1f);
    public Vector3 amplitude = new Vector3(1f,1f,1f);
    public Vector3 frequency = new Vector3(1f, 1f, 1f);

    public bool useUpVector = true;

    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);
    public float rotationSpeed = 5f;

    public bool initialRandomRotation = true;

    private Vector3 origPos;

    private float rndAngle = 0f;

    //--------------------------------------------------------------------------------------------------------------------------
    // Start is called before the first frame update
    void Start()
    {
        origPos = transform.position;

        if (useUpVector == true)
        {
            rotationAxis = transform.up;
        }

        if (initialRandomRotation == true)
        {
            rndAngle = Random.Range(0f, 360f);
            transform.RotateAround(Vector3.zero, rotationAxis, rndAngle);
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void FixedUpdate()
    {
        float posX = Mathf.Sin(frequency.x * Time.fixedTime * speed.x) * amplitude.x;
        float posY = Mathf.Cos(frequency.y * Time.fixedTime * speed.y) * amplitude.y;
        float posZ = Mathf.Sin(frequency.z * Time.fixedTime * speed.z) * amplitude.z;

        //transform.position = origPos + new Vector3(posX, posY, posZ);
        
        transform.RotateAround(Vector3.zero, rotationAxis, rotationSpeed * Time.fixedDeltaTime) ;
    }
}

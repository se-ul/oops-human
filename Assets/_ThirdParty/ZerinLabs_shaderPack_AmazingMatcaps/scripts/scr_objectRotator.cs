using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_objectRotator : MonoBehaviour
{
    public bool localAxis = true; 
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);
    public float rotationSpeed = 50f;

    private Vector3 origPos;
    // Start is called before the first frame update
    void Start()
    {
        origPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (localAxis == true)
        {
            transform.RotateAround(transform.position, transform.up, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.RotateAround(transform.position, rotationAxis, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}

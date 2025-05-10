using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public AudioSource splashSound;
    public Transform characterMesh;
    private bool wasAboveMaxAltitude = false;

    public Transform planetCenter;
    public float gravityStrength = 9.8f;
    public float ascendPower = 50f;
    public float turnSpeed = 20f;
    public float maxAltitude = 30f;
    public float moveSpeed = 6f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    float ascendVelocity = 0f;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        m_Rigidbody.useGravity = false;
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 gravityDir = (planetCenter.position - transform.position).normalized;
        float currentAltitude = Vector3.Distance(transform.position, planetCenter.position);

        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 right = Vector3.Cross(-gravityDir, transform.forward);
        Vector3 forward = Vector3.Cross(right, -gravityDir);
        m_Movement = (right * horizontal + forward * vertical).normalized;

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("Walk", isWalking);

        if (!Input.GetButton("Jump"))
        {
            float gravityMultiplier = currentAltitude > maxAltitude ? 3f : 1f;
            m_Rigidbody.AddForce(gravityDir * gravityStrength * gravityMultiplier, ForceMode.Acceleration);
        }
        else
        {
            if (currentAltitude >= maxAltitude)
            {
                // Do not apply ascend force
            }
            else
            {
                // reset vertical fall
                Vector3 velocity = m_Rigidbody.velocity;
                float verticalSpeed = Vector3.Dot(velocity, -gravityDir);
                if (verticalSpeed < 0f)
                {
                    velocity -= -gravityDir * verticalSpeed;
                    m_Rigidbody.velocity = velocity;
                }
                float targetSpeed = ascendPower;
                float smoothAscend = Mathf.SmoothDamp(
                    Vector3.Dot(m_Rigidbody.velocity, -gravityDir),
                    targetSpeed,
                    ref ascendVelocity,
                    0.2f
                );
                Vector3 newVelocity = m_Rigidbody.velocity;
                newVelocity += -gravityDir * (smoothAscend - Vector3.Dot(m_Rigidbody.velocity, -gravityDir));
                m_Rigidbody.velocity = newVelocity;
            }
        }

        // Rotate character to face movement direction, using correct up vector for spherical surface
        if (isWalking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_Movement, -gravityDir);
            if (characterMesh != null)
            {
                characterMesh.rotation = Quaternion.Slerp(characterMesh.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        Debug.Log("Jump button held: " + Input.GetButton("Jump") + ", Altitude: " + currentAltitude);

        bool isAboveMaxAltitude = currentAltitude > maxAltitude;
        if (isAboveMaxAltitude != wasAboveMaxAltitude)
        {
            if (splashSound != null)
            {
                splashSound.Play();
            }
        }
        wasAboveMaxAltitude = isAboveMaxAltitude;
    }

    void OnAnimatorMove()
    {
        // Fallback movement to guarantee motion regardless of animation delta
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * moveSpeed * Time.deltaTime);
    }
}
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

    [Header("Camera Follow")]
    public Transform followCamera;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraSmoothTime = 0.2f;
    private Vector3 cameraVelocity = Vector3.zero;

    private float yaw = 0f;
    private float pitch = 20f;
    public float mouseSensitivity = 2f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

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
        Vector3 cameraForward = Vector3.ProjectOnPlane(followCamera.forward, gravityDir).normalized;
        Vector3 cameraRight = Vector3.Cross(-gravityDir, cameraForward);
        m_Movement = (cameraRight * horizontal + cameraForward * vertical).normalized;

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

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Camera follow logic
        if (followCamera != null)
        {
            Vector3 basePosition = characterMesh != null ? characterMesh.position : transform.position;
            Vector3 surfaceNormal = (basePosition - planetCenter.position).normalized;
            Quaternion yawRotation = Quaternion.AngleAxis(yaw, surfaceNormal);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, Vector3.Cross(surfaceNormal, yawRotation * Vector3.forward));
            Vector3 rotatedDirection = (pitchRotation * yawRotation) * Vector3.forward;
            Vector3 targetCameraPos = basePosition - rotatedDirection * cameraDistance + surfaceNormal * cameraHeight;
            followCamera.position = Vector3.SmoothDamp(followCamera.position, targetCameraPos, ref cameraVelocity, cameraSmoothTime);

            followCamera.rotation = Quaternion.LookRotation((basePosition - followCamera.position).normalized, surfaceNormal);
        }
    }

    void OnAnimatorMove()
    {
        // Fallback movement to guarantee motion regardless of animation delta
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * moveSpeed * Time.deltaTime);
    }
}
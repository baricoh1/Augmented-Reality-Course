using UnityEngine;
using UnityEngine.InputSystem;

public class CubeJump : MonoBehaviour
{
    public float jumpForce = 5f;
    private bool isGrounded = true;
    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // If the cube doesn't have a Rigidbody, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Make sure the Rigidbody uses gravity
        rb.useGravity = true;
    }

    void Update()
    {
        // Check for Space key with the new Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            // Apply upward force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Detect when the cube hits the ground
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the ground
        if (collision.gameObject.CompareTag("Ground") || collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}
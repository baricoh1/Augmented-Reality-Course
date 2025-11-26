using UnityEngine;
using UnityEngine.InputSystem;

public class CubeJump : MonoBehaviour
{
    public float jumpForce = 5f;
    public GameObject groundObject;  // הקרקע שאתה גורר מה-Inpector

    private bool isGrounded = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = true;
    }

    void Update()
    {
        // SPACE – בדיקה עם Input System
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // בדיקה אם האובייקט שנגענו בו הוא הקרקע שהוגדרה
        if (groundObject != null && collision.gameObject == groundObject)
        {
            isGrounded = true;
        }

        // אופציונלי: גם בדיקת משטח שטוח מתחת לקובייה
        if (collision.contacts.Length > 0 &&
            collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
        UnityEngine.Debug.Log("message");
    }
}
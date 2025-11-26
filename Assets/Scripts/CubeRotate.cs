using UnityEngine;
using UnityEngine.InputSystem;

public class CubeJump : MonoBehaviour
{
    public float jumpForce = 300f; // ערך ברירת מחדל חזק
    public GameObject groundObject;

    // הוספתי SerializeField כדי שתוכל לראות את ה-V הזה ב-Inspector בזמן משחק!
    [SerializeField] private bool isGrounded = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // הגנה: אם אין ריגידבודי, נוסיף אחד
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = true;
        // מוודא שהאילוצים לא נועלים את הקפיצה
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // בדיקה האם המקלדת קיימת והמקש נלחץ
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                Debug.Log("JUMPING! Force applied."); // אם זה מודפס, הקוד עובד והבעיה בפיזיקה
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
            else
            {
                Debug.Log("Can't jump - isGrounded is FALSE"); // תדע אם המחשב חושב שאתה באוויר
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // בדיקה 1: לפי האובייקט שגררת
        if (groundObject != null && collision.gameObject == groundObject)
        {
            isGrounded = true;
            Debug.Log("Landed on Ground Object");
        }
        // בדיקה 2: לפי הזווית (למקרה שלא גררת או שנגעת ברצפה אחרת)
        else if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            Debug.Log("Landed on Flat Surface");
        }
    }
}
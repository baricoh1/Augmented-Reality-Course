using UnityEngine;

public class Racecar : MonoBehaviour
{
    // --- TAG FOR DEBUGGING ---
    // הוספתי צבע צהוב כדי שיהיה קל לזהות
    private const string TAG = "<color=yellow>[RaceCar]</color> ";

    [Header("Drive Settings")]
    public float driveSpeed = 0.2f;
    public float turnSpeed = 80f;

    [Header("Car Root")]
    public Transform carRoot;

    [Header("UI")]
    public GameObject joystickUI;

    private bool isDriving = false;

    // Inputs
    private bool moveForward = false;
    private bool moveBackward = false;
    private bool turnLeft = false;
    private bool turnRight = false;


    void Update()
    {
        if (!isDriving) return;

        if (carRoot == null)
        {
            Debug.LogError(TAG + "❌ Error: carRoot is NULL!");
            return;
        }

        // --- תנועה ---
        if (moveForward)
        {
            // שורה זו תראה לך אם הוא אשכרה מנסה לזוז בתוך ה-Update
            Debug.Log(TAG + "Update Loop: Moving Forward...");
            carRoot.Translate(Vector3.forward * driveSpeed * Time.deltaTime, Space.Self);
        }

        if (moveBackward)
        {
            Debug.Log(TAG + "Update Loop: Moving Backward...");
            carRoot.Translate(Vector3.back * driveSpeed * Time.deltaTime, Space.Self);
        }

        // --- סיבוב ---
        if (turnLeft)
            carRoot.Rotate(Vector3.up, -turnSpeed * Time.deltaTime, Space.Self);

        if (turnRight)
            carRoot.Rotate(Vector3.up, turnSpeed * Time.deltaTime, Space.Self);
    }

    public void StartTestDrive()
    {
        // 1. ניקוי רכיבים ראשיים (כמו קודם)
        var myCollider = GetComponent<Collider>();
        if (myCollider != null) Destroy(myCollider);

        var myRotator = GetComponent("ARObjectRotator") as MonoBehaviour;
        if (myRotator != null) Destroy(myRotator);

        // 2. טיפול בילד עם האנימציה (Step_05_Final)
        Animator childAnim = GetComponentInChildren<Animator>();
        if (childAnim != null)
        {
            Transform t = childAnim.transform;

            // --- עדכון נתונים כפוי (לפי התמונה ששלחת) ---
            // משתמשים ב-Local כי זה ביחס לאבא
            t.localPosition = new Vector3(1.3713f, -0.7999f, -0.0738f);
            t.localRotation = Quaternion.Euler(180f, 90f, 90f);
            t.localScale = new Vector3(1f, 1f, 1.042f);

            // עכשיו שהכל במקום - משמידים את האנימטור
            Destroy(childAnim);
        }

        // --- התחלת הנהיגה ---
        isDriving = true;

        if (joystickUI != null)
            joystickUI.SetActive(true);

        Debug.Log(TAG + "🚗 Test Drive Started! Transform fixed & Animator destroyed.");
    }

    public void StopTestDrive()
    {
        isDriving = false;
        if (joystickUI != null) joystickUI.SetActive(false);
    }

    // --- Events from UI ---

    public void DriveForward(bool pressed)
    {
        Debug.Log($"{TAG} Button: Forward is {pressed}");
        moveForward = pressed;
    }

    public void DriveBackward(bool pressed)
    {
        Debug.Log($"{TAG} Button: Backward is {pressed}");
        moveBackward = pressed;
    }

    public void TurnLeft(bool pressed)
    {
        Debug.Log($"{TAG} Button: Left is {pressed}");
        turnLeft = pressed;
    }

    public void TurnRight(bool pressed)
    {
        Debug.Log($"{TAG} Button: Right is {pressed}");
        turnRight = pressed;
    }
}
using UnityEngine;

public class Racecar : MonoBehaviour
{
    [Header("Drive Settings")]
    public float driveSpeed = 0.2f;   // מהירות קדימה/אחורה
    public float turnSpeed = 80f;    // מהירות סיבוב

    [Header("Car Root")]
    public Transform carRoot;         // האובייקט של הרכב (או ה-Root שלו)

    [Header("UI")]
    public GameObject joystickUI;     // פאנל הג'ויסטיק / החצים

    // מצב
    private bool isDriving = false;

    // דגלי קלט מהכפתורים
    private bool moveForward = false;
    private bool moveBackward = false;
    private bool turnLeft = false;
    private bool turnRight = false;


    void Update()
    {
        if (!isDriving || carRoot == null)
            return;

        // --- תנועה ---
        if (moveForward)
            carRoot.Translate(Vector3.forward * driveSpeed * Time.deltaTime, Space.Self);

        if (moveBackward)
            carRoot.Translate(Vector3.back * driveSpeed * Time.deltaTime, Space.Self);

        // --- סיבוב ---
        if (turnLeft)
            carRoot.Rotate(Vector3.up, -turnSpeed * Time.deltaTime, Space.Self);

        if (turnRight)
            carRoot.Rotate(Vector3.up, turnSpeed * Time.deltaTime, Space.Self);
    }

    // נקרא מ-BookSequenceManager בסוף הבנייה
    public void StartTestDrive()
    {
        isDriving = true;

        if (joystickUI != null)
            joystickUI.SetActive(true);
    }

    public void StopTestDrive()
    {
        isDriving = false;

        if (joystickUI != null)
            joystickUI.SetActive(false);
    }

    // פונקציות לאירועים מה-UI (EventTrigger)
    public void DriveForward(bool pressed) => moveForward = pressed;
    public void DriveBackward(bool pressed) => moveBackward = pressed;
    public void TurnLeft(bool pressed) => turnLeft = pressed;
    public void TurnRight(bool pressed) => turnRight = pressed;
}

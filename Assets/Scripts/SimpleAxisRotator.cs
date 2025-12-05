using UnityEngine;
using UnityEngine.EventSystems; // חובה בשביל לזהות לחיצה על כפתורים

public class SimpleAxisRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 0.2f;
    public bool invertDirection = false;

    [Header("Axis Selection")]
    public bool rotateX = true; // סיבוב למעלה/למטה
    public bool rotateY = false; // סיבוב לצדדים

    void Update()
    {
        // בדיקה: אם יש נגיעה במסך
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // 1. בדיקה שהמשתמש לא לוחץ כרגע על כפתור (UI)
            if (IsPointerOverUI(touch)) return;

            // 2. זיהוי תזוזה (Move)
            if (touch.phase == TouchPhase.Moved)
            {
                // קבלת כיוון הגרירה של האצבע
                float dragDistance = touch.deltaPosition.y; // גרירה אנכית משפיעה על ציר ה-X

                // אם רוצים לסובב לצדדים (Y), נשתמש ב-deltaPosition.x
                if (rotateY && !rotateX) dragDistance = touch.deltaPosition.x;

                float direction = invertDirection ? 1 : -1;
                float rotationAmount = dragDistance * rotationSpeed * direction;

                // 3. ביצוע הסיבוב
                if (rotateX)
                    transform.Rotate(Vector3.right * rotationAmount, Space.Self);

                if (rotateY)
                    transform.Rotate(Vector3.up * -rotationAmount, Space.Self);
            }
        }

        // --- תמיכה בעכבר (כדי שתוכל לבדוק במחשב) ---
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            float mouseDrag = Input.GetAxis("Mouse Y"); // עכבר למעלה/למטה
            float direction = invertDirection ? 1 : -1;
            
            if (rotateX)
                transform.Rotate(Vector3.right * mouseDrag * rotationSpeed * 50 * direction, Space.Self);
        }
#endif
    }

    // פונקציית עזר לבדוק אם נגענו בכפתור UI
    bool IsPointerOverUI(Touch touch)
    {
        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }
}
using UnityEngine;

public class ARObjectRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 0.2f;
    public bool rotateAroundYOnly = true;

    private bool _isDragging = false;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        // מוודא שיש קוליידר, אחרת אי אפשר לגעת באובייקט
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"[Rotator] ERROR: Object {name} is missing a Collider! Please add a BoxCollider.");
        }
    }

    void Update()
    {
        // מתמקד רק בלוגיקה, בלי להספים את הקונסול
#if UNITY_ANDROID || UNITY_IOS
        HandleTouch();
#else
        HandleMouse();
#endif
    }

    // ===============================
    // ----------- TOUCH -------------
    // ===============================
    void HandleTouch()
    {
        if (Input.touchCount == 0) return; // אם אין מגע, פשוט צא בשקט

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            // בדיקה: האם נגעתי באובייקט הזה?
            if (IsTouchingThisObject(touch.position))
            {
                _isDragging = true;
                Debug.Log("[Rotator] Started dragging object!");
            }
        }
        else if (touch.phase == TouchPhase.Moved && _isDragging)
        {
            RotateObject(touch.deltaPosition);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            _isDragging = false;
        }
    }

    // ===============================
    // ----------- MOUSE -------------
    // ===============================
    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchingThisObject(Input.mousePosition))
            {
                _isDragging = true;
                Debug.Log("[Rotator] Mouse started dragging!");
            }
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X") * 20f; // פקטור להתאמת מהירות עכבר
            float mouseY = Input.GetAxis("Mouse Y") * 20f;
            RotateObject(new Vector2(mouseX, mouseY));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }

    // ===============================
    // ----------- LOGIC -------------
    // ===============================

    // פונקציה שבודקת האם הקרן מהאצבע פוגעת באובייקט הזה
    bool IsTouchingThisObject(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    void RotateObject(Vector2 delta)
    {
        // המינוס בדלתא X הופך את התנועה לטבעית (גרירה שמאלה מסובבת שמאלה)
        float xRot = rotateAroundYOnly ? 0 : delta.y * rotationSpeed;
        float yRot = -delta.x * rotationSpeed;

        // Space.World = מסתובב סביב הציר של העולם (כמו סביבון) ולא סביב הציר העקום של עצמו
        transform.Rotate(xRot, yRot, 0f, Space.World);
    }
}
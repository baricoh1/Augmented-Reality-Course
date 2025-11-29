using UnityEngine;

public class ARObjectRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 1f; // אפשר להשאיר 1, השליטה תהיה ברגישות
    public bool rotateAroundYOnly = true;

    [Header("Safe Feel Settings")]
    public float sensitivity = 2f;    // במקום 20, שמנו 2. תוריד ל-1 אם זה עדיין מהיר
    public float heaviness = 5f;      // ככל שהמספר נמוך יותר = הכספת כבדה יותר

    private bool _isDragging = false;
    private Camera _mainCamera;

    // משתנה לשמירת התנועה החלקה
    private Vector2 _currentSmoothDelta;

    // חשיפת המשתנה החוצה למנהל הרצף
    public bool IsDragging { get { return _isDragging; } }

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) Debug.LogError("!!! CRITICAL ERROR: No camera tagged 'MainCamera' found! !!!");

        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError($"!!! CRITICAL ERROR: Object '{name}' has NO COLLIDER! Touch will fail. !!!");
        else
            Debug.Log($"[Rotator] Object '{name}' is ready with collider: {col.GetType().Name}");
    }

    void Update()
    {
        // בדיקת עכבר (עובדת גם בסימולטור)
        if (Input.GetMouseButtonDown(0))
        {
            CheckWhatDidWeHit(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            // --- כאן השינוי לתחושת הכספת ---

            // 1. קריאת התזוזה הגולמית (בלי ה-*20 האגרסיבי)
            float rawX = Input.GetAxis("Mouse X") * sensitivity;
            float rawY = Input.GetAxis("Mouse Y") * sensitivity;

            // 2. החלקה (Lerp) - נותן תחושה של משקל
            // אנחנו לא זזים מיד לערך החדש, אלא גולשים אליו לאט
            _currentSmoothDelta.x = Mathf.Lerp(_currentSmoothDelta.x, rawX, Time.deltaTime * heaviness);
            _currentSmoothDelta.y = Mathf.Lerp(_currentSmoothDelta.y, rawY, Time.deltaTime * heaviness);

            // 3. שליחה לסיבוב
            RotateObject(_currentSmoothDelta);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // איפוס הדרגתי כשעוזבים (כדי שלא יקפוץ בפעם הבאה)
            _currentSmoothDelta = Vector2.zero;

            if (_isDragging) Debug.Log("[Rotator] Stopped Dragging.");
            _isDragging = false;
        }
    }

    void CheckWhatDidWeHit(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // האם פגענו בעצמנו או בילד שלנו?
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                _isDragging = true;
            }
        }
    }

    void RotateObject(Vector2 delta)
    {
        float xRot = rotateAroundYOnly ? 0 : delta.y * rotationSpeed;
        float yRot = -delta.x * rotationSpeed;
        transform.Rotate(xRot, yRot, 0f, Space.World);
    }
}
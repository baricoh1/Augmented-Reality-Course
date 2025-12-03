using UnityEngine;

public class ARObjectRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 1f;

    [Header("Safe Feel Settings")]
    public float sensitivity = 2f;
    public float heaviness = 5f;

    private bool _isDragging = false;
    private Camera _mainCamera;
    private Vector2 _currentSmoothDelta;

    public bool IsDragging { get { return _isDragging; } }

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) Debug.LogError("!!! ERROR: Camera not found. Make sure your AR Camera is tagged as 'MainCamera' !!!");

        // בדיקה שהקוליידר קיים
        if (GetComponent<Collider>() == null)
            Debug.LogError($"!!! ERROR: Object '{name}' has NO COLLIDER! The script won't work without it. !!!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // בדיקת פגיעה ראשונית
            CheckWhatDidWeHit(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            float rawX = Input.GetAxis("Mouse X") * sensitivity;
            float rawY = Input.GetAxis("Mouse Y") * sensitivity;

            _currentSmoothDelta.x = Mathf.Lerp(_currentSmoothDelta.x, rawX, Time.deltaTime * heaviness);
            _currentSmoothDelta.y = Mathf.Lerp(_currentSmoothDelta.y, rawY, Time.deltaTime * heaviness);

            RotateObject(_currentSmoothDelta);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging) Debug.Log("Stopped Dragging");
            _currentSmoothDelta = Vector2.zero;
            _isDragging = false;
        }
    }

    void CheckWhatDidWeHit(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        // משתמשים ב-RaycastAll כדי לראות אם פגענו במשהו, גם אם הוא מאחורי הרצפה
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.transform.name); // <--- שורה חשובה לדיבוג

            // האם פגענו באובייקט הזה (האבא) או באחד הילדים שלו?
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Debug.Log("SUCCESS! Started Dragging " + name);
                _isDragging = true;
            }
            else
            {
                Debug.Log("Ignored hit on: " + hit.transform.name + " (Not me)");
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing.");
        }
    }

    void RotateObject(Vector2 delta)
    {
        // סיבוב ימינה/שמאלה - ביחס לעולם (שומר על המודל ישר)
        float yRot = -delta.x * rotationSpeed;
        transform.Rotate(Vector3.up, yRot, Space.World);

        // סיבוב למעלה/למטה - ביחס לאובייקט (מטה אותו אליך)
        float xRot = delta.y * rotationSpeed;
        transform.Rotate(Vector3.right, xRot, Space.Self);
    }
}
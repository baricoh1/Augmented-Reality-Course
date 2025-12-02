using UnityEngine;

public class ARObjectRotator : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 1f;
    public bool rotateAroundYOnly = true;

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
        // בדיקות תקינות שנשארו מהקוד המקורי שלך - מצוין
        if (_mainCamera == null) Debug.LogError("!!! CRITICAL ERROR: No camera tagged 'MainCamera' found! !!!");
        if (GetComponent<Collider>() == null) Debug.LogError($"!!! Object '{name}' has NO COLLIDER! !!!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
            _currentSmoothDelta = Vector2.zero;
            _isDragging = false;
        }
    }

    void CheckWhatDidWeHit(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                _isDragging = true;
            }
        }
    }

    void RotateObject(Vector2 delta)
    {
        // --- השינוי הגדול: Space.Self ---
        float xRot = rotateAroundYOnly ? 0 : delta.y * rotationSpeed;
        float yRot = -delta.x * rotationSpeed;

        // מסובבים ביחס לעצמנו (כלומר ביחס לדף שעליו אנחנו יושבים)
        transform.Rotate(xRot, yRot, 0f, Space.Self);
    }
}
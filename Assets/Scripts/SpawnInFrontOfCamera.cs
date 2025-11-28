using UnityEngine;

public class SpawnInFrontOfCamera : MonoBehaviour
{
    public GameObject objectPrefab;
    public float distance = 0.5f;

    public Camera targetCamera;   // 👈 לגרור לפה את ה-AR Camera

    private GameObject spawnedObject;

    void Awake()
    {
        // גיבוי – אם שכחנו לשים ב-Inspector ננסה Camera.main
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (targetCamera == null)
            {
                Debug.LogError("targetCamera is NULL!");
                return;
            }

            if (objectPrefab == null)
            {
                Debug.LogError("objectPrefab is NULL!");
                return;
            }

            Vector3 pos = targetCamera.transform.position +
                          targetCamera.transform.forward * distance;
            Quaternion rot = targetCamera.transform.rotation;

            Debug.Log("Spawning at: " + pos);

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(objectPrefab, pos, rot);
                Debug.Log("Spawned new object!");
            }
            else
            {
                spawnedObject.transform.SetPositionAndRotation(pos, rot);
                Debug.Log("Moved existing object!");
            }
        }
    }
}

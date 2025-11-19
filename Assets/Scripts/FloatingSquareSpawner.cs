using UnityEngine;

public class FloatingSquareSpawner : MonoBehaviour
{
    public Camera arCamera;           // ה-AR Camera מתוך XR Origin
    public GameObject squarePrefab;   // הפריפאב של הריבוע
    public float distanceFromCamera = 2.5f;

    void Update()
    {
        // נניח שמייצרים ריבוע על tap ראשון
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SpawnSquare();
        }
    }

    private void SpawnSquare()
    {
        if (arCamera == null || squarePrefab == null) return;

        Vector3 pos = arCamera.transform.position +
                      arCamera.transform.forward * distanceFromCamera;

        // שהריבוע יפנה למצלמה
        Quaternion rot = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);

        Instantiate(squarePrefab, pos, rot);
    }
}

using UnityEngine;

public class FloatingSquare : MonoBehaviour
{
    [Header("סיבוב רנדומלי")]
    public float minRotSpeed = 40f;   // מהירות מינימלית
    public float maxRotSpeed = 150f;  // מהירות מקסימלית

    private Vector3 axis;
    private float speed;

    void Start()
    {
        // בוחר ציר אקראי באורך 1
        axis = Random.onUnitSphere;

        // בוחר מהירות אקראית
        speed = Random.Range(minRotSpeed, maxRotSpeed);
    }

    void Update()
    {
        // מסובב סביב הציר שנבחר
        transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }
}

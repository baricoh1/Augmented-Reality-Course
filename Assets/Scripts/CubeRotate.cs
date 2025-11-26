using UnityEngine;  

public class CubeRotate : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float moveSpeed = 5f;

    void Update()
    {
        // סיבוב קבוע
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        Vector3 movement = Vector3.zero;

        // W או חץ למעלה
        if (Input.GetKey(KeyCode.W)  Input.GetKey(KeyCode.UpArrow))
            movement.z = 1;

        // S או חץ למטה
        if (Input.GetKey(KeyCode.S)  Input.GetKey(KeyCode.DownArrow))
            movement.z = -1;

        // A או חץ שמאלה
        if (Input.GetKey(KeyCode.A)  Input.GetKey(KeyCode.LeftArrow))
            movement.x = -1;

        // D או חץ ימינה
        if (Input.GetKey(KeyCode.D)  Input.GetKey(KeyCode.RightArrow))
            movement.x = 1;

        // הזזה בפועל
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
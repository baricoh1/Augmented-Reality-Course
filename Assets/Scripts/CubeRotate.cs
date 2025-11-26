using UnityEngine;
using UnityEngine.InputSystem;

public class CubeRotate : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float moveSpeed = 5f;

    void Update()
    {
        // סיבוב קבוע
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        if (Keyboard.current != null)
        {
            Vector3 movement = Vector3.zero;

            // W או חץ למעלה
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                movement.z = 1;

            // S או חץ למטה
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                movement.z = -1;

            // A או חץ שמאלה
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                movement.x = -1;

            // D או חץ ימינה
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                movement.x = 1;

            // הזזה בפועל
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
    }
}

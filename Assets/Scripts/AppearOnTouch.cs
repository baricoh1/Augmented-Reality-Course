using UnityEngine;

public class AppearOnTouch : MonoBehaviour
{
    public GameObject legoModel; // לכאן נגרור את המודל
    public float distance = 0.5f; // מרחק חצי מטר מהמצלמה

    void Update()
    {
        // בדיקה אם יש לחיצה על המסך
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SpawnLego();
        }

        // בדיקה למחשב (קליק עכבר) - כדי שתוכל לבדוק בלי לבנות לטלפון
        if (Input.GetMouseButtonDown(0))
        {
            SpawnLego();
        }
    }

    void SpawnLego()
    {
        // 1. הזזת המודל שיהיה מול המצלמה
        legoModel.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * distance);

        // 2. סיבוב המודל שיהיה מול המשתמש (אופציונלי, כדי שלא יראה עקום)
        Vector3 lookPos = Camera.main.transform.position - legoModel.transform.position;
        lookPos.y = 0;
        legoModel.transform.rotation = Quaternion.LookRotation(-lookPos);

        // 3. הפעלת המודל (זה אוטומטית יפעיל את האנימציה מהתחלה!)
        legoModel.SetActive(true);

        // אופציונלי: מכבים את הסקריפט הזה כדי שלא יזמן את הלגו שוב ושוב בכל לחיצה
        this.enabled = false;
    }
}
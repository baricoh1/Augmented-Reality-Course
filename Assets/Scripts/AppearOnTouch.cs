using UnityEngine;
using UnityEngine.XR.ARFoundation; // חובה בשביל AR
using UnityEngine.XR.ARSubsystems; // חובה בשביל סוגי פגיעה
using System.Collections.Generic;    // בשביל רשימות

public class AppearOnTouch : MonoBehaviour
{
    public GameObject legoModel;      // המודל למיקום
    public ARRaycastManager raycastManager; // המנהל שאחראי על זיהוי הפגיעה במשטח

    // רשימה שתשמור את המידע על איפה הקרן פגעה
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // בדיקת לחיצה בטלפון
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // בודקים רק ברגע הנגיעה הראשונית
            if (touch.phase == TouchPhase.Began)
            {
                // שליחת קרן: (נקודת המגע במסך, הרשימה לשמירת התוצאות, וסוג המשטח)
                // TrackableType.PlaneWithinPolygon מבטיח שזה יפגע רק בתוך הגבולות של המישור שזוהה
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // הפגיעה הראשונה (hits[0]) היא הקרובה ביותר למצלמה
                    Pose hitPose = hits[0].pose;

                    SpawnLego(hitPose.position);
                }
            }
        }
    }

    void SpawnLego(Vector3 positionOnPlane)
    {
        // 1. הזזת המודל למיקום הפגיעה במישור
        legoModel.transform.position = positionOnPlane;

        // 2. סיבוב המודל שיהיה מול המשתמש (אבל יישאר ישר על הרצפה)
        // אנחנו לוקחים את הכיוון למצלמה, אבל מאפסים את הגובה (y) כדי שהמודל לא ייטה למעלה/למטה
        Vector3 lookPos = Camera.main.transform.position - legoModel.transform.position;
        lookPos.y = 0;
        legoModel.transform.rotation = Quaternion.LookRotation(-lookPos);

        // 3. הפעלת המודל
        legoModel.SetActive(true);

        // אופציונלי: מכבים את הסקריפט כדי שלא יזוז יותר
        // this.enabled = false; 
    }
}
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class AppearOnTouch : MonoBehaviour
{
    public GameObject legoModel;
    public ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // משתנים לשמירת מיקום הלחיצה
        bool isPressed = false;
        Vector2 touchPosition = default;

        // בדיקה 1: האם אנחנו בטלפון? (Touch)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isPressed = true;
                touchPosition = touch.position;
            }
        }

        // בדיקה 2: האם אנחנו במחשב/סימולטור? (Mouse)
        // הקוד הזה ירוץ רק בתוך יוניטי אדיטור
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) // קליק שמאלי
        {
            isPressed = true;
            touchPosition = Input.mousePosition; // מיקום העכבר
        }
#endif

        // אם הייתה לחיצה (מהעכבר או מהאצבע) - נמשיך
        if (isPressed)
        {
            Debug.Log($"[Input] Click detected at: {touchPosition}");

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;
                Debug.Log($"[Raycast] HIT! Spawning at: {hitPose.position}");
                SpawnLego(hitPose.position);
            }
            else
            {
                Debug.LogWarning("[Raycast] MISSED! Raycast fired but hit nothing.");
            }
        }
    }

    void SpawnLego(Vector3 positionOnPlane)
    {
        // --- 1. חישוב המיקום החדש (חצי גובה) ---

        // לוקחים את הגובה הנוכחי של המצלמה (הטלפון)
        float cameraHeight = Camera.main.transform.position.y;

        // יוצרים מיקום חדש:
        // X, Z = המיקום שלחצת עליו ברצפה
        // Y = חצי מגובה המצלמה
        Vector3 finalPosition = new Vector3(positionOnPlane.x, cameraHeight / 2.0f, positionOnPlane.z);

        // הזזת המודל
        legoModel.transform.position = finalPosition;


        // --- 2. סיבוב לכיוון המצלמה ---

        // חישוב הווקטור: איפה המצלמה ביחס לאובייקט?
        Vector3 directionToCamera = Camera.main.transform.position - legoModel.transform.position;

        // מאפסים את הגובה כדי שהאובייקט לא "יטה" למעלה/למטה אלא רק יסתובב לצדדים
        directionToCamera.y = 0;

        // מסובבים את האובייקט.
        // הערה: מחקתי את המינוס (-) שהיה לך, כדי שהחלק הקדמי (Z) יפנה למצלמה.
        // אם המודל שלך יוצא "עם הגב למצלמה", תחזיר את המינוס לתוך הסוגריים.
        legoModel.transform.rotation = Quaternion.LookRotation(directionToCamera);

        // הדלקה
        legoModel.SetActive(true);

        Debug.Log($"[Spawn] Spawning at height: {finalPosition.y} (Camera was at: {cameraHeight})");
    }
}
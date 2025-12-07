using UnityEngine;

[ExecuteAlways] // <--- הנה הקסם! זה גורם לזה לעבוד באדיטור
[RequireComponent(typeof(LineRenderer))]
public class CableController : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("הגדרות כבל")]
    public float cableWidth = 0.05f;
    public float sagAmount = 0.3f;
    public int segments = 20;

    private LineRenderer lr;

    void Update() // רץ גם באדיטור וגם במשחק
    {
        // בדיקת בטיחות: אם לא גררת עדיין את הראשים, אל תעשה כלום
        if (startPoint == null || endPoint == null) return;

        if (lr == null) lr = GetComponent<LineRenderer>();

        // עדכון הגדרות בזמן אמת (כדי שתוכל לשחק עם העובי באדיטור)
        lr.startWidth = cableWidth;
        lr.endWidth = cableWidth;
        lr.positionCount = segments;

        DrawCurve();
    }

    void DrawCurve()
    {
        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position;

        // חישוב נקודת האמצע + הבטן
        Vector3 p1 = (p0 + p2) / 2 + (Vector3.down * sagAmount);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 pixel = CalculateBezierPoint(t, p0, p1, p2);
            lr.SetPosition(i, pixel);
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}
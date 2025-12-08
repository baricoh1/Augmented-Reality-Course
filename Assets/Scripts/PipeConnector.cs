using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PipeConnector : MonoBehaviour
{
    [Header("Settings")]
    public Transform[] waypoints;

    [Tooltip("מכפיל גודל - השאר על 1 אם הדיסקיות בגודל הנכון. שנה אם הצינור עבה/דק מדי יחסית לדיסקית")]
    public float radiusMultiplier = 0.5f; // 0.5 כי Scale מייצג קוטר, ואנחנו צריכים רדיוס

    public int pipeSegments = 16; // 16 בשביל שיהיה עגול ויפה
    public int curveDetail = 10;

    private MeshFilter meshFilter;
    private Mesh mesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        GeneratePipe();
    }

    void Update()
    {
        GeneratePipe();
    }

    void GeneratePipe()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        List<Vector3> pathPoints = new List<Vector3>();
        List<float> pathRadii = new List<float>(); // רשימה לשמירת הרדיוס בכל נקודה

        // 1. חישוב הנקודות והרדיוסים
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 p0 = waypoints[Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[i + 1].position;
            Vector3 p3 = waypoints[Mathf.Min(i + 2, waypoints.Length - 1)].position;

            // לוקחים את הגודל של הדיסקית הנוכחית והבאה
            float radiusStart = waypoints[i].localScale.x * radiusMultiplier;
            float radiusEnd = waypoints[i + 1].localScale.x * radiusMultiplier;

            for (int j = 0; j < curveDetail; j++)
            {
                float t = j / (float)curveDetail;

                // חישוב מיקום
                pathPoints.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));

                // חישוב רדיוס מדורג (Lerp) בין הדיסקית הזו לדיסקית הבאה
                pathRadii.Add(Mathf.Lerp(radiusStart, radiusEnd, t));
            }
        }

        // הוספת הנקודה האחרונה
        pathPoints.Add(waypoints[waypoints.Length - 1].position);
        pathRadii.Add(waypoints[waypoints.Length - 1].localScale.x * radiusMultiplier);

        // 2. בניית ה-Mesh
        BuildMesh(pathPoints, pathRadii);
    }

    void BuildMesh(List<Vector3> path, List<float> radii)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // שמירת הכיוון הקודם כדי למנוע פיתולים (Twisting)
        Vector3 lastUp = Vector3.up;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            // חישוב כיוון ה-Forward (משיק למסלול)
            if (i < path.Count - 1) forward += path[i + 1] - path[i];
            if (i > 0) forward += path[i] - path[i - 1];
            forward.Normalize();

            // תיקון פיתול: במקום לחשב UP מחדש, אנחנו לוקחים את ה-UP הקודם
            // ומיישרים אותו לכיוון החדש. זה שומר על רציפות.
            Vector3 right = Vector3.Cross(lastUp, forward).normalized;

            // טיפול במקרה קצה (אם הכיוון ישר למעלה)
            if (right == Vector3.zero) right = Vector3.Cross(Vector3.right, forward).normalized;

            Vector3 up = Vector3.Cross(forward, right).normalized;

            // עדכון ה-UP לפעם הבאה
            lastUp = up;

            float currentRadius = radii[i];

            // יצירת הטבעת
            for (int seg = 0; seg <= pipeSegments; seg++)
            {
                float angle = (float)seg / pipeSegments * Mathf.PI * 2;
                float x = Mathf.Cos(angle) * currentRadius;
                float y = Mathf.Sin(angle) * currentRadius;

                Vector3 pos = path[i] + (right * x) + (up * y);
                vertices.Add(transform.InverseTransformPoint(pos));

                uvs.Add(new Vector2((float)seg / pipeSegments, (float)i / path.Count));
            }
        }

        // חיבור המשולשים (ללא שינוי)
        for (int i = 0; i < path.Count - 1; i++)
        {
            for (int seg = 0; seg < pipeSegments; seg++)
            {
                int current = i * (pipeSegments + 1) + seg;
                int next = current + (pipeSegments + 1);

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
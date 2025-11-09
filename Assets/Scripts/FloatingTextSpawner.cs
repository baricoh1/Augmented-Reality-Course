using UnityEngine;
using TMPro;

public class FloatingTextSpawner : MonoBehaviour
{
    public Camera arCamera;
    public GameObject textPrefab;
    [Range(0.3f, 3f)] public float distanceFromCamera = 1.0f;
    [Range(0.02f, 0.5f)] public float worldScale = 0.22f;   
    [Range(0.2f, 2f)] public float floatSpeed = 0.3f;
    [Range(0.5f, 5f)] public float lifetime = 2f;

    private bool isSpawning = false;

    void Start()
    {
        if (!arCamera)
            arCamera = Camera.main;
    }

    void Update()
    {
        if (!isSpawning && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            StartCoroutine(SpawnOnce());
    }

    System.Collections.IEnumerator SpawnOnce()
    {
        isSpawning = true;
        SpawnFloatingText();
        yield return new WaitForSeconds(lifetime);
        isSpawning = false;
    }

    void SpawnFloatingText()
    {
        if (!arCamera || !textPrefab)
            return;

        Vector3 spawnPos = arCamera.transform.TransformPoint(0f, 0f, distanceFromCamera);
        var go = Instantiate(textPrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * worldScale;
        go.transform.rotation = Quaternion.LookRotation(
            (go.transform.position - arCamera.transform.position).normalized,
            Vector3.up
        );

        var tmp = go.GetComponent<TextMeshPro>();
        if (tmp)
        {
            if (string.IsNullOrWhiteSpace(tmp.text))
                tmp.text = "Hello AR";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 48;
        }

        go.AddComponent<FloatingMotion>().Initialize(floatSpeed, lifetime, worldScale);
    }
}

public class FloatingMotion : MonoBehaviour
{
    private float speed;
    private float lifetime;
    private float timer = 0f;
    private TextMeshPro tmp;
    private float rotationSpeed;
    private float baseScale;

    private Vector3 startPos;
    private Vector3 targetPos;

    public void Initialize(float speed, float lifetime, float baseScale)
    {
        this.speed = speed;
        this.lifetime = lifetime;
        this.baseScale = baseScale;
        tmp = GetComponent<TextMeshPro>();

        rotationSpeed = Random.Range(5f, 25f);
        startPos = transform.position;
        targetPos = startPos + Vector3.up * Random.Range(0.1f, 0.3f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        float scale = Mathf.Lerp(0.7f, 1f, Mathf.SmoothStep(0, 1, Mathf.Min(t * 2f, 1f)));
        transform.localScale = Vector3.one * (baseScale * scale);

        if (tmp)
        {
            Color c = tmp.color;
            c.a = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 1.5f)); 
            tmp.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}

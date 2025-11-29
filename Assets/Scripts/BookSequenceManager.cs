using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BookSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public struct Step
    {
        public string stepName;         // שם השלב (לסדר בעיניים)
        public GameObject sceneObject;  // האובייקט בסצנה
        public float duration;          // כמה זמן להציג (שניות)

        [Header("Step Adjustments")]
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [System.Serializable]
    public struct PageSequence
    {
        public string imageName; // שם התמונה בספרייה (למשל Page1)
        public List<Step> steps; // רשימת השלבים
    }

    [Header("Setup")]
    public ARTrackedImageManager imageManager;

    // --- התיקון כאן: מילה אחת בלבד ---
    public List<PageSequence> sequences;

    [Header("Debug")]
    public bool enableSimulation = true;

    // ניהול הרצף הפעיל
    private Coroutine currentSequenceRoutine = null;
    private string currentActivePage = "";

    // זיכרון לגדלים המקוריים
    private Dictionary<int, Vector3> _initialScales = new Dictionary<int, Vector3>();
    private Transform currentAnchor;

    void Awake()
    {
        // מעבר על כל הדפים והשלבים לשמירת הגדלים וכיבוי האובייקטים
        foreach (var page in sequences)
        {
            foreach (var step in page.steps)
            {
                if (step.sceneObject != null)
                {
                    int id = step.sceneObject.GetInstanceID();
                    if (!_initialScales.ContainsKey(id))
                    {
                        _initialScales.Add(id, step.sceneObject.transform.localScale);
                    }

                    step.sceneObject.SetActive(false);
                }
            }
        }
    }

    void Start()
    {
#if UNITY_EDITOR
        if (enableSimulation) StartCoroutine(SimulateScan());
#endif
    }

    void OnEnable() => imageManager.trackedImagesChanged += OnImageChanged;
    void OnDisable() => imageManager.trackedImagesChanged -= OnImageChanged;

    private void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            string imageName = newImage.referenceImage.name;

            // דף חדש זוהה
            if (imageName != currentActivePage)
            {
                StartSequenceForPage(imageName, newImage.transform);
            }
            else
            {
                UpdateAnchorPosition(newImage.transform);
            }
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if (updatedImage.referenceImage.name == currentActivePage)
            {
                UpdateAnchorPosition(updatedImage.transform);
            }
        }
    }

    void UpdateAnchorPosition(Transform anchor)
    {
        currentAnchor = anchor;
    }

    void StartSequenceForPage(string imageName, Transform anchor)
    {
        // חיפוש הרצף המתאים לדף
        PageSequence selectedPage = sequences.Find(p => p.imageName == imageName);

        // אם מצאנו רצף כזה ויש בו שלבים
        if (selectedPage.steps != null && selectedPage.steps.Count > 0)
        {
            if (currentSequenceRoutine != null) StopCoroutine(currentSequenceRoutine);

            currentActivePage = imageName;
            currentAnchor = anchor;
            currentSequenceRoutine = StartCoroutine(RunStepsRoutine(selectedPage));
        }
    }

    IEnumerator RunStepsRoutine(PageSequence pageData)
    {
        Debug.Log($"[Sequence] Starting sequence for {pageData.imageName}");

        for (int i = 0; i < pageData.steps.Count; i++)
        {
            Step currentStep = pageData.steps[i];
            GameObject model = currentStep.sceneObject;

            if (model != null)
            {
                // 1. הפעלה
                model.SetActive(true);

                if (currentAnchor != null)
                    model.transform.SetParent(currentAnchor, false);

                // 2. מיקום וסיבוב
                model.transform.localPosition = currentStep.positionOffset;
                model.transform.localRotation = Quaternion.Euler(currentStep.rotationOffset);

                // 3. שחזור גודל
                if (_initialScales.TryGetValue(model.GetInstanceID(), out Vector3 savedScale))
                    model.transform.localScale = savedScale;

                Debug.Log($"[Sequence] Showing: {currentStep.stepName}");

                // 4. המתנה (הצגת המודל)
                float timer = 0;
                while (timer < currentStep.duration)
                {
                    // מוודאים הצמדה לדף
                    if (currentAnchor != null && model.transform.parent != currentAnchor)
                        model.transform.SetParent(currentAnchor, false);

                    timer += Time.deltaTime;
                    yield return null;
                }

                // 5. אפקט יציאה (Fade Out)
                yield return StartCoroutine(FadeOutModel(model));

                model.SetActive(false);
            }
        }

        Debug.Log("[Sequence] Finished all steps.");
        currentActivePage = "";
    }

    IEnumerator FadeOutModel(GameObject model)
    {
        Vector3 originalScale = model.transform.localScale;
        float fadeTime = 0.5f;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / fadeTime;
            model.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
    }

    // --- סימולציה ---
    IEnumerator SimulateScan()
    {
        yield return new WaitForSeconds(2f);

        GameObject fakePage = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fakePage.name = "SIMULATED_PAGE";
        if (Camera.main)
        {
            fakePage.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1f;
            fakePage.transform.rotation = Quaternion.Euler(60, 0, 0);
        }

        if (sequences.Count > 0)
            StartSequenceForPage(sequences[0].imageName, fakePage.transform);
    }
}
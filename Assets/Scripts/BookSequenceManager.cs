using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BookSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public struct Step
    {
        public string stepName;
        public GameObject sceneObject;
        public float duration;

        [Header("Step Adjustments")]
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [System.Serializable]
    public struct PageSequence
    {
        public string imageName;
        public List<Step> steps;
    }

    [Header("Setup")]
    public ARTrackedImageManager imageManager;
    public List<PageSequence> sequences;

    [Header("Scanning Effect Settings (NEW)")]
    public GameObject scanningVisualPrefab; // <--- גרור לפה את הפריפאב של הגריד/סורק
    public float scanDuration = 2.5f;       // <--- כמה זמן הסריקה תימשך

    [Header("Cage Settings (The Prison)")]
    [Tooltip("המרחק המקסימלי מהמרכז שמותר לאובייקט לזוז (במטרים)")]
    public Vector3 cageLimits = new Vector3(0.1f, 0.05f, 0.15f);

    private Coroutine currentSequenceRoutine = null;
    private string currentActivePage = "";
    private Dictionary<int, Vector3> _initialScales = new Dictionary<int, Vector3>();

    // משתנים למנגנון הנעילה
    private GameObject _activeLockedModel = null;
    private Transform _activeAnchor = null;
    private Vector3 _targetLocalPos;

    void Awake()
    {
        foreach (var page in sequences)
        {
            foreach (var step in page.steps)
            {
                if (step.sceneObject != null)
                {
                    int id = step.sceneObject.GetInstanceID();
                    if (!_initialScales.ContainsKey(id))
                        _initialScales.Add(id, step.sceneObject.transform.localScale);

                    step.sceneObject.SetActive(false);
                }
            }
        }
    }

    void OnEnable() => imageManager.trackedImagesChanged += OnImageChanged;
    void OnDisable() => imageManager.trackedImagesChanged -= OnImageChanged;

    private void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            string imageName = newImage.referenceImage.name;
            // אם זו תמונה חדשה שלא מוצגת כרגע - נתחיל את רצף הסריקה
            if (imageName != currentActivePage)
                StartSequenceForPage(imageName, newImage.transform);
            else
                _activeAnchor = newImage.transform;
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            if (updatedImage.referenceImage.name == currentActivePage)
                _activeAnchor = updatedImage.transform;
        }
    }

    void LateUpdate()
    {
        if (_activeLockedModel != null && _activeAnchor != null)
        {
            if (_activeLockedModel.transform.parent != _activeAnchor)
                _activeLockedModel.transform.SetParent(_activeAnchor, false);

            Vector3 desiredPos = _targetLocalPos;
            desiredPos.x = Mathf.Clamp(desiredPos.x, -cageLimits.x, cageLimits.x);
            desiredPos.y = Mathf.Clamp(desiredPos.y, 0f, cageLimits.y);
            desiredPos.z = Mathf.Clamp(desiredPos.z, -cageLimits.z, cageLimits.z);

            _activeLockedModel.transform.localPosition = desiredPos;
        }
    }

    void StartSequenceForPage(string imageName, Transform anchor)
    {
        PageSequence selectedPage = sequences.Find(p => p.imageName == imageName);

        // בדוק אם יש דף כזה ואם יש לו צעדים
        if (selectedPage.steps != null && selectedPage.steps.Count > 0)
        {
            if (currentSequenceRoutine != null) StopCoroutine(currentSequenceRoutine);

            currentActivePage = imageName;
            _activeAnchor = anchor;

            // --- שינוי: במקום להריץ ישר את הצעדים, מריצים את הסריקה קודם ---
            currentSequenceRoutine = StartCoroutine(RunScanAndThenSteps(selectedPage));
        }
    }

    // --- פונקציה חדשה: קודם סורק, אחר כך מציג תוכן ---
    IEnumerator RunScanAndThenSteps(PageSequence pageData)
    {
        // שלב 1: הצגת אפקט הסריקה (הגריד)
        GameObject activeScanEffect = null;

        if (scanningVisualPrefab != null && _activeAnchor != null)
        {
            // יוצרים את הגריד על הדף המזוהה
            activeScanEffect = Instantiate(scanningVisualPrefab, _activeAnchor);

            // --- הגדרות מיקום וסיבוב (כמו שביקשת) ---
            activeScanEffect.transform.localPosition = Vector3.zero;
            activeScanEffect.transform.localRotation = Quaternion.Euler(90, 0, 0);

            // --- אנימציית הגדילה (Scale) ---

            // 1. שומרים את הגודל המקורי שקבעת בפריפאב (המטרה)
            Vector3 targetScale = activeScanEffect.transform.localScale;

            // 2. קובעים מצב התחלה: גובה 0 (סגור)
            // אנחנו מאפסים את ה-Y כדי שזה יתחיל כפס דק
            activeScanEffect.transform.localScale = new Vector3(targetScale.x, 0f, targetScale.z);

            // 3. לולאת האנימציה (נפתח לאט)
            float timer = 0;
            while (timer < scanDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / scanDuration; // מספר בין 0 ל-1

                // חישוב הגובה החדש
                float currentY = Mathf.Lerp(0f, targetScale.y, progress);

                // עדכון הגודל בפועל (שומרים על X ו-Z, משנים רק את Y)
                activeScanEffect.transform.localScale = new Vector3(targetScale.x, currentY, targetScale.z);

                yield return null;
            }

            // וידוא שהגענו לגודל הסופי
            activeScanEffect.transform.localScale = targetScale;
        }

        // המתנה קצרה כדי שיראו את הגריד המלא לרגע
        yield return new WaitForSeconds(0.2f);

        // שלב 2: מחיקת הגריד
        if (activeScanEffect != null)
        {
            Destroy(activeScanEffect);
        }

        // שלב 3: הפעלת הלוגיקה המקורית (הרובוט)
        yield return StartCoroutine(RunStepsRoutine(pageData));
    }

    IEnumerator RunStepsRoutine(PageSequence pageData)
    {
        for (int i = 0; i < pageData.steps.Count; i++)
        {
            Step currentStep = pageData.steps[i];
            GameObject model = currentStep.sceneObject;

            if (model != null)
            {
                model.SetActive(true);
                Rigidbody rb = model.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                _activeLockedModel = model;
                _targetLocalPos = currentStep.positionOffset;

                model.transform.SetParent(_activeAnchor, false);
                model.transform.localPosition = currentStep.positionOffset;
                model.transform.localRotation = Quaternion.Euler(currentStep.rotationOffset);

                if (_initialScales.TryGetValue(model.GetInstanceID(), out Vector3 savedScale))
                    model.transform.localScale = savedScale;

                ARObjectRotator rotator = model.GetComponent<ARObjectRotator>();
                float timer = 0;

                while (timer < currentStep.duration)
                {
                    bool isUserTouching = (rotator != null && rotator.IsDragging);
                    if (!isUserTouching)
                    {
                        timer += Time.deltaTime;
                    }
                    yield return null;
                }

                _activeLockedModel = null;

                yield return StartCoroutine(FadeOutModel(model));
                model.SetActive(false);
            }
        }
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
}
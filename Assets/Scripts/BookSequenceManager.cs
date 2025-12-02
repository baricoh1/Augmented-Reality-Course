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

    [Header("Cage Settings (The Prison)")]
    [Tooltip("המרחק המקסימלי מהמרכז שמותר לאובייקט לזוז (במטרים)")]
    public Vector3 cageLimits = new Vector3(0.1f, 0.05f, 0.15f);
    // X=0.1 (10 ס"מ לצדדים)
    // Y=0.05 (מקסימום 5 ס"מ גובה - שומר שלא ירחף גבוה מידי)
    // Z=0.15 (15 ס"מ למעלה/למטה על הדף)

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

    // --- מנגנון הכלוב ב-LateUpdate ---
    void LateUpdate()
    {
        if (_activeLockedModel != null && _activeAnchor != null)
        {
            // 1. ווידוא היררכיה
            if (_activeLockedModel.transform.parent != _activeAnchor)
                _activeLockedModel.transform.SetParent(_activeAnchor, false);

            // 2. חישוב המיקום הרצוי
            Vector3 desiredPos = _targetLocalPos;

            // 3. הפעלת הכלוב (Clamping)
            // אנחנו מכריחים את המיקום להישאר בתוך הגבולות שהגדרת
            desiredPos.x = Mathf.Clamp(desiredPos.x, -cageLimits.x, cageLimits.x);
            desiredPos.y = Mathf.Clamp(desiredPos.y, 0f, cageLimits.y); // לא נותנים לו לרדת מתחת לדף (0)
            desiredPos.z = Mathf.Clamp(desiredPos.z, -cageLimits.z, cageLimits.z);

            // 4. יישום המיקום הסופי
            _activeLockedModel.transform.localPosition = desiredPos;
        }
    }

    void StartSequenceForPage(string imageName, Transform anchor)
    {
        PageSequence selectedPage = sequences.Find(p => p.imageName == imageName);

        if (selectedPage.steps != null && selectedPage.steps.Count > 0)
        {
            if (currentSequenceRoutine != null) StopCoroutine(currentSequenceRoutine);

            currentActivePage = imageName;
            _activeAnchor = anchor;
            currentSequenceRoutine = StartCoroutine(RunStepsRoutine(selectedPage));
        }
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
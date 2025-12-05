using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BookSequenceManager : MonoBehaviour
{
    // --- מבני נתונים ---

    [System.Serializable]
    public struct Step
    {
        public string stepName;
        public GameObject sceneObject;
        public float duration;

        [Header("Positioning")]
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [System.Serializable]
    public struct PageSequence
    {
        public string imageName;

        [Header("Session Layout")]
        public GameObject partsLayoutPrefab;

        public List<Step> steps;
    }

    // --- הגדרות ---

    [Header("AR Setup")]
    public ARTrackedImageManager imageManager;
    public List<PageSequence> sequences;

    [Header("Phase 1: Scanning Effect")]
    public GameObject scanningVisualPrefab;
    public float scanDuration = 2.5f;

    [Header("Phase 2: Workbench (Simple Mode)")]
    public float displayDuration = 3.0f; // כמה זמן להציג את החלקים

    [Header("Interaction Settings")]
    public Vector3 cageLimits = new Vector3(0.1f, 0.05f, 0.15f);

    // --- משתנים פרטיים ---

    private Coroutine currentSequenceRoutine = null;
    private string currentActivePage = "";
    private Dictionary<int, Vector3> _initialScales = new Dictionary<int, Vector3>();
    private GameObject _activeLockedModel = null;
    private Transform _activeAnchor = null;
    private Vector3 _targetLocalPos;

    // --- Unity Methods ---

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

    void StartSequenceForPage(string imageName, Transform anchor)
    {
        PageSequence selectedPage = sequences.Find(p => p.imageName == imageName);

        if (!string.IsNullOrEmpty(selectedPage.imageName))
        {
            if (currentSequenceRoutine != null) StopCoroutine(currentSequenceRoutine);
            currentActivePage = imageName;
            _activeAnchor = anchor;
            currentSequenceRoutine = StartCoroutine(MainFlowRoutine(selectedPage));
        }
    }

    // --- Main Logic ---

    IEnumerator MainFlowRoutine(PageSequence pageData)
    {
        // 1. סריקה
        yield return StartCoroutine(RunScanningEffect());

        // 2. הצגת שולחן העבודה (ללא אנימציה - רק הצגה)
        if (pageData.partsLayoutPrefab != null)
        {
            yield return StartCoroutine(RunLayoutSimple(pageData.partsLayoutPrefab));
        }

        // 3. שלבי הבנייה
        yield return StartCoroutine(RunStepsLogic(pageData));
    }

    IEnumerator RunScanningEffect()
    {
        GameObject activeScanEffect = null;
        if (scanningVisualPrefab != null && _activeAnchor != null)
        {
            activeScanEffect = Instantiate(scanningVisualPrefab, _activeAnchor);
            activeScanEffect.SetActive(true);
            activeScanEffect.transform.localPosition = Vector3.zero;
            activeScanEffect.transform.localRotation = Quaternion.Euler(90, 0, 0);

            Vector3 targetScale = activeScanEffect.transform.localScale;
            activeScanEffect.transform.localScale = new Vector3(targetScale.x, 0f, targetScale.z);

            float timer = 0;
            while (timer < scanDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / scanDuration;
                float currentY = Mathf.Lerp(0f, targetScale.y, progress);
                activeScanEffect.transform.localScale = new Vector3(targetScale.x, currentY, targetScale.z);
                yield return null;
            }
            activeScanEffect.transform.localScale = targetScale;
        }

        yield return new WaitForSeconds(0.2f);
        if (activeScanEffect != null) Destroy(activeScanEffect);
    }

    // --- הפונקציה הפשוטה + אפקט גדילה (Pop In) ---
    IEnumerator RunLayoutSimple(GameObject layoutPrefab)
    {
        // 1. יצירה
        GameObject layoutObj = Instantiate(layoutPrefab, _activeAnchor);

        // אתחול ראשוני
        layoutObj.SetActive(true);
        layoutObj.transform.localPosition = Vector3.zero;
        layoutObj.transform.localRotation = Quaternion.identity;
        layoutObj.transform.localScale = Vector3.zero; // מתחילים מקטן

        // --- שלב א': ביטול הגרביטציה ---
        // אנחנו תופסים את כל ה-Rigidbodies ומוודאים שהגרביטציה כבויה
        // ככה הם "ירחפו" באוויר בזמן הגדילה
        Rigidbody[] allRbs = layoutObj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            rb.useGravity = false; // מכבה גרביטציה
            rb.velocity = Vector3.zero; // מאפס מהירות ליתר ביטחון
        }

        // הדלקת הילדים (Visuals)
        foreach (Transform child in layoutObj.transform)
        {
            child.gameObject.SetActive(true);
            if (child.localScale == Vector3.zero) child.localScale = Vector3.one;
        }

        // 2. אנימציית גדילה (Pop In)
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3;
            layoutObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
        layoutObj.transform.localScale = Vector3.one; // וידוא גודל סופי

        // --- שלב ב': הפעלת הגרביטציה (הנפילה!) ---
        // האנימציה נגמרה, עכשיו מפילים אותם
        foreach (var rb in allRbs)
        {
            rb.useGravity = true; // מפעיל גרביטציה
            rb.WakeUp(); // מנער את המנוע הפיזיקלי שיתחיל לעבוד
        }

        // 3. המתנה שהמשתמש יסתכל על החלקים
        yield return new WaitForSeconds(displayDuration);

        // 4. אנימציית כיווץ (Pop Out)
        // מכבים שוב גרביטציה כדי שלא יזוזו בזמן שהם נעלמים
        foreach (var rb in allRbs)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // מקפיא אותם במקום שלא ייפלו דרך הרצפה בזמן ההקטנה
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3;
            layoutObj.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        // 5. מחיקה
        Destroy(layoutObj);
    }

    IEnumerator RunStepsLogic(PageSequence pageData)
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
                    if (!isUserTouching) timer += Time.deltaTime;
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
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.5f;
            model.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
    }
}
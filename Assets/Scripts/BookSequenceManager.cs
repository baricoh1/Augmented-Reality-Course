using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

public class BookSequenceManager : MonoBehaviour
{
    // --- Data Structures ---
    [System.Serializable]
    public struct Step
    {
        public string stepName;
        public GameObject sceneObject;

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

    [Header("Test Drive")]
    public Racecar racecar;   // רפרנס אמיתי לסקריפט

    [Header("Controls UI")]
    public GameObject arrowRightUI;
    public GameObject arrowLeftUI;


    // --- Settings ---
    [Header("AR Setup")]
    public ARTrackedImageManager imageManager;
    public List<PageSequence> sequences;

    [Header("Phase 1: Scanning Effect")]
    public GameObject scanningVisualPrefab;
    public float scanDuration = 3.0f;

    [Header("Interaction Settings")]
    public Vector3 cageLimits = new Vector3(0.1f, 0.05f, 0.15f);

    [Header("UI")]
    public TMP_Text statusText;
    public TypewriterEffect typewriter;

    // --- Private Fields ---
    private Coroutine currentSequenceRoutine = null;
    private string currentActivePage = "";
    private Dictionary<int, Vector3> _initialScales = new Dictionary<int, Vector3>();
    private GameObject _activeLockedModel = null;
    private Transform _activeAnchor = null;
    private Vector3 _targetLocalPos;
    private bool _nextRequested = false;
    private bool _backRequested = false;

    // לעקוב אחרי אינדקס הדף ברשימת sequences
    private int _currentPageIndex = -1;

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
        // משתמשים ב-FindIndex ושומרים אינדקס
        int pageIndex = sequences.FindIndex(p => p.imageName == imageName);
        if (pageIndex < 0) return;

        PageSequence selectedPage = sequences[pageIndex];

        if (!string.IsNullOrEmpty(selectedPage.imageName))
        {
            if (currentSequenceRoutine != null) StopCoroutine(currentSequenceRoutine);

            _currentPageIndex = pageIndex;
            currentActivePage = imageName;
            _activeAnchor = anchor;
            // 👇 פעם ראשונה לדף הזה – עם סריקה
            currentSequenceRoutine = StartCoroutine(MainFlowRoutine(selectedPage, true));
        }
    }

    // --- Main Flow ---
    IEnumerator MainFlowRoutine(PageSequence pageData, bool doScan)
    {
        if (doScan)
            yield return StartCoroutine(RunScanningEffect());

        if (pageData.partsLayoutPrefab != null)
            yield return StartCoroutine(RunLayoutSimple(pageData.partsLayoutPrefab));

        yield return StartCoroutine(RunStepsLogic(pageData));
    }

    IEnumerator RunScanningEffect()
    {

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "סורק";
        }

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

                if (statusText != null)
                {
                    int dotCount = (int)(timer * 3f) % 4; // 0..3
                    string dots = new string('.', dotCount);
                    if (typewriter != null) typewriter.WriteText("");
                    statusText.text = "סורק" + dots;
                }

                yield return null;
            }
            activeScanEffect.transform.localScale = targetScale;
        }

        yield return new WaitForSeconds(0.2f);
        if (activeScanEffect != null) Destroy(activeScanEffect);

        if (statusText != null)
        {
            statusText.text = "";
        }

    }

    // Layout spawn + grow/fall effect
    IEnumerator RunLayoutSimple(GameObject layoutPrefab)
    {
        // כותרת הדף לפי אינדקס נוכחי
        if (typewriter != null)
        {
            int pageNumber = (_currentPageIndex >= 0 ? _currentPageIndex + 1 : 1);
            typewriter.WriteText("חלקים נדרשים עבור דף " + pageNumber);
        }

        GameObject layoutObj = Instantiate(layoutPrefab, _activeAnchor);

        layoutObj.SetActive(true);
        layoutObj.transform.localPosition = Vector3.zero;
        layoutObj.transform.localRotation = Quaternion.identity;
        layoutObj.transform.localScale = Vector3.zero;

        Rigidbody[] allRbs = layoutObj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRbs)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }

        foreach (Transform child in layoutObj.transform)
        {
            child.gameObject.SetActive(true);
            if (child.localScale == Vector3.zero) child.localScale = Vector3.one;
        }

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3;
            layoutObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        layoutObj.transform.localScale = Vector3.one;

        foreach (var rb in allRbs)
        {
            rb.useGravity = true;
            rb.WakeUp();
        }

        _nextRequested = false;
        while (!_nextRequested)
            yield return null;

        foreach (var rb in allRbs)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3;
            layoutObj.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        Destroy(layoutObj);
    }

    IEnumerator RunStepsLogic(PageSequence pageData)
    {
        int index = 0;

        while (index < pageData.steps.Count)
        {
            if (typewriter != null)
            {
                typewriter.WriteText("שלב " + (index + 1));
            }

            Step currentStep = pageData.steps[index];
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

                _nextRequested = false;
                _backRequested = false;

                while (!_nextRequested && !_backRequested)
                    yield return null;

                if (_backRequested)
                {
                    _activeLockedModel = null;
                    model.SetActive(false);
                    index = Mathf.Max(0, index - 1);
                    continue;
                }

                _activeLockedModel = null;

                // האם זה הדף האחרון בחוברת?
                bool isLastPage = (_currentPageIndex == sequences.Count - 1);
                // האם זה השלב האחרון בדף הנוכחי?
                bool isLastStepOnPage = (index == pageData.steps.Count - 1);

                // אם זה *לא* גם הדף האחרון *וגם* השלב האחרון – מעלימים את המודל
                if (!(isLastPage && isLastStepOnPage))
                {
                    yield return StartCoroutine(FadeOutModel(model));
                    model.SetActive(false);
                }

                index++;

            }
            else
            {
                index++;
            }
        }

        // --- סוף דף: בדיקה אם יש דף הבא ---
        if (typewriter != null)
            typewriter.WriteText("סיימת את דף " + (_currentPageIndex + 1) + "\nלחץ המשך לדף הבא");

        if (_currentPageIndex >= 0 && _currentPageIndex < sequences.Count - 1)
        {
            _nextRequested = false;

            // מחכים שהמשתמש ילחץ על כפתור 'המשך'
            while (!_nextRequested)
                yield return null;

            // מעבר לדף הבא
            _currentPageIndex++;
            PageSequence nextPage = sequences[_currentPageIndex];

            currentActivePage = nextPage.imageName;
            currentSequenceRoutine = StartCoroutine(MainFlowRoutine(nextPage, false));

            yield break;
        }

        else
        {
            // אין דף נוסף – סיום מוחלט
            currentActivePage = "";

            if (typewriter != null)
                typewriter.WriteText("סיימת! , לחץ המשך לנסיעת מבחן");


            // מחכים ללחיצה על כפתור 'המשך'
            _nextRequested = false;
            while (!_nextRequested)
                yield return null;

            // --- החלפת UI לשליטה בנהיגה ---
            if (arrowLeftUI != null) arrowLeftUI.SetActive(false);
            if (arrowRightUI != null) arrowRightUI.SetActive(false);

            // --- הפעלת נסיעת המבחן ---
            if (racecar != null)
                racecar.StartTestDrive();

        }


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

    public void OnNextButton() => _nextRequested = true;
    public void OnBackButton() => _backRequested = true;
}

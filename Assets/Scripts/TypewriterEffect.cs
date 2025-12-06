using System.Collections;
using UnityEngine;
using TMPro;
using System; // הוספנו את זה בשביל היפוך הטקסט

public class TypewriterEffect : MonoBehaviour
{
    [Header("Settings")]
    public float typingSpeed = 0.05f;

    [Header("UI Reference")]
    // גרור לכאן את אובייקט הטקסט שלך (StepTextLabel)
    public TextMeshProUGUI targetTextLabel;

    // --- הוספנו את ה-Start הזה ---
    void Start()
    {
        // כותב את הודעת הפתיחה כשהמשחק מתחיל
        string welcomeMsg = "אנא סרוק את דף ההוראות";
        WriteText(welcomeMsg);
    }

    // --- הפונקציה לכתיבת טקסט ---
    public void WriteText(string content)
    {
        if (targetTextLabel != null)
        {
            StopAllCoroutines(); // עוצר כתיבה קודמת
            StartCoroutine(TypewriterRoutine(content));
        }
        else
        {
            Debug.LogError("שכחת לגרור את הטקסט לשדה Target Text Label בסקריפט!");
        }
    }

    IEnumerator TypewriterRoutine(string textToType)
    {
        targetTextLabel.text = ""; // איפוס

        foreach (char letter in textToType.ToCharArray())
        {
            targetTextLabel.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
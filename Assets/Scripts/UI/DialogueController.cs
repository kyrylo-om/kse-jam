using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI typesetterText;
    [SerializeField] private RectTransform lettersContainer;
    [SerializeField] private GameObject continuePrompt;

    [Header("Prefabs")]
    [SerializeField] private GameObject letterPrefab;

    [Header("Settings")]
    [SerializeField] private float letterDelay = 0.05f;
    [SerializeField] private string appearAnimationTrigger = "Appear";

    private List<GameObject> activeLetters = new List<GameObject>();
    private Coroutine typingCoroutine;
    private string currentDisplayedText = "";

    private void Start()
    {
        // Hide typesetter text visually (alpha = 0) so only our animated letters are seen
        if (typesetterText != null)
        {
            typesetterText.color = new Color(0, 0, 0, 0);
        }
        
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        ClearLetters();
    }

    /// <summary>
    /// Displays the given text inside the dialogue box.
    /// The letters will type out with a delay and stay until the next call.
    /// </summary>
    public void ShowText(string text, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentDisplayedText = text;
        typingCoroutine = StartCoroutine(TypeText(text, onComplete));
    }

    /// <summary>
    /// Appends text to the currently displayed text without clearing existing letters.
    /// The new letters will type out after the existing ones.
    /// </summary>
    public void AppendText(string extraText, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentDisplayedText += extraText;
        typingCoroutine = StartCoroutine(TypeTextFromIndex(currentDisplayedText, currentDisplayedText.Length - extraText.Length, onComplete));
    }

    private IEnumerator TypeText(string text, System.Action onComplete)
    {
        // Clear active letters
        ClearLetters();

        yield return TypeLettersFrom(text, 0, onComplete);
    }

    /// <summary>
    /// Types out only the new portion of text starting from startIndex.
    /// Assumes activeLetters already contains letters for indices [0, startIndex).
    /// </summary>
    private IEnumerator TypeTextFromIndex(string fullText, int startIndex, System.Action onComplete)
    {
        yield return TypeLettersFrom(fullText, startIndex, onComplete);
    }

    private IEnumerator TypeLettersFrom(string text, int startIndex, System.Action onComplete)
    {
        if (typesetterText != null && letterPrefab != null)
        {
            // Update typesetter text and compute its mesh layout
            typesetterText.text = text;
            typesetterText.ForceMeshUpdate();

            TMP_TextInfo textInfo = typesetterText.textInfo;

            for (int i = startIndex; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // Only animate visible characters (skip spaces, newlines, etc.)
                if (charInfo.isVisible)
                {
                    // Calculate position relative to container
                    Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) * 0.5f;

                    // Instantiate letter prefab
                    GameObject letterGo = Instantiate(letterPrefab, lettersContainer);
                    letterGo.transform.SetParent(lettersContainer, false);
                    letterGo.transform.localPosition = charCenter;
                    letterGo.transform.localRotation = Quaternion.identity;
                    letterGo.transform.localScale = Vector3.one;

                    DialogueLetter letterScript = letterGo.GetComponent<DialogueLetter>();
                    if (letterScript != null)
                    {
                        letterScript.Initialize(
                            charInfo.character, 
                            typesetterText.font, 
                            typesetterText.fontSize, 
                            Color.white
                        );
                        letterScript.PlayAnimation(appearAnimationTrigger);
                    }

                    activeLetters.Add(letterGo);

                    // Delay between each letter
                    yield return new WaitForSeconds(letterDelay);
                }
            }
        }

        // Invoke callback when typesetting completes
        onComplete?.Invoke();
    }

    private void ClearLetters()
    {
        foreach (var letter in activeLetters)
        {
            if (letter != null)
            {
                Destroy(letter);
            }
        }
        activeLetters.Clear();
    }
}




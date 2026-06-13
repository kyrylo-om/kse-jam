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

        typingCoroutine = StartCoroutine(TypeText(text, onComplete));
    }

    private IEnumerator TypeText(string text, System.Action onComplete)
    {
        // Clear active letters
        ClearLetters();

        if (typesetterText != null && letterPrefab != null)
        {
            // Update typesetter text and compute its mesh layout
            typesetterText.text = text;
            typesetterText.ForceMeshUpdate();

            TMP_TextInfo textInfo = typesetterText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
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




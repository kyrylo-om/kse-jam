using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private SimonRuleManager ruleManager;
    [SerializeField] private SimonActionValidator actionValidator;
    [SerializeField] private HampterAnimator hampterAnimator;

    [Header("Settings")]
    [SerializeField] private float displayInterval = 10f;
    [SerializeField] private bool pickRandomLine = false;

    [Header("Simon Says...")]
    [SerializeField] private string simonSaysPrefix = "Simon says... ";
    [SerializeField] private float delayAfterSimonSays = 0.5f;

    [Header("Simon Lines")]
    [SerializeField] private List<SimonLine> lines = new List<SimonLine>();

    private int currentLineIndex = 0;
    private Coroutine loopCoroutine;

    private void Start()
    {
        if (dialogueController == null)
            dialogueController = FindAnyObjectByType<DialogueController>();

        if (ruleManager == null)
            ruleManager = FindAnyObjectByType<SimonRuleManager>();

        if (actionValidator == null)
            actionValidator = GetComponent<SimonActionValidator>();

        if (actionValidator == null)
            actionValidator = gameObject.AddComponent<SimonActionValidator>();

        if (hampterAnimator == null)
            hampterAnimator = FindAnyObjectByType<HampterAnimator>();

        loopCoroutine = StartCoroutine(SimonLoop());
    }

    private IEnumerator SimonLoop()
    {
        while (true)
        {
            if (lines.Count > 0 && dialogueController != null)
            {
                // Determine line index — skip rule lines whose rule type is already active
                SimonLine currentLine;
                if (pickRandomLine)
                {
                    // Build a list of eligible lines (skip active rules)
                    List<SimonLine> eligible = new List<SimonLine>();
                    foreach (var line in lines)
                    {
                        if (line.type == SimonLineType.Rule && ruleManager != null && ruleManager.IsRuleActive(line.rule.type))
                            continue;
                        eligible.Add(line);
                    }

                    if (eligible.Count == 0)
                    {
                        // Fallback: nothing eligible, just pick from all lines
                        currentLine = lines[Random.Range(0, lines.Count)];
                    }
                    else
                    {
                        currentLine = eligible[Random.Range(0, eligible.Count)];
                    }
                }
                else
                {
                    // Sequential mode — skip lines whose rule type is already active
                    int attempts = 0;
                    do
                    {
                        currentLineIndex = currentLineIndex % lines.Count;
                        currentLine = lines[currentLineIndex];
                        bool isActiveRule = currentLine.type == SimonLineType.Rule
                                            && ruleManager != null
                                            && ruleManager.IsRuleActive(currentLine.rule.type);
                        if (!isActiveRule) break;
                        currentLineIndex++;
                        attempts++;
                    } while (attempts < lines.Count);
                }

                // Phase 1: Type "Simon says... "
                hampterAnimator?.StartTalk();
                bool simonSaysDone = false;
                dialogueController.ShowText(simonSaysPrefix, () => simonSaysDone = true);
                yield return new WaitUntil(() => simonSaysDone);
                hampterAnimator?.StartIdle();

                yield return new WaitForSeconds(delayAfterSimonSays);

                // Phase 2: Append the line text (letters stay on screen)
                hampterAnimator?.StartTalk();
                bool lineDone = false;
                dialogueController.AppendText(currentLine.text, () => lineDone = true);
                yield return new WaitUntil(() => lineDone);

                // Wait for the configured delay before executing
                yield return new WaitForSeconds(currentLine.delayBeforeAction);

                // Route based on line type
                switch (currentLine.type)
                {
                    case SimonLineType.Action:
                        yield return HandleActionLine(currentLine);
                        break;

                    case SimonLineType.Rule:
                        HandleRuleLine(currentLine);
                        break;

                    case SimonLineType.Event:
                        HandleEventLine(currentLine);
                        break;
                }

                hampterAnimator?.StartIdle();

                if (!pickRandomLine)
                {
                    currentLineIndex++;
                }
            }

            yield return new WaitForSeconds(displayInterval);
        }
    }

    private IEnumerator HandleActionLine(SimonLine line)
    {
        bool succeeded = false;
        bool failed = false;

        actionValidator.BeginValidation(
            line.requiredInputAction,
            line.actionTimeLimit,
            () => succeeded = true,
            () => failed = true
        );

        // Wait for either success or failure
        yield return new WaitUntil(() => succeeded || failed);

        if (succeeded)
        {
            Debug.Log($"[SimonController] Action succeeded: {line.requiredInputAction}");
            // Optionally invoke the event on success too
            line.onTriggered?.Invoke();
        }
        else
        {
            Debug.Log($"[SimonController] Action FAILED: {line.failureMessage}");
            // Show failure message
            if (!string.IsNullOrEmpty(line.failureMessage))
            {
                dialogueController.ShowText(line.failureMessage, null);
                yield return new WaitForSeconds(1.5f);
            }
            // Placeholder death: reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void HandleRuleLine(SimonLine line)
    {
        if (ruleManager == null)
        {
            Debug.LogError("[SimonController] SimonRuleManager not assigned or found!");
            return;
        }

        ruleManager.ApplyRule(line.rule);
        line.onTriggered?.Invoke();
    }

    private void HandleEventLine(SimonLine line)
    {
        line.onTriggered?.Invoke();
    }

    #region Predefined Quirky Functions

    /// <summary>
    /// Finds the player and triggers a jump.
    /// </summary>
    public void JumpPlayer()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.ForceJump();
                Debug.Log("[SimonController] Triggered JumpPlayer!");
            }
        }
    }

    /// <summary>
    /// Finds the player and triggers a tumble.
    /// </summary>
    public void TumblePlayer()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.TriggerTumble();
                Debug.Log("[SimonController] Triggered TumblePlayer!");
            }
        }
    }

    /// <summary>
    /// Finds Hampter in the scene and rotates it 360 degrees.
    /// </summary>
    public void HampterSpin()
    {
        GameObject hampter = GameObject.Find("Canvas/Hampter");
        if (hampter == null) hampter = GameObject.Find("Hampter");

        if (hampter != null)
        {
            StartCoroutine(RotateOverTime(hampter.transform, 1.0f));
            Debug.Log("[SimonController] Triggered HampterSpin!");
        }
    }

    private IEnumerator RotateOverTime(Transform t, float duration)
    {
        float elapsed = 0f;
        Quaternion startRot = t.localRotation;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(0f, 360f, elapsed / duration);
            t.localRotation = startRot * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        t.localRotation = startRot;
    }

    /// <summary>
    /// Shakes the entire dialogue box.
    /// </summary>
    public void ShakeDialogueBox()
    {
        if (dialogueController != null)
        {
            StartCoroutine(ShakeTransform(dialogueController.transform, 0.5f, 15f));
            Debug.Log("[SimonController] Triggered ShakeDialogueBox!");
        }
    }

    private IEnumerator ShakeTransform(Transform t, float duration, float magnitude)
    {
        Vector3 originalPos = t.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            t.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }
        t.localPosition = originalPos;
    }

    /// <summary>
    /// Flashes the dialogue box color temporarily.
    /// </summary>
    public void FlashDialogueColor()
    {
        if (dialogueController != null)
        {
            UnityEngine.UI.Image img = dialogueController.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                StartCoroutine(FlashColor(img, new Color(0.8f, 0.2f, 0.2f, 0.95f), 0.5f));
                Debug.Log("[SimonController] Triggered FlashDialogueColor!");
            }
        }
    }

    private IEnumerator FlashColor(UnityEngine.UI.Image img, Color flashColor, float duration)
    {
        Color originalColor = img.color;
        img.color = flashColor;
        yield return new WaitForSeconds(duration);
        img.color = originalColor;
    }

    #endregion
}

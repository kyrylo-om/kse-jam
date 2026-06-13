using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimonActionValidator : MonoBehaviour
{
    private InputAction trackedAction;
    private Coroutine timeoutCoroutine;
    private Action onSuccess;
    private Action onFailure;
    private bool isWaiting;

    /// <summary>
    /// Begins listening for a specific input action with a time limit.
    /// </summary>
    public void BeginValidation(string actionName, float timeLimit, Action success, Action fail)
    {
        CancelValidation();

        onSuccess = success;
        onFailure = fail;
        isWaiting = true;

        // Look up the input action
        if (InputSystem.actions == null)
        {
            Debug.LogError("[SimonActionValidator] No InputSystem actions asset found!");
            Fail();
            return;
        }

        trackedAction = InputSystem.actions.FindAction(actionName);
        if (trackedAction == null)
        {
            Debug.LogError($"[SimonActionValidator] Action '{actionName}' not found in InputSystem actions!");
            Fail();
            return;
        }

        trackedAction.performed += OnActionPerformed;
        trackedAction.Enable();

        timeoutCoroutine = StartCoroutine(TimeoutRoutine(timeLimit));
        Debug.Log($"[SimonActionValidator] Listening for '{actionName}' — {timeLimit}s timeout");
    }

    public void CancelValidation()
    {
        if (!isWaiting) return;

        CleanUp();
        isWaiting = false;
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        if (!isWaiting) return;

        Debug.Log($"[SimonActionValidator] Action '{trackedAction?.name}' performed — success!");
        CleanUp();
        isWaiting = false;
        onSuccess?.Invoke();
    }

    private IEnumerator TimeoutRoutine(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);

        if (!isWaiting) yield break;

        Debug.Log($"[SimonActionValidator] Timeout ({timeLimit}s) reached — failure!");
        Fail();
    }

    private void Fail()
    {
        CleanUp();
        isWaiting = false;
        onFailure?.Invoke();
    }

    private void CleanUp()
    {
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        if (trackedAction != null)
        {
            trackedAction.performed -= OnActionPerformed;
            trackedAction = null;
        }

        onSuccess = null;
        onFailure = null;
    }

    private void OnDestroy()
    {
        CancelValidation();
    }
}
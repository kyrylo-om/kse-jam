using UnityEngine;
using TMPro;

public class BarrierTrigger : MonoBehaviour
{
    [SerializeField] private bool destroyOnTouch = true;

    private TextMeshProUGUI effectsText;

    private void Awake()
    {
        // Find EffectsCleared text — works even if it starts disabled
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var t in allTexts)
        {
            if (t.gameObject.name == "EffectsCleared")
            {
                effectsText = t;
                break;
            }
        }

        if (effectsText == null)
        {
            Debug.LogWarning("[BarrierTrigger] EffectsCleared text not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player (checking tag and/or CharacterController component)
        if (other.CompareTag("Player") || 
            other.GetComponent<CharacterController>() != null || 
            other.GetComponentInParent<CharacterController>() != null)
        {
            ClearAllEffects();
            Disappear();
        }
    }

    private void ClearAllEffects()
    {
        // Reset all active rules on the Simon Rule Manager
        if (SimonRuleManager.Instance != null)
        {
            SimonRuleManager.Instance.ResetAllRules();
            Debug.Log("[BarrierTrigger] Reset all active Simon rules.");
        }
        else
        {
            Debug.LogWarning("[BarrierTrigger] SimonRuleManager.Instance not found.");
        }

        // Show effects text for 3 seconds (coroutine runs on the text itself, survives this object's destruction)
        if (effectsText != null)
        {
            effectsText.gameObject.SetActive(true);
            effectsText.StartCoroutine(HideTextAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effectsText != null)
        {
            effectsText.gameObject.SetActive(false);
        }
    }

    private void Disappear()
    {
        if (destroyOnTouch)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using TMPro;

public class DialogueLetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Animator animator;

    // A fixed palette of basic colors to choose from for letter outlines
    private static readonly Color[] BasicColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        new Color(1f, 0.5f, 0f), // orange
    };

    public void Initialize(char character, TMP_FontAsset font, float fontSize, Color textColor)
    {
        if (textComponent != null)
        {
            textComponent.text = character.ToString();
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;

            // Add a randomly chosen outline from a fixed palette
            textComponent.outlineWidth = 0.2f; 
            textComponent.color = BasicColors[Random.Range(0, BasicColors.Length)];
            textComponent.outlineColor = Color.white;
            
            // Ensure the shader keyword is enabled on the material instance
            // Using fontMaterial creates a unique instance for this specific letter
            if (textComponent.fontMaterial != null)
            {
                textComponent.fontMaterial.EnableKeyword("OUTLINE_ON");
            }
        }
    }

    public void PlayAnimation(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            // Reset trigger first to prevent double-firing or issues
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }
    }
}

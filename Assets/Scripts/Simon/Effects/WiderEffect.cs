using UnityEngine;

public class WiderEffect : ISimonEffect
{
    private readonly float widthMultiplier;
    private CharacterController playerController;
    private bool applied;

    public WiderEffect(float widthMultiplier)
    {
        this.widthMultiplier = Mathf.Max(1f, widthMultiplier);
    }

    public void Apply()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player == null)
        {
            Debug.LogWarning("[WiderEffect] PlayerBean not found!");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        if (playerController == null) return;

        // Multiply the X and Z by the width multiplier
        Vector3 current = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            current.x * widthMultiplier,
            current.y,
            current.z * widthMultiplier
        );
        applied = true;
        Debug.Log($"[WiderEffect] Player widened by {widthMultiplier}x");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        // Undo only our multiplier
        Vector3 current = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            current.x / widthMultiplier,
            current.y,
            current.z / widthMultiplier
        );
        applied = false;
        Debug.Log("[WiderEffect] Player width restored.");
    }
}
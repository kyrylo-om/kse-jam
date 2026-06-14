using UnityEngine;

public class WiderEffect : ISimonEffect
{
    private readonly float widthMultiplier;
    private CharacterController playerController;
    private Vector3 originalMultiplier;
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

        originalMultiplier = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            originalMultiplier.x * widthMultiplier,
            originalMultiplier.y,
            originalMultiplier.z * widthMultiplier
        );
        applied = true;
        Debug.Log($"[WiderEffect] Player widened by {widthMultiplier}x");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        playerController.baseScaleMultiplier = originalMultiplier;
        applied = false;
        Debug.Log("[WiderEffect] Player scale restored.");
    }
}
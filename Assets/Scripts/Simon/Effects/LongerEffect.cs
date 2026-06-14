using UnityEngine;

public class LongerEffect : ISimonEffect
{
    private readonly float lengthMultiplier;
    private CharacterController playerController;
    private Vector3 originalMultiplier;
    private bool applied;

    public LongerEffect(float lengthMultiplier)
    {
        this.lengthMultiplier = Mathf.Max(1f, lengthMultiplier);
    }

    public void Apply()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player == null)
        {
            Debug.LogWarning("[LongerEffect] PlayerBean not found!");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        if (playerController == null) return;

        originalMultiplier = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            originalMultiplier.x,
            originalMultiplier.y,
            originalMultiplier.z * lengthMultiplier
        );
        applied = true;
        Debug.Log($"[LongerEffect] Player lengthened by {lengthMultiplier}x");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        playerController.baseScaleMultiplier = originalMultiplier;
        applied = false;
        Debug.Log("[LongerEffect] Player scale restored.");
    }
}
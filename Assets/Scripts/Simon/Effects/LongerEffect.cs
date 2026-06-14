using UnityEngine;

public class LongerEffect : ISimonEffect
{
    private readonly float lengthMultiplier;
    private CharacterController playerController;
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

        // Multiply the Z (forward) by the length multiplier
        Vector3 current = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            current.x,
            current.y,
            current.z * lengthMultiplier
        );
        applied = true;
        Debug.Log($"[LongerEffect] Player lengthened by {lengthMultiplier}x");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        // Undo only our multiplier
        Vector3 current = playerController.baseScaleMultiplier;
        playerController.baseScaleMultiplier = new Vector3(
            current.x,
            current.y,
            current.z / lengthMultiplier
        );
        applied = false;
        Debug.Log("[LongerEffect] Player length restored.");
    }
}
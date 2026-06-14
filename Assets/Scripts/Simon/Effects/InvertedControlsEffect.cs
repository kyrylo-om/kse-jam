using UnityEngine;

public class InvertedControlsEffect : ISimonEffect
{
    private CharacterController playerController;
    private Vector2 originalMultiplier;
    private bool applied;

    public InvertedControlsEffect(float _)
    {
        // floatValue is unused
    }

    public void Apply()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player == null)
        {
            Debug.LogWarning("[InvertedControlsEffect] PlayerBean not found!");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        if (playerController == null) return;

        originalMultiplier = playerController.moveInputMultiplier;
        playerController.moveInputMultiplier = new Vector2(-1f, -1f);
        applied = true;
        Debug.Log("[InvertedControlsEffect] Controls inverted!");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        playerController.moveInputMultiplier = originalMultiplier;
        applied = false;
        Debug.Log("[InvertedControlsEffect] Controls restored.");
    }
}
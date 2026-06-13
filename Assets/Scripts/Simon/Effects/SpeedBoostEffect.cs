using UnityEngine;

public class SpeedBoostEffect : ISimonEffect
{
    private readonly float multiplier;
    private float originalMoveSpeed;
    private CharacterController playerController;
    private bool applied;

    public SpeedBoostEffect(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public void Apply()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player == null)
        {
            Debug.LogWarning("[SpeedBoostEffect] PlayerBean not found!");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        if (playerController == null) return;

        originalMoveSpeed = playerController.moveSpeed;
        playerController.moveSpeed *= multiplier;
        applied = true;
        Debug.Log($"[SpeedBoostEffect] Move speed boosted from {originalMoveSpeed} to {playerController.moveSpeed}");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        playerController.moveSpeed = originalMoveSpeed;
        applied = false;
        Debug.Log($"[SpeedBoostEffect] Move speed reset to {originalMoveSpeed}");
    }
}
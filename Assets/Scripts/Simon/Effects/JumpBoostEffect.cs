using UnityEngine;

public class JumpBoostEffect : ISimonEffect
{
    private readonly float multiplier;
    private float originalJumpForce;
    private CharacterController playerController;
    private bool applied;

    public JumpBoostEffect(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public void Apply()
    {
        GameObject player = GameObject.Find("PlayerBean");
        if (player == null)
        {
            Debug.LogWarning("[JumpBoostEffect] PlayerBean not found!");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        if (playerController == null) return;

        originalJumpForce = playerController.jumpForce;
        playerController.jumpForce *= multiplier;
        applied = true;
        Debug.Log($"[JumpBoostEffect] Jump force boosted from {originalJumpForce} to {playerController.jumpForce}");
    }

    public void Reset()
    {
        if (!applied || playerController == null) return;

        playerController.jumpForce = originalJumpForce;
        applied = false;
        Debug.Log($"[JumpBoostEffect] Jump force reset to {originalJumpForce}");
    }
}
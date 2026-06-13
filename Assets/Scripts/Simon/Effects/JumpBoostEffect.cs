using UnityEngine;

public class JumpBoostEffect : ISimonEffect
{
    private readonly float multiplier;
    private GameObject playerObject;
    private CharacterController playerController;
    private Rigidbody playerRigidbody;
    private bool applied;

    public JumpBoostEffect(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public void Apply()
    {
        playerObject = GameObject.Find("PlayerBean");
        if (playerObject == null)
        {
            Debug.LogWarning("[JumpBoostEffect] PlayerBean not found!");
            return;
        }

        playerRigidbody = playerObject.GetComponent<Rigidbody>();
        if (playerRigidbody == null) return;

        playerController = playerObject.GetComponent<CharacterController>();
        if (playerController == null) return;

        if (SimonRuleManager.Instance == null)
        {
            Debug.LogWarning("[JumpBoostEffect] SimonRuleManager.Instance not found!");
            return;
        }

        applied = true;
        SimonRuleManager.Instance.StartCoroutine(AutoJumpRoutine());
        Debug.Log($"[JumpBoostEffect] Auto-jump started!");
    }

    private System.Collections.IEnumerator AutoJumpRoutine()
    {
        float jumpStrength = 12f;

        while (applied && playerObject != null)
        {
            // Wait until the player touches the ground
            yield return new WaitUntil(() => playerController != null && playerController.isGrounded);

            if (!applied || playerObject == null) break;

            // Jump instantly
            playerRigidbody.linearVelocity = new Vector3(
                playerRigidbody.linearVelocity.x,
                jumpStrength,
                playerRigidbody.linearVelocity.z
            );

            yield return null;
        }
    }

    public void Reset()
    {
        if (!applied) return;
        applied = false;
        Debug.Log("[JumpBoostEffect] Auto-jump stopped.");
    }
}
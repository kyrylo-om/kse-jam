using UnityEngine;

public class GravityChangeEffect : ISimonEffect
{
    private readonly float multiplier;
    private Vector3 originalGravity;
    private bool applied;

    public GravityChangeEffect(float multiplier)
    {
        this.multiplier = multiplier;
    }

    public void Apply()
    {
        originalGravity = Physics.gravity;
        Physics.gravity = originalGravity * multiplier;
        applied = true;
        Debug.Log($"[GravityChangeEffect] Gravity changed from {originalGravity} to {Physics.gravity}");
    }

    public void Reset()
    {
        if (!applied) return;

        Physics.gravity = originalGravity;
        applied = false;
        Debug.Log($"[GravityChangeEffect] Gravity reset to {originalGravity}");
    }
}
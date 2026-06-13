using UnityEngine;

public class FPSLimitEffect : ISimonEffect
{
    private readonly int targetFPS;
    private int originalTargetFPS;
    private bool applied;

    public FPSLimitEffect(float targetFPS)
    {
        this.targetFPS = Mathf.Max(1, Mathf.RoundToInt(targetFPS));
    }

    public void Apply()
    {
        originalTargetFPS = Application.targetFrameRate;
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0;
        applied = true;
        Debug.Log($"[FPSLimitEffect] FPS limited to {targetFPS}");
    }

    public void Reset()
    {
        if (!applied) return;

        Application.targetFrameRate = originalTargetFPS;
        applied = false;
        Debug.Log($"[FPSLimitEffect] FPS restored to {originalTargetFPS}");
    }
}
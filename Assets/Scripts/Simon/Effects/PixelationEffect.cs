using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationEffect : ISimonEffect
{
    private readonly float resolutionScale;
    private UniversalRenderPipelineAsset urpAsset;
    private float originalRenderScale;
    private UpscalingFilterSelection originalUpscaleFilter;
    private bool applied;

    public PixelationEffect(float resolutionScale)
    {
        this.resolutionScale = Mathf.Clamp(resolutionScale, 0.05f, 0.5f);
    }

    public void Apply()
    {
        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogWarning("[PixelationEffect] No URP pipeline asset found!");
            return;
        }

        originalRenderScale = urpAsset.renderScale;
        originalUpscaleFilter = urpAsset.upscalingFilter;

        urpAsset.renderScale = resolutionScale;
        urpAsset.upscalingFilter = UpscalingFilterSelection.Point;

        applied = true;
        Debug.Log($"[PixelationEffect] Render scale set to {resolutionScale}");
    }

    public void Reset()
    {
        if (!applied) return;

        if (urpAsset != null)
        {
            urpAsset.renderScale = originalRenderScale;
            urpAsset.upscalingFilter = originalUpscaleFilter;
        }

        applied = false;
        Debug.Log("[PixelationEffect] Render scale restored.");
    }
}
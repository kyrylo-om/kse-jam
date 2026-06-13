using UnityEngine;
using UnityEngine.UI;

public class PixelationEffect : ISimonEffect
{
    private readonly float resolutionScale;
    private Camera mainCamera;
    private RenderTexture lowResRT;
    private GameObject overlayObject;
    private bool applied;

    public PixelationEffect(float resolutionScale)
    {
        this.resolutionScale = Mathf.Clamp(resolutionScale, 0.05f, 0.5f);
    }

    public void Apply()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[PixelationEffect] No main camera found!");
            return;
        }

        // Create low-res render texture with point (nearest neighbor) filtering
        int w = Mathf.Max(32, Mathf.RoundToInt(Screen.width * resolutionScale));
        int h = Mathf.Max(24, Mathf.RoundToInt(Screen.height * resolutionScale));

        lowResRT = new RenderTexture(w, h, 24, RenderTextureFormat.Default);
        lowResRT.filterMode = FilterMode.Point;
        lowResRT.Create();

        // Point the camera at the low-res RT instead of the screen
        mainCamera.targetTexture = lowResRT;

        // Create a full-screen UI overlay to display the pixelated result
        overlayObject = new GameObject("PixelationOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = overlayObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        GameObject imageObj = new GameObject("PixelatedImage", typeof(RawImage));
        imageObj.transform.SetParent(overlayObject.transform, false);

        RawImage rawImage = imageObj.GetComponent<RawImage>();
        rawImage.texture = lowResRT;
        rawImage.raycastTarget = false;

        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        applied = true;
        Debug.Log($"[PixelationEffect] Pixelation active: {w}x{h} (scale: {resolutionScale})");
    }

    public void Reset()
    {
        if (!applied) return;

        CleanUp();
        applied = false;
        Debug.Log("[PixelationEffect] Pixelation removed.");
    }

    private void CleanUp()
    {
        // Restore camera rendering to screen
        if (mainCamera != null)
        {
            mainCamera.targetTexture = null;
        }

        // Destroy the overlay canvas
        if (overlayObject != null)
        {
            Object.Destroy(overlayObject);
            overlayObject = null;
        }

        // Release and destroy the RT
        if (lowResRT != null)
        {
            lowResRT.Release();
            Object.Destroy(lowResRT);
            lowResRT = null;
        }
    }
}
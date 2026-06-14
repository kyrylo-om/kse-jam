using UnityEngine;
using UnityEngine.UI;

public class PsychedelicEffect : ISimonEffect
{
    private GameObject overlayObject;
    private RawImage rawImage;
    private Texture2D rainbowTex;
    private bool applied;

    public PsychedelicEffect(float _)
    {
        // floatValue unused
    }

    public void Apply()
    {
        // Create full-screen overlay
        overlayObject = new GameObject("PsychedelicOverlay", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = overlayObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32766;

        GameObject imageObj = new GameObject("ColorOverlay", typeof(RawImage));
        imageObj.transform.SetParent(overlayObject.transform, false);

        rawImage = imageObj.GetComponent<RawImage>();
        rawImage.raycastTarget = false;

        // Create a 1-pixel-wide rainbow gradient texture
        rainbowTex = new Texture2D(256, 1, TextureFormat.RGB24, false);
        rainbowTex.filterMode = FilterMode.Point;
        rainbowTex.wrapMode = TextureWrapMode.Repeat;
        for (int x = 0; x < 256; x++)
        {
            float t = (float)x / 255f;
            rainbowTex.SetPixel(x, 0, Color.HSVToRGB(t, 1f, 1f));
        }
        rainbowTex.Apply();

        rawImage.texture = rainbowTex;
        rawImage.color = new Color(1f, 1f, 1f, 0.3f);

        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Animate the UV offset on SimonRuleManager's coroutine
        if (SimonRuleManager.Instance != null)
        {
            SimonRuleManager.Instance.StartCoroutine(PsychedelicRoutine());
        }

        applied = true;
        Debug.Log("[PsychedelicEffect] Color filter active!");
    }

    private System.Collections.IEnumerator PsychedelicRoutine()
    {
        float offset = 0f;
        while (applied && rawImage != null)
        {
            offset += Time.deltaTime * 0.5f;
            RawImage ri = rawImage;
            if (ri != null)
            {
                Rect uv = ri.uvRect;
                uv.x = offset;
                ri.uvRect = uv;
            }
            yield return null;
        }
    }

    public void Reset()
    {
        if (!applied) return;

        applied = false;

        if (overlayObject != null)
        {
            Object.Destroy(overlayObject);
            overlayObject = null;
        }

        if (rainbowTex != null)
        {
            Object.Destroy(rainbowTex);
            rainbowTex = null;
        }

        rawImage = null;
        Debug.Log("[PsychedelicEffect] Color filter removed.");
    }
}
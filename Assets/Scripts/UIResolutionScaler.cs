using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class UIResolutionScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    [Tooltip("The resolution your UI is designed for")]
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Scale Mode")]
    [Tooltip("How the UI should scale with different resolutions")]
    public CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    [Header("Match Settings")]
    [Tooltip("0 = match width, 0.5 = match both equally, 1 = match height")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;

    [Header("Screen Match Mode")]
    [Tooltip("Expand = UI expands beyond reference resolution, Shrink = UI shrinks to fit")]
    public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

    [Header("Physical Settings (for Constant Physical Size mode)")]
    public float physicalUnit = 1f;
    public float fallbackScreenDPI = 96f;
    public float defaultSpriteDPI = 96f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private CanvasScaler canvasScaler;
    private Canvas canvas;
    private Vector2 lastScreenSize;

    void Awake()
    {
        SetupCanvasScaler();
    }

    void Start()
    {
        ApplyScalerSettings();
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        if (showDebugInfo)
        {
            Debug.Log($"UIResolutionScaler initialized. Screen: {Screen.width}x{Screen.height}, Reference: {referenceResolution.x}x{referenceResolution.y}");
        }
    }

    void Update()
    {
        // Check if screen resolution changed (useful for windowed mode or resolution changes)
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            lastScreenSize = currentScreenSize;
            ApplyScalerSettings();

            if (showDebugInfo)
            {
                Debug.Log($"Screen resolution changed to: {Screen.width}x{Screen.height}");
            }
        }
    }

    private void SetupCanvasScaler()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        if (canvasScaler == null)
        {
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
        }
    }

    private void ApplyScalerSettings()
    {
        if (canvasScaler == null) return;

        canvasScaler.uiScaleMode = scaleMode;

        switch (scaleMode)
        {
            case CanvasScaler.ScaleMode.ConstantPixelSize:
                canvasScaler.scaleFactor = 1f;
                break;

            case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                canvasScaler.referenceResolution = referenceResolution;
                canvasScaler.screenMatchMode = screenMatchMode;
                canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
                break;

            case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                canvasScaler.physicalUnit = (CanvasScaler.Unit)((int)physicalUnit);
                canvasScaler.fallbackScreenDPI = fallbackScreenDPI;
                canvasScaler.defaultSpriteDPI = defaultSpriteDPI;
                break;
        }

        if (showDebugInfo)
        {
            float scaleRatio = GetCurrentScaleRatio();
            Debug.Log($"Canvas scaled. Scale Mode: {scaleMode}, Current Scale Ratio: {scaleRatio:F3}");
        }
    }

    /// <summary>
    /// Get the current scale ratio being applied to the UI
    /// </summary>
    public float GetCurrentScaleRatio()
    {
        if (canvasScaler == null || scaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            return 1f;

        float widthRatio = Screen.width / referenceResolution.x;
        float heightRatio = Screen.height / referenceResolution.y;

        return Mathf.Lerp(widthRatio, heightRatio, matchWidthOrHeight);
    }

    /// <summary>
    /// Manually force the scaler to update
    /// </summary>
    public void ForceUpdate()
    {
        ApplyScalerSettings();
    }

    // Editor utility to apply settings in edit mode
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyScalerSettings();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class UIResolutionAdapter : MonoBehaviour
{
    private const float ClassicAspectThreshold = 1.5f;      // 4:3, 5:4
    private const float WideAspectThreshold = 1.7f;         // 16:10
    private const float UltrawideAspectThreshold = 2.1f;    // 21:9
    private const float SuperUltrawideAspectThreshold = 3.2f; // 32:9

    private int cachedWidth;
    private int cachedHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var existing = FindFirstObjectByType<UIResolutionAdapter>();
        if (existing != null)
        {
            return;
        }

        var host = new GameObject(nameof(UIResolutionAdapter));
        DontDestroyOnLoad(host);
        host.AddComponent<UIResolutionAdapter>();
    }

    private void Start()
    {
        ApplyProfile();
    }

    private void Update()
    {
        if (cachedWidth == Screen.width && cachedHeight == Screen.height)
        {
            return;
        }

        ApplyProfile();
    }

    private void ApplyProfile()
    {
        cachedWidth = Screen.width;
        cachedHeight = Screen.height;

        var profile = GetDesktopProfile(cachedWidth, cachedHeight);
        var scalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var scaler in scalers)
        {
            if (scaler.TryGetComponent(out Canvas canvas) && canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = profile.referenceResolution;
            scaler.matchWidthOrHeight = profile.matchWidthOrHeight;
        }
    }

    private static (Vector2 referenceResolution, float matchWidthOrHeight) GetDesktopProfile(int width, int height)
    {
        var safeHeight = Mathf.Max(1, height);
        var aspect = (float)width / safeHeight;

        if (aspect >= SuperUltrawideAspectThreshold)
        {
            return (new Vector2(3840f, 1080f), 0.15f);
        }

        if (aspect >= UltrawideAspectThreshold)
        {
            return (new Vector2(2560f, 1080f), 0.25f);
        }

        if (aspect >= WideAspectThreshold)
        {
            return (new Vector2(1920f, 1080f), 0.5f);
        }

        if (aspect >= ClassicAspectThreshold)
        {
            return (new Vector2(1920f, 1200f), 0.6f);
        }

        return (new Vector2(1600f, 1200f), 0.8f);
    }
}

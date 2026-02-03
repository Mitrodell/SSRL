using UnityEngine;
using UnityEngine.UI;

public sealed class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;

    [Header("Layout")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector2 size = new Vector2(1.4f, 0.18f);

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.2f, 0.2f, 1f);

    private static Sprite builtInSprite;

    private Canvas canvas;
    private RectTransform canvasTransform;
    private Image fillImage;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
        CreateBar();
        UpdateFill();
    }

    private void LateUpdate()
    {
        if (enemy == null || canvasTransform == null) return;

        if (Camera.main != null)
        {
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - Camera.main.transform.position);
        }

        UpdateFill();
    }

    private void CreateBar()
    {
        if (canvasTransform != null) return;

        var barRoot = new GameObject("EnemyHealthBar");
        barRoot.transform.SetParent(transform, false);

        canvasTransform = barRoot.AddComponent<RectTransform>();
        canvasTransform.localPosition = worldOffset;
        canvasTransform.localScale = Vector3.one;

        canvas = barRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;

        if (builtInSprite == null)
        {
            builtInSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        var background = new GameObject("Background");
        background.transform.SetParent(canvasTransform, false);

        var backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.sizeDelta = size;

        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = builtInSprite;
        backgroundImage.color = backgroundColor;
        backgroundImage.raycastTarget = false;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(backgroundRect, false);

        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = fill.AddComponent<Image>();
        fillImage.sprite = builtInSprite;
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;
    }

    private void UpdateFill()
    {
        if (fillImage == null || enemy == null) return;

        float ratio = enemy.MaxHp <= 0f ? 0f : Mathf.Clamp01(enemy.CurrentHp / enemy.MaxHp);
        fillImage.fillAmount = ratio;
    }
}

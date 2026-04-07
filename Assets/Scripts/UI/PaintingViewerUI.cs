using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Full-screen painting viewer (shows a sprite + X close button)
public class PaintingViewerUI : MonoBehaviour
{
    public static PaintingViewerUI Instance { get; private set; }

    Canvas _canvas;
    Image _paintingImage;
    Button _closeButton;
    TextMeshProUGUI _titleText;
    System.Action _onClosed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("PaintingViewerUI");
        DontDestroyOnLoad(go);
        go.AddComponent<PaintingViewerUI>().Build();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Build()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 50; // above dialogue + prompt
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        var root = new GameObject("Root");
        root.transform.SetParent(transform, false);
        var rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(root.transform, false);
        var dimRt = dim.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.85f);

        var painting = new GameObject("PaintingImage");
        painting.transform.SetParent(root.transform, false);
        var paintRt = painting.AddComponent<RectTransform>();
        paintRt.anchorMin = new Vector2(0.05f, 0.05f);
        paintRt.anchorMax = new Vector2(0.95f, 0.95f);
        paintRt.offsetMin = Vector2.zero;
        paintRt.offsetMax = Vector2.zero;
        _paintingImage = painting.AddComponent<Image>();
        _paintingImage.preserveAspect = true;
        _paintingImage.color = Color.white;

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(root.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.95f);
        titleRt.anchorMax = new Vector2(0.75f, 1f);
        titleRt.offsetMin = new Vector2(0f, 0f);
        titleRt.offsetMax = new Vector2(0f, 0f);
        _titleText = titleGo.AddComponent<TextMeshProUGUI>();
        _titleText.fontSize = 28;
        _titleText.color = new Color(0.85f, 0.85f, 0.85f);
        _titleText.alignment = TextAlignmentOptions.TopLeft;
        _titleText.text = "";

        var closeGo = new GameObject("CloseButton");
        closeGo.transform.SetParent(root.transform, false);
        var closeRt = closeGo.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.94f, 0.93f);
        closeRt.anchorMax = new Vector2(0.99f, 0.99f);
        closeRt.offsetMin = Vector2.zero;
        closeRt.offsetMax = Vector2.zero;

        var closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0f, 0f, 0f, 0.55f);

        _closeButton = closeGo.AddComponent<Button>();
        _closeButton.onClick.AddListener(Hide);

        var closeTextGo = new GameObject("X");
        closeTextGo.transform.SetParent(closeGo.transform, false);
        var closeTextRt = closeTextGo.AddComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;
        var closeTmp = closeTextGo.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "X";
        closeTmp.fontSize = 30;
        closeTmp.color = new Color(0.9f, 0.9f, 0.9f);
        closeTmp.alignment = TextAlignmentOptions.Center;

        _canvas.enabled = false;
        root.SetActive(false);
    }

    public void Show(Sprite paintingSprite, string title = "", System.Action onClosed = null)
    {
        _onClosed = onClosed;
        _titleText.text = string.IsNullOrWhiteSpace(title) ? "" : title;
        _paintingImage.sprite = paintingSprite;
        _canvas.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);

        // Hide interaction prompt while viewing.
        InteractionPromptUI.Instance?.Hide();
    }

    public void Hide()
    {
        if (_canvas == null) return;
        _canvas.enabled = false;
        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);

        var cb = _onClosed;
        _onClosed = null;
        cb?.Invoke();
    }
}


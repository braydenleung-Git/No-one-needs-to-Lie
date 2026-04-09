using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Full-screen 4-letter code entry UI (matches retro green terminal look)
public class FourLetterCodeUI : MonoBehaviour
{
    public static FourLetterCodeUI Instance { get; private set; }

    Canvas _canvas;
    GameObject _root;
    TextMeshProUGUI _prompt;
    TextMeshProUGUI _caret;
    TextMeshProUGUI[] _slots;
    Button _closeButton;

    bool _active;
    int _maxLen = 4;
    string _buffer = "";
    string _correct = "JOHN";
    System.Action<bool> _onSubmitted; // true = correct

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("FourLetterCodeUI");
        DontDestroyOnLoad(go);
        go.AddComponent<FourLetterCodeUI>().Build();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!_active) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            var cb = _onSubmitted;
            Hide();
            cb?.Invoke(false);
            return;
        }

        if (kb.backspaceKey.wasPressedThisFrame && _buffer.Length > 0)
        {
            _buffer = _buffer[..^1];
            RefreshSlots();
            return;
        }

        for (Key k = Key.A; k <= Key.Z; k++)
        {
            if (kb[k].wasPressedThisFrame)
            {
                if (_buffer.Length < _maxLen)
                {
                    _buffer += ((char)('a' + (k - Key.A))).ToString().ToUpperInvariant();
                    RefreshSlots();
                }
                return;
            }
        }

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            Submit();
    }

    void Build()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 60;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        _root = new GameObject("Root");
        _root.transform.SetParent(transform, false);
        var rt = _root.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(_root.transform, false);
        var dimRt = dim.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.9f);

        var panel = new GameObject("Panel");
        panel.transform.SetParent(_root.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.08f, 0.25f);
        panelRt.anchorMax = new Vector2(0.92f, 0.75f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.08f, 0.02f, 0.95f);

        var outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 1f, 0.55f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        var promptGo = new GameObject("Prompt");
        promptGo.transform.SetParent(panel.transform, false);
        var promptRt = promptGo.AddComponent<RectTransform>();
        promptRt.anchorMin = new Vector2(0.04f, 0.70f);
        promptRt.anchorMax = new Vector2(0.96f, 0.96f);
        promptRt.offsetMin = Vector2.zero;
        promptRt.offsetMax = Vector2.zero;
        _prompt = promptGo.AddComponent<TextMeshProUGUI>();
        _prompt.text = "PLEASE ENTER THE 4-LETTER CODE BELOW:";
        _prompt.fontSize = 34;
        _prompt.color = new Color(0.55f, 1f, 0.65f);
        _prompt.alignment = TextAlignmentOptions.TopLeft;

        var caretGo = new GameObject("Caret");
        caretGo.transform.SetParent(panel.transform, false);
        var caretRt = caretGo.AddComponent<RectTransform>();
        caretRt.anchorMin = new Vector2(0.04f, 0.58f);
        caretRt.anchorMax = new Vector2(0.3f, 0.70f);
        caretRt.offsetMin = Vector2.zero;
        caretRt.offsetMax = Vector2.zero;
        _caret = caretGo.AddComponent<TextMeshProUGUI>();
        _caret.text = ">>";
        _caret.fontSize = 36;
        _caret.color = new Color(0.55f, 1f, 0.65f);
        _caret.alignment = TextAlignmentOptions.Left;

        _slots = new TextMeshProUGUI[4];
        for (int i = 0; i < 4; i++)
        {
            var slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(panel.transform, false);
            var srt = slot.AddComponent<RectTransform>();
            float left = 0.12f + i * 0.20f;
            srt.anchorMin = new Vector2(left, 0.18f);
            srt.anchorMax = new Vector2(left + 0.16f, 0.52f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            var img = slot.AddComponent<Image>();
            img.color = new Color(0.05f, 0.20f, 0.05f, 0.95f);

            var o = slot.AddComponent<Outline>();
            o.effectColor = new Color(0.35f, 1f, 0.55f, 0.75f);
            o.effectDistance = new Vector2(2f, -2f);

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(slot.transform, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 64;
            tmp.color = new Color(0.75f, 1f, 0.80f);
            tmp.alignment = TextAlignmentOptions.Center;
            _slots[i] = tmp;
        }

        // X close button
        var closeGo = new GameObject("CloseButton");
        closeGo.transform.SetParent(panel.transform, false);
        var closeRt = closeGo.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.92f, 0.86f);
        closeRt.anchorMax = new Vector2(0.98f, 0.98f);
        closeRt.offsetMin = Vector2.zero;
        closeRt.offsetMax = Vector2.zero;
        var closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0f, 0f, 0f, 0.55f);
        _closeButton = closeGo.AddComponent<Button>();
        _closeButton.onClick.AddListener(() =>
        {
            var cb = _onSubmitted;
            Hide();
            cb?.Invoke(false);
        });

        var closeTextGo = new GameObject("X");
        closeTextGo.transform.SetParent(closeGo.transform, false);
        var closeTextRt = closeTextGo.AddComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;
        var closeTmp = closeTextGo.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "X";
        closeTmp.fontSize = 32;
        closeTmp.color = new Color(0.9f, 0.95f, 0.9f);
        closeTmp.alignment = TextAlignmentOptions.Center;

        _canvas.enabled = false;
        _root.SetActive(false);
    }

    void RefreshSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].text = i < _buffer.Length ? _buffer[i].ToString() : "";
        }
    }

    void Submit()
    {
        string a = _buffer.Trim().ToUpperInvariant();
        string b = _correct.Trim().ToUpperInvariant();
        bool ok = a == b;

        var cb = _onSubmitted;
        Hide();
        cb?.Invoke(ok);
    }

    public void Show(string correctCode, int maxLen, System.Action<bool> onSubmitted)
    {
        _correct = string.IsNullOrWhiteSpace(correctCode) ? "JOHN" : correctCode.Trim().ToUpperInvariant();
        _maxLen = Mathf.Clamp(maxLen <= 0 ? 4 : maxLen, 1, 12);
        _buffer = "";
        _onSubmitted = onSubmitted;
        RefreshSlots();

        _active = true;
        _canvas.enabled = true;
        _root.SetActive(true);
        InteractionPromptUI.Instance?.Hide();
    }

    public void Hide()
    {
        _active = false;
        _canvas.enabled = false;
        if (_root != null) _root.SetActive(false);
        _onSubmitted = null;
    }
}


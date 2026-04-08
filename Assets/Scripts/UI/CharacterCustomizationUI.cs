using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// full-screen hat picker that shows up before the level 1 intro starts
// wire up the hatSprite field in the inspector, that's it
public class CharacterCustomizationUI : MonoBehaviour
{
    [SerializeField] Sprite hatSprite;

    Canvas _canvas;
    System.Action _onComplete;
    GameObject _detectiveTarget; // the detective's GameObject passed in from IntroController
    CursorLockMode _prevLockState;
    bool _prevCursorVisible;

    void Awake()
    {
        BuildCanvas();
        gameObject.SetActive(false); // stays hidden until Show() is called
    }

    // simple overload for when you don't have a direct detective reference
    public void Show(System.Action onComplete)
    {
        Show(onComplete, null);
    }

    // preferred — pass the detective GO directly so we don't have to guess
    public void Show(System.Action onComplete, GameObject detectiveTarget)
    {
        _onComplete = onComplete;
        _detectiveTarget = detectiveTarget;
        InteractionPromptUI.Instance?.Hide();
        EnsureEventSystem();
        CaptureAndShowCursor(); // unlock cursor so the player can click
        gameObject.SetActive(true);
    }

    void BuildCanvas()
    {
        // main canvas — render on top of literally everything
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // dark overlay — visual only, no raycast so it doesn't block the buttons
        var dim = MakeRect("Dim", transform);
        Stretch(dim);
        var dimBg = dim.gameObject.AddComponent<Image>();
        dimBg.color = new Color(0f, 0f, 0f, 0.88f);
        dimBg.raycastTarget = false;

        // centre panel where the options live
        var panel = MakeRect("Panel", transform);
        panel.anchorMin = new Vector2(0.15f, 0.1f);
        panel.anchorMax = new Vector2(0.85f, 0.9f);
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.97f);
        panelImg.raycastTarget = false; // not clickable, just background

        var panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.7f, 0.7f, 1f, 0.6f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        // big title at the top
        var titleRt = MakeRect("Title", panel);
        titleRt.anchorMin = new Vector2(0.05f, 0.78f);
        titleRt.anchorMax = new Vector2(0.95f, 0.95f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
        var title = titleRt.gameObject.AddComponent<TextMeshProUGUI>();
        title.text = "CUSTOMIZE YOUR DETECTIVE";
        title.fontSize = 52;
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(0.92f, 0.92f, 1f);
        title.alignment = TextAlignmentOptions.Center;
        title.raycastTarget = false;

        // little subtitle under it
        var subRt = MakeRect("Subtitle", panel);
        subRt.anchorMin = new Vector2(0.05f, 0.68f);
        subRt.anchorMax = new Vector2(0.95f, 0.80f);
        subRt.offsetMin = subRt.offsetMax = Vector2.zero;
        var sub = subRt.gameObject.AddComponent<TextMeshProUGUI>();
        sub.text = "Choose your look before the investigation begins";
        sub.fontSize = 26;
        sub.color = new Color(0.7f, 0.7f, 0.85f);
        sub.alignment = TextAlignmentOptions.Center;
        sub.raycastTarget = false;

        // left card = hat option
        BuildOption(panel, "Hat", hatSprite,
            new Vector2(0.06f, 0.08f), new Vector2(0.46f, 0.66f),
            () => Pick(true));

        // right card = no hat option
        BuildOption(panel, "No Hat", null,
            new Vector2(0.54f, 0.08f), new Vector2(0.94f, 0.66f),
            () => Pick(false));
    }

    // builds one of the two clickable option cards
    void BuildOption(RectTransform parent, string label, Sprite icon,
                     Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
    {
        var card = MakeRect(label + "Card", parent);
        card.anchorMin = anchorMin;
        card.anchorMax = anchorMax;
        card.offsetMin = card.offsetMax = Vector2.zero;

        var cardImg = card.gameObject.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.2f, 0.95f);

        var cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.5f, 0.5f, 0.9f, 0.7f);
        cardOutline.effectDistance = new Vector2(2f, -2f);

        // button — targetGraphic must be set explicitly when building at runtime
        var btn = card.gameObject.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.9f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        // icon image — raycastTarget off so it doesn't eat the button click
        var iconRt = MakeRect("Icon", card);
        iconRt.anchorMin = new Vector2(0.1f, 0.35f);
        iconRt.anchorMax = new Vector2(0.9f, 0.90f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;

        var iconImg = iconRt.gameObject.AddComponent<Image>();
        iconImg.raycastTarget = false;
        if (icon != null)
        {
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
        }
        else
        {
            // no hat — just show a dark box with a dash
            iconImg.color = new Color(0.25f, 0.25f, 0.35f);
            var noHatLabel = MakeRect("NoHatX", iconRt);
            noHatLabel.anchorMin = Vector2.zero;
            noHatLabel.anchorMax = Vector2.one;
            noHatLabel.offsetMin = noHatLabel.offsetMax = Vector2.zero;
            var noHatTxt = noHatLabel.gameObject.AddComponent<TextMeshProUGUI>();
            noHatTxt.text = "—";
            noHatTxt.fontSize = 80;
            noHatTxt.color = new Color(0.55f, 0.55f, 0.65f);
            noHatTxt.alignment = TextAlignmentOptions.Center;
            noHatTxt.raycastTarget = false;
        }

        // label text at the bottom of the card — also not a raycast target
        var lblRt = MakeRect("Label", card);
        lblRt.anchorMin = new Vector2(0.05f, 0.05f);
        lblRt.anchorMax = new Vector2(0.95f, 0.35f);
        lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
        var lbl = lblRt.gameObject.AddComponent<TextMeshProUGUI>();
        lbl.text = label.ToUpper();
        lbl.fontSize = 34;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = new Color(0.88f, 0.88f, 1f);
        lbl.raycastTarget = false;
        lbl.alignment = TextAlignmentOptions.Center;
    }

    // called when the player clicks hat or no hat
    void Pick(bool wantsHat)
    {
        GameProgress.SetWantsHat(wantsHat);

        if (wantsHat)
            SpawnHat();
        else
            RemoveHat(); // make sure no leftover hat exists

        gameObject.SetActive(false);
        RestoreCursor();
        _onComplete?.Invoke(); // fires StartIntro() in IntroController
        _onComplete = null;
    }

    void SpawnHat()
    {
        if (hatSprite == null) return; // no sprite? nothing to do lol

        var player = ResolveDetectiveTarget();
        if (player == null) return; // somehow no detective, give up

        // blow up any hat that's already on there before we add a new one
        RemoveHat(player);

        var hatGO = new GameObject("DetectiveHat");
        hatGO.transform.SetParent(player.transform, false);
        hatGO.layer = player.layer;

        var baseSr = player.GetComponent<SpriteRenderer>()
                     ?? player.GetComponentInChildren<SpriteRenderer>();

        // hardcoded local Y = 10 because the math kept being wrong lmao
        float localX = 0f;
        float localY = 10f;
        float localScale = 1f;

        if (baseSr != null && baseSr.sprite != null)
        {
            var lossy = player.transform.lossyScale;
            if (lossy.x == 0f) lossy.x = 1f;
            if (lossy.y == 0f) lossy.y = 1f;

            // scale the hat so it's about 90% of the detective's width in world space
            float wantedWorldWidth = baseSr.bounds.size.x * 0.9f;
            float hatNativeWorldWidth = hatSprite.bounds.size.x * lossy.x;
            localScale = hatNativeWorldWidth > 0f ? wantedWorldWidth / hatNativeWorldWidth : 1f;

            // hat pivot is (0.5, 0.5) center so placing at worldX puts the hat center there
            float worldX = baseSr.bounds.center.x;
            localX = (worldX - player.transform.position.x) / lossy.x;
        }

        hatGO.transform.localPosition = new Vector3(localX, localY, 0f);
        hatGO.transform.localScale = Vector3.one * localScale;

        var sr = hatGO.AddComponent<SpriteRenderer>();
        sr.sprite = hatSprite;
        sr.color = Color.white;
        // copy detective sorting layer, crank order to 1000 so it's always on top
        sr.sortingLayerID = baseSr != null ? baseSr.sortingLayerID : 0;
        sr.sortingOrder = 1000;
    }

    void RemoveHat()
    {
        var player = ResolveDetectiveTarget();
        if (player == null) return;
        RemoveHat(player);
    }

    static void RemoveHat(GameObject player)
    {
        var old = player.transform.Find("DetectiveHat");
        if (old != null) Destroy(old.gameObject);
    }

    // figures out which GameObject is the detective
    GameObject ResolveDetectiveTarget()
    {
        if (_detectiveTarget != null) return _detectiveTarget;

        // fallback: try the Player tag
        var tagged = GameObject.FindWithTag("Player");
        if (tagged != null) return tagged;

        // last resort: find any PlayerController in the scene
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) return pc.gameObject;

        return null;
    }

    // makes sure there's an EventSystem + InputSystem module so UI clicks work
    void EnsureEventSystem()
    {
        var existing = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (!existing.gameObject.activeInHierarchy)
                existing.gameObject.SetActive(true);

            if (existing.GetComponent<InputSystemUIInputModule>() == null)
                existing.gameObject.AddComponent<InputSystemUIInputModule>();

            return;
        }

        // no EventSystem at all, make one
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();
    }

    void CaptureAndShowCursor()
    {
        _prevLockState = Cursor.lockState;
        _prevCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestoreCursor()
    {
        Cursor.lockState = _prevLockState;
        Cursor.visible = _prevCursorVisible;
    }

    static RectTransform MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

// singleton that handles the little "press E" bubble floating above NPCs
// any interactable just calls Show/Hide and this thing figures out where to put it on screen
// attach this to the PromptCanvas (or whatever canvas you're using for the prompt)
// if no instance exists in the scene it auto-creates one at runtime so LEVEL4 and other
// scenes that don't use RuntimeSceneSetup still get the prompt working
public class InteractionPromptUI : MonoBehaviour
{
    private static InteractionPromptUI _instance;
    public static InteractionPromptUI Instance
    {
        get
        {
            if (_instance == null)
                _instance = CreateRuntime();
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Offset above the target (world units)")]
    [SerializeField] private float verticalOffset = 1.2f;

    private Transform followTarget;
    private Camera mainCam;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        mainCam = Camera.main;
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    // builds the prompt canvas from scratch - same light-grey style as RuntimeSceneSetup
    // called automatically the first time Instance is accessed in a scene that doesn't have one
    static InteractionPromptUI CreateRuntime()
    {
        var canvasGO = new GameObject("PromptCanvas_Auto");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("PromptPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(240f, 44f);
        panelRT.anchoredPosition = Vector2.zero;
        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.72f);

        var textGO = new GameObject("PromptText");
        textGO.transform.SetParent(panel.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(8f, 4f);
        textRT.offsetMax = new Vector2(-8f, -4f);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Press [E]";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var ui = canvasGO.AddComponent<InteractionPromptUI>();
        ui.Initialize(panel, tmp, 1.2f);
        return ui;
    }

    // used by RuntimeSceneSetup to wire up references without needing the inspector
    public void Initialize(GameObject panel, TextMeshProUGUI text, float offset = 1.2f)
    {
        promptPanel    = panel;
        promptText     = text;
        verticalOffset = offset;
        mainCam        = Camera.main;
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (followTarget == null || promptPanel == null || !promptPanel.activeSelf) return;

        var cam = mainCam != null ? mainCam : Camera.main;
        if (cam == null) return;

        // convert world pos to screen pos so the panel floats right above the object
        // for 2D scenes the interactable z might be behind the camera (e.g. z=100 but cam at z=89)
        // so we force the world point to use the same z as the camera before projecting
        Vector3 worldPos = followTarget.position;
        worldPos.z = cam.transform.position.z; // flatten to camera z so it's never "behind" it
        worldPos += Vector3.up * verticalOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // bail if somehow still behind camera
        if (screenPos.z < 0) return;

        promptPanel.transform.position = screenPos;
    }

    public void Show(string message, Transform target)
    {
        if (mainCam == null) mainCam = Camera.main;
        followTarget = target;
        if (promptText != null) promptText.text = message;
        if (promptPanel != null) promptPanel.SetActive(true);
    }

    public void Hide()
    {
        followTarget = null;
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}

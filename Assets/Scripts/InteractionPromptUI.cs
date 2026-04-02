using UnityEngine;
using TMPro;

// singleton that handles the little "press E" bubble floating above NPCs
// any interactable just calls Show/Hide and this thing figures out where to put it on screen
// attach this to the PromptCanvas (or whatever canvas you're using for the prompt)
public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Offset above the target (world units)")]
    [SerializeField] private float verticalOffset = 1.2f;

    private Transform followTarget;
    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCam = Camera.main;
        if (promptPanel != null) promptPanel.SetActive(false);
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
        if (followTarget == null || !promptPanel.activeSelf) return;

        // convert world pos to screen pos so the panel floats right above the npc
        Vector3 worldPos = followTarget.position + Vector3.up * verticalOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        promptPanel.transform.position = screenPos;
    }

    public void Show(string message, Transform target)
    {
        followTarget = target;
        promptText.text = message;
        promptPanel.SetActive(true);
    }

    public void Hide()
    {
        followTarget = null;
        promptPanel.SetActive(false);
    }
}

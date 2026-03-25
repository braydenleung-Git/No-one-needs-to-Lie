using UnityEngine;
using TMPro;

/// <summary>
/// Singleton UI component that floats a "Press [E]" prompt above interactable objects.
///
/// Setup (do this once in your scene):
///   1. Create a Canvas (Screen Space – Overlay, or World Space)
///   2. Add a child Panel (the promptPanel)
///   3. Add a TextMeshProUGUI inside the panel (assign to promptText)
///   4. Attach this script to the Canvas (or a dedicated child GameObject)
/// </summary>
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

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCam = Camera.main;
        promptPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (followTarget == null || !promptPanel.activeSelf) return;

        // Convert world position to screen position so the prompt floats above the sprite
        Vector3 worldPos = followTarget.position + Vector3.up * verticalOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        promptPanel.transform.position = screenPos;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

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

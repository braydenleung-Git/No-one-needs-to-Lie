using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for anything the player can interact with (NPCs, chests, items, etc.)
/// Attach this (or a subclass) to any GameObject that should be interactable.
///
/// Requirements:
///   - The interactable GameObject needs a Collider2D with "Is Trigger" = true
///   - The Player GameObject must be tagged "Player"
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactPrompt = "Press [E] to interact";

    protected bool playerInRange = false;

    // ── Trigger detection ──────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        InteractionPromptUI.Instance?.Show(interactPrompt, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        InteractionPromptUI.Instance?.Hide();
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!playerInRange) return;

        // Don't re-trigger if dialogue is already running
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    // ── Override this in subclasses ────────────────────────────────────────────

    /// <summary>Called once when the player presses E while in range.</summary>
    public abstract void Interact();
}

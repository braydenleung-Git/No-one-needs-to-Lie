using UnityEngine;
using UnityEngine.InputSystem;

// base class for anything the player can walk up to and press E on
// NPCs, doors, chests etc all extend this
// just need a trigger collider on the object and the player tagged "Player"
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactPrompt = "Press [E] to interact";

    protected bool playerInRange = false;

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

    private void Update()
    {
        if (!playerInRange) return;

        // don't let the player re-trigger while dialogue is already open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive()) return;

        // using new input system here, old Input.GetKeyDown doesn't work with it
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    // subclasses fill this in, e.g. NPCController starts dialogue
    public abstract void Interact();
}

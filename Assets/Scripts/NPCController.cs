using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCController : Interactable
{
    [Header("NPC Identity")]
    [Tooltip("Name shown in the dialogue box header.")]
    public string npcName = "Grandpa";

    [Header("Dialogue Lines")]
    [TextArea(2, 6)]
    public string[] dialogueLines =
    {
        "Oh... a visitor. Haven't seen one of those in a while.",
        "They say the old manor has a secret. I wouldn't go snooping if I were you.",
        "...But what do I know? I'm just an old man."
    };

    // ── Interact override ──────────────────────────────────────────────────────

    public override void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("NPCController: No DialogueManager found in the scene.");
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"NPCController ({npcName}): No dialogue lines assigned.");
            return;
        }

        // Hide the "Press E" prompt while talking
        InteractionPromptUI.Instance?.Hide();

        DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
    }
}

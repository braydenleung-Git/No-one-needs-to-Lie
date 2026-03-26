using UnityEngine;

// attach this to any NPC - fill in the name and dialogue lines in the inspector
// extends Interactable so the trigger + E key stuff is handled automatically
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

        // hide the prompt while talking so it doesn't overlap the dialogue box
        InteractionPromptUI.Instance?.Hide();

        DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
    }
}

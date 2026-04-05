using UnityEngine;

// Crime scene body in the kitchen — wire dialogue lines in the inspector.
public class VictimBodyInteractable : Interactable
{
    [Header("Examination")]
    [SerializeField] private string examinePrompt = "Press [E] to examine";

    [SerializeField] private string speakerLabel = "???";

    void Awake()
    {
        interactPrompt = examinePrompt;
    }

    [TextArea(2, 5)]
    [SerializeField] private string[] examineLines =
    {
        "A man in a suit. There's blood pooling near his head.",
        "No pulse. He's been here a while.",
        "I shouldn't move the body before forensics photographs this."
    };

    public override void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("VictimBodyInteractable: DialogueManager missing.");
            return;
        }

        InteractionPromptUI.Instance?.Hide();

        if (examineLines == null || examineLines.Length == 0)
            return;

        DialogueManager.Instance.StartDialogue(speakerLabel, examineLines);
    }
}

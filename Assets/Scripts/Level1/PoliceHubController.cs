using UnityEngine;

public class PoliceHubController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    private bool _isEnabled = false;
    private bool _isDialogueFinished = false;
    private void OnEnable()
    {
        _isEnabled = true;
        playerController.canMove = false;
        // Start dialogue when this component is enabled
        if (DialogueManager.Instance != null)
        {
            string[] dialogueLines = {
                "It's a brand new day",
                "I wonder what is going to happen today",
                "Better talk to my Partner"
            };
            StartDialogue("Detective Adams", dialogueLines);
        }
    }

    private void Update()
    {
        if (_isEnabled&& _isDialogueFinished)
        {
            playerController.canMove = true;
        }
    }
    private void StartDialogue(string speakerName, string[] dialogueLines)
    {
        DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
        _isDialogueFinished = true;
    }
}

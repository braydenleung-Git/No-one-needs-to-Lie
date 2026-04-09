using UnityEngine;

public class PartnerScript : Interactable
{
    
    private string _npcName = "Partner";

    private bool _isDialogueFinished = true;
    
    public string[] dialogueLines =
    {
        "Hey Adams, good seeing you here, cmon, we need you. ",
        "This new case is, intresting. I’ll meet you at the meeting room.",
        "Oh don't forget to grab the case file from the file room, and you will need to grab your new Officer ID.",
        "You can grab that in Chief's office"
    };

    public override void Interact()
    {
        if (!_isDialogueFinished) return;
        _isDialogueFinished = false;
        // hide the prompt while talking so it doesn't overlap the dialogue box
        InteractionPromptUI.Instance?.Hide();
        DialogueManager.Instance.StartDialogue(_npcName, dialogueLines);
        
        // Add XP for talking to partner
        ExperienceManager.Instance.AddXP(15);
        
        _isDialogueFinished = true;
    }
}
using UnityEngine;

// attach to the cassette player on the coffee table
// requires the cassette tape to be in inventory before it'll play
// after the recording finishes the witness NPC gets activated
public class CassettePlayerInteractable : Interactable
{
    // wired by Level2SceneBuilder at runtime
    [HideInInspector] public NPCController witnessNPC;

    private bool _hasBeenPlayed = false;

    private static readonly string[] _recordingLines =
    {
        "*click* -- the tape starts playing*",
        "\"...I heard arguing around midnight. Something about missing keys.\"",
        "\"There was a loud crash, then silence. I was too scared to look.\"",
        "*the recording ends*"
    };

    public override void Interact()
    {
        if (_hasBeenPlayed)
        {
            DialogueManager.Instance?.StartDialogue("Cassette Player",
                new[] { "You've already heard this recording." });
            return;
        }

        if (!PuzzleState.HasCassetteTape)
        {
            DialogueManager.Instance?.StartDialogue("Cassette Player",
                new[] { "You need a tape to play this." });
            return;
        }

        // play the recording - callback unlocks the witness NPC when done
        _hasBeenPlayed = true;
        PuzzleState.CassettePlayerUsed = true;

        InteractionPromptUI.Instance?.Hide();

        DialogueManager.Instance?.StartDialogue("Cassette Recording",
            _recordingLines, OnRecordingComplete);
    }

    private void OnRecordingComplete()
    {
        if (witnessNPC != null)
            witnessNPC.gameObject.SetActive(true);
    }
}

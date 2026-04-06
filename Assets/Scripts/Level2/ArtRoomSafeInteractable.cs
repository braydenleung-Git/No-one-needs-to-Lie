using UnityEngine;

// Optional wall safe — same JOHN code as the art-room painting; both share PuzzleState.ArtRoomCodeSolved
public class ArtRoomSafeInteractable : LucyFourLetterCodeInteractable
{
    protected override string SpeakerLabel => "Safe";

    protected override bool IsCodeAlreadySolved() =>
        PuzzleState.ArtRoomCodeSolved || GameProgress.HasUvFlashlight;

    protected override string[] GetWrongCodeLines() =>
        new[] { "Nothing happens — the letters don't match what the paintings implied." };

    protected override string[] GetAlreadySolvedLines() =>
        new[] { "The safe is already open. Whatever was inside is gone." };

    protected override string[] GetLinesWhenCassetteNotHeard() =>
        new[] { "A recessed wall safe. You have no reason to trust what code might open it yet." };

    protected override string[] GetIntroLines() =>
        new[]
        {
            "Four letters, going by the scraps hidden in the frames around the house.",
            "The paintings name a letter and a number of objects — that number is the letter's place in the word. Order them, then try the result here.",
            "When you're ready, type the code (letters A–Z), then press Enter."
        };

    protected override void OnCorrectCodeEntered()
    {
        PuzzleState.ArtRoomCodeSolved = true;
        DialogueManager.Instance?.StartDialogue("Safe",
            new[]
            {
                "The tumblers fall into place.",
                "Behind the false panel you find a small key and a folded note."
            });
    }
}

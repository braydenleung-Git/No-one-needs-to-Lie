using UnityEngine;

// Large framed piece in the art room — enter JOHN; grants UV flashlight (persistent inventory)
public class ArtRoomJohnCodePaintingInteractable : LucyFourLetterCodeInteractable
{
    protected override string SpeakerLabel => "Framed print";

    protected override bool IsCodeAlreadySolved() =>
        PuzzleState.ArtRoomCodeSolved || GameProgress.HasUvFlashlight;

    protected override string[] GetWrongCodeLines() =>
        new[] { "Nothing shifts — the letters don't match the pattern from the other paintings." };

    protected override string[] GetAlreadySolvedLines() =>
        new[]
        {
            "You've already worked out the hidden name. The frame has nothing more to offer."
        };

    protected override string[] GetLinesWhenCassetteNotHeard() =>
        new[] { "A stately still life. Without context, it's just paint and varnish." };

    protected override string[] GetIntroLines() =>
        new[]
        {
            "Up close, the gilded frame is deeper than it looked — as if something expects a word.",
            "Four letters, from the anagram hidden across the house: count the objects, note the scratched letters, order by count, then try that word here.",
            "Type the code (A–Z), then press Enter."
        };

    protected override void OnCorrectCodeEntered()
    {
        PuzzleState.ArtRoomCodeSolved = true;
        PersistentGameItems.GrantUvFlashlightToPlayer();

        DialogueManager.Instance?.StartDialogue("Framed print",
            new[]
            {
                "Something behind the canvas clicks.",
                "Tucked into the backing you find a compact UV flashlight — the kind used to read security ink and old repairs."
            });
    }
}

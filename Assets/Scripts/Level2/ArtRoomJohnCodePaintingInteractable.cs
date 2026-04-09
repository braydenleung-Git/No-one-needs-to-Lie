using UnityEngine;

// Large framed piece in the art room — enter JOHN; grants UV flashlight (persistent inventory)
public class ArtRoomJohnCodePaintingInteractable : LucyFourLetterCodeInteractable
{
    protected override string SpeakerLabel => "Framed print";

    // track whether the green code UI is currently open so E doesn't re-trigger
    bool _codeUIOpen;

    protected override bool IsCodeAlreadySolved() =>
        PuzzleState.ArtRoomCodeSolved || GameProgress.HasUvFlashlight;

    // block the E-press re-trigger while the green canvas UI is up
    protected override bool AllowInteractWithE() => !_codeUIOpen;

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
            "Type the code (A\u2013Z), then press Enter."
        };

    // Override Interact so we launch the green canvas UI instead of inline keyboard entry
    public override void Interact()
    {
        if (DialogueManager.Instance == null) return;

        if (IsCodeAlreadySolved())
        {
            DialogueManager.Instance.StartDialogue(SpeakerLabel, GetAlreadySolvedLines());
            return;
        }

        if (requireCassetteHeard && !PuzzleState.CassettePlayerUsed)
        {
            DialogueManager.Instance.StartDialogue(SpeakerLabel, GetLinesWhenCassetteNotHeard());
            return;
        }

        InteractionPromptUI.Instance?.Hide();
        // show intro dialogue then pop the green terminal UI
        DialogueManager.Instance.StartDialogue(SpeakerLabel, GetIntroLines(), OpenCodeUI);
    }

    void OpenCodeUI()
    {
        _codeUIOpen = true;
        FourLetterCodeUI.Instance?.Show(correctCode, maxCodeLength, OnCodeResult);
    }

    void OnCodeResult(bool correct)
    {
        _codeUIOpen = false;
        if (correct)
        {
            OnCorrectCodeEntered();
        }
        else
        {
            // wrong code — show feedback then let them try again if still in range
            DialogueManager.Instance?.StartDialogue(SpeakerLabel, GetWrongCodeLines(),
                () => { if (playerInRange) OpenCodeUI(); });
        }
    }

    protected override void OnCorrectCodeEntered()
    {
        PuzzleState.ArtRoomCodeSolved = true;
        PersistentGameItems.GrantUvFlashlightToPlayer();

        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.AddXP(20);

        DialogueManager.Instance?.StartDialogue("Framed print",
            new[]
            {
                "Something behind the canvas clicks.",
                "Tucked into the backing you find a compact UV flashlight — the kind used to read security ink and old repairs.",
                "We should probably check out the house above 218."
            });
    }
}

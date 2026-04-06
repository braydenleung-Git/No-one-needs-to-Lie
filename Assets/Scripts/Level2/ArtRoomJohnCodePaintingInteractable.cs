using UnityEngine;

// Large framed piece in the art room — shows 4-letter code UI (JOHN); grants UV flashlight (persistent inventory)
// rewrote this to not extend the base class anymore, way easier to just handle the UI callbacks directly here
public class ArtRoomJohnCodePaintingInteractable : Interactable
{
    [Header("Lock")]
    public string correctCode = PuzzleState.ArtRoomCodeSolution; // pulls JOHN from PuzzleState so we're not hardcoding the answer everywhere
    [Min(1)] public int maxCodeLength = 4; // 4 letters, one per cipher painting clue, don't change this lol
    public bool requireCassetteHeard = true; // gate it behind the tape, otherwise the player has zero context for what to enter

    public override void Interact()
    {
        InteractionPromptUI.Instance?.Hide();

        // if they already solved it or somehow already have the UV flashlight, just tell them and bail out early
        if (PuzzleState.ArtRoomCodeSolved || GameProgress.HasUvFlashlight)
        {
            DialogueManager.Instance?.StartDialogue("Framed print",
                new[] { "You've already worked out the hidden name. The frame has nothing more to offer." });
            return;
        }

        // cassette not heard yet = player has no context, show a generic "nothing to see here" line
        if (requireCassetteHeard && !PuzzleState.CassettePlayerUsed)
        {
            DialogueManager.Instance?.StartDialogue("Framed print",
                new[] { "A stately still life. Without context, it's just paint and varnish." });
            return;
        }

        // Bring up the retro code-entry UI.
        // if the UI prefab isn't in the scene for some reason, show a vague line and don't crash
        if (FourLetterCodeUI.Instance == null)
        {
            DialogueManager.Instance?.StartDialogue("Framed print",
                new[] { "You feel a mechanism behind the canvas, but can't focus on it right now." });
            return;
        }

        // open the code entry popup and wait for the player to type something and hit submit
        FourLetterCodeUI.Instance.Show(
            correctCode,
            maxCodeLength,
            onSubmitted: ok =>
            {
                if (!ok)
                {
                    // wrong answer - scold them a bit then immediately reopen the UI so they don't have to walk up again
                    DialogueManager.Instance?.StartDialogue("Framed print",
                        new[] { "No — try again. Pay attention to the paintings." },
                        onComplete: () =>
                        {
                            // Immediately let them retry without walking away.
                            FourLetterCodeUI.Instance?.Show(correctCode, maxCodeLength, onSubmitted: ok2 =>
                            {
                                if (!ok2)
                                {
                                    // still wrong on second attempt - show the message again and keep the UI open, no hard fail state
                                    DialogueManager.Instance?.StartDialogue("Framed print",
                                        new[] { "No — try again. Pay attention to the paintings." },
                                        onComplete: () => FourLetterCodeUI.Instance?.Show(correctCode, maxCodeLength, onSubmitted: null));
                                    return;
                                }

                                // correct on the second try - mark solved and hand over the flashlight
                                PuzzleState.ArtRoomCodeSolved = true;
                                PersistentGameItems.GrantUvFlashlightToPlayer();

                                DialogueManager.Instance?.StartDialogue("Framed print",
                                    new[]
                                    {
                                        "Something behind the canvas clicks.",
                                        "Tucked into the backing you find a compact UV flashlight — the kind used to read security ink and old repairs."
                                    });
                            });
                        });
                    return;
                }

                // got it right on the first try, nice - mark the puzzle done and give them the flashlight
                PuzzleState.ArtRoomCodeSolved = true;
                PersistentGameItems.GrantUvFlashlightToPlayer();

                DialogueManager.Instance?.StartDialogue("Framed print",
                    new[]
                    {
                        "Something behind the canvas clicks.",
                        "Tucked into the backing you find a compact UV flashlight — the kind used to read security ink and old repairs."
                    });
            });
    }
}

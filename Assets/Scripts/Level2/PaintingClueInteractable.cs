using UnityEngine;

// put this on any framed painting in the art room (or anywhere really)
// each cipher painting hides a letter and a position number, player collects all 4 to spell JOHN
// nothing useful shows up until the cassette recording has been played - that's the trigger
public class PaintingClueInteractable : Interactable
{
    public enum LetterCorner
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }

    [Header("Identity")]
    [Tooltip("Shown as the dialogue speaker label.")]
    public string paintingTitle = "Painting";

    [Header("Cipher (letter anagram puzzle)")]
    public bool isCipherClue = true;
    public char clueLetter = 'L';
    [Tooltip("How many of countedObject appear in the painting — also the index in the final word.")]
    [Min(1)]
    public int positionIndex = 1;
    public string countedObjectSingular = "boat";
    public string countedObjectPlural = "boats";

    [Tooltip("Where the scratched letter appears on the canvas (for flavour text).")]
    public LetterCorner letterInscribedCorner = LetterCorner.BottomLeft;

    [Header("Gating")]
    [Tooltip("If true, cipher lines only appear after the cassette has been played once.")]
    public bool requireCassetteForCipher = true;

    [TextArea(1, 3)]
    public string[] genericLinesBeforeCassette =
    {
        "A framed piece. You don't know what to look for yet."
    };

    [TextArea(1, 3)]
    public string[] flavorLinesNoCipher =
    {
        "Nice frame. It doesn't seem to hide a message."
    };

    [Header("Full-screen view")]
    [Tooltip("Optional: if set, interacting pops up a full-screen view of this painting.")]
    public Sprite fullScreenSprite; // drag the J/O/H/N sprite in here from the inspector, leave null if this painting has no image

    [Tooltip("If true, show the painting full-screen before any clue dialogue.")]
    public bool showFullScreenOnInteract = true; // basically always want this true for the cipher paintings

    [Tooltip("If true, show this painting's clue dialogue after closing the full-screen view.")]
    public bool showClueDialogueAfterClose = true; // set to false if the image alone is the clue and you don't want the text after

    static string CornerPhrase(LetterCorner c)
    {
        return c switch
        {
            LetterCorner.BottomLeft => "bottom-left",
            LetterCorner.BottomRight => "bottom-right",
            LetterCorner.TopLeft => "top-left",
            LetterCorner.TopRight => "top-right",
            _ => "bottom-left"
        };
    }

    // handles 1st, 2nd, 3rd, 4th etc - the 11/12/13 edge case trips people up so handling it explicitly
    static string Ordinal(int n)
    {
        int m = n % 100;
        if (m >= 11 && m <= 13) return $"{n}th";
        return (n % 10) switch
        {
            1 => $"{n}st",
            2 => $"{n}nd",
            3 => $"{n}rd",
            _ => $"{n}th"
        };
    }

    // builds "1 boat" or "3 boats" depending on how many objects are in the painting
    string CountPhrase()
    {
        if (positionIndex <= 0) positionIndex = 1;
        return positionIndex == 1
            ? $"1 {countedObjectSingular}"
            : $"{positionIndex} {countedObjectPlural}";
    }

    // assembles the actual cipher dialogue lines from the letter + count data
    string[] BuildCipherLines()
    {
        char L = char.ToUpper(clueLetter);
        string ord = Ordinal(positionIndex);
        string corner = CornerPhrase(letterInscribedCorner);
        return new[]
        {
            "You study the composition, the varnish, and the edge of the frame.",
            $"You notice {CountPhrase()}. Faintly scratched into the {corner} of the image is the letter '{L}'.",
            $"It feels deliberate — as if marking the {ord} letter of a hidden name."
        };
    }

    public override void Interact()
    {
        InteractionPromptUI.Instance?.Hide();

        // If we have a sprite, let the player inspect it full-screen first.
        // this is the new flow - show the image popup, THEN run the clue dialogue once they close it
        if (showFullScreenOnInteract && fullScreenSprite != null && PaintingViewerUI.Instance != null)
        {
            PaintingViewerUI.Instance.Show(fullScreenSprite, paintingTitle, onClosed: () =>
            {
                // player closed the painting viewer, now decide if we show dialogue
                if (!showClueDialogueAfterClose)
                    return;

                if (DialogueManager.Instance == null) return;

                // tape hasn't been played yet - player shouldn't know to look for cipher stuff
                if (requireCassetteForCipher && !PuzzleState.CassettePlayerUsed)
                {
                    DialogueManager.Instance.StartDialogue(paintingTitle, genericLinesBeforeCassette);
                    return;
                }

                // not a cipher painting, just show generic flavour text
                if (!isCipherClue)
                {
                    DialogueManager.Instance.StartDialogue(paintingTitle, flavorLinesNoCipher);
                    return;
                }

                // cipher painting + cassette heard = show the actual letter/position clue
                DialogueManager.Instance.StartDialogue(paintingTitle, BuildCipherLines());
            });

            return;
        }

        // If this painting is meant to be viewed full-screen but the sprite is missing, don't show fallback dialogue.
        // (Otherwise players see clue text instead of the image and think the popup is broken.)
        // just log a warning so we know to fix it in the editor, don't silently fail
        if (showFullScreenOnInteract && fullScreenSprite == null)
        {
            Debug.LogWarning($"{name}: PaintingClueInteractable missing fullScreenSprite. " +
                             "Place J/O/H/N PNGs under Assets/Resources/L1/ and ensure they import as Sprites.");
            return;
        }

        if (DialogueManager.Instance == null) return;

        // tape hasn't been played yet - player shouldn't know to look for cipher stuff
        if (requireCassetteForCipher && !PuzzleState.CassettePlayerUsed)
        {
            DialogueManager.Instance.StartDialogue(paintingTitle, genericLinesBeforeCassette);
            return;
        }

        // this painting is just decoration, no cipher hidden in it
        if (!isCipherClue)
        {
            DialogueManager.Instance.StartDialogue(paintingTitle, flavorLinesNoCipher);
            return;
        }

        DialogueManager.Instance.StartDialogue(paintingTitle, BuildCipherLines());
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box == null || !box.enabled) return;
        Gizmos.color = isCipherClue
            ? new Color(0.2f, 0.95f, 0.35f, 0.9f)
            : new Color(1f, 0.85f, 0.2f, 0.75f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube((Vector3)box.offset, new Vector3(box.size.x, box.size.y, 0.05f));
    }
#endif
}

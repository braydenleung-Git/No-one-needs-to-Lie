using UnityEngine;

// put this on any framed painting in the art room (or anywhere really)
// each cipher painting hides a letter and a position number, player collects all 4 to spell LUCY
// nothing useful shows up until the cassette recording has been played - that's the trigger
public class PaintingClueInteractable : Interactable
{
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
    // this is what the player reads and writes down to figure out the safe code
    string[] BuildCipherLines()
    {
        char L = char.ToUpper(clueLetter);
        string ord = Ordinal(positionIndex);
        return new[]
        {
            "You study the composition and the frame.",
            $"A letter seems worked into the scene: '{L}'.",
            $"You count {CountPhrase()} — as if marking the {ord} spot in a hidden name."
        };
    }

    public override void Interact()
    {
        if (DialogueManager.Instance == null) return;

        InteractionPromptUI.Instance?.Hide();

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

        // show the actual cipher clue
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

using UnityEngine;
using UnityEngine.InputSystem;

// this is the wall safe in the art room - player types in a code they figured out from the paintings
// the code is the word LUCY, each painting has a letter + a position, player orders them and types it
// doesn't do anything until you've heard the cassette tape recording first
public class ArtRoomSafeInteractable : Interactable
{
    [Header("Lock")]
    [Tooltip("Correct combination, case-insensitive (matches cipher paintings → PuzzleState.ArtRoomCodeSolution).")]
    public string correctCode = PuzzleState.ArtRoomCodeSolution;

    [Tooltip("Max letters the player can type (4 for LUCY).")]
    [Min(1)]
    public int maxCodeLength = 4;

    [Tooltip("Require the cassette recording first so the player knows to read the paintings.")]
    public bool requireCassetteHeard = true;

    // _awaitingCode means the player opened the code entry UI and is currently typing
    bool _awaitingCode;
    string _buffer = "";  // what the player has typed so far

    // if the player walks away while typing, cancel everything and reset
    protected override void OnInteractableUpdateEarly()
    {
        if (_awaitingCode && !playerInRange)
        {
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
        }
    }

    // this runs every frame while the player is standing next to the safe and the code UI is open
    // manually reading keyboard input because a text field widget is overkill for 4 letters
    protected override void OnInteractableUpdateInRange()
    {
        if (!_awaitingCode || PuzzleState.ArtRoomSafeOpened) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // backspace removes the last letter they typed
        if (kb.backspaceKey.wasPressedThisFrame && _buffer.Length > 0)
        {
            _buffer = _buffer[..^1];
            RefreshPrompt();
            return;
        }

        // escape cancels the whole thing
        if (kb.escapeKey.wasPressedThisFrame)
        {
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
            return;
        }

        // loop through A-Z and check if any letter key was pressed this frame
        // converts the Key enum to the actual char so we don't have a giant switch statement
        for (Key k = Key.A; k <= Key.Z; k++)
        {
            if (kb[k].wasPressedThisFrame)
            {
                if (_buffer.Length < maxCodeLength)
                {
                    _buffer += ((char)('a' + (k - Key.A))).ToString().ToUpperInvariant();
                    RefreshPrompt();
                }
                return;
            }
        }

        // enter submits whatever they typed
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            TrySubmit();
    }

    // while the code UI is open, pressing E shouldn't also trigger the Interact() flow again
    protected override bool AllowInteractWithE() => !_awaitingCode;

    // updates the on-screen prompt to show what the player has typed so far
    void RefreshPrompt()
    {
        InteractionPromptUI.Instance?.Show(
            $"Safe code: {_buffer}_  (Enter=submit, Esc=cancel, Backspace)", transform);
    }

    // called when player hits enter - checks if the code matches
    void TrySubmit()
    {
        if (_buffer.Length == 0) return;

        // trim and uppercase both sides so casing doesn't matter
        string a = _buffer.Trim().ToUpperInvariant();
        string b = correctCode.Trim().ToUpperInvariant();

        if (a == b)
        {
            // correct! mark the safe as opened and play the success dialogue
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
            PuzzleState.ArtRoomSafeOpened = true;
            DialogueManager.Instance?.StartDialogue("Safe",
                new[]
                {
                    "The tumblers fall into place.",
                    "Behind the false panel you find a small key and a folded note. Whatever was locked away is yours now."
                });
            return;
        }

        // wrong code - clear the buffer and let them try again after the dialogue closes
        _buffer = "";
        InteractionPromptUI.Instance?.Hide();
        DialogueManager.Instance?.StartDialogue("Safe",
            new[] { "Nothing happens — the letters don't match what the paintings implied." },
            () =>
            {
                // re-open the prompt if they're still standing there
                if (playerInRange)
                    RefreshPrompt();
            });
    }

    public override void Interact()
    {
        if (DialogueManager.Instance == null) return;

        // already solved, just tell them
        if (PuzzleState.ArtRoomSafeOpened)
        {
            DialogueManager.Instance.StartDialogue("Safe",
                new[] { "The safe is already open. The note lies where you left it." });
            return;
        }

        // haven't heard the tape yet - don't give away that there's a code to enter
        if (requireCassetteHeard && !PuzzleState.CassettePlayerUsed)
        {
            DialogueManager.Instance.StartDialogue("Safe",
                new[] { "A recessed wall safe. You have no reason to trust what code might open it yet." });
            return;
        }

        InteractionPromptUI.Instance?.Hide();

        // already in code-entry mode, don't re-open it
        if (_awaitingCode)
            return;

        // give the player the hint dialogue first, then open the code entry UI after they read it
        DialogueManager.Instance.StartDialogue("Safe",
            new[]
            {
                "Four letters, going by the scraps hidden in the frames around the house.",
                "The paintings name a letter and a number of objects — that number is the letter's place in the word. Order them, then try the result here.",
                "When you're ready, type the code (letters A–Z), then press Enter."
            },
            () =>
            {
                _awaitingCode = true;
                RefreshPrompt();
            });
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box == null || !box.enabled) return;
        Gizmos.color = new Color(0.35f, 0.55f, 1f, 0.95f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube((Vector3)box.offset, new Vector3(box.size.x, box.size.y, 0.05f));
    }
#endif
}

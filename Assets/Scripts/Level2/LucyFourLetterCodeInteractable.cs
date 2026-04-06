using UnityEngine;
using UnityEngine.InputSystem;

// Shared 4-letter code entry (e.g. JOHN) — subclass for wall safe vs art-room painting, etc.
public abstract class LucyFourLetterCodeInteractable : Interactable
{
    [Header("Lock")]
    [Tooltip("Correct combination, case-insensitive.")]
    public string correctCode = PuzzleState.ArtRoomCodeSolution;

    [Min(1)]
    public int maxCodeLength = 4;

    [Tooltip("Require the cassette recording first so the player knows to read the paintings.")]
    public bool requireCassetteHeard = true;

    bool _awaitingCode;
    string _buffer = "";

    protected abstract string SpeakerLabel { get; }

    protected virtual string PromptWhileTyping =>
        $"Code: {_buffer}_  (Enter=submit, Esc=cancel, Backspace)";

    protected override void OnInteractableUpdateEarly()
    {
        if (_awaitingCode && !playerInRange)
        {
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
        }
    }

    protected override void OnInteractableUpdateInRange()
    {
        if (!_awaitingCode || IsCodeAlreadySolved()) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.backspaceKey.wasPressedThisFrame && _buffer.Length > 0)
        {
            _buffer = _buffer[..^1];
            RefreshPrompt();
            return;
        }

        if (kb.escapeKey.wasPressedThisFrame)
        {
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
            return;
        }

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

        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            TrySubmit();
    }

    protected override bool AllowInteractWithE() => !_awaitingCode;

    void RefreshPrompt()
    {
        InteractionPromptUI.Instance?.Show(PromptWhileTyping, transform);
    }

    protected abstract bool IsCodeAlreadySolved();

    void TrySubmit()
    {
        if (_buffer.Length == 0) return;

        string a = _buffer.Trim().ToUpperInvariant();
        string b = correctCode.Trim().ToUpperInvariant();

        if (a == b)
        {
            _awaitingCode = false;
            _buffer = "";
            InteractionPromptUI.Instance?.Hide();
            OnCorrectCodeEntered();
            return;
        }

        _buffer = "";
        InteractionPromptUI.Instance?.Hide();
        DialogueManager.Instance?.StartDialogue(SpeakerLabel, GetWrongCodeLines(),
            () =>
            {
                if (playerInRange)
                    RefreshPrompt();
            });
    }

    protected abstract void OnCorrectCodeEntered();

    protected abstract string[] GetWrongCodeLines();

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

        if (_awaitingCode)
            return;

        DialogueManager.Instance.StartDialogue(SpeakerLabel, GetIntroLines(),
            () =>
            {
                _awaitingCode = true;
                RefreshPrompt();
            });
    }

    protected abstract string[] GetAlreadySolvedLines();
    protected abstract string[] GetLinesWhenCassetteNotHeard();
    protected abstract string[] GetIntroLines();

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

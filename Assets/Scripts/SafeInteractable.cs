using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Attach to your Safe GameObject
// Requires:
//   - BoxCollider2D (Is Trigger ON) for interaction detection
//   - SafeUI canvas assigned in inspector
//   - SafeOpenUI canvas assigned in inspector
//   - GameState.cs in project
public class SafeInteractable : Interactable
{
    [Header("Safe UI")]
    [SerializeField] private GameObject safeUICanvas;        // the SafeUI canvas GameObject
    [SerializeField] private TMP_InputField codeInputField;  // CodeInput
    [SerializeField] private TextMeshProUGUI attemptsText;   // AttemptsText
    [SerializeField] private Button submitButton;            // SubmitButton
    [SerializeField] private Button closeButton;             // CloseButton

    [Header("Safe Open UI")]
    [SerializeField] private SafeOpenUI safeOpenUI;          // drag SafeOpenUI canvas here

    [Header("Code")]
    // PLACEHOLDER - change to real DOB later (format DDMMYY or MMDDYY, confirm with team)
    [SerializeField] private string correctCode = "676767";

    private int attemptsLeft = 3;
    private bool safeOpen = false;
    private bool safeSolved = false;

    private void Start()
    {
        interactPrompt = "Press [E] to interact with safe";

        if (safeUICanvas != null)
            safeUICanvas.SetActive(false);

        // safety check for testing directly without Town scene
        if (GameState.Instance == null)
        {
            var go = new GameObject("GameStateManager");
            go.AddComponent<GameState>();
        }

        if (GameState.Instance.SafeSolved)
            safeSolved = true;

        submitButton?.onClick.AddListener(OnSubmitCode);
        closeButton?.onClick.AddListener(CloseSafe);

        UpdateAttemptsText();
    }

    // gate check + prompt logic
    protected override void OnInteractableUpdateInRange()
    {
        // hide prompt if safe is open, safe open UI is open, or sofa not investigated yet
        if (!GameState.Instance.SofaInvestigated || safeOpen || (safeOpenUI != null && safeOpenUI.IsOpen))
        {
            InteractionPromptUI.Instance?.Hide();
        }
        else if (GameState.Instance.SafeSolved)
        {
            interactPrompt = "Press [E] to examine safe";
            InteractionPromptUI.Instance?.Show(interactPrompt, transform);
        }
        else
        {
            interactPrompt = "Press [E] to interact with safe";
            InteractionPromptUI.Instance?.Show(interactPrompt, transform);
        }
    }

    public override void Interact()
    {
        if (!GameState.Instance.SofaInvestigated) return;

        // if already solved just show the open interior UI
        if (GameState.Instance.SafeSolved)
        {
            if (safeOpenUI != null) safeOpenUI.Show();
            return;
        }

        OpenSafe();
    }

    private void OpenSafe()
    {
        safeOpen = true;
        safeUICanvas.SetActive(true);

        // hide prompt while safe UI is open
        InteractionPromptUI.Instance?.Hide();

        // freeze player while safe is open
        Time.timeScale = 0f;

        // clear previous input
        if (codeInputField != null)
            codeInputField.text = "";

        // show attempts dialogue
        DialogueManager.Instance?.StartDialogue(
            "",
            new[] { $"You have {attemptsLeft} attempt(s). Find clues related to the owner..." }
        );
    }

    private void CloseSafe()
    {
        safeOpen = false;
        safeUICanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnSubmitCode()
    {
        if (codeInputField == null) return;

        string entered = codeInputField.text.Trim();

        // check if player actually entered something
        if (entered.Length < 6)
        {
            DialogueManager.Instance?.StartDialogue(
                "",
                new[] { "Enter a 6 digit code..." }
            );
            return;
        }

        if (entered == correctCode)
        {
            // correct!
            safeSolved = true;
            GameState.Instance.SafeSolved = true;
            CloseSafe();
            DialogueManager.Instance?.StartDialogue(
                "",
                new[] { "Oh... what's inside that?" },
                onComplete: TriggerNextEvent
            );
        }
        else
        {
            // wrong code
            attemptsLeft--;
            UpdateAttemptsText();

            if (attemptsLeft <= 0)
            {
                // no attempts left - restart game
                CloseSafe();
                DialogueManager.Instance?.StartDialogue(
                    "",
                    new[] { "The safe locks down. Everything goes dark..." },
                    onComplete: RestartGame
                );
            }
            else
            {
                // still have attempts
                codeInputField.text = "";
                DialogueManager.Instance?.StartDialogue(
                    "",
                    new[] { $"Wrong code. {attemptsLeft} attempt(s) remaining..." }
                );
            }
        }
    }

    private void UpdateAttemptsText()
    {
        if (attemptsText != null)
            attemptsText.text = $"Attempts: {attemptsLeft}";
    }

    private void TriggerNextEvent()
    {
        // safe is cracked - show the interior UI
        if (safeOpenUI != null) safeOpenUI.Show();
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // block E from firing Interact() while safe UI is open
    protected override bool AllowInteractWithE() => !safeOpen;
}
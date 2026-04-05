using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SafeInteractable : Interactable
{
    [Header("Safe UI")]
    [SerializeField] private GameObject safeUICanvas;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;

    [Header("Code")]
    // PLACEHOLDER - change to real DOB later (format DDMMYY or MMDDYY, confirm with team)
    [SerializeField] private string correctCode = "676767";

    private int attemptsLeft = 3;
    private bool safeOpen = false;
    private bool safeSolved = false;

    private void Start()
    {
        interactPrompt = "Press [E] to interact";

        if (safeUICanvas != null)
            safeUICanvas.SetActive(false);

        submitButton?.onClick.AddListener(OnSubmitCode);
        closeButton?.onClick.AddListener(CloseSafe);

        UpdateAttemptsText();
    }

    // gate check happens here instead of OnTriggerEnter2D
    // base class handles the trigger, we just block interaction if conditions aren't met
    protected override void OnInteractableUpdateInRange()
    {
        if (!GameState.SofaInvestigated || safeSolved || safeOpen) // ← add safeOpen check
            InteractionPromptUI.Instance?.Hide();
        else
            InteractionPromptUI.Instance?.Show(interactPrompt, transform);
    }

    public override void Interact()
    {
        if (!GameState.SofaInvestigated) return; // sofa must be done first
        if (safeSolved) return;
        OpenSafe();
    }

    private void OpenSafe()
    {
        safeOpen = true;
        safeUICanvas.SetActive(true);
    
        InteractionPromptUI.Instance?.Hide(); // ← add this line

        Time.timeScale = 0f;

        if (codeInputField != null)
            codeInputField.text = "";

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
            safeSolved = true;
            CloseSafe();
            DialogueManager.Instance?.StartDialogue(
                "",
                new[] { "Oh... what's inside that?" },
                onComplete: TriggerNextEvent
            );
        }
        else
        {
            attemptsLeft--;
            UpdateAttemptsText();

            if (attemptsLeft <= 0)
            {
                CloseSafe();
                DialogueManager.Instance?.StartDialogue(
                    "",
                    new[] { "The safe locks down. Everything goes dark..." },
                    onComplete: RestartGame
                );
            }
            else
            {
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
        Debug.Log("Safe solved - trigger next event");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    protected override bool AllowInteractWithE() => !safeOpen;
}
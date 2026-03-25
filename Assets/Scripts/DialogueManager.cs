using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton that manages the dialogue UI panel.
/// Displays lines with a typewriter effect; press E to skip typing or advance.
///
/// Canvas setup (do this once):
///   DialogueCanvas (Screen Space – Overlay)
///     └── DialoguePanel
///           ├── SpeakerNameText  (TextMeshProUGUI)
///           ├── DialogueBodyText (TextMeshProUGUI)
///           └── ContinuePrompt   (e.g. a small "▶" or "Press E" label)
///
/// Attach this script to DialogueCanvas and wire up the 4 serialised fields.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;
    [SerializeField] private GameObject continuePrompt;   // small "▶ E" indicator

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.04f;     // seconds between each character

    // ── State ──────────────────────────────────────────────────────────────────

    private string[] lines;
    private int lineIndex;
    private bool isTyping;
    private bool dialogueActive;
    private Coroutine typewriterRoutine;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                // Skip the rest of the typewriter for this line
                StopCoroutine(typewriterRoutine);
                dialogueBodyText.text = lines[lineIndex];
                isTyping = false;
                continuePrompt.SetActive(true);
            }
            else
            {
                AdvanceLine();
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Begin a dialogue sequence. Called by NPCController.Interact().</summary>
    public void StartDialogue(string speakerName, string[] dialogueLines)
    {
        lines = dialogueLines;
        lineIndex = 0;
        dialogueActive = true;

        speakerNameText.text = speakerName;
        dialoguePanel.SetActive(true);
        continuePrompt.SetActive(false);

        typewriterRoutine = StartCoroutine(TypeLine(lines[0]));
    }

    /// <summary>Returns true while a dialogue is currently open.</summary>
    public bool IsDialogueActive() => dialogueActive;

    // ── Private helpers ────────────────────────────────────────────────────────

    private void AdvanceLine()
    {
        lineIndex++;

        if (lineIndex >= lines.Length)
        {
            EndDialogue();
        }
        else
        {
            typewriterRoutine = StartCoroutine(TypeLine(lines[lineIndex]));
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        continuePrompt.SetActive(false);
        dialogueBodyText.text = string.Empty;

        foreach (char c in line)
        {
            dialogueBodyText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        continuePrompt.SetActive(true);
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);
        lines = null;
    }
}

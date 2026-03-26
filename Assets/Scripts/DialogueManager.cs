using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// manages the dialogue box UI - typewriter effect, advancing lines, closing it
// singleton so any NPC can call it without needing a reference
// attach this to the DialogueCanvas in the scene
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;
    [SerializeField] private GameObject continuePrompt; // the little "press E" indicator at the bottom

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.04f; // seconds between each character appearing

    private string[] lines;
    private int lineIndex;
    private bool isTyping;
    private bool dialogueActive;
    private Coroutine typewriterRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        // E to skip typing animation or move to next line
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                // skip the rest of the animation and show full line immediately
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

    // called by NPCController when the player interacts
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

    public bool IsDialogueActive() => dialogueActive;

    // lets RuntimeSceneSetup wire up references at runtime without needing the inspector
    public void Initialize(GameObject panel, TextMeshProUGUI nameText,
        TextMeshProUGUI bodyText, GameObject continuePromptObj, float delay = 0.04f)
    {
        dialoguePanel    = panel;
        speakerNameText  = nameText;
        dialogueBodyText = bodyText;
        continuePrompt   = continuePromptObj;
        charDelay        = delay;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

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

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PrinterPuzzle : MonoBehaviour
{
    public GameObject puzzlePanel;
    public TextMeshProUGUI resultText;

    public bool playerInRange = false;
    private bool puzzleSolved = false;
    
    
    void Start()
    {
        if (GameState.Instance != null && GameState.Instance.Level4_PrinterSolved)
        {
            puzzleSolved = true;
            resultText.text = "Correct! You found the printed report clue.";
        }
    }

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (puzzleSolved)
                DialogueManager.Instance?.StartDialogue("", new[] { "You've already solved this." });
            else
                puzzlePanel.SetActive(true);
        }
    }

    public void CheckAnswer(int answerIndex)
    {
        int correctAnswer = 1; // 0 = first button, 1 = second button, 2 = third button

        if (answerIndex == correctAnswer)
        {
            resultText.text = "Correct! You found the printed report clue.";
            puzzleSolved = true;
            if (GameState.Instance != null)
            {
                GameState.Instance.Level4_PrinterSolved = true;
                GameState.Instance.CheckLevel4Complete();
            }
        }
        else
        {
            resultText.text = "Incorrect. Try again.";
        }
    }

    public void ClosePuzzle()
    {
        puzzlePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PrinterPuzzle : MonoBehaviour
{
    public GameObject puzzlePanel;
    public TextMeshProUGUI resultText;

    public bool playerInRange = false;
    private bool puzzleSolved = false;

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame && !puzzleSolved)
        {
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

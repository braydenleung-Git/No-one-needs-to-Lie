using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class ShreddedPaperPuzzle : MonoBehaviour
{
    public GameObject puzzlePanel;
    public TextMeshProUGUI selectedOrderText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI reconstructedClueText;

    private bool playerInRange = false;
    private bool puzzleSolved = false;

    private List<int> playerOrder = new List<int>();
    private int[] correctOrder = { 0, 2, 3, 1 };

    void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !puzzleSolved)
        {
            puzzlePanel.SetActive(true);
            playerOrder.Clear();
            selectedOrderText.text = "Selected Order: ";
            resultText.text = "";
            reconstructedClueText.text = "";
        }
    }

    public void SelectPiece(int pieceIndex)
    {
        if (playerOrder.Count < correctOrder.Length)
        {
            playerOrder.Add(pieceIndex);
            UpdateSelectedOrderText();
        }
    }

    void UpdateSelectedOrderText()
    {
        string display = "Selected Order: ";

        for (int i = 0; i < playerOrder.Count; i++)
        {
            display += (playerOrder[i] + 1).ToString();

            if (i < playerOrder.Count - 1)
            {
                display += " → ";
            }
        }

        selectedOrderText.text = display;
    }

    public void CheckOrder()
    {
        if (playerOrder.Count != correctOrder.Length)
        {
            resultText.text = "You must select all 4 pieces first.";
            return;
        }

        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (playerOrder[i] != correctOrder[i])
            {
                resultText.text = "Incorrect order. Try again.";
                return;
            }
        }

        resultText.text = "Correct! The document has been reconstructed.";
        reconstructedClueText.text = "Recovered Record: The office report was modified after the meeting.";
        puzzleSolved = true;
    }

    public void ResetOrder()
    {
        playerOrder.Clear();
        selectedOrderText.text = "Selected Order: ";
        resultText.text = "";
        reconstructedClueText.text = "";
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

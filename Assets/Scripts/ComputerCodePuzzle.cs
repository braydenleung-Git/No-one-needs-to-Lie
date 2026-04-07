using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ComputerCodePuzzle : MonoBehaviour
{
    public GameObject puzzlePanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI resultText;

    public bool playerInRange = false;
    private bool puzzleSolved = false;

    private string correctCode = "3264";

    void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !puzzleSolved)
        {
            puzzlePanel.SetActive(true);
            resultText.text = "";
            inputField.text = "";

            // Focus the input field automatically
            inputField.ActivateInputField();
        }
    }

    public void SubmitCode()
    {
        if (inputField.text == correctCode)
        {
            resultText.text = "Correct! You found the private office log.";
            puzzleSolved = true;
        }
        else
        {
            resultText.text = "Incorrect code.";
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

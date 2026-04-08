using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NotebookInteraction : MonoBehaviour
{
    public GameObject notebookPanel;
    public TextMeshProUGUI notebookText;

    [TextArea(5,10)]
    public string clueText;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            notebookPanel.SetActive(true);
            notebookText.text = clueText;
        }
    }

    public void CloseNotebook()
    {
        notebookPanel.SetActive(false);
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
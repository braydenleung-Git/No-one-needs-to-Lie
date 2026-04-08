using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NotebookInteraction : MonoBehaviour
{
    public GameObject notebookPanel;
    public TextMeshProUGUI notebookText;
    [SerializeField] private int notebookID; // set to 1, 2, 3, or 4 in inspector per notebook

    [TextArea(5,10)]
    public string clueText;

    private bool playerInRange = false;
    void Start()
    {
        if (GameState.Instance == null) return;
        // nothing visual to restore, notebook just needs to be marked
    }
   
    
    void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            notebookPanel.SetActive(true);
            notebookText.text = clueText;

            if (GameState.Instance != null)
            {
                switch (notebookID)
                {
                    case 1: GameState.Instance.Level4_Notebook1Viewed = true; break;
                    case 2: GameState.Instance.Level4_Notebook2Viewed = true; break;
                    case 3: GameState.Instance.Level4_Notebook3Viewed = true; break;
                    case 4: GameState.Instance.Level4_Notebook4Viewed = true; break;
                }
                GameState.Instance.CheckLevel4Complete();
            }
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
using UnityEngine;
using UnityEngine.SceneManagement;

// attach to an exit trigger inside each level scene
// takes the player back to the Town scene
public class ReturnToTown : MonoBehaviour
{
    [SerializeField] private string exitPrompt = "Press [E] to exit";

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractionPromptUI.Instance?.Show(exitPrompt, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractionPromptUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            SceneManager.LoadScene("Town");
    }
}
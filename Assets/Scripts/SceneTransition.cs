using UnityEngine;
using UnityEngine.SceneManagement;

// attach to any building entrance trigger in the Town scene
// player walks into it and gets transported to that level
public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneName; // exact scene name e.g. "Level3"
    [SerializeField] private string enterPrompt = "Press [E] to enter";

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractionPromptUI.Instance?.Show(enterPrompt, transform);
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
            SceneManager.LoadScene(sceneName);
    }
}
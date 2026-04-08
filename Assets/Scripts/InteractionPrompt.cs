using UnityEngine;
using TMPro;

// legacy prompt component used in LEVEL4 and other scenes
// previously managed its own UI panel - now delegates to the InteractionPromptUI singleton
// so it shows the same light-grey popup as every other interactable in the game
// the old promptUI / promptText fields are kept so existing scene references don't break
// but they are no longer used at runtime
public class InteractionPrompt : MonoBehaviour
{
    [HideInInspector] public GameObject promptUI;   // unused - kept so scene serialization doesn't break
    [HideInInspector] public TextMeshProUGUI promptText; // same ^

    [TextArea]
    public string message = "Press E";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // use the shared singleton popup instead of the old per-object UI panel
        InteractionPromptUI.Instance?.Show(message, transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractionPromptUI.Instance?.Hide();
    }
}

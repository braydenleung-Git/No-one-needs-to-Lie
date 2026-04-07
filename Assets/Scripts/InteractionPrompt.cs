using UnityEngine;
using TMPro; 

public class InteractionPrompt : MonoBehaviour
{
    public GameObject promptUI;
    public TextMeshProUGUI promptText;

    [TextArea]
    public string message = "Press E";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            promptUI.SetActive(true);
            promptText.text = message;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            promptUI.SetActive(false);
        }
    }
}

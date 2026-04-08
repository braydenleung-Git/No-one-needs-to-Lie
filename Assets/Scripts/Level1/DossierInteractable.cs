using UnityEngine;

public class DossierInteractable : Interactable
{
    [SerializeField] private DossierItem dossierItem;
    [SerializeField] private GameObject exit;
    
    [Header("Dialogue Settings")]
    [SerializeField] private string speakerName = "System";
    [SerializeField] private string[] dialogueLines = {
        "You've collected the dossier!",
        "Now you can exit through the door."
    };

    public override void Interact()
    {
        
        if (player != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // Add the item to inventory
                inventory.AddItem(dossierItem);
                
                // Trigger dialogue about exiting through the door
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(speakerName, dialogueLines);
                }
                
                // Hide the dossier object after collection, and enable the exit
                gameObject.SetActive(false);
                exit.SetActive(true);
                
            }
        }
    }
}

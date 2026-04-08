using UnityEngine;

public class NewCardSwiperInteractable : Interactable
{
    [SerializeField] private GameObject normal;
    [SerializeField] private GameObject unlockedDoor; 
    [SerializeField] private GameObject block;
    [SerializeField] private SwipeCardItem swipeCardItem;
    
    public override void Interact()
    {
        // Check if player has Officer ID in inventory
        bool hasOfficerId = CheckPlayerInventoryForOfficerId();
        
        if (!hasOfficerId)
        {
            // Prompt user to grab Officer ID from Chief's Office using dialogue system
            string[] dialogueLines = { "Access Denied, You need to grab the Officer ID from the Chief's Office first!" };
            DialogueManager.Instance.StartDialogue("System", dialogueLines);
        }
        else
        {
            unlockedDoor.SetActive(true);
            block.SetActive(false);
            normal.SetActive(false);
        }
    }
    
    private bool CheckPlayerInventoryForOfficerId()
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        return inventory.HasItem(swipeCardItem);
    }
}

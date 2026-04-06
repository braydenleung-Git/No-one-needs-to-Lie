using UnityEngine;

public class SwipeCardDropped : Interactable
{
    [SerializeField] private SwipeCardItem swipeCardItem;
    public override void Interact()
    {
        if (player != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // Add your item here
                inventory.AddItem(swipeCardItem);
            }
        }
        Destroy(gameObject);
    }
}

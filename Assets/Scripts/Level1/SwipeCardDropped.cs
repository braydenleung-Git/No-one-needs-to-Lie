using UnityEngine;

public class SwipeCardDropped : Interactable
{
    [SerializeField] private SwipeCardItem swipeCardItem;

    public override void Interact()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var inventory = player != null ? player.GetComponent<PlayerInventory>() : null;
        if (inventory != null && swipeCardItem != null)
            inventory.AddItem(swipeCardItem);

        Destroy(gameObject);
    }
}

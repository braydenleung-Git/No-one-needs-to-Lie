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
                
                // Add XP for picking up swipecard
                ExperienceManager.Instance.AddXP(20);
            }
        }
        gameObject.SetActive(false);
    }
    protected override void Start()
    {
        base.Start();
        Hint(true);
    }
}

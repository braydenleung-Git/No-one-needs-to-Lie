using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventory
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void AddItem(GameItem item)
    {
        
    }
    
    public void RemoveItem(GameItem item)
    {
        
    }

    public bool HasItem(GameItem item)
    {
        throw new System.NotImplementedException();
    }

    public List<GameItem> ListItems()
    {
        return null;
    }

    public void TransferItem(IInventory other, GameItem item)
    {
        
    }
    
    public void SelectItem(GameItem item)
    {
        
    }
}

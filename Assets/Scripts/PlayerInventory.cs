using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, Inventory
{
    private List<GameItem> _items = new List<GameItem>();

    void Start() { }
    void Update() { }

    public void AddItem(GameItem item)
    {
        if (item == null) return;
        _items.Add(item);
    }

    public void RemoveItem(GameItem item)
    {
        _items.Remove(item);
    }

    public bool HasItem(GameItem item)
    {
        return _items.Contains(item);
    }

    public List<GameItem> ListItems()
    {
        return _items;
    }

    public void TransferItem(Inventory other, GameItem item)
    {
        
    }
    
    public void SelectItem(GameItem item)
    {
        
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ChestInventory : MonoBehaviour, Inventory
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddItem(GameItem item)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveItem(GameItem item)
    {
        throw new System.NotImplementedException();
    }

    public bool HasItem(GameItem item)
    {
        throw new System.NotImplementedException();
    }

    public List<GameItem> ListItems()
    {
        throw new System.NotImplementedException();
    }

    public void TransferItem(Inventory other, GameItem item)
    {
        throw new System.NotImplementedException();
    }
}

using UnityEngine;
using System.Collections.Generic;

public class ChestInventory : MonoBehaviour, Inventory
{
    void Start() { }

    void Update() { }

    public void AddItem(GameItem item) { }

    public void RemoveItem(GameItem item) { }

    public bool HasItem(GameItem item) { return false; }

    public List<GameItem> ListItems() { return new List<GameItem>(); }

    public void TransferItem(Inventory other, GameItem item) { }
}
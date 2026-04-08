using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventory
{
    public static PlayerInventory Instance { get; private set; }

    private List<GameItem> _items = new List<GameItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PersistentGameItems.HydrateUvFlashlight(this);
    }

    public void AddItem(GameItem item)
    {
        if (item == null) return;

        if (!_items.Contains(item))
        {
            _items.Add(item);
        }
    }

    public void RemoveItem(GameItem item)
    {
        if (item == null) return;
        _items.Remove(item);
    }

    public bool HasItem(GameItem item)
    {
        if (item == null) return false;
        return _items.Contains(item);
    }

    public List<GameItem> ListItems()
    {
        return new List<GameItem>(_items);
    }

    public void TransferItem(IInventory other, GameItem item)
    {
        if (other == null || item == null) return;

        if (_items.Contains(item))
        {
            _items.Remove(item);
            other.AddItem(item);
        }
    }

    public void SelectItem(GameItem item)
    {
        
    }
}
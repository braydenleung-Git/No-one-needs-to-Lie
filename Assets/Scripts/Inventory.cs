using System.Collections.Generic;

public interface Inventory
{
    public void AddItem(GameItem item);
    public void RemoveItem(GameItem item);
    public bool HasItem(GameItem item);
    public List<GameItem> ListItems();
    public void TransferItem(Inventory other, GameItem item);
}

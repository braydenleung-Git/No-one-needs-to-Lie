using UnityEngine;

public class Cosmetics: Item, IEquipable
{
    private EquipPosition _equipPosition;
    public GameObject Owner { get; set; }
    public void Equip(EquipPosition position)
    {
        _equipPosition = position;
        transform.SetParent(Owner.transform); //set parent of the cosmetics to the Owner(NPC/Player)
    }

    public void Unequip()
    {
        _equipPosition = EquipPosition.None;
        transform.SetParent(null); //remove cosmetic from Owner
    }

    public EquipPosition GetEquipPosition()
    {
        return (EquipPosition)_equipPosition;
    }
}



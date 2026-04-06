using UnityEngine;
public interface IEquipable
{
    public GameObject Owner {get; set;}
    public void Equip(EquipPosition position);
    public void Unequip();
    public EquipPosition GetEquipPosition();
}
public enum EquipPosition {None, Head, Chest, Legs, Feet, Hands};
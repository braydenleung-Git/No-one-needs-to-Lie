using UnityEngine;
public abstract class Equipable: ScriptableObject
{
    public GameObject Owner {get; set;}
    public abstract void Equip(int position);
    public abstract void Unequip();
    public abstract void GetEquipPosition();
}

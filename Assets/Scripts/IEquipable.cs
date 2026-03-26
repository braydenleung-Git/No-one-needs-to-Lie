using UnityEngine;
public interface IEquipable
{
    public GameObject Owner {get; set;}
    public void Equip(int position);
    public void Unequip();
    public int GetEquipPosition();
}

using UnityEngine;

public class Cosmetics: Item, IEquipable
{
    private EquipPosition _equipPosition;
    public GameObject Owner { get; set; }
    public void Equip(EquipPosition position)
    {
        _equipPosition = position;
        transform.SetParent(Owner.transform); //set parent of the cosmetics to the Owner(NPC/Player)
        _transformToPlayer(_equipPosition);
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

    /// <summary>
    /// this method is used to move the cosmetic item relative to the player
    /// </summary>
    private void _transformToPlayer(EquipPosition position)
    {
        switch(position)
        {
            case EquipPosition.None:
                gameObject.SetActive(false);
                break;
            case EquipPosition.Head:
                gameObject.SetActive(true);
                transform.localPosition = Vector3.up * 0.2f;
                break;
            case EquipPosition.Chest:
                gameObject.SetActive(true);
                transform.localPosition = Vector3.up * 0.2f;
                break;
            case EquipPosition.Legs:
                gameObject.SetActive(true);
                transform.localPosition = Vector3.down * 0.2f;
                break;
            case EquipPosition.Feet:
                gameObject.SetActive(true);
                transform.localPosition = Vector3.down * 0.2f;
                break;
            case EquipPosition.Hands:
                transform.localPosition = Vector3.zero;
                break;
        }
        
    }
}



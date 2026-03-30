using UnityEngine;
/// <summary>
/// This Abstract class is meant for any item used in the game.
/// e.g dropped items would extend from this class
/// </summary>
public abstract class GameItem : MonoBehaviour, Hintable
{
    [Header("Item Info")]
    [field: SerializeField] public string ItemName { get; set; }
    [field: SerializeField] [TextArea]  public string Description { get; set; }
    [field: SerializeField] public Sprite ItemIcon { get; set; }
    
    
    public bool IsVisible { get; set; }
    public bool IsOutlined { get; set; }
    public bool IsHinted { get; set; }
    public Sprite HintIcon { get; set; }
}
using UnityEngine;

public interface Hintable
{
    bool IsHinted { get; set; }
    Sprite HintIcon { get; set; }
}

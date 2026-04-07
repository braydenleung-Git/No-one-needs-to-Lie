using UnityEngine;

public interface Hintable
{
    bool IsHinted { get; }
    Sprite HintIcon { get; set; }
    void Hint(bool isHinted);
}

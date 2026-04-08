using UnityEngine;

/// <summary>
/// Abstract accessory that can be applied to the detective across scenes.
/// Use concrete subclasses (e.g., hat) to implement attachment details.
/// </summary>
public abstract class DetectiveAccessory : ScriptableObject
{
    /// <summary>Apply the accessory to the detective GameObject.</summary>
    public abstract void Apply(GameObject detective);

    /// <summary>Remove the accessory from the detective GameObject.</summary>
    public abstract void Remove(GameObject detective);
}


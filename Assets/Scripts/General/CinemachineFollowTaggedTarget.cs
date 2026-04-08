using UnityEngine;

// Keeps a CinemachineCamera following the current Player (tag).
// Fixes broken scene references when the Player is swapped/replaced by prefab instances.
public class CinemachineFollowTaggedTarget : MonoBehaviour
{
    [SerializeField] string targetTag = "Player";
    [SerializeField] bool setLookAtToo = true;

    bool _applied;

    void OnEnable() => TryApply();
    void Start() => TryApply();

    void TryApply()
    {
        if (_applied) return;

        // Avoid a hard compile-time dependency on Cinemachine API shape by using reflection.
        var cam = GetComponent("Unity.Cinemachine.CinemachineCamera");
        if (cam == null) return;

        var player = GameObject.FindGameObjectWithTag(targetTag);
        if (player == null) return;

        var camType = cam.GetType();
        var targetProp = camType.GetProperty("Target");
        if (targetProp == null) return;

        object target = targetProp.GetValue(cam, null);
        if (target == null) return;

        var targetType = target.GetType();
        var trackingProp = targetType.GetProperty("TrackingTarget");
        if (trackingProp != null && trackingProp.CanWrite)
            trackingProp.SetValue(target, player.transform, null);

        if (setLookAtToo)
        {
            var lookAtProp = targetType.GetProperty("LookAtTarget");
            if (lookAtProp != null && lookAtProp.CanWrite)
                lookAtProp.SetValue(target, player.transform, null);
        }

        // If Target is a struct, we must write it back to apply changes.
        targetProp.SetValue(cam, target, null);
        _applied = true;
    }
}


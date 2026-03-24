using UnityEngine;

public interface IPatroller
{
    void StartChase(Transform target);
    void StopChase();
}
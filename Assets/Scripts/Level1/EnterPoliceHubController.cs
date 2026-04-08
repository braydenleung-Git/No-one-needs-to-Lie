using System;
using UnityEngine;

public class EnterPoliceHubController : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.GetComponent<Animator>().Play("EnterPoliceHub");
    }
}

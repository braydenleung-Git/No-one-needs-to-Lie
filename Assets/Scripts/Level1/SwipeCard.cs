using UnityEngine;

//Code derived from https://youtu.be/izag_ZHwOtM
//This code is meant for only making dragging possible on the card


public class SwipeCard : MonoBehaviour
{
    private bool dragging = false;
    private Vector3 offset;

    private void Update()
    {
        if (dragging)
        {
            transform.position  = Camera.main.ScreenToWorldPoint(Input.mousePosition)+offset;
        }
    }

    private void OnMouseDown()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragging = true;
    }

    private void OnMouseUp()
    {
        dragging = false;
    }
}

using UnityEngine;

public class SwipeCard : MonoBehaviour
{
    private bool _isDragging = false;
    private Vector3 _offset;
    
    private void Update()
    {
        if (_isDragging)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + _offset;
        }
    }
    
    private void OnMouseDown()
    {
        _isDragging = true;
        _offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    private void OnMouseUp()
    {
        _isDragging = false;
    }
}
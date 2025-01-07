using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupScrollViewItem : MonoBehaviour, IPointerUpHandler
{
    public ScrollRect scrollRect;  
    Canvas canvas;
    // Start is called before the first frame update
    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        // Position popup at the mouse position (convert screen point to UI point)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 pos
        );
        transform.position = canvas.transform.TransformPoint(pos);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        scrollRect.enabled = true;
    }
}

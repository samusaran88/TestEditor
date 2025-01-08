using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PopupScrollViewItem : MonoBehaviour
{
    public CustomScrollRect scrollRect;
    public TMP_Text text;
    Canvas canvas;
    // Start is called before the first frame update
    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        scrollRect.itemPopup = gameObject;
        gameObject.SetActive(false);
    } 

    // Update is called once per frame
    void Update()
    {
        if (scrollRect.sourceItem != null)
        {
            text.text = scrollRect.sourceItem.name;
        }
        // Position popup at the mouse position (convert screen point to UI point)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 pos
        );
        transform.position = canvas.transform.TransformPoint(pos);
    } 
}

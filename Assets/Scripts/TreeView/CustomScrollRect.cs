using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect, IPointerDownHandler, IPointerUpHandler
{
    public bool passDragToChildren = true;
    public UnityEvent<HierarchyItem, HierarchyItem> setToChildItem;
    public UnityEvent<HierarchyItem, HierarchyItem> setToPriorSiblingItem;
    public UnityEvent<HierarchyItem, HierarchyItem> setToNextSiblingItem;
    public GameObject itemPopup;
    public HierarchyItem sourceItem;
    private ScrollItemUI hoverItemUI;
    private Coroutine coroutinePendPopup;
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!passDragToChildren)
        {
            base.OnBeginDrag(eventData);  // Allow default scrolling if needed
        }
    }
    public override void OnDrag(PointerEventData eventData)
    {
        if (hoverItemUI != null) hoverItemUI.DeactivateAllHighlights();
        ScrollItemUI itemUI = GetTargetItemUI(eventData);  // Get the item UI where the drop happened
        if (itemUI != null)
        {
            RectTransform itemRect = itemUI.GetComponent<RectTransform>();  // Get RectTransform of the item
            Vector2 localPoint;

            // Convert screen position to local position within the item
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            float rectHeight = itemRect.rect.height;  // Height of the item's RectTransform
            float normalizedY = (localPoint.y + rectHeight / 2) / rectHeight;  // Normalize to 0 (bottom) to 1 (top) 

            // Check the drop zone: top, middle, or bottom
            if (normalizedY >= 0.8f)  // Upper 30%
            { 
                itemUI.ActivateUpperHighlight(); 
            }
            else if (normalizedY <= 0.2f)  // Lower 30%
            { 
                itemUI.ActivateLowerHighlight(); 
            }
            else  // Middle 40%
            { 
                itemUI.ActivateMidHighlight(); 
            }
            hoverItemUI = itemUI; 
        }
        if (!passDragToChildren)
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (hoverItemUI != null) hoverItemUI.DeactivateAllHighlights();
        StopCoroutine(coroutinePendPopup);
        itemPopup.SetActive(false);
        ScrollItemUI itemUI = GetTargetItemUI(eventData); 
        if (itemUI != null && sourceItem != null)
        {
            RectTransform itemRect = itemUI.GetComponent<RectTransform>();  // Get RectTransform of the item
            Vector2 localPoint;

            // Convert screen position to local position within the item
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            float rectHeight = itemRect.rect.height;  // Height of the item's RectTransform
            float normalizedY = (localPoint.y + rectHeight / 2) / rectHeight;  // Normalize to 0 (bottom) to 1 (top)

            hoverItemUI = itemUI;
            HierarchyItem destItem = itemUI.currentItem;

            if (destItem == null)
                return;

            if (normalizedY >= 0.8f)
            {
                setToPriorSiblingItem?.Invoke(sourceItem, destItem);
            }
            else if (normalizedY <= 0.2f)
            {
                setToNextSiblingItem?.Invoke(sourceItem, destItem); 
            }
            else
            {
                setToChildItem?.Invoke(sourceItem, destItem); 
            }
            sourceItem = null;
        }
        if (!passDragToChildren)
        {
            base.OnEndDrag(eventData);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        ScrollItemUI itemUI = GetTargetItemUI(eventData);
        coroutinePendPopup = StartCoroutine(CoroutinePendPopup());
        if (itemUI != null)
        {
            sourceItem = itemUI.currentItem;
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        StopCoroutine(coroutinePendPopup);
        itemPopup.SetActive(false);
    }
    private ScrollItemUI GetTargetItemUI(PointerEventData eventData)
    { 
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position  // Get mouse position at drag end
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);  // Raycast against UI elements

        foreach (var result in results)
        {
            ScrollItemUI itemUI = result.gameObject.GetComponent<ScrollItemUI>();
            if (itemUI != null)  // Exclude the dragged item itself
            {
                return itemUI;  // Return the first valid drop target
            }
        }

        return null;
    }
    private bool IsPointerOverScrollView()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(), Input.mousePosition, Camera.main
        );
    }
    IEnumerator CoroutinePendPopup()
    {
        yield return new WaitForSeconds(0.1f);
        itemPopup.SetActive(true);
        yield return null;
    }
}

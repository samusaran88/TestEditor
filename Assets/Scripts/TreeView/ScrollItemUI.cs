using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Unity.VisualScripting;

public class ScrollItemUI : MonoBehaviour, IPointerClickHandler//, IBeginDragHandler, IEndDragHandler
{
    public TMP_Text nameText;
    public Image iconImage;
    public Button expandButton;
    public RectTransform paddingRT;
    public Sprite spriteExpanded;
    public Sprite spriteCollapsed;
    public UnityEvent<HierarchyItem> onClickSingle;
    public UnityEvent<HierarchyItem> onClickAddSingle;
    public UnityEvent<HierarchyItem> onClickMultiple;
    public UnityEvent onToggleExpand;
    public UnityEvent<HierarchyItem> onDragStart;
    public UnityEvent<HierarchyItem> onDragEnd;
    public UnityEvent<PointerEventData> onScroll;
    public Color selectedColor;
    public HierarchyItem currentItem;
    public Image upperLine;
    public Image lowerLine;
    public Color lineColor;
    public GameObject highLight;
    private Image backgroundImage;
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }
    public int SetItemData(HierarchyItem item)
    {
        currentItem = item;
        nameText.text = item.name;
        currentItem.isExpanded = item.isExpanded;
        expandButton.image.sprite = currentItem.isExpanded == true ? spriteExpanded : spriteCollapsed;

        float width = nameText.GetComponent<RectTransform>().sizeDelta.x + iconImage.GetComponent<RectTransform>().sizeDelta.x;
        float paddingSize = item.layerNum * expandButton.GetComponent<RectTransform>().sizeDelta.x;
        if (item.children.Count > 0)
        {
            expandButton.gameObject.SetActive(true);
            width += expandButton.GetComponent<RectTransform>().sizeDelta.x;
        }
        else
        {
            expandButton.gameObject.SetActive(false);
            paddingSize += expandButton.GetComponent<RectTransform>().sizeDelta.x;
        }
        width += paddingSize;
        paddingRT.sizeDelta = new Vector2(paddingSize, paddingRT.sizeDelta.y);
        upperLine.GetComponent<RectTransform>().offsetMin = new Vector2(paddingSize, 0);
        lowerLine.GetComponent<RectTransform>().offsetMin = new Vector2(paddingSize, 0);
        return Mathf.RoundToInt(width);
        //RectTransform rectTransform = GetComponent<RectTransform>();
        //rectTransform.anchoredPosition = new Vector2(20 * depth, rectTransform.anchoredPosition.y);
    }
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = Color.clear;
        }
    }
    public void SetSelected(List<HierarchyItem> selectedItems)
    {
        if (selectedItems.Contains(currentItem))
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = Color.clear;
        }
    }
    public void ToggleExpandCollapse()
    {
        currentItem.isExpanded = !currentItem.isExpanded;
        expandButton.image.sprite = currentItem.isExpanded == true ? spriteExpanded : spriteCollapsed;
        onToggleExpand?.Invoke();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        { 
            onClickAddSingle?.Invoke(currentItem);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        { 
            onClickMultiple?.Invoke(currentItem);
        }
        else
        { 
            onClickSingle?.Invoke(currentItem);
        }
    }
    public void OnBeginDrag(BaseEventData data)
    {
        Debug.Log("DragStart " + currentItem.name);
        onDragStart?.Invoke(currentItem);
    }

    public void OnEndDrag()
    {
        Debug.Log("DragEnd " + currentItem.name);
        onDragEnd?.Invoke(currentItem);
    }
    public void OnEndDrag(BaseEventData data)
    {
        ScrollItemUI itemUI = GetDropTarget(data);
        if (itemUI != null)
            itemUI.OnEndDrag();
    }
    private ScrollItemUI GetDropTarget(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
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
    public void OnScroll(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        onScroll?.Invoke(eventData);
    }
    public void ActivateUpperHighlight()
    {
        upperLine.color = lineColor;
    }
    public void ActivateLowerHighlight()
    {
        lowerLine.color = lineColor;
        //if (currentItem == null) return;
        //if (currentItem.GetNextSibling() == null)
        //{
        //    lowerLine.color = lineColor;
        //}
        //else
        //{
        //    ActivateMidHighlight(); 
        //}
    }
    public void ActivateMidHighlight()
    {
        highLight.SetActive(true);
    }
    public void DeactivateAllHighlights()
    {
        upperLine.color = Color.clear;
        lowerLine.color = Color.clear;
        highLight.SetActive(false);
    }
}

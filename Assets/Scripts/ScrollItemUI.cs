using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollItemUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text nameText;
    public Image iconImage;
    public Button expandButton;
    public RectTransform paddingRT;
    public Sprite spriteExpanded;
    public Sprite spriteCollapsed;
    public UnityEvent<HierarchyItem> onClickSingle;
    public UnityEvent<HierarchyItem> onClickMultiple;
    public UnityEvent onToggleExpand;
    public Color selectedColor;
    private HierarchyItem currentItem;
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
        return Mathf.RoundToInt(width);
        //RectTransform rectTransform = GetComponent<RectTransform>();
        //rectTransform.anchoredPosition = new Vector2(20 * depth, rectTransform.anchoredPosition.y);
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
            onClickMultiple?.Invoke(currentItem);
        }
        else
        {
            onClickSingle?.Invoke(currentItem);
        }
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
}
public class HierarchyItem
{
    public string name;
    public int layerNum;
    public HierarchyItem parent;
    public List<HierarchyItem> children;
    public bool isExpanded = true; 
    public HierarchyItem(string name)
    {
        this.name = name;
        children = new List<HierarchyItem>();
    }
    public void AddChild(HierarchyItem item)
    {
        children.Add(item);
        item.parent = this;
        item.layerNum = layerNum + 1;
    }
    public List<HierarchyItem> GetAll()
    {
        List<HierarchyItem> items = new List<HierarchyItem>();
        HierarchyItem node = GetNext();
        while (node != null)
        {
            items.Add(node);
            node = node.GetNext();
        } 
        return items;
    }
    public HierarchyItem GetNext()
    {
        if (children.Count > 0 && isExpanded) return children[0];
        return parent.GetNextSibling(this);
    }
    public HierarchyItem GetNextSibling(HierarchyItem item)
    {
        int index = children.FindIndex(x => x == item);
        if (index == children.Count - 1)
        {
            if (parent == null) return null;
            return parent.GetNextSibling(this);
        }
        return children[index + 1];
    }
}
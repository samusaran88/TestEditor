using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image img;
    public Button expandButton;
    public RectTransform paddingRT;
    public Sprite spriteExpanded;
    public Sprite spriteCollapsed;
    private HierarchyItem currentItem; 
    public int SetItemData(HierarchyItem item)
    {
        currentItem = item;
        nameText.text = item.Name;
        currentItem.IsExpanded = item.IsExpanded;
        expandButton.image.sprite = currentItem.IsExpanded == true ? spriteExpanded : spriteCollapsed;

        float width = nameText.GetComponent<RectTransform>().sizeDelta.x + img.GetComponent<RectTransform>().sizeDelta.x;
        float paddingSize = item.layerNum * expandButton.GetComponent<RectTransform>().sizeDelta.x;
        if (item.Children.Count > 0)
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
        currentItem.IsExpanded = !currentItem.IsExpanded;
        expandButton.image.sprite = currentItem.IsExpanded == true ? spriteExpanded : spriteCollapsed;
    }
}
public class HierarchyItem
{
    public string Name;
    public int layerNum;
    public List<HierarchyItem> Children;
    public bool IsExpanded = false; 
    public HierarchyItem(string name)
    {
        Name = name;
        Children = new List<HierarchyItem>();
    }
}
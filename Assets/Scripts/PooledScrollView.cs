using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PooledScrollView : MonoBehaviour
{
    public RectTransform content;       // Content RectTransform of the ScrollView.
    public ScrollRect scrollRect;       // ScrollRect component.
    public GameObject itemPrefab;       // Prefab for scroll items.
    private ObjectPool<ScrollItemUI> pool;  // Pool for UI items.
    private List<HierarchyItem> data;   // The list of hierarchy items.

    private List<ScrollItemUI> activeItems = new List<ScrollItemUI>();

    private float itemHeight;           // Height of a single item.
    private int visibleItemCount;       // Number of visible items that fit in the viewport.
    private int currentIndex = -1;       // The index of the first visible item. 

    private void Start()
    {
        pool = new ObjectPool<ScrollItemUI>(itemPrefab.GetComponent<ScrollItemUI>(), 20);
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;

        // Calculate how many items can fit in the viewport at once.
        visibleItemCount = Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.y / itemHeight) + 1;

        List<HierarchyItem> hierarchyData = new List<HierarchyItem>();
        for (int i = 0; i < 100; i++)
        {
            hierarchyData.Add(new HierarchyItem("Item " + i.ToString()));
        } 

        SetData(hierarchyData);
    }
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        HierarchyItem rootItem = new HierarchyItem("Root");
    //        SetData(new List<HierarchyItem> { rootItem });
    //        for (int i = 0; i < 100; i++)
    //        {
    //            HierarchyItem newItem = new HierarchyItem("New Child");
    //            AddItem(rootItem, newItem);
    //        }
    //        //HierarchyItem anotherRootItem = new HierarchyItem("Another Root");
    //        //AddItem(null, anotherRootItem);
    //    }
    //}

    public void SetData(List<HierarchyItem> hierarchyData)
    {
        data = hierarchyData;
        RebuildVisibleItems();
    }

    private void OnScrollValueChanged(Vector2 scrollPos)
    {
        RebuildVisibleItems();
    }

    private void RebuildVisibleItems()
    {
        float scrollY = content.anchoredPosition.y;
        int newIndex = Mathf.FloorToInt(scrollY / itemHeight);
        if (newIndex < 0)
        {
            scrollY = 0;
            newIndex = 0;
        }

        // If the visible index hasn't changed, do nothing.
        if (newIndex == currentIndex) return;

        currentIndex = newIndex;

        // Clear and recycle active items.
        foreach (var item in activeItems)
        {
            pool.ReturnToPool(item);
        }
        activeItems.Clear();

        int startIndex = Mathf.Clamp(currentIndex, 0, data.Count - 1);
        int endIndex = Mathf.Clamp(currentIndex + visibleItemCount, 0, data.Count);

        int maxWidth = Mathf.RoundToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.x);
        for (int i = startIndex; i < endIndex; i++)
        {
            ScrollItemUI itemUI = pool.Get();
            int w = itemUI.SetItemData(data[i]);
            if (w > maxWidth) maxWidth = w;
            itemUI.transform.SetParent(content, false);

            float yPos = - (i + 1) * itemHeight; 
            itemUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);

            activeItems.Add(itemUI);
        }

        content.sizeDelta = new Vector2(maxWidth, (data.Count + 1) * itemHeight);  // Set content height.
        Debug.Log(maxWidth);
    }
}

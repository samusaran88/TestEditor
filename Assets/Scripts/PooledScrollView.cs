using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PooledScrollView : MonoBehaviour
{
    public RectTransform content;       // Content RectTransform of the ScrollView.
    public CustomScrollRect scrollRect;       // ScrollRect component.
    public GameObject itemPrefab;       // Prefab for scroll items.
    public GameObject itemPopup;
    public Canvas canvas;

    private ObjectPool<ScrollItemUI> pool;  // Pool for UI items.
    private List<HierarchyItem> data = new List<HierarchyItem>();   // The list of hierarchy items.
    private HierarchyItem root = new HierarchyItem("Root");

    private List<ScrollItemUI> activeItems = new List<ScrollItemUI>();
    private List<HierarchyItem> selectedItems = new List<HierarchyItem>();

    private float itemHeight;           // Height of a single item.
    private int visibleItemCount;       // Number of visible items that fit in the viewport.
    private int currentIndex = -1;       // The index of the first visible item. 

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        //스크롤 뷰에 사용될 오브젝트 풀을 미리 20개 생성
        pool = new ObjectPool<ScrollItemUI>(itemPrefab.GetComponent<ScrollItemUI>(), 20); 
        //스크롤뷰의 값이 변할때마다 내용물 갱신해주는 함수 콜백 추가
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        scrollRect.setToChildItem.AddListener(SetToChildItem);
        scrollRect.setToPriorSiblingItem.AddListener(SetToPriorSiblingItem);
        scrollRect.setToNextSiblingItem.AddListener(SetToNextSiblingItem);
        //아이템 하나당 높이값 저장
        itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height; 
        //뷰포트에 몇개의 아이템이 보이는지 한개의 여유분을 두고 계산
        visibleItemCount = Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.y / itemHeight) + 1;

        //테스트용 코드 일단 100개 테스트
        //List<HierarchyItem> hierarchyData = new List<HierarchyItem>();
        //for (int i = 0; i < 10; i++)
        //{
        //    hierarchyData.Add(new HierarchyItem("Item " + i.ToString()));
        //}  
        //SetData(hierarchyData);

        int i = 0;
        HierarchyItem tempParent = new HierarchyItem("Item" + i++);
        root.AddChild(tempParent);
        HierarchyItem tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempParent = tempChild;
        for (int k = 0; k < 1000; k++)
        {
            tempChild = new HierarchyItem("Item" + i++);
            tempParent.AddChild(tempChild);
        }
        //tempChild = new HierarchyItem("Item" + i++);
        //tempParent.AddChild(tempChild);
        //tempChild = new HierarchyItem("Item" + i++);
        //tempParent.AddChild(tempChild);
        SetData();
    }
    public void SetData()
    {
        data.Clear();
        data = root.GetAll();
        RebuildVisibleItems();
    }
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
            HierarchyItem item = data[i];
            ScrollItemUI itemUI = GetItemUI();
            int w = itemUI.SetItemData(item);
            itemUI.SetSelected(selectedItems); 
            if (w > maxWidth) maxWidth = w;
            itemUI.transform.SetParent(content, false);

            float yPos = -((float)i + 0.5f) * itemHeight; 
            itemUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);

            activeItems.Add(itemUI);
        }

        content.sizeDelta = new Vector2(maxWidth, data.Count * itemHeight);  // Set content height. 
    }
    private ScrollItemUI GetItemUI()
    {
        ScrollItemUI itemUI = pool.Get();
        itemUI.onClickSingle.RemoveAllListeners();
        itemUI.onClickSingle.AddListener(OnSelectSingle);
        itemUI.onClickAddSingle.RemoveAllListeners();
        itemUI.onClickAddSingle.AddListener(OnSelectAnother);
        itemUI.onClickMultiple.RemoveAllListeners();
        itemUI.onClickMultiple.AddListener(OnSelectMultiple);
        itemUI.onToggleExpand.RemoveAllListeners();
        itemUI.onToggleExpand.AddListener(OnToggleExpand);
        //itemUI.onDragStart.RemoveAllListeners();
        //itemUI.onDragStart.AddListener(OnDragStart);
        //itemUI.onDragEnd.RemoveAllListeners();
        //itemUI.onDragEnd.AddListener(OnDragEnd);
        //itemUI.onScroll.RemoveAllListeners();
        //itemUI.onScroll.AddListener(scrollRect.OnScroll);
        return itemUI;
    }
    public void OnSelectSingle(HierarchyItem item)
    { 
        if (selectedItems.Count == 1 && selectedItems[0] == item)
        {
            selectedItems.Clear(); 
        }
        else
        {
            selectedItems.Clear();
            selectedItems.Add(item);
        }
        foreach (var itemUI in activeItems)
        {
            itemUI.SetSelected(selectedItems);
        }
    }
    public void OnSelectAnother(HierarchyItem item)
    {
        if (selectedItems.Count == 1 && selectedItems[0] == item)
        {
            selectedItems.Clear(); 
        }
        else if (selectedItems.Count > 1 && selectedItems.Contains(item))
        {
            selectedItems.Remove(item);
        }
        else
        { 
            selectedItems.Add(item); 
        } 
        foreach (var itemUI in activeItems)
        {
            itemUI.SetSelected(selectedItems);
        }
    }
    public void OnSelectMultiple(HierarchyItem item)
    {
        int lowestIndex = data
            .Select((data, index) => new { data, index })
            .Where(pair => selectedItems.Contains(pair.data))
            .Min(pair => pair.index);
        int currentIndex = data.FindIndex(x => x == item);
        for (int i = lowestIndex; i <= currentIndex; i++)
        {
            HierarchyItem tempItem = data[i];
            if (!selectedItems.Contains(tempItem))
                selectedItems.Add(tempItem);
        }
        foreach (var itemUI in activeItems)
        {
            itemUI.SetSelected(selectedItems);
        }
    }
    public void OnToggleExpand()
    {
        currentIndex = -1;
        SetData();
    }
    public void OnDragStart(HierarchyItem item)
    {
        // Disable the scroll when popup is active
        scrollRect.enabled = false; 

        // Set popup as selected (focus)
        //EventSystem.current.SetSelectedGameObject(itemPopup);

        // Enable popup
        //itemPopup.SetActive(true);
    }
    public void OnDragEnd(HierarchyItem item)
    {
        // Disable the scroll when popup is active
        scrollRect.enabled = true;

        // Set popup as selected (focus)
        //EventSystem.current.SetSelectedGameObject(itemPopup);

        // Enable popup
        //itemPopup.SetActive(true);
    }
    public void SetToChildItem(HierarchyItem srcItem, HierarchyItem destItem)
    {
        if (srcItem == null || destItem == null)
            return;
        if (selectedItems.Contains(srcItem))
        {
            for (int i = 0; i < selectedItems.Count; i++)
            {
                HierarchyItem selectedItem = selectedItems[i];
                if (selectedItem == destItem)
                    continue;
                if (root.RemoveItem(selectedItem) == false)
                {
                    Debug.LogError("Failed to remove item : " + srcItem.name);
                    return;
                }
                destItem.AddChild(selectedItem);
            }
        }
        else
        { 
            if (srcItem == destItem)
                return;
            if (root.RemoveItem(srcItem) == false)
            {
                Debug.LogError("Failed to remove item : " + srcItem.name);
                return;
            }
            destItem.AddChild(srcItem);
        }
        data.Clear();
        data = root.GetAll();
        currentIndex = -1;
        RebuildVisibleItems();
    }
    public void SetToNextSiblingItem(HierarchyItem srcItem, HierarchyItem destItem)
    {
        if (srcItem == null || destItem == null)
            return;
        if (selectedItems.Contains(srcItem))
        {
            for (int i = 0; i < selectedItems.Count; i++)
            {
                HierarchyItem selectedItem = selectedItems[i];
                if (selectedItem == destItem)
                    continue;
                if (root.RemoveItem(selectedItem) == false)
                {
                    Debug.LogError("Failed to remove item : " + srcItem.name);
                    return;
                }
                destItem.AddToNextSibling(selectedItem);
            }
        }
        else
        {
            if (srcItem == destItem)
                return;
            if (root.RemoveItem(srcItem) == false)
            {
                Debug.LogError("Failed to remove item : " + srcItem.name);
                return;
            }
            destItem.AddToNextSibling(srcItem);
        }
        data.Clear();
        data = root.GetAll();
        currentIndex = -1;
        RebuildVisibleItems();
    }
    public void SetToPriorSiblingItem(HierarchyItem srcItem, HierarchyItem destItem)
    {
        if (srcItem == null || destItem == null)
            return;
        if (selectedItems.Contains(srcItem))
        {
            for (int i = 0; i < selectedItems.Count; i++)
            {
                HierarchyItem selectedItem = selectedItems[i];
                if (selectedItem == destItem)
                    continue;
                if (root.RemoveItem(selectedItem) == false)
                {
                    Debug.LogError("Failed to remove item : " + srcItem.name);
                    return;
                }
                destItem.AddToPriorSibling(selectedItem);
            }
        }
        else
        {
            if (srcItem == destItem)
                return;
            if (root.RemoveItem(srcItem) == false)
            {
                Debug.LogError("Failed to remove item : " + srcItem.name);
                return;
            }
            destItem.AddToPriorSibling(srcItem);
        }
        data.Clear();
        data = root.GetAll();
        currentIndex = -1;
        RebuildVisibleItems();
    }
}

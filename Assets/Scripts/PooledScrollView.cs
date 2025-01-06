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
    private List<HierarchyItem> data = new List<HierarchyItem>();   // The list of hierarchy items.
    private HierarchyItem root = new HierarchyItem("Root");

    private List<ScrollItemUI> activeItems = new List<ScrollItemUI>();
    private List<HierarchyItem> selectedItems = new List<HierarchyItem>();

    private float itemHeight;           // Height of a single item.
    private int visibleItemCount;       // Number of visible items that fit in the viewport.
    private int currentIndex = -1;       // The index of the first visible item. 

    private void Start()
    { 
        //��ũ�� �信 ���� ������Ʈ Ǯ�� �̸� 20�� ����
        pool = new ObjectPool<ScrollItemUI>(itemPrefab.GetComponent<ScrollItemUI>(), 20);
        //�� �����ۿ� ��Ŭ�� �ݹ� �Ҵ�
        foreach (var itemUI in pool.GetAll())
        {
            itemUI.onClickSingle.AddListener(OnSelectSingle);
            itemUI.onClickMultiple.AddListener(OnSelectMultiple);
            itemUI.onToggleExpand.AddListener(OnToggleExpand);
        }
        //��ũ�Ѻ��� ���� ���Ҷ����� ���빰 �������ִ� �Լ� �ݹ� �߰�
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        //������ �ϳ��� ���̰� ����
        itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height; 
        //����Ʈ�� ��� �������� ���̴��� �Ѱ��� �������� �ΰ� ���
        visibleItemCount = Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.y / itemHeight) + 1;

        //�׽�Ʈ�� �ڵ� �ϴ� 100�� �׽�Ʈ
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
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
        tempChild = new HierarchyItem("Item" + i++);
        tempParent.AddChild(tempChild);
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

        int maxWidth = Mathf.RoundToInt(scrollRect.GetComponent<RectTransform>().sizeDelta.x) - 20;
        for (int i = startIndex; i < endIndex; i++)
        {
            HierarchyItem item = data[i];
            ScrollItemUI itemUI = pool.Get();
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
    public void OnSelectSingle(HierarchyItem item)
    { 
        selectedItems.Clear();
        selectedItems.Add(item);
        foreach (var itemUI in activeItems)
        {
            itemUI.SetSelected(selectedItems);
        }
    }
    public void OnSelectMultiple(HierarchyItem item)
    { 
        selectedItems.Add(item);
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
}

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
        //스크롤 뷰에 사용될 오브젝트 풀을 미리 20개 생성
        pool = new ObjectPool<ScrollItemUI>(itemPrefab.GetComponent<ScrollItemUI>(), 20);
        //각 아이템에 온클릭 콜백 할당
        foreach (var itemUI in pool.GetAll())
        {
            itemUI.onClickSingle.AddListener(OnSelectSingle);
            itemUI.onClickMultiple.AddListener(OnSelectMultiple);
            itemUI.onToggleExpand.AddListener(OnToggleExpand);
        }
        //스크롤뷰의 값이 변할때마다 내용물 갱신해주는 함수 콜백 추가
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
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

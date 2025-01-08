using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battlehub.UIControls;
using TMPro;
using System.Linq;

public class StartTreeView : MonoBehaviour
{
    private VirtualizingTreeView m_treeView; 
    //root level data items
    private List<DataItem> m_items; 

    void Start()
    {
        m_treeView = GetComponent<VirtualizingTreeView>(); 
        //This event fired for each item that becomes visible
        m_treeView.ItemDataBinding += OnItemDataBinding; 
        //This event is fired for each expanded item
        m_treeView.ItemExpanding += OnItemExpanding; 
        //This event is triggered for each item after drag & drop
        m_treeView.ItemDrop += OnItemDrop;

        m_treeView.SelectionChanged += OnSelectionChanged;

        //Create data items 
        m_items = new List<DataItem>();

        //for (int i = 0; i < 10; i++)
        //{
        //    DataItem p = new DataItem("Data Item " + i * 1000);
        //    for (int k = 0; k < 100; k++)
        //    {
        //        p.SetChild(new DataItem("Data Item " + k));
        //    }
        //    m_items.Add(p);
        //}

        for (int i = 0; i < 1000; ++i)
        {
            m_items.Add(new DataItem("Data Item " + i));
        }

        //DataItem item = new DataItem("Data Item " + 0);
        //DataItem parent = item;
        //for (int i = 1; i < 1000; ++i)
        //{
        //    DataItem child = new DataItem("Data Item " + i);
        //    parent.SetChild(child);
        //    parent = child;
        //}
        //m_items.Add(item);

        //Populate tree view with data items
        m_treeView.Items = m_items;
    }

    void OnDestroy()
    {
        if (m_treeView != null)
        {
            m_treeView.ItemDataBinding -= OnItemDataBinding;
            m_treeView.ItemExpanding -= OnItemExpanding;
            m_treeView.ItemDrop -= OnItemDrop;
        }
    }

    void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
    {
        DataItem item = (DataItem)e.Item;

        //Get the controls from ItemsPresenter and copy the data into them.
        TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
        text.text = item.Name;

        Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
        icon.sprite = Resources.Load<Sprite>("IconNew");

        //Notify the tree of the presence of child data items.
        e.HasChildren = item.Children.Count > 0;
    }

    void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
    {
        DataItem item = (DataItem)e.Item;

        //Return children to the tree view
        e.Children = item.Children;
    }

    void OnItemDrop(object sender, ItemDropArgs args)
    {
        if (args.DropTarget == null)
        {
            return;
        }

        //Handle ItemDrop event using standard handler.
        m_treeView.ItemDropStdHandler<DataItem>(args,
            (item) => item.Parent,
            (item, parent) => item.Parent = parent,
            (item, parent) => ChildrenOf(parent).IndexOf(item),
            (item, parent) => ChildrenOf(parent).Remove(item),
            (item, parent, i) => ChildrenOf(parent).Insert(i, item));
    }
    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
#if UNITY_EDITOR
        List<DataItem> tempList = new List<DataItem>(e.NewItems.OfType<DataItem>().ToArray());
        foreach (DataItem item in tempList)
        {
            foreach (DataItem child in item.Children)
                Debug.Log(child.Name);
        }
#endif
    }

    List<DataItem> ChildrenOf(DataItem parent)
    {
        if (parent == null)
        {
            return m_items;
        }
        return parent.Children;
    }
}
public class DataItem
{
    public string Name;


    public DataItem Parent;
    public List<DataItem> Children;

    public DataItem(string name)
    {
        Name = name;
        Children = new List<DataItem>();
    }

    public override string ToString()
    {
        return Name;
    }
    public void SetChild(DataItem item)
    {
        item.Parent = this;
        Children.Add(item);
    }
}
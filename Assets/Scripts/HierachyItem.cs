using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public HierarchyItem(string name, int layerNum)
    {
        this.name = name;
        this.layerNum = layerNum;
        children = new List<HierarchyItem>();
    }
    public void AddChild(HierarchyItem item, int index = 0)
    {
        if (index == 0 || index >= children.Count) children.Add(item);
        else children.Insert(index, item);
        item.parent = this;
        item.ResetLayerNum();
    }
    public void AddToPriorSibling(HierarchyItem item)
    {
        if (parent == null) return;
        int index = parent.children.FindIndex(x => x == this);
        parent.children.Insert(index, item);
        item.parent = parent;
        item.ResetLayerNum();
    }
    public void AddToNextSibling(HierarchyItem item)
    {
        if (parent == null) return;
        int index = parent.children.FindIndex(x => x == this);
        if (index == parent.children.Count - 1)
        {
            parent.AddChild(item);
        }
        else
        {
            parent.children.Insert(index + 1, item);
        }
        item.parent = parent;
        item.ResetLayerNum();
    }
    public bool RemoveItem(HierarchyItem item)
    {
        if (children.Contains(item))
        {
            children.Remove(item);
            return true;
        }
        foreach (HierarchyItem childItem in children)
        {
            if (childItem.RemoveItem(item))
                return true;
        } 
        return false;
    }
    public void ResetLayerNum()
    {
        layerNum = parent.layerNum + 1;
        foreach (HierarchyItem childItem in children)
        {
            childItem.ResetLayerNum();
        }
    }
    public List<HierarchyItem> GetAll(bool includeCollapsed = false)
    {
        List<HierarchyItem> items = new List<HierarchyItem>();
        HierarchyItem node = GetNext(includeCollapsed);
        while (node != null && node.layerNum != layerNum)
        {
            items.Add(node);
            node = node.GetNext(includeCollapsed);
        }
        return items;
    }
    public HierarchyItem GetNext(bool includeCollapsed = false)
    {
        if (children.Count > 0 && (isExpanded || includeCollapsed)) return children[0];
        return parent.GetNext(this);
    }
    public HierarchyItem GetNext(HierarchyItem item)
    {
        int index = children.FindIndex(x => x == item);
        if (index == children.Count - 1)
        {
            if (parent == null) return null;
            return parent.GetNext(this);
        }
        return children[index + 1];
    }
    public HierarchyItem GetNextSibling()
    {
        if (parent == null) return null;
        int index = GetSiblingIndex();
        if (index == parent.children.Count - 1)
        {
            return null;
        }
        return parent.children[index + 1];
    }
    public bool HasChildItem(HierarchyItem item)
    {
        return GetAll(false).Contains(item);
    }
    public int GetSiblingIndex()
    {
        return parent.children.FindIndex(x => x == this);
    }
}

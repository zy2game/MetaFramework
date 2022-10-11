using System;
using System.Collections.Generic;
using UnityEngine;


public class UGUITabGroup : UGUIBaseTabGroup
{
    [SerializeField]
    [HideInInspector]
    private UGUITab currentSelect;
    private Action<int> onSelected;
    public bool isStartNotify = false;
    public bool isAutoCreate = false;
    public int autoCreateCount = 0;
    public UGUITab tab;
    private List<UGUITab> tabs;

    public void Create()
    { 
        if (!isAutoCreate)
            return;
        if (autoCreateCount == 0 || autoCreateCount == 1)
            return;
        tabs = new List<UGUITab>();
        for (int i = 0; i < autoCreateCount; i++)
        {
            CreateTab(i);
        }
    }

    public override void SetTabOn(UGUITab tab)
    {
        if (tab.isOn)
            return;
        if (currentSelect != null)
        {
            if (currentSelect.Equals(tab))
            {
                tab.SetSelect(true);
                Notify(tab);
                return;
            }
            currentSelect.SetSelect(false);
        }
        tab.SetSelect(true);
        currentSelect = tab;
        Notify(tab);
    }

    public void OnSelected(Action<int> callback)
    {
        onSelected = callback;
        if (isStartNotify && currentSelect != null)
            onSelected(currentSelect.intParam);
    }

    private void Notify(UGUITab tab)
    {
        if (onSelected != null)
            onSelected(tab.intParam);
    }

    public int GetCurrentSelect()
    {
        if (currentSelect == null)
            return -1;
        return currentSelect.intParam;
    }


    private void CreateTab(int index)
    {
        if (tab == null)
            return;
        UGUITab t = Instantiate(tab);
        t.transform.SetParent(tab.transform.parent);
        t.transform.localEulerAngles = Vector3.zero;
        t.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        t.transform.localScale = Vector3.one;
        t.gameObject.SetActive(true);
        if (tab.gameObject.activeSelf)
            tab.gameObject.SetActive(false);
        t.group = this;
        t.intParam = index;
        t.isOn = index == 0;
        tabs.Add(t);
    }

    public void SelectIndex(int index)
    {
        if (tabs == null)
            return;
        if (index >= tabs.Count)
            return;
        tabs[index].isOn = true;
    }
 }
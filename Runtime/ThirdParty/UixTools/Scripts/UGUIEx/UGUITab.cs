using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UGUITab : MonoBehaviour
{
    public GameObject obj_deselect;
    public GameObject obj_select;
    public UGUIBaseTabGroup group;
    public int intParam;
    [SerializeField]
    private bool isSelect;
    public bool isOn
    {
        get { return isSelect; }
        set
        {
            //if (group != null && value)
            //    group.SetTabOn(this);
            //else if (group == null)
            //    SetSelect(value);
            if (group != null)
            {
                if (!group.IsMultiple)
                {
                    if (value)
                        group.SetTabOn(this);
                }
                else
                {
                    SetSelect(value);
                    group.SetTabOn(this);
                }
            }
            else
            {
                SetSelect(value);
            }
        }
    }
    public bool isNotify = true;

    public List<GameObject> selectShowObj;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (group != null)
        {
            if (!group.IsMultiple)
            {
                if (!isSelect)
                    group.SetTabOn(this);
            }
            else
            {
                SetSelect(!isSelect);
                group.SetTabOn(this);
            }
        }
        else
        {
            SetSelect(!isSelect);
        }
    }

    public void SetSelect(bool b)
    {
        if (obj_deselect)
            obj_deselect.SetActive(!b);
        if (obj_select)
            obj_select.SetActive(b);
        if (selectShowObj != null && selectShowObj.Count > 0)
        {
            foreach (GameObject obj in selectShowObj)
            {
                if (obj)
                    obj.SetActive(b);
            }
        }

        isSelect = b;

        if (onChange != null && isNotify)
            onChange(b);
    }

    private Action<bool> onChange;
    public void OnChangeValue(Action<bool> func)
    {
        onChange = func;
    }

    private void OnDestroy()
    {
        if (onChange != null)
        {
            onChange = null;
        }
    }
}

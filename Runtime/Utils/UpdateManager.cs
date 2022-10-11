using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : SingletonBehaviour<UpdateManager>
{
    private int id = 0;
    private Dictionary<int, Action> map = new Dictionary<int, Action>();
    private List<Action> list = new List<Action>();
    private List<Action> addTemp = new List<Action>();
    private List<Action> delTemp = new List<Action>();

    public int RegUpdate(Action func)
    {
        if (list.Contains(func) || addTemp.Contains(func)) return -1;
        id++;
        map.Add(id, func);
        addTemp.Add(func);
        return id;
    }

    public bool Remove(int key)
    {
        if (!map.ContainsKey(key)) return false;
        Action func = map[key];
        map.Remove(key);
        delTemp.Add(func);
        return true;
    }

    public bool Remove(Action func)
    {
        if (addTemp.Contains(func))
            addTemp.Remove(func);
        int key = 0;
        foreach (var v in map)
        {
            if (v.Value == func)
            {
                key = v.Key;
                break;
            }
        }
        return Remove(key);
    }

    private void Update()
    {
        if (addTemp.Count > 0)
        {
            list.AddRange(addTemp);
            addTemp.Clear();
        }

        if (delTemp.Count > 0)
        {
            foreach (var v in delTemp)
            {
                list.Remove(v);
            }
            delTemp.Clear();
        }

        foreach (var v in list)
        {
            v();
        }
    }

}

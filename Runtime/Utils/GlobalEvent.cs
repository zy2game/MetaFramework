using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvent: GlobalEvent<object>
{
    public static void Notify(string key)
    {
        Notify(key, null);
    }
}

public class GlobalEvent<T>
{
    private static Dictionary<string, List<Action<T>>> map = new Dictionary<string, List<Action<T>>>();

    public static void AddEvent(string key,Action<T> func)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("key 不能为空");
            return;
        }

        List<Action<T>> list = null;
        if (!map.ContainsKey(key))
        {
            list = new List<Action<T>>();
            map.Add(key, list);
        }
        else
        {
            list = map[key];
        }
        if (list.Contains(func))
        {
            Debug.LogError("已添加对应的事件方法");
            return;
        }
        list.Add(func);
    }

    public static void Notify(string key,T t)
    {
        List<Action<T>> list = null;
        if (!map.TryGetValue(key,out list))
            return;
        foreach (var v in list)
        {
            v(t);
        }
    }

    public static bool Remove(string key)
    {
        return map.Remove(key);
    }

    public static bool Remove(string key,Action<T> func)
    {
        List<Action<T>> list = null;
        if (!map.TryGetValue(key, out list))
            return false;
        return list.Remove(func);
    }

    public static void ClearAll()
    {
        map.Clear();
    }
}

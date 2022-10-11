using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 协程管理器
/// </summary>
public class CorManager : SingletonBehaviour<CorManager>
{
    private Dictionary<object, Dictionary<int, Coroutine>> map = new Dictionary<object, Dictionary<int, Coroutine>>();
    private int cor_id = 0;

    /// <summary>
    /// 开启一个协程
    /// </summary>
    /// <param name="obj">对象标识，当前开启协程的对象</param>
    /// <param name="enumerator">协程</param>
    /// <returns>当前协程id</returns>
    public int StartCor(object obj, IEnumerator enumerator)
    {
        if (obj == null) obj = this;
        cor_id++;
        var cor = StartCoroutine(Cor(enumerator, obj, cor_id));
        Dictionary<int, Coroutine> list = null;
        if (!map.TryGetValue(obj, out list))
        {
            list = new Dictionary<int, Coroutine>();
            map.Add(obj, list);
        }
        list.Add(cor_id, cor);
        return cor_id;
    }

    private IEnumerator Cor(IEnumerator enumerator, object obj, int cor_id)
    {
        yield return enumerator;
        Dictionary<int, Coroutine> list = null;
        if (map.TryGetValue(obj, out list))
        {
            list.Remove(cor_id);
        }
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="obj">对象标识，当前开启协程的对象</param>
    /// <param name="cor_id">协程id</param>
    public void StopCor(object obj, int cor_id)
    {
        if (obj == null) obj = this;
        Dictionary<int, Coroutine> list = null;
        if (map.TryGetValue(obj, out list))
        {

            Coroutine cor = null;
            if (list.TryGetValue(cor_id, out cor))
            {
                if (cor != null)
                    StopCoroutine(cor);
                list.Remove(cor_id);
            }
        }
    }

    /// <summary>
    /// 停止所有协程
    /// </summary>
    /// <param name="obj"></param>
    public void StopAll(object obj)
    {
        if (obj == null) obj = this;
        Dictionary<int, Coroutine> list = null;
        if (map.TryGetValue(obj, out list))
        {
            foreach (var v in list)
            {
                if (v.Value != null)
                    StopCoroutine(v.Value);
            }
            list = null;
            map.Remove(obj);
        }
    }

    /// <summary>
    /// 延时调用
    /// </summary>
    /// <param name="obj">对象标识</param>
    /// <param name="time">延时时间</param>
    /// <param name="func">调用方法</param>
    /// <param name="repeat">重复次数，0 无限次</param>
    /// <returns>协程id</returns>
    public int DelayCall(object obj, float time, Action func, int repeat = 1)
    {
        if (obj == null) obj = this;
        if (repeat == 0)
            repeat = int.MaxValue;
        return StartCor(obj, Timer(func, time, repeat));
    }

    IEnumerator Timer(Action func, float time, int repeat)
    {
        WaitForSeconds wait = new WaitForSeconds(time);
        if (repeat > 1)
        {
            for (int i = 0; i < repeat; i++)
            {
                yield return wait;
                func();
            }
        }
        else
        {
            yield return wait;
            func();
        }
    }

    public int WebRequest(UnityEngine.Networking.UnityWebRequest request,Action func)
    {
        return StartCor(this, WebRequestCor(request,func));
    }

    public int Wait(YieldInstruction wait, Action func)
    {
        return StartCor(this, WaitCor(wait, func));
    }

    public int Wait(CustomYieldInstruction wait, Action func)
    {
        return StartCor(this, WaitCor(wait, func));
    }

    IEnumerator WebRequestCor(UnityEngine.Networking.UnityWebRequest request, Action func)
    {
        yield return request.SendWebRequest();
        func?.Invoke();
    }

    IEnumerator WaitCor(YieldInstruction wait,Action func)
    {
        yield return wait;
        func?.Invoke();
    }

    IEnumerator WaitCor(CustomYieldInstruction wait, Action func)
    {
        yield return wait;
        func?.Invoke();
    }
}

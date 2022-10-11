using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Э�̹�����
/// </summary>
public class CorManager : SingletonBehaviour<CorManager>
{
    private Dictionary<object, Dictionary<int, Coroutine>> map = new Dictionary<object, Dictionary<int, Coroutine>>();
    private int cor_id = 0;

    /// <summary>
    /// ����һ��Э��
    /// </summary>
    /// <param name="obj">�����ʶ����ǰ����Э�̵Ķ���</param>
    /// <param name="enumerator">Э��</param>
    /// <returns>��ǰЭ��id</returns>
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
    /// ֹͣһ��Э��
    /// </summary>
    /// <param name="obj">�����ʶ����ǰ����Э�̵Ķ���</param>
    /// <param name="cor_id">Э��id</param>
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
    /// ֹͣ����Э��
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
    /// ��ʱ����
    /// </summary>
    /// <param name="obj">�����ʶ</param>
    /// <param name="time">��ʱʱ��</param>
    /// <param name="func">���÷���</param>
    /// <param name="repeat">�ظ�������0 ���޴�</param>
    /// <returns>Э��id</returns>
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

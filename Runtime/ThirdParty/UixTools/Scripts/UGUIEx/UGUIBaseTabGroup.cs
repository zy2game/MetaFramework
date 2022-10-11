/*
 * File Name:               UGUIBaseTabGroup.cs
 * 
 * Description:             继承于MonoBehaviour的类
 * Author:                  wangfan
 * Create Date:             2017/10/17
 */

using UnityEngine;
public abstract class UGUIBaseTabGroup : MonoBehaviour
{
    public virtual bool IsMultiple { get { return false; } }

    public abstract void SetTabOn(UGUITab tab);
}

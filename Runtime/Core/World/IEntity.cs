using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XLua;

namespace GameFramework
{
    /// <summary>
    /// 实体对象，游戏中所有物体都可以看成一个实体，每个实体由组件构成
    /// </summary>
    public interface IEntity : GObject
    {
        /// <summary>
        /// 实体名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 实体唯一ID
        /// </summary>
        string guid { get; }

        string path { get; }

        int tag { get; }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        LuaTable AddComponent(LuaTable table);

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="compnent">组件</param>
        ICompnent AddComponent(ICompnent compnent);

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="tag">组件类型</param>
        void RemoveComponent(int tag);

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        ICompnent GetComponent(int tag);

        /// <summary>
        /// 获取所有组件
        /// </summary>
        /// <returns></returns>
        ICompnent[] GetComponents();

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        ICompnent[] GetComponents(params int[] tags);

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        LuaTable GetLuaComponent(int tag);

        /// <summary>
        /// 获取所有组件
        /// </summary>
        /// <returns></returns>
        LuaTable[] GetLuaComponents();

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        LuaTable[] GetLuaComponents(params int[] tags);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameFramework
{
    public delegate void GameFrameworkAction();
    public delegate void GameFrameworkAction<T>(T args);
    public delegate void GameFrameworkAction<T1, T2>(T1 args, T2 args2);
    public delegate void GameFrameworkAction<T1, T2, T3>(T1 args, T2 args2, T3 args3);
    public delegate void GameFrameworkAction<T1, T2, T3, T4>(T1 args, T2 args2, T3 args3, T4 args4);
    public interface GObject : IDisposable
    {
    }

    /// <summary>
    /// 唯一标志
    /// </summary>
    /// <remarks>如果组件标记了当前特性，则表明每个实体只能拥有一个相同类型的组件</remarks>
    public sealed class OnlyOne : Attribute
    {

    }

    /// <summary>
    /// 引用组件
    /// </summary>
    public sealed class Require : Attribute
    {
        public Type refrence;
        public Require(Type type)
        {
            refrence = type;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameFramework
{

    /// <summary>
    /// 组件接口
    /// </summary>
    public interface ICompnent : GObject
    {
        int tag { get; }

        void Awake(IEntity entity);

        void FixedUpdate(IEntity entity);
    }
}
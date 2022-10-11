using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameFramework
{
    /// <summary>
    /// 游戏模块
    /// </summary>
    public interface GModule : GObject
    {
        /// <summary>
        /// 轮询模块
        /// </summary>
        void Update();

    }
}
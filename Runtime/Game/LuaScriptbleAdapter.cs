using System;
using XLua;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// Lua逻辑模块
    /// </summary>
    public sealed class LuaScriptbleAdapter : IScriptble
    {
        private LuaTable table;
        private LuaFunction _executed;
        private LuaFunction _dispose;

        public LuaScriptbleAdapter(LuaTable table)
        {
            if (table == null)
            {
                throw new NullReferenceException("table");
            }
            this.table = table;
            _executed = this.table.Get<LuaFunction>("executed");
            _dispose = this.table.Get<LuaFunction>("dispose");
        }

        /// <summary>
        /// 释放当前逻辑模块
        /// </summary>
        public void Dispose()
        {
            if (_dispose != null)
            {
                _dispose.Call(table);
            }
            _executed = null;
            _dispose = null;
            table.Dispose();
        }


        /// <summary>
        /// 执行当前逻辑模块
        /// </summary>
        public void Executed(IWorld world)
        {
            if (_executed == null)
            {
                return;
            }
            _executed.Call(table, world);
        }
    }
}
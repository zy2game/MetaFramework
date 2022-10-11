using System;
using XLua;
namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 组件
    /// </summary>
    public sealed class LuaComponentAdapter : ICompnent
    {
        private LuaFunction _awake;
        private LuaFunction _dispose;
        private LuaFunction _fixedUpdate;

        public int tag
        {
            get;
        }
        public LuaTable table;



        /// <summary>
        /// 构造一个lua组件
        /// </summary>
        /// <param name="table"></param>
        public LuaComponentAdapter(LuaTable table)
        {
            if (table == null)
            {
                throw new NullReferenceException("table");
            }
            this.table = table;
            tag = table.Get<int>("tag");
            _awake = table.Get<LuaFunction>("awake");
            _fixedUpdate = table.Get<LuaFunction>("update");
            _dispose = table.Get<LuaFunction>("dispose");
        }

        public void Awake(IEntity entity)
        {
            if (_awake == null)
            {
                return;
            }
            _awake.Call(table, entity);
        }

        /// <summary>
        /// 释放组件
        /// </summary>
        public void Dispose()
        {
            if (_dispose != null)
            {
                _dispose.Call(table);
            }
            table.Dispose();
        }

        /// <summary>
        /// 轮询组件
        /// </summary>
        /// <param name="entity"></param>
        public void FixedUpdate(IEntity entity)
        {
            if (_fixedUpdate == null)
            {
                return;
            }
            _fixedUpdate.Call(table, entity);
        }

        /// <summary>
        /// 获取lua数据
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (table == null)
            {
                return default;
            }
            return table.Get<T>(key);
        }

        /// <summary>
        /// 设置lua数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Set<T>(string key, T value)
        {
            if (table == null)
            {
                return;
            }
            table.Set(key, value);
        }

    }
}
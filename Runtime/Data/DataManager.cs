using System.Collections.Generic;
using System;
using System.Linq;

namespace GameFramework.Runtime.Data
{

    public sealed class DataManager : Singleton<DataManager>, GModule
    {
        private List<IGameDatable> datables = new List<IGameDatable>();

        /// <summary>
        /// 创建指定类型数据模板
        /// </summary>
        /// <typeparam name="T">数据模板类型</typeparam>
        /// <returns></returns>
        public T Create<T>() where T : IGameDatable
        {
            return (T)Create(typeof(T));
        }

        /// <summary>
        /// 创建指定类型数据模板
        /// </summary>
        /// <param name="dataType">数据模板类型</param>
        /// <returns></returns>
        public IGameDatable Create(Type dataType)
        {
            if (dataType == null || dataType.IsAbstract || dataType.IsInterface)
            {
                throw new Exception("dataType");
            }
            IGameDatable datable = (IGameDatable)Activator.CreateInstance(dataType);
            datables.Add(datable);
            return datable;
        }

        /// <summary>
        /// 创建指定类型数据模板
        /// </summary>
        /// <param name="dataType">数据模板类型</param>
        /// <returns></returns>
        public Datable Create(string dataType)
        {
            return Create(Type.GetType(dataType));
        }

        /// <summary>
        /// 移除指定的数据模板
        /// </summary>
        /// <param name="guid">数据模板的唯一ID</param>
        public void Remove(string guid)
        {
            IGameDatable datable = GetDatable(guid);
            if (datable == null)
            {
                return;
            }
            datable.Dispose();
            datables.Remove(datable);
        }

        /// <summary>
        /// 移除所有数据模型
        /// </summary>
        public void Clear()
        {
            datables.ForEach(x => x.Dispose());
            datables.Clear();
        }

        /// <summary>
        /// 获取指定的数据表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IGameDatable GetDatable(string guid)
        {
            return datables.Find(x => x.guid == guid);
        }

        /// <summary>
        /// 获取指定数据组的数据表
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public List<IGameDatable> GetDatables(string group)
        {
            return datables.Where(x => x.group == group).ToList();
        }

        public void Update()
        {

        }
    }
}

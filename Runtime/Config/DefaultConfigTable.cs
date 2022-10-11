using System.IO;
using System.Text;
using System.Collections.Generic;

namespace GameFramework.Runtime.Config
{
    public sealed class DefaultConfigTable : IConfigTable
    {
        /// <summary>
        /// 表名
        /// </summary>
        /// <value></value>
        public string name { get; private set; }

        /// <summary>
        /// 配置项数量
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return configs.Count;
            }
        }

        private List<IConfig> configs = new List<IConfig>();


        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="id">配置项ID</param>
        /// <returns></returns>
        public IConfig GetConfig(int id) => configs.Find(x => x.id == id);

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="name">配置项名称</param>
        /// <returns></returns>
        public IConfig GetConfig(string name) => configs.Find(x => x.name == name);

        /// <summary>
        /// 是否存在指定的配置项
        /// </summary>
        /// <param name="id">配置项ID</param>
        /// <returns></returns>
        public bool HasConfig(int id) => configs.Find(x => x.id == id) != null;

        /// <summary>
        /// 是否存在指定的配置项
        /// </summary>
        /// <param name="name">配置项名称</param>
        /// <returns></returns>
        public bool HasConfig(string name) => configs.Find(x => x.name == name) != null;

        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        internal async void LoadConfig(string configName)
        {
            configName = configName.EndsWith(AppConst.ConfigExtension) ? configName : configName + AppConst.ConfigExtension;
            byte[] bytes = await Utility.ReadFileDataAsync(AppConst.ConfigPath + configName);
            if (bytes == null || bytes.Length <= 0)
            {
                return;
            }

            List<CatJson.JsonObject> jsons = CatJson.JsonParser.ParseJson<List<CatJson.JsonObject>>(UTF8Encoding.UTF8.GetString(bytes));
            foreach (var item in jsons)
            {
                LuaConfig config = new LuaConfig(item);
                configs.Add(config);
            }
            name = Path.GetFileNameWithoutExtension(configName);
        }

        /// <summary>
        /// 释放配置表
        /// </summary>
        public void Dispose()
        {
            configs.ForEach(x => x.Dispose());
            configs.Clear();
        }
    }
}
namespace GameFramework.Runtime.Config
{
    /// <summary>
    /// 配置表
    /// </summary>
    public interface IConfigTable : GObject
    {
        /// <summary>
        /// 表名
        /// </summary>
        /// <value></value>
        string name { get; }

        /// <summary>
        /// 配置项数量
        /// </summary>
        /// <value></value>
        int Count { get; }

        /// <summary>
        /// 获取指定的配置项
        /// </summary>
        /// <param name="id">配置项ID</param>
        /// <returns></returns>
        IConfig GetConfig(int id);
        /// <summary>
        /// 获取指定的配置项
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IConfig GetConfig(string name);

        /// <summary>
        /// 是否存在指定的配置项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasConfig(int id);

        /// <summary>
        /// 是否存在指定的配置项
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasConfig(string name);
    }
}
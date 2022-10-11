namespace GameFramework.Runtime.Config
{
    /// <summary>
    /// 配置表管理器
    /// </summary>
    public interface IConfigManager : GModule
    {
        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <param name="configName">配置表名称</param>
        /// <returns></returns>
        IConfigTable LoadConfig(string configName);

        /// <summary>
        /// 卸载配置表
        /// </summary>
        /// <param name="configName">配置表名称</param>
        void UnloadConfig(string configName);

        /// <summary>
        /// 获取配置表
        /// </summary>
        /// <param name="configName">配置表名称</param>
        /// <returns></returns>
        IConfigTable GetConfigure(string configName);

        /// <summary>
        /// 指定的配置表是否已经加载
        /// </summary>
        /// <param name="configName">配置表名称</param>
        /// <returns></returns>
        bool HasLoadConfig(string configName);

        /// <summary>
        /// 清理所有配置表
        /// </summary>
        void Clear();
    }
}
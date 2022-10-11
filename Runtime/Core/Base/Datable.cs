namespace GameFramework
{
    /// <summary>
    /// 数据表
    /// </summary>
    /// <remarks>在游戏中，一些临时数据应该继承于此接口，方便后面做热重载</remarks>
    public interface Datable : GObject
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        string guid { get; }
    }
}

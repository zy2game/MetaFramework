namespace GameFramework.Runtime.Data
{
    /// <summary>
    /// 数据表
    /// </summary>
    public interface IGameDatable : Datable
    {
        string group { get; }
        void Init(object data);
    }
}

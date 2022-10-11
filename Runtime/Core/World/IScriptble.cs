using XLua;

namespace GameFramework
{
    /// <summary>
    /// 表示一个逻辑单元
    /// </summary>
    /// <remarks>表示一个逻辑单元</remarks>
    [CSharpCallLua]
    public interface IScriptble : GObject
    {
        /// <summary>
        /// 执行逻辑单元
        /// </summary>
        void Executed(IWorld world);
    }
}
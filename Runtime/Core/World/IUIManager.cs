using GameFramework.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public interface IUIManager : GObject
    {
        /// <summary>
        /// UI摄像机
        /// </summary>
        /// <value></value>
        Camera UICamera { get; }

        /// <summary>
        /// 当前最上层的UI
        /// </summary>
        /// <value></value>
        IUIHandler current { get; }

        /// <summary>
        /// 轮询
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="name"></param>
        IUIHandler OpenUI(string name);

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="name"></param>
        void CloseUI(string name, bool isCache = false);

        /// <summary>
        /// 获取指定的UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IUIHandler GetUIHandler(string name);

        /// <summary>
        /// 设置管理器显示状态
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);

        /// <summary>
        /// 将UI设置到指定的层级
        /// </summary>
        /// <param name="handler"></param>
        Canvas ToLayer(IUIHandler handler, int layer);

        /// <summary>
        /// 将物体设置到指定的层级
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="layer"></param>
        Canvas ToLayer(GameObject handle, int layer);

        /// <summary>
        /// 显示进度界面
        /// </summary>
        /// <returns></returns>
        ILoading OnLoading();

        /// <summary>
        /// 显示一个提示窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        IMessageBox OnMsgBox(string text, GameFrameworkAction ok = null, GameFrameworkAction cancel = null);

        /// <summary>
        /// 清理所有弹窗消息
        /// </summary>
        void ClearMessageBox();

        /// <summary>
        /// 清理所有加载界面
        /// </summary>
        void ClearLoading();

        /// <summary>
        /// 清理所有UI
        /// </summary>
        void Clear();
    }

    public interface IMessageBox : GObject
    {
        string tilet { get; set; }
        string message { get; set; }
        GameObject gameObject { get; }
        event GameFrameworkAction cancel;
        event GameFrameworkAction entry;
    }

    public interface ILoading : GObject
    {
        string text { get; set; }
        string version { get; set; }
        GameObject gameObject { get; }
    }
}
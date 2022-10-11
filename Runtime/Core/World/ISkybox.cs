namespace GameFramework
{
    /// <summary>
    /// ��պж���
    /// </summary>
    public interface ISkybox : GObject
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="skyName"></param>
        void Initialize(float rotationTime, float rotationAngle, string skyName);

        /// <summary>
        /// 轮询
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 设置可见性
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);
    }
}

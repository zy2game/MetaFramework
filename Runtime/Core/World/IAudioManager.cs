
namespace GameFramework
{
    /// <summary>
    /// 音效播放器
    /// </summary>
    /// <remarks>播放音效时，只需传递音效的名称即可，播放器会自动加载对应的资源并进行播放</remarks>
    public interface IAudioManager : GObject
    {
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <param name="isLoop">是否循环播放，一般只有bgm才会是true</param>
        void PlaySound(string name, bool isLoop);

        /// <summary>
        /// 停止指定的音效
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <remarks>如果<paramref name="name"/>为空，则停止所有音效的播放</remarks>
        void StopSound(string name);

        /// <summary>
        /// 重新开始播放
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <remarks>如果<paramref name="name"/>为空，则重新播放所有音效</remarks>
        void ReplaySound(string name);

        /// <summary>
        /// 暂停音效的播放
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <remarks>如果<paramref name="name"/>为空，则暂停所有音效</remarks>
        void PauseSound(string name);

        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="name">音效名称</param>
        /// <param name="volume">音量</param>
        void SetVolume(float volume);

        /// <summary>
        /// 设置激活状态
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);

        /// <summary>
        /// 轮询
        /// </summary>
        void FixedUpdate();
    }
}
using System.IO;
using UnityEngine;
using GameFramework.Runtime.Assets;
namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 音效管道
    /// </summary>
    class AudioHandle : MonoBehaviour
    {
        private AudioSource source;

        /// <summary>
        /// 当前正在播放的音效名
        /// </summary>
        /// <value></value>
        public string audioName
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否播放完成
        /// </summary>
        /// <value></value>
        public bool finished
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        /// <value></value>
        public bool isPause
        {
            get;
            private set;
        }

        private AssetHandle assetHandle;

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isLoop"></param>
        public void Play(string name, bool isLoop)
        {
            if (source == null)
            {
                source = this.gameObject.AddComponent<AudioSource>();
            }
            isPause = false;
            audioName = Path.GetFileNameWithoutExtension(name);
            source.loop = isLoop;
            finished = false;
            AssetLoadAsync assetLoadAsync = ResourcesManager.Instance.LoadAsync(name);
            assetLoadAsync.callback = AssetLoadCompleted;
        }

        /// <summary>
        /// 加载资源完成
        /// </summary>
        /// <param name="handle"></param>
        private void AssetLoadCompleted(AssetHandle handle)
        {
            assetHandle = handle;
            AudioClip clip = (AudioClip)handle.LoadAsset(typeof(AudioClip));
            if (clip == null)
            {
                return;
            }
            source.clip = clip;
            if (!source.loop)
            {
                Invoke("PlaySoundFinished", clip.length);
            }
            handle.AddRefCount();
            source.Play();
        }

        /// <summary>
        /// 播放完成回调
        /// </summary>
        private void PlaySoundFinished()
        {
            finished = true;
            assetHandle.SubRefCount();
        }

        /// <summary>
        /// 重新播放
        /// </summary>
        public void Replay()
        {
            if (!isPause)
            {
                return;
            }
            isPause = false;
            source.Play();
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (source == null)
            {
                return;
            }
            source.loop = false;
            source.Stop();
            finished = true;
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (source == null)
            {
                return;
            }
            isPause = true;
            source.Pause();
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolument(float volume)
        {
            if (source == null)
            {
                return;
            }
            source.volume = volume;
        }
    }
}
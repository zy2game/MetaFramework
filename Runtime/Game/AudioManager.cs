using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 音效管理器
    /// </summary>
    public sealed class AudioManager : IAudioManager
    {
        private GameObject _object;
        private GameWorld gameWorld;
        private Queue<AudioHandle> handles;

        private List<AudioHandle> playings;

        public AudioManager(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            handles = new Queue<AudioHandle>();
            playings = new List<AudioHandle>();
            _object = new GameObject(gameWorld.name + "_AudioManager");
        }

        /// <summary>
        /// 释放音效管理器
        /// </summary>
        public void Dispose()
        {
            GameObject.DestroyImmediate(_object);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        public void FixedUpdate()
        {
            for (int i = playings.Count - 1; i >= 0; i--)
            {
                AudioHandle handle = playings[i];
                if (!handle.finished)
                {
                    continue;
                }
                handles.Enqueue(handle);
                playings.Remove(handle);
            }
        }

        /// <summary>
        /// 暂停音效
        /// </summary>
        /// <param name="name"></param>
        public void PauseSound(string name)
        {
             AudioHandle handle = playings.Find(x => x.audioName == name && x.isPause);
            if (handle == null)
            {
                return;
            }
            handle.Pause();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isLoop"></param>
        public void PlaySound(string name, bool isLoop)
        {
            if (!handles.TryDequeue(out AudioHandle handle))
            {
                handle = _object.AddComponent<AudioHandle>();
            }
            playings.Add(handle);
            handle.Play(name, isLoop);
        }

        /// <summary>
        /// 重新播放音效
        /// </summary>
        /// <param name="name"></param>
        public void ReplaySound(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                playings.ForEach(x =>
                    {
                        if (!x.isPause)
                        {
                            x.Replay();
                        }
                    });
                return;
            }
            AudioHandle handle = playings.Find(x => x.audioName == name && x.isPause);
            if (handle == null)
            {
                return;
            }
            handle.Replay();
        }

        /// <summary>
        /// 设置音效管理器状态
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            _object.SetActive(active);
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            playings.ForEach(x => x.SetVolument(volume));
        }

        /// <summary>
        /// 停止音效播放
        /// </summary>
        /// <param name="name"></param>
        public void StopSound(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                playings.ForEach(x => x.Stop());
                return;
            }
            AudioHandle handle = playings.Find(x => x.audioName == name);
            if (handle == null)
            {
                return;
            }
            handle.Stop();
        }
    }
}
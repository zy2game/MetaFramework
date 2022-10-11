
using UnityEngine;
using DG.Tweening;
using System;

namespace UixFramework
{
    public abstract class ITweener : MonoBehaviour
    {
        public enum PlayMode
        {
            None,//调用播放
            PlayOnStart,//开始时播放
            PlayOnEnable,//启用时播放
        }

        public PlayMode playMode = PlayMode.PlayOnStart;//播放模式
        public bool isPlaying = false;//是否开始播放
        public bool isPause = false;//是否暂停了

        protected Tweener tweener;//当前播放的动画

        public Action finished;//完成回调

        public ITweener finishedPlay;//完成后播放的下一段动画       

        protected virtual void Start()
        {
            if (playMode == PlayMode.PlayOnStart)
                Play();
        }

        protected abstract void Init();

        public virtual void Play()
        {
            if (isPlaying)
                tweener.Kill();
            Clear();
            Init();
            isPlaying = true;
            tweener = GetTweener();
            tweener.onComplete = OnComplete;
        }

        public virtual void Pause()
        {
            if (tweener == null) return;
            tweener.Pause();
            isPause = true;
        }

        public virtual void Stop()
        {
            if (tweener == null) return;
            tweener.Kill();
            Clear();
        }

        protected abstract Tweener GetTweener();

        protected virtual void OnComplete()
        {
            Clear();
            if (finished != null)
                finished();
            if (finishedPlay != null)
                finishedPlay.Play();
        }

        protected virtual void Clear()
        {
            tweener = null;
            isPause = false;
            isPlaying = false;
        }

        private void OnDestroy()
        {
            tweener.Kill();
        }
    }
}

using DG.Tweening;
using UnityEngine;

namespace UixFramework
{
    public abstract class TweenAnimation<T> : ITweener
    {
        public bool isSetStart;//是否设置开始参数
        public T start;//开始参数
        public T end;//结束参数
        public float delay = 0.0f;//延时播放时间      
        public float duration = 1.0f;//播放时间    
        public LoopType loopType = LoopType.Restart;//循环类型
        public int loops = 1;//循环次数 -1||0为无限循环
        public AnimationCurve curve;//动画曲线

        private void OnEnable()
        {
            if (playMode == PlayMode.PlayOnEnable)
                Play();
        }
    }
}
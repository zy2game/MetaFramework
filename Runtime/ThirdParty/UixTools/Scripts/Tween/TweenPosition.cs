using DG.Tweening;
using UnityEngine;

namespace UixFramework
{
    public class TweenPosition : TweenAnimation<Vector3> 
    {
        [SerializeField]
        private Transform t_target;
        public bool isLocal = true;

        protected override Tweener GetTweener()
        {
            Tweener t = isLocal ? t_target.DOLocalMove(end, duration) : t_target.DOMove(end, duration);
            t.SetEase(curve);
            if (loops == 0)
                loops = int.MaxValue;
            t.SetLoops(loops, loopType);
            return t;
        }

        protected override void Init()
        {
            if (t_target == null)
                t_target = transform;
            if (isSetStart)
            {
                if (isLocal)
                    t_target.localPosition = start;
                else
                    t_target.position = start;
            }
        }

    }
}

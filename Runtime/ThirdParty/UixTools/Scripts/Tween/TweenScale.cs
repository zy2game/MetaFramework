using DG.Tweening;
using UnityEngine;

namespace UixFramework
{
    public class TweenScale : TweenAnimation<Vector3>
    {
        [SerializeField]
        private Transform t_target;

        protected override Tweener GetTweener()
        {
            Tweener t = t_target.DOScale(end, duration);
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
                t_target.localScale = start;
        }

    }
}
using DG.Tweening;
using UnityEngine;

namespace UixFramework
{
    public class TweenLocalRotate : TweenAnimation<Vector3>
    {
        [SerializeField]
        private Transform t_target;
        public RotateMode rotateModel = RotateMode.Fast;

        protected override Tweener GetTweener()
        {
            Tweener t = t_target.DOLocalRotate(end, duration, rotateModel);
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
                t_target.localEulerAngles = start;
        }

    }
}

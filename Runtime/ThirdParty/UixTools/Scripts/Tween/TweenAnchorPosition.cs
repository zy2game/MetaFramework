using DG.Tweening;
using UnityEngine;

namespace UixFramework
{
    public class TweenAnchorPosition : TweenAnimation<Vector2>
    {
        [SerializeField]
        private RectTransform t_target;

        protected override Tweener GetTweener()
        {
            Tweener t = t_target.DOAnchorPos(end, duration);
            t.SetEase(curve);
            if (loops == 0)
                loops = int.MaxValue;
            t.SetLoops(loops, loopType);
            return t;
        }

        protected override void Init()
        {
            if (t_target == null)
                t_target = GetComponent<RectTransform>();
            if (isSetStart)
                t_target.anchoredPosition = start;
        }

    }
}
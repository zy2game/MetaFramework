using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UixFramework
{
    public class TweenGraphicColor : TweenAnimation<Color> 
    {
        [SerializeField]
        private MaskableGraphic t_target;

        protected override Tweener GetTweener()
        {
            Tweener t = t_target.DOColor(end, duration); 
            t.SetEase(curve);
            if (loops == 0)
                loops = int.MaxValue;
            t.SetLoops(loops, loopType);
            return t;
        }

        protected override void Init()
        {
            if (t_target == null)
                t_target = GetComponent<MaskableGraphic>();
            if (isSetStart)
                t_target.color = start;
        }

    }
}
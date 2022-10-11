using UnityEditor;
using UnityEngine;

namespace UixFramework
{
    [CustomEditor(typeof(TweenScale))]
    public class TweenScaleInspector : TweenAnimationInspector
    {

        protected override void SetEndCurValue()
        {
            Transform transform = (target as TweenScale).transform;
            end.vector3Value = transform.localScale;
        }

        protected override void SetStartCurValue()
        {
            Transform transform = (target as TweenScale).transform; 
            start.vector3Value = transform.localScale;
        }
    }
}

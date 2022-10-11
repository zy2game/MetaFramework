using UnityEngine;
using UnityEditor;

namespace UixFramework
{
    [CustomEditor(typeof(TweenAnchorPosition))]
    public class TweenAnchorPositionInspector : TweenAnimationInspector
    {

        protected override void SetEndCurValue()
        {
            RectTransform rectTransform = (target as TweenAnchorPosition).GetComponent<RectTransform>();
            end.vector2Value = rectTransform.anchoredPosition;
        }

        protected override void SetStartCurValue()
        {
            RectTransform rectTransform = (target as TweenAnchorPosition).GetComponent<RectTransform>();
            start.vector2Value = rectTransform.anchoredPosition;
        }
    }
}
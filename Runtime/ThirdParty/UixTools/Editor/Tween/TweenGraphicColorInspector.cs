using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UixFramework
{
    [CustomEditor(typeof(TweenGraphicColor))]
    public class TweenGraphicColorInspector : TweenAnimationInspector
    {

        protected override void SetEndCurValue()
        {
            MaskableGraphic gr = (target as TweenScale).GetComponent<MaskableGraphic>();
            end.colorValue = gr.color;
        }

        protected override void SetStartCurValue()
        {
            MaskableGraphic gr = (target as TweenScale).GetComponent<MaskableGraphic>();
            start.colorValue = gr.color;
        }
    }
}

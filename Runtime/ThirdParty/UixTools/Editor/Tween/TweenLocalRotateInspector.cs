using UnityEditor;
using UnityEngine;

namespace UixFramework
{
    [CustomEditor(typeof(TweenLocalRotate))]
    public class TweenLocalRotateInspector : TweenAnimationInspector
    {

        SerializedProperty rotateModel;

        protected override void OnEnable()
        {
            base.OnEnable();
            rotateModel = serializedObject.FindProperty("rotateModel");
        }

        protected override void DrawGUI()
        {
            base.DrawGUI();
            EditorGUILayout.PropertyField(rotateModel);
        }

        protected override void SetEndCurValue()
        {
            Transform transform = (target as TweenScale).transform;
            end.vector3Value = transform.localEulerAngles;
        }

        protected override void SetStartCurValue()
        {
            Transform transform = (target as TweenScale).transform; 
            start.vector3Value = transform.localEulerAngles;
        }
    }
}

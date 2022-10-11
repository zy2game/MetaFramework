using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UixFramework
{
    public class TweenAnimationInspector : ITweenerInspector
    {
        protected SerializedProperty isSetStart;
        protected SerializedProperty start;
        protected SerializedProperty end;
        protected SerializedProperty delay;
        protected SerializedProperty duration;
        protected SerializedProperty loopType;
        protected SerializedProperty loops;
        protected SerializedProperty curve;

        protected override void OnEnable()
        {
            base.OnEnable();
            isSetStart = serializedObject.FindProperty("isSetStart");

            start = serializedObject.FindProperty("start");
            end = serializedObject.FindProperty("end");

            delay = serializedObject.FindProperty("delay");
            duration = serializedObject.FindProperty("duration");
            loopType = serializedObject.FindProperty("loopType");
            loops = serializedObject.FindProperty("loops");
            curve = serializedObject.FindProperty("curve");

        }

        protected override void DrawGUI()
        {
            base.DrawGUI();
            EditorGUILayout.PropertyField(delay);
            EditorGUILayout.PropertyField(duration);
            EditorGUILayout.PropertyField(loops);
            EditorGUILayout.PropertyField(loopType);
            EditorGUILayout.PropertyField(isSetStart);
            if (isSetStart.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(start);
                if (DrawButton("Cur"))
                    SetStartCurValue();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(end);
            if (DrawButton("Cur"))
                SetEndCurValue();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(curve);
        }

        protected virtual void SetEndCurValue()
        {

        }

        protected virtual void SetStartCurValue()
        {
            
        }
    }
}

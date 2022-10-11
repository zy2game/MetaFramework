using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using UnityEngine;

[CustomEditor(typeof(MyButton), true)]
[CanEditMultipleObjects]
public class MyButtonEditor : SelectableEditor
{
    SerializedProperty m_OnClickProperty;
    SerializedProperty m_TransitionProperty;

    SerializedProperty anim_normal;
    SerializedProperty anim_down;
    SerializedProperty anim_up;

    SerializedProperty isThroughClick;

    SerializedProperty unityAction;

    protected override void OnEnable()
    {
        base.OnEnable();
        //m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
        m_TransitionProperty = serializedObject.FindProperty("m_Transition");
        anim_normal = serializedObject.FindProperty("anim_normal");
        anim_down = serializedObject.FindProperty("anim_down");
        anim_up = serializedObject.FindProperty("anim_up");

        isThroughClick= serializedObject.FindProperty("isThroughClick");

        unityAction = serializedObject.FindProperty("buttonEvent");
    }

    static Selectable.Transition GetTransition(SerializedProperty transition)
    {
        return (Selectable.Transition)transition.enumValueIndex;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();

        var trans = GetTransition(m_TransitionProperty);
        if (trans == Selectable.Transition.Animation)
        {
            EditorGUILayout.PropertyField(anim_normal);
            EditorGUILayout.PropertyField(anim_down);
            EditorGUILayout.PropertyField(anim_up);
        }

        EditorGUILayout.PropertyField(isThroughClick);
        //EditorGUILayout.PropertyField(m_OnClickProperty);

        
        EditorGUILayout.PropertyField(unityAction.FindPropertyRelative("target"));
        EditorGUILayout.PropertyField(unityAction.FindPropertyRelative("eventName"));

        serializedObject.ApplyModifiedProperties();


    
    }
}

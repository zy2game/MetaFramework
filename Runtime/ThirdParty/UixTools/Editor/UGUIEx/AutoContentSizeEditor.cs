using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

[CustomEditor(typeof(AutoContentSize))]
public class AutoContentSizeEditor : Editor
{
    private Vector2 temp = Vector2.zero;
    int labelWidth = 65;

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        AutoContentSize a = target as AutoContentSize;
        a.horizontalFit = (AutoContentSize.FitMode)DrawEnum("Horizontal", a.horizontalFit);
        a.verticalFit = (AutoContentSize.FitMode)DrawEnum("Vertical", a.verticalFit);
        a.image = DrawObject<Image>("Target", a.image);
        if (a.image == null)
        {            
            EditorUtility.SetDirty(a);
            return;
        }
        a.startSize = DrawVec2("OffectSize", a.startSize);
        a.limit = DrawBool("Limit", a.limit);
        if (a.limit)
            a.limitSize = DrawVec2("LimitSize", a.limitSize);
        if (temp == a.startSize)
        {
            EditorUtility.SetDirty(a);
            return;
        }

        temp = a.startSize;
        a.SetImageSize();


        EditorUtility.SetDirty(a);

    }

    protected bool DrawBool(string name, bool b)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(labelWidth));
        b = EditorGUILayout.Toggle(b);
        GUILayout.EndHorizontal();
        return b;
    }

    protected Vector2 DrawVec2(string name, Vector2 vec3)
    {
        vec3 = EditorGUILayout.Vector2Field(name, vec3);
        return vec3;
    }

    protected Enum DrawEnum(string name, Enum e)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(labelWidth));
        e = EditorGUILayout.EnumPopup(e);
        GUILayout.EndHorizontal();
        return e;
    }

    protected T DrawObject<T>(string name, T t) where T : UnityEngine.Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(labelWidth));
        t = EditorGUILayout.ObjectField(t, typeof(T), true) as T;
        GUILayout.EndHorizontal();
        return t;
    }
}

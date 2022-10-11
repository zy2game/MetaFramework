using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UGUITabGroup))]
public class UGUITabGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UGUITabGroup group = target as UGUITabGroup;
        group.isStartNotify = BoolFiled("StartNotify", group.isStartNotify);
        group.isAutoCreate = BoolFiled("AutoCreate", group.isAutoCreate);
        if (group.isAutoCreate)
        {
            group.autoCreateCount = IntFiled("autoCreateCount", group.autoCreateCount);
            group.tab = ObjFiled<UGUITab>("Tab", group.tab);
            if (GUILayout.Button("Test"))
            {
                group.Create();
            }
        }

        EditorUtility.SetDirty(group);

    }


    private T ObjFiled<T>(string name, Object obj) where T : Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(90));
        obj = EditorGUILayout.ObjectField(obj, typeof(T), true);
        GUILayout.EndHorizontal();
        return obj as T;
    }

    private int IntFiled(string name, int v)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(90));
        v = EditorGUILayout.IntField(v);
        GUILayout.EndHorizontal();
        return v;

    }

    private bool BoolFiled(string name, bool v)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(90));
        v = EditorGUILayout.Toggle(v);
        GUILayout.EndHorizontal();
        return v;
    }
}
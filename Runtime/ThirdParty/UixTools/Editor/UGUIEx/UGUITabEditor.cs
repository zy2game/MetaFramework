using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(UGUITab))]
public class UGUITabEditor : Editor
{

    protected GUIStyle stepTitleStyle;
    protected GUIStyle buttonStyle;

    private void CreateStepTitleStyle()
    {
        Texture2D bg = new Texture2D(1, 1);
        bg.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        bg.Apply();
        Texture2D bg3 = new Texture2D(1, 1);
        bg3.SetPixel(0, 0, new Color(0.8f, 0, 0, 0.5f));
        bg3.Apply();
        stepTitleStyle = new GUIStyle();
        stepTitleStyle.alignment = TextAnchor.MiddleCenter;
        stepTitleStyle.fontSize = 12;
        stepTitleStyle.normal = new GUIStyleState { background = bg, textColor = new Color(0.8f, 0.8f, 0.8f) };

        buttonStyle = new GUIStyle();
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.fontSize = 15;
        buttonStyle.normal = new GUIStyleState { background = bg, textColor = Color.yellow };
        buttonStyle.hover = new GUIStyleState { background = bg, textColor = Color.red };
        buttonStyle.active = new GUIStyleState { background = bg3, textColor = Color.yellow };

    }
     
    public override void OnInspectorGUI()
    {
        if (buttonStyle == null)
            CreateStepTitleStyle();

        UGUITab tab = target as UGUITab;
        tab.obj_deselect = ObjFiled<GameObject>("deselect", tab.obj_deselect);
        tab.obj_select = ObjFiled<GameObject>("select", tab.obj_select);
        tab.group = ObjFiled<UGUIBaseTabGroup>("group", tab.group);
        tab.intParam = IntFiled("intParam", tab.intParam);
        tab.isOn = BoolFiled("isOn", tab.isOn);
        if (tab.selectShowObj == null)
            tab.selectShowObj = new List<GameObject>();
        if (tab.selectShowObj == null) 
            return;

        if (tab.selectShowObj.Count > 0)
            GUILayout.Box("Selected Show GameObject", stepTitleStyle, GUILayout.Width(Screen.width), GUILayout.Height(20));

        for (int i = 0; i < tab.selectShowObj.Count; i++)
        {
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", buttonStyle,GUILayout.Width(30)))
            {
                tab.selectShowObj.RemoveAt(i);
                break;
            }
            GUILayout.Label(i+".", GUILayout.Width(35));
            tab.selectShowObj[i] = EditorGUILayout.ObjectField(tab.selectShowObj[i], typeof(GameObject), true) as GameObject;
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add select show obj"))
        {
            tab.selectShowObj.Add(null);
        }

        

        EditorUtility.SetDirty(tab);
    }

    private T ObjFiled<T>(string name, Object obj) where T : Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(70));
        obj = EditorGUILayout.ObjectField(obj, typeof(T), true);
        GUILayout.EndHorizontal();
        return obj as T;
    }

    private int IntFiled(string name, int v)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(70));
        v = EditorGUILayout.IntField(v);
        GUILayout.EndHorizontal();
        return v;

    }

    private bool BoolFiled(string name, bool v)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(70));
        v = EditorGUILayout.Toggle(v);
        GUILayout.EndHorizontal();
        return v;
    }
}

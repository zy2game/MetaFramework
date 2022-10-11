using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class SearchingProto : EditorWindow
{

    string message;
    List<string> list=new List<string>();
    static SearchingProto window;
    [MenuItem("XLua / Searching Proto")]
    static void Init()
    {
        window = EditorWindow.CreateInstance<SearchingProto>();
        window.Show();
    }

    private void OnGUI()
    {
        message = EditorGUILayout.TextField(message);
        if(GUILayout.Button("go"))
        {
            list=  Searching(message);
        }
        foreach(var i in list)
        {
            GUILayout.Label(i);
        }
    }

    static List<string> Searching(string m)
    {
         string[] alls= AssetDatabase.FindAssets("", new[] { "Assets/MyXLua/protocol/proto" });

        List<string> list = new List<string>();

        foreach (var item in alls)
        {
           
            string path = AssetDatabase.GUIDToAssetPath(item);
            string name = path;
            string tt = Application.dataPath.Replace("Assets", "");
            path = tt + path;

           // Debug.Log(path);

            string content= System.IO.File.ReadAllText(path);
            Regex regex = new Regex(m);
            if (regex.IsMatch(content))
            //    if (content.Contains("message acc_t"))
            {
                list.Add(name);
                Debug.Log(name);
            }
        }

        return list;

    }

}

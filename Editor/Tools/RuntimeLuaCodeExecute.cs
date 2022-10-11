using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Tooles
{

    public class RuntimeLuaCodeExecute : BaseEditorWindow
    {
        [MenuItem("Tools/Other/ִ��lua����", false, 0)]
        public static void Open()
        {
            Open<RuntimeLuaCodeExecute>(500, 600, "ִ��lua����");

        }

        private string luaText;

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                Close();
                return;
            }
            
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("����Lua����", GUILayout.Width(150),GUILayout.Height(25)))
            {
                LuaManager.Instance.DoString(luaText);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            luaText = GUILayout.TextArea(luaText, GUILayout.Height(560));
           


        }
    }
}
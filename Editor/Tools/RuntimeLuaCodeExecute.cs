using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Tooles
{

    public class RuntimeLuaCodeExecute : BaseEditorWindow
    {
        [MenuItem("Tools/Other/执行lua代码", false, 0)]
        public static void Open()
        {
            Open<RuntimeLuaCodeExecute>(500, 600, "执行lua代码");

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
            if (GUILayout.Button("运行Lua代码", GUILayout.Width(150),GUILayout.Height(25)))
            {
                LuaManager.Instance.DoString(luaText);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            luaText = GUILayout.TextArea(luaText, GUILayout.Height(560));
           


        }
    }
}
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Tooles
{
    public class ScriptableObjectCreator : Editor
    {
        //[MenuItem("Assets/ScriptableObjectCreator", false, 0)]
        static void Creator()
        {
            var contex = Selection.activeObject as MonoScript;
            if (contex == null) return;
            Type type = contex.GetClass();
            var obj = CreateInstance(type);
            if (!(obj is ScriptableObject)) return;
            string path = AssetDatabase.GetAssetPath(contex).Replace(".cs", ".asset");
            if (File.Exists(path)) return;
            AssetDatabase.CreateAsset(obj, path);

        }
    }
}

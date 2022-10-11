using System.Collections;
using UnityEngine;
using UnityEditor;

namespace GameEditor
{

    public class BaseEditorWindow : EditorWindow
    {
        private EditorCoroutine editorCoroutine = new EditorCoroutine();

        public static T Open<T>(int width, int height, string title = "") where T : BaseEditorWindow
        {
            BaseEditorWindow window = null;
            if (string.IsNullOrEmpty(title)) window = GetWindow<T>();
            else window = GetWindow<T>(title);
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width + 10, height);
            window.Show();
            window.Init();
            return window as T;
        }

        protected virtual void Init() { }

        public int StartCor(IEnumerator enumerator)
        {
            return editorCoroutine.StartCor(enumerator);
        }

        public void StopCor(int id)
        {
            editorCoroutine.StopCor(id);
        }

        public void StopCorAll()
        {
            editorCoroutine.StopAll();
        }

        protected virtual void Update()
        {
            editorCoroutine.Update();
        }

        protected void Label(string v, int width, int height)
        {
            if (height != 0)
                GUILayout.Label(v, GUILayout.Width(width), GUILayout.Height(height));
            else
                GUILayout.Label(v, GUILayout.Width(width));
        }

        protected float DrawFloat(string name, float value, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            float f = EditorGUILayout.FloatField(value, options);
            GUILayout.EndHorizontal();
            return f;
        }

        public virtual T ObjField<T>(string name, T t, int width = 75, int height = 15) where T : Object
        {
            GUILayout.BeginHorizontal();
            Label(name, width, height);
            t = (T)EditorGUILayout.ObjectField(t, typeof(T), true, GUILayout.Height(height));
            GUILayout.EndHorizontal();

            return t;

        }

        public virtual bool DrawToggle(string name, bool b, int width = 75, int height = 0)
        {
            GUILayout.BeginHorizontal();
            Label(name, width, height);
            b = EditorGUILayout.Toggle(b);
            GUILayout.EndHorizontal();
            return b;
        }
    }
}
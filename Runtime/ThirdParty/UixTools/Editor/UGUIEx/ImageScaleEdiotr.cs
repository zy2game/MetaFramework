using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ImageScaleEdiotr : EditorWindow
{
    [MenuItem("Tools/ToolsWindows/ImageScaleEdiotr", false, 0)]
    static void Open()
    {
        ImageScaleEdiotr re = GetWindow<ImageScaleEdiotr>("UIScaleWindow");
        re.minSize = new Vector2(200, 100);
        re.maxSize = new Vector2(200, 110);
        re.Show();
        re.Init();
    }

    private float scale = 1.0f;


    private void Init()
    {
        if (PlayerPrefs.HasKey("Editor_ImageScale"))
            scale = PlayerPrefs.GetFloat("Editor_ImageScale");
    }

    private void OnGUI()
    {
        scale = EditorGUILayout.FloatField("scale", scale);
        if (GUILayout.Button("Set"))
        {
            this.Set();
        }
    }

    private void Set()
    {
        var vs = Selection.objects;
        foreach (var v in vs)
        {
            GameObject go = v as GameObject;
            Image img = go.GetComponent<Image>();
            if (!img) return;

            img.transform.localScale = Vector3.one;
            float w = img.preferredWidth;
            float h = img.preferredHeight;
            Debug.LogError(w + "  " + h);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(w * scale, h * scale);
        }

        PlayerPrefs.SetFloat("Editor_ImageScale", 0.50641f);
        PlayerPrefs.Save();
    }

}

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


//[CustomEditor(typeof(Image), true)]
//[CanEditMultipleObjects]
//public class MyImageEditor : ImageEditor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        if (GUILayout.Button("Set Sprite"))
//        {
//            ImageEditorWindow.Open();
//        }
//    }
//}

public class ImageEditorWindow : OtherEditorTools.BaseEditorWindow
{
    private List<Texture2D> atlas;
    private  string atlasPath = "Assets/LuaFramework/Resources/UI/";
    private int cellW = 150;
    private int cellH = 170;

    [MenuItem("Tools/ImageEditorWindow", false, 100)]
    public static void Open()
    {
        ImageEditorWindow iew = GetWindow<ImageEditorWindow>();
        iew.minSize = new Vector2(810, 800);
        iew.maxSize = new Vector2(820, 820);
        iew.Show();
        iew.Start();        
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("AtlasSearchPth"))
            atlasPath = PlayerPrefs.GetString("AtlasSearchPth");
    }

    public static void Open(string path)
    {
        ImageEditorWindow iew = GetWindow<ImageEditorWindow>();
        iew.minSize = new Vector2(810, 800);
        iew.maxSize = new Vector2(820, 820);
        iew.Show();
        iew.Start();
        iew.atlasPath = path;
        iew.Init();
    }

    public void Init()
    {
        if (atlas != null)
            atlas.Clear();
        string path=Application.dataPath.Replace("Assets", "") + atlasPath;
        atlas = new List<Texture2D>();
        // atlas = Resources.LoadAll<Texture2D>(atlasPath);
        LoadAtlas(path);

        PlayerPrefs.SetString("AtlasSearchPth", atlasPath);
    }

    private void LoadAtlas(string dirPath)
    {
        string[] fs = Directory.GetFiles(dirPath);
        foreach (string f in fs)
        {
            if (f.EndsWith(".png"))
            {
                string pt = f;
                pt = "Assets" + pt.Replace(Application.dataPath, "");
                Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(pt);
                if (t2d)
                    atlas.Add(t2d);
            }
        }

        string[] ds = Directory.GetDirectories(dirPath);

        foreach (var d in ds)
        {
            LoadAtlas(d);
        }
    }

    private Vector2 scl = Vector2.zero;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        atlasPath = DrawTextField("AtlasPath:", atlasPath);
        if (GUILayout.Button("Search"))
        {
            Init();
        }
        GUILayout.EndHorizontal();

        if (atlas == null)
            return;

        int width = (int)position.width;
        int height = (int)position.height-30;

        int cw = width / cellW;
        int ch = (atlas.Count / cw + 1) * cellH;
        scl = GUI.BeginScrollView(new Rect(0, 30, width, height), scl, new Rect(0, 0, width, ch));
        for (int i = 0; i < atlas.Count; i++)
        {
            Rect rect = new Rect(i % cw * cellW, i / cw * cellH, cellW, cellH);
            DrawTexture(atlas[i], rect, i + 1);
        }
        GUI.EndGroup();
    }

    private void DrawTexture(Texture2D t, Rect rect, int index)
    {
        GUI.BeginGroup(rect);
        GUI.Box(new Rect(0, 0, rect.width, rect.height), "");
        int w = t.width;
        int h = t.height - 20;
        int x = 0;
        int y = 0;
        if (w > h)
        {
            float f = w / (float)h;
            w = (int)rect.width;
            h = (int)(w / f);
        }
        else if (w < h)
        {
            float f = h / (float)w;
            h = (int)rect.width;
            w = (int)(h / f);
        }
        else
        {
            w = (int)rect.width;
            h = (int)rect.height;
        }

        if (GUI.Button(new Rect(0, 0, rect.width, rect.height - 20), ""))
        {
            SpriteEditorWindow.Open(t);
        }

        x = (int)(rect.width - w) / 2;
        y = (int)(rect.height - h) / 2;

        GUI.DrawTexture(new Rect(x, y, w, h), t);
        if (GUI.Button(new Rect(0, cellH - 20, cellW, 20), index + "." + t.name))
        {
            Selection.activeObject = t;
        }
        GUI.EndGroup();
    }
}

public class SpriteEditorWindow : OtherEditorTools.BaseEditorWindow
{
    //private int cellW = 150;
    //private int cellH = 150;
    private List<Sprite> sps;
    private Texture2D t;
    private Texture2D bg_name;

    public static void Open(Texture2D t)
    {
        SpriteEditorWindow iew = GetWindow<SpriteEditorWindow>();
        iew.minSize = new Vector2(1050, 960);
        iew.maxSize = new Vector2(1060, 960);
        iew.titleContent = new GUIContent { text = t.name };
        iew.Show();
        iew.Init(t);
    }

    private void Init(Texture2D t)
    {
        bg_name = new Texture2D(1,1);
        bg_name.SetPixel(0, 0, new Color(0,0,0,0.5f)); 
        bg_name.Apply();


        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(t)); 
        sps = new List<Sprite>();
        foreach (var v in objs)
        {
            Sprite s = v as Sprite;
            if (s == null)
            {
                continue;
            }
            sps.Add(s);
        }

        this.t = t;
    }

    private Vector2 scl1 = Vector2.zero;
    private string search = "";
    private List<Sprite> temp = new List<Sprite>();

    private bool showModel = true;

    private void OnGUI()
    {
        GUI.Label(new Rect(0,5,95,18), "Search Name:");
        string sMsg = GUI.TextField(new Rect(95, 5, 750, 18), search);

        if (sMsg.Length > 0)
        {
            if (!search.Equals(sMsg))
            {
                temp.Clear();
                foreach (var v in sps)
                {
                    if (v.name.Contains(sMsg))
                        temp.Add(v);
                }
            }
        }      
        else
        {
            if (temp.Count != sps.Count)
            {
                temp.Clear();
                temp.AddRange(sps.ToArray());
            }
        }
        search = sMsg;

        if (search.Length > 0)
        {
            if (GUI.Button(new Rect(845, 5, 20, 20), EditorGUIUtility.IconContent("RotateTool")))
            {
                search = "";
            }
        }

        if (GUI.Button(new Rect(960, 5, 80, 20), "ShowType"))
        {
            showModel = !showModel;
        }

        if (GUI.Button(new Rect(875, 5, 80, 20), "SpriteEditor"))
        {
            this.SpriteEditor();
        }

        int width = (int)position.width;
        int height = (int)position.height;
        //int cw = width / cellW;
        //int ch = (temp.Count / cw + 1) * cellH;
        
        if (showModel)
        {
            // GUI.DrawTexture(new Rect(0, 0, t.width, t.height), t);
            scl1 = GUI.BeginScrollView(new Rect(0, 30, position.width, position.height), scl1, new Rect(0, 0, position.x - 50, (temp.Count / 7 + 1) * 148 + 50));
            for (int i = 0; i < temp.Count; i++)
            {
                DrawButton(temp[i], i);
            }
            GUI.EndScrollView();
        }
        else
        {
            scl1 = GUI.BeginScrollView(new Rect(0, 30, position.width, position.height-30), scl1, new Rect(0, 0, t.width+30,t.height+50));
            GUI.DrawTexture(new Rect(0, 30, t.width, t.height), t);           
            for (int i = 0; i < sps.Count; i++)
            {
                DrawButton(sps[i]);
            }
            GUI.EndScrollView();
        }
       
    }

    private void DrawButton(Sprite sprite)
    {

        Rect rect = new Rect(sprite.rect.x, t.height - sprite.rect.y - sprite.rect.height+30, sprite.rect.width, sprite.rect.height);

        GUI.BeginGroup(rect);
        {
            Rect outRect = new Rect(0, 0, rect.width, rect.height);
            GUI.Box(outRect, "");

            if (GUI.Button(outRect, "", new GUIStyle { fontSize = 20, normal = new GUIStyleState { textColor = Color.gray }, alignment = TextAnchor.LowerCenter }))
            {
                GameObject go = Selection.activeObject as GameObject;
                if (go)
                {
                    Image img = go.GetComponent<Image>();
                    bool isAutoSize = false;
                    if (img == null)
                    {
                        GameObject newGo = new GameObject("newImage");
                        newGo.transform.SetParent(go.transform);
                        RectTransform rt = newGo.AddComponent<RectTransform>();
                        rt.anchoredPosition3D = Vector3.zero;
                        rt.localScale = Vector3.one;
                        rt.localEulerAngles = Vector3.zero;
                        img = newGo.AddComponent<Image>();
                        img.raycastTarget = false;
                        isAutoSize = true;
                        Selection.activeObject = newGo;
                    }

                    img.sprite = sprite;
                    img.enabled = false;
                    img.enabled = true;
                    if (isAutoSize)
                        img.SetNativeSize();

                }
                CopySprite(sprite);
            }
        }
        GUI.EndGroup();

        if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
        {
            Debug.LogError(sprite.name);
        }
    }

    private void DrawButton(Sprite sprite,int index)
    {
        Rect rect =new Rect(index%7*148,index/7*148, 148,148);      
        GUI.BeginGroup(rect);
        {     
            Rect outRect = new Rect(0, 0, rect.width, rect.height);
            Rect sprRect = new Rect(sprite.rect.x / t.width, sprite.rect.y / t.height, sprite.rect.width / t.width, sprite.rect.height / t.height);
            Rect drawRect = outRect;
            if (sprRect.width < sprRect.height)
            {
                float dt = sprite.rect.height/148;
                drawRect.width = sprite.rect.width / dt;
            }
            else
            {
                float dt = sprite.rect.width/148;
                drawRect.height = sprite.rect.height / dt;
            }

            if (drawRect.width > drawRect.height)
            {
                drawRect.y = (148 - drawRect.height) / 2;
            }
            else
            {
                drawRect.x = (148 - drawRect.width) / 2;
            }           

            GUI.DrawTextureWithTexCoords(drawRect, sprite.texture, sprRect);
            GUI.DrawTexture(new Rect(0, 0, 60, 16), bg_name);
            GUI.Label(outRect, string.Format("{0}*{1}", sprite.rect.width, sprite.rect.height));
            string txt = sprite.name;
            if (txt.Length > 16)
                txt ="..."+txt.Substring(txt.Length - 16, 16);
            GUI.Box(outRect, "");
            //GUI.DrawTexture(new Rect(2, 132, 144, 16), bg_name);

            if (GUI.Button(new Rect(2, 132, 144, 16),"",new GUIStyle { normal = { background=bg_name} }))
            {
                GUIUtility.systemCopyBuffer = sprite.name;
            }

            if (GUI.Button(outRect, txt, new GUIStyle { fontSize = 12, normal=new GUIStyleState { textColor=Color.white }, alignment= TextAnchor.LowerCenter }))
            {
                GameObject go = Selection.activeObject as GameObject;
                if (go)
                {
                    Image img = go.GetComponent<Image>();

                    bool isAutoSize=false;
                    if (img == null)
                    {
                        GameObject newGo = new GameObject("newImage");
                        newGo.transform.SetParent(go.transform);
                        RectTransform rt = newGo.AddComponent<RectTransform>();
                        rt.anchoredPosition3D = Vector3.zero;
                        rt.localScale = Vector3.one;
                        rt.localEulerAngles = Vector3.zero;
                        img = newGo.AddComponent<Image>();
                        img.raycastTarget = false;
                        isAutoSize = true;
                        Selection.activeObject = newGo;
                    }

                    img.sprite = sprite;
                    img.enabled = false;
                    img.enabled = true;
                    if (isAutoSize)
                        img.SetNativeSize();

                }
                CopySprite(sprite);
            }
        }
        GUI.EndGroup();

        if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
        {
            Debug.LogError(sprite.name);
        }
    }

    private void CopySprite(Sprite spr) 
    {        
        Texture2D t2d = spr.texture;
        string path=AssetDatabase.GetAssetPath(t2d);
        //"Assets/LuaFramework/Resources/UI/Games/NewMahjong/Atlas/mj_textTips.png"
        int lastIndex = path.LastIndexOf(".");
        path = path.Substring(0, lastIndex);
        string[] arr = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        string newPath = "";
        bool b = false;
        foreach (var v in arr)
        {
            if (!b)
            {
                if (v.Equals("UI"))
                {
                    b = true;
                }
            }
            else
            {
                newPath += v + "/";
            }
        }
        newPath = newPath.Substring(0, newPath.Length-1);     
        GUIUtility.systemCopyBuffer = newPath;
    }

    private void SpriteEditor()
    {
        Selection.activeObject = t;
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SpriteEditorWindow");
        var window = EditorWindow.GetWindow(type);
        window.Show();        
    }
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.IO;
using UnityEditor.SceneManagement;
using System.Threading;
using UnityEngine.UI;

namespace OtherEditorTools
{
    public class BaseEditorWindow : EditorWindow
    {
        private GUIStyle textField;
        public GUIStyle TextField
        {
            get
            {
                if (textField == null)
                {
                    textField = new GUIStyle(GUI.skin.textField);
                }
                return textField;
            }
        }

        //复制和粘贴
        private string HandleCopyPaste(int controlID)
        {
            if (controlID == GUIUtility.keyboardControl)
            {
                if (Event.current.type == UnityEngine.EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
                {
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        Event.current.Use();
                        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.Copy();
                    }
                    else if (Event.current.keyCode == KeyCode.V)
                    {
                        Event.current.Use();
                        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.Paste();
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                        return editor.text; //以及更高的unity版本中editor.content.text已经被废弃，需使用editor.text代替
#else
                    return editor.content.text;
#endif
                    }
                    else if (Event.current.keyCode == KeyCode.A)
                    {
                        Event.current.Use();
                        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        editor.SelectAll();
                    }
                }
            }
            return null;
        }

        private void Label(string v, int width, int height)
        {
            if (height != 0)
                GUILayout.Label(v, GUILayout.Width(width), GUILayout.Height(height));
            else
                GUILayout.Label(v, GUILayout.Width(width));
        }

        private float DrawFloat_t(string name, float value, int width, int height)
        {
            float f = EditorGUILayout.FloatField(name, value, TextField);
            return f;
        }

        public virtual float DrawFloat(string name, float value, int width = 75, int height = 0)
        {
            int textFieldID = GUIUtility.GetControlID("FloatField".GetHashCode(), FocusType.Keyboard) + 1;
            if (textFieldID == 0)
            {
                return DrawFloat_t(name, value, width, height);
            }
            string str = HandleCopyPaste(textFieldID);
            if (str != null)
                float.TryParse(str, out value);
            return DrawFloat_t(name, value, width, height);
        }

        private int DrawInt_t(string name, int value, int width, int height)
        {
            int v = EditorGUILayout.IntField(name, value);
            return v;
        }

        public virtual int DrawInt(string name, int value, int width = 75, int height = 0)
        {
            int textFieldID = GUIUtility.GetControlID("IntField".GetHashCode(), FocusType.Keyboard) + 1;
            if (textFieldID == 0)
            {
                return DrawInt_t(name, value, width, height);
            }
            string str = HandleCopyPaste(textFieldID);
            if (str != null)
                int.TryParse(str, out value);
            return DrawInt_t(name, value, width, height);

        }

        private string DrawTextField_t(string name, string value, int width, int height, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            Label(name, width, height);
            string str = GUILayout.TextField(value, options);
            GUILayout.EndHorizontal();
            return str;
        }

        public virtual string DrawTextField(string name, string value, int width = 75, int height = 0, params GUILayoutOption[] options)
        {
            int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
            if (textFieldID == 0)
            {
                return DrawTextField_t(name, value, width, height, options);
            }
            //处理复制粘贴的操作
            //value = HandleCopyPaste(textFieldID) ?? value;
            return DrawTextField_t(name, value, width, height, options);
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

    public class EditorWindowOtherTools : BaseEditorWindow
    {


    }

    public class RenameObject : EditorWindowOtherTools
    {
        [MenuItem("Tools/Other/ReNameObject")]
        static void Open()
        {
            RenameObject re = GetWindow<RenameObject>();
            re.minSize = new Vector2(300, 100);
            re.maxSize = new Vector2(300, 110);
            re.Show();
        }

        private string rename;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            rename = DrawTextField("NameCount", rename);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Rename"))
            {
                GameObject[] objs = Selection.gameObjects;

                foreach (var v in objs)
                {
                    v.name = v.name + rename;
                }
            }

            if (GUILayout.Button("Remove1b"))
            {
                GameObject[] objs = Selection.gameObjects;
                foreach (var v in objs)
                {
                    if (v.name.Length > 1)
                        v.name = v.name.Substring(0, v.name.Length - 1);
                }
            }

            if (GUILayout.Button("RenameChildCount"))
            {
                GameObject obj = Selection.activeObject as GameObject;
                if (obj == null)
                    return;
                Transform[] trans = obj.GetComponentsInChildren<Transform>();

                int c = 0;
                int.TryParse(rename, out c);

                foreach (var v in trans)
                {
                    if (v.parent == obj.transform)
                    {
                        v.gameObject.name = v.name + ++c;
                    }
                }
            }
        }
    }

    public class GameObjectOther : EditorWindowOtherTools
    {
        [MenuItem("Tools/Other/GameObjectOther")]
        static void Open()
        {
            GameObjectOther re = GetWindow<GameObjectOther>();
            re.minSize = new Vector2(300, 100);
            re.maxSize = new Vector2(300, 110);
            re.Show();
            re.Init();
        }

        private string[] modelNames = new string[] { "Horizontal", "Vertical", "Grid" };
        private int mode = 0;

        private float horizontalSize = 100;
        private float vertical = 100;
        private int lineCount = 3;
        private List<RectTransform> t_selects;

        private List<Transform> t_selects1;

        private void Init()
        {
            GameObject go = Selection.activeObject as GameObject;
            if (go == null)
            {
                Close();
                return;
            }
            RectTransform[] trans = go.GetComponentsInChildren<RectTransform>();



            foreach (var v in trans)
            {
                if (v.parent == go.transform)
                {
                    t_selects.Add(v);
                }
            }

            if (t_selects.Count <= 1)
            {
                Close();
                return;
            }

        }

        private void OnGUI()
        {
            mode = GUILayout.Toolbar(mode, modelNames);

            switch (mode)
            {
                case 0:
                    float h = DrawFloat("Horizontal:", horizontalSize);
                    if (h != horizontalSize)
                    {
                        horizontalSize = h;
                        Vector3 oh = t_selects[0].anchoredPosition3D;
                        for (int i = 1; i < t_selects.Count; i++)
                        {
                            t_selects[i].anchoredPosition3D = oh + new Vector3(i * h, 0, 0);
                        }
                    }
                    break;
                case 1:
                    float v = DrawFloat("Vertical:", vertical);
                    if (v != vertical)
                    {
                        vertical = v;
                        Vector3 ov = t_selects[0].anchoredPosition3D;
                        for (int i = 1; i < t_selects.Count; i++)
                        {
                            t_selects[i].anchoredPosition3D = ov + new Vector3(0, -i * v, 0);
                        }
                    }
                    break;
                case 2:
                    float gh = DrawFloat("Horizontal:", horizontalSize);
                    float gv = DrawFloat("Vertical:", vertical);
                    int gc = DrawInt("LineCount:", lineCount);

                    if (gh != horizontalSize || gv != vertical || gc != lineCount)
                    {
                        horizontalSize = gh;
                        vertical = gv;
                        lineCount = gc;

                        Vector3 ov = t_selects[0].anchoredPosition3D;
                        for (int i = 1; i < t_selects.Count; i++)
                        {
                            int y = i / gc;
                            int x = i % gc;
                            t_selects[i].anchoredPosition3D = ov + new Vector3(x * gh, -y * gv, 0);
                        }
                    }

                    break;
            }
            if (lineCount < 1)
                lineCount = 1;
        }
    }

    //自定义字体创建
    //public class CustomFontCreater : EditorWindowOtherTools
    //{
    //    [MenuItem("Tools/Other/CreateCustomFont")]
    //    static void Open()
    //    {
    //        CustomFontCreater re = GetWindow<CustomFontCreater>();
    //        re.minSize = new Vector2(300, 150);
    //        re.maxSize = new Vector2(300, 160);
    //        re.Show();
    //    }

    //    private Font font;
    //    private int cellWidth;
    //    private Texture2D t2d;

    //    void CreateFont()
    //    {
    //        if (t2d == null)
    //            return;

    //        string t2d_path = AssetDatabase.GetAssetPath(t2d);
    //        FileInfo fileInfo = new FileInfo(t2d_path);
    //        string path = t2d_path.Replace(fileInfo.Name, "");
    //        string name = fileInfo.Name.Replace(fileInfo.Extension, "");

    //        string matPath = path + name + ".mat";
    //        string fontPath = path + name + ".fontsettings";

    //        if (File.Exists(matPath))
    //        {
    //            Debug.LogError("当前目录下存在相同名字的材质:" + matPath);
    //            return;
    //        }

    //        if (File.Exists(fontPath))
    //        {
    //            Debug.LogError("当前目录下存在相同名字的字体:" + fontPath);
    //            return;
    //        }

    //        Material mat = new Material(Shader.Find("GUI/Text Shader"));
    //        mat.SetTexture("_MainTex", t2d);
    //        AssetDatabase.CreateAsset(mat, matPath);
    //        font = new Font();
    //        font.material = mat;
    //        AssetDatabase.CreateAsset(font, fontPath);
    //        AssetDatabase.Refresh();

    //        Selection.objects = new UnityEngine.Object[] { mat, font };
    //    }

    //    private void OnGUI()
    //    {


    //        t2d = ObjField("Texture2D:", t2d);
    //        cellWidth = DrawInt("cellWidth:", cellWidth);
    //        if (t2d == null)
    //            return;
    //        if (GUILayout.Button("Create"))
    //        {
    //            Create();
    //        }
    //        GUILayout.Label(string.Format("创建到{0}文件路径下", t2d.name));
    //    }

    //    private void Create()
    //    {
    //        if (font == null)
    //        {
    //            CreateFont();
    //        }

    //        int w = t2d.width;
    //        int h = t2d.height;

    //        TextureImporter texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t2d));
    //        SpriteMetaData[] spDatas = texImport.spritesheet;

    //        List<CharacterInfo> charInfos = new List<CharacterInfo>();
    //        for (int i = 0; i < spDatas.Length; i++)
    //        {
    //            SpriteMetaData spData = spDatas[i];
    //            CharacterInfo info = new CharacterInfo();
    //            info.index = (int)spData.name.ToCharArray()[0];
    //            Debug.LogError(spData.name);
    //            Rect uv = new Rect(spData.rect.x / w, spData.rect.y / h, spData.rect.width / w, spData.rect.height / h);
    //            Rect vert = new Rect(0, spData.rect.height / 2, spData.rect.width, -spData.rect.height);
    //            info.uv = uv;
    //            info.vert = vert;

    //            int cw = (int)spData.rect.width;


    //            info.advance = cw + cellWidth;
    //            charInfos.Add(info);
    //        }
    //        font.characterInfo = charInfos.ToArray();
    //        EditorUtility.SetDirty(font);
    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }
    //}

    public class AnimitorClipConvertAnimationClip : BaseEditorWindow
    {
        [MenuItem("Tools/Other/AnimitorClipConvertAnimationClip")]
        static void Open()
        {
            AnimitorClipConvertAnimationClip re = GetWindow<AnimitorClipConvertAnimationClip>();
            re.minSize = new Vector2(300, 150);
            re.maxSize = new Vector2(300, 160);
            re.Show();
        }

        private AnimationClip atorClip;

        private void OnGUI()
        {
            atorClip = ObjField("atorClip", atorClip);
            if (GUILayout.Button("Convert"))
            {
                if (atorClip == null)
                    return;
                AnimationClip temp = Instantiate(atorClip);
                string path = AssetDatabase.GetAssetPath(atorClip);
                int lastIndex = path.LastIndexOf("/");
                path = path.Substring(0, lastIndex + 1) + atorClip.name + "Clip.anim";
                temp.legacy = true;
                AssetDatabase.CreateAsset(temp, path);
                AssetDatabase.Refresh();


                Selection.activeObject = AssetDatabase.LoadAssetAtPath(path.Replace(".anim", ""), typeof(AnimationClip));

            }
        }
    }

    public class GetPos
    {
        [MenuItem("Tools/Other/GetPos")]
        static void Open()
        {
            GameObject[] objs = Selection.gameObjects;
            string str = "{";
            foreach (var v in objs)
            {
                RectTransform rect = v.GetComponent<RectTransform>();
                str += string.Format("new Vector2({0}f,{1}f)", rect.anchoredPosition.x, rect.anchoredPosition.y) + ",";
            }
            str += "}";
            Debug.LogError(str);
        }
    }

    public class SequenceFrameAnim : BaseEditorWindow
    {
        [MenuItem("Tools/Other/SequenceFrameAnim")]
        static void Open()
        {
            SequenceFrameAnim re = GetWindow<SequenceFrameAnim>();
            re.minSize = new Vector2(328, 550);
            re.maxSize = new Vector2(328, 560);
            re.Show();
        }

        private float frameTime = 0.1f;
        private WrapMode wrapMode;
        private bool legacy = true;
        private int frameRate = 30;

        //private int spCount = 0;
        private List<Sprite> listSpr = new List<Sprite>();

        private Vector2 scrollView = Vector2.zero;

        private void OnGUI()
        {
            frameRate = DrawInt("frameTime:", frameRate);
            wrapMode = (WrapMode)EditorGUILayout.EnumPopup(wrapMode);
            legacy = DrawToggle("legacy:", legacy);
            frameTime = DrawFloat("frameTime:", frameTime);
            if (GUILayout.Button("Create"))
            {
                Create();
            }

            scrollView = GUILayout.BeginScrollView(scrollView);
            int index = 0;
            for (int i = 0; i < listSpr.Count; i++)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 6; j++)
                {
                    GUILayout.BeginVertical(GUILayout.Width(50));
                    listSpr[index] = (Sprite)EditorGUILayout.ObjectField(listSpr[index], typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
                    GUILayout.Label(index + ".", GUILayout.Width(50));
                    GUILayout.EndVertical();
                    index++;
                    if (listSpr.Count <= index)
                        break;
                }
                GUILayout.EndHorizontal();
                if (listSpr.Count <= index)
                    break;
            }
            if (GUILayout.Button("Add"))
            {
                listSpr.Add(null);
            }
            GUILayout.EndScrollView();
        }

        private void Create()
        {
            AnimationClip clip = new AnimationClip();
            clip.wrapMode = wrapMode;
            clip.legacy = legacy;
            clip.frameRate = frameRate;

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[listSpr.Count];
            //AnimationCurve ac = new AnimationCurve();
            float frameTime = 1 / 10f;
            for (int i = 0; i < listSpr.Count; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe();
                keyFrames[i].value = listSpr[i];
                keyFrames[i].time = frameTime * i;
            }

            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.type = typeof(SpriteRenderer);
            curveBinding.path = "";
            curveBinding.propertyName = "m_Sprite";
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
            AssetDatabase.CreateAsset(clip, "Assets/tt.anim");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public class AnimatorControllerCreator : BaseEditorWindow
    {
        [MenuItem("Tools/Other/AnimatorControllerCreator")]
        static void Open()
        {
            AnimatorControllerCreator re = GetWindow<AnimatorControllerCreator>();
            re.minSize = new Vector2(328, 350);
            re.maxSize = new Vector2(328, 360);
            re.Show();
            re.Init();
        }

        private string tempPath = @"D:\WorkSpaces\Bjqp\Client\Assets\LuaFramework\Editor\AnimatorControllerTemplate\";
        private string clipName = "AnimationClip.txt";
        private string controllerName = "AnimatorController.txt";

        private string ctrName;
        private List<string> clipNames = new List<string>();

        private void Init()
        {
            tempPath = "./Assets/LuaFramework/Editor/AnimatorControllerTemplate/";
        }

        private void OnGUI()
        {
            ctrName = DrawTextField("ctrName:", ctrName);

            int delIndex = -1;
            for (int i = 0; i < clipNames.Count; i++)
            {
                int ii = -1;
                clipNames[i] = ClipNameSet(i, clipNames[i], out ii);
                if (ii != -1)
                    delIndex = ii;
            }

            if (delIndex != -1)
            {
                clipNames.RemoveAt(delIndex);
            }

            if (GUILayout.Button("AddClip"))
            {
                clipNames.Add("");
            }
            if (GUILayout.Button("Create"))
            {
                Create();
            }
        }

        private string ClipNameSet(int index, string str, out int delIndex)
        {
            delIndex = -1;
            GUILayout.BeginHorizontal();
            GUILayout.Label(index + ".", GUILayout.Width(15));
            str = GUILayout.TextField(str);
            if (GUILayout.Button("del", GUILayout.Width(35)))
            {
                delIndex = index;
            }
            GUILayout.EndHorizontal();
            return str;
        }

        private void Create()
        {
            string controller = File.ReadAllText(tempPath + controllerName);
            string clip = File.ReadAllText(tempPath + clipName);

            string machineId = (System.DateTime.Now.Ticks + 1000).ToString();
            string newClip = string.Empty;

            for (int i = 0; i < clipNames.Count; i++)
            {
                string clipId = (System.DateTime.Now.Ticks + i).ToString();
                string v = clipNames[i];
                string c = string.Empty;
                c = clip.Replace("#ClipId", clipId);
                c = c.Replace("#Name", v);

                if (i > 0)
                {
                    newClip += "\n" + c;
                }
                else
                    newClip += c;

            }
            controller = controller.Replace("#MachineId", machineId);
            controller = controller.Replace("#Clip", newClip);
            controller = controller.Replace("#Name", ctrName);
            File.WriteAllText("Assets/" + ctrName + ".controller", controller);
            AssetDatabase.Refresh();
        }

    }

    public class ParticleOrderSetting : BaseEditorWindow
    {
        [MenuItem("Tools/Other/ParticleOrderSetting")]
        static void Open()
        {
            ParticleOrderSetting re = GetWindow<ParticleOrderSetting>();
            re.minSize = new Vector2(328, 350);
            re.maxSize = new Vector2(328, 360);
            re.Show();
        }

        private int minOrder;
        private int addOrder;
        void OnGUI()
        {
            minOrder = DrawInt("minOrder:", minOrder);
            if (GUILayout.Button("Setting"))
            {
                Transform root = (Selection.activeObject as GameObject).transform;
                Renderer[] rds = root.GetComponentsInChildren<Renderer>();

                int min = int.MaxValue;
                foreach (var v in rds)
                {
                    if (min > v.sortingOrder && v.enabled)
                        min = v.sortingOrder;
                }

                addOrder = minOrder - min;
                Debug.LogError(addOrder);
                foreach (var v in rds)
                {
                    if (v.enabled)
                        v.sortingOrder = v.sortingOrder + addOrder;
                }



            }
        }

    }

    public class UITextCreator : Editor
    {
        [MenuItem("GameObject/UI/MyText")]
        public static void Create()
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("Text"));
            go.name = "Text";
            //UnityEngine.UI.Text text = go.AddComponent<UnityEngine.UI.Text>();
            Transform p = (Selection.activeObject as GameObject).transform;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(p);
            rt.anchoredPosition3D = Vector3.zero;
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;
            Selection.activeObject = go;
        }
    }


    public class LoadScene : Editor
    {
        [MenuItem("Tools/Other/LoadScene1 #1")]
        public static void Scene1()
        {
            EditorSceneManager.OpenScene("Assets/Temp/UITest.unity");
        }
        [MenuItem("Tools/Other/LoadScene2 #2")]
        public static void Scene2()
        {
            EditorSceneManager.OpenScene("Assets/LuaFramework/Scenes/main.unity");
        }
    }

    public class MjSetting : BaseEditorWindow
    {
        [MenuItem("Tools/Other/MjSetting")]
        static void Open()
        {
            MjSetting re = GetWindow<MjSetting>();
            re.minSize = new Vector2(328, 350);
            re.maxSize = new Vector2(328, 360);
            re.Show();
            re.Init();
        }

        private RectTransform[] selects;

        private void Init()
        {
            GameObject obj = Selection.activeObject as GameObject;
            if (obj == null)
            {
                selects = new RectTransform[0];
                Debug.LogError("选择的对象下的子对象为空!");
                return;
            }
            selects = obj.GetComponentsInChildren<RectTransform>();
            List<RectTransform> list = new List<RectTransform>();
            foreach (var t in selects)
            {
                if (t.parent == obj.transform)
                {
                    list.Add(t);
                }
            }
            selects = list.ToArray();
        }

        private void OnGUI()
        {
            Sort();
        }

        private float v = 20;
        private float h = 50;
        private float s = 1;
        private void Sort()
        {
            float t_h = DrawFloat("x:", h);
            float t_v = DrawFloat("y:", v);
            float t_s = DrawFloat("s:", s);
            if (v != t_v || h != t_h || s != t_s)
            {
                v = t_v;
                h = t_h;
                s = t_s;

                Vector2 orPos = Vector2.zero;
                float orSize = 1;
                for (int i = 0; i < selects.Length; i++)
                {
                    RectTransform t = selects[i];
                    Vector2 size = t.Find("paiBei").GetComponent<RectTransform>().sizeDelta;
                    if (i == 0)
                    {
                        orPos = t.anchoredPosition;
                        orSize = size.x + size.y;
                        continue;
                    }

                    Vector2 pos = new Vector2(h * i, v * i) * ((size.x + size.y) / orSize * s) + orPos;
                    t.anchoredPosition = pos;
                }
            }
        }
    }

    public class FindChildGameObjects : BaseEditorWindow
    {
        [MenuItem("Tools/Other/FindChildGameObjects")]
        static void Open()
        {
            FindChildGameObjects re = GetWindow<FindChildGameObjects>();
            re.minSize = new Vector2(328, 350);
            re.maxSize = new Vector2(328, 360);
            re.Show();
        }

        private string objName = "";
        private void OnGUI()
        {
            objName = DrawTextField("Name:", objName);
            if (GUILayout.Button("FindChild"))
            {
                GameObject go = Selection.activeObject as GameObject;
                if (go == null || string.IsNullOrEmpty(objName))
                    return;
                Transform[] ts = go.transform.GetComponentsInChildren<Transform>(true);
                List<GameObject> select = new List<GameObject>();
                foreach (var t in ts)
                {
                    GameObject obj = t.gameObject;
                    if (obj.name.Equals(objName) && obj.activeSelf)
                    {
                        select.Add(obj);
                    }
                }
                Selection.objects = select.ToArray();
            }
        }
    }

    public class GetAnimatopnClipInfo : BaseEditorWindow
    {
        [MenuItem("Tools/Other/GetAnimatopnClipInfo")]
        static void Open()
        {
            GetAnimatopnClipInfo re = GetWindow<GetAnimatopnClipInfo>();
            re.minSize = new Vector2(328, 350);
            re.maxSize = new Vector2(328, 360);
            re.Show();
        }

        private AnimationClip clip;
        private EditorCurveBinding[] ecbs;
        private bool[] togs;

        private void OnGUI()
        {
            if (ecbs == null)
            {
                clip = ObjField("AnimaClip:", clip);
                if (GUILayout.Button("Get"))
                {
                    List<EditorCurveBinding> list = new List<EditorCurveBinding>();
                    EditorCurveBinding[] objCurve = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    EditorCurveBinding[] valueCurve = AnimationUtility.GetCurveBindings(clip);
                    list.AddRange(valueCurve);
                    list.AddRange(objCurve);
                    ecbs = list.ToArray();
                    togs = new bool[ecbs.Length];


                    //Temp 
                    //EditorCurveBinding oc = objCurve[0];
                    //ObjectReferenceKeyframe[] acs = AnimationUtility.GetObjectReferenceCurve(clip, oc);
                    //Debug.LogError(acs.Length+"  "+oc.type);
                    //AnimationCurve ac = new AnimationCurve();
                    //AnimationUtility.SetObjectReferenceCurve(clip, oc, acs);
                }
            }
            else
            {
                int c = 1;
                foreach (var v in ecbs)
                {
                    Draw(v, c++);
                }
                if (GUILayout.Button("Get Code"))
                {
                    GetCode();
                }
            }
        }

        private void DraeEditorCurveBinding(EditorCurveBinding e, int index)
        {
            if (GUILayout.Button(string.Format("{0}. {1}:{2}", index, e.path, e.propertyName)))
            {
                AnimationCurve ac = AnimationUtility.GetEditorCurve(clip, e);
                Keyframe[] keys = ac.keys;
                int c = 1;
                string outContent = "keys[{0}]=UnityEngine.Keyframe.New({1},{2},{3},{4})";
                string keysContent = "local keys={};\n";
                foreach (var v in keys)
                {
                    keysContent += string.Format(outContent, c++, v.time, v.value, v.inTangent, v.outTangent) + "\n";
                }

                string content = keysContent;
                content += "local curve= UnityEngine.AnimationCurve.New();\n";
                content += "curve.preWrapMode=UnityEngine.WrapMode." + ac.preWrapMode.ToString() + "\n";
                content += "curve.postWrapMode=UnityEngine.WrapMode." + ac.postWrapMode.ToString() + "\n";
                content += "curve.keys=keys\n";
                content += string.Format("animClip:SetCurve(\"{0}\",typeof({1}),\"{2}\",{3})", e.path, e.type, e.propertyName, "curve");
                Debug.LogError(content);
            }
        }


        private void Draw(EditorCurveBinding e, int index)
        {
            GUILayout.BeginHorizontal();
            togs[index - 1] = EditorGUILayout.Toggle(togs[index - 1], GUILayout.Width(25));
            GUILayout.Label(index + "." + e.propertyName);
            GUILayout.EndHorizontal();
        }

        private void GetCode()
        {
            string content = "local curve=UnityEngine.AnimationCurve.New();\n";
            int c = ecbs.Length;
            int index = 0;
            for (int i = 0; i < c; i++)
            {
                if (togs[i] == false)
                    continue;
                EditorCurveBinding e = ecbs[i];
                string outContent = "";
                if (index != 0)
                {
                    outContent = "curve=UnityEngine.AnimationCurve.New();\n";
                }
                index++;
                AnimationCurve ac = AnimationUtility.GetEditorCurve(clip, e);
                Keyframe[] keys = ac.keys;
                foreach (var v in keys)
                {
                    outContent += string.Format("curve:AddKey({0},{1});", v.time, v.value) + "\n";
                }
                content += outContent;
                content += string.Format("animClip:SetCurve(\"{0}\",typeof({1}),\"{2}\",{3})", e.path, e.type, e.propertyName, "curve") + "\n\n";
            }
            Debug.LogError(content);
        }
    }


    public class ParticleSetting : BaseEditorWindow
    {
        [MenuItem("Tools/Other/ParticleSetting")]
        static void Open()
        {
            ParticleSetting re = GetWindow<ParticleSetting>();
            re.minSize = new Vector2(428, 350);
            re.maxSize = new Vector2(428, 360);
            re.Show();
        }

        private GameObject selectObj;
        private GameObject curObj;
        private List<Renderer> list;
        private int order = 0;
        private Vector2 scroll = Vector2.zero;
        private void OnGUI()
        {
            if (curObj != selectObj)
            {
                curObj = selectObj;
                if (curObj == null)
                {
                    list = null;
                    return;
                }
                list = new List<Renderer>();
                Renderer[] ps = curObj.GetComponentsInChildren<Renderer>(true);
                list.AddRange(ps);
            }

            order = DrawInt("Order:", order);

            if (list == null)
                return;

            if (GUILayout.Button("Set"))
            {
                SetParticleOrder();
            }

            scroll = GUILayout.BeginScrollView(scroll);
            int index = 1;
            foreach (var v in list)
            {
                GUILayout.BeginHorizontal();
                ObjField(index + ".order:" + v.sortingOrder, v);
                EditorGUILayout.ObjectField(v.sharedMaterial, typeof(Material), true);
                GUILayout.EndHorizontal();
                index++;
            }
            GUILayout.EndScrollView();
        }

        private void Update()
        {
            selectObj = Selection.activeObject as GameObject;
        }

        private void SetParticleOrder()
        {
            foreach (var v in list)
            {
                Renderer r = v;
                r.sortingOrder = order;
            }
            EditorUtility.SetDirty(curObj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


    public class OpenAtlasEditor : Editor
    {
        [MenuItem("Assets/OpenAtlasEditor", false, 0)]
        public static void Open()
        {
            Object obj = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(obj) + "/";
            ImageEditorWindow.Open(path);
        }
    }

    public class ProjectTempCopy : Editor
    {
        private static string outPath = @"D:\WorkSpaces\Bjqp\ClientTemp\";
        private static string resPath;
        private static string rootName;
        private static int theardCount;
        private static int curCount;

        [MenuItem("Assets/ProjectTempCopy")]
        public static void Copy()
        {
            Object obj = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                path = "Assets";
            if (!Directory.Exists(path))
                return;
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            resPath = dirInfo.Parent.FullName;
            rootName = dirInfo.Name + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss");
            Directory.CreateDirectory(outPath + "/" + rootName);
            CopyDir(dirInfo);
            EditorUtility.ClearProgressBar();

            EditorApplication.update += Update;
        }

        private static void CopyDir(DirectoryInfo dirInfo)
        {
            EditorUtility.DisplayProgressBar(dirInfo.Name, dirInfo.FullName, 0);
            if (ThreadPool.QueueUserWorkItem(CopyFiles, dirInfo))
                theardCount++;
            DirectoryInfo[] dirs = dirInfo.GetDirectories();
            foreach (var info in dirs)
            {
                CopyDir(info);
            }
        }

        private static void CopyFiles(object state)
        {
            DirectoryInfo dirInfo = state as DirectoryInfo;
            string path = outPath + rootName + dirInfo.FullName.Replace(resPath, "");
            Directory.CreateDirectory(path);
            FileInfo[] fileInfos = dirInfo.GetFiles();
            foreach (var info in fileInfos)
            {
                info.CopyTo(path + "/" + info.Name);
            }
            curCount++;
        }

        private static void Update()
        {
            EditorUtility.DisplayProgressBar(theardCount.ToString(), theardCount + "/" + curCount, (float)curCount / theardCount);
            if (theardCount == curCount)
            {
                EditorApplication.update -= Update;
                EditorUtility.ClearProgressBar();
                return;
            }
        }
    }

    //查找空材质
    public class FindNullMaterial : Editor
    {
        [MenuItem("Assets/FindNullMaterial")]
        static void Find()
        {
            Object obj = Selection.activeObject;
            if (obj == null)
                return;
            if (obj.GetType() != typeof(DefaultAsset))
                return;
            string path = AssetDatabase.GetAssetPath(obj);
            string[] pts = AssetDatabase.FindAssets("", new string[] { path });
            List<string> list = new List<string>();
            foreach (var v in pts)
            {
                if (list.Contains(v))
                    continue;
                list.Add(v);
            }

            foreach (var p in list)
            {
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(p));

                foreach (var v in objs)
                {
                    Renderer r = v as Renderer;
                    if (r)
                    {
                        if (r.sharedMaterial == null)
                            Debug.LogError(r + "  " + r.transform.root.name);

                    }
                }
            }
        }

    }

  


    public class GetColorValue : Editor
    {
        [MenuItem("GameObject/UI/GetColorValue")]
        static void Get()
        {
            GameObject go = Selection.activeObject as GameObject;
            if (go == null)
                return;
            Graphic g = go.GetComponent<Graphic>();
            Color c = Color.white;
            if (g != null)
            {
                c = g.color;
                string v = string.Format("{0},{1},{2},{3}", c.r, c.g, c.b, c.a);
                Debug.LogError(v);
            }

        }
    }

    public class EditorTest : Editor
    {
        [MenuItem("Tools/EditorTest")]
        static void Get()
        {
            string[] strs = AssetDatabase.GetLabels(Selection.activeObject);

            foreach (var v in strs)
            {
                Debug.LogError("Labels:" + v);
            }
        }
    }

    public class EditorBuild : Editor
    {
        void Init()
        {

        }
    }

    public class SortTransform : BaseEditorWindow
    {
        [MenuItem("Tools/SortTransform")]
        static void Get()
        {
            Rect wr = new Rect(0, 0, 300, 100);
            SortTransform window = (SortTransform)GetWindowWithRect(typeof(SortTransform), wr, true, "UpdatePack");
            window.Show();
            window.Init();
        }

        float x, tx = 0;
        float y, ty = 0;
        float z, tz = 0;
        private Transform rootTrans, tempRoot;
        private List<Transform> list;

        private void Init()
        {
            rootTrans = Selection.activeTransform;
            if (rootTrans == null)
            {
                Debug.LogError("没有选择对象");
                Close();
                return;
            }
            list = new List<Transform>();
        }

        private void SetSortList()
        {
            list.Clear();
            Transform[] ts = rootTrans.GetComponentsInChildren<Transform>();
            foreach (var v in ts)
            {
                if (v == rootTrans)
                    continue;
                if (v.parent == rootTrans)
                    list.Add(v);
            }
            if (list.Count <= 1)
                return;

            Vector3 dtPos = list[1].localPosition - list[0].localPosition;
            x = dtPos.x;
            y = dtPos.y;
            z = dtPos.z;

            tx = x;
            ty = y;
            tz = z;
        }


        private void OnGUI()
        {
            if (rootTrans == null)
                return;
            rootTrans = ObjField("root", rootTrans);
            if (rootTrans != tempRoot)
            {
                tempRoot = rootTrans;
                SetSortList();
            }
            if (list.Count == 0)
            {
                GUILayout.Label("Root下根节点数量为0");
                return;
            }
            x = DrawFloat("x:", x);
            y = DrawFloat("y:", y);
            z = DrawFloat("z:", z);

            if (x != tx || y != ty || z != tz)
            {
                tx = x;
                ty = y;
                tz = z;
                Vector3 startPos = list[0].localPosition;
                Vector3 offectPos = new Vector3(x, y, z);
                for (int i = 1; i < list.Count; i++)
                {
                    startPos += offectPos;
                    list[i].localPosition = startPos;
                }
            }
        }
    }


}

public class TableNameJsonClass
{
    public List<string> tableName;

    public TableNameJsonClass()
    {

    }
}

public class DealTabelNameJson
{
    public static TableNameJsonClass myJsonOBJ;

    public static TableNameJsonClass GetTableName()
    {
        string t_name = PlayerPrefs.GetString("tabelName");
        if (string.IsNullOrEmpty(t_name))
            t_name = "{ \"tableName\" :[] }";
        myJsonOBJ = JsonUtility.FromJson<TableNameJsonClass>(t_name);
        return myJsonOBJ;
    }

    public static void AddTableName(string addTableName)
    {
        if (myJsonOBJ.tableName.Exists(x => x == addTableName))
            return;
        myJsonOBJ.tableName.Add(addTableName);
        string saveData = JsonUtility.ToJson(myJsonOBJ);
        PlayerPrefs.SetString("tabelName", saveData);
    }
}

public class OtherTools: Editor
{
    [MenuItem("Tools/ClearPlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
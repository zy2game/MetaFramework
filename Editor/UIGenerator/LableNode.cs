using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using PhotoshopFile;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class LableNode : ViewNode, UIComponent
    {
        public sealed class Data : NodeData
        {
            public int language { get; set; }
            public string text { get; set; }
            public string fontName { get; set; }
            public int fontSize { get; set; }
            public int LineSpacing { get; set; }
            public TextAnchor anchor { get; set; }
            public HorizontalWrapMode horizontal { get; set; }
            public VerticalWrapMode vertical { get; set; }
            public bool RaycastTarget { get; set; }
            public bool Maskable { get; set; }
            public Color color = Color.white;
            public string material;
        }

        public Data Datable;
        public Material material;
        public LableNode()
        {
            this.AddPort<LableNode>("Content", Direction.Output, Capacity.Multi);
        }

        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                Datable = new Data();
                return;
            }
            Datable = (Data)data;
            if (!string.IsNullOrEmpty(Datable.material))
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(Datable.material));
            }
        }
        public override NodeData GetNodeData()
        {
            Datable.material = AssetDatabase.GetAssetPath(material);
            return Datable;
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Text");
                Datable.text = GUILayout.TextField(Datable.text);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Font Size");
                Datable.fontSize = EditorGUILayout.IntField(Datable.fontSize);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Color");
                Datable.color = EditorGUILayout.ColorField("", Datable.color);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Material");
                material = (Material)EditorGUILayout.ObjectField("", material, typeof(Material), false);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("LineSpacing");
                Datable.LineSpacing = EditorGUILayout.IntField(Datable.LineSpacing);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Algnment");
                Datable.anchor = (TextAnchor)EditorGUILayout.EnumPopup(Datable.anchor);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Horizontal Overflow");
                Datable.horizontal = (HorizontalWrapMode)EditorGUILayout.EnumPopup(Datable.horizontal);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Vertical Overflow");
                Datable.vertical = (VerticalWrapMode)EditorGUILayout.EnumPopup(Datable.vertical);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Raycast Target");
                Datable.RaycastTarget = GUILayout.Toggle(Datable.RaycastTarget, "");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Maskable");
                Datable.Maskable = GUILayout.Toggle(Datable.Maskable, "");
                GUILayout.EndHorizontal();
            }
        }
        public static LableNode Create(string guid, Layer layer)
        {
            LableNode lable = new LableNode();
            lable.Initialized(null);
            lable.Datable.text = layer.Text;
            lable.Datable.fontSize = (int)layer.FontSize;
            if (layer.Justification == TextJustification.Left)
            {
                lable.Datable.anchor = TextAnchor.MiddleLeft;
            }
            if (layer.Justification == TextJustification.Right)
            {
                lable.Datable.anchor = TextAnchor.MiddleRight;
            }
            if (layer.Justification == TextJustification.Center)
            {
                lable.Datable.anchor = TextAnchor.MiddleCenter;
            }
            lable.Datable.fontName = layer.FontName;
            lable.Datable.RaycastTarget = true;
            lable.Datable.vertical = VerticalWrapMode.Overflow;
            lable.Datable.horizontal = HorizontalWrapMode.Overflow;
            lable.Datable.guid = guid;
            return lable;
        }

        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            if (target.GetComponent<UnityEngine.UI.Text>() != null)
            {
                return;
            }
            UnityEngine.UI.Text component = target.AddComponent<UnityEngine.UI.Text>();
            component.text = Datable.text;
            ;
            component.font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(Datable.fontName).First()));
            component.fontSize = Datable.fontSize;
            component.lineSpacing = Datable.LineSpacing;
            component.alignment = Datable.anchor;
            component.horizontalOverflow = Datable.horizontal;
            component.verticalOverflow = Datable.vertical;
            component.raycastTarget = Datable.RaycastTarget;
            component.maskable = Datable.Maskable;
            component.material = material;
            component.color = Datable.color;
        }

        public override GameObject GenerateGameObject(Transform parent, string savedAssetPath)
        {
            GameObject obj = new GameObject();
            RectTransform transform = obj.AddComponent<RectTransform>();
            transform.parent = parent;
            transform.name = "text";
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;
            transform.pivot = Vector2.one / 2f;
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            return obj;
        }
    }
}
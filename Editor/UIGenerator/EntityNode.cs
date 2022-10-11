using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using PhotoshopFile;

namespace GameFramework.Editor.UIGenerator
{
    public class EntityNode : ViewNode
    {
        public enum LayoutType : byte
        {
            Center,
            CenterLeft,
            CenterRight,
            Top,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Bottom,
            FullScreen,
        }
        public sealed class Data : NodeData
        {
            public string name { get; set; }
            public LayoutType uilayout { get; set; }
            public Vector3 position { get; set; }
            public Vector3 rotation { get; set; }
            public Vector3 scale { get; set; }
            public Vector2 pivot { get; set; }
            public Vector2 size { get; set; }

            public Data()
            {
                uilayout = LayoutType.Center;
                position = Vector3.zero;
                rotation = Vector3.zero;
                scale = Vector3.one;
                pivot = new Vector2(0.5f, 0.5f);
                size = Vector2.one;
            }
        }
        public Data Datable;
        public EntityNode()
        {
            this.AddPort<UIComponent>("Components", Direction.Input, Capacity.Multi);
            this.AddPort<EntityNode>("Childs", Direction.Input, Capacity.Multi);
            this.AddPort<EntityNode>("Content", Direction.Output, Capacity.Single);
        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            Datable = (Data)data;
        }
        public override NodeData GetNodeData()
        {
            return Datable;
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("name", GUILayout.Width(40));
                Datable.name = GUILayout.TextField(Datable.name);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Layout");
                Datable.uilayout = (LayoutType)EditorGUILayout.EnumPopup(Datable.uilayout);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Pos", GUILayout.Width(40));
                Datable.position = EditorGUILayout.Vector3Field("", Datable.position);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Rota", GUILayout.Width(40));
                Datable.rotation = EditorGUILayout.Vector3Field("", Datable.rotation);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Scale", GUILayout.Width(40));
                Datable.scale = EditorGUILayout.Vector3Field("", Datable.scale);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Pivot", GUILayout.Width(40));
                Datable.pivot = EditorGUILayout.Vector2Field("", Datable.pivot);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Size", GUILayout.Width(40));
                Datable.size = EditorGUILayout.Vector2Field("", Datable.size);
                GUILayout.EndHorizontal();
            }
        }

        public void SetPsdLayer(Layer layer)
        {
            if (!layer.HasLayer())
            {
                Datable.position = Vector3.zero;
            }
            else
            {
                Datable.position = new Vector3(layer.Rect.position.x + layer.Rect.width / 2 - layer.PsdFile.ColumnCount / 2, -(layer.Rect.position.y + layer.Rect.height / 2 - layer.PsdFile.RowCount / 2));
            }
            Datable.scale = Vector3.one;
            Datable.size = layer.Rect.size;
        }
        public override GameObject GenerateGameObject(Transform parent,string savedAssetPath)
        {
            GameObject obj = new GameObject(Datable.name);
            RectTransform transform = obj.AddComponent<RectTransform>();
            switch (Datable.uilayout)
            {
                case LayoutType.Center:
                    transform.anchorMin = new Vector2(0.5f, 0.5f);
                    transform.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case LayoutType.CenterLeft:
                    transform.anchorMin = new Vector2(0f, 0.5f);
                    transform.anchorMax = new Vector2(0f, 0.5f);
                    break;
                case LayoutType.CenterRight:
                    transform.anchorMin = new Vector2(0.5f, 0.0f);
                    transform.anchorMax = new Vector2(0.5f, 0.0f);
                    break;
                case LayoutType.Top:
                    transform.anchorMin = new Vector2(0.5f, 1.0f);
                    transform.anchorMax = new Vector2(0.5f, 1.0f);
                    break;
                case LayoutType.TopLeft:
                    transform.anchorMin = new Vector2(0.0f, 1.0f);
                    transform.anchorMax = new Vector2(0.0f, 1.0f);
                    break;
                case LayoutType.TopRight:
                    transform.anchorMin = new Vector2(1.0f, 1.0f);
                    transform.anchorMax = new Vector2(1.0f, 1.0f);
                    break;
                case LayoutType.Bottom:
                    transform.anchorMin = new Vector2(0.5f, 0.0f);
                    transform.anchorMax = new Vector2(0.5f, 0.0f);
                    break;
                case LayoutType.BottomLeft:
                    transform.anchorMin = new Vector2(0.0f, 0.0f);
                    transform.anchorMax = new Vector2(0.0f, 0.0f);
                    break;
                case LayoutType.BottomRight:
                    transform.anchorMin = new Vector2(1.0f, 0.0f);
                    transform.anchorMax = new Vector2(1.0f, 0.0f);
                    break;
                case LayoutType.FullScreen:
                    transform.anchorMin = new Vector2(0.0f, 0.0f);
                    transform.anchorMax = new Vector2(1.0f, 1.0f);
                    break;
            }
            transform.parent = parent;
            transform.name = Datable.name;
            transform.pivot = Datable.pivot;
            transform.localPosition = Datable.position - parent.localPosition;
            transform.localRotation = Quaternion.Euler(Datable.rotation);
            transform.localScale = Datable.scale;
            transform.sizeDelta = Datable.size;

            Port port = this.GetPort("Components");
            foreach (var item in port.connections)
            {
                ViewNode node = (ViewNode)item.output.node;
                if (node == null)
                {
                    continue;
                }
                node.GenerateComponent(obj, savedAssetPath);
            }

            port = this.GetPort("Childs");
            foreach (var item in port.connections)
            {
                ViewNode node = (ViewNode)item.output.node;
                if (node == null)
                {
                    continue;
                }
                node.GenerateGameObject(obj.transform, savedAssetPath);
            }
            return obj;
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using System.Linq;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class ScrollViewNode : ViewNode, UIComponent
    {
        private Data datable;
        public sealed class Data : NodeData
        {
            public bool horizontal;
            public bool vertical;
            public UnityEngine.UI.ScrollRect.ScrollbarVisibility hscrollbar;
            public UnityEngine.UI.ScrollRect.ScrollbarVisibility vscrollbar;
            public float hspacing = -3;
            public float vspacing = -3;
        }
        public ScrollViewNode()
        {
            this.AddPort<ScrollViewNode>("Content", Direction.Output, Capacity.Single);
            this.AddPort<EntityNode>("Handle", Direction.Input, Capacity.Single);
            this.AddPort<EntityNode>("H Scrollbar", Direction.Input, Capacity.Single);
            this.AddPort<EntityNode>("V Scrollbar", Direction.Input, Capacity.Single);

        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            datable = (Data)data;
        }
        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            UnityEngine.UI.ScrollRect scrollRect = target.AddComponent<UnityEngine.UI.ScrollRect>();
            scrollRect.horizontal = datable.horizontal;
            scrollRect.vertical = datable.vertical;
            Edge edge = this.GetPort("Handle").connections.FirstOrDefault();
            if (edge != null)
            {
                EntityNode entity = (EntityNode)edge.output.node;
                scrollRect.content = entity.GenerateGameObject(target.transform, savedAssetPath).GetComponent<RectTransform>();
            }
            edge = this.GetPort("H Scrollbar").connections.FirstOrDefault();
            if (edge != null)
            {
                EntityNode entity = (EntityNode)edge.output.node;
                scrollRect.horizontalScrollbar = entity.GenerateGameObject(target.transform, savedAssetPath).GetComponent<UnityEngine.UI.Scrollbar>();
                scrollRect.horizontalScrollbarVisibility = datable.hscrollbar;
                scrollRect.horizontalScrollbarSpacing = datable.hspacing;
            }
            edge = this.GetPort("V Scrollbar").connections.FirstOrDefault();
            if (edge != null)
            {
                EntityNode entity = (EntityNode)edge.output.node;
                scrollRect.verticalScrollbar = entity.GenerateGameObject(target.transform, savedAssetPath).GetComponent<UnityEngine.UI.Scrollbar>();

                scrollRect.verticalScrollbarVisibility = datable.vscrollbar;
                scrollRect.verticalScrollbarSpacing = datable.vspacing;
            }
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Horizontal");
                datable.horizontal = GUILayout.Toggle(datable.horizontal, "");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Vertical");
                datable.vertical = GUILayout.Toggle(datable.vertical, "");
                GUILayout.EndHorizontal();
            }
            if (this.GetPort("H Scrollbar").connections.Count() > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Horizontal Scrollbar");
                    Edge edge = this.GetPort("H Scrollbar").connections.FirstOrDefault();
                    EntityNode entity = (EntityNode)edge.output.node;
                    GUILayout.Label(entity.Datable.name);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Visiblility");
                    datable.hscrollbar = (UnityEngine.UI.ScrollRect.ScrollbarVisibility)EditorGUILayout.EnumPopup("", datable.hscrollbar);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Spacing");
                    datable.hspacing = EditorGUILayout.FloatField("", datable.hspacing);
                    GUILayout.EndHorizontal();
                }
            }

            if (this.GetPort("V Scrollbar").connections.Count() > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Vertical Scrollbar");
                    Edge edge = this.GetPort("V Scrollbar").connections.FirstOrDefault();
                    EntityNode entity = (EntityNode)edge.output.node;
                    GUILayout.Label(entity.Datable.name);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Visiblility");
                    datable.vscrollbar = (UnityEngine.UI.ScrollRect.ScrollbarVisibility)EditorGUILayout.EnumPopup("", datable.vscrollbar);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Spacing");
                    datable.vspacing = EditorGUILayout.FloatField("", datable.vspacing);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
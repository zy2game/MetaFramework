using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.UI.Selectable;
using System.Linq;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class ScrollbarNode : ViewNode, UIComponent
    {
        private Data datable;
        public sealed class Data : NodeData
        {
            public UnityEngine.UI.Scrollbar.Direction direction;
            public float value;
            public float size;

        }
        public ScrollbarNode()
        {
            this.AddPort<ScrollbarNode>("Content", Direction.Output, Capacity.Single);
            this.AddPort<EntityNode>("Handle", Direction.Input, Capacity.Single);
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
            UnityEngine.UI.Scrollbar scrollbar = target.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollbar.transition = Transition.None;
            scrollbar.value = datable.value;
            scrollbar.size = datable.size;
            scrollbar.direction = datable.direction;
            Edge edge = this.GetPort("Handle").connections.FirstOrDefault();
            if (edge != null)
            {
                EntityNode entity = (EntityNode)edge.output.node;
                scrollbar.handleRect = entity.GenerateGameObject(target.transform, savedAssetPath).GetComponent<RectTransform>();
            }
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Direction");
                datable.direction = (UnityEngine.UI.Scrollbar.Direction)EditorGUILayout.EnumPopup("", datable.direction);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Value");
                datable.value = EditorGUILayout.Slider(datable.value, 0, 1);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Size");
                datable.size = EditorGUILayout.Slider(datable.size, 0, 1);
                GUILayout.EndHorizontal();
            }
        }
    }
}
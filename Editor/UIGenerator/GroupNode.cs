using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class GroupNode : ViewNode, UIComponent
    {
        public sealed class Data : NodeData
        {
            public UnityEngine.UI.GridLayoutGroup.Corner corner;
            public UnityEngine.UI.GridLayoutGroup.Constraint constraint;
            public UnityEngine.UI.GridLayoutGroup.Axis axis;
            public RectInt Padding;
            public Vector2 cellsize;
            public Vector2 spacing;
            public UnityEngine.TextAnchor childalignment;
            public int count;
        }

        private Data datable;
        public GroupNode()
        {
            this.AddPort<GroupNode>("Content", Direction.Output, Capacity.Single);
        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            datable = (Data)data;
        }
        public override NodeData GetNodeData()
        {
            return datable;
        }
        public override void GenerateComponent(GameObject target,string savedAssetPath)
        {
            UnityEngine.UI.GridLayoutGroup group = target.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            group.padding = new RectOffset(datable.Padding.xMin, datable.Padding.xMax, datable.Padding.yMin, datable.Padding.yMax);
            group.cellSize = datable.cellsize;
            group.spacing = datable.spacing;
            group.startCorner = datable.corner;
            group.startAxis = datable.axis;
            group.childAlignment = datable.childalignment;
            group.constraint = datable.constraint;
            group.constraintCount = datable.count;
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Padding");
                datable.Padding = EditorGUILayout.RectIntField("", datable.Padding);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Cell Size");
                datable.cellsize = EditorGUILayout.Vector2Field("", datable.cellsize);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Spacing");
                datable.spacing = EditorGUILayout.Vector2Field("", datable.spacing);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Start Corner");
                datable.corner = (UnityEngine.UI.GridLayoutGroup.Corner)EditorGUILayout.EnumPopup(datable.corner);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Start Axis");
                datable.axis = (UnityEngine.UI.GridLayoutGroup.Axis)EditorGUILayout.EnumPopup(datable.axis);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Child Alignment");
                datable.childalignment = (UnityEngine.TextAnchor)EditorGUILayout.EnumPopup(datable.childalignment);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Constraint");
                datable.constraint = (UnityEngine.UI.GridLayoutGroup.Constraint)EditorGUILayout.EnumPopup(datable.constraint);
                GUILayout.EndHorizontal();
            }
            if (datable.constraint != UnityEngine.UI.GridLayoutGroup.Constraint.Flexible)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Count");
                    datable.count = EditorGUILayout.IntField("", datable.count);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
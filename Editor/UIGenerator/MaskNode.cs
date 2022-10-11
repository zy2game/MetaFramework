using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class MaskNode : ViewNode, UIComponent
    {
        private Data datable;
        public sealed class Data : NodeData
        {
            public bool showImage;
        }
        public MaskNode()
        {
            this.AddPort<MaskNode>("Content", Direction.Output, Capacity.Single);
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
        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            target.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = datable.showImage;
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Show Mask Graphic");
                datable.showImage = GUILayout.Toggle(datable.showImage, "");
                GUILayout.EndHorizontal();
            }
        }
    }
}
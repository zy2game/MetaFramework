using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class EffectNode : EntityNode, UIEvent, UIComponent
    {
        public EffectNode()
        {
            this.AddPort<EffectNode>("Content", Direction.Output, Capacity.Multi);
        }
    }
}
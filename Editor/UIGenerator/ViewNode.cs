using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using System.Linq;

namespace GameFramework.Editor.UIGenerator
{
    public interface UIComponent
    {

    }
    public interface UIEvent
    {

    }
    public abstract class ViewNode : Node
    {
        public string guid { get; set; }
        private Dictionary<string, Port> portDic;
        public ViewNode()
        {
            this.titleContainer.style.backgroundColor = UIGeneratorWindow.TiletStyleColor;
            this.outputContainer.style.backgroundColor = UIGeneratorWindow.BackgroundColor;
            this.inputContainer.style.backgroundColor = UIGeneratorWindow.BackgroundColor;
            this.extensionContainer.style.backgroundColor = UIGeneratorWindow.BackgroundColor;
            this.extensionContainer.Add(new IMGUIContainer(OnInspector));
            this.name = this.GetType().Name.Replace("Node", "");
            this.title = this.name;
            this.guid = Guid.NewGuid().ToString();
            this.portDic = new Dictionary<string, Port>();
        }

        public void AutoLayout(int columns)
        {
            Dictionary<int, List<ViewNode>> dic = new Dictionary<int, List<ViewNode>>();
            dic.Add(columns, new List<ViewNode>() { this });
            GetAllNodes(++columns, dic);
            Rect rect = new Rect(1000, 0, 100, 100);
            foreach (var item in dic.Values)
            {
                rect.y = item.Count / 2 * 300;
                foreach (var node in item)
                {
                    node.SetPosition(new Rect(rect.x, rect.y -= 300, 100, 100));
                }
                rect.x -= 350;
            }
        }

        private float GetUsegeHeight()
        {
            float childHeight = 0;
            foreach (var item in portDic.Values)
            {
                if (item.direction == Direction.Output)
                {
                    continue;
                }

                foreach (var edge in item.connections)
                {
                    ViewNode node = (ViewNode)edge.input.node;
                    childHeight += node.GetUsegeHeight();
                }
            }


            return 0;
        }

        private void GetAllNodes(int deep, Dictionary<int, List<ViewNode>> dic)
        {
            foreach (var item in portDic.Values)
            {
                if (item.direction == Direction.Output)
                {
                    continue;
                }
                Rect posotion = this.GetPosition();
                float offset = item.connections.Count() / 2 * posotion.height;
                if (!dic.TryGetValue(deep, out List<ViewNode> nodes))
                {
                    nodes = new List<ViewNode>();
                    dic.Add(deep, nodes);
                }
                for (var i = 0; i < item.connections.Count(); i++)
                {
                    Edge edge = item.connections.ElementAt(i);
                    ViewNode child = (ViewNode)edge.output.node;
                    nodes.Add(child);
                    child.GetAllNodes(deep + 1, dic);
                }
            }
        }


        public virtual void Initialized(NodeData data) { }
        public virtual void OnInspector() { }
        public virtual NodeData GetNodeData() { return default; }
        public override void OnSelected() { UIGeneratorWindow.window.OnSelectionNode(this, true); }
        public override void OnUnselected() { UIGeneratorWindow.window.OnSelectionNode(this, false); }
        public Port GetPort(string name)
        {
            if (portDic.TryGetValue(name, out Port port))
            {
                return port;
            }
            return default;
        }
        public void RemovePort(string name)
        {
            if (portDic.TryGetValue(name, out Port port))
            {
                if (this.inputContainer.Contains(port))
                {
                    this.inputContainer.Remove(port);
                }
                if (this.outputContainer.Contains(port))
                {
                    this.outputContainer.Remove(port);
                }

                port.DisconnectAll();
                portDic.Remove(name);
                this.Refresh();
            }
        }
        public void AddPort<T>(string name, Direction dir, Capacity capacity)
        {
            if (portDic.TryGetValue(name, out Port port))
            {
                throw new Exception("the port is already exist");
            }
            port = this.InstantiatePort(Orientation.Horizontal, dir, capacity, typeof(T));
            port.portColor = Color.green;
            port.name = name;
            port.portName = name;
            portDic.Add(name, port);
            if (dir == Direction.Input)
            {
                this.inputContainer.Add(port);
            }
            else
            {
                this.outputContainer.Add(port);
            }
            Refresh();
        }

        public EntityNode GetEntityNode()
        {
            Port outPort = GetPort("Content");
            if (outPort.connections.Count() <= 0)
            {
                return default;
            }
            foreach (var item in outPort.connections)
            {
                if (item.input.node is EntityNode entity)
                {
                    return entity;
                }
                entity = ((ViewNode)item.input.node).GetEntityNode();
                if (entity != null)
                {
                    return entity;
                }
            }
            return default;
        }
        public void Refresh()
        {
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
        public virtual GameObject GenerateGameObject(Transform parent, string savedAssetPath)
        {
            return default;
        }
        public virtual void GenerateComponent(GameObject target, string savedAssetPath)
        {

        }


    }
}
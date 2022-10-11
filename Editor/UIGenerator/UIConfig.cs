using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using PhotoshopFile;
using System.Text;
using GameFramework.Utils;
using System.IO;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class UIConfig
    {
        public class Linker
        {
            public string source;
            public string sourcePort;
            public string target;
            public string targetPort;

            public Linker()
            {

            }

            public Linker(ViewNode sourceNode, ViewNode targetNode, string sourcePort, string targetPort)
            {
                source = sourceNode.guid;
                target = targetNode.guid;
                this.sourcePort = sourcePort;
                this.targetPort = targetPort;
            }
        }
        private const string UI_GENERATE_DATA_PATH = "/GameFranework/Editor/Config/{0}.ini";
        private const int width_interval = 300;
        private const int heigth_interval = 300;
        private static int node_index = 0;
        public List<Linker> linkers;
        public Dictionary<string, NodeData> dic;
        public int width;
        public int heigth;

        public UIConfig()
        {
            linkers = new List<Linker>();
            dic = new Dictionary<string, NodeData>();
        }
        public static UIConfig Initialized(string psdFilePath, UIGeneratorWindow window)
        {
            node_index = 0;
            string configPath = string.Format(UI_GENERATE_DATA_PATH, Path.GetFileNameWithoutExtension(psdFilePath));
            string filePath = Application.dataPath + configPath;
            UIConfig config = new UIConfig();
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, CatJson.JsonParser.ToJson(config));
            }

            config = CatJson.JsonParser.ParseJson<UIConfig>(File.ReadAllText(filePath));
            foreach (var item in config.dic.Values)
            {
                ViewNode node = (ViewNode)Activator.CreateInstance(Type.GetType(item.type));
                if (node is ExportNode)
                {
                    window.export = (ExportNode)node;
                }
                node.Initialized(item);
                node.guid = item.guid;
                node.SetPosition(item.rect);
                window.AddViewNode(node);
            }

            if (window.export == null)
            {
                window.export = new ExportNode();
                window.AddViewNode(window.export);
            }

            PsdFile psdFile = new PsdFile(psdFilePath, Encoding.Default);
            Layer current = null;
            psdFile.Layers.Reverse();
            List<Layer> layers = new List<Layer>();
            foreach (var item in psdFile.Layers)
            {
                if (!item.HasLayer() && item.Name != "</Layer group>")
                {
                    if (current != null)
                    {
                        current.Chiled.Add(item);
                    }
                    else
                    {
                        layers.Add(item);
                    }
                    item.Parent = current;
                    current = item;
                    continue;
                }
                if (item.Name == "</Layer group>")
                {
                    if (current == null)
                    {
                        continue;
                    }
                    current = current.Parent;
                    continue;
                }
                if (current == null)
                {
                    layers.Add(item);
                    continue;
                }
                item.Parent = current;
                current.Chiled.Add(item);
            }
            config.width = psdFile.ColumnCount;
            config.heigth = psdFile.RowCount;
            layers.Reverse();
            foreach (var layer in layers)
            {
                InitializeLayerEntity(config, window.export, layer);
            }

            foreach (var linked in config.linkers)
            {
                ViewNode source = window.GetViewNode(linked.source);
                ViewNode target = window.GetViewNode(linked.target);
                if (source == null || target == null)
                {
                    continue;
                }
                Port sourcePort = source.GetPort(linked.sourcePort);
                Port targetPort = target.GetPort(linked.targetPort);
                if (sourcePort == null || targetPort == null)
                {
                    continue;
                }
                Edge edge = new Edge();
                edge.input = targetPort;
                edge.output = sourcePort;
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                window.AddConnect(edge);
            }
            window.export.AutoLayout(0);
            return config;
        }

        private static void InitializeLayerEntity(UIConfig config, ViewNode parent, Layer layer)
        {
            string guid = Utility.GetMd5Hash(layer.Id.ToString());
            ViewNode componentNode = UIGeneratorWindow.window.GetViewNode(guid);
            if (componentNode != null)
            {
                ((SpriteNode)componentNode).SetPsdLayer(layer);
                return;
            }
            EntityNode entity = new EntityNode();
            entity.guid = Utility.GetMd5Hash(layer.Id + "Object");
            entity.Initialized(null);
            entity.Datable.name = layer.Name.Replace(" ", "").Replace("-", "");
            UIGeneratorWindow.window.AddViewNode(entity);
            entity.SetPsdLayer(layer);
            config.linkers.Add(new Linker(entity, parent, "Content", "Childs"));
            if (layer.HasTextureLayer())
            {
                SpriteNode spriteNode = new SpriteNode();
                spriteNode.guid = guid;
                spriteNode.Initialized(null);
                UIGeneratorWindow.window.AddViewNode(spriteNode);
                spriteNode.SetPsdLayer(layer);
                config.linkers.Add(new Linker(spriteNode, entity, "Content", "Components"));
            }
            else if (layer.IsTextLayer)
            {
                LableNode lable = LableNode.Create(guid, layer);
                UIGeneratorWindow.window.AddViewNode(lable);
                config.linkers.Add(new Linker(lable, entity, "Content", "Components"));
            }

            if (layer.Chiled.Count > 0)
            {
                layer.Chiled.Reverse();
                for (var i = 0; i < layer.Chiled.Count; i++)
                {
                    InitializeLayerEntity(config, entity, layer.Chiled[i]);
                }
            }
        }
        public static void Saved(string name, List<ViewNode> nodes, List<Edge> edges)
        {
            UIConfig config = new UIConfig();
            foreach (ViewNode node in nodes)
            {
                NodeData data = node.GetNodeData();
                data.type = node.GetType().FullName;
                data.guid = node.guid;
                data.rect = node.GetPosition();
                config.dic.Add(node.guid, data);
            }
            foreach (Edge edge in edges)
            {
                ViewNode source = (ViewNode)edge.output.node;
                ViewNode target = (ViewNode)edge.input.node;
                Linker linker = new Linker();
                linker.source = source.guid;
                linker.target = target.guid;
                linker.sourcePort = edge.output.portName;
                linker.targetPort = edge.input.portName;
                config.linkers.Add(linker);
            }
            string configPath = string.Format(UI_GENERATE_DATA_PATH, name);
            string filePath = Application.dataPath + configPath;
            File.Delete(filePath);
            File.WriteAllText(filePath, CatJson.JsonParser.ToJson(config));
        }
    }
}
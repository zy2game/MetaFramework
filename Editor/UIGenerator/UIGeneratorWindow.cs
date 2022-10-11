using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Linq;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class UIGeneratorWindow : EditorWindow
    {
        public static StyleColor TiletStyleColor = new StyleColor(new Color(.3f, .3f, .3f, 1f));
        public static StyleColor BackgroundColor = new StyleColor(new Color(.2f, .2f, .2f, 1f));

        private string exportName;
        public ExportNode export;
        private ViewNode seletion;
        private UIGraphEditor graphEditor;
        private RectTransform canvas;
        public static UIGeneratorWindow window;
        public List<ViewNode> Nodes
        {
            get
            {
                return graphEditor.nodes.Cast<ViewNode>().ToList();
            }
        }
        public List<Edge> Edges
        {
            get
            {
                return graphEditor.edges.ToList();
            }
        }
        public void OnEnable()
        {
            window = this;
            if (graphEditor != null)
            {
                return;
            }
            graphEditor = new UIGraphEditor(this);
            this.rootVisualElement.Add(graphEditor);
            GameObject canvasObject = GameObject.Find("UICamera/Canvas");
            if (canvasObject == null)
            {
                canvasObject = (GameObject)GameObject.Instantiate(Resources.Load("Camera/UICamera"));
                canvasObject.SetParent(Utility.EmptyTransform);
                Canvas canvasTemplate = (Canvas)GameObject.Instantiate(Resources.Load<Canvas>("Camera/Canvas"));
                canvas = canvasTemplate.GetComponent<RectTransform>();
                canvasTemplate.gameObject.SetParent(canvasObject);
                canvasTemplate.worldCamera = canvasObject.GetComponent<Camera>();
            }
            this.rootVisualElement.Add(new IMGUIContainer(OnGenerateGUI));
        }
        public List<T> GetNodes<T>() where T : ViewNode
        {
            List<T> result = new List<T>();
            foreach (var item in graphEditor.nodes)
            {
                if (item.GetType() == typeof(T))
                {
                    result.Add((T)item);
                }
            }
            return result;
        }
        public void OnGenerateGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("+", EditorStyles.toolbarDropDown))
            {
                if (graphEditor.nodes.Count() > 0)
                {
                    bool state = EditorUtility.DisplayDialog("Tips", "Is saved current edit?", "Yes", "No");
                    if (state)
                    {
                        Saved();
                    }
                }
                ClearNode();
                LoadPsdFile();
            }
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Root:", GUILayout.Width(40));
                canvas = EditorGUILayout.ObjectField("", canvas, typeof(RectTransform), true) as RectTransform;
                GUILayout.EndHorizontal();
            }
            GUI.enabled = graphEditor.nodes.Count() > 0;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name:", GUILayout.Width(40));
                exportName = GUILayout.TextField(exportName, GUILayout.Width(240));
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Priview", EditorStyles.toolbarButton))
            {
                if (EnsureExportNodeState())
                {
                    GenerateUIPrefab("", false);
                }
            }
            if (GUILayout.Button("Export Prefab", EditorStyles.toolbarButton))
            {
                string exportPrefabPath = EditorUtility.SaveFilePanel("Choose Save Prefab Path", EditorPrefs.GetString("ui_prefab_path", Application.dataPath), exportName, "prefab");
                EditorPrefs.SetString("ui_prefab_path", Path.GetDirectoryName(exportPrefabPath));
                if (EnsureExportNodeState())
                {
                    GenerateUIPrefab(exportPrefabPath, true);
                }
            }

            if (GUILayout.Button("Export Script", EditorStyles.toolbarButton))
            {
                string exportScriptPath = EditorUtility.SaveFilePanel("Choose Save LuaScript Path", EditorPrefs.GetString("ui_script_path", Application.dataPath), exportName, "lua");
                EditorPrefs.SetString("ui_script_path", Path.GetDirectoryName(exportScriptPath));
                if (EnsureExportNodeState())
                {
                    GenerateScriptCode(exportScriptPath);
                }
            }

            if (GUILayout.Button("Export Sprite", EditorStyles.toolbarButton))
            {
                string exportSpritePath = EditorUtility.OpenFolderPanel("Choose Save LuaScript Path", EditorPrefs.GetString("ui_sprite_path", Application.dataPath), "");
                EditorPrefs.SetString("ui_sprite_path", Path.GetDirectoryName(exportSpritePath));
                if (EnsureExportNodeState())
                {
                    ExportSpriteAsset(exportSpritePath);
                }
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                Saved();
            }
            if (GUILayout.Button("Layout", EditorStyles.toolbarButton))
            {
                export.AutoLayout(0);
            }
            GUILayout.EndHorizontal();
        }
        public void ClearNode()
        {
            exportName = string.Empty;
            seletion = null;

            foreach (var item in graphEditor.nodes)
            {
                graphEditor.RemoveElement(item);
            }
            foreach (var item in graphEditor.edges)
            {
                graphEditor.RemoveElement(item);
            }
        }
        public void LoadPsdFile()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Seletion a PSD file", EditorPrefs.GetString("psdDir", Application.dataPath), new string[] { "PSD", "psd" });
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            Dictionary<string, ViewNode> nodes = new Dictionary<string, ViewNode>();
            EditorPrefs.SetString("psdDir", Path.GetDirectoryName(path));
            exportName = Path.GetFileNameWithoutExtension(path);
            UIConfig.Initialized(path, this);
        }
        public void Saved()
        {
            UIConfig.Saved(exportName, Nodes, Edges);
            AssetDatabase.Refresh();
        }
        public void GenerateUIPrefab(string exportPrefabPath, bool createAsset)
        {
            GameObject obj = export.GenerateGameObject(canvas, createAsset ? exportPrefabPath : string.Empty);
            obj.name = exportName;
            if (!createAsset)
            {
                return;
            }
            PrefabUtility.SaveAsPrefabAsset(obj, exportPrefabPath);
        }
        public void GenerateScriptCode(string exportScriptPath)
        {
            if (export == null)
            {
                return;
            }
            string code = export.GenerateScriptCode(exportName);

            if (File.Exists(exportScriptPath))
            {
                File.Delete(exportScriptPath);
            }
            File.WriteAllText(exportScriptPath, code);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(exportScriptPath);
        }

        public void ExportSpriteAsset(string path)
        {
            List<SpriteNode> sprites = GetNodes<SpriteNode>();
            foreach (var sprite in sprites)
            {
                sprite.ExportSpriteAssetFile(path);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool EnsureExportNodeState()
        {
            foreach (var item in graphEditor.nodes)
            {
                if (item is ExportNode)
                {
                    export = (ExportNode)item;
                }
            }
            if (export == null)
            {
                EditorUtility.DisplayDialog("Tips", "current edit not find export node", "yes");
                return false;
            }
            if (string.IsNullOrEmpty(exportName))
            {
                EditorUtility.DisplayDialog("tips", "please write export name", "yes");
                return false;
            }
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("tips", "not find canvas transform", "yes");
                return false;
            }
            return true;
        }
        private void OnDestroy()
        {
            ClearNode();
            graphEditor = null;
        }
        public void OnSelectionNode(ViewNode node, bool selet)
        {
            if (selet)
            {
                seletion = node;
                if (node is SpriteNode)
                {

                }
            }
            else
            {
                if (seletion.guid == node.guid)
                {
                    seletion = null;
                }
            }
        }
        public void AddViewNode(ViewNode node)
        {
            graphEditor.AddElement(node);
        }
        public ViewNode GetViewNode(string guid)
        {
            foreach (var item in graphEditor.nodes)
            {
                ViewNode node = (ViewNode)item;
                if (node.guid == guid)
                {
                    return node;
                }
            }
            return default;
        }
        public void AddConnect(Edge edge)
        {
            graphEditor.AddElement(edge);
        }
    }
    public sealed class UIGraphEditor : GraphView
    {
        private static UIGraphEditor instance;
        private static UIGeneratorWindow window;
        public UIGraphEditor(UIGeneratorWindow wnd)
        {
            window = wnd;
            instance = this;
            this.StretchToParentSize();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            GridBackground background = new GridBackground();
            background.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(@"Assets/GameFranework/Editor/Style/EditorStyle.uss"));
            Insert(0, background);
            var menuWindowProvider = ScriptableObject.CreateInstance<SearchWindowContent>();
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> temp = new List<Port>();
            ports.ForEach(port =>
            {
                ViewNode node = port.node as ViewNode;
                if (port.node != startPort.node && (port.portType == startPort.portType || port.portType.IsAssignableFrom(startPort.portType)) && port.direction != Direction.Output)
                {
                    temp.Add(port);
                }
            });
            return temp;
        }
        class SearchWindowContent : ScriptableObject, ISearchWindowProvider
        {
            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("Nodes")));
                Type[] types = typeof(ViewNode).Assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }
                    if (type.IsSubclassOf(typeof(ViewNode)))
                    {
                        entries.Add(new SearchTreeEntry(new GUIContent(type.Name.Replace("Node", "")))
                        {
                            level = 1,
                            userData = type,
                        });
                    }
                }

                return entries;
            }

            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                ViewNode node = (ViewNode)Activator.CreateInstance(searchTreeEntry.userData as Type);
                VisualElement rootVisualElement = window.rootVisualElement;
                var windowMousePosition = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent, context.screenMousePosition - window.position.position);
                var graphMousePosition = instance.contentViewContainer.WorldToLocal(windowMousePosition);
                node.Initialized(null);
                node.SetPosition(new Rect(new Vector3(graphMousePosition.x, graphMousePosition.y, 0), Vector2.one));
                node.Refresh();
                window.AddViewNode(node);

                return true;
            }
        }
    }
}
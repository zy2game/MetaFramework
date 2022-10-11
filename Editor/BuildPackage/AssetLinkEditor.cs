using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Unity.VisualScripting.YamlDotNet.RepresentationModel;
using System.Linq;

namespace GameEditor.BuildAsset
{
    public class AssetLinkEditor : BaseEditorWindow
    {
        private class Item
        {
            public int index;
            public bool isSelect;
            public string name;
            public List<string> dependList;
        }

        public static void Open()
        {
            Open<AssetLinkEditor>(500, 600, "资源包引用");
        }

        protected SelfGUIStyle selfGUIStyle;
        private List<Item> items;
        private Vector2 scrollPos;
        private bool isLinkDepondAsset = true;

        //设置资源配置关联
        private bool isSetConfigLink = false;

        private string assetProjectPath;

        protected override void Init()
        {
            selfGUIStyle = new SelfGUIStyle();
            assetProjectPath = ReadAssetProjectPath();
            Start();
        }

        private string ReadAssetProjectPath()
        {
            string path = "./Assets/ArtistRes/assetProjectConfig.la";
            if (!File.Exists(path)) return string.Empty;
            return File.ReadAllText(path);
        }

        private void WriteAssetProjectPath(string assetProjectPath)
        {
            string path = "./Assets/ArtistRes/assetProjectConfig.la";
            File.WriteAllText(path, assetProjectPath);
            AssetDatabase.Refresh();
        }

        private void Start()
        {
            if (!ReadConfig())
            {
                //读取
                isSetConfigLink = true;
                return;
            }
            isSetConfigLink = false;

            string[] dirs = Directory.GetDirectories("./Assets/ArtistRes");
            foreach (var dir in dirs)
            {
                foreach (Item item in items)
                {
                    if (item.name.Equals(Path.GetFileNameWithoutExtension(dir)))
                    {
                        item.isSelect = true;
                        break;
                    }
                }
            }

            
        }

        private void LinkAssetConfig()
        {
            GUILayout.Space(5);
            GUILayout.Label("添加资源配置引用:");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源工程路径:", GUILayout.Width(80));
            assetProjectPath = GUILayout.TextField(assetProjectPath);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("设置", GUILayout.Width(75)))
            {
                SetLuaToAssetProject();
                LinkLocalFrameworkToAssetProject();
                Start();
            }
        }

        private void Execute(string command)
        {
            Debug.Log(command);

            command = command.Trim().TrimEnd('&') + "&exit";

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c" + command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            process.WaitForExit();
            process.Close();
            AssetDatabase.Refresh();


        }

        private void OnGUI()
        {
            if (isSetConfigLink)
            {
                LinkAssetConfig();
                return;
            }

            Title();
            DrawItems();
        }

        private void Title()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("关联依赖资源包:", GUILayout.Width(90));
            isLinkDepondAsset = EditorGUILayout.Toggle(isLinkDepondAsset, GUILayout.Width(15));
            if (GUILayout.Button("引用", GUILayout.Width(75))) LinkSelect();
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(position.width), GUILayout.Height(5));
        }

        private void LinkSelect()
        {
            string[] dirs = Directory.GetDirectories("./Assets/ArtistRes");
            List<string> delDirList = new List<string>();
            foreach (var dir in dirs)
            {
                foreach (Item item in items)
                {
                    if (item.name.Equals(Path.GetFileNameWithoutExtension(dir)))
                    {
                        delDirList.Add(item.name);
                        break;
                    }
                }
            }

            foreach (var item in items)
            {
                if (!item.isSelect) continue;

                string scrPath = assetProjectPath + "ArtistRes/" + item.name;
                if (!Directory.Exists(scrPath))
                {
                    Debug.LogError("路径不存在:" + scrPath);
                    continue;
                }

                delDirList.Remove(item.name);
                string destPath = new DirectoryInfo("./Assets/" + "ArtistRes/" + item.name).FullName;
                if (Directory.Exists(destPath)) continue;
                Execute(string.Format("mklink /j {0} {1}", destPath, scrPath.Replace("/", @"\")));
            }

            foreach (var v in delDirList)
            {
                string path = "./Assets/ArtistRes/" + v;
                Execute("rmdir " + new DirectoryInfo(path).FullName);
                string metaPath = path + ".meta";
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }

            AssetDatabase.Refresh();
            Close();


        }

        private void DrawItems()
        {
            GUILayout.Space(5);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var item in items)
            {
                DrawItem(item);
            }
            GUILayout.EndScrollView();
        }

        private void DrawItem(Item item)
        {
            GUIStyle style = selfGUIStyle.item;
            if (item.isSelect) style = selfGUIStyle.newItem;
            GUILayout.Space(2);
            GUILayout.BeginHorizontal(style);
            bool isSelect = item.isSelect;
            item.isSelect = EditorGUILayout.Toggle(item.isSelect, GUILayout.Width(15));
            if (item.isSelect != isSelect)
            {
                if (item.isSelect && isLinkDepondAsset)
                {
                    SelectDepend(item);
                }
            }
            GUILayout.Label((item.index + 1) + ". " + item.name, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }

        private void SelectDepend(Item item)
        {
            foreach (var v in item.dependList)
            {
                foreach (var i in items)
                {
                    if (i.name.Equals(v))
                    {
                        i.isSelect = true;
                        break;
                    }
                }
            }
        }

        private bool ReadConfig()
        {
            if (string.IsNullOrEmpty(assetProjectPath)) return false;
            string path = assetProjectPath;
            if (!path.EndsWith("/") && !path.EndsWith(@"\"))
                path += "/";
            if (!Directory.Exists(path))
            {
                Debug.LogError("路径不存在:" + assetProjectPath);
                return false;
            }

            path = path + "ArtistRes/assetBuildConfig";
            DirectoryInfo di = new DirectoryInfo(path);
            path = di.FullName;

            if (!Directory.Exists(path))
            {
                Debug.LogError("文件夹不存在:" + path);
                return false;
            }

            List<string> allConfigGuid = new List<string>();
            Dictionary<string, string> guidToNameMap = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                if (!file.EndsWith(".meta"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string matePath = file + ".meta";
                    string guid = GetMetaGuid(matePath);
                    if (string.IsNullOrEmpty(guid)) continue;
                    guidToNameMap.Add(guid, name);
                }
            }

            string buildAssetConfigPath = path + "/" + AssetManager.configName;
            if (!File.Exists(buildAssetConfigPath))
            {
                Debug.LogError("配置文件不存在:" + buildAssetConfigPath);
                return false;
            }
            var node = FindNode(buildAssetConfigPath, "MonoBehaviour");
            if (node != null)
            {
                node = FindNode(node, "buidlAssetDataList");
                if (node != null)
                {
                    bool readGuid = false;
                    foreach (var cnd in node.AllNodes)
                    {
                        if (cnd.ToString().Equals("guid"))
                        {
                            readGuid = true;
                            continue;
                        }
                        else if (readGuid)
                        {
                            string cfgGuid = cnd.ToString();
                            allConfigGuid.Add(cfgGuid);
                            readGuid = false;
                        }
                    }
                }
            }

            if (allConfigGuid.Count == 0)
            {
                Debug.LogError("没有找到配置文件");
                return false;
            }

            int index = 0;
            items = new List<Item>();
            foreach (var guid in allConfigGuid)
            {
                string name = string.Empty;
                if (!guidToNameMap.TryGetValue(guid, out name))
                    continue;
                string file = path + "/" + name + ".asset";
                string[] denpendsGuid = GetDepondGUID(file);
                List<string> dependName = new List<string>();
                foreach (var dpGuid in denpendsGuid)
                {
                    string cfgName = string.Empty;
                    if (!guidToNameMap.TryGetValue(dpGuid, out cfgName))
                    {
                        Debug.LogError("找不到guid对应的名字:" + guid);
                        continue;
                    }
                    dependName.Add(cfgName);
                }

                Item item = new Item();
                item.index = index++;
                item.isSelect = false;
                item.name = name;
                item.dependList = dependName;
                items.Add(item);
            }

            if (!assetProjectPath.EndsWith("/") && !assetProjectPath.EndsWith(@"\"))
                assetProjectPath += "/";

            WriteAssetProjectPath(assetProjectPath);

            return true;
        }

        private string GetMetaGuid(string metaFile)
        {
            if (!File.Exists(metaFile))
            {
                Debug.LogError("找不到对应的文件:" + metaFile);
                return string.Empty;
            }

            StreamReader stream = new StreamReader(metaFile, System.Text.Encoding.UTF8);
            YamlStream yamls = new YamlStream();
            yamls.Load(stream);
            foreach (var yd in yamls)
            {
                var map = yd.RootNode as YamlMappingNode;
                foreach (var n in map)
                {
                    if (n.Key.ToString().Equals("guid"))
                    {
                        return n.Value.ToString();
                    }
                }
            }

            return string.Empty;
        }

        private string[] GetDepondGUID(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("找不到对应的文件:" + path);
                return new string[0];
            }

            StreamReader stream = new StreamReader(path, System.Text.Encoding.UTF8);
            YamlStream yamls = new YamlStream();
            yamls.Load(stream);

            List<string> dependGuidList = new List<string>();

            foreach (var v in yamls)
            {
                var node = FindNode(v.RootNode, "MonoBehaviour");
                if (node != null)
                {
                    node = FindNode(node, "dependPackage");
                    if (node != null)
                    {
                        bool readGuid = false;
                        foreach (var n in node.AllNodes)
                        {
                            if (n.ToString().Equals("guid"))
                            {
                                readGuid = true;
                            }
                            else if (readGuid)
                            {
                                readGuid = false;
                                dependGuidList.Add(n.ToString());
                            }
                        }
                    }
                }

            }

            return dependGuidList.ToArray();
        }

        private YamlNode FindNode(string path, string nodeName)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("找不到对应的文件:" + path);
                return new string[0];
            }

            StreamReader stream = new StreamReader(path, System.Text.Encoding.UTF8);
            YamlStream yamls = new YamlStream();
            yamls.Load(stream);

            foreach (var v in yamls)
            {
                var node = FindNode(v.RootNode, nodeName);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private YamlNode FindNode(YamlNode rootNode, string nodeName)
        {
            var map = rootNode as YamlMappingNode;
            foreach (var m in map)
            {
                if (m.Key.ToString().Equals(nodeName))
                    return m.Value;
            }
            return null;

        }

        private void SetLuaToAssetProject()
        {
            if (!assetProjectPath.EndsWith("/") && !assetProjectPath.EndsWith(@"\"))
                assetProjectPath += "/";
            string scrPath = new DirectoryInfo("./Assets/GameScript").FullName;
            string destPath = new DirectoryInfo(assetProjectPath + "GameScript").FullName;
            if (!Directory.Exists(scrPath))
            {
                Debug.LogError("找不到lua目录");
                return;
            }
            if (Directory.Exists(destPath)) return;
            Execute(string.Format("mklink /j {0} {1}", destPath, scrPath));

        }

        private void LinkLocalFrameworkToAssetProject()
        {
            string scrPath = new DirectoryInfo("./Assets/GameFranework").FullName;
            string destPath = new DirectoryInfo(assetProjectPath + "GameFranework").FullName;
            if (Directory.Exists(destPath))
                Directory.Delete(destPath,true);
            Execute(string.Format("mklink /j {0} {1}", destPath, scrPath));

            scrPath = new DirectoryInfo("./Assets/Plugins").FullName;
            destPath = new DirectoryInfo(assetProjectPath + "Plugins").FullName;

            if (Directory.Exists(destPath))
                Directory.Delete(destPath, true); 
            Execute(string.Format("mklink /j {0} {1}", destPath, scrPath));
        }
    }
}

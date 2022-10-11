using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using GameFramework.Runtime.Assets;

namespace GameEditor.BuildAsset
{
    public class PageLocalAssetsManager : BuildAssetPage
    {
        private class Item
        {
            public int index;
            public bool isSelect;
            public int version;
            public string moduleName;
            public string alias;
            public string assetPath;
        }

        private int curSelectCount;
        private bool isSelectAll;
        private Vector2 scrollPos;
        private List<Item> items;
        private string cacheRootPath; //资源缓存根路径
        private AssetVersion assetVersion;//资源版本配置
        private AssetVersion localVersion;//本地版本配置

        public PageLocalAssetsManager(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            items = new List<Item>();
        }

        public override void Enter(object param)
        {
            curSelectCount = 0;
            items.Clear();
            cacheRootPath = buildSetting.assetConfig.buildRootPath + BuildAssetConfig.buildCachePath + buildSetting.PlatformName + "/";
            string versionPath = cacheRootPath + "version.txt";
            if (!File.Exists(versionPath))
            {
                return;
            }
            assetVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(versionPath));

            Item commonItem = null;
            Item mainItem = null;
            foreach (var v in assetVersion.versionMap)
            {
                string moduleName = v.Key;
                int version = v.Value;
                var assetData = buildSetting.assetConfig.FindAssetData(moduleName);
                string alias = "未定义";
                if (assetData != null)
                    alias = assetData.alias;
                Item item = new Item
                {
                    version = version,
                    alias = alias,
                    moduleName = moduleName
                };

                item.assetPath = cacheRootPath + moduleName + "_cache/" + version + "/";

                if (assetData != null)
                {
                    if (assetData.assetType == AssetType.Common)
                    {
                        commonItem = item;
                        continue;
                    }

                    if (assetData.assetType == AssetType.Main)
                    {
                        mainItem = item;
                        continue;
                    }
                }
                items.Add(item);
            }

            if (mainItem != null)
                items.Insert(0, mainItem);
            if (commonItem != null)
                items.Insert(0, commonItem);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].index = i;
            }

            SetSelected();
        }

        private void SetSelected()
        {
            string localVersionPath = AppConst.AppAssetPath + "version.txt";
            if (!File.Exists(localVersionPath)) return;
            localVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(localVersionPath));
            foreach (var v in localVersion.versionMap)
            {
                string moduleName = v.Key;
                foreach (var item in items)
                {
                    if (moduleName.Equals(item.moduleName))
                        item.isSelect = true;
                }
            }
        }

        public override void Exit()
        {
        }

        public override void Run()
        {
            Title();
            DrawItems();
        }

        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(100))) Return();
            if (GUILayout.Button("更新本地资源", GUILayout.Height(25), GUILayout.Width(150))) SetLoaclAsset();

            GUILayout.Label(string.Format("已选择{0}个", curSelectCount), GUILayout.Width(65), GUILayout.Height(25));
            bool temp = isSelectAll;
            isSelectAll = EditorGUILayout.Toggle(isSelectAll, GUILayout.Width(25), GUILayout.Height(25));
            if (isSelectAll != temp) ChangeSelectAll(isSelectAll);
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));
        }

        private void DrawItems()
        {
            curSelectCount = 0;
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
            item.isSelect = EditorGUILayout.Toggle(item.isSelect, GUILayout.Width(15));
            if (item.isSelect) curSelectCount++;
            GUILayout.Label((item.index + 1) + ". " + item.alias, GUILayout.Width(120));
            GUILayout.Label("模块名: " + item.moduleName, GUILayout.Width(180));
            GUILayout.Label("版本: " + item.version);
            GUILayout.EndHorizontal();
        }

        private void ChangeSelectAll(bool select)
        {
            foreach (var v in items)
            {
                v.isSelect = select;
            }
        }

        private void Return()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
        }

        private void SetLoaclAsset()
        {
            List<Item> itemList = new List<Item>();
            foreach (var v in items)
            {
                if (v.isSelect)
                    itemList.Add(v);
            }

            if (items.Count == 0) return;


            if (localVersion == null)
            {
                localVersion = new AssetVersion();
                localVersion.versionMap = new Dictionary<string, int>();
                localVersion.packageVersion = assetVersion.packageVersion;
            }

            List<string> delModule = new List<string>();

            string outPath = AppConst.AppAssetPath;
            foreach (var v in itemList)
            {
                string outDir = outPath + v.moduleName + "/";
                CopyAssetsToLocal(v.assetPath, outDir);
                if (localVersion.versionMap.ContainsKey(v.moduleName))
                    localVersion.versionMap[v.moduleName] = v.version;
                else
                    localVersion.versionMap.Add(v.moduleName, v.version);
            }

            //检测需要删除的本地资源
            foreach (var v in localVersion.versionMap)
            {
                bool del = true;
                foreach (var item in itemList)
                {
                    if (item.moduleName.Equals(v.Key))
                    {
                        del = false;
                        break;
                    }
                }

                if (del)
                    delModule.Add(v.Key);
            }

            //删除没有选择的本地资源
            foreach (var v in delModule)
            {
                localVersion.versionMap.Remove(v);
                string outDir = outPath + v;
                Directory.Delete(outDir, true);
                string metaFile = outDir + ".meta";
                if (File.Exists(metaFile))
                    File.Delete(metaFile);
            }


            File.WriteAllText(outPath + "version.txt", JsonObject.Serialize(localVersion));
            AssetDatabase.Refresh();

        }

        //拷贝资源到本地
        private void CopyAssetsToLocal(string scrDir, string outDir)
        {
            if (!Directory.Exists(scrDir))
            {
                Debug.LogError("资源不存在:" + scrDir);
                return;
            }
            string fileListPath = scrDir + "files.txt";
            if (!File.Exists(fileListPath))
            {
                Debug.LogError("资源列表文件不存在:" + fileListPath);
                return;
            }

            string fileListContent = File.ReadAllText(fileListPath);
            var fileListEntity = JsonObject.Deserialize<AssetFileEntity>(fileListContent);

            if (Directory.Exists(outDir))
                Directory.Delete(outDir, true);
            Directory.CreateDirectory(outDir);
            File.WriteAllText(outDir + "files.txt", fileListContent);

            foreach (var v in fileListEntity.files)
            {
                string fileName = v.name;
                string scrFilePath = scrDir + fileName;
                string outFilePath = outDir + fileName;
                File.Copy(scrFilePath, outFilePath);
            }
        }
    }

}
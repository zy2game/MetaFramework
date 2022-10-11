using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildAsset
{
    public class PageMain : BuildAssetPage
    {
        private class Item
        {
            public int index;
            public bool isSelect;
            public BuildAssetData assetData;
        }

        private bool isSelectAll = false;
        private int curSelectCount;
        private Vector2 scrollPos;
        private List<Item> items;
        private Item delItem;
        private bool isBuild = false;

        public PageMain(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            items = new List<Item>();
        }

        public override void Enter(object param)
        {
            curSelectCount = 0;
            items.Clear();
            for (int i = 0; i < buildSetting.assetConfig.buidlAssetDataList.Count; i++)
            {
                var data = buildSetting.assetConfig.buidlAssetDataList[i];
                Item item = new Item();
                item.index = i;
                item.assetData = data;
                items.Add(item);
            }
        }

        public override void Exit()
        {
            delItem = null;
        }

        public override void Run()
        {
            Title();
            DrawItems();
            if (delItem != null)
                items.Remove(delItem);
        }

        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("打包路径", GUILayout.Height(25)))
            {
                OpenDirectory(buildSetting.assetConfig.buildRootPath + BuildAssetConfig.buildTempPath);
            }
            if (GUILayout.Button("缓存路径", GUILayout.Height(25)))
            {
                OpenDirectory(buildSetting.assetConfig.buildRootPath + BuildAssetConfig.buildCachePath);
            }
            if (GUILayout.Button("新建模块", GUILayout.Height(25))) CreatorModule();
            if (GUILayout.Button("打包所选项", GUILayout.Height(25))) isBuild = true;
            if (GUILayout.Button("本地资源", GUILayout.Height(25))) LocalAsset();

            GUILayout.Label(string.Format("已选择{0}个", curSelectCount), GUILayout.Width(65), GUILayout.Height(25));
            bool temp = isSelectAll;
            isSelectAll = EditorGUILayout.Toggle(isSelectAll, GUILayout.Width(25), GUILayout.Height(25));
            if (isSelectAll != temp) ChangeSelectAll(isSelectAll);
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));

            if (isBuild)
            {
                isBuild = false;
                BuildAsset();
            }
        }

        //编辑公共资源
        private void OpenDirectory(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path.Replace("/", @"\"));
        }

        //新建模块
        private void CreatorModule()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.EditorModule);
        }

        //资源打包
        private void BuildAsset()
        {
            List<BuildAssetData> assetsData = new List<BuildAssetData>();
            foreach (var v in items)
            {
                if (v.isSelect)
                    assetsData.Add(v.assetData);
            }

            foreach (var v in assetsData)
            {
                new BuildAssetBundle(buildSetting.assetConfig, v,buildSetting.PlatformName);
            }            
        }

        //本地资源管理
        private void LocalAsset()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.LocalAsset);
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
            GUILayout.Label((item.index + 1) + ". " + item.assetData.alias, GUILayout.Width(120));
            GUILayout.Label("模块名: " + item.assetData.moduleName);
            if (GUILayout.Button("Editor", GUILayout.Width(75))) EditorAssetData(item);
            if (item.assetData.assetType == AssetType.Module)
            {
                if (GUILayout.Button("Del", GUILayout.Width(45))) DeleteAssetData(item);
            }
            GUILayout.EndHorizontal();
        }

        private void ChangeSelectAll(bool select)
        {
            foreach (var v in items)
            {
                v.isSelect = select;
            }
        }

        private void EditorAssetData(Item item)
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.EditorAsset, item.assetData);
        }

        private void DeleteAssetData(Item item)
        {
            if (EditorUtility.DisplayDialog("提示", string.Format("确定删除【{0}】的资源配置?", item.assetData.alias), "确定", "取消"))
            {
                buildSetting.assetConfig.buidlAssetDataList.Remove(item.assetData);
                delItem = item;
                buildSetting.Save();
                AssetDatabase.Refresh();
            }
        }

    }
}

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace GameEditor.BuildAsset
{
    public class PageEditorModule : BuildAssetPage
    {
        private class Item
        {
            public int index;
            public bool isSelect;
            public BuildAssetData assetData;
        }

        private BuildAssetData assetData;
        private bool isNew;
        private DefaultAsset assetRoot;
        private string alias;

        private int curSelectCount;
        private Vector2 scrollPos;
        private List<Item> items;

        public PageEditorModule(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            items = new List<Item>();
        }

        public override void Enter(object param)
        {
            assetData = param as BuildAssetData;
            if (assetData == null)
            {
                isNew = true;
                assetData = new BuildAssetData();
                assetData.assetType = AssetType.Module;
                assetData.dependPackage = new List<BuildAssetData>();
            }

            alias = assetData.alias;
            assetRoot = assetData.assetRoot;

            curSelectCount = 0;
            items.Clear();
            for (int i = 0; i < buildSetting.assetConfig.buidlAssetDataList.Count; i++)
            {
                var data = buildSetting.assetConfig.buidlAssetDataList[i];
                if (assetData == data) continue;//排除自己
                Item item = new Item();
                item.index = i;
                item.assetData = data;

                foreach (var v in assetData.dependPackage)
                {
                    if (v == data)
                    {
                        item.isSelect = true;
                        break;
                    }
                }

                items.Add(item);
            }
        }

        public override void Exit()
        {
            assetData = null;
            isNew = false;
            alias = string.Empty;
            assetRoot = null;
        }

        public override void Run()
        {
            GUILayout.Space(5);
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(80))) Return();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));
            GUILayout.BeginVertical(selfGUIStyle.item);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("根目录:", GUILayout.Width(60), GUILayout.Height(20));
            assetRoot = (DefaultAsset)EditorGUILayout.ObjectField(assetRoot, typeof(DefaultAsset), true, GUILayout.Height(20), GUILayout.Width(300));

            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Label("模块别名:", GUILayout.Width(60), GUILayout.Height(20));
            alias = GUILayout.TextField(alias, GUILayout.Width(300), GUILayout.Height(20));
            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            GUILayout.Space(5);
            if (GUILayout.Button("保存并返回", GUILayout.Width(150), GUILayout.Height(30))) Save();
            GUILayout.Space(5);
            GUILayout.EndVertical();


            DrawItems();
        }

        private void DrawItems()
        {
            curSelectCount = 0;
            GUILayout.Space(5);
            GUILayout.Label("选择依赖包体:");
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(2));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var item in items)
            {
                DrawItem(item);
            }
            GUILayout.EndScrollView();


            GUILayout.Label(string.Format("已选择{0}个", curSelectCount), GUILayout.Width(65), GUILayout.Height(25));
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
            GUILayout.EndHorizontal();
        }


        //返回到主界面
        private void Return()
        {
            if (isNew)
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
            else
                buildSetting.ChangePage(AssetBundleBuildSetting.Page.EditorAsset, assetData);
        }

        //保存
        private void Save()
        {
            assetData.alias = alias;
            assetData.assetRoot = assetRoot;
            if (isNew)
            {
                assetData.buildAssets = new List<BuildAssetItem>();
                string path = AssetDatabase.GetAssetPath(buildSetting.assetConfig);
                int index = path.LastIndexOf("/");
                path = path.Substring(0, index);
                AssetDatabase.CreateAsset(assetData, path + "/" + assetData.moduleName + ".asset");
                buildSetting.assetConfig.buidlAssetDataList.Add(assetData);
            }


            List<BuildAssetData> selectDatas = new List<BuildAssetData>();
            foreach (var v in items)
            {
                if (v.isSelect)
                    selectDatas.Add(v.assetData);
            }

            assetData.dependPackage = selectDatas;

            AssetDatabase.SaveAssetIfDirty(assetData);
            EditorUtility.SetDirty(assetData);
            buildSetting.Save();
            AssetDatabase.Refresh();
            Return();

        }
    }
}

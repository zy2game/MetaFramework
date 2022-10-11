using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEditor.BuildAsset
{
    public class PageEditorAsset : BuildAssetPage
    {
        private class Item
        {
            public int index;
            public bool isSelect;//是否选择了
            public bool isNew;//是否是新加的资源
            public bool isRepeat;//是否是重复的资源
            public BuildAssetItem assetItem;//资源基础配置
            public Object obj;
        }

        private BuildAssetData assetData;//当前模块资源配置
        private Vector2 scrollPos;
        private List<Item> items;//当前设置上的所有资源Item
        private List<string> itemsPath;//所有资源路径
        private List<Item> newItems;//新选择还未加上的资源
        private int curSelectCount;//当前已勾选多少个资源
        private bool isSelectAll;//是否全选

        public PageEditorAsset(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            items = new List<Item>();
            itemsPath = new List<string>();
            newItems = new List<Item>();
        }

        public override void Enter(object param)
        {
            assetData = param as BuildAssetData;

            for (int i = 0; i < assetData.buildAssets.Count; i++)
            {
                var assetItem = assetData.buildAssets[i];
                Item item = new Item();
                item.assetItem = assetItem;
                item.index = i;
                item.obj = assetItem.asset;
                items.Add(item);
                itemsPath.Add(assetItem.path);
            }
        }

        public override void Exit()
        {
            items.Clear();
            itemsPath.Clear();
            newItems.Clear();
        }

        public override void Run()
        {
            Title();
            SelectAsset();
            DrawAssetItems();
            Bottom();
        }

        //当前页面title
        private void Title()
        {
            GUILayout.Space(5);
            GUILayout.Label(assetData.alias+ "      模块名: " + assetData.moduleName);
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(5));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(80))) Return();
            if (GUILayout.Button("添加所选项", GUILayout.Height(25))) AddSelect();
            if (GUILayout.Button("异常检测", GUILayout.Height(25))) CheckAsset();
            if (GUILayout.Button("编辑依赖包体", GUILayout.Height(25))) AddDependPackage();
            if (GUILayout.Button("保存", GUILayout.Height(25))) Save();
            GUILayout.EndHorizontal();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(3));
        }
       
        //新选择的资源
        private void SelectAsset()
        {
            var objs = Selection.objects;
            newItems.Clear();

            foreach (var v in items)
            {
                v.isRepeat = false;
            }

            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                string path = AssetDatabase.GetAssetPath(obj);
                int itemIndex = itemsPath.IndexOf(path);
                if (itemIndex != -1)
                {
                    items[itemIndex].isRepeat = true;
                    continue;
                }
                Item item = new Item();
                item.index = i;
                item.isNew = true;
                BuildAssetItem assetItem = new BuildAssetItem();
                assetItem.asset = obj;
                item.assetItem = assetItem;
                item.obj = obj;
                newItems.Add(item);
            }
        }

        //绘制所资源items
        private void DrawAssetItems()
        {
            GUILayout.Space(5);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(buildSetting.position.height - 100));
            DrawCodePackage();
            if (GUILayout.Button("添加代码包", GUILayout.Width(150), GUILayout.Height(25))) AddCodePackage();
            GUILayout.Space(5);
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(3));
            GUILayout.Space(5);
            foreach (var item in newItems)
            {
                DrawItem(item);
            }
            curSelectCount = 0;
            foreach (var item in items)
            {
                DrawItem(item);
            }
            GUILayout.EndScrollView();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(3));
        
        }

        private void AddCodePackage()
        {
            if (assetData.codesPath == null)
                assetData.codesPath = new List<DefaultAsset>();
            assetData.codesPath.Add(null);
        }

        private void DrawCodePackage()
        {
            if (assetData.codesPath == null) return;
            for (int i = 0; i < assetData.codesPath.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i+".",GUILayout.Width(30));
                assetData.codesPath[i] = (DefaultAsset)EditorGUILayout.ObjectField(assetData.codesPath[i], typeof(DefaultAsset), true, GUILayout.Width(300));
                if (GUILayout.Button("del",GUILayout.Width(35)))
                {
                    assetData.codesPath.RemoveAt(i);
                    GUILayout.EndHorizontal();
                    break;
                }
                GUILayout.EndHorizontal();

            }
        }
        
        //绘制资源item
        private void DrawItem(Item item)
        {
            GUIStyle style = selfGUIStyle.item;
            if (item.isNew) style = selfGUIStyle.newItem;
            else if (item.isSelect) style = selfGUIStyle.delItem;
            else if (item.isRepeat) style = selfGUIStyle.line;

            GUILayout.Space(2);
            GUILayout.BeginHorizontal(style, GUILayout.Height(20));
            if (!item.isNew)
            {
                item.isSelect = EditorGUILayout.Toggle(item.isSelect, GUILayout.Width(15));
                if (item.isSelect) curSelectCount++;
            }
            else
                GUILayout.Space(15);
            GUILayout.Label((item.index + 1) + ". " + item.assetItem.name,GUILayout.Width(170));
            EditorGUILayout.ObjectField(item.obj, typeof(Object), true, GUILayout.Width(300));
            GUILayout.EndHorizontal();
        }

        private void Bottom()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("当前共{0}个资源", items.Count), GUILayout.Width(85));
            if (items.Count > 0)
            {
                GUILayout.Space(15);

                if (items.Count == curSelectCount)
                    isSelectAll = true;
                bool temp = isSelectAll;
                isSelectAll = EditorGUILayout.Toggle(isSelectAll, GUILayout.Width(15));
                if (isSelectAll != temp) ChangeSelectAll(isSelectAll);
                GUILayout.Label(string.Format("已选中{0}个资源", curSelectCount), GUILayout.Width(85));

                if (curSelectCount > 0)
                {
                    if (GUILayout.Button("删除所选项", GUILayout.Width(100))) DelSelectItem();
                }
            }
            GUILayout.EndHorizontal();
        }

        //返回到主界面
        private void Return()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
        }

        //添加所选项
        private void AddSelect()
        {
            foreach (var v in newItems)
            {
                v.isNew = false;
                v.index = items.Count;
                items.Add(v);
                itemsPath.Add(v.assetItem.path);
            }

            Selection.objects = null;

            foreach (var v in items)
            {
                v.isRepeat = false;
            }
        }

        //全选或取消
        private void ChangeSelectAll(bool select)
        {
            foreach (var v in items)
            {
                v.isSelect = select;
            }
        }

        //删除所选择的资源
        private void DelSelectItem()
        {
            List<Item> delItems = new List<Item>();
            foreach (var v in items)
            {
                if (v.isSelect)
                    delItems.Add(v);
            }
            foreach (var del in delItems)
            {
                items.Remove(del);
                itemsPath.Remove(del.assetItem.path);
            }
        }

        //检测资源异常
        private void CheckAsset()
        {
            int errorCount = 0;
            foreach (var v in items)
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(v.assetItem.path);
                if (!obj)
                {
                    v.isSelect = true;
                    errorCount++;
                }
            }

            EditorUtility.DisplayDialog("提示",string.Format("检测完成,共{0}个错误资源!",errorCount), "确定");
        }

        //保存当前配置
        private void Save()
        {
            assetData.buildAssets.Clear();
            foreach (var v in items)
            {
                assetData.buildAssets.Add(v.assetItem);
            }
            AssetDatabase.SaveAssetIfDirty(assetData);
            EditorUtility.SetDirty(assetData);
            buildSetting.Save();
            EditorUtility.DisplayDialog("提示", "保存完成!", "确定");
            AssetDatabase.Refresh();

        }

        //添加依赖包体
        private void AddDependPackage()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.EditorModule,assetData);
        }

    }
}

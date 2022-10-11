using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GameFramework.Runtime.Assets;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class AssetBundleBuildSetting : BaseEditorWindow
    {
        public enum Page
        {
            Main,
            EditorAsset,
            EditorModule,
            LocalAsset,
        }

        public static void Open(BuildAssetConfig assetConfig)
        {
            var window = Open<AssetBundleBuildSetting>(500, 600, "资源打包");
            window.assetConfig = assetConfig;
            window.Start();
        }

        public BuildAssetConfig assetConfig;
        private Dictionary<Page, BuildAssetPage> pageMap;
        private BuildAssetPage curPage;

        protected override void Init()
        {
            pageMap = new Dictionary<Page, BuildAssetPage>();
            LoadLocalConfig();
        }
         
        private void LoadLocalConfig()
        {
            string path = AppConst.AppConfigPath;
            if (!File.Exists(path))
                AppConst.config = new LocalCommonConfig();
            else
                AppConst.config = JsonObject.Deserialize<LocalCommonConfig>(File.ReadAllText(path));
        }

        private void Start()
        {
            ChangePage(Page.Main);
        }

        public void ChangePage(Page page, object param = null)
        {
            BuildAssetPage buildAssetPage = null;
            if (!pageMap.TryGetValue(page, out buildAssetPage))
            {
                switch (page)
                {
                    case Page.Main:
                        buildAssetPage = new PageMain(this, page);
                        break;
                    case Page.EditorAsset:
                        buildAssetPage = new PageEditorAsset(this, page);
                        break;
                    case Page.EditorModule:
                        buildAssetPage = new PageEditorModule(this, page);
                        break;
                    case Page.LocalAsset:
                        buildAssetPage = new PageLocalAssetsManager(this, page);
                        break;
                }
                if (buildAssetPage == null) return;
                pageMap.Add(page, buildAssetPage);
            }
            if (curPage != null) curPage.Exit();
            curPage = buildAssetPage;
            curPage.Enter(param);
        }

        private void OnGUI()
        {
            if (curPage == null)
                Init();
            curPage.Run();
        }

        public void Save()
        {
            AssetDatabase.SaveAssetIfDirty(assetConfig);
            EditorUtility.SetDirty(assetConfig);
            AssetDatabase.Refresh();
        }

        //当前发布设置平台名称
        public string PlatformName
        {
            get
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:
                        return "Android";
                    case BuildTarget.iOS:
                        return "iOS";
                    case BuildTarget.StandaloneWindows64:
                    case BuildTarget.StandaloneWindows:
                        return "Windows";
                }

                return "None";
            }
        }
    }

    public class GeneratedFileList : BaseEditorWindow
    {
        [MenuItem("Tools/Other/生成FileList")]
        private static void Open()
        {
            Open<GeneratedFileList>(500, 150, "生成FileList");
        }

        private string path;

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("资源根路径:");
            path = GUILayout.TextField(path);
            GUILayout.Space(10);
            if (GUILayout.Button("生成", GUILayout.Height(30), GUILayout.Width(150))) Generated();
        }

        private void Generated()
        {
            GenerateFileList(path, 1);
        }

        //生成文件列表
        private void GenerateFileList(string path, int version)
        {
            path = NormalizePath(path);
            if (!path.EndsWith("/"))
                path += "/";
            string fileListPath = path + "files.txt";
            if (File.Exists(fileListPath))
                File.Delete(fileListPath);
            AssetFileEntity fileEntity = new AssetFileEntity();
            fileEntity.files = new List<AssetFileEntity.FileItem>();
            string[] files = GetFiles(path);
            foreach (var file in files)
            {
                string name = NormalizePath(file).Replace(path, "");
                string md5 = Util.md5file(file);
                int size = (int)new FileInfo(file).Length;
                var item = new AssetFileEntity.FileItem();
                item.name = name;
                item.md5 = md5;
                item.size = size;
                fileEntity.files.Add(item);
            }
            fileEntity.version = version;
            fileEntity.moduleName = new DirectoryInfo(path).Name;
            File.WriteAllText(fileListPath, JsonObject.Serialize(fileEntity));
            Debug.Log("生成完成:" + fileListPath);
        }

        private string[] GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            List<string> list = new List<string>();
            foreach (var file in files)
            {
                list.Add(file);
            }
            return list.ToArray();
        }

        //统一路径格式
        private string NormalizePath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}
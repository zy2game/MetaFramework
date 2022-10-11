using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class AssetManager : Editor
    {
        //资源配置文件夹路径
        public static readonly string configDirPath = "Assets/ArtistRes/assetBuildConfig/";
        public static readonly string configName = "BuildAssetConfig.asset";
        private static readonly string  assetRootPath= "Assets/ArtistRes";//资源根目录

        [MenuItem("Tools/资源管理", false, 0)]
        public static void Open()
        {
            DirectoryInfo rootDi = null;
            if (!Directory.Exists(assetRootPath))
            {
                if (EditorUtility.DisplayDialog("提示", "没有找到资源根目录,是否创建?", "确定", "取消"))
                {
                    rootDi = Directory.CreateDirectory(assetRootPath);
                }
                else
                    return;
            }
            else
                rootDi = new DirectoryInfo(assetRootPath);

            bool isAssetProject = false;//是否是资源工程
            if (File.Exists(configDirPath+ configName))
                isAssetProject = true;

            string configPath = configDirPath + configName;
            BuildAssetConfig assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
            if (assetConfig == null)//配置文件不存在
            {
                if (isAssetProject)//如果是资源工程则创建配置文件
                {
                    if (!Directory.Exists(configDirPath))
                        Directory.CreateDirectory(configDirPath);
                    var config = CreateInstance<BuildAssetConfig>();
                    AssetDatabase.CreateAsset(config, configPath);
                    AssetDatabase.Refresh();
                    assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
                }
            }

            if (isAssetProject)
                AssetBundleBuildSetting.Open(assetConfig);
            else
                AssetLinkEditor.Open();
        }
    }
}

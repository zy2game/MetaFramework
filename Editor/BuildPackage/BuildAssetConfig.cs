using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameEditor.BuildAsset
{
    public enum AssetType
    {
        Common,//公共资源
        Module,//模块资源
        Main,//主资源
    }

    [System.Serializable]
    public class BuildAssetItem
    {
        public Object asset;
        public string name 
        {
            get
            {
                if (asset == null) return string.Empty;
                return asset.name;
            }
        }//资源名
        public string path
        {
            get
            {
                if (asset == null) return string.Empty;
                return AssetDatabase.GetAssetPath(asset);
            }
        }//资源路径
    }

    public class BuildAssetConfig : ScriptableObject
    {
        public string buildRootPath = "./assetbuild/";//资源打包根目录
        public string extName = ".unity3d";//打包资源后缀名                                          
        public const string buildCachePath = "BuildCache/"; //打包缓存路径       
        public const string buildTempPath = "BuildTemp/"; //打包临时路径      
        public const int cacheCount = 10;//版本缓存数量

        public List<BuildAssetData> buidlAssetDataList;//资源包列表
         
        //获取公共资源
        public BuildAssetData GetCommonAssetData()
        {
            foreach (var v in buidlAssetDataList)
            {
                if (v.assetType == AssetType.Common)
                    return v;
            }
            return null;
        }

        public BuildAssetData FindAssetData(string module)
        {
            foreach (var v in buidlAssetDataList)
            {
                if (module.Equals(v.moduleName))
                    return v;
            }

            return null;
        }
    }
}

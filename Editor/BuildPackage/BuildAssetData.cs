using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace GameEditor.BuildAsset
{
    public class BuildAssetData : ScriptableObject
    {
        public AssetType assetType;//资源类型
        public int version;//资源版本
        public DefaultAsset assetRoot;
        public string moduleName
        {
            get
            {
                if (!assetRoot) return string.Empty;
                return assetRoot.name.ToLower();
            }
        }//包名
        public string alias;//别名
        public List<BuildAssetData> dependPackage;//依赖包体
        public List<DefaultAsset> codesPath;//代码包路径
        public List<BuildAssetItem> buildAssets;//资源列表

        //获取深度资源依赖
        public static List<BuildAssetData> GetDephDepends(BuildAssetData assetData)
        {
            List<BuildAssetData> dephDepend = new List<BuildAssetData>();
            GetDephDepends(assetData, dephDepend);
            //移除需要查询的依赖包体
            if (dephDepend.Contains(assetData))
                dephDepend.Remove(assetData);
            return dephDepend;
        }

        static int GetDephDepends(BuildAssetData assetData, List<BuildAssetData> list)
        {
            if (assetData.dependPackage == null) return 0;
            foreach (var v in assetData.dependPackage)
            {
                if (list.Contains(v)) continue;
                list.Add(v);
                GetDephDepends(v, list);
            }
            return list.Count;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetVersion
    {
        public static AssetVersion remoteVersion;
        public static AssetVersion localVersion;

        public int packageVersion;
        public Dictionary<string, int> versionMap;
        public Dictionary<string, string[]> dependsMap;

        public void UpdateVersion(string name,int version)
        {          
            if (versionMap.ContainsKey(name))
            {
                versionMap[name] = version;
                return;
            }
            versionMap.Add(name,version);
        }

        public void UpdateDepends(string name, string[] depends)
        {
            if (versionMap.ContainsKey(name))
            {
                dependsMap[name] = depends;
                return;
            }
            dependsMap.Add(name, depends);
        }

        /// <summary>
        /// 查找模块版本
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public int FindVersion(string moduleName)
        {
            if (versionMap.ContainsKey(moduleName))
                return versionMap[moduleName];
            return 0;
        }

        //深度依赖查询
        public int FindDepends(string moduleName, List<string> dependList)
        {
            if (dependList == null) return 0;
            if (dependsMap == null || !dependsMap.ContainsKey(moduleName)) return 0;
            string[] depends = dependsMap[moduleName];
            if (depends != null)
            {
                foreach (var d in depends)
                {
                    if (dependList.Contains(d))
                        continue;
                    dependList.Add(d);
                    FindDepends(moduleName, dependList);
                }
            }
            return dependList.Count;
        }

        // 查询资源包依赖
        public string[] FindDenpends(string moduleName)
        {
            List<string> dependList = new List<string>();
            FindDepends(moduleName, dependList);
            return dependList.ToArray();
        }

        //获取需要更新的资源包
        public static string[] GetUpdateModulesName(string modeleName)
        {
            if (remoteVersion == null)
            {
                Debug.LogError("远程配置为空");
                return new string[0];
            }
            if (localVersion == null)
            {
                Debug.LogError("本地配置为空");
                return new string[0];
            }
            List<string> updateModules = new List<string>();
            string[] depneds = remoteVersion.FindDenpends(modeleName);
            if (!CompareVersion(modeleName))
                updateModules.Add(modeleName);
            foreach (var name in depneds)
            {
                if (!CompareVersion(name))
                {
                    if (!updateModules.Contains(name))
                        updateModules.Add(name);
                }
            }
            return updateModules.ToArray();
        }

        //对比版本
        public static bool CompareVersion(string moduleName)
        {
            int local = localVersion.FindVersion(moduleName);
            int remote = remoteVersion.FindVersion(moduleName);
            return local == remote;
        }
    }
}

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
        /// ����ģ��汾
        /// </summary>
        /// <param name="moduleName">ģ����</param>
        /// <returns></returns>
        public int FindVersion(string moduleName)
        {
            if (versionMap.ContainsKey(moduleName))
                return versionMap[moduleName];
            return 0;
        }

        //���������ѯ
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

        // ��ѯ��Դ������
        public string[] FindDenpends(string moduleName)
        {
            List<string> dependList = new List<string>();
            FindDepends(moduleName, dependList);
            return dependList.ToArray();
        }

        //��ȡ��Ҫ���µ���Դ��
        public static string[] GetUpdateModulesName(string modeleName)
        {
            if (remoteVersion == null)
            {
                Debug.LogError("Զ������Ϊ��");
                return new string[0];
            }
            if (localVersion == null)
            {
                Debug.LogError("��������Ϊ��");
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

        //�ԱȰ汾
        public static bool CompareVersion(string moduleName)
        {
            int local = localVersion.FindVersion(moduleName);
            int remote = remoteVersion.FindVersion(moduleName);
            return local == remote;
        }
    }
}

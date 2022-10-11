using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    //资源自动管理系统
    public class AssetHandleSmartManager:Singleton<AssetHandleSmartManager>
    {
        //资源释放检测时间
        private float checkReleaseTime=5;
        //上一次检测的时间
        private float lastCheckTime;
        //自动管理对象
        private Dictionary<int, List<AssetHandle>> assetHandleMap=new Dictionary<int, List<AssetHandle>>();
        private Dictionary<int, int> assetBundleBehaviourUseCount = new Dictionary<int, int>();

        //记录资源加载次数
        private Dictionary<string, int> assetLoadCountMap=new Dictionary<string, int>();
        //待释放资源列表
        private Dictionary<string,AssetHandle> waitReleaseMap = new Dictionary<string, AssetHandle>();
        private List<AssetHandle> waitReleaseList = new List<AssetHandle>();

        public AssetHandleSmartManager()
        {
            lastCheckTime = Time.time;
        }

        public List<AssetHandle> GetAssetHandleList(int id)
        {
            if (assetBundleBehaviourUseCount.ContainsKey(id))
                assetBundleBehaviourUseCount[id]++;
            else
                assetBundleBehaviourUseCount.Add(id, 1);
            if (assetHandleMap.TryGetValue(id, out List<AssetHandle> list))
                return list;
            list = new List<AssetHandle>();
            assetHandleMap.Add(id, list);
            return list;
        }

        public void OnAssetBundleBehaviourDestroy(int id)
        {
            if (!assetBundleBehaviourUseCount.ContainsKey(id))
                return;
            assetBundleBehaviourUseCount[id]--;
            if (assetBundleBehaviourUseCount[id] <= 0)
            {
                assetBundleBehaviourUseCount.Remove(id);
                assetHandleMap.Remove(id);
            }
        }

        //添加到待释放资源列表
        public void AddWaitReleaseList(AssetHandle assetHandle)
        {
            if (waitReleaseMap.ContainsKey(assetHandle.path))
                return;
            waitReleaseMap.Add(assetHandle.path,assetHandle);
        }

        //资源加载计数
        public void AssetLoadCount(string path)
        {
            if (assetLoadCountMap.ContainsKey(path))
                assetLoadCountMap[path]++;
            else
                assetLoadCountMap.Add(path, 1);
        }

        public void Update() 
        {
            if (Time.time - lastCheckTime < checkReleaseTime) return;
            lastCheckTime = Time.time;
            CheckRelease();
        }

        //资源释放检测
        private void CheckRelease()
        {
            //获取没有使用的资源
            foreach (var v in waitReleaseMap)
            {
                if (v.Value.refCount <= 0)
                    waitReleaseList.Add(v.Value);
            }
            //释放没有使用的资源
            foreach (var v in waitReleaseList)
            {
                v.Unload(true);
            }
            waitReleaseList.Clear();
            waitReleaseMap.Clear();
        }
    } 
}

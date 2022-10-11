using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetLoadAsync : WaitFinished
    {
        private const int MAXPOOLCOUNT = 100;//对象池最大数量
        private static Queue<AssetLoadAsync> pool = new Queue<AssetLoadAsync>();
        public float progress;
        public int needLoadAssetCount;//需要加载的资源数

        public static AssetLoadAsync Get()
        {
            AssetLoadAsync loadAsync;
            if (pool.Count == 0)
                loadAsync = new AssetLoadAsync();
            else
                loadAsync = pool.Dequeue();
            loadAsync.Reset();
            return loadAsync;
        }

        public Action<AssetHandle> callback;

        public override void Reset()
        {
            base.Reset();
            needLoadAssetCount = 0;
            progress = 0;
        }

        public void Finished(AssetHandle assetHandle)
        {
            base.Finished();
            progress = 1;
            callback?.Invoke(assetHandle);

            if (!pool.Contains(this) && pool.Count < MAXPOOLCOUNT)
                pool.Enqueue(this);
        }
    }

}

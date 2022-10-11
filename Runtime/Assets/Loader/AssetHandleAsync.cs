using System;
using System.Collections.Generic;

namespace GameFramework.Runtime.Assets
{
    public class AssetHandleAsync<T>:WaitFinished where T : UnityEngine.Object
    {
        private const int MAXPOOLCOUNT = 100;//对象池最大数量
        private static Queue<AssetHandleAsync<T>> pool = new Queue<AssetHandleAsync<T>>();

        public static AssetHandleAsync<T> Get()
        {
            AssetHandleAsync<T> handleAsync;
            if (pool.Count == 0)
                handleAsync = new AssetHandleAsync<T>();
            else
                handleAsync = pool.Dequeue();
            handleAsync.Reset();
            return handleAsync;
        }

        public Action<T> callback;

        public void Finished(T obj)
        {
            base.Finished();
            callback?.Invoke(obj);

            if (!pool.Contains(this) && pool.Count < MAXPOOLCOUNT)
                pool.Enqueue(this);
        }
    }
}

using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public abstract class AssetHandle
    {
        public string path { get; private set; }
        public int refCount { get; protected set; }//引用次数  
        public bool isRelease { get; private set; }//资源是否已经释放了
        public string packageName { get; private set; }

        public AssetHandle(string path)
        {
            this.path = path;
            AssetHandleSmartManager.Instance.AssetLoadCount(path);

            int index = path.IndexOf("/");
            if (index == -1)
                index = path.IndexOf(@"\");
            if (index != -1)
                packageName = path.Substring(0, index);
            else
                packageName = string.Empty;
        }

        public void AddRefCount()
        {
            refCount++;
        }

        public void SubRefCount()
        {
            if (refCount <= 0) return;
            refCount--;
            if (refCount == 0)
            {
                //添加到待释放对象池
                AddToReleasePool();
            }
        }

        public void SetRefCount(bool isAdd)
        {
            ResourcesManager.Instance.SetRefCount(this, isAdd);

        }

        protected GameObject Instantiate(GameObject obj, Transform parent = null)
        {
            GameObject go = Object.Instantiate(obj, parent);
            go.name = obj.name;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        //添加到释放对象池
        public abstract void AddToReleasePool();

        /// <summary>
        /// 创建一个游戏对象,生成到场景
        /// </summary>
        /// <param name="parent">游戏对象父物体</param>
        /// <param name="assetName">游戏资源名，为空是区路径最后的名字</param>
        /// <returns></returns>
        public abstract GameObject CreateGameObject(Transform parent = null, string assetName = "");

        /// <summary>
        /// 加载一个资源
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="assetName">资源名称</param>
        /// <returns></returns>
        public abstract Object LoadAsset(System.Type type, string assetName = "");

        /// <summary>
        /// 异步创建游戏对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public abstract AssetHandleAsync<GameObject> CreateGameObjectAsync(Transform parent = null, string assetName = "");

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public abstract AssetHandleAsync<Object> LoadAssetAsync(System.Type type, string assetName = "");

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetName"></param>
        public abstract AsyncOperation LoadSceneAsync(string assetName);
         
        public abstract string[] GetDepends();

        public virtual void Unload(bool unloadAllLoadedObjects)
        {
            if (isRelease && unloadAllLoadedObjects) return;
            isRelease = true;
            if (unloadAllLoadedObjects)
                ResourcesManager.Instance.RemoveCache(path);
        }
    }


}
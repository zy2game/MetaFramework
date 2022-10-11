using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Runtime.Assets
{
    public class AssetBundleHandle : AssetHandle
    {
        public AssetBundle assetBundle { get; private set; }
        private string[] depends;

        //上次加载过的资源
        private string lastLoadAssetName;
        private Object lastLoadAsset;

        private string pathAssetName;

        public AssetBundleHandle(string path, AssetBundle ab) : base(path)
        {
            assetBundle = ab;
            pathAssetName = Path.GetFileNameWithoutExtension(path);
        }

        public AssetBundleHandle(string path,string assetName, AssetBundle ab) : base(path)
        {
            assetBundle = ab;
            pathAssetName = assetName;
        }

        public void SetDepends(string[] depends)
        {
            this.depends = depends;

        }

        public override void AddToReleasePool()
        {
            AssetHandleSmartManager.Instance.AddWaitReleaseList(this);
        }

        public override GameObject CreateGameObject(Transform parent = null, string assetName = "")
        {
            GameObject obj = (GameObject)LoadAsset(typeof(GameObject), assetName);
            if (obj == null)
                return null;
            GameObject go = Instantiate(obj, parent);
            return go;
        }

        public override Object LoadAsset(Type type, string assetName = "")
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = pathAssetName;

            if (lastLoadAsset != null && assetName.Equals(lastLoadAssetName))
                return lastLoadAsset;

            if (assetBundle == null)
            {
                Debug.LogError("加载资源错误,AssetBundle为空:" + path);
                return null;
            }

            var obj = assetBundle.LoadAsset(assetName, type);
            if (!obj)
            {
                Debug.LogError(string.Format("加载资源Path:{0}错误,找不到对应的资源:{1}", path, assetName));
                return null;
            }
            lastLoadAsset = obj;
            lastLoadAssetName = assetName;
            return obj;
        }

        public override AssetHandleAsync<GameObject> CreateGameObjectAsync(Transform parent = null, string assetName = "")
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = pathAssetName;
            AssetHandleAsync<GameObject> handleAsync = AssetHandleAsync<GameObject>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(typeof(GameObject), (obj) =>
            {
                if (obj == null)
                {
                    handleAsync.Finished(null);
                    return;
                }
                GameObject go = Instantiate(obj as GameObject, parent);
                handleAsync.Finished(go);
            }, assetName));
            return handleAsync;
        }

        public override AssetHandleAsync<Object> LoadAssetAsync(Type type, string assetName = "")
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = pathAssetName;
            AssetHandleAsync<Object> handleAsync = AssetHandleAsync<Object>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(type, (obj) =>
             {
                 handleAsync.Finished(obj);
             }, assetName));
            return handleAsync;
        }

        private IEnumerator LoadCor(Type type, Action<Object> func, string assetName)
        {
            if (lastLoadAsset != null && assetName.Equals(lastLoadAssetName))
            {
                yield return null;
                func(lastLoadAsset);
                yield break;
            }
            var request = assetBundle.LoadAssetAsync(assetName, type);
            yield return request;
            if (request.asset)
            {
                lastLoadAsset = request.asset;
                lastLoadAssetName = assetName;
            }
            else
            {
                Debug.LogError(string.Format("加载资源Path:{0}错误,找不到对应的资源:{1}", path, assetName));
            }
            func(request.asset);
        }

        public override string[] GetDepends()
        {
            return depends;
        }

        public override void Unload(bool unloadAllLoadedObjects)
        {
            if (assetBundle != null)
                assetBundle.Unload(unloadAllLoadedObjects);
            if (unloadAllLoadedObjects)
                assetBundle = null;
            base.Unload(unloadAllLoadedObjects);
        }

        public override AsyncOperation LoadSceneAsync(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = pathAssetName;
           return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(assetName);
        }
    }
}
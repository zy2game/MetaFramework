
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetManifest
    {
        private AssetBundleHandle assetBundleHandle;
        public AssetBundleManifest manifest { get; private set; }
        private string[] emptyArr;

        public AssetManifest(AssetBundleHandle assetBundleHandle,string assetName)
        {
            assetBundleHandle.AddRefCount();
            this.assetBundleHandle = assetBundleHandle;          
            manifest = assetBundleHandle.LoadAsset(typeof(AssetBundleManifest), "AssetBundleManifest") as AssetBundleManifest;
            if (manifest == null)
            {
                Debug.LogError("加载AssetBundleManifest错误:"+ assetName);
                return;
            }
            emptyArr = new string[0];
        }

        public string[] GetAllAssetBundleNames()
        {
            return manifest == null ? emptyArr : manifest.GetAllAssetBundles();
        }

        public string[] GetAllAssetBundlesWithVariant()
        {
            return manifest == null ? emptyArr : manifest.GetAllAssetBundlesWithVariant();
        }

        public string[] GetAllDependencies(string assetbundleName)
        {
            return manifest == null ? emptyArr : manifest.GetAllDependencies(assetbundleName);
        }

        public string[] GetDirectDependencies(string assetbundleName)
        {
            return manifest == null ? emptyArr : manifest.GetDirectDependencies(assetbundleName);
        }
    }
}

using System.Collections.Generic;
using System.IO;

namespace GameFramework.Runtime.Assets
{
    public abstract class AssetLoad
    {
        protected string GetAssetName(string path)
        {
            return Path.GetFileNameWithoutExtension(path).ToLower(); 
        }

        public abstract AssetHandle Load(string path);

        public abstract AssetLoadAsync LoadAsync(string path);

        public abstract void SetRefCount(AssetHandle assetHandle, bool isAdd);

        public abstract List<AssetHandle> GetAssetHandleListByPackageName(string packageName);

        public virtual void RemoveCache(string path)
        {

        }

        public virtual void UnloadByPath(string path,bool unloadAllLoadedObjects) { }

        public virtual void UnloadByPackageName(string name, bool unloadAllLoadedObjects) { }

        

    }
}
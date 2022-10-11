#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Runtime.Assets
{
    public class EditorAssetLoad : AssetLoad
    {
        //加载速度
        private const int loadSpeed = 1024 * 10;

        public override AssetHandle Load(string path)
        {
            EditorAssetHandle handle = new EditorAssetHandle(path);
            return handle;
        }

        public override AssetLoadAsync LoadAsync(string path)
        {
            AssetLoadAsync loadAsync = AssetLoadAsync.Get();
            CorManager.Instance.StartCoroutine(LoadCor(path, loadAsync));
            return loadAsync;
        }

        //编辑器下模拟异步加载
        private IEnumerator LoadCor(string path, AssetLoadAsync loadAsync)
        {
            EditorAssetHandle handle = new EditorAssetHandle(path);            
            long fileSize = handle.fileSize;
            float dt = fileSize/loadSpeed ;
            dt = (dt > 1 ? 1 : dt) < 0.01f ? 0.01f : dt;

            loadAsync.progress = 0;
            while (loadAsync.progress < 1)
            {
                yield return null;
                loadAsync.progress += dt;
            }
            loadAsync.progress = 1;
          
            loadAsync.Finished(handle);
        }

        public override void SetRefCount(AssetHandle assetHandle, bool isAdd)
        {

        }

        //获取包体的所有资源
        public override List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            return new List<AssetHandle>();
        }
    }
}
#endif
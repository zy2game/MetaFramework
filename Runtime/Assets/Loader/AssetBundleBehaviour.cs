using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetBundleBehaviour : MonoBehaviour
    {
        public static int behaviourId = 0;

        private static int PopId()
        {
            behaviourId++;
            return behaviourId;
        }

        private List<AssetHandle> handleList;
        private bool isAwake = false;

        public int id = -1;

        public void AddAssetHandle(AssetHandle assetHandle)
        {
            if (id == -1)
                id = PopId();
            if (handleList == null)
                handleList = AssetHandleSmartManager.Instance.GetAssetHandleList(id);
            if (handleList.Contains(assetHandle))
                return;
            handleList.Add(assetHandle);
            if (isAwake)
                assetHandle.SetRefCount(true);
        }

        private void Awake()
        {
            isAwake = true;
            if (id == -1)
                id = PopId();
            if (handleList == null)
                handleList = AssetHandleSmartManager.Instance.GetAssetHandleList(id);
            foreach (var handle in handleList)
            {
                handle.SetRefCount(true);
            }

        }

        private void OnDestroy()
        {
            AssetHandleSmartManager.Instance.OnAssetBundleBehaviourDestroy(id);
            ReleaseAll();
        }

        public void ReleaseAll()
        {
            if (handleList == null) return;
            foreach (var handle in handleList)
            {
                handle.SetRefCount(false);
            }
        }

        public void ReleaseHandle(AssetHandle handle)
        {
            if (handleList == null) return;
            if (!handleList.Contains(handle)) return;
            handleList.Remove(handle);
            handle.SetRefCount(false);
        }

        public void ReleaseByName(string name)
        {
            if (handleList == null) return;
            AssetHandle handle = null;
            foreach (var v in handleList)
            {
                if (v.path.Equals(name))
                {
                    handle = v;
                    break;
                }    
            }
            ReleaseHandle(handle);
        }
    }
}

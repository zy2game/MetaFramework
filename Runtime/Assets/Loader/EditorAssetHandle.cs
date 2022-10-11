
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEditor.SceneManagement;

namespace GameFramework.Runtime.Assets
{
    public class EditorAssetHandle : AssetHandle
    {
        public const string searchPath = "Assets/ArtistRes/";
        public long fileSize;

        public EditorAssetHandle(string path) : base(path)
        {
            var fullPath = GetAssetFullPath();
            if (File.Exists(fullPath))
                fileSize = new FileInfo(fullPath).Length;
        }

        public override void AddToReleasePool()
        {
           
        }

        public override GameObject CreateGameObject(Transform parent=null, string assetName = "")
        {
            GameObject obj = (GameObject)LoadAsset(typeof(GameObject),assetName);
            if (obj == null)
                return null;
            GameObject go = Instantiate(obj , parent);
            return go;
        }       

        public override Object LoadAsset(System.Type type, string assetName = "")
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = Path.GetFileNameWithoutExtension(path);
            string pt = searchPath + path;
            string dir = Path.GetDirectoryName(pt);
            if (Directory.Exists(dir))
            {
                var t = EditorLoadAsset(type,dir, assetName);
                if (t) return t;
            }

            if (Directory.Exists(pt))
            {
                var t = EditorLoadAsset(type,pt, assetName);
                if (t) return t;
            }

            return null;
        }             

        private Object EditorLoadAsset(System.Type type, string dir, string fileName) 
        {
            string[] files = Directory.GetFiles(dir, fileName + ".*");

            foreach (var v in files)
            {
                if (!v.EndsWith(".meta"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath(v, type);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }        

        public override AssetHandleAsync<GameObject> CreateGameObjectAsync(Transform parent = null, string assetName = "")
        {
            AssetHandleAsync<GameObject> handleAsync =  AssetHandleAsync<GameObject>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(typeof(GameObject), (obj) =>
            {
                if (obj == null)
                {
                    handleAsync.Finished(null);
                    return;
                }
                GameObject go=Instantiate(obj as GameObject,parent);               
                handleAsync.Finished(go);
            }, assetName));
            return handleAsync;
        }

        public override AssetHandleAsync<Object> LoadAssetAsync(System.Type type, string assetName = "")
        {
            AssetHandleAsync<Object> handleAsync =  AssetHandleAsync<Object>.Get();
            CorManager.Instance.StartCoroutine(LoadCor(type,(t)=> 
            {
                handleAsync.Finished(t);
            },assetName));

            return handleAsync;
        }

        //编辑器下模拟异步加载
        private IEnumerator LoadCor(System.Type type,  System.Action<Object> func, string assetName) 
        {
            yield return null;
            var obj = LoadAsset(type,assetName);
            func(obj);
        }

        public override string[] GetDepends()
        {
            return null;
        }

        public override AsyncOperation LoadSceneAsync(string assetName)
        {
            var scenePath = GetAssetFullPath(assetName);
            return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, default);
        }

        private string GetAssetFullPath(string assetName="") 
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = Path.GetFileNameWithoutExtension(path);
            string pt = searchPath + path;
            string dir = Path.GetDirectoryName(pt);
            string[] files = Directory.GetFiles(dir, assetName + ".*");

            string fullPath = string.Empty;
            foreach (var v in files)
            {
                if (!v.EndsWith(".meta"))
                {
                    fullPath = v;
                    break;
                }
            }
            return fullPath;
        }
    }
}
#endif
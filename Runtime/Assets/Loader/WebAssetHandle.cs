using System;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;


namespace GameFramework.Runtime.Assets
{
    //远程资源类型
    public enum RomoteAssetType
    {
        Image,//图片
        Audio,//音频
        AssetBundle,//ab包
        Video,//视频
        Text,//文本
        Bytes,
    }
        

    public class WebAssetHandle : AssetHandle
    {
        public RomoteAssetType type { get; internal set; }
        //资源对象缓存
        public byte[] data { get; internal set; }
        public string text { get; internal set; }        
        public Texture2D t2d { get; internal set; }
        public AudioClip audioClip { get; internal set; }
        public AssetBundle assetBundle { get; internal set; }
        public string videoPath { get; internal set; }

        public WebAssetHandle(string path, RomoteAssetType type) : base(path)
        {
            this.type = type;
        }

        public override void AddToReleasePool()
        {
            AssetHandleSmartManager.Instance.AddWaitReleaseList(this);
        }

        public override GameObject CreateGameObject(Transform parent = null, string assetName = "")
        {
            Debug.LogError("远程资源对应加载不到游戏对象");
            return null;
        }

        public override AssetHandleAsync<GameObject> CreateGameObjectAsync(Transform parent = null, string assetName = "")
        {
            AssetHandleAsync<GameObject> handleAsync = AssetHandleAsync<GameObject>.Get();
            CorManager.Instance.DelayCall(this, 0, () =>
            {
                Debug.LogError("远程资源对应加载不到游戏对象");
                handleAsync.Finished(null);                
            });
            return handleAsync;
        }

        public override string[] GetDepends()
        {
            return new string[0];
        }
      
        public override AssetHandleAsync<Object> LoadAssetAsync(Type type, string assetName = "")
        {
            AssetHandleAsync<Object> handleAsync = AssetHandleAsync<Object>.Get();
            CorManager.Instance.DelayCall(this, 0, () =>
            {
                Debug.LogError("远程资源已经加载过了,使用同步加载方式");
                handleAsync.Finished(null);
            });
            return handleAsync;
        }

        public override AsyncOperation LoadSceneAsync(string assetName)
        {
            throw new NotImplementedException("当前Handle是远程资源,无法加载场景");
        }

        public override Object LoadAsset(Type type, string assetName = "")
        {
            switch (this.type)
            {
                case RomoteAssetType.Image:
                    return t2d;
                case RomoteAssetType.Audio:
                    return audioClip;
                default:
                    return null;
            }
        }

        public void SetAsset(UnityWebRequest request,string localPath)
        {
            switch (type)
            {
                case RomoteAssetType.Image:
                    
                    t2d = DownloadHandlerTexture.GetContent(request);
                    break;
                case RomoteAssetType.Text:
                    text = request.downloadHandler.text;
                    break;
                case RomoteAssetType.Audio:
                    audioClip = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case RomoteAssetType.Video:
                    videoPath = localPath;
                    break;
                case RomoteAssetType.Bytes:
                    data = request.downloadHandler.data;
                    break;
            }
        }

        //释放资源
        public override void Unload(bool unloadAllLoadedObjects)
        {
            switch (type)
            {
                case RomoteAssetType.Image:
                    Resources.UnloadAsset(t2d);
                    t2d = null;
                    break;
                case RomoteAssetType.Text:
                    text = string.Empty;
                    break;
                case RomoteAssetType.Audio:
                    Resources.UnloadAsset(audioClip);
                    audioClip =null;
                    break;
                case RomoteAssetType.Video:
                    videoPath = string.Empty;
                    break;
                case RomoteAssetType.Bytes:
                    data = null;
                    break;
            }
         
            base.Unload(unloadAllLoadedObjects);
        }
    }
}

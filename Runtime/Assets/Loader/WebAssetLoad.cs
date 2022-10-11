using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Threading;

namespace GameFramework.Runtime.Assets
{
    ///从网络上加载资源
    ///资源名唯一,不同玩家上传的同一位置的资源使用玩家id区分
    ///相同类型的资源可以用版本号区分,同一位置使用的资源玩家每次上传更新的资源名带版本号
    ///资源地址:http://wwww.test.com/assetserver/(玩家ID位置ID_版本.后缀名||资源名_版本.后缀名)
    ///后缀名用于区分是什么类型的资源(*.png *.jpg|图片 *.mp3 *.wav *.ogg|音频 *.unity3d|ab包 *.mp4|视频文件 *.txt|文本 *.bin|二进制数据)
    ///玩家ID_位置ID_版本用于判断是否替换本地资源和清理缓存
    public class WebAssetLoad : AssetLoad
    {
        private const string configName = "datalist.txt";
        private long clearCacheLimitTicks =864000L* 10000000L;//缓存清理时间(10天)

        private Dictionary<string, AssetHandle> assetHandleMap;//资源缓存
        private LocalWebAssetConfig localAssetConfig;//本地资源配置
        private float updateConfigDelayTime = 2;//配置更新延时时间
        private int updateConfigTimerId = -1;
        private string curLoadingAssetName;//当前正在加载的资源
        private string curClearAssetName;//当前正在清理的资源

        public WebAssetLoad()
        {
            assetHandleMap = new Dictionary<string, AssetHandle>();

            string configPath = AppConst.WebDataCachePath + configName;
            if (!File.Exists(configPath))
            {
                localAssetConfig = new LocalWebAssetConfig();
                localAssetConfig.map = new Dictionary<string, LocalWebAssetItem>();
            }
            else
            {
                localAssetConfig = JsonObject.Deserialize<LocalWebAssetConfig>(File.ReadAllText(configPath));
                if (localAssetConfig.map == null)
                    localAssetConfig.map = new Dictionary<string, LocalWebAssetItem>();
            }
            ClearCache();
        }

        public override List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            return new List<AssetHandle>();
        }

        //从网络上下载资源无法使用同步方式
        public override AssetHandle Load(string path)
        {
            return null;
        }

        public override AssetLoadAsync LoadAsync(string url)
        {
            AssetLoadAsync loadAsync = AssetLoadAsync.Get();
            if (assetHandleMap.TryGetValue(url, out AssetHandle assetHandle))
            {
                //已经加载了这个资源包,等待到下一帧返回
                CorManager.Instance.DelayCall(this, 0, () =>
                {
                    loadAsync.Finished(assetHandle);
                });
                return loadAsync;
            }
            CorManager.Instance.StartCoroutine(LoadCor(url, loadAsync));
            return loadAsync;
        }

        public override void SetRefCount(AssetHandle handle, bool isAdd)
        {
            if (isAdd)
                handle.AddRefCount();
            else
                handle.SubRefCount();
        }

        private IEnumerator LoadCor(string url, AssetLoadAsync loadAsync)
        {
            string path = url;

            string assetName = string.Empty;
            string assetVersion = string.Empty;
            string ext = string.Empty;
            GetUrlInfo(url, ref assetName, ref assetVersion, ref ext);

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("解析url错误:" + url);
                loadAsync.Finished(null);
                yield break;
            }
            bool isLoadLocalAsset = false;//加载的是否是本地资源
            LocalWebAssetItem localItem;

            if (localAssetConfig.HasAsset(assetName))//本地是否有这个资源
            {
                localItem = localAssetConfig.GetAssetItem(assetName);
                if (localItem.version.Equals(assetVersion))//对比本地和要下载的资源版本号
                {
                    //版本号一致,从本地加载
                    path = AppConst.WebDataCachePath + assetName;
                    if (!File.Exists(path))
                    {
                        //本地不存在文件,转从服务器加载
                        path = url;
                    }
                    else
                        isLoadLocalAsset = true;
                }
            }
            else
            {
                localItem = new LocalWebAssetItem();
                localItem.name = assetName;
                localItem.version = assetVersion;
                localItem.ext = ext;
            }

            RomoteAssetType assetType = GetAssetType(ext);

            using (UnityWebRequest request = GetRequest(assetType, path, ext))
            {
                curLoadingAssetName = assetName;
                if (!string.IsNullOrEmpty(curClearAssetName))
                {
                    //正在清理要下载的资源,等待清理结束
                    while (curLoadingAssetName.Equals(curClearAssetName))
                    {
                        yield return null;
                    }
                }

                if (assetType == RomoteAssetType.Image)
                    request.downloadHandler = new DownloadHandlerTexture();
                request.SendWebRequest();

                while (!request.isDone)
                {
                    yield return null;
                    loadAsync.progress = request.downloadProgress;
                }
                //再次检测是否已经加载了
                if (!assetHandleMap.TryGetValue(url, out AssetHandle assetHandle))
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        curLoadingAssetName = string.Empty;
                        //不是从网络下载并且本地不存在，尝试重新从网络上下载
                        if (!path.StartsWith("http")&&!File.Exists(path))
                        {
                            localAssetConfig.RemoveItem(assetName);
                            CorManager.Instance.StartCoroutine(LoadCor(url, loadAsync));
                            yield break;
                        }
                        Debug.LogError("下载资源错误:" + path + "\n" + request.error);                       
                        loadAsync.Finished(null);                        
                        yield break;
                    }

                    localItem.lastTime = System.DateTime.Now.Ticks;
                    localAssetConfig.AddItem(localItem);

                    byte[] bts = request.downloadHandler.data;
                    if (!isLoadLocalAsset)
                    {
                        localItem.size = bts.Length;
                        path = SaveDownloadAsset(bts, localItem);
                    }
                    else
                    {
                        UpdateLocalConfig();
                    }
                    if (assetType != RomoteAssetType.AssetBundle)
                    {
                        WebAssetHandle webAssetHandle = new WebAssetHandle(url, assetType);
                        webAssetHandle.SetAsset(request, path);
                        assetHandle = webAssetHandle;
                    }
                    else
                    {
                        AssetBundleHandle assetBundleHandle = new AssetBundleHandle(url, assetName, AssetBundle.LoadFromMemory(bts));
                        assetBundleHandle.SetDepends(new string[0]);
                        assetHandle = assetBundleHandle;
                    }
                    assetHandleMap.Add(url, assetHandle);//添加到资源缓存map中
                    loadAsync.Finished(assetHandle);
                }
                else
                {
                    UpdateLocalConfig();
                    localItem.lastTime = System.DateTime.Now.Ticks;
                    loadAsync.Finished(assetHandle);
                }
                curLoadingAssetName = string.Empty;
            }
        }

        private UnityWebRequest GetRequest(RomoteAssetType type, string url, string exit)
        {
            switch (type)
            {
                //case RomoteAssetType.AssetBundle:
                //    return  UnityWebRequestAssetBundle.GetAssetBundle(url);
                case RomoteAssetType.Image:
                    return UnityWebRequestTexture.GetTexture(url);
                case RomoteAssetType.Audio:
                    return UnityWebRequestMultimedia.GetAudioClip(url, GetAudioType(exit));
                default:
                    return UnityWebRequest.Get(url);
            }
        }

        public override void RemoveCache(string path)
        {
            assetHandleMap.Remove(path);
        }

        private AudioType GetAudioType(string ext)
        {
            switch (ext)
            {
                case ".mp3":
                    return AudioType.MPEG;
                case ".wav":
                    return AudioType.WAV;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                default:
                    return AudioType.UNKNOWN;
            }
        }

        //获取资源名(玩家ID_位置ID)
        private void GetUrlInfo(string url, ref string assetName, ref string version, ref string ext)
        {
            int index = url.LastIndexOf("/");
            if (index == -1 || url.Length <= index + 1) return;
            string endcontent = url.Substring(index + 1);
            index = endcontent.IndexOf("_");
            if (index == -1) return;
            assetName = endcontent.Substring(0, index);
            if (endcontent.Length < index + 1) return;
            endcontent = endcontent.Substring(index + 1);
            index = endcontent.IndexOf(".");
            if (index == -1) return;
            version = endcontent.Substring(0, index);
            endcontent = endcontent.Substring(index);
            index = endcontent.IndexOf("?");
            if (index == -1)
            {
                if (endcontent.Length == 0) return;
                ext = endcontent.Substring(0);
            }
            else
            {
                if (endcontent.Length <= index) return;
                ext = endcontent.Substring(0, index);
            }

            if (ext.Equals(".mp4"))
                assetName += ext;
        }

        private RomoteAssetType GetAssetType(string ext)
        {
            switch (ext)
            {
                case ".png":
                case ".jpg":
                    return RomoteAssetType.Image;
                case ".mp3":
                case ".wav":
                case ".ogg":
                    return RomoteAssetType.Audio;
                case ".unity3d":
                    return RomoteAssetType.AssetBundle;
                case ".mp4":
                    return RomoteAssetType.Video;
                case ".txt":
                    return RomoteAssetType.Text;
                default:
                    return RomoteAssetType.Bytes;
            }
        }

        //保存下载的资源
        private string SaveDownloadAsset(byte[] bts, LocalWebAssetItem item)
        {
            if (bts == null || bts.Length == 0) return string.Empty;
            if (!Directory.Exists(AppConst.WebDataCachePath))
                Directory.CreateDirectory(AppConst.WebDataCachePath);
            string path = AppConst.WebDataCachePath + item.name;
            File.WriteAllBytes(path, bts);
            SaveConfig();
            return path;
        }

        //保存配置
        private void SaveConfig()
        {
            if (localAssetConfig == null) return;
            string content = JsonObject.Serialize(localAssetConfig);
            File.WriteAllText(AppConst.WebDataCachePath + configName, content);
        }

        //更新本地配置
        private void UpdateLocalConfig()
        {
            if (updateConfigTimerId != -1)
                CorManager.Instance.StopCor(this, updateConfigTimerId);
            updateConfigTimerId = CorManager.Instance.DelayCall(this, updateConfigDelayTime, () =>
            {
                updateConfigTimerId = -1;
                SaveConfig();
            });
        }

        //缓存清理
        private void ClearCache()
        {
            if (localAssetConfig.map == null) return;
            long curTicks = System.DateTime.Now.Ticks;
            List<LocalWebAssetItem> needClearItems = new List<LocalWebAssetItem>();
            foreach (var v in localAssetConfig.map)
            {
                var item = v.Value;
                if (curTicks - item.lastTime > clearCacheLimitTicks)
                {
                    Debug.Log(curTicks - item.lastTime + "  " + curTicks + "  " + v.Value.name);
                    needClearItems.Add(item);
                }
            }
            if (needClearItems.Count == 0)
                return;
            CorManager.Instance.StartCoroutine(ClearCacheCor(needClearItems));
        }

        //间隔清理缓存,防止缓存过多卡主线程,或者在游戏中重新加载了
        private IEnumerator ClearCacheCor(List<LocalWebAssetItem> needClearItems)
        {
            string dirPath = AppConst.WebDataCachePath;
            bool clearFinished = false;//清理完成
            List<string> clearAssetNames = new List<string>();
            new Thread(() =>
            {               
                foreach (var v in needClearItems)
                {
                    //已经重新加载了或者正在加载这个资源
                    if (assetHandleMap.ContainsKey(v.name) || v.name.Equals(curLoadingAssetName))
                        continue;
                    string delPath = dirPath + v.name;
                    if (!File.Exists(delPath))
                    {
                        clearAssetNames.Add(v.name);
                        continue;
                    }
                    clearAssetNames.Add(v.name);
                    curClearAssetName = v.name;
                    Thread.Sleep(100);//延时删除,保证主线程能有效获取到当前删除的资源名
                    File.Delete(delPath);                 
                    Thread.Sleep(100);
                    curClearAssetName = string.Empty;
                }
                clearFinished = true;

            }).Start();

            WaitForSeconds wait = new WaitForSeconds(0.1f);
            while (!clearFinished)
            {
                yield return wait;
            }

            foreach (var name in clearAssetNames)
            {
                localAssetConfig.RemoveItem(name);
            }
            SaveConfig();
        }

        public class LocalWebAssetConfig
        {
            public Dictionary<string, LocalWebAssetItem> map;

            public bool HasAsset(string name)
            {
                return map.ContainsKey(name);
            }

            public LocalWebAssetItem GetAssetItem(string name)
            {
                if (!HasAsset(name)) return null;
                return map[name];
            }

            public void AddItem(LocalWebAssetItem item)
            {
                if (HasAsset(item.name)) RemoveItem(item.name);
                map.Add(item.name, item);
            }

            public void RemoveItem(string name)
            {
                if (!HasAsset(name)) return;
                map.Remove(name);
            }

            //获取缓存大小
            public long GetCacheSize()
            {
                if (map == null) return 0;
                long size = 0;
                foreach (var v in map)
                {
                    size += v.Value.size;
                }
                return size;
            }
        }

        public class LocalWebAssetItem
        {
            public string name;//资源名
            public string version;//资源版本
            public long lastTime; //最后一次使用时间
            public int size;//资源大小
            public string ext;//资源后缀
        }
    }

}

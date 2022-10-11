using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework.Runtime.Assets
{
    //本地资源释放
    public class ReleaseAssets
    {
        private const string fileName = "files.txt";
        private const int maxReleaseCount = 4;//同时释放数量
        private const int connectLimitCount = 5;//链接尝试
        private WaitFinished async;
        private string rootPath;
        private Queue<FileItem> fileItems;
        private List<AssetFileEntity> fileEntities;
        private AssetVersion version;
        private int releaseCount;
        private int releaseTotalCount;
        private int releaseFileCount;
        private bool isError;

        //释放本地资源
        public WaitFinished Release()
        {
            async = new WaitFinished();
            rootPath = AppConst.AppAssetPath;
            CorManager.Instance.StartCoroutine(LoadAllReleaseFile());
            return async;
        }

        //加载所有需要释放发文件
        private IEnumerator LoadAllReleaseFile()
        {
            string versionPath = rootPath + "version.txt";
            UnityWebRequest request = UnityWebRequest.Get(versionPath);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("读取不到本地资源版本配置:"+ versionPath);
                //加载不到到版本文件，直接退出从远程更新
                async.Finished();
                yield break;
            }

            version = JsonObject.Deserialize<AssetVersion>(request.downloadHandler.text);
            request.Dispose();
            fileItems = new Queue<FileItem>();
            fileEntities = new List<AssetFileEntity>();
            foreach (var v in version.versionMap)
            {
                string moduleName = v.Key;
                string fileListPath = rootPath + moduleName + "/"+ fileName;
                request = UnityWebRequest.Get(fileListPath);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    //加载到资源文件，直接退出从远程更新
                    async.Finished();
                    Debug.LogError("读取不到本地资源文件:" + fileListPath);
                    yield break;
                }
                AssetFileEntity entity = JsonObject.Deserialize<AssetFileEntity>(request.downloadHandler.text);
                fileEntities.Add(entity);
                LoadReleaseItems(entity);
            }
            StartRelease();

        }

        private void LoadReleaseItems(AssetFileEntity entity)
        {
            foreach (var v in entity.files)
            {
                string path = rootPath + entity.moduleName + "/" + v.name;
                FileItem item = new FileItem
                {
                    url = path,
                    moduleName = entity.moduleName,
                    name = v.name
                };
                fileItems.Enqueue(item);
            }
        }

        //开始释放资源
        private void StartRelease() 
        {
            releaseCount = 0;
            releaseFileCount = 0;
            releaseTotalCount = fileItems.Count;
            isError = false;
            for (int i = 0; i < maxReleaseCount; i++)
            {
                CorManager.Instance.StartCoroutine(ReleaseToDesk());
            }
        }

        //释放资源
        private IEnumerator ReleaseToDesk()
        {
            while (fileItems.Count > 0&&!isError)
            {
                FileItem item = fileItems.Dequeue();
                item.downloadCount++;
                UnityWebRequest request = UnityWebRequest.Get(item.url);
                yield return request.SendWebRequest();
                if (isError)
                    break;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string localPath = AppConst.GetModulePath(item.moduleName) + item.name;
                    string dirName = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                    File.WriteAllBytes(localPath, request.downloadHandler.data);//写入磁盘
                    releaseFileCount++;
                    SetReleaseProgress(releaseFileCount / (float)releaseTotalCount);

                }
                else
                {
                    if (item.downloadCount > connectLimitCount)
                    {
                        Debug.LogError("资源释放错误:" + item.url);
                        isError = true;
                        break;
                    }
                    else
                    {
                        //尝试重新下载
                        fileItems.Enqueue(item);
                    }
                }

                request.Dispose();
            }
            releaseCount ++;
            if(releaseCount>=maxReleaseCount)
                ReleaseAssetComplete();
        }

        //释放进度
        private void SetReleaseProgress(float v)
        {
            GlobalEvent<float>.Notify(EventName.ReleaseAssetProgress, v);
        }

        private void ReleaseAssetComplete()
        {
            if (isError)
            {
                Debug.LogError("资源释放错误");
                async.Finished();//退出从远程更新
                return;
            }
            //保存文件配置到本地
            foreach (var v in fileEntities)
            {
                SaveFileList(v);
            }

            if (!Directory.Exists(AppConst.DataPath))
                Directory.CreateDirectory(AppConst.DataPath);

            //保存版本文件
            File.WriteAllText(AppConst.DataPath + "version.txt", JsonObject.Serialize(version));

            async.Finished();//退出
        }

        //保存文件列表
        private void SaveFileList(AssetFileEntity entity)
        {
            string path = AppConst.GetModulePath(entity.moduleName) + fileName;
            File.WriteAllText(path, JsonObject.Serialize(entity));
        }

        //文件数据
        private struct FileItem
        {
            public string url;
            public string moduleName;
            public string name;
            public int downloadCount;
        }
    }
}
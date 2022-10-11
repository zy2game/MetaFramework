using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;

namespace GameFramework.Runtime.Assets
{
    //资源更新
    public class AssetUpdate : IDisposable
    {
        //下载资源错误(string:下载失败资源地址)
        public Action<string> onDownloadAssetFail;
        //通知需要更新文件
        public Action onNotifyUpdateAsset;
        //资源更新失败
        public Action onUpdateFail;
        //资源更新完成
        public Action<string[]> onUpdateFinished;
        //正在下载资源
        public Action<int, int> onDownloadingAsset;

        private const string fileConfigName = "files.txt";
        private const int connectLimitCount = 5;//链接尝试
        private const int maxUpdateCount = 4;//同时下载数量
        private WaitFinished updateAsync;
        private string[] moduleNames;//需要更新的模块        
        private AssetVersion remoteVersion;//远程版本配置
        private int checkFilesCount = 0;

        private Dictionary<string, AssetFileEntity> localFileEntity;//本地文件列表
        private Dictionary<string, AssetFileEntity> remoteFileEntity;//远程文件列表
        private Queue<FileItem> updateItems;//需要更新的文件
        private List<FileItem> delItems;//需要删除的文件 
        private List<FileItem> downloadErrorItems;//下载错误的文件
        public int downloadSize { get; private set; }//已更新文件大小
        public int totalSize { get; private set; }//文件总大小
        public int downloadCount { get; private set; }//下载了的文件的数量
        private bool downloadComplete;//下载完成


        /// <summary>
        /// 资源更新
        /// </summary>
        /// <param name="moduleNames">需要更新的模块</param>
        public AssetUpdate(string[] moduleNames)
        {
            this.moduleNames = moduleNames;
            remoteVersion = AssetVersion.remoteVersion;
            updateAsync = new WaitFinished();
        }

        public WaitFinished CheckUpdate()
        {
            //获取本地文件数据
            localFileEntity = new Dictionary<string, AssetFileEntity>();
            foreach (var moduleName in moduleNames)
            {
                var entity = LoadLocalFileEntity(moduleName);
                if (entity != null)
                    localFileEntity.Add(moduleName, entity);
            }
            checkFilesCount = 0;
            CorManager.Instance.StartCoroutine(CheckUpdateFiles());
            return updateAsync;
        }

        //检测需要更新的文件
        private IEnumerator CheckUpdateFiles()
        {
            totalSize = 0;
            remoteFileEntity = new Dictionary<string, AssetFileEntity>();
            foreach (var moduleName in moduleNames)//获取远程文件列表
            {
                //string url = AppConst.GetModuleUrl(moduleName) + remoteVersion.FindVersion(moduleName) + "/" + fileConfigName;
                string url = GetFileUrl(moduleName, remoteVersion.FindVersion(moduleName), fileConfigName);
                using UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    AssetFileEntity entity = JsonObject.Deserialize<AssetFileEntity>(request.downloadHandler.text);
                    remoteFileEntity.Add(moduleName, entity);
                }
                else
                {
                    checkFilesCount++;
                    if (checkFilesCount > connectLimitCount)
                    {
                        onDownloadAssetFail?.Invoke(url);
                        yield break;
                    }
                    CorManager.Instance.StartCoroutine(CheckUpdateFiles());
                    yield break;
                }
            }
            updateItems = new Queue<FileItem>();//需要更新的文件
            delItems = new List<FileItem>();//需要删除的文件 
            foreach (var v in remoteFileEntity)
            {
                var romoteEntity = v.Value;
                string moduleName = v.Key;
                int version = remoteVersion.FindVersion(moduleName);
                if (!localFileEntity.ContainsKey(moduleName))//本地不存在这个模块
                {
                    //所有资源都需要下载
                    foreach (var item in romoteEntity.files)
                    {
                        string url = GetFileUrl(moduleName, version, item.name);
                        FileItem updateItem = new FileItem
                        {
                            url = url,
                            moduleName = moduleName,
                            fileItem = item
                        };
                        updateItems.Enqueue(updateItem);
                        totalSize += item.size;
                    }

                    AssetFileEntity entity = new AssetFileEntity();
                    entity.version = romoteEntity.version;
                    entity.moduleName = romoteEntity.moduleName;
                    entity.files = new List<AssetFileEntity.FileItem>();
                    localFileEntity.Add(moduleName, entity);
                }
                else//对比文件MD5值
                {
                    var localEntity = localFileEntity[moduleName];
                    foreach (var item in romoteEntity.files)//获取要更新的文件
                    {
                        string localPath = AppConst.GetModulePath(moduleName) + item.name;
                        bool exists = File.Exists(localPath);

                        if (!localEntity.ContainsMd5(item) || !exists)
                        {
                            string url = GetFileUrl(moduleName, version, item.name);
                            FileItem updateItem = new FileItem
                            {
                                url = url,
                                moduleName = moduleName,
                                fileItem = item
                            };
                            updateItems.Enqueue(updateItem);
                            totalSize += item.size;
                        }
                    }

                    //获取本地需要删除的文件
                    foreach (var item in localEntity.files)
                    {
                        if (!romoteEntity.ContainsName(item))
                        {
                            FileItem delItem = new FileItem
                            {
                                moduleName = romoteEntity.moduleName,
                                fileItem = item
                            };
                            delItems.Add(delItem);
                        }
                    }

                }
            }

            //检测更新完成

            //没有需要更新的文件
            if (updateItems.Count == 0)
            {
                SaveVersion();
                onUpdateFinished?.Invoke(moduleNames);
                updateAsync.Finished();
                yield break;
            }

            onNotifyUpdateAsset?.Invoke();
        }

        private string GetFileUrl(string module, int version, string fileName)
        {
            if (module.Equals(AppConst.config.configModuleName))//是否是配置路径
                return AppConst.config.configUrl + fileName;
            return AppConst.GetModuleUrl(module) + version + "/" + fileName;
        }

        //加载本地模块文件数据
        private AssetFileEntity LoadLocalFileEntity(string moduleName)
        {
            string path = AppConst.GetModulePath(moduleName) + fileConfigName;
            if (!File.Exists(path)) return null;
            return JsonObject.Deserialize<AssetFileEntity>(File.ReadAllText(path));
        }

        //开始更新
        public void StartUpdate()
        {
            downloadSize = 0;
            downloadCount = 0;
            downloadErrorItems = new List<FileItem>();
            downloadComplete = false;
            for (int i = 0; i < maxUpdateCount; i++)
            {
                CorManager.Instance.StartCoroutine(DownloadAssets());
            }
        }

        //下载资源
        private IEnumerator DownloadAssets()
        {
            while (updateItems.Count > 0)
            {
                FileItem item = updateItems.Dequeue();
                item.downloadCount++;
                UnityWebRequest request = UnityWebRequest.Get(item.url);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)//下载成功
                {
                    downloadSize += item.fileItem.size;
                    onDownloadingAsset?.Invoke(totalSize, downloadSize);
                    string localPath = AppConst.GetModulePath(item.moduleName) + item.fileItem.name;
                    string dirName = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                    File.WriteAllBytes(localPath, request.downloadHandler.data);//写入磁盘
                    UpdateLocalFileItem(item);
                }
                else//下载错误处理
                {
                    if (item.downloadCount > connectLimitCount)
                    {
                        Debug.LogError("资源下载错误:" + item.url);
                        downloadErrorItems.Add(item);
                    }
                    else
                    {
                        //尝试重新下载
                        updateItems.Enqueue(item);
                    }
                }
                request.Dispose();
            }

            downloadCount++;
            //资源更新完成
            if (downloadCount >= maxUpdateCount)
                DownloadAssetComplete();
        }

        //更新文件信息
        private void UpdateLocalFileItem(FileItem newItem)
        {
            var entity = localFileEntity[newItem.moduleName];
            var localItem = entity.FindItem(newItem.fileItem.name);
            if (localItem == null)
                entity.files.Add(newItem.fileItem);
            else
                localItem.Copy(newItem.fileItem);
        }

        //下载完成
        private void DownloadAssetComplete()
        {
            if (downloadComplete) return;
            downloadComplete = true;
            if (downloadErrorItems.Count > 0)//有未成功下载的资源
            {
                //保存已更新的本地配置
                foreach (var v in localFileEntity)
                {
                    SaveFileList(v.Value);
                }
                onUpdateFail?.Invoke();
                return;
            }

            //保存远程的文件配置到本地
            foreach (var v in remoteFileEntity)
            {
                SaveFileList(v.Value);
            }
            SaveVersion();
            DeleteFile();
            onUpdateFinished?.Invoke(moduleNames);
            GlobalEvent<string[]>.Notify(EventName.AssetUpdateFinished, moduleNames);
            //更新完成
            updateAsync.Finished();
        }

        //删除多余的文件
        private void DeleteFile()
        {
            foreach (var v in delItems)
            {
                string path = AppConst.GetModulePath(v.moduleName) + v.fileItem.name;
                if (File.Exists(path))
                    File.Delete(path);
            }
            delItems.Clear();
        }

        //保存文件列表
        private void SaveFileList(AssetFileEntity entity)
        {
            string path = AppConst.GetModulePath(entity.moduleName) + fileConfigName;
            File.WriteAllText(path, JsonObject.Serialize(entity));
        }

        //保存版本配置
        private void SaveVersion()
        {
            string path = AppConst.DataPath + "version.txt";
            AssetVersion localVersion;
            if (File.Exists(path))
                localVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(path));
            else
            {
                localVersion = new AssetVersion();
                localVersion.packageVersion = remoteVersion.packageVersion;
                localVersion.versionMap = new Dictionary<string, int>();
            }

            foreach (var moduleName in moduleNames)
            {
                if (moduleName.Equals(AppConst.config.configModuleName)) continue;
                int version = remoteVersion.FindVersion(moduleName);
                if (localVersion.versionMap.ContainsKey(moduleName))
                    localVersion.versionMap[moduleName] = version;
                else
                    localVersion.versionMap.Add(moduleName, version);
            }

            AssetVersion.localVersion = localVersion;
            File.WriteAllText(path, JsonObject.Serialize(localVersion));
        }

        //是否有更新错误的资源
        public bool IsUpdateError
        {
            get
            {
                return downloadErrorItems != null && downloadErrorItems.Count > 0;
            }
        }

        public void Dispose()
        {
            onDownloadAssetFail = null;
            onNotifyUpdateAsset = null;
            localFileEntity.Clear();
            remoteFileEntity.Clear();
            if (delItems != null)
                delItems.Clear();
            if (downloadErrorItems != null)
                downloadErrorItems.Clear();
        }

        //要更新的文件
        private struct FileItem
        {
            public string url;
            public string moduleName;
            public int downloadCount;//尝试下载次数
            public AssetFileEntity.FileItem fileItem;
        }
    }
}
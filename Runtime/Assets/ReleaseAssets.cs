using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework.Runtime.Assets
{
    //������Դ�ͷ�
    public class ReleaseAssets
    {
        private const string fileName = "files.txt";
        private const int maxReleaseCount = 4;//ͬʱ�ͷ�����
        private const int connectLimitCount = 5;//���ӳ���
        private WaitFinished async;
        private string rootPath;
        private Queue<FileItem> fileItems;
        private List<AssetFileEntity> fileEntities;
        private AssetVersion version;
        private int releaseCount;
        private int releaseTotalCount;
        private int releaseFileCount;
        private bool isError;

        //�ͷű�����Դ
        public WaitFinished Release()
        {
            async = new WaitFinished();
            rootPath = AppConst.AppAssetPath;
            CorManager.Instance.StartCoroutine(LoadAllReleaseFile());
            return async;
        }

        //����������Ҫ�ͷŷ��ļ�
        private IEnumerator LoadAllReleaseFile()
        {
            string versionPath = rootPath + "version.txt";
            UnityWebRequest request = UnityWebRequest.Get(versionPath);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("��ȡ����������Դ�汾����:"+ versionPath);
                //���ز������汾�ļ���ֱ���˳���Զ�̸���
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
                    //���ص���Դ�ļ���ֱ���˳���Զ�̸���
                    async.Finished();
                    Debug.LogError("��ȡ����������Դ�ļ�:" + fileListPath);
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

        //��ʼ�ͷ���Դ
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

        //�ͷ���Դ
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
                    File.WriteAllBytes(localPath, request.downloadHandler.data);//д�����
                    releaseFileCount++;
                    SetReleaseProgress(releaseFileCount / (float)releaseTotalCount);

                }
                else
                {
                    if (item.downloadCount > connectLimitCount)
                    {
                        Debug.LogError("��Դ�ͷŴ���:" + item.url);
                        isError = true;
                        break;
                    }
                    else
                    {
                        //������������
                        fileItems.Enqueue(item);
                    }
                }

                request.Dispose();
            }
            releaseCount ++;
            if(releaseCount>=maxReleaseCount)
                ReleaseAssetComplete();
        }

        //�ͷŽ���
        private void SetReleaseProgress(float v)
        {
            GlobalEvent<float>.Notify(EventName.ReleaseAssetProgress, v);
        }

        private void ReleaseAssetComplete()
        {
            if (isError)
            {
                Debug.LogError("��Դ�ͷŴ���");
                async.Finished();//�˳���Զ�̸���
                return;
            }
            //�����ļ����õ�����
            foreach (var v in fileEntities)
            {
                SaveFileList(v);
            }

            if (!Directory.Exists(AppConst.DataPath))
                Directory.CreateDirectory(AppConst.DataPath);

            //����汾�ļ�
            File.WriteAllText(AppConst.DataPath + "version.txt", JsonObject.Serialize(version));

            async.Finished();//�˳�
        }

        //�����ļ��б�
        private void SaveFileList(AssetFileEntity entity)
        {
            string path = AppConst.GetModulePath(entity.moduleName) + fileName;
            File.WriteAllText(path, JsonObject.Serialize(entity));
        }

        //�ļ�����
        private struct FileItem
        {
            public string url;
            public string moduleName;
            public string name;
            public int downloadCount;
        }
    }
}
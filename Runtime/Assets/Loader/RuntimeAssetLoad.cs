using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

namespace GameFramework.Runtime.Assets
{
    public class RuntimeAssetLoad : AssetLoad
    {
        // 最大同时进行的ab创建数量
        private int MAXASSETBUNDLECREATENUM = 4;
        private string rootPath;//资源本地根路径
        private string abExtName;
        private Dictionary<string, AssetManifest> manifestMap;//资源依赖文件        
        private Dictionary<string, AssetBundleHandle> assetHandleMap;//资源缓存
        //同步加载的资源列表
        private List<LoadAssetTask> loadTaskList;
        //需要异步加载的任务列表
        private List<LoadAssetTask> loadAsyncTaskList;
        private int curLoadAsyncCount;//当前异步加载数量
        private string[] emptyArr;
        //引用计数列表
        private List<AssetHandle> refCountHandleList;
        //标记已获取过依赖的对象
        private List<AssetHandle> maskRefCountHandleDependsList;
        //加载一个资源包时所需要加载的所有资源统计
        private List<string> assetLoadCensus;

        public RuntimeAssetLoad()
        {
            rootPath = AppConst.DataPath;
            abExtName = AppConst.config.assetBundleExtName;
            manifestMap = new Dictionary<string, AssetManifest>();
            loadTaskList = new List<LoadAssetTask>();
            assetHandleMap = new Dictionary<string, AssetBundleHandle>();
            loadAsyncTaskList = new List<LoadAssetTask>();
            emptyArr = new string[0];
            refCountHandleList = new List<AssetHandle>();
            maskRefCountHandleDependsList = new List<AssetHandle>();
            assetLoadCensus = new List<string>();
            UpdateManager.Instance.RegUpdate(Update);
        }

        //同步资源加载
        public override AssetHandle Load(string path)
        {
            path = path.ToLower() + abExtName;
            AssetBundleHandle assetHandle;
            if (assetHandleMap.TryGetValue(path, out assetHandle))
                return assetHandle;
            SetLoadAssetTask(path, loadTaskList, null);
            foreach (var task in loadTaskList)
            {
                string fullPath = rootPath + task.path;
                AssetBundleHandle handle = LoadAssetBundle(fullPath, task.path);
                handle.SetDepends(task.depends);
            }
            loadTaskList.Clear();
            assetHandleMap.TryGetValue(path, out assetHandle);
            if (assetHandle == null)
            {
                Debug.LogError("加载资源错误:" + path);
                return null;
            }
            //依赖资源加载完成后加载当前ab包            
            return assetHandle;
        }

        //异步加载
        public override AssetLoadAsync LoadAsync(string path)
        {
            path = path.ToLower() + abExtName;
            AssetLoadAsync loadAsync = AssetLoadAsync.Get();
            if (assetHandleMap.TryGetValue(path, out AssetBundleHandle assetHandle))
            {
                //已经加载了这个资源包,等待到下一帧返回
                CorManager.Instance.DelayCall(this, 0, () =>
                {
                    loadAsync.Finished(assetHandle);
                });
                return loadAsync;
            }
            SetLoadAssetTask(path, loadAsyncTaskList, loadAsync);

            return loadAsync;
        }

        //直接加载ab包
        private AssetBundleHandle LoadAssetBundle(string fullPath, string abPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError("加载错误:" + fullPath);
                return null;
            }

            if (assetHandleMap.TryGetValue(abPath, out AssetBundleHandle bundleHandle))
                return bundleHandle;

            var ab = AssetBundle.LoadFromFile(fullPath);
            if (!ab)
            {
                Debug.LogError("加载错误:" + fullPath);
                return null;
            }
            bundleHandle = new AssetBundleHandle(abPath, ab);
            assetHandleMap.Add(abPath, bundleHandle);//添加到资源缓存map中
            return bundleHandle;
        }

        //获取依赖文件
        private AssetManifest GetManifest(string path)
        {
            int index = path.IndexOf("/");
            if (index == -1)
                index = path.IndexOf(@"\");
            if (index == -1)
            {
                Debug.LogError("路径错误:" + path);
                return null;
            }
            string packageName = path.Substring(0, index);
            string abName = packageName + abExtName;
            AssetManifest manifest;
            if (manifestMap.TryGetValue(abName, out manifest))
                return manifest;
            string fullPath = rootPath + packageName + "/" + abName;
            var abh = LoadAssetBundle(fullPath, abName);
            if (abh == null) return null;
            manifest = new AssetManifest(abh, packageName);
            if (manifest.manifest == null) return null;
            manifestMap.Add(abName, manifest);
            return manifest;
        }

        //设置加载任务
        private void SetLoadAssetTask(string path, List<LoadAssetTask> taskList, AssetLoadAsync loadAsync)
        {
            assetLoadCensus.Clear();

            var depends = LoadDependsTask(path, taskList, loadAsync);//添加依赖加载任务
            //如果包含当前要加载的任务放到最后去加载
            LoadAssetTask loadAssetTask = GetLoadTask(path, taskList);
            if (loadAssetTask != null)
                taskList.Remove(loadAssetTask);
            else
                loadAssetTask = LoadAssetTask.Get(path, depends);
            loadAssetTask.SetLoadAsync(loadAsync);
            taskList.Add(loadAssetTask);

            if (loadAsync != null)
            {
                //统计加载这个资源包总共需要加载多少个资源
                int needLoadAssetCount = assetLoadCensus.Count;
                if (!assetLoadCensus.Contains(path))
                    needLoadAssetCount++;
                loadAsync.needLoadAssetCount = needLoadAssetCount;
            }

        }

        //加载依赖任务列表
        private string[] LoadDependsTask(string path, List<LoadAssetTask> taskList, AssetLoadAsync mainLoadAsync)
        {
            var manifest = GetManifest(path);
            if (manifest == null) return emptyArr;
            string[] depends = manifest.GetAllDependencies(path);
            foreach (var v in depends)
            {
                if (assetHandleMap.ContainsKey(v)) continue;//已经加载了这个ab包
                var curDepends = LoadDependsTask(v, taskList, mainLoadAsync);
                var task = GetLoadTask(v, taskList);
                if (task == null)
                {
                    task = LoadAssetTask.Get(v, curDepends);
                    taskList.Add(task);
                }
                if (mainLoadAsync != null)
                {
                    if (!assetLoadCensus.Contains(v))
                        assetLoadCensus.Add(v);
                    task.SetMainAsync(mainLoadAsync);
                }
            }
            return depends;
        }

        //是否包含加载任务
        private LoadAssetTask GetLoadTask(string path, List<LoadAssetTask> taskList)
        {
            foreach (var item in taskList)
            {
                if (item.EqualsPath(path))
                    return item;
            }
            return null;
        }

        //资源加载协程
        private IEnumerator LoadAssetCor(LoadAssetTask task)
        {
            string fullPath = rootPath + task.path;
            if (!File.Exists(fullPath))
            {
                Debug.LogError("加载错误:" + fullPath);
                LoadAsyncFinished(task,null);
                yield break;
            }
            if (!assetHandleMap.TryGetValue(task.path, out AssetBundleHandle bundleHandle))
            {
                var request = AssetBundle.LoadFromFileAsync(fullPath);

                //设置加载进度,包括所有依赖资源的加载进度
                while (!request.isDone)
                {
                    yield return null;
                    task.SetLoadProgress(request.progress);
                }

                var ab = request.assetBundle;
                if (!ab)
                {
                    Debug.LogError("加载错误:" + fullPath);
                    LoadAsyncFinished(task,null);
                    yield break;
                }
                bundleHandle = new AssetBundleHandle(task.path, ab);
                bundleHandle.SetDepends(task.depends);
                assetHandleMap.Add(task.path, bundleHandle);//添加到资源缓存map中
            }
            else
            {
                //已加载过的进度直接设为1
                task.SetLoadProgress(1);
            }
          
            LoadAsyncFinished(task, bundleHandle);
        }

        private void LoadAsyncFinished(LoadAssetTask task, AssetBundleHandle bundleHandle)
        {
            var loadAsyncList = task.loadAsyncList;
            if (loadAsyncList != null)
            {
                foreach (var loadAsync in loadAsyncList)
                {
                    loadAsync.Finished(bundleHandle);
                }
            }

            task.Finished();
            curLoadAsyncCount--;
            if (curLoadAsyncCount < 0) curLoadAsyncCount = 0;
            loadAsyncTaskList.Remove(task);
        }

        private void Update()
        {
            AssetBundleAsyncLoader();
        }

        private void AssetBundleAsyncLoader()
        {
            if (loadAsyncTaskList.Count == 0)
            {
                //没有需要异步加载的ab包的时候才去检测资源释放
                //防止这种异步加载过程中释放掉需要的资源
                AssetHandleSmartManager.Instance.Update();
                return;
            }
            if (curLoadAsyncCount >= MAXASSETBUNDLECREATENUM) return;          
            LoadAssetTask task = null;
            foreach (var v in loadAsyncTaskList)
            {
                if (!v.isLoading)
                {
                    task = v;
                    break;
                }
            }
            if (task == null) return;
            task.isLoading = true;
            curLoadAsyncCount++;
            CorManager.Instance.StartCoroutine(LoadAssetCor(task));
        }

        //设置这个AssetHandle和其相关依赖的引用计数
        public override void SetRefCount(AssetHandle assetHandle, bool isAdd)
        {
            SetRefCountHandle(assetHandle);
            foreach (var handle in refCountHandleList)
            {
                if (isAdd)
                    handle.AddRefCount();
                else
                    handle.SubRefCount();
            }
            refCountHandleList.Clear();
            maskRefCountHandleDependsList.Clear();
        }

        private void SetRefCountHandle(AssetHandle assetHandle)
        {
            if (!refCountHandleList.Contains(assetHandle))
                refCountHandleList.Add(assetHandle);
            if (maskRefCountHandleDependsList.Contains(assetHandle)) return;
            maskRefCountHandleDependsList.Add(assetHandle);
            string[] depends = assetHandle.GetDepends();
            if (depends == null) return;
            foreach (var name in depends)
            {
                if (assetHandleMap.TryGetValue(name, out AssetBundleHandle handle))
                    SetRefCountHandle(handle);
            }
        }

        public override void RemoveCache(string path)
        {
            assetHandleMap.Remove(path);
        }

        //手动卸载一个资源包
        public override void UnloadByPath(string path, bool unloadAllLoadedObjects)
        {
            if (assetHandleMap.TryGetValue(path, out var handle))
                handle.Unload(unloadAllLoadedObjects);
        }

        //手动卸载对应包体的所有资源包
        public override void UnloadByPackageName(string packageName, bool unloadAllLoadedObjects)
        {
            var list = GetAssetHandleListByPackageName(packageName);
            foreach (var v in list)
            {
                v.Unload(unloadAllLoadedObjects);
            }
        }

        //获取包体的所有资源
        public override List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            packageName = packageName.ToLower();
            List<AssetHandle> handles = new List<AssetHandle>();
            foreach (var v in assetHandleMap)
            {
                if (packageName.Equals(v.Value.packageName))
                    handles.Add(v.Value);
            }

            return null;
        }

        //资源加载任务
        private class LoadAssetTask
        {
            private const int MAXPOOLCOUNT = 100;//对象池最大数量
            private static Queue<LoadAssetTask> pool = new Queue<LoadAssetTask>();

            public static LoadAssetTask Get(string path, string[] depends)
            {
                LoadAssetTask task;
                if (pool.Count == 0)
                    task = new LoadAssetTask();
                else
                    task = pool.Dequeue();
                task.path = path;
                task.depends = depends;
                task.isLoading = false;
                return task;
            }

            public string path;
            public string[] depends;
            public bool isLoading;
            public List<AssetLoadAsync> loadAsyncList;
            private List<AssetLoadAsync> mainAssetAsyncList;//主资源包
            private float lastLoadProgress;

            public bool EqualsPath(string other)
            {
                if (string.IsNullOrEmpty(other)) return false;
                return path.Equals(other);
            }

            public void SetLoadAsync(AssetLoadAsync loadAsync)
            {
                if (loadAsync == null) return;
                if (loadAsyncList == null) loadAsyncList = new List<AssetLoadAsync>();
                if (loadAsyncList.Contains(loadAsync)) return;
                loadAsyncList.Add(loadAsync);
            }

            //设置加载主资源,用于异步计算总的加载进度
            public void SetMainAsync(AssetLoadAsync mainLoadAsync)             
            {
                if (mainLoadAsync == null) return;
                if (mainAssetAsyncList == null) mainAssetAsyncList = new List<AssetLoadAsync>();
                if (mainAssetAsyncList.Contains(mainLoadAsync)) return;
                mainAssetAsyncList.Add(mainLoadAsync);
            }

            //设置当前资源加载进度
            public void SetLoadProgress(float v)
            {
                if (mainAssetAsyncList == null || mainAssetAsyncList.Count == 0) return;

                float dt = v - lastLoadProgress;
                dt = dt < 0 ? 0 : dt;
                foreach (var loadAsync in mainAssetAsyncList)
                {
                    float value = 1.0f / loadAsync.needLoadAssetCount * dt + loadAsync.progress;
                    value = value > 1 ? 1 : value;
                    loadAsync.progress =  value;
                }
            }

            public void Finished()
            {
                isLoading = false;
                path = string.Empty;
                depends = null;
                lastLoadProgress = 0;
                if (loadAsyncList != null)
                    loadAsyncList.Clear();
                if (mainAssetAsyncList != null)
                    mainAssetAsyncList.Clear();
                if (!pool.Contains(this) && pool.Count < MAXPOOLCOUNT)
                    pool.Enqueue(this);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameFramework.Runtime.Assets;

namespace GameEditor.BuildAsset
{
    public class BuildAssetBundle
    {
        private BuildAssetConfig assetConfig;
        private BuildAssetData assetData;
        private string platformName;
        private LocalCommonConfig localConfig;

        public BuildAssetBundle(BuildAssetConfig assetConfig, BuildAssetData assetData, string platformName)
        {
            this.assetConfig = assetConfig;
            this.assetData = assetData;
            this.platformName = platformName;

            string localConfigPath = AppConst.AppConfigPath;
            if (!File.Exists(localConfigPath))
            {
                Debug.LogError("找不到本地配置:" + localConfigPath);
                return;
            }

            localConfig = JsonObject.Deserialize<LocalCommonConfig>(File.ReadAllText(localConfigPath));
            Build();
        }

        private void Build()
        {
            string outpath = GetOutPath();
            if (!Directory.Exists(outpath))
                Directory.CreateDirectory(outpath);
            var bundles = GetAssetBundleBuilds(assetData);
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            builds.AddRange(bundles);
            //设置这个包体的依赖资源包
            if (assetData.dependPackage != null)
            {
                var dependPackage = BuildAssetData.GetDephDepends(assetData);
                foreach (var depend in dependPackage)
                {
                    Debug.Log("依赖包体:"+depend.moduleName);
                    var dependItems= GetAssetBundleBuilds(depend);
                    builds.AddRange(dependItems);
                }
            }
            RemoveSurplusAssets(builds, outpath);
            BuildPipeline.BuildAssetBundles(outpath, builds.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            BuildCode(outpath + assetData.moduleName + "/");
            CopyToCacheDir(bundles);
        }

        private List<AssetBundleBuild> GetAssetBundleBuilds(BuildAssetData assetData)
        {
            if (assetData == null)
            {
                Debug.LogError("选择的依赖资源为空!");
                return new List<AssetBundleBuild>();       
            }

            string root = AssetDatabase.GetAssetPath(assetData.assetRoot) + "/";
            string packageName = assetData.moduleName + "/";

            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

            foreach (var v in assetData.buildAssets)
            {
                AssetBundleBuild bundle = new AssetBundleBuild();
                bundle.assetBundleName = packageName + Path.ChangeExtension(v.path.Replace(root, ""), assetConfig.extName).ToLower();
                bundle.assetNames = new string[] { v.path };
                bundles.Add(bundle);

            }
            return bundles;
        }

        //获取打包输出路径
        private string GetOutPath()
        {
            return assetConfig.buildRootPath + BuildAssetConfig.buildTempPath + platformName + "/" + assetData.moduleName + "_temp/";
        }

        //获取缓存路径
        private string GetCachePath()
        {
            return assetConfig.buildRootPath + BuildAssetConfig.buildCachePath + platformName + "/" + assetData.moduleName + "_cache/";
        }

        private string[] GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            List<string> list = new List<string>();
            foreach (var file in files)
            {
                if (file.EndsWith(assetConfig.extName) || file.EndsWith(localConfig.buildLuaCodeExtName))
                    list.Add(file);
            }
            return list.ToArray();
        }

        //统一路径格式
        private string NormalizePath(string path)
        {
            return path.Replace(@"\", "/");
        }

        //移除原来打包的多余的文件
        private void RemoveSurplusAssets(List<AssetBundleBuild> bundles, string outpath)
        {
            string rootpath = GetOutPath();
            rootpath = new DirectoryInfo(rootpath).FullName;
            rootpath = NormalizePath(rootpath);

            List<string> curBuildAssetName = new List<string>();
            foreach (var v in bundles)
            {
                string name = NormalizePath(v.assetBundleName);
                curBuildAssetName.Add(name);
            }
            string[] dirFiles = GetFiles(outpath);
            foreach (var file in dirFiles)
            {
                FileInfo fi = new FileInfo(file);
                string path = NormalizePath(fi.FullName);
                string abName = path.Replace(rootpath, "");
                if (file.EndsWith(localConfig.buildLuaCodeExtName)) continue;
                if (!curBuildAssetName.Contains(abName))
                {
                    File.Delete(path);
                    string manifestFile = path + ".manifest";
                    if (File.Exists(manifestFile))
                        File.Delete(manifestFile);
                }
            }

        }

        //拷贝到缓存文件夹下
        private void CopyToCacheDir(List<AssetBundleBuild> bundles)
        {
            string cachepath = GetCachePath();
            string outpath = GetOutPath();

            if (!Directory.Exists(cachepath))
                Directory.CreateDirectory(cachepath);
            string[] dirs = Directory.GetDirectories(cachepath);
            List<int> dirNames = new List<int>();
            foreach (var dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                dirNames.Add(int.Parse(di.Name));
            }

            int version = 1;//当前发布资源本地版本
            if (dirNames.Count > 0)
            {
                dirNames.Sort();
                version = dirNames[dirNames.Count - 1] + 1;
            }

            string versionCachepath = cachepath + version + "/";
            if (!Directory.Exists(versionCachepath))
                Directory.CreateDirectory(versionCachepath);

            string packageName = assetData.moduleName + "/";

            foreach (var bundle in bundles)
            {
                string path = outpath + bundle.assetBundleName;
                string newpath = versionCachepath + bundle.assetBundleName.Replace(packageName, "");
                FileInfo newFi = new FileInfo(newpath);
                string dirName = newFi.Directory.FullName;
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                File.Copy(path, newpath);
            }

            //拷贝manifest文件
            string manifestPath = outpath + assetData.moduleName + "_temp";
            string newManifestPath = versionCachepath + assetData.moduleName + assetConfig.extName;

            //拷贝lua代码
            string[] luasPath = Directory.GetFiles(outpath + assetData.moduleName, "*" + localConfig.buildLuaCodeExtName, SearchOption.AllDirectories);
            foreach (var v in luasPath)
            {
                string name = Path.GetFileName(v);
                string newPath = versionCachepath + name;
                File.Copy(v, newPath);
            }


            if (!File.Exists(manifestPath))
            {
                Debug.LogError("拷贝manifest文件错误,找不到文件:" + manifestPath);
                return;
            }

            if (File.Exists(newManifestPath))
            {
                Debug.LogError("拷贝manifest文件错误,已有相同的文件:" + newManifestPath);
                return;
            }

            File.Copy(manifestPath, newManifestPath);

            //当前数量大于设置的保存数量，需删除多余的缓存版本，从小到大开始删除
            if (dirNames.Count > BuildAssetConfig.cacheCount)
            {
                int dt = dirNames.Count - BuildAssetConfig.cacheCount;
                for (int i = 0; i < dt; i++)
                {
                    int v = dirNames[i];
                    Directory.Delete(cachepath + v, true);
                }
            }

            GenerateFileList(versionCachepath, version);

            UpdateVersionFile(version);

            Debug.Log("打包完成:" + versionCachepath);
            System.Diagnostics.Process.Start("explorer.exe", versionCachepath.Replace("/", @"\"));

        }

        //生成文件列表
        private void GenerateFileList(string path, int version)
        {
            path = NormalizePath(path);
            AssetFileEntity fileEntity = new AssetFileEntity();
            fileEntity.files = new List<AssetFileEntity.FileItem>();
            string[] files = GetFiles(path);
            foreach (var file in files)
            {
                string name = NormalizePath(file).Replace(path, "");
                string md5 = Util.md5file(file);
                int size = (int)new FileInfo(file).Length;
                var item = new AssetFileEntity.FileItem();
                item.name = name;
                item.md5 = md5;
                item.size = size;
                fileEntity.files.Add(item);
            }
            fileEntity.version = version;
            fileEntity.moduleName = assetData.moduleName;
            File.WriteAllText(path + "files.txt", JsonObject.Serialize(fileEntity));
        }

        //更新版本文件
        private void UpdateVersionFile(int version)
        {
            string path = assetConfig.buildRootPath + BuildAssetConfig.buildCachePath + platformName + "/" + "version.txt";
            AssetVersion assetVersion = null;
            if (File.Exists(path))
                assetVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(path));
            else
            {
                assetVersion = new AssetVersion();
                assetVersion.versionMap = new Dictionary<string, int>();                
            }

            if (assetVersion.dependsMap == null) assetVersion.dependsMap = new Dictionary<string, string[]>();
            string[] depends;
            if (assetData.dependPackage != null)
            {
                depends = new string[assetData.dependPackage.Count];
                int index = 0;
                foreach (var d in assetData.dependPackage)
                {
                    depends[index++] = d.moduleName;
                }
            }
            else
            {
                depends = new string[0];
            }
            assetVersion.UpdateDepends(assetData.moduleName,depends);
            assetVersion.UpdateVersion(assetData.moduleName, version);
            assetVersion.packageVersion = localConfig.version;
            File.WriteAllText(path, JsonObject.Serialize(assetVersion));

        }

        //打包代码文件
        private void BuildCode(string outPath)
        {
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            //删除原有的所有lua代码
            string[] files = Directory.GetFiles(outPath, "*" + localConfig.buildLuaCodeExtName);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            if (assetData.codesPath == null) return;

            Dictionary<string, byte[]> luabytes = new Dictionary<string, byte[]>();
            //打包lua
            foreach (DefaultAsset luaAsset in assetData.codesPath)
            {
                if (luaAsset == null) continue;
                string name = luaAsset.name.ToLower();
                byte[] bts = new LuaBuildBytes(luaAsset).Build();
                if (bts == null) continue;
                luabytes.Add(name, bts);
            }

            foreach (var v in luabytes)
            {
                string filePath = outPath + v.Key + localConfig.buildLuaCodeExtName;
                File.WriteAllBytes(filePath, v.Value);
            }

        }
    }
}

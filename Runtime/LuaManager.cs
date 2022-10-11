using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using GameFramework.Runtime.Assets;
using System.IO;

public class LuaManager : Singleton<LuaManager>, IDisposable
{
    private delegate byte[] LoadDelegate(ref string luaPath);

    public LuaEnv luaEnv { get; private set; }//lua状态机
    private List<string> loadPath;//代码搜索路径
    private string[] systemPackageNames;//系统lua包名
    private LoadDelegate load;
    private Dictionary<string, LuaBytesEntity> luabytes;//lua代码包
    private bool isLoadLoacl;//是否加载本地代码
    private LuaFunction pbloader;

    public override void Dispose()
    {
        base.Dispose();
        luaEnv.Dispose();
        luaEnv = null;
        GlobalEvent<string[]>.Remove("UpdateFinished", UpdateFinished);
    }

    public void Init()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LuaLoader);
        luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPb);
        loadPath = new List<string>();
        isLoadLoacl =(AppConst.config==null)||(Application.isEditor && !AppConst.config.editorUseLuaBytes);
        if (isLoadLoacl)
        {
            load = LoadLuaFile;
            loadPath.AddRange(Resources.Load<XLuaLoadPathAsset>("XLuaLoadPathAsset").loadPath);
        }
        else
        {
            systemPackageNames = Resources.Load<XLuaLoadPathAsset>("XLuaLoadPathAsset").systemPackageNames;
            load = LoadLuaBytes;
            InitBuildLuaBytes();
        }


        //资源更新完成事件接收
        GlobalEvent<string[]>.AddEvent(EventName.AssetUpdateFinished, UpdateFinished);
    }

    public LuaTable GetTable(string key)
    {
        return luaEnv.Global.Get<LuaTable>(key);
    }

    //初始化打包的lua代码
    private void InitBuildLuaBytes()
    {
        luabytes = new Dictionary<string, LuaBytesEntity>();
        string path = AppConst.DataPath + "version.txt";
        if (!File.Exists(path)) return;
        var localVersion = JsonObject.Deserialize<AssetVersion>(File.ReadAllText(path));
        foreach (var v in localVersion.versionMap)
        {
            string moduleName = v.Key;
            string dir = AppConst.DataPath + moduleName + "/";
            loadPath.Add(dir);
        }
        //获取本地所有的代码文件路径
        LoadDirectoryLuaBytesName(AppConst.DataPath, false);
    }

    /// <summary>
    /// 加载文件夹下所有lua包
    /// </summary>
    /// <param name="dirPath"></param>
    ///<param name="isRepeatUnload">重复代码包是否卸载,包括卸载已经加载的lua代码</param>
    private void LoadDirectoryLuaBytesName(string dirPath, bool isRepeatUnload)
    {
        string[] files = Directory.GetFiles(dirPath, "*" + AppConst.config.buildLuaCodeExtName, SearchOption.AllDirectories);
        foreach (var file in files)
        {
            LuaBytesEntity entity = new LuaBytesEntity();
            entity.path = file;
            string name = Path.GetFileNameWithoutExtension(file);

            if (luabytes.ContainsKey(name))
            {
                if (isRepeatUnload)
                {
                    UnloadModule(name, true);
                    luabytes[name] = entity;
                }
                continue;
            }
            luabytes.Add(name, entity);
        }
    }

    /// <summary>
    /// 卸载对应模块的代码包
    /// </summary>
    /// <param name="moduleName">模块名</param>
    /// <param name="isForce">是否卸载lua加载了的代码</param>
    public void UnloadModule(string moduleName, bool isForce)
    {
        moduleName = moduleName.ToLower();
        LuaBytesEntity entity;
        if (!luabytes.TryGetValue(moduleName, out entity))
            return;
        if (!isForce)
        {
            entity.luaBytes = null;
            return;
        }
        //判断是否是系统代码包
        foreach (var v in systemPackageNames)
        {
            if (v.Equals(moduleName))
            {
                Debug.LogError("不能卸载标记为系统的代码包");
                return;
            }
        }

        if (entity.luaBytes == null)
            return;
        List<string> allCodesName = entity.luaBytes.GetCodesName();
        foreach (var codeName in allCodesName)
        {
            //调用进lua
        }
        entity.luaBytes = null;
    }

    private byte[] LuaLoader(ref string luaPath)
    {
        if (IsProtoFile(luaPath))
        {
            LoadPbFile(luaPath);
            return new byte[0];
        }
        return load(ref luaPath);
    }

    //加载工程内的lua代码
    private byte[] LoadLuaFile(ref string luaPath)
    {
        string path = luaPath.Replace(".", "/");
        if (!path.EndsWith(".lua"))
            path += ".lua";

        //foreach (var v in loadPath)
        //{
        //    string tempPath = "./Assets/" + v + path;
        //    if (File.Exists(tempPath))
        //    {
        //        luaPath = tempPath;
        //        return File.ReadAllBytes(luaPath);
        //    }
        //}

        byte[] bts = LoadLoaclFile(ref path);
        if (bts != null) luaPath = path;
        return bts;
    }

    //加载本地文件
    private byte[] LoadLoaclFile(ref string path)
    {
        foreach (var v in loadPath)
        {
            string tempPath = "./Assets/" + v + path;
            if (File.Exists(tempPath))
            {
                path = tempPath;
                return File.ReadAllBytes(path);
            }
        }

        return null;
    }

    //加载磁盘上的代码包
    private byte[] LoadLuaBytes(ref string luaPath)
    {
        string path = luaPath;
        if (!IsProtoFile(path))
            path = luaPath.Replace(".", "/");
        int index = path.IndexOf("/");
        if (index == -1)
            index = luaPath.IndexOf(@"/");
        if (index == -1)
        {
            return LoadSystemLuaBytes(ref path);
        }
        else
        {
            string packageName = path.Substring(0, index).ToLower();
            if (!luabytes.ContainsKey(packageName))
            {
                //找不到对于的代码包，尝试从系统代码包加载
                return LoadSystemLuaBytes(ref path);
            }
            LuaBytes luaBytes = LoadLuaPackageData(packageName);
            return luaBytes.GetLuaByte(path);
        }
    }

    //加载系统代码包
    private byte[] LoadSystemLuaBytes(ref string luaPath)
    {
        foreach (var v in systemPackageNames)
        {
            LuaBytes luaBytes = LoadLuaPackageData(v.ToLower());
            if (luaBytes == null) continue;
            byte[] bts = luaBytes.GetLuaByte(v + "/" + luaPath);
            if (bts != null) return bts;
        }

        return null;
    }

    //加载lua包数据
    private LuaBytes LoadLuaPackageData(string packageName)
    {
        LuaBytesEntity entity;
        if (!luabytes.TryGetValue(packageName, out entity))
            return null;
        LuaBytes luaBytes = entity.luaBytes;
        if (luaBytes == null)
        {
            if (!File.Exists(entity.path))
                return null;
            luaBytes = new LuaBytes(entity.path);
            entity.luaBytes = luaBytes;
        }
        return entity.luaBytes;
    }

    public object[] DoString(string content)
    {
        return luaEnv.DoString(content);
    }

    //有新资源更新完成
    private void UpdateFinished(string[] moduleNames)
    {
        if (Application.isEditor && !AppConst.config.editorUseLuaBytes) return;

        foreach (var moduleName in moduleNames)
        {
            string path = AppConst.DataPath + moduleName;
            LoadDirectoryLuaBytesName(path, true);
        }
    }

    //是否是协议文件
    private bool IsProtoFile(string path)
    {
        return path.EndsWith(".pb") || path.EndsWith("proto");
    }

    //注册pb加载函数
    public void RegPbLoader(LuaFunction func)
    {
        pbloader = func;
    }

    //加载pb文件
    private void LoadPbFile(string path)
    {
        if (pbloader == null) return;
        string reqPath = path;
        byte[] bts;
        if (isLoadLoacl)
            bts = LoadLoaclFile(ref path);
        else
            bts = LoadLuaBytes(ref path);
        string content;
        if (bts != null)
            content = System.Text.Encoding.UTF8.GetString(bts);
        else
            content = "error";
        //content= content.Replace("package pb;", "");
        pbloader.Action(content, reqPath);
    }

    private class LuaBytesEntity
    {
        public string path;
        public LuaBytes luaBytes;
    }
}

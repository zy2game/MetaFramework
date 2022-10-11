using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//本地配置
public class LocalCommonConfig
{
    [Desc("应用程序版本")]
    public int version;
    [Desc("资源后缀名")]
    public string assetBundleExtName = ".unity3d";
    [Desc("服务端地址")]
    public string remoteurl;
    [Desc("是否有本地释放发资源")]
    public bool localReleaseAsset;
    [Desc("游戏帧频")]
    public int gameFrameRate=45;
    [Desc("公共模块名")]
    public string commonModuleName = "common";
    [Desc("主模块名")]
    public string mainModuleName = "main";
    [Desc("配置模块名")]
    public string configModuleName = "config";
    [Desc("打包的lua代码后缀名")]
    public string buildLuaCodeExtName = ".luabytes";
    [Desc("解包密钥")]
    public string compressPassword = "123456";
    [Desc("编辑器下使用luabytes")]
    public bool editorUseLuaBytes = false;
    [Desc("编辑器下做资源更新")]
    public bool editorUpdateAssets = false;
    [Desc("编辑器加载assetbundle")]
    public bool editorLoadAssetBundle = false;

    //http 连接地址
    [Newtonsoft.Json.JsonIgnore]
    public string httpUrl;
    //主websocket连接地址
    [Newtonsoft.Json.JsonIgnore]
    public string websocketUrl;
    //资源服务器地址
    [Newtonsoft.Json.JsonIgnore]
    public string asseturl;
    //配置地址
    [Newtonsoft.Json.JsonIgnore]
    public string configUrl;

    //使用测试地址
    public bool useTestUrl = false;    
    public List<TestUrl> testUrls;

}

//测试地址
public class TestUrl
{
    public string desc;
    public string assetUrl;
    public string httpUrl;
    public string websocketUrl;
    public string configUrl;
    public bool isUse;
}

public class DescAttribute : Attribute  
{
    public readonly string desc;
    public readonly int type;

    public DescAttribute(string desc) 
    {
        this.desc = desc;
    }

    public DescAttribute(string desc, int type)
    {
        this.desc = desc;
        this.type = type;
    }
}
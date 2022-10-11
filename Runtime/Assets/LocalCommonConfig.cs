using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��������
public class LocalCommonConfig
{
    [Desc("Ӧ�ó���汾")]
    public int version;
    [Desc("��Դ��׺��")]
    public string assetBundleExtName = ".unity3d";
    [Desc("����˵�ַ")]
    public string remoteurl;
    [Desc("�Ƿ��б����ͷŷ���Դ")]
    public bool localReleaseAsset;
    [Desc("��Ϸ֡Ƶ")]
    public int gameFrameRate=45;
    [Desc("����ģ����")]
    public string commonModuleName = "common";
    [Desc("��ģ����")]
    public string mainModuleName = "main";
    [Desc("����ģ����")]
    public string configModuleName = "config";
    [Desc("�����lua�����׺��")]
    public string buildLuaCodeExtName = ".luabytes";
    [Desc("�����Կ")]
    public string compressPassword = "123456";
    [Desc("�༭����ʹ��luabytes")]
    public bool editorUseLuaBytes = false;
    [Desc("�༭��������Դ����")]
    public bool editorUpdateAssets = false;
    [Desc("�༭������assetbundle")]
    public bool editorLoadAssetBundle = false;

    //http ���ӵ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string httpUrl;
    //��websocket���ӵ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string websocketUrl;
    //��Դ��������ַ
    [Newtonsoft.Json.JsonIgnore]
    public string asseturl;
    //���õ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string configUrl;

    //ʹ�ò��Ե�ַ
    public bool useTestUrl = false;    
    public List<TestUrl> testUrls;

}

//���Ե�ַ
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
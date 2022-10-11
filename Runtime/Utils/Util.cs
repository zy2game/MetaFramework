using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public static class Util 
{
    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    public static bool IsNull(UnityEngine.Object obj)
    {
        if (obj == null) return true;
        if (!obj) return true;
        return false;
    }

    public static void LuaThread(XLua.LuaFunction func)
    {
        new Thread(() =>
        {
            func.Call();
        }).Start();

    }

    public static byte[] StringToByte(string content)
    {
        if (string.IsNullOrEmpty(content)) return new byte[0];
        return Encoding.UTF8.GetBytes(content);
    }

    public static UnityWebRequest PostJson(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(StringToByte(json));
        return request;
    }

    private static long lastTicks;
    public static void LogTicks(bool start=false)
    {
        long cur = DateTime.Now.Ticks;
        if (start) lastTicks = 0;
        else Debug.Log("deltaTicks:" + (cur - lastTicks) / 10000);
        lastTicks = cur;        
    }
}

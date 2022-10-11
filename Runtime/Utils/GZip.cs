using System;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.GZip;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

public class GZip
{
    /// <summary>
    /// —πÀı
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] zip(byte[] data, string password = "")
    {
        using MemoryStream ms = new();
        using ZipOutputStream outputStream = new(ms);
        outputStream.SetLevel(6);
        if (!string.IsNullOrEmpty(password))
            outputStream.Password = password;
        ZipEntry zipEntry = new("-");
        zipEntry.DateTime = DateTime.Now;
        outputStream.PutNextEntry(zipEntry);
        outputStream.Write(data, 0, data.Length);
        outputStream.CloseEntry();
        outputStream.IsStreamOwner = false; ;
        outputStream.Close();
        byte[] press = ms.ToArray();       
        ms.Close();


        return press;
    }

    public static bool zipFile(string scrFile, string outFile, string password = "")
    {
        if (!File.Exists(scrFile))
            return false;
        byte[] bts = File.ReadAllBytes(scrFile);
        bts = zip(bts, password);
        File.WriteAllBytes(outFile, bts);
        return true;
    }

    /// <summary>
    /// Ω‚—π
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] unzip(byte[] press, string password = "")
    {
        using ZipInputStream zipInputStream = new ZipInputStream(new MemoryStream(press));
        if (!string.IsNullOrEmpty(password))
            zipInputStream.Password = password;
        using MemoryStream re = new();
        byte[] data = new byte[4096];
        ZipEntry e;
        while ((e=zipInputStream.GetNextEntry()) != null)
        {
            int count;
            while ((count = zipInputStream.Read(data, 0, data.Length)) != 0)
            {
                re.Write(data, 0, count);
            }
        }
        byte[] depress = re.ToArray();
        re.Close();
        zipInputStream.Close();
        return depress;
    }

    public static bool unzipFile(string outFile, byte[] bts, string password)
    {
        try
        {
            byte[] data = unzip(bts, password);
            File.WriteAllBytes(outFile, data);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }

        return true;
    }


}

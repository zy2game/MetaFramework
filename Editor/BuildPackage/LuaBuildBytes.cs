using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Linq;

namespace GameEditor.BuildAsset
{
    public class LuaBuildBytes
    {
        private DefaultAsset dirObject;//要打包的文件夹
        public LuaBuildBytes(DefaultAsset dirObject)
        {
            this.dirObject = dirObject;
        }

        public byte[] Build()
        {
            if (dirObject == null) return null;
            string dirPath = AssetDatabase.GetAssetPath(dirObject);
            dirPath = Path.GetFullPath(dirPath).Replace(@"\", "/") + "/";
            string dirName = dirObject.name;
            string[] files = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories).Where(v=> 
            v.EndsWith(".lua")||
            v.EndsWith(".proto")
            ).ToArray();
            List<LuaByte> luaBytes = new List<LuaByte>();
            foreach (string v in files)
            {
                string filePath = Path.GetFullPath(v).Replace(@"\", "/");
                if (!File.Exists(filePath))
                {
                    Debug.LogError("打包错误，找不到lua文件:" + filePath);
                    return null;
                }
                byte[] bts = File.ReadAllBytes(filePath);

                string luaPath = dirName + "/" + filePath.Replace(dirPath, "");
                luaBytes.Add(new LuaByte(luaPath, bts));
            }

            return CreatorLuaByteData(luaBytes);
        }

        //创建luabyte数据
        private byte[] CreatorLuaByteData(List<LuaByte> luaBytes)
        {
            int len = 0;
            foreach (var v in luaBytes)
            {
                len += v.bts.Length;
            }
            byte[] bts = new byte[len];
            int offset = 0;
            foreach (var v in luaBytes)
            {
                int curLen = v.bts.Length;
                Array.Copy(v.bts, 0, bts, offset, curLen);
                offset += curLen;
            }
            byte[] zipBts = GZip.zip(bts,AppConst.config.compressPassword);

            return zipBts;
        
        }

        private class LuaByte
        {
            public byte[] bts;
            public LuaByte(string path, byte[] luaByte)
            {
                byte[] pathByte = Encoding.UTF8.GetBytes(path);
                int len = pathByte.Length + 4 + luaByte.Length + 4;
                bts = new byte[len];

                int offset = 0;
                //写入lua文件路径名
                writeInt(pathByte.Length, bts, offset);
                offset += 4;
                Array.Copy(pathByte, 0, bts, offset, pathByte.Length);

                offset += pathByte.Length;
                //写入lua字节
                writeInt(luaByte.Length, bts, offset);
                offset += 4;
                Array.Copy(luaByte, 0, bts, offset, luaByte.Length);
            }

            private void writeInt(int i, byte[] bytes, int offsetIndex)
            {
                bytes[offsetIndex] = (byte)((i >> 24) & 0xff);
                bytes[offsetIndex + 1] = (byte)((i >> 16) & 0xff);
                bytes[offsetIndex + 2] = (byte)((i >> 8) & 0xff);
                bytes[offsetIndex + 3] = (byte)(i & 0xff);
            }
        }
    }
}

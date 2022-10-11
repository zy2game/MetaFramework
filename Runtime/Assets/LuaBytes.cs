using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class LuaBytes
    {
        private Dictionary<string, byte[]> luaMap = new Dictionary<string, byte[]>();
        private string compressPassword;

        public LuaBytes(string path)
        {
            compressPassword = AppConst.config.compressPassword;
            LoadByPath(path, true, true);
        }

        private void LoadByPath(string path, bool isUnzip, bool isRepace)
        {
            byte[] bts = File.ReadAllBytes(path);
            if (isUnzip)
                LoadUnzip(bts, isRepace);
            else
                Load(bts, isRepace);
        }

        /// <summary>
        /// 加载并解压luabytes
        /// </summary>
        /// <param name="bts">lua代码字节</param>
        /// <param name="isRepace">重复是否替换</param>
        private void LoadUnzip(byte[] bts, bool isRepace)
        {
            bts = GZip.unzip(bts, compressPassword);
            Load(bts, isRepace);
        }

        /// <summary>
        /// 加载luabytes
        /// </summary>
        /// <param name="bts">lua代码字节</param>
        /// <param name="isRepace">重复是否替换</param>
        private void Load(byte[] bts, bool isRepace)
        {
            UnSerializableLuaBytes(bts, 0, isRepace);
        }

        private void UnSerializableLuaBytes(byte[] bts, int offset, bool isRepace)
        {
            if (offset >= bts.Length)
                return;
            int len = ReadInt(bts, offset);
            if (len <= 0 || len >= 1024) return;
            offset += 4;
            string name = Encoding.UTF8.GetString(bts, offset, len);

            offset += len;
            len = ReadInt(bts, offset);
            offset += 4;
            byte[] luaBts = new byte[len];
            Array.Copy(bts, offset, luaBts, 0, len);
            offset += len;
            if (luaMap.ContainsKey(name))
            {
                if (!isRepace)
                {
                    Debug.LogError("反序列化lua代码错误 重复的名字:" + name);
                    return;
                }
                else
                {
                    luaMap[name] = luaBts;
                }
            }
            else
            {
                luaMap.Add(name, luaBts);
            }
            UnSerializableLuaBytes(bts, offset, isRepace);
        }

        private int ReadInt(byte[] bts, int startIndex)
        {
            int length = (bts[startIndex + 3] & 0xff) << 0 | (bts[startIndex + 2] & 0xff) << 8 | (bts[startIndex + 1] & 0xff) << 16 | (bts[startIndex] & 0xff) << 24;
            return length;
        }

        /// <summary>
        /// 获取lua代码字节
        /// </summary>
        /// <param name="name">lua代码名字</param>
        /// <returns></returns>
        public byte[] GetLuaByte(string name)
        {
            if (!name.EndsWith(".proto"))
            {
                if (!name.EndsWith(".lua"))
                    name += ".lua";
            }

            return luaMap.ContainsKey(name) ? luaMap[name] : null;
        }

        //获取当前模块所有代码名
        public List<string> GetCodesName()
        {
            List<string> list = new List<string>();
            foreach (var v in luaMap)
            {
                string name = v.Key.Replace(".lua","");
                list.Add(name);
            }

            return list;
        }
    }
}
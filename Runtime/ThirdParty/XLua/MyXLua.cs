using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace XLua
{



    public static class MyXLua
    {
        static XLuaLoadPathAsset LuaLoadPathProfile;


        public static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!

        public static LuaEnv luaHotfixEnv = new LuaEnv();


        static Dictionary<string, string> luaDic = new Dictionary<string, string>();



        public static void Init()
        {

            //  LoadLuaAddressableAssets((msg,col)=> { D.Log(msg,col); });


            LuaLoadPathProfile = Resources.Load<XLuaLoadPathAsset>("XLuaLoadPathAsset");


            luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPb);
            luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);

            AddHotfixLoader();
            AddLuaBehaviourLoader();
        }


        public static string GetPbFile()
        {
#if GAME_DEV
        
           return Application.streamingAssetsPath + "/all.pb.bytes";
            // D.Log(ta);
           // return proto;
#else
#endif
            return string.Empty;
        }


        public static byte[] GetPB()
        {
            var pb = Resources.Load<TextAsset>("all.pb");

            Debug.Log("--------------GetPB()-----------");
            return pb.bytes;
        }

        public static string LoadProtobuf(string name)
        {
            string luaPath = Application.dataPath + "/GameScript/LuaScripts/protocol/" + name;
            string strLuaContent = File.ReadAllText(luaPath);
            return strLuaContent;
        }


        public static LuaFunction GetFunction(string funcName)
        {
            string[] arr = funcName.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 2)
            {
                LuaTable t = luaEnv.Global.Get<LuaTable>(arr[0]);
                LuaFunction func = t.Get<LuaFunction>(arr[1]);
                return func;
            }
            else
            {
                LuaFunction func = luaEnv.Global.Get<LuaFunction>(funcName);
                return func;
            }

        }

        public static LuaTable GetTable(string ss, string key)
        {
            MyXLua.DoString(ss);
            return MyXLua.luaEnv.Global.Get<LuaTable>(key);
        }


        public static void DoHotfix(string script)
        {
            luaHotfixEnv.DoString(script);
        }

        public static object[] DoString(string script)
        {
            return luaEnv.DoString(script);
        }

        private static void AddHotfixLoader()
        {
#if GAME_DEV
            luaHotfixEnv.AddLoader((ref string filename) =>
            {
                try
                {
                    string[] paths = new[] {
                            Application.dataPath+"/MyXLua/Hotfix/",
                             //Application.dataPath+"/MyXLua/Common/",
                             // Application.dataPath+"/MyXLua/Net/",
                             //  Application.dataPath+"/MyXLua/protobuf/",
                             //   Application.dataPath+"/MyXLua/View/",

                    };

                    foreach (var item in paths)
                    {
                        string file = item+ filename + ".lua";
                        if (File.Exists(file))
                        {
                            string luaScript = File.ReadAllText(file);
                            Debug.Log("MyXLua ==>> " + filename + ".lua : \n" + luaScript);
                            return System.Text.Encoding.UTF8.GetBytes(luaScript);
                        }
                       
                    }
                   
                     Debug.LogWarning(" Hotfix ==>> 找不到lua文件：" + filename + ".lua.txt");
                     return null;
                   


                }
                catch (System.Exception ex)
                {
                    return null; ;
                }
            });
#else

            luaHotfixEnv.AddLoader((ref string filename) =>
            {

                try
                {


                    string abPath = Application.persistentDataPath + "/AssetBundles/Hotfix/" + filename + ".lua";
                    if (File.Exists(abPath))
                    {
                        AssetBundle ab = AssetBundle.LoadFromFile(abPath);
                        string luaScript = ab.LoadAsset<TextAsset>(filename + ".lua").text;
                        Debug.Log(" Hotfix ==>> " + filename + ".lua : \n" + luaScript);
                        ab.Unload(true);
                        return System.Text.Encoding.UTF8.GetBytes(luaScript);
                    }
                    else
                    {

                        Debug.LogWarning(" Hotfix ==>> 找不到lua文件：" + filename + ".lua.txt");
                        return null;
                    }

                }
                catch (System.Exception e)
                {
                    // Debug.LogError(e.StackTrace);
                    return null;

                }

            });


#endif
        }


        private static void AddLuaBehaviourLoader()
        {
#if GAME_DEV
            luaEnv.AddLoader((ref string filename) =>
            {

                 try
                {

                    string[] arr = filename.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length >=2)
                        filename = arr[ arr.Length-1];

                  //  D.Log(" AddLuaBehaviourLoader :" + filename + ".lua", "#990000");
                    //foreach (var item in LuaLoadPathProfile.loadPath)
                    //{
                    //    string file = Application.dataPath +"/"+ item + filename + ".lua";
                    //    if (File.Exists(file))
                    //    {
                    //        string luaScript = File.ReadAllText(file);

                    //        return System.Text.Encoding.UTF8.GetBytes(luaScript);
                    //    }

                    //}

                    if (luaDic.ContainsKey(filename))
                        return System.Text.Encoding.UTF8.GetBytes(luaDic[filename]);


                    D.Log(" LuaBehaviour ==>> 找不到lua文件：" + filename + ".lua.txt","#990000");
                    return null;
                  
                }
                catch (System.Exception ex)
                {
                    return null; ;
                }
               
            });
#else

            luaEnv.AddLoader((ref string filename) =>
            {

                try
                {

                    string abPath = Application.persistentDataPath + "/AssetBundles/Lua/" + filename + ".lua";
                    if (File.Exists(abPath))
                    {
                        AssetBundle ab = AssetBundle.LoadFromFile(abPath);
                        string luaScript = ab.LoadAsset<TextAsset>(filename + ".lua").text;
                        Debug.Log(" Lua ==>> " + filename + ".lua : \n" + luaScript);
                        ab.Unload(true);
                        return System.Text.Encoding.UTF8.GetBytes(luaScript);
                    }
                    else
                    {
                        Debug.LogWarning(" LuaBehaviour ==>> 找不到lua 文件：" + filename + ".lua.txt");
                        return null;
                    }

                }
                catch (System.Exception e)
                {
                    // Debug.LogError(e.StackTrace);
                    return null;

                }
            });


#endif
        }

        static byte[] GetLuaScript(string path, string filename)
        {
            Debug.Log(" ======================>>GetLuaScript   " + path);


            AssetBundle ab = AssetBundle.LoadFromFile(path);

            string luaScript = ab.LoadAsset<TextAsset>(filename + ".lua").text;

            ab.Unload(true);

            return System.Text.Encoding.UTF8.GetBytes(luaScript);


        }






    }



}
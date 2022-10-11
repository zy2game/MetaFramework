using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using XLua;
using System.Linq;

namespace GameEditor.ProtcolEditor
{
    public class MessageIdMapEditor : Editor
    {
        public class ProtoField
        {
            public int number;
            public string name;
        }

        public class ProtoTypeData
        {
            public string name;
            public ProtoField[] value;
        }

        public class ProtoData
        {
            public string name;
            public string package;
            public ProtoTypeData[] message_type;
            public ProtoTypeData[] enum_type;
        }

        public class MessageIdData : IComparer<MessageIdData>
        {
            public string package;
            public string name;
            public int number;
            public string messageName;



            public int Compare(MessageIdData x, MessageIdData y)
            {
                if (x.number > y.number) return 1;
                return 0;
            }
        }

        private const string messageIdName = "MessageId";

        [MenuItem("Assets/��ϢIDӳ��", false, 0)]
        static void Create()
        {
            var obj = Selection.assetGUIDs;
            if (obj == null || obj.Length == 0)
            {
                Debug.LogError("ѡ��pb���ڵ��ļ���Ŀ¼");
                return;
            }
            string guid = obj[0];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] files = Directory.GetFiles(path, "*.proto");

            if (files.Length == 0)
            {
                Debug.LogError("û���ҵ�Э���ļ�");
                return;
            }

            List<ProtoData> protoDatas = new List<ProtoData>();
            foreach (var file in files)
            {
                string content = File.ReadAllText(file);
                string json = ParseProto(content, Path.GetFileNameWithoutExtension(file));

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("����proto�ļ�����:" + content);
                    return;
                }
                ProtoData protoData = JsonObject.Deserialize<ProtoData>(json);
                protoDatas.Add(protoData);
            }
            GeneratedMap(protoDatas, path);
        }

        static void GeneratedMap(List<ProtoData> protoDatas, string rootPath)
        {
            List<MessageIdData> msgIdDatas = new List<MessageIdData>();
            List<int> msgIdNumbers = new List<int>();
            List<string> msgIdNames = new List<string>();

            Dictionary<string, List<ProtoField>> enumMap = new Dictionary<string, List<ProtoField>>();

            foreach (var proto in protoDatas)
            {

                ProtoTypeData messageIdData = proto.enum_type.FirstOrDefault(o => messageIdName.Equals(o.name));
                if (messageIdData == null)
                {
                    Debug.LogError("δ�ҵ������MessageId:" + proto.name);
                    return;
                }

                if (string.IsNullOrEmpty(proto.package))
                {
                    Debug.LogError("����Ϊ����:" + proto.name);
                    return;
                }

                foreach (var messageIdField in messageIdData.value)
                {
                    int number = messageIdField.number;
                    if (number == 0) continue;
                    string name = messageIdField.name;

                    if (msgIdNames.Contains(name))
                    {
                        Debug.LogError("�ظ�����ϢId����:" + name);
                        return;
                    }
                    if (msgIdNumbers.Contains(number))
                    {
                        Debug.LogError("�ظ�����ϢIdֵ:" + name + "=" + number);
                        return;
                    }

                    string messageName = name.Replace("_", "");
                    var o = proto.message_type.FirstOrDefault(o => messageName.Equals(o.name.ToUpper()));
                    if (o == null)
                    {
                        Debug.LogError("�Ҳ�����ϢId��Ӧ����Ϣ��:" + messageName);
                        return;
                    }
                    msgIdNames.Add(name);
                    msgIdNumbers.Add(number);
                    MessageIdData data = new()
                    {
                        package = proto.package,
                        name = name,
                        number = number,
                        messageName = o.name
                    };
                    msgIdDatas.Add(data);
                }

                foreach (var _enumType in proto.enum_type)
                {
                    if (_enumType.name.Equals(messageIdName)) continue;
                    List<ProtoField> enumList = new List<ProtoField>();
                    enumList.AddRange(_enumType.value);
                    enumMap.Add(proto.package + "_" + _enumType.name, enumList);
                }

            }

            string msgIdContent = "\n";
            string msgMapConent = "\n";
            string msgEnumContent = "\n";

            if (msgIdDatas.Count == 0)
            {
                Debug.LogError("û���ҵ���Ϣid");
                return;
            }


            msgIdDatas = msgIdDatas.OrderBy(a => a.number).ToList();


            foreach (var v in msgIdDatas)
            {
                string msgId = "\nMsgId." + v.name + " = " + v.number;
                string map = string.Format("\nMsgIdMap[{0}] = '{1}.{2}'", v.number, v.package, v.messageName);
                msgIdContent += msgId;
                msgMapConent += map;
            }

            foreach (var v in enumMap)
            {
                string str = "pbEnum." + v.Key + "={";
                foreach (var _enum in v.Value)
                {
                    str += "\n    " + _enum.name + "='" + _enum.name + "',";
                }
                str += "\n}";
                msgEnumContent += str + "\n\n";
            }


            string content = "--���ɵĴ��벻Ҫ�ֶ�ȥ�޸�!" + msgIdContent + msgMapConent + "\n" + msgEnumContent;

            File.WriteAllText(rootPath + "/pbmapping.lua", content);

            AssetDatabase.Refresh();

            Debug.Log("�������");
        }




        static string ParseProto(string content, string name)
        {
            LuaManager.Instance.Init();
            var luaEnv = LuaManager.Instance.luaEnv;
            string luacode = "json = require 'xlua/json' function load(content,name) return json.encode(require('protoc'):parse(content,name)) end";
            luaEnv.DoString(luacode);
            var func = luaEnv.Global.Get<LuaFunction>("load");
            object[] objs = func.Call(content, name);
            LuaManager.Instance.Dispose();
            if (objs == null || objs.Length == 0)
                return string.Empty;
            return objs[0].ToString();
        }


    }
}

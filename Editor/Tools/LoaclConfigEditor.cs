using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;
using Object = UnityEngine.Object;

namespace GameEditor.Tooles
{
    public class LoaclConfigEditor : BaseEditorWindow
    {
        private class ConfigField
        {
            public enum FiledType
            {
                Object,
                String,
                Bool,
                Int,
                Float,
                Vector2,
                Vector3,
            }

            public FieldInfo field;
            public FiledType type;
            public string fieldName;
            public string desc;
            public object value;

            public string strValue;
            public bool boolValue;
            public int intValue;
            public float floatValue;
            public Vector2 v2Value;
            public Vector3 v3Value;

            public Action drawAction;
            public Action updateValueAction;
        }

        [MenuItem("Tools/本地配置", false, 0)]
        static void Open()
        {
            Open<LoaclConfigEditor>(500, 590, "本地配置");
        }

        private LocalCommonConfig localConfig;
        private List<ConfigField> configFields;
        private int fieldWidth = 300;

        protected override void Init()
        {
            string path = AppConst.AppConfigPath;
            if (!File.Exists(path))
                localConfig = new LocalCommonConfig();
            else
                localConfig = JsonObject.Deserialize<LocalCommonConfig>(File.ReadAllText(path));


            Type type = localConfig.GetType();
            var fieldsInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            configFields = new List<ConfigField>();
            foreach (var v in fieldsInfo)
            {
                var desc = v.GetCustomAttribute<DescAttribute>();
                if (desc == null) continue;
                ConfigField configField = new();
                configField.field = v;
                configField.fieldName = v.Name;
                configField.desc = desc.desc;
                object value = v.GetValue(localConfig);
                Type filedType = v.FieldType;
                SetFiledValue(configField, value, filedType);
                configField.strValue = value == null ? "" : value.ToString();
                configFields.Add(configField);
            }


        }

        private void SetFiledValue(ConfigField configField, object value, Type filedType)
        {
            GUILayoutOption width = GUILayout.Width(fieldWidth);

            configField.value = value;
            if (filedType == typeof(int))
            {
                configField.type = ConfigField.FiledType.Int;
                configField.intValue = (int)value;
                configField.drawAction = () => configField.intValue = EditorGUILayout.IntField(configField.intValue, width);
                configField.updateValueAction = () => configField.value = configField.intValue;
            }
            else if (filedType == typeof(bool))
            {
                configField.type = ConfigField.FiledType.Bool;
                configField.boolValue = (bool)value;
                configField.drawAction = () => configField.boolValue = EditorGUILayout.Toggle(configField.boolValue, width);
                configField.updateValueAction = () => configField.value = configField.boolValue;

            }
            else if (filedType == typeof(float))
            {
                configField.type = ConfigField.FiledType.Float;
                configField.floatValue = (float)value;
                configField.drawAction = () => configField.floatValue = EditorGUILayout.FloatField(configField.floatValue, width);
                configField.updateValueAction = () => configField.value = configField.floatValue;

            }
            else if (filedType == typeof(Vector2))
            {
                configField.type = ConfigField.FiledType.Vector2;
                configField.v2Value = (Vector2)value;
                configField.drawAction = () => configField.v2Value = EditorGUILayout.Vector2Field("", configField.v2Value, width);
                configField.updateValueAction = () => configField.value = configField.v2Value;

            }
            else if (filedType == typeof(Vector3))
            {
                configField.type = ConfigField.FiledType.Vector3;
                configField.v3Value = (Vector3)value;
                configField.drawAction = () => configField.v3Value = EditorGUILayout.Vector3Field("", configField.v3Value, width);
                configField.updateValueAction = () => configField.value = configField.v3Value;
            }
            else if (filedType == typeof(string))
            {
                configField.type = ConfigField.FiledType.String;
                configField.strValue = value == null ? "" : value.ToString();
                configField.drawAction = () => configField.strValue = GUILayout.TextField(configField.strValue, width);
                configField.updateValueAction = () => configField.value = configField.strValue;
            }
            else
            {
                configField.type = ConfigField.FiledType.Object;
                configField.strValue = value == null ? "" : value.ToString();
                GUILayout.TextField(configField.strValue);
            }
        }

        private bool DrawCenterButton(string name, int w, int h)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 2 - w / 2);
            bool b = GUILayout.Button(name, GUILayout.Height(h), GUILayout.Width(w));
            GUILayout.EndHorizontal();
            return b;
        }

        private void DrawLine(int hight)
        {
            GUILayout.Box("", GUILayout.Width(position.width-30), GUILayout.Height(hight));
        }

        Vector2 scroll = Vector2.zero;

        private void OnGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Space(5);
            foreach (var v in configFields)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(v.desc + ":", GUILayout.Width(180));
                v.drawAction();
                GUILayout.EndHorizontal();
                DrawLine(3);
            }

            DrawTestUrl();

            GUILayout.Space(10);
            if (DrawCenterButton("保存", 150, 30))
            {
                Save();
            }

            GUILayout.EndScrollView();
        }

        private void Save()
        {
            foreach (var v in configFields)
            {
                if (v.updateValueAction != null)
                {
                    v.updateValueAction();
                    v.field.SetValue(localConfig, v.value);
                }
            }
            string path = AppConst.AppConfigPath;
            File.WriteAllText(path, JsonObject.Serialize(localConfig));
            AssetDatabase.Refresh();
        }

        private void DrawTestUrl()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("使用测试地址:", GUILayout.Width(180));
            localConfig.useTestUrl = EditorGUILayout.Toggle(localConfig.useTestUrl);
            GUILayout.EndHorizontal();
            DrawLine(3);
            if (!localConfig.useTestUrl) return;
            if (localConfig.testUrls != null)
            {
                int index = 0;
                int delIndex = -1;
                foreach (var item in localConfig.testUrls)
                {
                    DrawTestUrlItem(item, ++index,out bool isDel);
                    if (isDel) delIndex = index - 1;
                }

                if (delIndex != -1)
                    localConfig.testUrls.RemoveAt(delIndex);
            }
            if (GUILayout.Button("添加测试地址", GUILayout.Height(20)))
            {
                if (localConfig.testUrls == null) localConfig.testUrls = new List<TestUrl>();
                TestUrl item = new TestUrl();
                localConfig.testUrls.Add(item);
            }
        }

        private void DrawTestUrlItem(TestUrl item, int index, out bool isDel)
        {
            isDel = false;
            GUILayoutOption width = GUILayout.Width(fieldWidth);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label(index + ".", GUILayout.Width(180));
            bool use = EditorGUILayout.Toggle(item.isUse, GUILayout.Width(50));
            if (GUILayout.Button("删除", GUILayout.Width(35))) isDel = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("描述:", GUILayout.Width(180));
            item.desc = GUILayout.TextField(item.desc, width);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源服务器Url:", GUILayout.Width(180));
            item.assetUrl = GUILayout.TextField(item.assetUrl, width);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("配置下载Url:", GUILayout.Width(180));
            item.configUrl = GUILayout.TextField(item.configUrl, width);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("http服务器Url:", GUILayout.Width(180));
            item.httpUrl = GUILayout.TextField(item.httpUrl, width);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label("websocket服务器Url:", GUILayout.Width(180));
            item.websocketUrl = GUILayout.TextField(item.websocketUrl, width);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
           
            DrawLine(3);

            if (use != item.isUse)
            {
                CheckUseTestUrl(index - 1);
            }
        }

        private void CheckUseTestUrl(int changeIndex)
        {
            foreach (var item in localConfig.testUrls)
            {
                if (item.isUse)
                    item.isUse = false;
            }

            localConfig.testUrls[changeIndex].isUse = true;
        }

        [MenuItem("Tools/清理本地数据", false, 1)]
        private static void ClearLocalAsset()
        {
            if (Directory.Exists(AppConst.DataPath))
                Directory.Delete(AppConst.DataPath, true);
            Debug.Log("清理完成");
        }
    }
}

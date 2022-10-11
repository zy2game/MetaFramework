using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using GameFramework.Runtime.Assets;

namespace GameEditor
{
    public class RuntimeAssetBundleView : BaseEditorWindow
    {
        [MenuItem("Tools/Other/����ʱAb����Ϣ", false, 0)]
        public static void Open()
        {
            Open<RuntimeAssetBundleView>(500, 600, "����ʱAb����Ϣ");
        
        }

       
        private RuntimeAssetLoad runtimeAssetLoad;
        private AssetHandleSmartManager assetHandleSmartManager;

        private Dictionary<string, AssetBundleHandle> assetHandleMap;//��Դ����
        private Vector2 scroll;

        protected override void Init()
        {           
            runtimeAssetLoad =GetFieldValue<AssetLoad>(ResourcesManager.Instance, "assetLoad") as RuntimeAssetLoad;
            if (runtimeAssetLoad == null)
            {
                Close();
                return;
            }
            assetHandleMap = GetFieldValue<Dictionary<string, AssetBundleHandle>>(runtimeAssetLoad, "assetHandleMap");
            assetHandleSmartManager = AssetHandleSmartManager.Instance;

        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                Close();
                return;
            }

            UpdateAllInfo();
            AssetHandleCount();
            ManifestMapCount();
            AssetHandleSmartManager_assetHandleMapCount();
            AssetHandleSmartManager_waitReleaseMapCount();


            scroll = GUILayout.BeginScrollView(scroll);
            int index = 0;
            foreach (var v in assetHandleMap)
            {
                index++;
                DrawItem(v.Value,index);
            }
            GUILayout.EndScrollView();

        }

        private void DrawItem(AssetBundleHandle handle,int index)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(index + ".", GUILayout.Width(25));
            GUILayout.Label(handle.path,GUILayout.Width(300));
            GUILayout.Label("refCount:" + handle.refCount);
            GUILayout.EndHorizontal();
        }


        private int assetHandleCount;
        private void AssetHandleCount()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("��Դ��������:",GUILayout.Width(150));
            GUILayout.Label(assetHandleCount.ToString(),GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private int manifestMapCount;
        private void ManifestMapCount()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("��Դ�����ļ�����:", GUILayout.Width(150));
            GUILayout.Label(manifestMapCount.ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private int assetHandleSmartManager_assetHandleMapCount;
        private void AssetHandleSmartManager_assetHandleMapCount()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("�Զ������������:", GUILayout.Width(150));
            GUILayout.Label(assetHandleSmartManager_assetHandleMapCount.ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private int assetHandleSmartManager_waitReleaseMapCount;
        private void AssetHandleSmartManager_waitReleaseMapCount()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("���ͷ���Դ����:", GUILayout.Width(150));
            GUILayout.Label(assetHandleSmartManager_waitReleaseMapCount.ToString(), GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private void UpdateAllInfo()
        {
            var obj = GetFieldValue<object>(runtimeAssetLoad, "assetHandleMap");
            assetHandleCount = GetPropertyValue<int>(obj, "Count");

            obj = GetFieldValue<object>(runtimeAssetLoad, "manifestMap");
            manifestMapCount = GetPropertyValue<int>(obj, "Count");

            obj = GetFieldValue<object>(assetHandleSmartManager, "assetHandleMap");
            assetHandleSmartManager_assetHandleMapCount = GetPropertyValue<int>(obj, "Count");

            obj = GetFieldValue<object>(assetHandleSmartManager, "waitReleaseMap");
            assetHandleSmartManager_waitReleaseMapCount = GetPropertyValue<int>(obj, "Count");
        }

        private T GetFieldValue<T>(object instance, string fileName)
        {
            var fi=  instance.GetType().GetField(fileName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)fi.GetValue(instance);
        }

        private T GetPropertyValue<T>(object instance, string fileName) 
        {
            var pi= instance.GetType().GetProperty(fileName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)pi.GetValue(instance);
        }
    }
}

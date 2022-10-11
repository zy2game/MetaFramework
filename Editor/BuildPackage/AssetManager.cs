using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class AssetManager : Editor
    {
        //��Դ�����ļ���·��
        public static readonly string configDirPath = "Assets/ArtistRes/assetBuildConfig/";
        public static readonly string configName = "BuildAssetConfig.asset";
        private static readonly string  assetRootPath= "Assets/ArtistRes";//��Դ��Ŀ¼

        [MenuItem("Tools/��Դ����", false, 0)]
        public static void Open()
        {
            DirectoryInfo rootDi = null;
            if (!Directory.Exists(assetRootPath))
            {
                if (EditorUtility.DisplayDialog("��ʾ", "û���ҵ���Դ��Ŀ¼,�Ƿ񴴽�?", "ȷ��", "ȡ��"))
                {
                    rootDi = Directory.CreateDirectory(assetRootPath);
                }
                else
                    return;
            }
            else
                rootDi = new DirectoryInfo(assetRootPath);

            bool isAssetProject = false;//�Ƿ�����Դ����
            if (File.Exists(configDirPath+ configName))
                isAssetProject = true;

            string configPath = configDirPath + configName;
            BuildAssetConfig assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
            if (assetConfig == null)//�����ļ�������
            {
                if (isAssetProject)//�������Դ�����򴴽������ļ�
                {
                    if (!Directory.Exists(configDirPath))
                        Directory.CreateDirectory(configDirPath);
                    var config = CreateInstance<BuildAssetConfig>();
                    AssetDatabase.CreateAsset(config, configPath);
                    AssetDatabase.Refresh();
                    assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
                }
            }

            if (isAssetProject)
                AssetBundleBuildSetting.Open(assetConfig);
            else
                AssetLinkEditor.Open();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace GameEditor.BuildAsset
{
    public class BuildAssetData : ScriptableObject
    {
        public AssetType assetType;//��Դ����
        public int version;//��Դ�汾
        public DefaultAsset assetRoot;
        public string moduleName
        {
            get
            {
                if (!assetRoot) return string.Empty;
                return assetRoot.name.ToLower();
            }
        }//����
        public string alias;//����
        public List<BuildAssetData> dependPackage;//��������
        public List<DefaultAsset> codesPath;//�����·��
        public List<BuildAssetItem> buildAssets;//��Դ�б�

        //��ȡ�����Դ����
        public static List<BuildAssetData> GetDephDepends(BuildAssetData assetData)
        {
            List<BuildAssetData> dephDepend = new List<BuildAssetData>();
            GetDephDepends(assetData, dephDepend);
            //�Ƴ���Ҫ��ѯ����������
            if (dephDepend.Contains(assetData))
                dephDepend.Remove(assetData);
            return dephDepend;
        }

        static int GetDephDepends(BuildAssetData assetData, List<BuildAssetData> list)
        {
            if (assetData.dependPackage == null) return 0;
            foreach (var v in assetData.dependPackage)
            {
                if (list.Contains(v)) continue;
                list.Add(v);
                GetDephDepends(v, list);
            }
            return list.Count;
        }
    }
}
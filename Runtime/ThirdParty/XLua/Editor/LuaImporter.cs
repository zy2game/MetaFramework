using System.IO;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, ".lua")]
public class LuaImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        //��ȡ�ļ�����
        var luaTxt = File.ReadAllText(ctx.assetPath);
        //ת��TextAsset��Unity��ʶ�����ͣ�
        var assetsText = new TextAsset(luaTxt);
        //������assetText��ӵ��������(AssetImportContext)�Ľ���С�
        ctx.AddObjectToAsset("main obj", assetsText);
        //������assetText��Ϊ�����������Ҫ����
        ctx.SetMainObject(assetsText);
    }
}
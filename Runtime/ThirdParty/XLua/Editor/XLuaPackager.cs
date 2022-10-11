using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
public class XLuaPackager : Editor
{
   
    [MenuItem("XLua / hotfix 打包 ")]
    static void PackageHotfix()
    {

        string[] guids=  AssetDatabase.FindAssets("", new[] { "Assets/MyXLua/Hotfix" });

        AssetBundleBuild[] builds = new AssetBundleBuild[guids.Length];


        for (int i=0;i<guids.Length;i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            UnityEngine. Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            builds[i].assetBundleName= obj.name;
            builds[i].assetNames = new[] { path };
        }



        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/AssetBundles/Hotfix", builds, BuildAssetBundleOptions.None, GetBuildTarget());
        AssetDatabase.Refresh();
    }
   

    [MenuItem("XLua / Lua 打包 ")]
    static void PackageLua()
    {

        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/MyXLua/Lua" });

        AssetBundleBuild[] builds = new AssetBundleBuild[guids.Length];


        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            builds[i].assetBundleName = obj.name;
            builds[i].assetNames = new[] { path };
        }

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/AssetBundles/Lua", builds, BuildAssetBundleOptions.None, GetBuildTarget());

        AssetDatabase.Refresh();
    }



    //=================================================================

    [MenuItem("XLua / 开发者模式")]
    static void ChangeEnviromentToDev()
    {
       string symbols=  PlayerSettings.GetScriptingDefineSymbolsForGroup(GetBuildTargetGroup());
        if (!symbols.Contains("GAME_DEV"))
            symbols += ";GAME_DEV";

        if (!symbols.Contains("HOTFIX_ENABLE"))
            symbols += ";HOTFIX_ENABLE";

        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(), symbols);
    }

    [MenuItem("XLua / 发布模式")]
    static void ChangeEnviromentToPublish()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetBuildTargetGroup());
        symbols= symbols .Replace( "GAME_DEV","");
        if (!symbols.Contains("HOTFIX_ENABLE"))
            symbols += ";HOTFIX_ENABLE";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(), symbols);
    }

    //==================================================

    static BuildTarget GetBuildTarget()
    {
#if UNITY_ANDROID
        return BuildTarget.Android;
#elif UNITY_IOS
        return BuildTarget.iOS;
#else
        return BuildTarget.StandaloneWindows;
#endif
    }

    static BuildTargetGroup GetBuildTargetGroup()
    {
#if UNITY_ANDROID
        return BuildTargetGroup.Android;
#elif UNITY_IOS
        return BuildTargetGroup.iOS;
#else
        return BuildTargetGroup.Standalone;
#endif
    }

}

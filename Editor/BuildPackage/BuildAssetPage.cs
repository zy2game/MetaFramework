using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEditor.BuildAsset
{
    public abstract class BuildAssetPage 
    {
        protected readonly AssetBundleBuildSetting buildSetting;
        protected readonly AssetBundleBuildSetting.Page page;
        protected readonly SelfGUIStyle selfGUIStyle;

        public BuildAssetPage(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page)
        {
            this.buildSetting = buildSetting;
            this.page = page;
            selfGUIStyle = new SelfGUIStyle();
        }

        //运行当前页面
        public abstract void Run();

        //进入
        public abstract void Enter(object param);

        //退出
        public abstract void Exit();

       
    }

}
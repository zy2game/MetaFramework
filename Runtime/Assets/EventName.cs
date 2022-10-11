using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace GameFramework.Runtime.Assets
{
    public static class EventName
    {
        //开始释放资源
        public const string StartReleaseAsset = "STARTRELEASEASSET";
        //释放资源进度(float:0-1)
        public const string ReleaseAssetProgress = "RELEASEASSETPROGRESS";
        //结束释放资源
        public const string EndReleaseAsset = "ENDRELEASEASSET";
        //没有下载到远程版本文件
        public const string NotLoadRemoteVersion = "NOTLOADREMOTEVERSION";
        //大版本更新
        public const string BigVersionUpdate = "BIGVERSIONUPDATE";
        //通知需要更新文件(AssetUpdate:需要更新的文件总大小)
        public const string NotifyUpdateAsset = "NOTIFYUPDATEASSET";
        //下载资源错误(string:下载失败资源地址)
        public const string DownloadAssetFail = "DOWNLOADASSETFAIL";
        //资源更新完成
        public const string AssetUpdateFinished = "ASSETUPDATEFINISHED";
        //资源更新失败
        public const string AssetUpdateFail = "ASSETUPDATEFAIL";
        //重新检测版本
        public const string RecheckVersion = "RECHECKVERSION";
        //进入游戏
        public const string EnterGame = "ENTERGAME";
    }
}


using UnityEngine;

[CreateAssetMenu(fileName = "XLuaLoadPathAsset", menuName = "XLua/XLuaLoadPathAsset", order = 1)]
public class XLuaLoadPathAsset : ScriptableObject
{
    public string[] loadPath;
    public string[] systemPackageNames;//系统lua包名
}

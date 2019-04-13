using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathConfig : Singleton<PathConfig> {

    //ab包的二进制文件路径
    public static string ABDataBaseBytesPath = "Assets/Scripts/Config/ABDataBase.bytes";
    //需要打ab包的路径配置表的路径
    public static string ABCONFIGASSETSPATH = "Assets/Editor/ABConfig.asset";
    //ab包的存储路径
    public static string AssetBundleTargetPath = Application.streamingAssetsPath;

    //根据不同的平台，选择不同的ab包存储路径
    public static string Android_ABTargetPath = Application.dataPath + "/../" + "AssetBundleTarget/Android/";
    public static string IOS_ABTargetPath = Application.dataPath + "/../" + "AssetBundleTarget/IOS/";
    public static string Windows_ABTargetPath = Application.dataPath + "/../" + "AssetBundleTarget/Windows/";


    public string GetABTargetPath()
    {
        string path = "";
#if UNITY_EDITOR
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            path = Android_ABTargetPath;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            path = IOS_ABTargetPath;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            path = Windows_ABTargetPath;
        }
#endif
        return path;
    }

}

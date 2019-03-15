using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Config {

    //ab包的二进制文件路径
    public static string ABDataBaseBytesPath = "Assets/Scripts/Data/ABDataBase.bytes";
    //需要打ab包的路径配置表的路径
    public static string ABCONFIGASSETSPATH = "Assets/Editor/ABConfig.asset";
    //ab包的存储路径
    public static string AssetBundleTargetPath = Application.streamingAssetsPath;
}

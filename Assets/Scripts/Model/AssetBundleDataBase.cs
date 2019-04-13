using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;


[System.Serializable] 
public class AssetBundleData {
    [XmlElement("ABDataBaseList")]
    public List<AssetBundleDataBase> abDataBaseList = new List<AssetBundleDataBase>();
}

[System.Serializable]
public class AssetBundleDataBase {
    [XmlAttribute("Path")]
    public string Path;      //ab包路径
    [XmlAttribute("Crc")]
    public uint Crc;         //标识码
    [XmlAttribute("ABName")]
    public string ABName;   //包名
    [XmlAttribute("AssetName")]
    public string AssetName;    //资源名
    [XmlElement("DependenceList")]
    public List<string> DependenceList = new List<string>();    //该依赖项表
	 
}

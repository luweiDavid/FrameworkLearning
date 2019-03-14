using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager> {
    //key：crc值， value：资源块
    public Dictionary<uint, AssetBundleDataItem> m_crcDataItemDic = new Dictionary<uint, AssetBundleDataItem>();

    public void LoadABConfigData() {
        AssetBundle abDataBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data");
        TextAsset dataTA = abDataBundle.LoadAsset<TextAsset>("ABDataBase");

        MemoryStream ms = new MemoryStream(dataTA.bytes);
        BinaryFormatter binaryF = new BinaryFormatter();
        AssetBundleData abData = (AssetBundleData)binaryF.Deserialize(ms);
        ms.Close();

        foreach (AssetBundleDataBase abDataBase in abData.abDataBaseList)
        {
            AssetBundleDataItem item = new AssetBundleDataItem();
            item.Crc = abDataBase.Crc;
            item.ABName = abDataBase.ABName;
            item.AssetName = abDataBase.AssetName;
            item.DependenceAB = abDataBase.DependenceList;

        }
    }
     

}

//资源块
public class AssetBundleDataItem {
    //crc码
    public uint Crc = 0;
    //包名
    public string ABName = string.Empty;
    //资源名
    public string AssetName = string.Empty;
    //依赖项表
    public List<string> DependenceAB = null;
    //ab包对应的ab对象
    public AssetBundle AB = null; 
}
 
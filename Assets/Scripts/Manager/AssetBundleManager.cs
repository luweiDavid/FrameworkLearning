using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;

public class AssetBundleManager : Singleton<AssetBundleManager> {
    //key：path的crc值， value：资源数据块
    public Dictionary<uint, AssetBundleDataItem> m_crcDataItemDic = new Dictionary<uint, AssetBundleDataItem>();
    //key: abName的crc值， value：单个ab包块
    public Dictionary<uint, AssetBundleItem> m_crcABItemDic = new Dictionary<uint, AssetBundleItem>();
    //AssetBundleItem的类对象池
    public ClassObjectPool<AssetBundleItem> m_abItemPool = ObjectsManager.Instance.GetClassObjectPool<AssetBundleItem>(2000);
      
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
            item.DependenceABList = abDataBase.DependenceList; 
            if (m_crcDataItemDic.ContainsKey(item.Crc))
            {
                Debug.LogError(string.Format("重复加载资源： 包名:{0}, 资源名：{1}", item.ABName, item.AssetName));
            }
            else {
                m_crcDataItemDic.Add(item.Crc, item);
            }
        }
    }

    public AssetBundleDataItem LoadABDataItem(uint pathCrc) {
        AssetBundleDataItem abDataItem = null;
        if (!m_crcDataItemDic.TryGetValue(pathCrc, out abDataItem)) {
            Debug.LogError(string.Format("没有该crc：{0} 的AssetBundleDataItem", pathCrc));
            return null;
        } 

        //依赖包加载
        if (abDataItem.DependenceABList != null) {
            for (int i = 0; i < abDataItem.DependenceABList.Count; i++)
            {
                LoadAB(abDataItem.DependenceABList[i]);
            }
        }

        abDataItem.AB = LoadAB(abDataItem.ABName);

        return abDataItem;
    }

    /// <summary>
    /// 通过资源名字加载ab包，但是加载过的ab包需要保存起来  
    /// </summary>
    /// <param name="name">资源名</param>
    /// <returns></returns>
    private AssetBundle LoadAB(string abName) {  
        uint nameCrc = Crc32.GetCRC32(abName);
        AssetBundleItem abItem = null;

        if (!m_crcABItemDic.TryGetValue(nameCrc, out abItem))
        {
            AssetBundle tempAB = null; 
            string fullPath = PathConfig.AssetBundleTargetPath + "/" + abName;
             
            tempAB = AssetBundle.LoadFromFile(fullPath);   //加载ab包
            if (tempAB == null)
            {
                Debug.LogError("没有对应名字的AssetBundle");
                return null;
            }
            abItem = m_abItemPool.Create(true);
            abItem.AB = tempAB;
            abItem.RefCount++;
            m_crcABItemDic.Add(nameCrc, abItem);
        }
        else {
            abItem.RefCount++;
        }
        
        return abItem.AB;
    }

    public void ReleaseAB(AssetBundleDataItem item) {
        if (item == null) {
            return;
        } 
       //先释放依赖项
        if (item.DependenceABList != null && item.DependenceABList.Count > 0) {
            for (int i = 0; i < item.DependenceABList.Count; i++)
            { 
                UnloadAB(item.DependenceABList[i]);
            }
        }
        UnloadAB(item.ABName);
    }
    private void UnloadAB(string name) {
        AssetBundleItem abItem = null; 
        uint nameCrc = Crc32.GetCRC32(name);
        if (m_crcABItemDic.TryGetValue(nameCrc, out abItem) && abItem != null)
        {
            abItem.RefCount--;
            if (abItem.RefCount <= 0 && abItem.AB != null)
            {
                abItem.AB.Unload(true);
                abItem.Reset();
                m_abItemPool.Recycle(abItem);
                m_crcABItemDic.Remove(nameCrc);
            } 
        }
        else {
            Debug.LogError("没有对应crc的ab包");
        }
    }

    public AssetBundleDataItem GetABDataItem(uint pathCrc)
    {
        if (m_crcDataItemDic.ContainsKey(pathCrc))
        {
            return m_crcDataItemDic[pathCrc];
        }
        return null;
    }
}

//单个ab块 
public class AssetBundleItem {
    public AssetBundle AB = null;
    public int RefCount = 0;

    public void Reset() {
        AB = null;
        RefCount = 0;
    }
} 

//资源数据块
public class AssetBundleDataItem {
    //crc码
    public uint Crc = 0;
    //包名
    public string ABName = string.Empty;
    //资源名
    public string AssetName = string.Empty;
    //依赖项表
    public List<string> DependenceABList = null;
    //ab包对应的ab对象
    public AssetBundle AB = null;
    //---------------------------------------------
    //资源生成的对象
    public UnityEngine.Object Obj = null;
    //obj的GUID
    public int ObjGuid = 0;
    //最后的使用时间
    public float LastUsedTime;
    //是否在切换场景时清除
    public bool IsClear = true;
    //引用次数
    protected int refCount = 0;  
    public int RefCount {
        get { return refCount; }
        set {
            refCount = value;
            if (refCount < 0) {
                Debug.LogError(string.Format("引用次数小于0，{0}", Obj == null ? AssetName : Obj.name));
            }
        }
    }
}
 
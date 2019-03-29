using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectsManager : Singleton<ObjectsManager> {

    //对象池缓存和实例化对象，放在GameRoot物体下 
    public Transform RecycleObjectsTr;
    public Transform SceneTr;

    protected Dictionary<uint, List<ResourceObj>> m_crcResObjDic = new Dictionary<uint, List<ResourceObj>>();

    protected ClassObjectPool<ResourceObj> m_resObjPool = ObjectsManager.Instance.GetClassObjectPool<ResourceObj>(1000);

    public void Init(Transform recycleTr, Transform sceneTr) {
        RecycleObjectsTr = recycleTr;
        SceneTr = sceneTr;
    }


    private ResourceObj GetResObjFromDic(uint crc) {
        ResourceObj resObj = null;

        List<ResourceObj> tempList = null;
        if (m_crcResObjDic.TryGetValue(crc, out tempList) && tempList.Count > 0) {
            resObj = tempList[0];
            tempList.RemoveAt(0);
            GameObject go = resObj.CloneGo;
#if UNITY_EDITOR
            if (go.name.EndsWith("(Recycle)")) {
                go.name.Replace("(Recycle)", "");
            }
#endif
        }
        return resObj;
    }


    public GameObject InstantiateGameObj(string path, bool isSetSceneTr, bool isClear) {

        ResourceObj resObj = null;
        uint pathCrc = Crc32.GetCRC32(path);
        resObj = GetResObjFromDic(pathCrc);
        if (resObj == null) {
            resObj.Crc = pathCrc;
            
            ResourcesManager.Instance.LoadResourceObj(path, ref resObj);
             
            if (resObj.ABDataItem.Obj != null) {
                resObj.CloneGo = GameObject.Instantiate(resObj.ABDataItem.Obj) as GameObject;
            } 
        }

        if (isSetSceneTr) {
            resObj.CloneGo.transform.SetParent(SceneTr);
        }
        return resObj.CloneGo;
    }




    #region  类对象池的使用
    private Dictionary<Type, object> m_typeObjectDic = new Dictionary<Type, object>();
     
    public ClassObjectPool<T> GetClassObjectPool<T>(int count) where T:class, new() {
        Type _type = typeof(T);
        object tObj = null;  
        if (!m_typeObjectDic.ContainsKey(_type))
        {
            tObj = new ClassObjectPool<T>(count);
            m_typeObjectDic.Add(_type, tObj);
        }
        else {
            m_typeObjectDic.TryGetValue(_type, out tObj); 
        }
        return tObj as ClassObjectPool<T>;
    }
    #endregion
}


/// <summary>
/// 针对实例化对象的中间类
/// </summary>
public class ResourceObj {
    public uint Crc;
    public AssetBundleDataItem ABDataItem = null;
    public GameObject CloneGo;
    public int Guid;
    public bool IsClear = true;

    public void Reset() {
        Crc = 0;
        ABDataItem = null;
        CloneGo = null;
        Guid = 0;
        IsClear = true;
    }
}


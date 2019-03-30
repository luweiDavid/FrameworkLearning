using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectsManager : Singleton<ObjectsManager> {
    //如果既不放在对象池节点，也不放在场景节点，就需要在回收对象时，手动设置隐藏
    //对象池节点
    protected Transform RecycleObjectsTr;
    //场景节点
    protected Transform SceneTr;

    //回收的（crc，对象列表）的字典
    protected Dictionary<uint, List<ResourceGameObject>> m_recycleResGoDic = new Dictionary<uint, List<ResourceGameObject>>(); 
    //实例化中间类的类对象池
    protected ClassObjectPool<ResourceGameObject> m_resGoPool;

    //所有的实例化的正在使用（还没被回收的）的对象字典(key为guid)
    protected Dictionary<int, ResourceGameObject> m_guidResGoDic = new Dictionary<int, ResourceGameObject>();

    public void Init(Transform recycleTr, Transform sceneTr) {
        RecycleObjectsTr = recycleTr;
        SceneTr = sceneTr;

        m_resGoPool = Instance.GetClassObjectPool<ResourceGameObject>(1000);
    } 


    /// <summary>
    /// 在回收池查找有没有空闲的resGo
    /// </summary> 
    private ResourceGameObject GetResGoFromRecycleDic(uint crc) {
        ResourceGameObject resGo = null; 
        List<ResourceGameObject> tempList = null;

        if (m_recycleResGoDic.TryGetValue(crc, out tempList) && tempList.Count > 0)
        { 
            resGo = tempList[0];
            tempList.RemoveAt(0);
            GameObject go = resGo.CloneGo;

            ResourcesManager.Instance.IncreaseResGoRefCount(resGo);  //取出时，增加ABDataItem的引用计数 
            //判断go的是否被引用了,还有引用的话，要重置IsRecycle
            if (!System.Object.ReferenceEquals(go, null))
            {
                resGo.IsRecycle = false;
            }
#if UNITY_EDITOR
            if (go.name.EndsWith("(Recycle)"))
            {
                go.name = go.name.Replace("(Recycle)", ""); 
            }
            if (!go.activeInHierarchy) {
                go.SetActive(true);
            }
#endif
        } 
        return resGo;
    }


    public GameObject InstantiateGameObj(string path, bool isSetSceneTr = true, bool isClear = true) {
        
        uint pathCrc = Crc32.GetCRC32(path);
        ResourceGameObject resGo = GetResGoFromRecycleDic(pathCrc);

        if (resGo == null)
        {
            resGo = m_resGoPool.Create(true);
            resGo.Crc = pathCrc;
            //加载resGo的abDataItem等
            ResourcesManager.Instance.LoadResourceGameObj(path, ref resGo);
            if (resGo.ABDataItem.Obj != null)
            {
                resGo.CloneGo = GameObject.Instantiate(resGo.ABDataItem.Obj) as GameObject;
            }

            resGo.IsClear = isClear;
            resGo.GoGuid = resGo.CloneGo.GetInstanceID();

            if (!m_guidResGoDic.ContainsKey(resGo.GoGuid))
            {
                m_guidResGoDic.Add(resGo.GoGuid, resGo);
            }
        } 
        if (isSetSceneTr) {
            resGo.CloneGo.transform.SetParent(SceneTr);
        }
        return resGo.CloneGo;
    }

    public void AsyncInstantiateGameObj(string path, AsyncLoadResGoDealFinish resGoDealFinish, AsyncLoadPriority priority, bool isSetSceneTr = false,
        object param1 = null, object param2 = null, object param3 = null) {

        uint pathCrc = Crc32.GetCRC32(path);
        ResourceGameObject resGo = GetResGoFromRecycleDic(pathCrc);
        if (resGo != null) {

            if (resGoDealFinish != null) {
                resGoDealFinish(path, resGo, param1, param2, param3);
            }

            return;
        }

        resGo = m_resGoPool.Create(true);
        resGo.Crc = pathCrc;
         

        resGo.IsSetSceneTr = isSetSceneTr;
        resGo.Priority = priority;
        resGo.ResGoDealFinish = resGoDealFinish;
        resGo.Param1 = param1;
        resGo.Param2 = param2;
        resGo.Param3 = param3;


    }

    /// <summary>
    /// 根据go回收
    /// </summary>
    /// <param name="go"></param>
    /// <param name="maxRecycleCount">最大的回收数量, -1表示不限制数量</param>
    /// <param name="isDestroy">是否销毁</param>
    /// <param name="toParent">是否设置到父节点（RecycleObjectsTr：对象池节点）</param>
    public void ReleaseGameObject(GameObject go, int maxRecycleCount = -1, bool isDestroy = false, bool toParent = false) {
        ResourceGameObject resGo = null;
        int tempId = go.GetInstanceID();
        if (!m_guidResGoDic.TryGetValue(tempId, out resGo) || resGo == null) {
            Debug.Log("此回收对象非ObjectsManager创建,找不到对应Guid的ResourceObj");
            return;
        }
        if (maxRecycleCount == 0) //不回收
        {
            m_guidResGoDic.Remove(tempId); 
            ResourcesManager.Instance.ReleaseResources(resGo, isDestroy);
            resGo.Reset();
            m_resGoPool.Recycle(resGo);
        }
        else {
            uint pathCrc = resGo.Crc; 
            if (!m_recycleResGoDic.ContainsKey(pathCrc)) { 
                m_recycleResGoDic.Add(pathCrc, new List<ResourceGameObject>());
            }

            List<ResourceGameObject> tempList = m_recycleResGoDic[pathCrc];
            if (tempList.Count < maxRecycleCount || maxRecycleCount < 0)
            {
#if UNITY_EDITOR
                go.name += "(Recycle)";
#endif  
                if (resGo.IsRecycle == false)
                {
                    //回收 
                    ResourcesManager.Instance.DecreaseResGoRefCount(resGo); 
                    resGo.IsRecycle = true; 
                    tempList.Add(resGo);
                     
                }
                if (toParent)
                {
                    go.transform.SetParent(RecycleObjectsTr);
                }
                else {
                    go.SetActive(false);
                }
            }
            else {
                m_guidResGoDic.Remove(tempId);
                ResourcesManager.Instance.ReleaseResources(resGo, isDestroy);
                resGo.Reset();
                m_resGoPool.Recycle(resGo);
            }
        }


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
public class ResourceGameObject {
    //路径crc
    public uint Crc;
    //abDataItem引用，原生资源的引用
    public AssetBundleDataItem ABDataItem = null;
    //克隆的物体
    public GameObject CloneGo;
    //guid， 便于后面的查找
    public int GoGuid;
    //是否跳场景清除，默认清除
    public bool IsClear = true;
    //是否已经回收，防止多次回收
    public bool IsRecycle = false;

    //-----------------------------

    //是否设置到场景节点
    public bool IsSetSceneTr = false;
    public AsyncLoadPriority Priority = AsyncLoadPriority.Low;
    //异步加载ResGo的回调
    public AsyncLoadResGoDealFinish ResGoDealFinish = null;
    public object Param1 = null;
    public object Param2 = null;
    public object Param3 = null;

    public void Reset() {
        Crc = 0;
        ABDataItem = null;
        CloneGo = null;
        GoGuid = 0;
        IsClear = true;
        IsRecycle = false;

        //------------------------
        IsSetSceneTr = false;
        Priority = AsyncLoadPriority.Low;
        ResGoDealFinish = null;
        Param1 = null;
        Param2 = null;
        Param3 = null;
    }
}


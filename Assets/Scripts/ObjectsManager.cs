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

    protected long CancelId = 0;
    //key：取消异步加载的id
    protected Dictionary<long, ResourceGameObject> m_cancelIdResGoDic = new Dictionary<long, ResourceGameObject>();

    //回收的（crc，对象列表）的字典
    protected Dictionary<uint, List<ResourceGameObject>> m_resGoObjectPoolDic = new Dictionary<uint, List<ResourceGameObject>>(); 
    //实例化中间类的类对象池
    protected ClassObjectPool<ResourceGameObject> m_resGoClassPool;

    //所有的实例化的正在使用（还没被回收的）的对象字典(key为guid)
    protected Dictionary<int, ResourceGameObject> m_guidResGoDic = new Dictionary<int, ResourceGameObject>();


    public void Init(Transform recycleTr, Transform sceneTr) {
        RecycleObjectsTr = recycleTr;
        SceneTr = sceneTr;

        m_resGoClassPool = Instance.GetClassObjectPool<ResourceGameObject>(1000);
    }

    protected long CreateCancelId() {
        return CancelId++;
    }
     
    /// <summary>
    /// 在回收池查找有没有空闲的resGo
    /// </summary> 
    private ResourceGameObject GetResGoFromRecycleDic(uint crc) {
        ResourceGameObject resGo = null; 
        List<ResourceGameObject> tempList = null;

        if (m_resGoObjectPoolDic.TryGetValue(crc, out tempList) && tempList.Count > 0)
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

    /// <summary>
    /// 是否正在异步加载
    /// </summary> 
    public bool IsAsyncLoading(int cancelId) {
        return m_cancelIdResGoDic[cancelId] != null;
    }

    /// <summary>
    /// 是否是对象池管理器创建的go
    /// </summary> 
    public bool IsObjectsMgrLoaded(GameObject go) {
        int guid = go.GetInstanceID();
        return m_guidResGoDic[guid] != null;
    }
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearCache() {
        List<uint> uintList = new List<uint>();

        foreach (uint crc in m_resGoObjectPoolDic.Keys) {
            List<ResourceGameObject> tList = m_resGoObjectPoolDic[crc];
            for (int i = tList.Count - 1; i >= 0; i++)
            {
                ResourceGameObject resGo = tList[i];
                if (resGo != null && resGo.IsClear) {
                    GameObject.Destroy(resGo.CloneGo);
                    tList.Remove(resGo);
                    resGo.Reset();
                    m_guidResGoDic.Remove(resGo.CloneGo.GetInstanceID());
                    m_resGoClassPool.Recycle(resGo);
                }

                if (tList.Count <= 0) {
                    uintList.Add(crc);
                }
            } 
        }

        for (int i = 0; i < uintList.Count; i++)
        {
            if (m_resGoObjectPoolDic.ContainsKey(uintList[i])) { 
                m_resGoObjectPoolDic.Remove(uintList[i]);
            }
        }

        uintList.Clear();
    }

    public void ClearCacheByCrc(uint crc) {
        List<ResourceGameObject> tList = null;
        if (!m_resGoObjectPoolDic.TryGetValue(crc, out tList) || tList == null) {
            return;
        }

        for (int i = tList.Count - 1; i >= 0; i++)
        {
            ResourceGameObject resGo = tList[i];
            if (resGo != null && resGo.IsClear) {
                tList.Remove(resGo);
                resGo.Reset();
                m_resGoClassPool.Recycle(resGo);

                var guid = resGo.GoGuid;
                if (m_guidResGoDic.ContainsKey(guid)) {
                    m_guidResGoDic.Remove(guid);
                }
            }
        }
        if (tList.Count <= 0) {
            if (m_resGoObjectPoolDic.ContainsKey(crc)) {
                m_resGoObjectPoolDic.Remove(crc);
            } 
        }
     }

    /// <summary>
    /// 取消异步加载  
    /// </summary>
    /// <param name="cancelid"></param>
    public void CancelAsyncLoad(long cancelid) {
        ResourceGameObject resGo;
        if (m_cancelIdResGoDic.TryGetValue(cancelid, out resGo)) {
            if (ResourcesManager.Instance.CancelAsyncLoad(resGo)) {
                m_cancelIdResGoDic.Remove(cancelid);
                resGo.Reset();
                m_resGoClassPool.Recycle(resGo);
            }
        }
    } 

    /// <summary>
    /// 预加载实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="count">预加载的数量</param>
    public void PreloadGameObj(string path, int count) {
        List<GameObject> tempGoList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject go = InstantiateGameObj(path);
            tempGoList.Add(go);
            go = null;
        }
        for (int i = 0; i < tempGoList.Count; i++)
        {
            ReleaseGameObject(tempGoList[i]); 
        }

        tempGoList.Clear();
    }

    /// <summary>
    /// 同步加载实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isSetSceneTr"></param>
    /// <param name="isClear"></param>
    /// <returns></returns>
    public GameObject InstantiateGameObj(string path, bool isSetSceneTr = true, bool isClear = true) {
        
        uint pathCrc = Crc32.GetCRC32(path);
        ResourceGameObject resGo = GetResGoFromRecycleDic(pathCrc);

        if (resGo == null)
        {
            resGo = m_resGoClassPool.Create(true);
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

    /// <summary>
    /// 异步加载实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dealFinish">外层调用时传入的加载完成回调</param>
    /// <param name="priority"></param>
    /// <param name="isSetSceneTr">是否设置到场景节点</param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="isClear">是否跳转场景销毁/清除</param>
    public long AsyncInstantiateGameObj(string path, AsyncLoadDealFinish dealFinish, AsyncLoadPriority priority, bool isSetSceneTr = false,
        object param1 = null, object param2 = null, object param3 = null, bool isClear = true) {

        uint pathCrc = Crc32.GetCRC32(path);
        ResourceGameObject resGo = GetResGoFromRecycleDic(pathCrc);
        if (resGo != null) {
            if (isSetSceneTr) {
                resGo.CloneGo.transform.SetParent(SceneTr);
            }

            if (dealFinish != null) {
                dealFinish(path, resGo.CloneGo, param1, param2, param3);
            }

            return resGo.CancelId;
        } 

        resGo = m_resGoClassPool.Create(true);
        resGo.Crc = pathCrc;
        resGo.IsClear = isClear;
        resGo.IsSetSceneTr = isSetSceneTr;
        resGo.Priority = priority;
        resGo.DealFinish = dealFinish;
        resGo.Param1 = param1;
        resGo.Param2 = param2;
        resGo.Param3 = param3; 
        ResourcesManager.Instance.AsyncLoadResource(path, resGo, OnAsyncLoadResGoDealFinish);

        resGo.CancelId = CreateCancelId();
        m_cancelIdResGoDic.Add(CancelId, resGo);

        return resGo.CancelId;
    }

    /// <summary>
    /// 内层异步加载完成的回调，用来实例化对象，并且执行外层的异步加载完成的回调
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resGo"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    public void OnAsyncLoadResGoDealFinish(string path, ResourceGameObject resGo, object param1 = null, object param2 = null, object param3 = null) {
        if (resGo == null) {
            return;
        }
        if (resGo.ABDataItem == null) {
            Debug.LogError("没有加载ABDataItem，请检查ResourceManager中的协程方法");
            return;
        }
        if (resGo.ABDataItem.Obj != null) {
            resGo.CloneGo = GameObject.Instantiate(resGo.ABDataItem.Obj) as GameObject;
            resGo.GoGuid = resGo.CloneGo.GetInstanceID();

            m_cancelIdResGoDic.Remove(resGo.CancelId);

            if (!m_guidResGoDic.ContainsKey(resGo.GoGuid)) {
                m_guidResGoDic.Add(resGo.GoGuid, resGo);
            }

            if (resGo.DealFinish != null) {
                resGo.DealFinish(path, resGo.CloneGo, param1, param2, param3);
            }
        }  
    }

    /// <summary>
    /// 根据go回收
    /// </summary>
    /// <param name="go"></param>
    /// <param name="maxRecycleCount">最大的回收数量, -1表示不限制数量</param>
    /// <param name="isDestroy">是否销毁</param>
    /// <param name="toParent">是否设置到父节点（RecycleObjectsTr：对象池节点）</param>
    public void ReleaseGameObject(GameObject go, int maxRecycleCount = -1, bool isDestroy = false, bool toParent = false) {
        if (go == null) {
            Debug.Log("需要释放的go为null");
            return;
        }

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
            m_resGoClassPool.Recycle(resGo);
        }
        else {
            uint pathCrc = resGo.Crc; 
            if (!m_resGoObjectPoolDic.ContainsKey(pathCrc)) { 
                m_resGoObjectPoolDic.Add(pathCrc, new List<ResourceGameObject>());
            }

            List<ResourceGameObject> tempList = m_resGoObjectPoolDic[pathCrc];
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
                m_resGoClassPool.Recycle(resGo);
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
    //取消异步加载的id，（情景是：）
    public long CancelId = 0;
    //是否设置到场景节点
    public bool IsSetSceneTr = false;
    public AsyncLoadPriority Priority = AsyncLoadPriority.Low;
    //异步加载ResGo的回调
    public AsyncLoadDealFinish DealFinish = null;
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
        CancelId = 0;
        IsSetSceneTr = false;
        Priority = AsyncLoadPriority.Low;
        DealFinish = null;
        Param1 = null;
        Param2 = null;
        Param3 = null;
    }
}


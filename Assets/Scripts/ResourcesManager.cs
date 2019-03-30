using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResourcesManager : Singleton<ResourcesManager> { 
    //缓存的已加载的资源
    public Dictionary<uint, AssetBundleDataItem> m_crcAssetDic = new Dictionary<uint, AssetBundleDataItem>();

    //缓存没有被引用的abDataItem，当缓存次数达到最大时，清除使用时间最早的并且没有被引用的资源
    public DoubleLinkMap<AssetBundleDataItem> m_noRefDataItemDLMap = new DoubleLinkMap<AssetBundleDataItem>();
        
    /// <summary>
    /// 同步加载资源，外部直接调用，仅加载不需要实例化的资源，eg：Texture，Audio
    /// </summary>
    public T LoadResources<T>(string path) where T: UnityEngine.Object
    {
        //什么时候使用双向链表中的数据呢？（需要加进AssetBundleManager中的m_crcDataItemDic） TODO
        AssetBundleDataItem abDataItem = null;
        uint pathCrc = Crc32.GetCRC32(path);
        //先从缓存的资源中找
        abDataItem = GetCacheABDataItem(pathCrc);
        if (abDataItem != null && abDataItem.Obj != null)
        {
            return abDataItem.Obj as T;
        }

        //在缓冲中没有找到，就要去abmanager中load
        Object tempObj = null;
#if UNITY_EDITOR
        if (!GameRoot.m_UseAssetBundleInEditor) { 
            abDataItem = AssetBundleManager.Instance.GetABDataItem(pathCrc);
            if (abDataItem != null && abDataItem.Obj != null)
            {
                //因为在释放资源时，abDataItem是可以被放进双向链表中的，
                //放在双向链表的item的资源数据是没有被释放的，所以可以直接使用
                tempObj = abDataItem.Obj;
            }
            else {
                tempObj = LoadABDataItemInEditorMode<T>(path);
            } 
            //这里得到的abDataItem是null的，但是tempObj是有值的（所以在编辑器不使用ab资源时，在释放时就不会有这个ab包）
            //所以在释放资源时，要用编辑器的卸载方法
        }
#endif
        if (tempObj == null) {
            abDataItem = AssetBundleManager.Instance.LoadABDataItem(pathCrc);
            if (abDataItem != null && abDataItem.AB != null) {
                if (abDataItem.Obj != null)
                {
                    //因为在释放资源时，abDataItem是可以被放进双向链表中的，
                    //放在双向链表的item的资源数据是没有被释放的，所以可以直接使用
                    tempObj = abDataItem.Obj;
                }
                else {
                    tempObj = abDataItem.AB.LoadAsset<T>(abDataItem.AssetName);  //加载资源
                } 
            }
        }
        //从abmanager中load出来后，就缓存在字典m_crcAssetDic中
        CacheABDataItem(path,ref abDataItem, tempObj);

        return tempObj as T;
    }

    #region   提供给ObjectsManager的接口
    /// <summary>
    /// 给ObjectsManager提供的接口：加载ResourceObj的ABDataItem等
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    public bool LoadResourceGameObj(string path, ref ResourceGameObject resObj) {
        if (resObj == null || string.IsNullOrEmpty(path)) {
            return false;
        }
        uint pathCrc = Crc32.GetCRC32(path);
        AssetBundleDataItem dataItem = null;
        dataItem = GetCacheABDataItem(pathCrc);
        if (dataItem != null && dataItem.Obj != null) {
            resObj.ABDataItem = dataItem; 
            return true;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!GameRoot.m_UseAssetBundleInEditor) {
            dataItem = AssetBundleManager.Instance.GetABDataItem(pathCrc);
            if (dataItem != null && dataItem.Obj != null)
            { 
                obj = dataItem.Obj;
            }
            else
            {
                obj = LoadABDataItemInEditorMode<Object>(path);
            }
        }
#endif

        if (obj == null) {
            dataItem = AssetBundleManager.Instance.LoadABDataItem(pathCrc);
            if (dataItem != null && dataItem.AB != null)
            {
                if (dataItem.Obj != null)
                { 
                    obj = dataItem.Obj;
                }
                else
                {
                    obj = dataItem.AB.LoadAsset<Object>(dataItem.AssetName);
                }
            }
        }
        CacheABDataItem(path, ref dataItem, obj);

        resObj.ABDataItem = dataItem;
        dataItem.IsClear = resObj.IsClear;   

        return true;
    } 
     
    /// <summary>
    /// 从对象池中获取时要增加ABDataItem的引用计数
    /// </summary> 
    public int IncreaseResGoRefCount(ResourceGameObject resGo, int count = 1) { 
        return resGo != null ? IncreaseRefCount(resGo.Crc, count) : 0;
    }

    private int IncreaseRefCount(uint crc, int count = 1) { 
        AssetBundleDataItem dataItem = null;
        if (!m_crcAssetDic.TryGetValue(crc, out dataItem) || dataItem == null) { 
            return 0;
        }
        
        dataItem.RefCount += count; 

        return dataItem.RefCount;
    }

    /// <summary>
    /// 回收到对象池时减少ABDataItem的引用计数
    /// </summary> 
    public int DecreaseResGoRefCount(ResourceGameObject resGo, int count = 1)
    { 
        return resGo != null ? DecreaseRefCount(resGo.Crc, count) : 0;
    }

    private int DecreaseRefCount(uint crc, int count = 1) { 
        AssetBundleDataItem dataItem = null;
        if (!m_crcAssetDic.TryGetValue(crc, out dataItem) || dataItem == null) {
            return 0;
        }
        dataItem.RefCount -= count;

        return dataItem.RefCount;
    } 

    #endregion

    /// <summary>
    /// 根据resGo释放资源
    /// </summary> 
    public bool ReleaseResources(ResourceGameObject resGo, bool isDestroy = true) {
        if (resGo == null) {
            return false;
        } 
        AssetBundleDataItem dataItem = m_crcAssetDic[resGo.Crc];
        if (dataItem == null)
        {
            Debug.LogError(string.Format("不存在对应pathCrc的ABDataItem: {0}, {1}", resGo.Crc, resGo.CloneGo.name));
            return false;
        }
        //销毁对象
        GameObject.Destroy(resGo.CloneGo);

        dataItem.RefCount--;
        ReleaseABDataItem(dataItem, isDestroy);
        return true; 
    } 
     
    /// <summary>
    /// 释放资源， 根据Object清除
    /// (调用一次释放， dataItem的引用次数减1，直到引用次数小于等于0时，才决定是删除掉，还是加进m_noRefDataItemDLMap)
    /// 构建m_noRefDataItemDLMap这个双向链表的好处是：一些常用的资源在某个阶段没用被引用时，先保存起来，而不是直接释放掉
    /// 若释放掉，那再次加载时就要去磁盘中加载，这样会降低游戏性能
    /// </summary> 
    public bool ReleaseResources(Object obj, bool isDestroy = true) {
        if (obj == null) { 
            return false;
        }
        //通过Guid在m_crcAssetDic中找obj对应的dataItem
        AssetBundleDataItem dataItem = null;
        foreach (AssetBundleDataItem temp in m_crcAssetDic.Values)
        {
            if (temp.ObjGuid == obj.GetInstanceID())
            {
                dataItem = temp;
                break;
            }
        }
        if (dataItem == null) {
            Debug.LogError(string.Format("不存在obj：{0}对应的AssetBundleDataItem", obj.name));
            return false;
        }
        dataItem.RefCount--; 
        ReleaseABDataItem(dataItem, isDestroy);
        return true;
    }

    /// <summary>
    /// 根据路径清除资源
    /// </summary> 
    public bool ReleaseResources(string path, bool isDestroy = true) {
        if (string.IsNullOrEmpty(path)) {
            return false;
        }
        uint pathCrc = Crc32.GetCRC32(path);
        AssetBundleDataItem dataItem = m_crcAssetDic[pathCrc];
        if (dataItem == null)
        {
            Debug.LogError(string.Format("不存在此路径{0}的AssetBundleDataItem", path));
            return false;
        }

        dataItem.RefCount--;
        ReleaseABDataItem(dataItem, isDestroy);
        return true;
    }

    /// <summary>
    /// 释放单个dateitem
    /// </summary> 
    private void ReleaseABDataItem(AssetBundleDataItem dataItem, bool isDestroy = true) {
        if (dataItem.RefCount > 0) {
            return;
        }

        m_crcAssetDic.Remove(dataItem.Crc);      //从有计数的字典中移除 
        //if (!isDestroy) {
        //    //如果不销毁就加进m_noRefDataItemDLMap的表头
        //    m_noRefDataItemDLMap.InsertToHead(dataItem);
        //    return;
        //}

        //如果销毁，就释放dataitem 
        AssetBundleManager.Instance.ReleaseAB(dataItem);
        if (dataItem.Obj != null) {
            dataItem.Obj = null; 

#if UNITY_EDITOR
            //如果m_useAssetBundleInEditor == false时，是没有加载ab包的，但是缓存需要用到dataItem
            Resources.UnloadAsset(dataItem.Obj);
            Resources.UnloadUnusedAssets();
#endif
        }
    }

    public void WashOut() {    //TODO
        //当资源的内存占用量超过80%时，需要主动释放资源 
        //{
            //if (m_noRefDataItemDLMap.GetCount() <= 0) {
            //    break;
            //}
            //AssetBundleDataItem dataItem = m_noRefDataItemDLMap.GetTail();
            //ReleaseABDataItem(dataItem, false);
            //m_noRefDataItemDLMap.Remove(dataItem);
        //}
    }

    /// <summary>
    /// 缓存加载的资源
    /// </summary>
    private void CacheABDataItem(string path, ref AssetBundleDataItem dataItem,Object obj, int addRefCount = 1) {
        if (dataItem == null || obj == null) {
            Debug.LogError(string.Format("_{0}_ or _{1}_ is null",dataItem, obj));
            return;
        }
        //每次缓存时，判断内存占用量，如果太大，就需要清除资源 TODO(清除使用时间最早的并且没有被引用的资源)
        WashOut();

        dataItem.Obj = obj;
        dataItem.ObjGuid = obj.GetInstanceID();
        dataItem.LastUsedTime = Time.realtimeSinceStartup;
        dataItem.RefCount += addRefCount;

        m_crcAssetDic.Add(dataItem.Crc, dataItem);
    }

    /// <summary>
    /// 在缓存中找dataItem
    /// </summary>
    private AssetBundleDataItem GetCacheABDataItem(uint pathCrc, int addRefCount = 1) {
        AssetBundleDataItem dataItem = null;
        if (m_crcAssetDic.TryGetValue(pathCrc, out dataItem)) {
            if (dataItem != null) {
                //如果在缓存中找到了，就直接更新一下引用次数和使用时间就行
                dataItem.RefCount += addRefCount;
                dataItem.LastUsedTime = Time.realtimeSinceStartup;
            }
        }
        return dataItem;
    }

#if UNITY_EDITOR
    private T LoadABDataItemInEditorMode<T>(string path) where T : UnityEngine.Object { 
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif



    //----------跳转场景清除缓存，预加载--------------------------------------------------------------------------------- 
    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache() {
        List<AssetBundleDataItem> tempList = new List<AssetBundleDataItem>();
        foreach (AssetBundleDataItem dataItem in m_crcAssetDic.Values) {
            if (dataItem.IsClear) {
                tempList.Add(dataItem);
            }
        }
        for (int i = 0; i < tempList.Count; i++)
        { 
            ReleaseABDataItem(tempList[i], true);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    public void PreloadResources(string path) {
        if (string.IsNullOrEmpty(path)) {
            return;
        }
        uint pathCrc = Crc32.GetCRC32(path);
        AssetBundleDataItem dataItem = null;
        Object obj = null;
#if UNITY_EDITOR
        if (!GameRoot.m_UseAssetBundleInEditor) {
            obj = LoadABDataItemInEditorMode<Object>(path);

            dataItem = AssetBundleManager.Instance.GetABDataItem(pathCrc);
        }
#endif
        if (obj == null) {
            dataItem = AssetBundleManager.Instance.LoadABDataItem(pathCrc);

            if (dataItem != null && dataItem.AB != null) {
                obj = dataItem.AB.LoadAsset<Object>(dataItem.AssetName);
            } 
        }
        //预加载的资源就在切换场景时清除
        dataItem.IsClear = false;
        //预加载，引用计数不用加，为0
        CacheABDataItem(path, ref dataItem, obj, 0);
    }



    //-------------------------以下处理异步加载  ------------------------------------------------------- 
    private MonoBehaviour m_StartMono;  
    //异步加载的dataItem的列表数组， 根据优先级确定数组大小
    public List<AsyncLoadDataItem>[] m_asyncLoadItemListArray = new List<AsyncLoadDataItem>[(int)AsyncLoadPriority.Num];
    //正在异步加载的dataitem， （如果同时又两个以上加载相同资源的请求， 正在加载了就不用重新赋值了）
    public Dictionary<uint, AsyncLoadDataItem> m_asyncLoadingItemDic = new Dictionary<uint, AsyncLoadDataItem>();

    //数据类、回调类的类对象池
    public ClassObjectPool<AsyncLoadDataItem> m_asyncDataItemPool = ObjectsManager.Instance.GetClassObjectPool<AsyncLoadDataItem>(50);
    public ClassObjectPool<AsyncLoadCallBack> m_asyncCallBackPool = ObjectsManager.Instance.GetClassObjectPool<AsyncLoadCallBack>(100);

    //异步加载的最长时间，（微秒）
    private const long MAXASYNCLOADTIME = 200000;


    public void Init(MonoBehaviour mono) {
        m_StartMono = mono;
        for (int i = 0; i < (int)AsyncLoadPriority.Num; i++)
        {
            m_asyncLoadItemListArray[i] = new List<AsyncLoadDataItem>();
        }

        m_StartMono.StartCoroutine(AsyncLoadCoroutine());
    }

    /// <summary>
    /// 异步加载资源， 仅加载不需要实例化的资源， 音频，图片等
    /// </summary> 
    public void AsyncLoadResource(string path, AsyncLoadDealFinish dealFinish, AsyncLoadPriority priority, object param1 = null, 
        object param2 = null, object param3 = null, uint pathCrc = 0) {

        if (pathCrc == 0) {
            pathCrc = Crc32.GetCRC32(path);
        }
        //这里跟同步加载一样，缓存列表中有的话就直接取出来就行了， 然后把path，obj还有相应的参数传给回调就行了
        AssetBundleDataItem dataItem = GetCacheABDataItem(pathCrc);
        if (dataItem != null && dataItem.Obj != null)
        {
            if (dealFinish != null) {
                dealFinish(path, dataItem.Obj, param1, param2, param3);
            } 
            return;
        }
        
        //缓存列表中没有时，才进行异步加载
        AsyncLoadDataItem asyncDataItem = null;
        if (!m_asyncLoadingItemDic.TryGetValue(pathCrc, out asyncDataItem) && asyncDataItem == null)
        {
            //初始化数据类
            asyncDataItem = m_asyncDataItemPool.Create(true);
            asyncDataItem.Path = path;
            asyncDataItem.Crc = pathCrc;
            asyncDataItem.Priority = priority;
            m_asyncLoadItemListArray[(int)priority].Add(asyncDataItem);   //加到优先级列表数组中
            m_asyncLoadingItemDic.Add(pathCrc, asyncDataItem);             //加到正在加载的dic中
        }
        //不论正在异步加载的dic（m_asyncLoadingItemDic）中有没有asyncDataItem， 都要进行回调列表的追加
        AsyncLoadCallBack callBack = m_asyncCallBackPool.Create(true);
        callBack.DealFinish = dealFinish;
        callBack.Param1 = param1;
        callBack.Param2 = param2;
        callBack.Param3 = param3;
        asyncDataItem.CallBackList.Add(callBack);
    }

    public void AsyncLoadResource(string path, ResourceGameObject resGo) {

    }

    private IEnumerator AsyncLoadCoroutine() {
        long preYieldTime = System.DateTime.Now.Ticks;    //上一次返回的时间点
        List<AsyncLoadCallBack> callBackList = null;
        while (true)
        {
            bool hadYield = false;
            for (int i = 0; i < (int)AsyncLoadPriority.Num; i++)  //每次循环都是从最高优先级开始的（0）
            { 
                List<AsyncLoadDataItem> asyncDataItemList = m_asyncLoadItemListArray[i];
                if (asyncDataItemList.Count <= 0)
                {
                    continue;
                }
                AsyncLoadDataItem asyncDataItem = asyncDataItemList[0];
                asyncDataItemList.Remove(asyncDataItem);
                 
                Object obj = null;
                AssetBundleDataItem dataItem = null;
#if UNITY_EDITOR
                if (!GameRoot.m_UseAssetBundleInEditor) {
                    if (asyncDataItem.IsSprite)
                    {
                        obj = LoadABDataItemInEditorMode<Sprite>(asyncDataItem.Path);
                    }
                    else {
                        obj = LoadABDataItemInEditorMode<Object>(asyncDataItem.Path);
                    } 

                    //模拟异步加载
                    yield return new WaitForSeconds(0.2f);

                    dataItem = AssetBundleManager.Instance.GetABDataItem(asyncDataItem.Crc);
                }
#endif
                if (obj == null) {
                    dataItem = AssetBundleManager.Instance.LoadABDataItem(asyncDataItem.Crc);
                    AssetBundleRequest abRequest = null;
                    if (asyncDataItem.IsSprite)
                    {
                        abRequest = dataItem.AB.LoadAssetAsync<Sprite>(dataItem.AssetName);
                    }
                    else
                    {
                        abRequest = dataItem.AB.LoadAssetAsync(dataItem.AssetName);
                    }
                    
                    yield return abRequest;
                    if (abRequest.isDone) { 
                        obj = abRequest.asset; 
                    }
                    yield return obj;
                    preYieldTime = System.DateTime.Now.Ticks;
                }
                callBackList = asyncDataItem.CallBackList;
                
                //缓存dataitem 
                CacheABDataItem(asyncDataItem.Path, ref dataItem, obj, callBackList.Count);

                //执行回调列表
                
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsyncLoadCallBack callBack = null;
                    callBack = callBackList[j];
                    if (callBack != null) {
                        if (callBack.DealFinish != null) {
                            callBack.DealFinish(asyncDataItem.Path, obj, callBack.Param1, callBack.Param2, callBack.Param3);
                            callBack.DealFinish = null;
                        }

                        callBack.Reset();  //重置
                        m_asyncCallBackPool.Recycle(callBack);    //回收类对象
                    }
                }

                //清理,回收
                asyncDataItem.Reset();   //重置
                m_asyncLoadingItemDic.Remove(asyncDataItem.Crc);  //从正在加载dic中移除
                m_asyncDataItemPool.Recycle(asyncDataItem);    //回收类对象
                obj = null;
                callBackList.Clear();

                if (System.DateTime.Now.Ticks - preYieldTime > MAXASYNCLOADTIME) {
                    hadYield = true;
                    //等待一帧
                    yield return null;
                    preYieldTime = System.DateTime.Now.Ticks;
                }
            }
            if (!hadYield && System.DateTime.Now.Ticks - preYieldTime > MAXASYNCLOADTIME)
            {
                preYieldTime = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    } 
}

/// <summary>
/// 异步加载资源的优先级（  如果同时有角色模型和武器的异步加载请求，那么应该先加载模型，后加载武器）
/// </summary>
public enum AsyncLoadPriority {
    High = 0,
    Middile,
    Low,
    Num,
}

/// <summary>
/// 数据类（中间类）
/// </summary>
public class AsyncLoadDataItem {
    public List<AsyncLoadCallBack> CallBackList = new List<AsyncLoadCallBack>();
    public string Path;
    public uint Crc;
    public bool IsSprite = false;
    public AsyncLoadPriority Priority = AsyncLoadPriority.Low;

    public void Reset() {
        Path = "";
        Crc = 0;
        IsSprite = false;
        Priority = AsyncLoadPriority.Low;
        CallBackList.Clear();
    }

}
//异步加载完成后的回调
public delegate void AsyncLoadDealFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);

//异步加载ResGo的回调
public delegate void AsyncLoadResGoDealFinish(string path, ResourceGameObject resGo, object param1 = null, object param2 = null, object param3 = null);

/// <summary>
/// 异步加载的回调类
/// </summary>
public class AsyncLoadCallBack {
    //加载完成的回调
    public AsyncLoadDealFinish DealFinish = null;
    //回调参数
    public object Param1 = null;
    public object Param2 = null;
    public object Param3 = null;

    public void Reset() {
        DealFinish = null;
        Param1 = null;
        Param2 = null;
        Param3 = null;
    }
}




















/// <summary>
/// 双向链表顶层封装
/// </summary>
public class DoubleLinkMap<T> where T : class, new()
{
    DoubleLinkList<T> m_doubleLinkList = new DoubleLinkList<T>();
    Dictionary<T, DoubleLinkNode<T>> m_tNodeMap = new Dictionary<T, DoubleLinkNode<T>>();

    ~DoubleLinkMap() {
        Clear();
    }

    void Clear() {
        while (m_doubleLinkList.Tail.cur != null) {
            Remove(m_doubleLinkList.Tail.cur);
        }
    }

    public void InsertToHead(T t) {
        DoubleLinkNode<T> node = null;
        if (!m_tNodeMap.TryGetValue(t, out node)) {
            m_doubleLinkList.AddToHead(t);
            m_tNodeMap.Add(t, m_doubleLinkList.Head);
        }
    }

    public void Remove(T t) {
        DoubleLinkNode<T> node = null;
        if (m_tNodeMap.TryGetValue(t, out node)) {
            m_doubleLinkList.RemoveNode(node);
            m_tNodeMap.Remove(t);
        }
    }
    public int GetCount() {
        return m_doubleLinkList.GetCount();
    }

    public void RemoveTail() {
        if (m_doubleLinkList.Tail != null) {
            Remove(m_doubleLinkList.Tail.cur);
        }
    }

    public T GetTail() {
        if (m_doubleLinkList.Tail != null) {
            return m_doubleLinkList.Tail.cur;
        }
        return null;
    }

    public bool Find(T t) {
        DoubleLinkNode<T> node = null;
        if (!m_tNodeMap.TryGetValue(t, out node) && node == null) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 移动到表头
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool MoveToHead(T t) {
        DoubleLinkNode<T> node = null;
        if (m_tNodeMap.TryGetValue(t, out node) && node != null) {
            m_doubleLinkList.MoveToHead(node);
            return true;
        }
        return false;
    }

}

/// <summary>
/// 双向链表结构的节点
/// </summary>
public class DoubleLinkNode<T> where T : class, new()
{
    public DoubleLinkNode<T> prev;
    public DoubleLinkNode<T> next;
    public T cur;

}
/// <summary>
/// 双向链表结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkList<T> where T:class, new() {
    public DoubleLinkNode<T> Head;
    public DoubleLinkNode<T> Tail;
    private int Count;

    public ClassObjectPool<DoubleLinkNode<T>> m_nodePool = ObjectsManager.Instance.GetClassObjectPool<DoubleLinkNode<T>>(500);

    public int GetCount() {
        return Count;
    }

    public DoubleLinkNode<T> AddToHead(T t) {
        DoubleLinkNode<T> pNode = m_nodePool.Create(true);

        pNode.prev = null;
        pNode.next = null;
        pNode.cur = t;
        return AddToHead(pNode);
    }
    public DoubleLinkNode<T> AddToHead(DoubleLinkNode<T> pNode) {
        if (pNode == null) {
            return null;
        }
        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else {
            Head.prev = pNode;
            pNode.next = Head;
            Head = pNode;
        }
        Count++;
        return Head;
    }

    public DoubleLinkNode<T> AddToTail(T t) {
        DoubleLinkNode<T> pNode = m_nodePool.Create(true);
        pNode.prev = null;
        pNode.next = null;
        pNode.cur = t;
        return AddToTail(pNode);
    }
    public DoubleLinkNode<T> AddToTail(DoubleLinkNode<T> pNode) {
        if (pNode == null) {
            return null;
        }
        pNode.next = null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else {
            Tail.next = pNode;
            pNode.prev = Tail;
            Tail = pNode;
        }
        Count++;
        return Tail;
    }

    public void RemoveNode(DoubleLinkNode<T> pNode) {
        if (pNode == null)
        {
            return;
        }
        if (pNode == Head) { 
            Head = pNode.next; 
        }
        if (pNode == Tail) {
            Tail = pNode.prev;
        }
        if (pNode.prev != null) {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null) {
            pNode.next.prev = pNode.prev;
        }
        //回收
        pNode.next = pNode.prev = null;
        pNode.cur = default(T);
        m_nodePool.Recycle(pNode);
        Count--;
    }

    public void MoveToHead(DoubleLinkNode<T> pNode) {
        if (pNode == null || pNode == Head) {
            return;
        }
        if (pNode == Tail) {
            Tail = pNode.prev;
        }
        if (pNode.prev != null) {
            pNode.prev.next = pNode.next;
        }
        if (pNode.next != null) {
            pNode.next.prev = pNode.prev;
        }
        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;

        //感觉这种情况不存在
        if (Tail == null) {
            Head = Tail;
        }
    }
}

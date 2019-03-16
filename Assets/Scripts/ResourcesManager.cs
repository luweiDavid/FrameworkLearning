using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResourcesManager : Singleton<ResourcesManager> {
    public bool m_useAssetBundleInEditor = true;

    //缓存的已加载的资源
    public Dictionary<uint, AssetBundleDataItem> m_crcAssetDic = new Dictionary<uint, AssetBundleDataItem>();

    //缓存没有被引用的abDataItem，当缓存次数达到最大时，清除使用时间最早的并且没有被引用的资源
    public DoubleLinkMap<AssetBundleDataItem> m_noRefDataItemDLMap = new DoubleLinkMap<AssetBundleDataItem>();
        
    /// <summary>
    /// 同步加载资源，外部直接调用，仅加载不需要实例化的资源，eg：Texture，Audio
    /// </summary>
    public T LoadResources<T>(string path) where T: UnityEngine.Object
    { 
        //什么时候使用双向链表中的数据呢？ TODO
        AssetBundleDataItem abDataItem = null;
        uint pathCrc = Crc32.GetCRC32(path);
        //先从缓存的资源中找
        abDataItem = GetCacheABDataItem(pathCrc);
        if (abDataItem != null)
        {
            return abDataItem.Obj as T;
        }
        //在缓冲中没有找到，就要去abmanager中load
        Object tempObj = null;
#if UNITY_EDITOR
        if (!m_useAssetBundleInEditor) { 
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
                    tempObj = abDataItem.AB.LoadAsset<T>(abDataItem.AssetName);
                } 
            }
        }
        //从abmanager中load出来后，就缓存在字典m_crcAssetDic中
        CacheABDataItem(path,ref abDataItem, tempObj);

        return tempObj as T;
    }

    /// <summary>
    /// 释放资源， 
    /// (调用一次释放， dataItem的引用次数减1，直到引用次数小于等于0时，才决定是删除掉，还是加进m_noRefDataItemDLMap)
    /// 构建m_noRefDataItemDLMap这个双向链表的好处是：一些常用的资源在某个阶段没用被引用时，先保存起来，而不是直接释放掉
    /// 若释放掉，那再次加载时就要去磁盘中加载，这样会降低游戏性能
    /// </summary> 
    public bool ReleaseResources(Object obj, bool isDestroy = false) {
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
    /// 释放单个dateitem
    /// </summary> 
    private void ReleaseABDataItem(AssetBundleDataItem dataItem, bool isDestroy) {
        if (dataItem.RefCount > 0) {
            return;
        }

        m_crcAssetDic.Remove(dataItem.Crc);

        if (!isDestroy) {
            //如果不销毁就加进m_noRefDataItemDLMap的表头
            m_noRefDataItemDLMap.InsertToHead(dataItem);
            return;
        }
         
        AssetBundleManager.Instance.ReleaseAB(dataItem);
        if (dataItem.Obj != null) {
            dataItem.Obj = null;
        }
    }

    public void WashOut() {
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
            Debug.LogError(string.Format("dataItem {0} or obj {1} is null",dataItem, obj));
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

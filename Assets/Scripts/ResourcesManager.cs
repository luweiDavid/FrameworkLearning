using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager<T> : Singleton<ResourcesManager<T>> {

    //public T LoadResources() {

    //}







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

    public DoubleLinkNode<T> GetTail() {
        if (m_doubleLinkList.Tail != null) {
            return m_doubleLinkList.Tail;
        }
        return null;
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

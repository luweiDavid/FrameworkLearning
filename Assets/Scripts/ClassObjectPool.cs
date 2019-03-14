using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T : class, new() {
    private Stack<T> m_pool = new Stack<T>();
    //类对象池的最大数量  <=0代表不限制数量
    private int m_maxCount = 0;
    //没有回收的数量
    private int m_noRecycleCount = 0;

    public ClassObjectPool(int maxcount) {
        this.m_maxCount = maxcount;
        for (int i = 0; i < maxcount; i++)
        {
            m_pool.Push(new T());
        }
    }

    public T Create(bool isCreateNewOne) {
        if (m_pool.Count > 0)
        {
            T tt = m_pool.Pop();
            m_noRecycleCount++;
            return tt;
        }
        else {
            if (isCreateNewOne) {
                T tt = new T();
                m_noRecycleCount++;
                return tt;
            }
        }
        return null;
    }

    public bool Recycle(T obj) {
        if (obj == null) {
            return false;
        }
        if (m_pool.Count > m_maxCount && m_maxCount > 0) {
            obj = null;
            return true;
        }
        m_pool.Push(obj);
        m_noRecycleCount--;
        return true;
    }
}

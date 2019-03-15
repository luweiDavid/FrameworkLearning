using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectsManager : Singleton<ObjectsManager> {
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
}

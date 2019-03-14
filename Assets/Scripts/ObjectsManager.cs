using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectsManager : Singleton<ObjectsManager> {
    private Dictionary<Type, object> m_typeObjectDic = new Dictionary<Type, object>();
     
    public void GetClassObjectPool<T>(int count) where T:class, new() {
        Type _type = typeof(T);
        object tObj = null;
        ClassObjectPool<T> t = new ClassObjectPool<T>(count);

        if (!m_typeObjectDic.ContainsKey(_type)) {
            m_typeObjectDic.Add(_type, tObj);
        } 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName ="ABConfig",menuName ="CreateABConfig",order = 0)]
public class ABConfig : ScriptableObject {

    [Tooltip("所有需要打ab包的Prefab根路径集合")]
    public List<string> m_AllPrefabPath = new List<string>();
    
    [Tooltip("所有需要打ab包的文件夹根路径集合")]
    public List<AllFileDirABName> m_AllFileDirAB = new List<AllFileDirABName>();
}

[Serializable]
public struct AllFileDirABName
{
    public string ABName;
    public string Path;
}
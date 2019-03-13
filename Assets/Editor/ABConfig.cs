using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName ="ABConfig",menuName ="CreateABConfig",order = 0)]
public class ABConfig : ScriptableObject {

    public List<string> m_AllPrefabPath = new List<string>();
    
    public List<AllFileDirABName> m_AllFileDirAB = new List<AllFileDirABName>();
}

[Serializable]
public struct AllFileDirABName
{
    public string ABName;
    public string Path;
}
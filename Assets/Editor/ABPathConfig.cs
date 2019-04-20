using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ABPathConfig", menuName ="CreateABConfig",order = 0)]
public class ABPathConfig : ScriptableObject {

    [Tooltip("Prefab根路径")]
    public string PrefabRootPath = "";

    public List<string> m_prefabPathList = new List<string>();
    
    [Tooltip("所有需要打ab包的文件夹根路径集合")]
    public List<PathABNameClass> m_pathABNameList = new List<PathABNameClass>();


    /// <summary>
    /// 注意不要加最后的下划线
    /// </summary>
    public ABPathConfig() {
        PrefabRootPath = "Assets/GameData/Prefabs/";

        m_prefabPathList.Add(PrefabRootPath);

        PathABNameClass c1 = new PathABNameClass
        {
            ABName = "shaders",
            Path = "Assets/GameData/Shaders",
        };
        PathABNameClass c2 = new PathABNameClass
        {
            ABName = "sounds",
            Path = "Assets/GameData/Sounds",
        };
        PathABNameClass c3 = new PathABNameClass
        {
            ABName = "textures",
            Path = "Assets/GameData/Textures",
        };
        PathABNameClass c4 = new PathABNameClass
        {
            ABName = "data",
            Path = "Assets/GameData/Config/ABDataBase.bytes",
        };
        m_pathABNameList.Add(c1);
        m_pathABNameList.Add(c2);
        m_pathABNameList.Add(c3);
        m_pathABNameList.Add(c4);
    }
}

[Serializable]
public struct PathABNameClass
{
    public string ABName;
    public string Path;
}
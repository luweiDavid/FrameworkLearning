using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigDataBase
{
    /// <summary>
    /// 初始化数据的结构，生成xml或者excel，便于填充数据
    /// </summary>
    public virtual void Construct() { }

    public virtual void Init() { }
    
}

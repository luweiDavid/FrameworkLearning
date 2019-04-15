using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : Singleton<ConfigManager>
{
    public Dictionary<string, ConfigDataBase> m_configDataDic = new Dictionary<string, ConfigDataBase>();
     
    /// <summary>
    /// 加载二进制配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">二进制文件路径</param>
    /// <returns></returns>
    public T LoadBinaryConfigData<T>(string binaryPath) where T : ConfigDataBase {
        if (string.IsNullOrEmpty(binaryPath)) {
            Debug.Log("二进制文件路径为空");
            return default(T);
        }
        if (m_configDataDic.ContainsKey(binaryPath)) {
            return (T)m_configDataDic[binaryPath];
        }

        ConfigDataBase data = GameDataConvert.BinaryDeserialize<T>(binaryPath);
#if UNITY_EDITOR
        if (data == null) {
            Debug.Log("二进制文件加载失败，尝试加载xml");
            string xmlPath = binaryPath.Replace("Binary", "Xml").Replace(".bytes", ".xml");
            data = GameDataConvert.XmlDeserializeInEditorMode<T>(xmlPath);
        }
#endif
        if (data != null) { 
            data.Init();

            m_configDataDic.Add(binaryPath, data);

            return data as T;
        }
        return default(T);
    }

    public T GetConfigData<T>(string path) where T:ConfigDataBase {
        if (string.IsNullOrEmpty(path))
        {
            return default(T);
        }
        ConfigDataBase data = null;
        if (m_configDataDic.TryGetValue(path, out data))
        {
            return data as T;
        }
        else {
            //LoadBinaryConfigData(path);
        }

        return default(T);
    }

}

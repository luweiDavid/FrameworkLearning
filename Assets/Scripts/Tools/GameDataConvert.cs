using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class GameDataConvert
{ 
    /// <summary>
    /// 实例类转xml
    /// </summary>  
    public static bool ClassToXml(System.Object obj) {
        string savePath = PathConfig.GameDataConfigXmlPath + obj.GetType().Name + ".xml"; 
        try
        { 
            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) {
                    XmlSerializer xmlS = new XmlSerializer(obj.GetType()); 
                    xmlS.Serialize(sw, obj);
                    return true;
                } 
            } 
        }
        catch (System.Exception e)
        {
            Debug.LogError("class to xml fail ： " + e);
            if (File.Exists(savePath)) {
                File.Delete(savePath);
            }
        }
        return false;
    }
     
    /// <summary>
    /// 读取xml，编辑器模式下
    /// </summary>
    /// <param name="path">xml路径</param>
    /// <param name="obj">实例类</param>
    public static T XmlDeserializeInEditorMode<T>(string path){
        T tObj = default(T);
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                XmlSerializer xmls = new XmlSerializer(tObj.GetType());
                tObj = (T)xmls.Deserialize(fs);
                return tObj;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取xml失败: " + e);
        }

        return default(T);
    }

    public static System.Object XmlDeserializeInEditorMode(string path, Type type)
    {
        System.Object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xmls = new XmlSerializer(type);
                obj = xmls.Deserialize(fs);
                return obj;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取xml失败: " + e);
        }

        return obj;
    }

    /// <summary>
    /// 读取xml 运行时模式下
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T XmlDeserialize<T>(string path) {
        T t = default(T);
        try
        {
            TextAsset tAsset = ResourcesManager.Instance.LoadResources<TextAsset>(path); 
            if (tAsset == null) {
                Debug.LogError("xml文件加载失败,路径： " + path);
                return t;
            }

            using (MemoryStream ms = new MemoryStream(tAsset.bytes)) {
                XmlSerializer xmls = new XmlSerializer(typeof(T));
                t = (T)xmls.Deserialize(ms);
            }
            ResourcesManager.Instance.ReleaseResources(path, true);
            
            return t;
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取xml失败， " + e);
        }
        return default(T);
    } 

    /// <summary>
    /// 二进制序列化
    /// </summary>
    public static bool BinarySerialize(System.Object obj) {
        string savePath = PathConfig.GameDataConfigBinaryPath + obj.GetType().Name + ".bytes"; 
        try
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) {
                BinaryFormatter binaryF = new BinaryFormatter();
                binaryF.Serialize(fs, obj);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("class to binary fail : " + e);
        }
        return false;
    }

    /// <summary>
    /// 读取二进制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T BinaryDeserialize<T>(string path) {
        T t = default(T);
        try
        {
            TextAsset tAsset = ResourcesManager.Instance.LoadResources<TextAsset>(path);
            if (tAsset == null)
            {
                Debug.LogError("二进制文件加载失败, 路径： " + path);
                return t;
            }
            using (MemoryStream ms = new MemoryStream(tAsset.bytes)) {
                BinaryFormatter binaryF = new BinaryFormatter();
                t = (T)binaryF.Deserialize(ms);
            }
            return t;
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取二进制失败, "+ e);
        }

        return default(T);
    }


}

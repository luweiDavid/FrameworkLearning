using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using System.Xml.Serialization;
using System;

public class TestLoad : Editor {
	[MenuItem("Tools/LoadAB")]
    public static void LoadAB()
    {
        //AssetBundle abDataBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data");
        //TextAsset dataTA = abDataBundle.LoadAsset<TextAsset>("ABDataBase");
        
        //MemoryStream ms = new MemoryStream(dataTA.bytes);
        //BinaryFormatter binaryF = new BinaryFormatter();
        //AssetBundleData  abData = (AssetBundleData)binaryF.Deserialize(ms);

        //foreach (AssetBundleDataBase temp in abData.abDataBaseList)
        //{
        //    Debug.Log("  路径：" + temp.Path + "  包名：  " + temp.ABName + "  资源名： " + temp.AssetName);

        //    foreach (string name in temp.DependenceList)
        //    {
        //        Debug.Log(name + "    ： 依赖项名");
        //    }

        //    Debug.Log("----------------------");
        //}
         
        
    } 
} 

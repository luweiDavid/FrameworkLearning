using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class ConfigDataEditor
{
    [MenuItem("Assets/ClassToXml")]
    public static void Assets_ClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("类转xml", "正在扫描：" + objs[i].name, 1.0f * i / objs.Length);
            ClassToXml(objs[i].name);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/XmlToBinary")]
    public static void Assets_XmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("xml转binary", "正在扫描：" + objs[i].name, 1.0f * i / objs.Length);
            XmlToBinary(objs[i].name);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/AllXmlToBinary")]
    public static void AllXmlToBinary() {
        string path = Application.dataPath.Replace("Assets", "") + PathConfig.GameDataConfigXmlPath;
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        { 
            EditorUtility.DisplayProgressBar("xml转binary", "正在扫描：" + files[i], 1.0f * i / files.Length);
            string str1 = files[i].Substring(files[i].LastIndexOf("/") + 1);
            if (str1.EndsWith(".xml")) {
                string name = str1.Replace(".xml", "");
                XmlToBinary(name);
            } 
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
     }

    private static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        Type _type = null;
        foreach (var temp in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = temp.GetType(name);
            if (tempType != null)
            {
                _type = tempType;
                break;
            }
        }

        if (_type != null)
        {
            var ints = Activator.CreateInstance(_type);
            if (ints is ConfigDataBase)
            {
                (ints as ConfigDataBase).Construct();
                bool isSuc = GameDataConvert.ClassToXml(ints);
            }
        }

    }

    private static void XmlToBinary(string name) {
        if (string.IsNullOrEmpty(name)) {
            return;
        }
        Type _type = null;
        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = item.GetType(name);
            if (tempType != null) {
                _type = tempType;
                break;
            }
        }
        if (_type != null) {
            string xmlPath = PathConfig.GameDataConfigXmlPath + name + ".xml";
            object obj = GameDataConvert.XmlDeserializeInEditorMode(xmlPath, _type);
            GameDataConvert.BinarySerialize(obj);
        }
    } 


}


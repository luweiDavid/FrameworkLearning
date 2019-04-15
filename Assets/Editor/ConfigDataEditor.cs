using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;

public class ConfigDataEditor
{
    [MenuItem("Tools/Test/ReadXml")]
    public static void ReadXml() {
        string dataPath = PathConfig.OuterDataRegPath + "MonsterData.xml";
        XmlDocument xmlDoc = new XmlDocument();
        XmlReader xmlR = XmlReader.Create(dataPath);
        xmlDoc.Load(xmlR);

        XmlNode rootNode = xmlDoc.SelectSingleNode("data");
        XmlElement rootElement = (XmlElement)rootNode;
        string className = rootElement.GetAttribute("name");
        string excelName = rootElement.GetAttribute("from");
        string xmlName = rootElement.GetAttribute("to");
        Debug.Log(className + "    " + excelName + "    " + xmlName);
        foreach (XmlNode item in rootNode.ChildNodes)
        { 
            XmlElement listElement = (XmlElement)item;
            string listName = listElement.GetAttribute("name");
            string typeStr = listElement.GetAttribute("type");
            Debug.Log(listName + "  ---  " + typeStr);

            XmlNode classNode = item.FirstChild;
            XmlElement classElement = (XmlElement)classNode;
            string classStructureName = classElement.GetAttribute("name");
            string sheetName = classElement.GetAttribute("sheetname");
            string mainKey = classElement.GetAttribute("mainKey");
            Debug.Log(classStructureName + "  == " + sheetName + " == " + mainKey);
            foreach (XmlNode temp in classNode.ChildNodes)
            {
                XmlElement varElement = (XmlElement)temp;
                string varName = varElement.GetAttribute("name");
                string varCol = varElement.GetAttribute("col");
                string varType = varElement.GetAttribute("type");
                Debug.Log(varName + " ....." + varCol + "....." + varType);
            }
        }
    }

    [MenuItem("Tools/Test/WriteExcel")]
    public static void WriteExcel() {
        string path = PathConfig.OuterDataExcelPath + "怪物配置.xlsx";
        FileInfo fileInfo = new FileInfo(path);
        if (File.Exists(path)) {
            File.Delete(path);
            fileInfo.Create();
        }


    }




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


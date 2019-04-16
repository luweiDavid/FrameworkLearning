using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.Reflection;

public class ConfigDataEditor
{
    [MenuItem("Tools/Xml/XmlToExcel")]
    public static void XmlToExcel() {
        string xmlPath = PathConfig.OuterDataRegPath + "MonsterData.xml";

        XmlDocument xmlDoc = new XmlDocument();
        XmlReader reader = XmlReader.Create(xmlPath);
        xmlDoc.Load(reader);

        XmlNode rootNode = xmlDoc.SelectSingleNode(xmlPath);
        XmlElement rootEle = (XmlElement)rootNode;
        string className = rootEle.GetAttribute("name");
        string excelName = rootEle.GetAttribute("from");
        string xmlName = rootEle.GetAttribute("to");
        
        //key：表名，
        Dictionary<string, SheetClass> nameSheetDic = new Dictionary<string, SheetClass>();  
        ReadXmlVariable(rootEle, nameSheetDic,0);
         
        Dictionary<string, SheetData> nameSheetDataDic = new Dictionary<string, SheetData>();

        //保存最外层的表
        List<SheetClass> outerSheetList = new List<SheetClass>();
        foreach (SheetClass sheet in nameSheetDic.Values)
        {
            if (sheet.Depth == 1) {
                outerSheetList.Add(sheet);
            }
        }

        //通过类名，获得脚本实例
        object classObj = null;
        Type classType = null;
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetType(className) != null) {
                classType = asm.GetType(className);
                break;
            }
        }
        if (classType != null) {

        }

        reader.Close();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlEle"></param>
    /// <param name="nameSheetDic"></param>
    /// <param name="depth">sheet的深度</param>
    private static void ReadXmlVariable(XmlElement xmlEle, Dictionary<string, SheetClass> nameSheetDic, int depth)
    {
        depth++;
        foreach (XmlNode node in xmlEle.ChildNodes)
        {
            XmlElement xe = (XmlElement)node;
            if (xe.GetAttribute("type") == "list") {

                XmlElement subEle = (XmlElement)xe.FirstChild;

                VariableClass varClass = new VariableClass()
                {
                    Name = xe.GetAttribute("name"),
                    Col = xe.GetAttribute("col"),
                    Type = xe.GetAttribute("type"),
                    SplitStr = xe.GetAttribute("split"),
                    Foreign = xe.GetAttribute("foreign"),
                    DefaultValue = xe.GetAttribute("default"),
                };
                 
                SheetClass _sheetClass = new SheetClass()
                {
                    ParentVarClass = varClass,
                    Name = subEle.GetAttribute("name"),
                    SheetName = subEle.GetAttribute("sheetname"),
                    MainKey = subEle.GetAttribute("mainKey"),
                    SplitStr = subEle.GetAttribute("split"),
                };

                if (!string.IsNullOrEmpty(_sheetClass.SheetName)) {
                    if (!nameSheetDic.ContainsKey(_sheetClass.SheetName)) {
                        foreach (XmlNode inNode in subEle.ChildNodes)
                        {
                            XmlElement inEle = (XmlElement)inNode;
                            VariableClass inVarClass = new VariableClass()
                            {
                                Name = inEle.GetAttribute("name"),
                                Col = inEle.GetAttribute("col"),
                                Type = inEle.GetAttribute("type"),
                                SplitStr = inEle.GetAttribute("split"),
                                Foreign = inEle.GetAttribute("foreign"),
                                DefaultValue = inEle.GetAttribute("default"),
                            };
                            _sheetClass.AllVariableList.Add(inVarClass);
                        }

                        nameSheetDic.Add(_sheetClass.SheetName, _sheetClass);
                    }
                }

                ReadXmlVariable(subEle, nameSheetDic, depth);
            }
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

public class SheetData {
    public List<string> AllNameList = new List<string>();
    public List<string> AllTypeList = new List<string>();
    public List<RowData> AllRowDataList = new List<RowData>();
}

public class RowData {
    //一行数据，key为Name， Value为对应的具体数值
    Dictionary<string, string> OneRowDataDic = new Dictionary<string, string>();
}

public class SheetClass {
    //父变量
    public VariableClass ParentVarClass { get; set; }
    //当前表的深度（第几张表）
    public int Depth { get; set; }
    //list名
    public string Name { get; set; }
    //表名
    public string SheetName { get; set; }
    //对外的主键
    public string MainKey { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //list中包含的所有变量
    public List<VariableClass> AllVariableList = new List<VariableClass>();
}

public class VariableClass {
    //变量名（对应原类的Name）
    public string Name { get; set; }
    //excel表的列名
    public string Col { get; set; }
    //变量类型
    public string Type { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //对外的键
    public string Foreign { get; set; }
    //默认值
    public string DefaultValue { get; set; } 
}







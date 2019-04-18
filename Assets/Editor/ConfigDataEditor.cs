using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml; 
using System.Reflection;
using System.ComponentModel;

public class ConfigDataEditor
{
    [MenuItem("Assets/XmlToExcel")]
    public static void Assets_XmlToExcel() {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Assets下的Xml 转 Excel", "正在扫描" + objs[i].name, 1.0f * i / objs.Length);
            XmlToExcel(objs[i].name);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static void XmlToExcel(string objName) {
        string regPath = PathConfig.OuterDataRegPath + objName + ".xml";
        if (!File.Exists(regPath)){
            Debug.LogError(regPath+ "  该文件不存在");
            return;
        }
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlReaderSettings setting = new XmlReaderSettings();
        setting.IgnoreComments = true;
        XmlReader reader = XmlReader.Create(regPath); 
        xmlDoc.Load(reader);
         
        XmlNode rootNode = xmlDoc.SelectSingleNode("data");
        XmlElement rootEle = (XmlElement)rootNode;
        
        string className = rootEle.GetAttribute("name");
        string excelName = rootEle.GetAttribute("from");
        string xmlName = rootEle.GetAttribute("to"); 
        //key：sheetname， 保存所有的sheetclass
        Dictionary<string, SheetClass> nameSheetClassDic = new Dictionary<string, SheetClass>();
        ReadRegXmlNode(rootEle, nameSheetClassDic,0); 
         
        Dictionary<string, SheetData> nameSheetDataDic = new Dictionary<string, SheetData>();

        //保存最外层的表（也就是data包含的variable）
        List<SheetClass> outerSheetList = new List<SheetClass>();
        foreach (SheetClass sheet in nameSheetClassDic.Values)
        { 
            if (sheet.Depth == 1) {
                outerSheetList.Add(sheet);
            }
        }
        //通过类名，获得脚本实例
        object classObj = GetClassObjFromXml(className);
        if (classObj != null)
        {
            foreach (SheetClass sheet in outerSheetList)
            {
                ReadData(classObj, sheet, nameSheetClassDic, nameSheetDataDic);
            }
        }
        else {
            Debug.Log("获取xml的类实例失败！");
            return;
        }
        reader.Close();

        //写入excel
        string excelPath = PathConfig.OuterDataExcelPath + excelName; 
        if (IsFileUsed(excelPath))
        {
            Debug.LogError("文件被占用");
            return;
        }
        try
        {
            FileInfo fileInfo = new FileInfo(excelPath);
            if (File.Exists(excelPath))
            {
                fileInfo.Delete();
                fileInfo = new FileInfo(excelPath);
            }
            foreach (string tName in nameSheetDataDic.Keys)
            {
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(tName);
                     
                    SheetData tempData = nameSheetDataDic[tName];
                    for (int i = 0; i < tempData.AllNameList.Count; i++)
                    {
                        string colName = tempData.AllNameList[i];
                        ExcelRange nameRange = workSheet.Cells[1, i + 1]; 
                        nameRange.Value = colName;
                        nameRange.AutoFitColumns();

                        string type = tempData.AllTypeList[i];
                        ExcelRange typeRange = workSheet.Cells[2, i + 1];
                        typeRange.Value = type;
                        typeRange.AutoFitColumns();
                    } 
                    for (int i = 0; i < tempData.AllRowDataList.Count; i++)
                    { 
                        RowData rData = tempData.AllRowDataList[i];
                        
                        for (int j = 0; j < rData.OneRowDataDic.Count; j++)
                        {
                            string colName = tempData.AllNameList[j];
                            string theValue = rData.OneRowDataDic[colName];
                            ExcelRange valueRange = workSheet.Cells[3 + i, j + 1];
                            valueRange.Value = theValue;
                            valueRange.AutoFitColumns();
                        } 
                    } 
                    
                    package.Save();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        Debug.Log("xml 转 excel 成功");
    }

    private static void ReadData(object obj, SheetClass sClass, Dictionary<string, SheetClass> nameSheetClassDic,
        Dictionary<string, SheetData> nameSheetDataDic) {

        List<VariableClass> allVarList = sClass.AllVariableList;
        VariableClass parentVar = sClass.ParentVarClass;

        string mainKey = sClass.MainKey; 

        //通过反射获取list
        object dataList = GetMemberValue(obj, parentVar.Name);
        int listCount = (int)dataList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, 
            null, dataList, new object[] { }); 
         
        SheetData shtData = new SheetData();
        for (int i = 0; i < allVarList.Count; i++)
        {
            if (!string.IsNullOrEmpty(allVarList[i].Col)) {
                shtData.AllNameList.Add(allVarList[i].Col);
                shtData.AllTypeList.Add(allVarList[i].Type);
            }  
        }
        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
                   null, dataList, new object[] { i });

            RowData rData = new RowData();
            for (int j = 0; j < allVarList.Count; j++)
            {
                VariableClass varClass = allVarList[j];
                if (allVarList[j].Type == "list")
                {
                    //如果list中的item包含list，就需要递归读取,这里需要引入一个外键，才能正确添加到nameSheetDataDic 
                    SheetClass tempClass = nameSheetClassDic[varClass.ListSheetName];

                    if (!string.IsNullOrEmpty(varClass.Foreign)) {
                        //是否存在外键

                    }

                    ReadData(item, tempClass, nameSheetClassDic, nameSheetDataDic);
                }
                else if (varClass.Type == "listStr" || varClass.Type == "listInt" ||
                  varClass.Type == "listFloat" || varClass.Type == "listBool") {

                    //处理基础数据类型的list变量 比如List<string>, 处理方式是用分隔符连接所有的值，然后返回一个字符串
                    string str = GetBaseListStr(item, varClass); 
                    rData.OneRowDataDic.Add(varClass.Col, str);
                }
                else
                {
                    object tempObj = GetMemberValue(item, varClass.Name);
                    rData.OneRowDataDic.Add(varClass.Col, tempObj.ToString());
                }
            }

            shtData.AllRowDataList.Add(rData); 
        }
        nameSheetDataDic.Add(sClass.SheetName, shtData);
    }
     
    /// <summary>
    /// 读取reg文件夹的xml文件， 主要是为了确定excel的数据结构
    /// </summary> 
    private static void ReadRegXmlNode(XmlElement xmlEle, Dictionary<string, SheetClass> nameSheetClassDic, int depth)
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
                    DefaultValue = xe.GetAttribute("defaultvalue"), 

                    ListName = ((XmlElement)xe.FirstChild).GetAttribute("name"),
                    ListSheetName = ((XmlElement)xe.FirstChild).GetAttribute("sheetname"),
                };

                SheetClass _sheetClass = new SheetClass()
                {
                    ParentVarClass = varClass,
                    Name = subEle.GetAttribute("name"),
                    SheetName = subEle.GetAttribute("sheetname"),
                    MainKey = subEle.GetAttribute("mainKey"),
                    SplitStr = subEle.GetAttribute("split"),
                    Depth = depth,
                };

                if (!string.IsNullOrEmpty(_sheetClass.SheetName)) {
                    if (!nameSheetClassDic.ContainsKey(_sheetClass.SheetName)) {
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
                                DefaultValue = inEle.GetAttribute("defaultvalue"),
                            };
                            if (inVarClass.Type == "list") {
                                inVarClass.ListName = ((XmlElement)inEle.FirstChild).GetAttribute("name");
                                inVarClass.ListSheetName = ((XmlElement)inEle.FirstChild).GetAttribute("sheetname");
                            }

                            _sheetClass.AllVariableList.Add(inVarClass);
                        }

                        nameSheetClassDic.Add(_sheetClass.SheetName, _sheetClass);
                    }
                }

                ReadRegXmlNode(subEle, nameSheetClassDic, depth);
            }
        }
    }


    /// <summary>
    /// 获取类的实例 （这是有数据的）
    /// </summary>
    /// <param name="className">类名</param>
    /// <returns></returns>
    private static object GetClassObjFromXml(string className) { 
        Type classType = null;
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetType(className) != null)
            {
                classType = asm.GetType(className);
                break;
            }
        }
        if (classType != null)
        {
            string xmlPath = PathConfig.GameDataConfigXmlPath + className + ".xml";
            return GameDataConvert.XmlDeserializeInEditorMode(xmlPath, classType);
        }
        return null;
    }

    /// <summary>
    /// 创建类的实例 （这是没有数据的）
    /// </summary>
    /// <param name="className">类名</param>
    /// <returns></returns>
    private static object CreateClassObjByName(string className) {
        object obj = null;
        Type _type = null;

        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (item.GetType(className) != null)
            {
                _type = item.GetType(className);
                break;
            }
        }

        if (_type != null)
        {
            obj = Activator.CreateInstance(_type); 
        }

        return obj;
    }

    private static object GetMemberValue(System.Object obj, string memberName, BindingFlags flags = BindingFlags.Public |
        BindingFlags.Instance | BindingFlags.Static)
    {

        Type _type = obj.GetType();
        MemberInfo[] members = _type.GetMember(memberName, flags);

        switch (members[0].MemberType)
        {
            case MemberTypes.Field:
                return _type.GetField(memberName, flags).GetValue(obj);
            case MemberTypes.Property:
                return _type.GetProperty(memberName, flags).GetValue(obj, new object[] { });
            default:
                return null;
        }
    }

    /// <summary>
    /// 判断一个文件是否被占用
    /// </summary>
    /// <param name="fullPath">文件的相对路径</param>
    /// <returns></returns>
    private static bool IsFileUsed(string fullPath) {
        bool result = false;
        if (!File.Exists(fullPath))
        {
            result = false;
        }
        else {
            try
            {
                using (FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    result = false;
                }
            }
            catch (Exception)
            {
                result = true;
            }
        }

        return result;
    }


    /// <summary>
    /// 基础数据List的所有值的拼接
    /// </summary>
    /// <param name="data"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetBaseListStr(object data,VariableClass varClass) {
        string str = ""; 
        if (string.IsNullOrEmpty(varClass.SplitStr)) {
            Debug.LogError(varClass.Name + " 的分隔符为空，请补充");
            return str;
        }
        object listObj = GetMemberValue(data, varClass.Name);
        int listCount = (int)listObj.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod,
            null, listObj, new object[] { });

        for (int i = 0; i < listCount; i++)
        {
            object item = listObj.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
            null, listObj, new object[] { i });

            str += item.ToString();
            if (i != listCount - 1) {
                str += varClass.SplitStr;
            }
        }

        return str;
    }


    private static object CreateListByType(Type type) {
        Type listType = typeof(List<>);
        Type pType = listType.MakeGenericType(new Type[] { type });
        object pListObj = Activator.CreateInstance(pType, new object[] { });

        return pListObj;
    }


    //-------------------------------------------------------------------------------------------------------------------
    [MenuItem("Tools/Xml/ExcelToXml")]
    public static void ExcelToXml() {
        string regPath = PathConfig.OuterDataRegPath + "MonsterData.xml";
        if (!File.Exists(regPath)) {
            Debug.LogError("reg文件不存在" + regPath);
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        XmlReader reader = XmlReader.Create(regPath);
        xmlDoc.Load(reader);

        XmlNode rootNode = xmlDoc.SelectSingleNode("data");
        XmlElement rootEle = (XmlElement)rootNode; 
        string className = rootEle.GetAttribute("name");
        string excelName = rootEle.GetAttribute("from");
        string xmlName = rootEle.GetAttribute("to");

        Dictionary<string, SheetClass> nameSheetClassDic = new Dictionary<string, SheetClass>();
        ReadRegXmlNode(rootEle, nameSheetClassDic, 0);

        Dictionary<string, SheetData> nameSheetDataDic = new Dictionary<string, SheetData>();
        string excelPath = PathConfig.OuterDataExcelPath + excelName;
         

        //读取excel的数据
        using (FileStream fs = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            using (ExcelPackage package = new ExcelPackage(fs)) {
                ExcelWorksheets sheets = package.Workbook.Worksheets;
                for (int i = 0; i < sheets.Count; i++)
                {
                    //注意从excel读取/写入数据时，下标是从1开始的
                    ExcelWorksheet workSheet = sheets[i + 1]; 
                    SheetData _shtData = new SheetData(); 
                    SheetClass _shtClass = nameSheetClassDic[workSheet.Name];
                    int colCount = workSheet.Dimension.End.Column;
                    int rowCount = workSheet.Dimension.End.Row; 

                    for (int m = 0; m < _shtClass.AllVariableList.Count; m++)
                    {
                        //保存变量名和类型
                        _shtData.AllNameList.Add(_shtClass.AllVariableList[m].Name);
                        _shtData.AllTypeList.Add(_shtClass.AllVariableList[m].Type);
                    }

                    //保存每一行的数据，从第三行开始是实际的数据
                    for (int m = 3; m <= rowCount; m++)
                    {
                        RowData _rowData = new RowData();
                        for (int n = 1; n <= colCount; n++)
                        {
                            string key = workSheet.Cells[1, n].Value.ToString(); 
                            string value = "";
                            if (workSheet.Cells[m, n].Value != null) {
                                value = workSheet.Cells[m, n].Value.ToString().Trim();
                            } 
                            _rowData.OneRowDataDic.Add(key, value);
                        }
                        _shtData.AllRowDataList.Add(_rowData); 
                    }
                    nameSheetDataDic.Add(workSheet.Name, _shtData);
                }
            }
        }

        //写入到类中
        object classObj = CreateClassObjByName(className);
        if (classObj != null)
        {
            //首先获取到外层表的表名
            List<string> outerKeyList = new List<string>();
            foreach (string str in nameSheetClassDic.Keys)
            {
                SheetClass _shtClass = nameSheetClassDic[str];
                if (_shtClass.Depth == 1)
                {
                    outerKeyList.Add(str);
                }
            }

            foreach (string str in outerKeyList)
            {
                WriteToClass(classObj, str, nameSheetClassDic, nameSheetDataDic);
            }
        }
    }

    private static void WriteToClass(object classObj,string shtName, Dictionary<string, SheetClass> nameSheetClassDic,
        Dictionary<string, SheetData> nameSheetDataDic) { 
        SheetClass shtClass = nameSheetClassDic[shtName];
        SheetData shtData = nameSheetDataDic[shtName]; 

        //根据T类型， 创建list
        object tempObj = CreateClassObjByName(shtClass.Name);
        object listObj = CreateListByType(tempObj.GetType());
        for (int i = 0; i < shtData.AllRowDataList.Count; i++)
        { 
            object tObj = CreateClassObjByName(shtClass.Name);
            for (int j = 0; j < shtClass.AllVariableList.Count; j++)
            {
                VariableClass varClass = shtClass.AllVariableList[j];
                if (varClass.Type == "list")
                {
                    //如果list中的变量也是list， 那么就要去变量（varClass）中的ListSheetName（表格名），然后递归写入
                    WriteToClass(tObj, varClass.ListSheetName, nameSheetClassDic, nameSheetDataDic);
                }
                else if (varClass.Type == "listStr" || varClass.Type == "listInt" ||
                  varClass.Type == "listFloat" || varClass.Type == "listBool") {

                    Type _tmpType = GetBaseTypeByType(varClass.Type);
                    if (_tmpType == null) {
                        Debug.LogError(varClass.Type + " 在已存在的类型中没有找到");
                        return;
                    }
                    object inlistObj = CreateListByType(_tmpType);
                    string invalue = shtData.AllRowDataList[i].OneRowDataDic[varClass.Col];

                    string[] strArray = invalue.Split(System.Convert.ToChar(varClass.SplitStr));
                    for (int strcount = 0; strcount < strArray.Length; strcount++)
                    {
                        string inlistValue = strArray[strcount].Trim();
                        if (!string.IsNullOrEmpty(inlistValue))
                        {
                            inlistObj.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                                null, inlistObj, new object[] { });
                        }
                        else {
                            Debug.Log("基础数据类型的List的值为null");
                        } 
                    }

                    tObj.GetType().GetProperty(varClass.Name).SetValue(tObj, inlistObj, new object[] { });
                }
                else
                {
                    //根据变量名，从类实例中反射出来PropertyInfo
                    PropertyInfo pInfo = tObj.GetType().GetProperty(varClass.Name);
                    string value = shtData.AllRowDataList[i].OneRowDataDic[varClass.Col];
                    if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DefaultValue))
                    {
                        value = varClass.DefaultValue;
                    }
                    if (string.IsNullOrEmpty(value)) {
                        Debug.LogError(string.Format("当前列< {0} >下的值是空的，并且reg文件中也没有设置默认值", varClass.Col));
                        return;
                    }

                    SetValueByType(pInfo, tObj, value, shtData.AllTypeList[j]);

                    listObj.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                        null, listObj, new object[] { tObj });
                }
            }
        }  
        classObj.GetType().GetProperty(shtClass.ParentVarClass.Name).SetValue(classObj, listObj, new object[] { });
         
        //xml序列化
        GameDataConvert.ClassToXml(classObj);
        AssetDatabase.Refresh();
    }

    private static void SetValueByType(PropertyInfo pInfo, object obj, string value, string type)
    {
        object valObj = (object)value; 
        switch (type)
        {
            case "int":
                valObj = System.Convert.ToInt32(valObj);
                break;  
            case "float":
                valObj = System.Convert.ToSingle(valObj);
                break;
            case "bool":
                valObj = System.Convert.ToBoolean(valObj);
                break;
            case "enum":
                valObj = TypeDescriptor.GetConverter(pInfo.PropertyType).ConvertFromInvariantString(valObj.ToString());
                break; 
            default:
                break;
        }
        pInfo.SetValue(obj, valObj, new object[] { });
    }

    private static Type GetBaseTypeByType(string type) {

        Type _resultType = null;
        switch (type)
        {
            case "listStr":
                _resultType = typeof(string);
                break;
            case "listInt":
                _resultType = typeof(int);
                break;
            case "listBool":
                _resultType = typeof(bool);
                break;
            case "listFloat":
                _resultType = typeof(float);
                break;
            //不支持enmu的List
            //case "listEnum":
            //    _resultType = typeof(enum);
            //    break;
            default:
                break;
        }
        return _resultType;
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
            }
            GameDataConvert.ClassToXml(ints); 
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
    //所有的excel表中的名称
    public List<string> AllNameList = new List<string>();
    public List<string> AllTypeList = new List<string>();
    public List<RowData> AllRowDataList = new List<RowData>();
}

public class RowData {
    //一行数据，key为Name， Value为对应的具体数值
    public Dictionary<string, string> OneRowDataDic = new Dictionary<string, string>();
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

    //以下两个变量是当自身是个list时，还应该保存自身的firstNode的name（也就是List<T>中T的名字）和sheetname，
    //因为nameSheetClassDic会保存所有的满足sheet结构的数据，这样就可以通过sheetname在nameSheetClassDic中找到对应的SheetClass
    //具体可以看本脚本最下面的注释

    public string ListName { get; set; } 
    public string ListSheetName { get; set; }
} 
/*
<?xml version = "1.0" encoding = "utf-8"?>

<data name = "MonsterData" from = "G怪物.xlsx" to = "MonsterData.xml">
	<variable name = "m_monstersDataList" type = "list">
		<list name = "MonsterDataStructure" sheetname = "怪物配置" mainKey = "Id">
			<variable name = "Id" col = "ID" type = "int"/>
			<variable name = "Name" col = "名字" type = "string"/>
			<variable name = "OutLook" col = "预制路径" type = "string"/>
			<variable name = "Rare" col = "稀有度" type = "int"/>
			<variable name = "m_monstersDataList" type = "list">
				<list name = "MonsterDataStructure" sheetname = "怪物配置" mainKey = "Id">
					<variable name = "Id" col = "ID" type = "int"/>
					<variable name = "Name" col = "名字" type = "string"/>
					<variable name = "OutLook" col = "预制路径" type = "string"/>
					<variable name = "Rare" col = "稀有度" type = "int"/>
					 
					<variable name = "Level" col = "等级" type = "int"/> 
				</list>
			</variable>
			<variable name = "Level" col = "等级" type = "int"/> 
		</list>
	</variable>
</data>
*/





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

public class TestEditor
{

    [MenuItem("Test/ReadXml")]
    public static void ReadXml()
    {
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

    [MenuItem("Test/WriteExcel")]
    public static void WriteExcel()
    {
        string path = PathConfig.OuterDataExcelPath + "怪物配置.xlsx";
        FileInfo fileInfo = new FileInfo(path);
        if (File.Exists(path))
        {
            File.Delete(path);
            fileInfo = new FileInfo(path);
        }
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("怪物");
            //workSheet.DefaultColWidth = 10;    //默认行宽
            //workSheet.DefaultRowHeight = 10;      //默认列高
            //workSheet.Cells.Style.WrapText = true;    //设置所有单元格的自动换行
            //workSheet.InsertColumn()              //插入行
            //workSheet.InsertRow()                 //插入列
            //workSheet.DeleteColumn()              //删除行
            //workSheet.DeleteRow()                 //删除列
            //workSheet.Column(1).Width = 10;        //某一行的宽
            //workSheet.Row(1).Height = 10;          //某一列的高度
            //workSheet.Column(1).Hidden = true;    //某一行是否隐藏
            //workSheet.Row(1).Hidden = true;       //某一列是否隐藏
            //workSheet.Column(1).Style.Locked = true;  //某一行是否锁定
            //设置所有单元格的对齐方式
            //workSheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            ExcelRange range = workSheet.Cells[1, 1];
            range.Value = "test excel";
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.LightUp;
            //range.Style.Fill.BackgroundColor.SetColor()   //
            //range.Style.Font.Color.SetColor()
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            range.AutoFitColumns();
            range.Style.WrapText = true;

            package.Save();
        }
    }

    [MenuItem("Test/Reflection")]
    public static void Reflection()
    {
        //TestClass1 t1 = new TestClass1
        //{
        //    Id = 1,
        //    Name = "testclass",
        //    strList = new List<string>(),
        //    peopleList = new List<PeopleInfo>(),
        //};
        //t1.strList.Add("csharp");
        //t1.strList.Add("java");
        //t1.strList.Add("cpp");

        //for (int i = 0; i < 3; i++)
        //{
        //    PeopleInfo p = new PeopleInfo() {
        //        Age = i + 10,
        //        Name = i + " : name",
        //    };
        //    t1.peopleList.Add(p);
        //} 

        //object list = GetMemberValue(t1, "strList");
        //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod,
        //    null, list, new object[] { }));

        //for (int i = 0; i < listCount; i++)
        //{
        //    object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
        //        null, list, new object[] { i });
        //    Debug.Log(item);
        //}


        //自定义类的list
        //object pList = GetMemberValue(t1, "peopleList");
        //int listCount = System.Convert.ToInt32(pList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod,
        //    null, pList, new object[] { }));

        //for (int i = 0; i < listCount; i++)
        //{
        //    object item = pList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod,
        //        null, pList, new object[] { i });

        //    int age = (int)GetMemberValue(item, "Age");
        //    string name = (string)GetMemberValue(item, "Name");
        //    Debug.Log(age + "  " + name);
        //} 
    }
     
    [MenuItem("Test/ReflectionWrite")]
    public static void ReflectionWrite() {
        object obj = CreateClassInstanceByName("TestClass1");

        PropertyInfo info1 = obj.GetType().GetProperty("Id");
        //info1.SetValue(obj, System.Convert.ToInt32("123"),new object[] { }); 
        PropertyInfo info2 = obj.GetType().GetProperty("Name");
        //info2.SetValue(obj, "TestClass1", new object[] { }); 
        PropertyInfo info3 = obj.GetType().GetProperty("Height");
        //info3.SetValue(obj, System.Convert.ToSingle("182.3"), new object[] { }); 
        PropertyInfo info4 = obj.GetType().GetProperty("IsMale");
        //info4.SetValue(obj, System.Convert.ToBoolean("true"), new object[] { }); 
        PropertyInfo info5 = obj.GetType().GetProperty("testEnum");
        //object enumObj = TypeDescriptor.GetConverter(info5.PropertyType).ConvertFromInvariantString("Happy");
        //info5.SetValue(obj, enumObj, new object[] { });

        SetValueByType(info1, obj, "123", "int");
        SetValueByType(info2, obj, "TestClass1", "string");
        SetValueByType(info3, obj, "183.5", "float");
        SetValueByType(info4, obj, "true", "bool");
        SetValueByType(info5, obj, "Happy", "enum");

        //Type listType = typeof(List<>);
        //Type strtype = listType.MakeGenericType(new Type[] { typeof(string) });
        //object strListObj = Activator.CreateInstance(strtype);
        //for (int i = 0; i < 3; i++)
        //{
        //    object temp = " str " + i;
        //    strListObj.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null,
        //        strListObj, new object[] { temp });
        //}

        //obj.GetType().GetProperty("strList").SetValue(obj, strListObj, new object[] { });

        Type listType = typeof(List<>);
        Type pType = listType.MakeGenericType(new Type[] { typeof(PeopleInfo) });
        object pListObj = Activator.CreateInstance(pType,new object[] { });
        for (int i = 0; i < 3; i++)
        {
            PeopleInfo p = new PeopleInfo()
            {
                Age = i + 10,
                Name = i + " : name",
            };
            pListObj.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                pListObj, new object[] { (object)p });
        }
        obj.GetType().GetProperty("peopleList").SetValue(obj, pListObj, new object[] { });


        TestClass1 tc = obj as TestClass1;
        //Debug.Log(tc.Id + "    " + tc.Name + "    " + tc.Height + "    " + tc.IsMale + "    " + tc.testEnum); 
        //foreach (var item in tc.strList)
        //{
        //    Debug.Log(item);
        //}

        foreach (var item in tc.peopleList)
        {
            Debug.Log(item.Age + "    " + item.Name);
        }
    }
      

    private static object GetMemberValue(System.Object obj, string memberName, BindingFlags flags = BindingFlags.Public |
        BindingFlags.Instance | BindingFlags.Static) {

        Type _type = obj.GetType();
        MemberInfo[] members = _type.GetMember(memberName, flags);
         
        switch (members[0].MemberType)
        {
            case MemberTypes.Field:
                return _type.GetField(memberName, flags).GetValue(obj);  
            case MemberTypes.Property: 
                return _type.GetProperty(memberName,  flags).GetValue(obj, new object[] { }); 
            default:
                return null;
        }
    } 
    private static object CreateClassInstanceByName(string name) {
        object obj = null;
        Type _type = null;

        foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (item.GetType(name) != null) {
                _type = item.GetType(name);
                break;
            }
        }

        if (_type != null) {
            obj = Activator.CreateInstance(_type);
        }

        return obj;
    }

    private static void SetValueByType(PropertyInfo pInfo, object obj, string value, string type) {
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

}




public class TestClass1 {
    public int Id { get; set; }
    public string Name { get; set; }
    public float Height { get; set; }
    public bool IsMale { get; set; }

    public TestEnum testEnum { get; set; }

    public List<string> strList { get; set; }
    public List<PeopleInfo> peopleList { get; set; }
}

public class PeopleInfo {
    public int Age { get; set; }
    public string Name { get; set; }
} 

public enum TestEnum
{
    None = 0,
    Happy,
    Sad,
}
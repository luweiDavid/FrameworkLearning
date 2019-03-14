using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

public class ResourcesTest : MonoBehaviour
{

    private void Start()
    {
        //XmlSerilizeClassFunc();
        //XmlDeserilizeClassFunc();
        //BinarySerilizeClassFunc();
        //BinaryDeserilizeClassFunc();
    }
     
    //#region   xml
    //public void XmlSerilizeClassFunc()
    //{
    //    TestSerilize ts = new TestSerilize();
    //    ts.Id = 1;
    //    ts.Name = "test";
    //    ts._intList = new List<int>();
    //    ts._intList.Add(2);
    //    ts._intList.Add(3);
    //    XmlSerilize(ts);
    //}

    //void XmlDeserilizeClassFunc()
    //{
    //    TestSerilize ts = XmlDeserilize();
    //    Debug.Log(ts.Id + ts.Name);
    //    foreach (int i in ts._intList)
    //    {
    //        Debug.Log(i + "_");
    //    }
    //}

    //void XmlSerilize(TestSerilize tests)
    //{
    //    FileStream fs = new FileStream(Application.dataPath + "/test.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    //    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
    //    XmlSerializer xmls = new XmlSerializer(tests.GetType());
    //    xmls.Serialize(sw, tests);
    //    sw.Close();
    //    fs.Close();
    //}

    //TestSerilize XmlDeserilize()
    //{
    //    FileStream fs = new FileStream(Application.dataPath + "/test.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
    //    XmlSerializer xmls = new XmlSerializer(typeof(TestSerilize));
    //    TestSerilize ts = (TestSerilize)xmls.Deserialize(fs);
    //    fs.Close();
    //    return ts;
    //}
    //#endregion

    //#region binary
    //void BinarySerilizeClassFunc()
    //{
    //    TestSerilize ts = new TestSerilize();
    //    ts.Id = 10;
    //    ts.Name = "二进制测试";
    //    ts._intList = new List<int>();
    //    ts._intList.Add(29);
    //    ts._intList.Add(13);
    //    BinarySerilize(ts);
    //}

    //void BinaryDeserilizeClassFunc()
    //{
    //    TestSerilize ts = BinaryDeserilize();
    //    Debug.Log(ts.Id + ts.Name);
    //    foreach (int i in ts._intList)
    //    {
    //        Debug.Log(i + "__");
    //    }
    //}

    //void BinarySerilize(TestSerilize tests)
    //{
    //    FileStream fs = new FileStream(Application.dataPath + "/binaryaTest.bytes", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    //    BinaryFormatter binaryF = new BinaryFormatter();
    //    binaryF.Serialize(fs, tests);
    //    fs.Close();
    //}

    //TestSerilize BinaryDeserilize()
    //{
    //    TextAsset testa = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/binaryaTest.bytes");
    //    MemoryStream ms = new MemoryStream(testa.bytes);
    //    BinaryFormatter binaryF = new BinaryFormatter();
    //    TestSerilize ts = (TestSerilize)binaryF.Deserialize(ms);
    //    ms.Close();
    //    return ts;
    //}


    //#endregion

    //#region  assets
    //void AssetsSerilizeFunc()
    //{
    //    TestAssets ta = AssetDatabase.LoadAssetAtPath<TestAssets>("Assets/Scripts/TestAssets.asset");
    //    Debug.Log(ta.Id + ta.Name);
    //    foreach (string str in ta._strList)
    //    {
    //        Debug.Log(str + "_");
    //    }
    //}
    //#endregion









}

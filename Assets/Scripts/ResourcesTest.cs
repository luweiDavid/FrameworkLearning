using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Xml.Serialization;

public class ResourcesTest : MonoBehaviour {

    private void Start()
    {
        SerilizeClassFunc();
    }


    public void SerilizeClassFunc() {
        TestSerilize ts = new TestSerilize();
        ts.Id = 1;
        ts.Name = "test";
        ts._intList = new List<int>();
        ts._intList.Add(2);
        ts._intList.Add(3);
        XmlSerilizeFunc(ts);
    } 

    void XmlSerilizeFunc(TestSerilize tests) {
        FileStream fs = new FileStream(Application.dataPath + "/test.xml", FileMode.Create, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        XmlSerializer xmls = new XmlSerializer(tests.GetType());
        xmls.Serialize(sw, tests);
        sw.Close();
        fs.Close();
    }
}

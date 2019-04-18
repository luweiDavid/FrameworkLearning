using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CustomData : ConfigDataBase
{
    [XmlElement("customDataList")]
    public List<CustomDataStructure> customDataList { get; set; }

    [XmlIgnore]
    public Dictionary<int, CustomDataStructure> idCusDataDic = new Dictionary<int, CustomDataStructure>();


    public override void Init()
    {
        idCusDataDic.Clear();
        foreach (var item in customDataList)
        {
            int id = item.Id;
            if (!idCusDataDic.ContainsKey(id)) {
                idCusDataDic.Add(id, item);
            }
        }
    }

    public CustomDataStructure GetCusDataById(int id) {
        if (idCusDataDic.ContainsKey(id)) {
            return idCusDataDic[id];
        }

        return null;
    }

    public override void Construct()
    {
        customDataList = new List<CustomDataStructure>();
        for (int i = 0; i < 1; i++)
        {
            CustomDataStructure cusData = new CustomDataStructure()
            {
                Id = 5050 + i,
                Name = "CustomDataStructure" + i,
            };
            customDataList.Add(cusData);

            cusData.TestCustomList = new List<TestCustomData>();
            for (int j = 0; j < 1; j++)
            {
                TestCustomData test = new TestCustomData()
                {
                    Id = 2000333 + j,
                    Name = "TestCustomList" + i,
                };
                cusData.TestCustomList.Add(test);
            }
        }
    }
}

public class CustomDataStructure {
    [XmlElement("Id")]
    public int Id { get; set; }
    [XmlElement("Name")]
    public string Name { get; set; }

    [XmlElement("TestCustomList")]
    public List<TestCustomData> TestCustomList { get; set; }
}

public class TestCustomData {
    [XmlElement("Id")]
    public int Id { get; set; }
    [XmlElement("Name")]
    public string Name { get; set; }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class MonsterData : ConfigDataBase
{

    [XmlElement("MonsterDataList")]
    public List<MonsterDataStructure> m_monstersDataList = new List<MonsterDataStructure>(); 
     
    [XmlIgnore]
    public Dictionary<int, MonsterDataStructure> m_monsterDataDic = new Dictionary<int, MonsterDataStructure>();
     
    public override void Init()
    {
        m_monsterDataDic.Clear();
        for (int i = 0; i < m_monstersDataList.Count; i++)
        {
            int id = m_monstersDataList[i].Id;
            if (m_monsterDataDic.ContainsKey(id)) {
                Debug.Log("有相同的Monster ID :" + id);
                continue;
            }
            m_monsterDataDic.Add(id, m_monstersDataList[i]);
        }
    }

    public MonsterDataStructure GetMonsterDataById(int id) {
        if (m_monsterDataDic.ContainsKey(id)) {
            return m_monsterDataDic[id];
        }
        return null;
    }

    public override void Construct()
    {  
        for (int i = 0; i < 5; i++)
        {
            MonsterDataStructure data = new MonsterDataStructure();
            data.Id = i + 1;
            data.Name = "Monster " + i;
            data.OutLook = "Assets/GameData/Prefabs/Attack.prefab";
            data.Rare = i + 1;
            data.Level = i + 1;
            m_monstersDataList.Add(data);
        } 
    } 
}
[System.Serializable]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class MonsterDataStructure {
    [XmlElement("Id")]
    public int Id { get; set; }
    [XmlElement("Name")]
    public string Name { get; set; }
    [XmlElement("OutLook")]
    public string OutLook { get; set; }
    [XmlElement("Rare")]
    public int Rare { get; set; }
    [XmlElement("Level")]
    public int Level { get; set; } 
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable] 
public class MonsterData : ConfigDataBase
{ 
    [XmlElement("MonsterDataList")]
    public List<MonsterDataStructure> m_monstersDataList { get; set; }

    [XmlElement("MonsterDataList2")]
    public List<MonsterDataStructure> m_monstersDataList2 { get; set; }

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
#if UNITY_EDITOR
    public override void Construct()
    {
        m_monstersDataList = new List<MonsterDataStructure>();
        m_monstersDataList2 = new List<MonsterDataStructure>();
        for (int i = 0; i < 5; i++)
        {
            MonsterDataStructure data = new MonsterDataStructure();
            data.Id = i + 1;
            data.Name = "Monster " + i;
            data.OutLook = "Assets/GameData/Prefabs/Attack.prefab";
            data.Rare = i + 1;
            data.Level = i + 1;
            data.Height = Random.Range(1.7f, 5.2f);
            data.Type = MonsterType.JiangShi;
            data.StrList = new List<string>();
            data.StrList.Add("一级");
            data.StrList.Add("二级");
            data.StrList.Add("三级");
            data.StrList.Add("四级");
            m_monstersDataList.Add(data);
            m_monstersDataList2.Add(data);
        } 
    }
#endif
}
[System.Serializable] 
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
    [XmlElement("Height")]
    public float Height { get; set; }
    [XmlElement("Type")]
    public MonsterType Type { get; set; }

    [XmlElement("StrList")]
    public List<string> StrList { get; set; } 
}

[System.Serializable]
public enum MonsterType {
    None = 0,
    JiangShi,
    NvWu,

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]  //可序列标记
public class TestSerilize {
    [XmlAttribute("Id")]
    public int Id;
    [XmlAttribute("Name")]
    public string Name;
    [XmlElement("intList")]
    public List<int> _intList;
}

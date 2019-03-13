using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LoadAssetBundleManager : MonoBehaviour {
     
	void Start () {
        //LoadAB();
	}

    void LoadAB() {
        //通过加载写入的二进制文件，
        AssetBundle abDataBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data");
        TextAsset dataTA = abDataBundle.LoadAsset<TextAsset>("ABDataBase");

        MemoryStream ms = new MemoryStream(dataTA.bytes);
        BinaryFormatter binaryF = new BinaryFormatter();
        var c = binaryF.Deserialize(ms);
    }

    public void LoadAssetByABName(string name) {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour {

    private void Awake()
    {
        AssetBundleManager.Instance.LoadABConfigData();
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameRoot : MonoBehaviour {   
    public const bool m_UseAssetBundleInEditor = false;
         
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadABConfigData();
        ResourcesManager.Instance.Init(this);

        ObjectsManager.Instance.Init(transform.Find("RecycleObjectsTr"), transform.Find("SceneTr"));
         
        #region
        var uiroot = GameObject.Find("UIRoot").GetComponent<Transform>();
        var uicamera = GameObject.Find("UICamera").GetComponent<Camera>();
        var entsys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        UIManager.Instance.Init(uiroot, uicamera, entsys);
        #endregion
         
        InitConfig();
    }
    public void InitConfig() {
        //游戏开始时，加载所有的二进制数据
        string path = PathConfig.GameDataConfigBinaryPath + "MonsterData.bytes";
        MonsterData monsData =  ConfigManager.Instance.LoadBinaryConfigData<MonsterData>(path);
        foreach (MonsterDataStructure item in monsData.m_monsterDataDic.Values)
        {
            Debug.Log(item.Id + "  " + item.Name + "  " + item.OutLook + "   " + item.Type);
        }
    }

    public void Start()
    { 


        UIManager.Instance.OpenWindow<MainPanel>(UIPanelName.MainPanel);
    } 
    

    private void Update()
    {
        
    } 

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR 
        AssetBundle.UnloadAllAssetBundles(true);
#endif
    }

}

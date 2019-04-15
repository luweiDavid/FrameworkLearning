using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameRoot : MonoBehaviour {   
    public const bool m_UseAssetBundleInEditor = true;
         
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

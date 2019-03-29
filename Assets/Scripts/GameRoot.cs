using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class GameRoot : MonoBehaviour {   
    public const bool m_UseAssetBundleInEditor = true;
       
    public AudioSource audioS;
    private AudioClip clip;
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadABConfigData();
        ResourcesManager.Instance.Init(this);

        ObjectsManager.Instance.Init(transform.Find("RecycleObjectsTr"),transform.Find("SceneTr"));
    }

    public void Start()
    {
        //clip = ResourcesManager.Instance.LoadResources<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
        //ResourcesManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/senlin.mp3", DealFinish, AsyncLoadPriority.High);

        //预加载
        //ResourcesManager.Instance.PreloadResources("Assets/GameData/Sounds/senlin.mp3");
    }

    void DealFinish(string path,UnityEngine.Object obj, object param1, object param2, object param3) {
        clip = obj as AudioClip;
        audioS.clip = clip;
        audioS.Play();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    long time = DateTime.Now.Ticks;
        //    clip = ResourcesManager.Instance.LoadResources<AudioClip>("Assets/GameData/Sounds/senlin.mp3"); 
        //    Debug.Log(DateTime.Now.Ticks - time);  //对比有预加载和没有预加载时， 加载资源所需的时间
        //    audioS.clip = clip;
        //    audioS.Play();
        //}
        //if (Input.GetKeyDown(KeyCode.D)) {
        //    ResourcesManager.Instance.ReleaseResources(clip, true);
        //}
    }

}

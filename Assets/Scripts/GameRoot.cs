using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class GameRoot : MonoBehaviour {   
    public const bool m_UseAssetBundleInEditor = true;
       
    //public AudioSource audioS;
    //private AudioClip clip;

    private GameObject gameObj;
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

        //gameObj = ObjectsManager.Instance.InstantiateGameObj("Assets/GameData/Prefabs/Attack.prefab",true);

        ObjectsManager.Instance.PreloadGameObj("Assets/GameData/Prefabs/Attack.prefab", 20);
    } 
    //void DealFinish(string path,UnityEngine.Object obj, object param1, object param2, object param3) {
    //    clip = obj as AudioClip;
    //    audioS.clip = clip;
    //    audioS.Play();
    //}

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
         
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    //从对象池中加载 
        //    gameObj = ObjectsManager.Instance.InstantiateGameObj("Assets/GameData/Prefabs/Attack.prefab", true);
        //}

        //if (Input.GetKeyDown(KeyCode.W)) {
        //    ObjectsManager.Instance.ReleaseGameObject(gameObj, 0);
        //}
        if (Input.GetKeyDown(KeyCode.A)) {
            ObjectsManager.Instance.AsyncInstantiateGameObj("Assets/GameData/Prefabs/Attack.prefab", InstantiateDealFinish, AsyncLoadPriority.High);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            //回收
            ObjectsManager.Instance.ReleaseGameObject(gameObj);
        }
    }

    void InstantiateDealFinish(string path, UnityEngine.Object obj, object param1, object param2, object param3) {
        gameObj = obj as GameObject;
        Debug.Log("玩也");
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR 
        AssetBundle.UnloadAllAssetBundles(true);
#endif
    }

}

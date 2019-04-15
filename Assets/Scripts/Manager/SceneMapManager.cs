using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMapManager : Singleton<SceneMapManager>
{
    private MonoBehaviour m_mono;

    public int m_loadProgress = 0;
    public bool m_isSceneLoaded = false;
    public string m_curSceneName;

    //场景加载开始的回调
    private Action m_loadSceneBeginCB;
    //场景加载结束的回调
    private Action m_loadSceneFinishedCB;

    public void Init(MonoBehaviour mono) {
        this.m_mono = mono;
    }

    public void LoadScene(string name, Action beginCB = null, Action finishCB = null) {
        m_loadSceneBeginCB = beginCB;
        m_loadSceneFinishedCB = finishCB;
        m_mono.StartCoroutine(LoadSceneAsync(name));
    }

    /// <summary>
    /// 异步加载场景
    /// </summary> 
    IEnumerator LoadSceneAsync(string sceneName) {
        m_loadProgress = 0;
        m_isSceneLoaded = false;
        if (m_loadSceneBeginCB != null) {
            m_loadSceneBeginCB();
        }
        ClearCache();

        Scene activeScene = SceneManager.GetActiveScene();
        m_curSceneName = activeScene.name;

        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        int targetProgress = 0;
        while (asyncOper != null && asyncOper.isDone == false)
        {
            asyncOper.allowSceneActivation = false;
            targetProgress = (int)asyncOper.progress;
            while (targetProgress < 0.9)
            {
                if (m_loadProgress < targetProgress)
                {
                    ++m_loadProgress;
                    yield return new WaitForEndOfFrame();
                }
            }

            targetProgress = 100;
            if (m_loadProgress < targetProgress - 2) {
                ++m_loadProgress;
                yield return new WaitForEndOfFrame();
            }
            asyncOper.allowSceneActivation = true;
            m_isSceneLoaded = true;
            m_loadProgress = 100;
            while (m_loadSceneFinishedCB != null) {
                m_loadSceneFinishedCB();
            }
            m_curSceneName = sceneName;
        }
    }

    /// <summary>
    /// 根据场景名设置场景环境
    /// </summary>
    public void SetSceneSetting(string sceneName)
    {
        // TODO

    }

    private void ClearCache() {
        ObjectsManager.Instance.ClearCache();
        ResourcesManager.Instance.ClearCache();
    }

}

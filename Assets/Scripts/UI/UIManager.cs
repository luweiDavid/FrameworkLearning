using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager> {

    public Transform m_uiRoot;
    public Transform m_normalLayerTr; 

    public Canvas m_normalLayerCanvas;

    public Camera m_uiCamera;

    public EventSystem m_eventSys;

    protected Dictionary<UIPanelName, BaseView> m_nameViewDic = new Dictionary<UIPanelName, BaseView>();

    public void Init(Transform uiroot, Camera uicamera, EventSystem entSys) {
        m_uiRoot = uiroot;
        m_uiCamera = uicamera;
        m_eventSys = entSys; 

        m_normalLayerTr = m_uiRoot.Find("NormalLayer").GetComponent<Transform>();
        m_normalLayerCanvas = m_normalLayerTr.GetComponent<Canvas>();
    } 

    public T OpenWindow<T>(UIPanelName name, bool isTop = true, params object[] paramArray) where T : BaseView {
        if (!m_nameViewDic.ContainsKey(name))
        {
            if (UIConfig.Instance.m_panelDic.ContainsKey(name))
            {
                UIPanelConfig panelConfig = UIConfig.Instance.m_panelDic[name];

                BaseView view = System.Activator.CreateInstance(Type.GetType(panelConfig.ClassName)) as BaseView;
                if (view != null)
                {
                    var obj = ObjectsManager.Instance.InstantiateGameObj(panelConfig.Path, false, false);
                    if (obj != null)
                    {
                        view.m_gameObj = obj;
                        view.m_trans = view.m_gameObj.transform as RectTransform;


                        if (panelConfig.Layer == UILayer.None || panelConfig.Layer == UILayer.NormalLayer)
                        {  
                            view.m_trans.SetParent(m_normalLayerTr); 
                            //Debug.Log(view.m_trans.anchoredPosition + "  -after-  " + view.m_trans.sizeDelta
                            //    + " --- " + view.m_trans.offsetMax + " --- " + view.m_trans.offsetMin + " --- " + view.m_trans.rect); 
                        }
                        else if(panelConfig.Layer == UILayer.BaseLayer) {

                        }
                        else if (panelConfig.Layer == UILayer.TipsLayer)
                        {

                        }
                        else if (panelConfig.Layer == UILayer.TopLayer)
                        {

                        }

                        view.Init();
                        m_nameViewDic.Add(name, view);

                        if (isTop) {
                            view.m_trans.SetAsLastSibling();
                        }
                        view.OnShow();

                        return view as T;
                    }
                    else
                    {
                        Debug.LogError(string.Format("该路径{0}没有对应的prefab: {1}", panelConfig.Path, panelConfig.PrefabName));
                    }
                }
                else
                {
                    Debug.LogError("无法创建View， 请检查类名");
                }
            }
            else
            {
                Debug.LogError("不存在该Panel : " + name);
            }
        }
        else {
            BaseView temp = m_nameViewDic[name];
            if (temp != null) {
                temp.OnShow();
            }
        }

        return null;
    }

    public void CloseWindow(UIPanelName name) {
        BaseView view = GetWindow(name);
        if (view != null) {
            view.OnDisable(); 
        }
    } 

    public BaseView GetWindow(UIPanelName name) {
        BaseView view = null;
        if (m_nameViewDic.TryGetValue(name, out view)) {
            return view;
        }

        return null;
    }

    public void AsyncLoadSprite(string path, Image targetImg, AsyncLoadPriority priority = AsyncLoadPriority.Middile, bool isSetNativeSize = false) {
        if (targetImg == null) {
            return;
        }

        ResourcesManager.Instance.AsyncLoadResource(path, AsyncLoadSpriteFinish, priority,true, targetImg, isSetNativeSize);
    }

    protected void AsyncLoadSpriteFinish(string path, UnityEngine.Object obj, object param1, object param2, object param3) {
        if (obj == null) {
            return;
        }

        Image targetImg = param1 as Image;
        bool isSetNativesize = (bool)param2;
        Sprite newSprite = obj as Sprite;
        if (targetImg.sprite != null) {
            targetImg.sprite = null;
        }
        if (newSprite) {
            targetImg.sprite = newSprite;

            if (isSetNativesize)
            {
                targetImg.SetNativeSize();
            }
        }     }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BaseView {
    private Button m_startBtn;
    private Button m_exitBtn;
    private Image m_bgImg;
    private Image m_leftImg;
    private Image m_rightImg;

    public override void Init(params object[] paramArray)
    {
        base.Init(paramArray); 
        m_startBtn = m_gameObj.transform.Find("StartBtn").GetComponent<Button>();
        m_exitBtn = m_gameObj.transform.Find("ExitBtn").GetComponent<Button>();
        m_bgImg = m_gameObj.transform.Find("Bg").GetComponent<Image>();
        m_leftImg = m_gameObj.transform.Find("Left").GetComponent<Image>();
        m_rightImg = m_gameObj.transform.Find("Right").GetComponent<Image>();

        m_startBtn.onClick.AddListener(OnStartBtnClick);
        m_exitBtn.onClick.AddListener(OnExitBtnClick);
         

        //可以调试一下异步加载图片的优先级是否正常
        UIManager.Instance.AsyncLoadSprite("Assets/GameData/Textures/loading4.jpg", m_bgImg, AsyncLoadPriority.Low);

        UIManager.Instance.AsyncLoadSprite("Assets/GameData/Textures/loading4.jpg", m_leftImg, AsyncLoadPriority.Middile);

        
    }

    public void OnStartBtnClick()
    {
        Debug.Log("点击开始按钮");
    }

    public void OnExitBtnClick()
    {
        Debug.Log("点击退出按钮");
    }

    public override void OnShow(params object[] paramArray)
    {
        base.OnShow(paramArray);
        UIManager.Instance.AsyncLoadSprite("Assets/GameData/Textures/loading4.jpg", m_rightImg, AsyncLoadPriority.High);

    }

}

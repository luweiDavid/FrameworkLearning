using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BaseView {
    public Button m_startBtn;
    public Button m_exitBtn;

    public override void Init(params object[] paramArray)
    {
        base.Init(paramArray); 

        m_startBtn = m_gameObj.transform.Find("StartBtn").GetComponent<Button>();
        m_exitBtn = m_gameObj.transform.Find("ExitBtn").GetComponent<Button>();
         
        m_startBtn.onClick.AddListener(OnStartBtnClick);
        m_exitBtn.onClick.AddListener(OnExitBtnClick);
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

    }

}

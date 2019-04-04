using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseView
{

    public GameObject m_gameObj;

    public RectTransform m_trans;

    public virtual void Init(params object[] paramArray)
    {
        //m_trans.localScale = Vector3.one;
        //m_trans.localRotation = Quaternion.identity;
         
    }

    public virtual void OnShow(params object[] paramArray)
    {
        if (!m_gameObj.activeInHierarchy) {
            m_gameObj.SetActive(true);
        }
    }

    public virtual void OnDisable()
    {
        if (m_gameObj.activeInHierarchy) {
            m_gameObj.SetActive(false);
        }
    }
    public virtual void OnDestroy() { }

}

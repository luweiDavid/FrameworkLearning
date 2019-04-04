using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineDataBase : MonoBehaviour {

    public Rigidbody m_rigBody;
    public Collider m_collider;

    public Transform[] m_allTransArray;
    public bool[] m_allTransActiveStateArray;
    public Vector3[] m_allPosArray;
    public Quaternion[] m_allRotArray;
    public Vector3[] m_allScaleArray;
    
    public virtual void BindData(GameObject gameObj) {
        m_rigBody = gameObj.GetComponent<Rigidbody>();
        m_collider = gameObj.GetComponent<Collider>();

        m_allTransArray = gameObj.GetComponentsInChildren<Transform>();
        int count = m_allTransArray.Length;
        m_allTransActiveStateArray = new bool[count];
        m_allPosArray = new Vector3[count];
        m_allRotArray = new Quaternion[count];
        m_allScaleArray = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            m_allTransActiveStateArray[i] = m_allTransArray[i].gameObject.activeInHierarchy;
            m_allPosArray[i] = m_allTransArray[i].localPosition;
            m_allRotArray[i] = m_allTransArray[i].localRotation;
            m_allScaleArray[i] = m_allTransArray[i].localScale;
        }
    }

    public virtual void ResetData() {
        int count = m_allTransArray.Length;
        for (int i = 0; i < count; i++)
        {
            if (m_allTransArray[i] != null) {
                m_allTransArray[i].localPosition = m_allPosArray[i];
                m_allTransArray[i].localRotation = m_allRotArray[i];
                m_allTransArray[i].localScale = m_allScaleArray[i];

                if (m_allTransActiveStateArray[i])
                {
                    if (!m_allTransArray[i].gameObject.activeSelf)
                    {
                        m_allTransArray[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (m_allTransArray[i].gameObject.activeSelf)
                    {
                        m_allTransArray[i].gameObject.SetActive(false);
                    }
                }
            } 
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOfflineData : OfflineDataBase {

    public Vector2[] m_anchorMinArray;
    public Vector2[] m_anchorMaxArray;
    public Vector3[] m_anchoredPosArray;
    public Vector2[] m_sizeDeltaArray;
    public Vector2[] m_pivotArray;
    public ParticleSystem[] m_particleSysArray;

    public override void BindData(GameObject gameObj)
    {
        m_allTransArray = gameObj.GetComponentsInChildren<RectTransform>();
        int count = m_allTransArray.Length;
        m_allTransActiveStateArray = new bool[count];
        m_anchorMinArray = new Vector2[count];
        m_anchorMaxArray = new Vector2[count];
        m_anchoredPosArray = new Vector3[count];
        m_sizeDeltaArray = new Vector2[count];
        m_pivotArray = new Vector2[count];
        m_particleSysArray = new ParticleSystem[count];

        for (int i = 0; i < count; i++)
        {
            RectTransform rectTr = m_allTransArray[i] as RectTransform;
            if (rectTr == null) return;

            m_allTransActiveStateArray[i] = rectTr.gameObject.activeInHierarchy;
            m_anchorMinArray[i] = rectTr.anchorMin;
            m_anchorMaxArray[i] = rectTr.anchorMax;
            m_anchoredPosArray[i] = rectTr.anchoredPosition;
            m_sizeDeltaArray[i] = rectTr.sizeDelta;
            m_pivotArray[i] = rectTr.pivot;
            m_particleSysArray[i] = rectTr.GetComponent<ParticleSystem>();
        }
    }

    public override void ResetData()
    {

        for (int i = 0; i < m_allTransArray.Length; i++)
        {
            if (m_allTransArray[i] != null) {
                RectTransform rectTr = m_allTransArray[i] as RectTransform;
                if (rectTr == null) return;

                rectTr.anchorMin = m_anchorMinArray[i];
                rectTr.anchorMax = m_anchorMaxArray[i];
                rectTr.anchoredPosition = m_anchoredPosArray[i];
                rectTr.sizeDelta = m_sizeDeltaArray[i];
                rectTr.pivot = m_pivotArray[i]; 

                if (m_allTransActiveStateArray[i])
                {
                    if (!rectTr.gameObject.activeSelf) {
                        rectTr.gameObject.SetActive(true);
                    }
                }
                else {
                    if (rectTr.gameObject.activeSelf)
                    {
                        rectTr.gameObject.SetActive(false);
                    }
                }
                if (m_particleSysArray[i] != null) {
                    m_particleSysArray[i].Clear();
                    m_particleSysArray[i].Play();
                }

            }
        }
    }
}

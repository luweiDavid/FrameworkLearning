using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOffilineData : OfflineDataBase {
    public ParticleSystem[] m_particleSysArray;
    public LineRenderer[] m_lineRenArray;

    public override void BindData(GameObject gameObj)
    {
        m_particleSysArray = gameObj.GetComponentsInChildren<ParticleSystem>();
        m_lineRenArray = gameObj.GetComponentsInChildren<LineRenderer>();
    }

    public override void ResetData()
    {
       
    }
}

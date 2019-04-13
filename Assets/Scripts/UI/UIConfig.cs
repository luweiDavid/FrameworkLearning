
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIConfig:Singleton<UIConfig> {
    public const string UIPrefabRootPath = "Assets/GameData/Prefabs/UI/";

    public Dictionary<UIPanelName, UIPanelConfig> m_panelDic = new Dictionary<UIPanelName, UIPanelConfig>()
    {
        { UIPanelName.MainPanel,
          new UIPanelConfig().Init("MainPanel",
                                   "MainPanel",
                                   UIPrefabRootPath+"Main/MainPanel.prefab",
                                   UILayer.NormalLayer
                                   )
        },

    };
}

//可以通过json来保存面板和对应的数据的字典，比如 [UIPanelName.MainPanel] = { Name = "", Path = "", Layer = UILayer.None }

public enum UIPanelName {
    None,
    MainPanel,
}
 

public class UIPanelConfig {
    
    public string PrefabName;
    public string ClassName;
    public string Path;
    public UILayer Layer;
    private string suffix = ".prefab";

    public UIPanelConfig Init(string wname,string cname,string path, UILayer layer) {
        this.PrefabName = wname + suffix;
        this.ClassName = cname;
        this.Path = path;
        this.Layer = layer;

        return this;
    }
}

public enum UIMsgName {
    
}


public enum UILayer
{
    None,
    BaseLayer,
    NormalLayer,
    TipsLayer,
    TopLayer,
}

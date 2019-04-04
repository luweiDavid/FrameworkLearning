using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OfflineDataEditor : Editor {

    [MenuItem("Assets/生成Prefab离线数据")]
    public static void CreateOfflineData() {
        GameObject[] objsArray = Selection.gameObjects;

        for (int i = 0; i < objsArray.Length; i++)
        {
            OfflineDataBase dataBase = objsArray[i].GetComponent<OfflineDataBase>();
            if (dataBase == null) {
                dataBase = objsArray[i].AddComponent<OfflineDataBase>();
            }
            dataBase.BindData(objsArray[i]);
            EditorUtility.DisplayProgressBar("生成prefab离线数据", objsArray[i].name, i * 1.0f / objsArray.Length);
            EditorUtility.SetDirty(objsArray[i]);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("生成prefab离线数据完成。。");
    }

    [MenuItem("Assets/生成UI离线数据")]
    public static void CreateUIOfflineData() {
        GameObject[] objsArray = Selection.gameObjects;

        for (int i = 0; i < objsArray.Length; i++)
        {
            UIOfflineData uiData = objsArray[i].GetComponent<UIOfflineData>();
            if (uiData == null)
            {
                uiData = objsArray[i].AddComponent<UIOfflineData>();
            }
            uiData.BindData(objsArray[i]);
            EditorUtility.DisplayProgressBar("生成UI离线数据", objsArray[i].name, i * 1.0f / objsArray.Length);
            EditorUtility.SetDirty(objsArray[i]);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Debug.Log("生成UI离线数据完成。。");
    }

}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class AssetBundleEditor : Editor {
    private static string m_abDataBaseBytesPath = "Assets/Scripts/Data/ABDataBase.bytes";

    public static string ABCONFIGASSETSPATH = "Assets/Editor/ABConfig.asset";

    public static string m_AssetBundleTargetPath = Application.streamingAssetsPath;

    //key值为ABName， value为路径Path
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    //需要过滤的path   (因为ab打包的逻辑是：先处理文件夹的，再处理单个文件的，所以
    //如果处理完文件夹的资源后，如果后面打包单个prefab文件时，其中的依赖项已经在前面加过了，就需要过滤掉，避免重复加入)
    public static List<string> m_filterPathList = new List<string>();
    //单个prefab对应的所有依赖项
    public static Dictionary<string, List<string>> m_singlePrefabAllDepPathDic = new Dictionary<string, List<string>>();

    [MenuItem("Tools/Build")]
    public static void Build()
    { 
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGASSETSPATH);

        m_AllFileDir.Clear();
        m_filterPathList.Clear();
        m_singlePrefabAllDepPathDic.Clear();
        foreach (AllFileDirABName namePath in abConfig.m_AllFileDirAB) { 
            if (m_AllFileDir.ContainsKey(namePath.ABName))
            {
                Debug.LogError("名称重复，请检查!");
            }
            else {
                m_AllFileDir.Add(namePath.ABName, namePath.Path);
                m_filterPathList.Add(namePath.Path);
            } 
        }

        //获取m_AllPrefabPath文件夹下的所有文件（包括子文件夹的）
        string[] prefabGuidArray = AssetDatabase.FindAssets("t:prefab", abConfig.m_AllPrefabPath.ToArray()); 
        for (int i = 0; i < prefabGuidArray.Length;i++) {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);
            EditorUtility.DisplayProgressBar("查找path", "prefab:" + path, (float)i / prefabGuidArray.Length);

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            //查找到prefab的路径后，检索出所有dependence（依赖项）
            string[] tempDependenceArray = AssetDatabase.GetDependencies(path); 
            List<string> tempList = new List<string>();
            if (!FilterPath(path)) {
                for (int j = 1; j < tempDependenceArray.Length; j++) {
                    if (!FilterPath(tempDependenceArray[j])&&!tempDependenceArray[j].EndsWith(".cs")) {
                        m_filterPathList.Add(tempDependenceArray[j]); 
                        tempList.Add(tempDependenceArray[j]);
                    }
                }
            }
            if (m_singlePrefabAllDepPathDic.ContainsKey(go.name))
            {
                Debug.LogError("存在相同名称的prefab：" + go.name);
            }
            else {
                m_singlePrefabAllDepPathDic.Add(go.name, tempList);
            }  
        } 

        foreach (string name in m_AllFileDir.Keys) {
            SetABName(name, m_AllFileDir[name]);
        }
        foreach (string name in m_singlePrefabAllDepPathDic.Keys) {
            SetABName(name, m_singlePrefabAllDepPathDic[name]);
        } 

        BuildAssetBundle();
        //清空所有abname
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < names.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(names[i], true);
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();

        //以下两行代码是刷新编辑器的，文件的meta文件会被刷新
        //AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();
    }

    public static void BuildAssetBundle() {
        #region   生成配置表
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();   //直接得到不带后缀的包名
        //key为全路径，value为bundlename
        Dictionary<string, string> tempDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++) { 
            string[] bundlePathArray = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < bundlePathArray.Length; j++) {
                //Debug.Log(allBundles[i] + "_______" + bundlePathArray[j]);
                tempDic.Add(bundlePathArray[j], allBundles[i]);
            }
        }

        //这里还有一个步骤，就是如果之前资源的一些属性改变了（比如名字），那么如果之前已经打包过了，如果重新打包时，
        //不将这些属性改变了的资源的ab包删除掉，就会产生冗余，所以打包之前先判断
        //判断的逻辑是：已经设置的ab包名 跟 已经打包好的ab包 进行比较，如果设置的ab包名中不包含当前存在的ab包名，则删除
        DeleteNotExistABName();

        //生成配置表
        CreateConfigTable(tempDic);
        #endregion

        //执行打包
        BuildPipeline.BuildAssetBundles(m_AssetBundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }

    public static void CreateConfigTable(Dictionary<string,string> tempDic) {
        AssetBundleData abData = new AssetBundleData();
        abData.abDataBaseList = new List<AssetBundleDataBase>();
        foreach (string path in tempDic.Keys) {
            AssetBundleDataBase abBase = new AssetBundleDataBase();
            abBase.Path = path;
            abBase.Crc = Crc32.GetCRC32(path);
            abBase.ABName = tempDic[path]; 
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.DependenceList = new List<string>();
            string[] tempDependenceArray = AssetDatabase.GetDependencies(path);    //得到的是相对路径
            for (int i = 0; i < tempDependenceArray.Length; i++) {
                if (tempDependenceArray[i] == path) {
                    continue;
                }
                string dependenceABName = "";
                if(tempDic.TryGetValue(tempDependenceArray[i], out dependenceABName))
                {
                    if (!abBase.DependenceList.Contains(dependenceABName)) {  //一个prefab可能用到多次同一种资源，所以这里应该排除，避免重复添加依赖
                        abBase.DependenceList.Add(dependenceABName);
                    }
                }
            }
            abData.abDataBaseList.Add(abBase);
        }

        //xml
        FileStream xmlfs = new FileStream(Application.dataPath + "/abBaseList.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(xmlfs, System.Text.Encoding.UTF8);
        XmlSerializer xmls = new XmlSerializer(typeof(AssetBundleData));

        //binary
        FileStream binaryfs = new FileStream(m_abDataBaseBytesPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();

        //注意序列化时的对象类型要跟反序列化时的一致
        #region   关于xml序列化的报错
        //报错1： the type of the argument object "AssetBundleData" is not primitive 参数对象不是原始的  
        //报错解析：要序列化的对象跟传入的对象不一致
        //原因：1.(typeof(List<AssetBundleDataBase>))  (typeof(AssetBundleData)) 类型问题 
        xmls.Serialize(sw, abData);
        bf.Serialize(binaryfs, abData);

        #endregion

        sw.Close();
        xmlfs.Close();
        binaryfs.Close();
    }

    public static void DeleteNotExistABName() {
        string[] allbundles = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo dirInfo = new DirectoryInfo(m_AssetBundleTargetPath);
        FileInfo[] filesArray = dirInfo.GetFiles("*");
        foreach (FileInfo info in filesArray) {
            if (!info.FullName.EndsWith(".meta") && !IsExistInABName(info.Name, allbundles))
            {
                if (File.Exists(info.FullName))
                {
                    File.Delete(info.FullName);
                }
            }
        } 
    }

    public static bool IsExistInABName(string name, string[] strArray) {
        for (int i = 0; i < strArray.Length; i++) {
            if (name == strArray[i]) {
                return true;
            }
        }
        return false;
    }

    public static void SetABName(string name, string path) {
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError("不存在此路径：" + path);
        }
        else { 
            importer.assetBundleName = name; 
        }
    }

    public static void SetABName(string name,List<string> pathList) {
        for (int i = 0; i < pathList.Count; i++) {
            SetABName(name, pathList[i]);
        }
    }

    public static bool FilterPath(string path)
    {
        //Assets/GameData/Test                  1
        //Assets/GameData/TestTT/a.shader       2
        //像上面的情况，路径2是包含路径1的，但这种情况不能过滤掉路径1
        for (int i = 0; i < m_filterPathList.Count; i++) {
            if (path == m_filterPathList[i]|| path.Contains(m_filterPathList[i])){
                if (path.Replace(m_filterPathList[i], "")[0] == '/') {
                    return true;
                }
            }
        }
        return false;
    }

}

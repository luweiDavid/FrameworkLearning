using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class AppBuildEditor 
{
    private static bool isNull = false;

    [MenuItem("Tools/BuildApp")]
    public static void BuildApp() {
        AssetBundleEditor.Build();

        CopyABToStreamAssetsPath();
        CleanBuildTargetSubDirectory();

        string buildPath = GetBuildPath(); 

        BuildPipeline.BuildPlayer(GetEditorScenesPathArray(), buildPath, EditorUserBuildSettings.activeBuildTarget,BuildOptions.None);
    }

    private static string[] GetEditorScenesPathArray() {
        List<string> sceneList = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
            if (scene == null) {
                continue;
            }
            sceneList.Add(scene.path);
        }
        return sceneList.ToArray();
    }

    private static string GetBuildPath() {
        string buildPath = "";
        string tmpPath = Application.dataPath + "/../" + "BuildTarget/";
        string programName = PlayerSettings.productName;
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
            buildPath = tmpPath + "Android/" + programName + "_" + EditorUserBuildSettings.
                activeBuildTarget.ToString() + "_"  + ".apk";
        }
        else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS){
            buildPath = tmpPath + "IOS/" + programName + "_" + EditorUserBuildSettings.activeBuildTarget
                .ToString();
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            buildPath = tmpPath + "Windows/" + programName + "_" + EditorUserBuildSettings.activeBuildTarget
                .ToString() + ".exe";
        }
        return buildPath;
    }

    private static void CopyABToStreamAssetsPath() {
        string tarPath = Application.streamingAssetsPath;
        string srcPath = PathConfig.Instance.GetABTargetPath(); 
        CleanDirectory(tarPath);
        CopyAllFiles(srcPath, tarPath); 
    }

    private static void CopyAllFiles(string srcPath, string targetPath) {
        try
        {
            DirectoryInfo sourceDirInfo = new DirectoryInfo(srcPath);
            DirectoryInfo targetDirInfo = new DirectoryInfo(targetPath);

            if (targetDirInfo.FullName.StartsWith(sourceDirInfo.FullName, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("父目录不能拷贝到子目录");
            }

            if (!Directory.Exists(targetPath)) {
                Directory.CreateDirectory(targetPath);
            } 
              
            FileInfo[] fileInfoArray = sourceDirInfo.GetFiles();
            DirectoryInfo[] dirInfoArray = sourceDirInfo.GetDirectories();
            if (fileInfoArray.Length <= 0 && dirInfoArray.Length <= 0) {
                Debug.LogError(string.Format("当前目录{0}不存在任何文件和文件夹", srcPath));
                isNull = true;
                return;
            } 
            for (int i = 0; i < fileInfoArray.Length; i++)
            { 
                File.Copy(fileInfoArray[i].FullName, Path.Combine(targetDirInfo.FullName, fileInfoArray[i].Name), true);
            }

            for (int i = 0; i < dirInfoArray.Length; i++)
            {
                CopyAllFiles(dirInfoArray[i].FullName, Path.Combine(targetDirInfo.FullName, dirInfoArray[i].Name));
            }
               
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            //Debug.LogError(string.Format("复制文件失败! 原路径：{0}， 目标路径：{1}", srcPath, targetPath));
        }
    }

    private static void CleanDirectory(string targetPath) {
        if (string.IsNullOrEmpty(targetPath)) {
            Debug.LogError("目标路径为null");
            return;
        }

        foreach (var subDir in Directory.GetDirectories(targetPath))
        {
            Directory.Delete(subDir, true);
        }
        foreach (var subFile in Directory.GetFiles(targetPath))
        {
            File.Delete(subFile);
        }
    }


    private static void CleanBuildTargetSubDirectory() {
        string buildPath = "";
        string tmpPath = Application.dataPath + "/../" + "BuildTarget/";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            buildPath = tmpPath + "Android/";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            buildPath = tmpPath + "IOS/";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            buildPath = tmpPath + "Windows/";
        }

        CleanDirectory(buildPath);
    }

    //------------------------------------- TEST -------------------

    //[MenuItem("Tools/Test/TestFileCopy")]
    //public static void TestCopy()
    //{
    //    string srcPath = Application.dataPath + "/../" + "test1/";
    //    string tarPath = Application.dataPath + "/../" + "test2/";
    //    if (!Directory.Exists(srcPath))
    //    {
    //        Debug.Log(string.Format("原文件夹不存在:{0}" + srcPath));
    //        return;
    //    }
    //    CleanDirectory(tarPath);
    //    CopyAllFiles(srcPath, tarPath);
    //}


}

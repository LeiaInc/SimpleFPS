using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerLogOpener
{
    [MenuItem("Debug/Open Player.log")]
    private static void OpenOutputLog()
    {
        string filename = "C:/Users/" + Environment.UserName + "/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/Player.log";

        Debug.Log("Attempting to open " + filename);

        if (File.Exists(filename))
        {
            Application.OpenURL(filename);
        }
        else
        {
            Debug.Log("Failed to open file");
        }
    }


    [MenuItem("Debug/Open Builds Folder")]
    private static void OpenBuildsFolder()
    {
        string filename = Application.dataPath.Replace("Assets", "Builds");

        Debug.Log("Attempting to open " + filename);

        ShowExplorer(filename);
        /*
            if (File.Exists(filename))
            {
                //Application.OpenURL(filename);
            }
            else
            {
                Debug.Log("Failed to open file");
            }*/
    }
    public static void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }
}

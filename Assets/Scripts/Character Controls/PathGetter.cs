using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PathGetter : EditorWindow
    {
    private static string m_ScriptFilePath;
    private static string m_ScriptFolder;

    public static string getScriptFolderPath(MonoBehaviour script)
    {
        MonoScript ms = MonoScript.FromMonoBehaviour( script );
        m_ScriptFilePath = AssetDatabase.GetAssetPath( ms );

        FileInfo fi = new FileInfo( m_ScriptFilePath);
        m_ScriptFolder = fi.Directory.ToString();
        m_ScriptFolder.Replace( '\\', '/');

        Debug.Log( m_ScriptFolder );

        return m_ScriptFolder;
    }
}
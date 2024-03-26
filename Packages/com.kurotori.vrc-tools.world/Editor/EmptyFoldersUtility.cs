using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KurotoriTools
{
    public class EmptyFoldersUtility : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, bool> emptyFolders = new Dictionary<string, bool>();

        [MenuItem("KurotoriTools/Empty Folders Utility")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EmptyFoldersUtility), false, "Empty Folders Utility");
        }

        private void OnGUI()
        {
            KurotoriUtility.GUITitle("Empty Folder Find", "プロジェクト内の空フォルダを見つけて削除できる");

            if (GUILayout.Button("Find Empty Folders"))
            {
                FindEmptyFolders();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var folder in emptyFolders.Keys.ToList())
            {
                emptyFolders[folder] = EditorGUILayout.ToggleLeft(folder, emptyFolders[folder]);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Delete Selected Folders"))
            {
                DeleteSelectedFolders();
            }
        }

        private void FindEmptyFolders()
        {
            emptyFolders.Clear();
            var allFolders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
            foreach (var folder in allFolders)
            {
                if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0)
                {
                    var relativePath = "Assets" + folder.Substring(Application.dataPath.Length);
                    emptyFolders.Add(relativePath, false);
                }
            }
        }

        private void DeleteSelectedFolders()
        {
            foreach (var folder in emptyFolders.Where(f => f.Value).ToList())
            {
                if (AssetDatabase.DeleteAsset(folder.Key))
                {
                    Debug.Log("Deleted empty folder: " + folder.Key);
                }
                else
                {
                    Debug.LogError("Failed to delete folder: " + folder.Key);
                }
            }

            FindEmptyFolders();
        }
    }
}
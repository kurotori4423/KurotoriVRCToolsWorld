using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KurotoriTools
{
    /// <summary>
    /// プロジェクトファイルのブックマーク機能
    /// </summary>
    public class ProjectBookmark : EditorWindow
    {
        private ProjectBookmarkData bookmarkData;

        [MenuItem("KurotoriTools/Bookmark Window")]
        public static void ShowWindow()
        {
            GetWindow<ProjectBookmark>("Project Bookmark");
        }

        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                bookmarkData = (ProjectBookmarkData)EditorGUILayout.ObjectField("Bookmark Data", bookmarkData, typeof(ProjectBookmarkData), false);
            }

            if (bookmarkData == null)
            {
                EditorGUILayout.HelpBox("BookmarkData is not set. Please assign a BookmarkData object.", MessageType.Warning);
                var bookmark = AssetDatabase.LoadAssetAtPath<ProjectBookmarkData>("Assets/BookmarkData.asset");

                if (bookmark)
                {
                    bookmarkData = bookmark;
                }
                else
                {
                    // 見つからない場合は新たに生成する
                    var newBookmark = new ProjectBookmarkData();
                    AssetDatabase.CreateAsset(newBookmark, "Assets/BookmarkData.asset");
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                if (GUILayout.Button("選択オブジェクト・フォルダをブックマーク"))
                {
                    SaveCurrentSelection();
                }

                EditorGUILayout.Space();

                // Missingになっているブックマークがあれば削除する
                bookmarkData.bookmarks.RemoveAll(item => item == null);


                if (bookmarkData.bookmarks != null)
                {
                    Object m_revomeObject = null;

                    foreach (var bookmark in bookmarkData.bookmarks)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            var guiStyleLarge = new GUIStyle();
                            var guiStyleSmall = new GUIStyle();

                            // 大きい文字用のスタイル設定
                            guiStyleLarge.fontSize = 18;
                            guiStyleLarge.normal.textColor = Color.white;
                            guiStyleLarge.alignment = TextAnchor.UpperLeft;

                            // 小さい文字用のスタイル設定
                            guiStyleSmall.fontSize = 12;
                            guiStyleSmall.normal.textColor = Color.white;
                            guiStyleSmall.alignment = TextAnchor.LowerLeft;

                            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, GUILayout.Height(36));
                            if (GUI.Button(buttonRect, GUIContent.none))
                            {
                                Selection.activeObject = bookmark;
                            }

                            // ボタンのテキストをカスタム描画
                            GUI.Label(buttonRect, " " + bookmark.name, guiStyleLarge);
                            GUI.Label(buttonRect, " " + AssetDatabase.GetAssetPath(bookmark), guiStyleSmall);

                            Rect buttonRectClose = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, GUILayout.Height(36), GUILayout.Width(30));

                            GUI.backgroundColor = Color.red;
                            if (GUI.Button(buttonRectClose, "X"))
                            {
                                m_revomeObject = bookmark;
                            }
                            GUI.backgroundColor = Color.white;
                        }

                    }

                    bookmarkData.bookmarks.Remove(m_revomeObject);
                }


            }


        }

        private void SaveCurrentSelection()
        {
            if (bookmarkData != null)
            {
                if (bookmarkData.bookmarks == null)
                {
                    bookmarkData.bookmarks = new List<Object>();
                }

                var path = Selection.GetFiltered<DefaultAsset>(SelectionMode.DeepAssets).Select(x => AssetDatabase.GetAssetPath(x))
                    .Where(x => AssetDatabase.IsValidFolder(x))
                    .ToArray();

                if (path.Length != 0)
                {
                    var rootPath = path.OrderBy(s => s.Length).FirstOrDefault();

                    var folderAssets = AssetDatabase.LoadAssetAtPath<Object>(rootPath);

                    if (!bookmarkData.bookmarks.Contains(folderAssets))
                    {
                        bookmarkData.bookmarks.Add(folderAssets);
                    }
                }
                else
                {
                    if (Selection.activeObject != null)
                    {
                        if (!bookmarkData.bookmarks.Contains(Selection.activeObject))
                        {
                            bookmarkData.bookmarks.Add(Selection.activeObject);
                        }
                    }
                    else
                    {
                        Debug.Log("No Selection");
                    }
                }

                EditorUtility.SetDirty(bookmarkData); // Mark the bookmark data as dirty so it gets saved
            }
        }
    }
}
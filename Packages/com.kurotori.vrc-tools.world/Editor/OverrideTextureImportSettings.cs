using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace KurotoriTools
{
    /// <summary>
    /// 選択したフォルダ以下の画像のテクスチャ圧縮設定を変更する
    /// </summary>
    public class OverrideTextureImportSettings : EditorWindow
    {
        private bool useCrunchCompression = true;
        private TextureImporterCompression compression = TextureImporterCompression.Compressed;

        [MenuItem("KurotoriTools/Texture Import Settings")]
        public static void ShowWindow()
        {
            GetWindow<OverrideTextureImportSettings>("Texture Import Settings");
        }

        private void OnGUI()
        {
            GUILayout.Label("Batch Texture Import Settings", EditorStyles.boldLabel);

            useCrunchCompression = EditorGUILayout.Toggle("Use Crunch Compression", useCrunchCompression);
            compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", compression);

            if (GUILayout.Button("Apply Settings to Selected Folder"))
            {
                ApplySettingsToSelectedFolder();
            }
        }

        private void ApplySettingsToSelectedFolder()
        {
            // プロジェクトビューで選択されたフォルダを取得
            var selectedObject = Selection.activeObject;
            string targetFolder = AssetDatabase.GetAssetPath(selectedObject);

            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                Debug.LogError("Please select a valid folder in the Project view.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { targetFolder });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (textureImporter != null)
                {
                    textureImporter.crunchedCompression = useCrunchCompression;
                    textureImporter.textureCompression = compression;

                    EditorUtility.SetDirty(textureImporter);
                    textureImporter.SaveAndReimport();

                    Debug.Log($"Applied settings to {assetPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
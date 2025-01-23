using System.IO;
using System.Linq;
using UnityEngine;

namespace KurotoriTools
{

#if UNITY_EDITOR

    using UnityEditor;

    /// <summary>
    /// Unityパッケージのエクスポートを簡略化するスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "PackageExporter", menuName = "ScriptableObjects/PackageExporter")]
    public class UnityPackageExporter : ScriptableObject
    {
        [SerializeField] public string DefaultExportFolder = "";
        [SerializeField] public string DefaultFileName = "package";
        [SerializeField] public Object[] objects;

    }


    [CustomEditor(typeof(UnityPackageExporter))]
    public class UnityPackageExporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var targetPackage = (UnityPackageExporter)target;
            base.OnInspectorGUI();

            if (GUILayout.Button("Export unityPackage"))
            {
                const string ext = "unitypackage";

                var openDirectoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetPackage));

                if(Directory.Exists(targetPackage.DefaultExportFolder))
                {
                    openDirectoryPath = targetPackage.DefaultExportFolder;
                }

                var path = EditorUtility.SaveFilePanel("Export unitypackage",
                    openDirectoryPath,
                    targetPackage.DefaultFileName + "." + ext, ext);

                if(path.Length != 0)
                    Export(targetPackage, path);
            }
        }

        public static void Export(UnityPackageExporter package, string path)
        {
            EditorUtility.DisplayProgressBar("Export Package", "Export Package", 0.5f);

            var exportObjects = package.objects.Select(e => AssetDatabase.GetAssetPath(e)).ToList();

            AssetDatabase.ExportPackage(
                exportObjects.ToArray(),
                path,
                ExportPackageOptions.Recurse);
            EditorUtility.ClearProgressBar();

        }
    }

#endif
}
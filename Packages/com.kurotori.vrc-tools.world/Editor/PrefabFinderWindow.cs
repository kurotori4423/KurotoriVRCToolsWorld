using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KurotoriTools
{
    /// <summary>
    /// Prefabを見つけるEditorWindow
    /// https://qiita.com/r-ngtm/items/9bbe6926b25a97a05488#%E6%89%8B%E9%A0%861-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%82%92%E4%BD%9C%E6%88%90
    /// </summary>
    public class PrefabFinderWindow : EditorWindow
    {
        [SerializeField] private string baseClassName;

        [MenuItem("KurotoriTools/Prefab Finder")]
        static void Open()
        {
            GetWindow<PrefabFinderWindow>("Prefab Finder");
        }

        void OnGUI()
        {

            KurotoriUtility.GUITitle("Prefab Finder", "指定したスクリプトが付いているPrefabを選択する");

            EditorGUILayout.LabelField("クラス名");
            this.baseClassName = EditorGUILayout.TextField(this.baseClassName);

            if (GUILayout.Button("Find"))
            {
                Selection.objects = GetPrefabs(this.baseClassName).ToArray();
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>
        /// 指定したクラスがアタッチされているPrefabを検索して取得する
        /// </summary>
        static IEnumerable<UnityEngine.Object> GetPrefabs(string typeName)
        {
            var guids = AssetDatabase.FindAssets("t:prefab", null);
            foreach (var guid in guids)
            {
                var classTypes = TypeGetter.GetComponentTypes(typeName);
                if (classTypes == null) { continue; }

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.GameObject)) as GameObject;
                if (go == null) { continue; }

                var components = classTypes.SelectMany(classType => go.GetComponents(classType))
                .Where(component => component != null)
                .Distinct()
                .ToArray();

                if (components != null && components.Length != 0)
                {
                    yield return go;
                }
            }
        }
    }
}
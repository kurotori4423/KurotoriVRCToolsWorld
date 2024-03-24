namespace PrefabFinder
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// タイプの取得を行うクラス
    /// </summary>
    public static class TypeGetter
    {
        private static Dictionary<string, List<Type>> typeDict;

        private static MonoScript[] monoScripts;

        /// <summary>
        /// プロジェクト内に存在する全スクリプトファイル
        /// </summary>
        private static MonoScript[] MonoScripts { get { return monoScripts ?? (monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>().ToArray()); } }

        /// <summary>
        /// 指定した名前と同名のクラスを全て取得する
        /// </summary>
        public static Type[] GetComponentTypes(string className)
        {
            // Dictionary作成
            if (typeDict == null)
            {
                typeDict = new Dictionary<string, List<Type>>();
                foreach (var type in GetAllTypes())
                {
                    if (!type.IsSubclassOf(typeof(Component))) { continue; } // Component継承クラスかどうか判定

                    if (!typeDict.ContainsKey(type.Name))
                    {
                        typeDict.Add(type.Name, new List<Type>());
                    }
                    typeDict[type.Name].Add(type);
                }
            }

            if (typeDict.ContainsKey(className))
            {
                return typeDict[className].ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 全てのクラスタイプを取得
        /// </summary>
        private static IEnumerable<Type> GetAllTypes()
        {
            // Unity標準のクラスタイプ
            var buitinTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(type => type != null && !string.IsNullOrEmpty(type.Namespace))
            .Where(type => type.Namespace.Contains("UnityEngine"));

            // 自作のクラスタイプ
            var myTypes = MonoScripts
            .Where(script => script != null)
            .Select(script => script.GetClass())
            .Where(classType => classType != null)
            .Where(classType => classType.Module.Name == "Assembly-CSharp.dll");

            return buitinTypes.Concat(myTypes)
            .Distinct();
        }
    }
}
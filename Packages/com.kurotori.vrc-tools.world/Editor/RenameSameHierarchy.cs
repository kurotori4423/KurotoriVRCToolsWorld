using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 選択したオブジェクト以下のオブジェクトで同一名同一階層のオブジェクトに通し番号を入れる。
/// </summary>

public class RenameSameHierarchy
{

    public static void CreateHierarchyMap(Transform parentObject,ref Dictionary<string, List<Transform>> hierarchyMap)
    {
        Transform children = parentObject.GetComponentInChildren<Transform>();

        string path = "/" + parentObject.name;

        if(children.childCount > 0)
        {
            foreach(Transform child in children)
            {
                CreateChildHierarchyMap(path, child, ref hierarchyMap);
            }
        }
    }

    private static void CreateChildHierarchyMap(string path, Transform parentObject, ref Dictionary<string, List<Transform>> hierarchyMap)
    {
        string newPath = path + "/" + parentObject.name;

        if(hierarchyMap.ContainsKey(newPath))
        {
            hierarchyMap[newPath].Add(parentObject);
        }
        else
        {
            var newList = new List<Transform>();
            newList.Add(parentObject);
            hierarchyMap.Add(newPath, newList);
        }

        Transform children = parentObject.GetComponentInChildren<Transform>();
        if (children.childCount > 0)
        {
            foreach (Transform child in children)
            {
                CreateChildHierarchyMap(newPath, child, ref hierarchyMap);
            }
        }
    }

    private static int RecursiveRename(int renameCount, Transform topParent)
    {
        var nameDictionary = new Dictionary<string, List<Transform>>();
        Transform children = topParent.GetComponentInChildren<Transform>();
        foreach(Transform child in children){

            if (nameDictionary.ContainsKey(child.name))
            {
                nameDictionary[child.name].Add(child);
            }
            else
            {
                var childrenList = new List<Transform>();
                childrenList.Add(child);
                nameDictionary.Add(child.name, childrenList);
            }
        }
        
        foreach(KeyValuePair<string, List<Transform>> kvp in nameDictionary)
        {
            if (kvp.Value.Count > 1)
            {
                int count = 0;
                int digit = Digit(kvp.Value.Count) + 1;
                foreach (var target in kvp.Value)
                {
                    Undo.RecordObject(target.gameObject, "Rename Hierarchy");
                    target.name += string.Format("_{0:D" + digit + "}", count);

                    count++;
                    renameCount++;
                }
            }
        }

        foreach(Transform child in children)
        {
            renameCount = RecursiveRename(renameCount, child);
        }

        return renameCount;
    }

    public static int Digit(int num)
    {
        int digit = 1;
        for (int i = num; i >= 10; i /= 10)
        {
            digit++;
        }
        return digit;
    }

    [MenuItem("GameObject/同一階層のオブジェクトをリネーム", false, 11)]
    public static void RenameOnScene()
    {
        foreach (var parent in Selection.gameObjects)
        {
            int renameCount = RecursiveRename(0 , parent.transform);
            if (renameCount > 0)
            {
                Debug.Log(parent.name + "以下、" + renameCount + "個のオブジェクトをリネームしました。");
            }
        }

    }
    
    [MenuItem("Assets/同一階層のオブジェクトをリネーム", priority = 11)]
    public static void RenamePrefab()
    {

        foreach(var parent in Selection.gameObjects)
        {
            string prefabPath = AssetDatabase.GetAssetPath(parent);

            if (Path.GetExtension(prefabPath).Equals(".prefab"))
            {
                
                var contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);


                int renameCount = RecursiveRename(0,contentsRoot.transform);

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);

                if (renameCount > 0)
                {
                    Debug.Log(Path.GetFileName(prefabPath) + " 以下、" + renameCount + "個のオブジェクトをリネームしました。");
                }
            }
        }
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// オブジェクトの配置関係のユーティリティ群
/// </summary>
public class ObjectPlaceUtility
{
    [MenuItem("GameObject/Object Place/選択オブジェクトを一つ前のオブジェクトの位置へ", false, 11)]
    public static void FitPrevSelectObjectPosition()
    {
        if(Selection.gameObjects.Length == 2)
        {
            var target = Selection.gameObjects[1].transform;
            Undo.RegisterCompleteObjectUndo(target, "Fit Prev Select Object Position");
            target.position = Selection.gameObjects[0].transform.position;
        }
    }

    [MenuItem("GameObject/Object Place/子供のAnchorOverrideを親に設定", false, 11)]
    public static void SetAnchorOverrideParent()
    {
        var childrenMeshes = Selection.activeGameObject.GetComponentsInChildren<MeshRenderer>();

        Undo.RecordObjects(childrenMeshes, "Set Probe Anchor Parent");

        foreach (var mesh in childrenMeshes)
        {
            mesh.probeAnchor = Selection.activeGameObject.transform;
        }
    }

    [MenuItem("GameObject/Object Place/子供のオブジェクトを全削除", false)]
    public static void DeleteAllChildren()
    {

        Transform parent = Selection.activeGameObject.transform;

        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; ++i)
        {
            var child = parent.GetChild(i);

            children.Add(child);
        }

        foreach(var child in children)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }
}

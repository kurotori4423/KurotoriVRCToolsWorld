using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 選択したオブジェクトをHierarchyの先頭に持ってくる
/// </summary>
public class SetSiblingFirst
{
    [MenuItem("GameObject/オブジェクトを先頭に移動", false, 11)]
    public static void SetFirstSibling()
    {
        Selection.activeGameObject.transform.SetSiblingIndex(0);
    }
}

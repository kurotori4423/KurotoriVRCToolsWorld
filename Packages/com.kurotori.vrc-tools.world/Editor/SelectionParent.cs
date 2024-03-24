using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 選択したオブジェクトを指定したオブジェクトの子供にする
/// </summary>

public class SelectionParent : EditorWindow
{
    private GameObject parentObject;

    [MenuItem("KurotoriTools/SelectionParent")]
    static void Open()
    {
        var window = EditorWindow.GetWindow<SelectionParent>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Parent Object");
        parentObject = EditorGUILayout.ObjectField(parentObject, typeof(GameObject), true) as GameObject;
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField(string.Format("現在{0}個のオブジェクトが選択中", Selection.gameObjects.Length));

        if (GUILayout.Button("親子付けする"))
        {
            SelectionParenting();
        }


    }

    private void SelectionParenting()
    {
        foreach(var child in Selection.gameObjects)
        {
            child.transform.parent = parentObject.transform;
        }
    }
}
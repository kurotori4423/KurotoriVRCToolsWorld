using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectReplace : EditorWindow
{
    private GameObject basePrefab;

    [MenuItem("KurotoriTools/SelectReplace")]
    // Start is called before the first frame update
    static void Open()
    {
        var window = EditorWindow.GetWindow<SelectReplace>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Base Prefab");
        basePrefab = EditorGUILayout.ObjectField(basePrefab, typeof(GameObject), true) as GameObject;
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField(string.Format("現在{0}個のオブジェクトが選択中", Selection.gameObjects.Length));

        if (GUILayout.Button("入れ替える"))
        {
            ReplacePrefab();
        }


    }

    private void ReplacePrefab()
    {


        foreach(var child in Selection.gameObjects)
        {
            var pos = child.transform.position;
            var rot = child.transform.rotation;
            var scale = child.transform.localScale;
            var parent = child.transform.parent;
            


            GameObject newObject = PrefabUtility.InstantiatePrefab(basePrefab, parent) as GameObject;

            newObject.name = child.name;
            newObject.transform.position = pos;
            newObject.transform.rotation = rot;
            newObject.transform.localScale = scale;

            GameObject.DestroyImmediate(child);    

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// スタティック設定ショートカット
/// </summary>
public class SetStaticSettings
{
    public static void ChangeScaleInLightmap(MeshRenderer meshRenderer,float scale)
    {
        SerializedObject so = new SerializedObject(meshRenderer);
        so.FindProperty("m_ScaleInLightmap").floatValue = scale;
        so.ApplyModifiedProperties();
    }

    public static void ChangeStitchLightmapSeams(MeshRenderer meshRenderer, bool flag)
    {
        SerializedObject so = new SerializedObject(meshRenderer);
        so.FindProperty("m_StitchLightmapSeams").boolValue = flag;
        so.ApplyModifiedProperties();
    }

    private static void SetStaticProp(GameObject[] gameObjects)
    {
        Undo.RecordObjects(gameObjects, "Set Prop Static");

        foreach (var target in gameObjects)
        {

            var flags =
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.ReflectionProbeStatic;

            GameObjectUtility.SetStaticEditorFlags(target, flags);

            var meshRenderer = target.GetComponent<MeshRenderer>();

            if (meshRenderer)
            {
                ChangeScaleInLightmap(meshRenderer, 0);
            }
        }
    }

    [MenuItem("Assets/プロップ向けStaticSetting", false, 11)]
    public static void SetStaticPropPrefabs()
    {
        foreach (var assets in Selection.gameObjects)
        {
            string prefabPath = AssetDatabase.GetAssetPath(assets);

            if (Path.GetExtension(prefabPath).Equals(".prefab"))
            {
                var contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                // 全ての子オブジェクトを取得する
                var objectList = contentsRoot.GetComponentsInChildren<Transform>().Select(n => n.gameObject);
                
                SetStaticProp(objectList.ToArray());

                Debug.Log(assets.name + "以下の" + objectList.Count() + "個をプロップ向けStatic設定しました。");

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
    }

    [MenuItem("GameObject/プロップ向けStaticSetting", false, 11)]
    public static void SetStaticPropScene()
    {
        SetStaticProp(Selection.gameObjects);
    }

    [MenuItem("GameObject/プロップ向けStaticSetting階層", false, 11)]
    public static void SetStaticPropSceneHierarchy()
    {
        foreach(var target in Selection.gameObjects)
        {
            var objectList = target.GetComponentsInChildren<Transform>().Select(n => n.gameObject);
            SetStaticProp(objectList.ToArray());

            Debug.Log(target.name + "以下の" + objectList.Count() + "個をプロップ向けStatic設定しました。");
        } 
    }
    private static void SetStaticBGObject(GameObject[] gameObjects)
    {
        Undo.RecordObjects(gameObjects, "Set Backgorund Static");

        foreach (var target in gameObjects)
        {

            var flags =
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.ReflectionProbeStatic;

            GameObjectUtility.SetStaticEditorFlags(target, flags);

            var meshRenderer = target.GetComponent<MeshRenderer>();

            if (meshRenderer)
            {
                ChangeScaleInLightmap(meshRenderer, 1);
                ChangeStitchLightmapSeams(meshRenderer, true);
            }
        }
    }

    [MenuItem("Assets/背景オブジェクト用StaticSetting", false, 11)]
    public static void SetStaticBGObjectPrefabs()
    {
        foreach (var assets in Selection.gameObjects)
        {
            string prefabPath = AssetDatabase.GetAssetPath(assets);

            if (Path.GetExtension(prefabPath).Equals(".prefab"))
            {
                var contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                // 全ての子オブジェクトを取得する
                var objectList = contentsRoot.GetComponentsInChildren<Transform>().Select(n => n.gameObject);

                SetStaticBGObject(objectList.ToArray());

                Debug.Log(assets.name + "以下の" + objectList.Count() + "個を背景向けStatic設定しました。");

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
    }

    [MenuItem("GameObject/背景オブジェクト用StaticSetting", false, 11)]
    public static void SetStaticBGObjectScene()
    {
        SetStaticBGObject(Selection.gameObjects);
    }

    [MenuItem("GameObject/背景オブジェクト用StaticSetting階層", false, 11)]
    public static void SetStaticBGObjectSceneHierarchy()
    {
        foreach (var target in Selection.gameObjects)
        {
            var objectList = target.GetComponentsInChildren<Transform>().Select(n => n.gameObject);
            SetStaticBGObject(objectList.ToArray());

            Debug.Log(target.name + "以下の" + objectList.Count() + "個を背景向けStatic設定しました。");
        }
    }
}

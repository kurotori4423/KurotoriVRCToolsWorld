﻿using UnityEngine;
using UnityEditor;

namespace KurotoriTools
{

    public class SetStaticFlagsUtility : EditorWindow
    {

        private StaticEditorFlags staticFlags = StaticEditorFlags.BatchingStatic;

        private bool ContributeGI = false;
        private bool OccluderStatic = false;
        private bool OccludeeStatic = false;
        private bool BatchingStatic = false;
        private bool NavigationStatic = false;
        private bool OffMeshLinkGeneration = false;
        private bool ReflectionProbeStatic = false;

        private bool ApplyRecursively = true;


        [MenuItem("KurotoriTools/Set Static Flags")]
        public static void ShowWindow()
        {
            GetWindow<SetStaticFlagsUtility>("Set Static Flags");
        }

        void OnGUI()
        {
            KurotoriUtility.GUITitle("Set Static Flags", "選択したオブジェクトに一括でStaticフラグを指定する。\n子供のオブジェクトにも再帰的に割り当てる");

            EditorGUILayout.LabelField("プリセット", EditorStyles.boldLabel);

            if(GUILayout.Button("背景オブジェクト"))
            {
                ContributeGI = true;
                OccluderStatic = true;
                OccludeeStatic = false;
                BatchingStatic = true;
                NavigationStatic = false;
                OffMeshLinkGeneration = false;
                ReflectionProbeStatic = true;
            }

            if(GUILayout.Button("背景プロップ"))
            {
                ContributeGI = true;
                OccluderStatic = false;
                OccludeeStatic = true;
                BatchingStatic = true;
                NavigationStatic = false;
                OffMeshLinkGeneration = false;
                ReflectionProbeStatic = true;
            }

            GUILayout.Space(10);

            if(GUILayout.Button("選択したオブジェクトのStaticフラグを取得"))
            {
                if(Selection.activeGameObject)
                {
                    var selectionStatic = GameObjectUtility.GetStaticEditorFlags(Selection.activeGameObject);

                    ContributeGI = selectionStatic.HasFlag(StaticEditorFlags.ContributeGI);
                    OccluderStatic = selectionStatic.HasFlag(StaticEditorFlags.OccluderStatic);
                    OccludeeStatic = selectionStatic.HasFlag(StaticEditorFlags.OccludeeStatic);
                    BatchingStatic = selectionStatic.HasFlag(StaticEditorFlags.BatchingStatic);
                    NavigationStatic = selectionStatic.HasFlag(StaticEditorFlags.NavigationStatic);
                    OffMeshLinkGeneration = selectionStatic.HasFlag(StaticEditorFlags.OffMeshLinkGeneration);
                    ReflectionProbeStatic = selectionStatic.HasFlag(StaticEditorFlags.ReflectionProbeStatic);

                }
            }


            EditorGUILayout.LabelField("Staticフラグ設定", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            ContributeGI = EditorGUILayout.Toggle(new GUIContent("ContributeGI"), ContributeGI);
            OccluderStatic = EditorGUILayout.Toggle(new GUIContent("OccluderStatic"), OccluderStatic);
            BatchingStatic = EditorGUILayout.Toggle(new GUIContent("BatchingStatic"), BatchingStatic);
            NavigationStatic = EditorGUILayout.Toggle(new GUIContent("NavigationStatic"), NavigationStatic);
            OccludeeStatic = EditorGUILayout.Toggle(new GUIContent("OccludeeStatic"), OccludeeStatic);
            OffMeshLinkGeneration = EditorGUILayout.Toggle(new GUIContent("OffMeshLinkGeneration"), OffMeshLinkGeneration);
            ReflectionProbeStatic = EditorGUILayout.Toggle(new GUIContent("ReflectionProbeStatic"), ReflectionProbeStatic);

            EditorGUI.indentLevel--;

            staticFlags = 0;
            staticFlags |= (StaticEditorFlags)(ContributeGI ? 0x1 : 0);
            staticFlags |= (StaticEditorFlags)(OccluderStatic ? 0x2 : 0);
            staticFlags |= (StaticEditorFlags)(OccludeeStatic ? 0x10 : 0);
            staticFlags |= (StaticEditorFlags)(BatchingStatic ? 0x4 : 0);
            staticFlags |= (StaticEditorFlags)(NavigationStatic ? 0x8 : 0);
            staticFlags |= (StaticEditorFlags)(OffMeshLinkGeneration ? 0x20 : 0);
            staticFlags |= (StaticEditorFlags)(ReflectionProbeStatic ? 0x40 : 0);

            EditorGUILayout.Space();

            ApplyRecursively = EditorGUILayout.Toggle(new GUIContent("子オブジェクトに再帰的に適用"), ApplyRecursively);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("選択したフラグを設定する"))
            {
                SetStaticFlags();
            }
            if(GUILayout.Button("選択したフラグを追加する"))
            {
                AddStaticFlags();
            }
        }

        void SetStaticFlags()
        {
            if (ApplyRecursively)
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    SetStaticFlagsRecursively(obj);
                }
            }
            else
            {
                // 再帰的に適用しないモード
                foreach (GameObject obj in Selection.gameObjects)
                {
                    Undo.RecordObject(obj, "Set Static Flags");
                    GameObjectUtility.SetStaticEditorFlags(obj, staticFlags);
                }
            }
        }

        void SetStaticFlagsRecursively(GameObject obj)
        {
            // Record the object before making changes for undo support
            Undo.RecordObject(obj, "Set Static Flags");

            GameObjectUtility.SetStaticEditorFlags(obj, staticFlags);

            // Apply the same to all children recursively
            foreach (Transform child in obj.transform)
            {
                SetStaticFlagsRecursively(child.gameObject);
            }
        }

        void AddStaticFlags()
        {
            if(ApplyRecursively)
            {
                foreach(GameObject obj in Selection.gameObjects)
                {
                    AddStaticFlagsRecursively(obj);
                }
            }
            else
            {
                foreach(GameObject obj in Selection.gameObjects)
                {
                    Undo.RecordObject(obj, "Add Static Flags");
                    var currentStatic = GameObjectUtility.GetStaticEditorFlags(obj);

                    var newStatic = currentStatic | staticFlags;

                    GameObjectUtility.SetStaticEditorFlags(obj, newStatic);
                }
            }
        }

        void AddStaticFlagsRecursively(GameObject obj)
        {
            Undo.RecordObject(obj, "Add Static Flags");
            var currentStatic = GameObjectUtility.GetStaticEditorFlags(obj);
            var newStatic = currentStatic | staticFlags;
            GameObjectUtility.SetStaticEditorFlags(obj, newStatic);

            foreach(Transform child in obj.transform)
            {
                AddStaticFlagsRecursively(child.gameObject);
            }
        }
    }
}
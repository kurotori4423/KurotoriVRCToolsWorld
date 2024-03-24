using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// LightProbe編集を支援するエディタ拡張
/// </summary>




[CustomEditor(typeof(AdvancedLightProbeSetting))]
public class AdvancedLightProbeEdit : Editor
{

    SerializedProperty layerNum;
    SerializedProperty offset;
    SerializedProperty firstLayerPoints;

    private void OnEnable()
    {
        if(serializedObject.FindProperty("text") == null)
        {
            Debug.Log("ない");
        }

        layerNum = serializedObject.FindProperty("layernum");
        offset = serializedObject.FindProperty("layerOffset");
        firstLayerPoints = serializedObject.FindProperty("firstLayerPoints");
    }

    public override void OnInspectorGUI()
    {
        var lightProbeSetting = (AdvancedLightProbeSetting)target;

        serializedObject.Update();

        var lightProbeGroup = lightProbeSetting.lightProbeGroup;


        if(GUILayout.Button("Set LightProbe Height 0"))
        {
            SetLightProbeHeight(lightProbeGroup,0);
        }

        

        string layerLabel = "Layer0: ";

        if(firstLayerPoints.arraySize > 0)
        {
            layerLabel += firstLayerPoints.arraySize;
        }else
        {
            layerLabel += "NULL";
        }
        GUILayout.Label(layerLabel);

        if (GUILayout.Button("Set Layer 0"))
        {
            SetLayer(lightProbeGroup, 0);
        }

        if (GUILayout.Button("Reset Layer 0"))
        {
            ResetLayer();
        }


        EditorGUILayout.PropertyField(layerNum);
        EditorGUILayout.PropertyField(offset);
        if (GUILayout.Button("BuildLayer"))
        {
            BuildLayer(lightProbeGroup);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void ClearLightProbe(LightProbeGroup lightProbeGroup)
    {
        lightProbeGroup.probePositions = null;
    }

    private void SetLightProbeHeight(LightProbeGroup lightProbeGroup, float height)
    {

        var lightProbes = lightProbeGroup.probePositions;

        for(int i = 0; i < lightProbes.Length; ++i)
        {
            lightProbes[i] = new Vector3(lightProbes[i].x, height, lightProbes[i].z);
        }

        lightProbeGroup.probePositions = lightProbes;
    }

    private void SetLayer(LightProbeGroup lightProbeGroup, float height)
    {
        firstLayerPoints.arraySize = lightProbeGroup.probePositions.Length;

        for (int i = 0; i < lightProbeGroup.probePositions.Length; ++i)
        {
            firstLayerPoints.GetArrayElementAtIndex(i).vector3Value = lightProbeGroup.probePositions[i]; 
        }
    }

    private void ResetLayer()
    {
        firstLayerPoints.ClearArray();
    }

    private void BuildLayer(LightProbeGroup lightProbeGroup)
    {



        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < layerNum.intValue + 1; ++i)
        {
            for(int j = 0; j < firstLayerPoints.arraySize; ++j)
            {
                Vector3 newPoint = firstLayerPoints.GetArrayElementAtIndex(j).vector3Value;

                newPoint.y = newPoint.y + offset.floatValue * i;

                points.Add(newPoint);
            }
        }

        lightProbeGroup.probePositions = points.ToArray();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[RequireComponent(typeof(LightProbeGroup))]
[ExecuteInEditMode]
public class AdvancedLightProbeSetting : MonoBehaviour
{
    [System.Serializable]
    public class LightProbeLayer
    {

        public string name;
        public Vector3[] pointlist;
    }

    public LightProbeGroup lightProbeGroup;

    [SerializeField]
    public int layernum = 0;

    [SerializeField]
    public float layerOffset = 1.0f;

    [SerializeField]
    public LightProbeLayer[] lightProbeLayers = new LightProbeLayer[0];

    [SerializeField]
    public Vector3[] firstLayerPoints = new Vector3[0];

    [SerializeField]
    string[] texts = new string[0];

    [SerializeField]
    string text = "";

    public void OnEnable()
    {
        lightProbeGroup = gameObject.GetComponent<LightProbeGroup>();

    }
}

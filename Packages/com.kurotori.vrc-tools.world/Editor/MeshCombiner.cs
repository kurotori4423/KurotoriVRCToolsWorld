using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace KurotoriTools
{
    public class MeshCombiner : EditorWindow
    {
        private GameObject baseModel;
        private string saveFolder = "CombineMesh";

        public bool calcurateLightMapUV = true;
        public bool excludeChildren = false;
        public GameObject[] excludeObjects;

        [MenuItem("KurotoriTools/MeshCombiner")]
        static void Open()
        {
            var window = EditorWindow.GetWindow<MeshCombiner>();

        }

        private void OnGUI()
        {
            KurotoriUtility.GUITitle("Mesh Combiner");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Save Folder");
                saveFolder = EditorGUILayout.TextField(saveFolder);
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent Object");
            baseModel = EditorGUILayout.ObjectField(baseModel, typeof(GameObject), true) as GameObject;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Calcurate LightMapUV");
            calcurateLightMapUV = EditorGUILayout.Toggle(calcurateLightMapUV);
            GUILayout.EndHorizontal();
            
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);

            SerializedProperty excludeChildrenProperty = so.FindProperty("excludeChildren");
            EditorGUILayout.PropertyField(excludeChildrenProperty, false);

            SerializedProperty excludeMeshesProperty = so.FindProperty("excludeObjects");
            EditorGUILayout.PropertyField(excludeMeshesProperty, true);

            so.ApplyModifiedProperties();

            if (GUILayout.Button("Check"))
            {
                Check();
            }

            if (GUILayout.Button("Combine"))
            {
                Combine();
            }

            if( GUILayout.Button("Split By Material"))
            {
                SpritByMaterial();
            }
        }

        private void Check()
        {
            Debug.Log("Test");

            if (baseModel == null) return;

            var meshRenderers = baseModel.GetComponentsInChildren<MeshRenderer>();

            Debug.Log("MeshRenderer Count: " + meshRenderers.Length);

            List<Material> matList = new List<Material>();

            foreach (var meshRenderer in meshRenderers)
            {
                var materials = meshRenderer.sharedMaterials;

                var mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;

                if(mesh == null)
                {
                    Debug.LogError(string.Format("MeshRenderer:{0}にはメッシュが含まれていません。", meshRenderer.gameObject.name));
                    return;
                }

                string debugLog = "";

                debugLog += "Mesh:" + mesh.name + "\n";
                debugLog += "SubMeshCount:" + mesh.subMeshCount + "\n";
                debugLog += "MaterialCount:" + materials.Length + "\n";

                for(int i = 0; i < mesh.subMeshCount; ++i)
                {
                    var triangles = mesh.GetTriangles(i);
                    debugLog += "Triangles" + triangles.Length + "\n";

                    List<int> vertexList;
                    CheckTrianglesVertisesNum(triangles, out vertexList);

                    debugLog += "VertexCount" + vertexList.Count + "\n";
                }

                Debug.Log(debugLog);

                foreach(var material in materials)
                {
                    if(!matList.Contains(material))
                    {
                        matList.Add(material);
                    }
                }
            }

            Debug.Log("MaterialCount: " + matList.Count);
        }

        private void DeleteAllChildren(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < parent.childCount; ++i)
            {
                var child = parent.GetChild(i);

                children.Add(child);
            }

            foreach (var child in children)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        private void CheckTrianglesVertisesNum(int[] triangles, out List<int> vertexList)
        {
            vertexList = new List<int>();

            foreach(var triangle in triangles)
            {
                if(!vertexList.Contains(triangle))
                {
                    vertexList.Add(triangle);
                }
            }
        }
        
        private void Combine()
        {
            string ProgressBarTitle = "MeshCombine";

            if (baseModel == null) return;

            // ベースモデルの座標を一旦リセットします
            var baseModelPosition = baseModel.transform.position;
            var baseModelRotation = baseModel.transform.rotation;
            var baseModelScale = baseModel.transform.localScale;

            baseModel.transform.position = Vector3.zero;
            baseModel.transform.rotation = Quaternion.identity;
            baseModel.transform.localScale = Vector3.one;

            var meshRenderers = baseModel.GetComponentsInChildren<MeshRenderer>().ToList();

            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Create ExportExcludeMeshList", 0.0f);

            // 除外リスト
            var exportExcludeMeshObjects = new List<GameObject>();
            if (excludeObjects != null)
            {
                foreach (var excludeObject in excludeObjects)
                {
                    if (excludeObject != null && excludeObject.transform.IsChildOf(baseModel.transform))
                    {
                        exportExcludeMeshObjects.Add(excludeObject);

                        if (excludeChildren)
                        {
                            var childrenList = excludeObject.GetComponentsInChildren<MeshRenderer>();
                            foreach (var child in childrenList)
                            {
                                meshRenderers.Remove(child);
                            }
                        }
                        else
                        {
                            var excludeMeshRenderer = excludeObject.GetComponent<MeshRenderer>();
                            if (excludeMeshRenderer)
                            {
                                meshRenderers.Remove(excludeMeshRenderer);
                            }
                        }
                    }
                }
            }
            Dictionary<Material, List<Mesh>> materialMeshList = new Dictionary<Material, List<Mesh>>();

            EditorUtility.DisplayProgressBar(ProgressBarTitle + " Generate Material List", "", 0.0f);

            float meshRendererNum = meshRenderers.Count;
            float progressCount = 0.0f;

            foreach(var meshRenderer in meshRenderers)
            {
                var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();

                var materials = meshRenderer.sharedMaterials;

                foreach(var material in materials)
                {
                    if(!materialMeshList.ContainsKey(material))
                    {
                        materialMeshList.Add(material, new List<Mesh>());
                    }
                }

                List<Mesh> splitMeshList;

                if(SpritMeshesByMaterial(meshFilter, out splitMeshList, true))
                {
                    for(int i = 0; i < splitMeshList.Count; ++i)
                    {
                        splitMeshList[i].name = meshFilter.name + materials[i].name;
                        materialMeshList[materials[i]].Add(splitMeshList[i]);
                    }
                }

                progressCount += 1.0f;
                EditorUtility.DisplayProgressBar(ProgressBarTitle + " Generate Material List", meshRenderer.name, progressCount / meshRendererNum);

            }

            // ライトマップUVの生成
            UnwrapParam unwrapParam;
            UnwrapParam.SetDefaults(out unwrapParam);

            GameObject newBaseObject = new GameObject(baseModel.name);

            newBaseObject.transform.position = baseModel.transform.position;
            newBaseObject.transform.rotation = baseModel.transform.rotation;
            newBaseObject.transform.localScale = baseModel.transform.localScale;

            EditorUtility.DisplayProgressBar(ProgressBarTitle + "Genereate Split Meshes", "", 0);

            float materialCount = materialMeshList.Count();
            progressCount = 0.0f;

            foreach (var materialMesh in materialMeshList)
            {
                GameObject newObject = new GameObject("Mesh_" + materialMesh.Key.name);

                var newMeshRenderer = newObject.AddComponent<MeshRenderer>();
                var newMeshFilter = newObject.AddComponent<MeshFilter>();

                var newCombineMesh = CombineMeshKai(materialMesh.Value.ToArray());

                newCombineMesh.name = "Mesh_" + materialMesh.Key.name;

                if (newCombineMesh.vertexCount > 65535)
                {
                    Debug.LogWarning(newCombineMesh.name + " has Many Vertices :" + newCombineMesh.vertexCount + ". Change the IndexFormat to 32Bit");
                }

                newMeshFilter.sharedMesh = newCombineMesh;
                newMeshRenderer.sharedMaterial = materialMesh.Key;

                if (calcurateLightMapUV)
                {
                    Debug.Log("UnwrapMesh :" + newCombineMesh.name + ":" + newCombineMesh.vertexCount);
                    Unwrapping.GenerateSecondaryUVSet(newCombineMesh, unwrapParam);
                }
                SaveMeshAsset(saveFolder, baseModel.name, baseModel.name + "_" + newCombineMesh.name, newCombineMesh);

                newObject.transform.SetParent(newBaseObject.transform);

                progressCount += 1.0f;
                EditorUtility.DisplayProgressBar(ProgressBarTitle + "Genereate Split Meshes", newCombineMesh.name, progressCount/materialCount);

            }

            EditorUtility.DisplayProgressBar(ProgressBarTitle + "Copy Exclude Mesh Models", "", 0.0f);
            // 除外リストに入れていたモデルをコピーして追加する。
            foreach (var obj in exportExcludeMeshObjects)
            {
                GameObject.Instantiate(obj, newBaseObject.transform);
            }

            EditorUtility.DisplayProgressBar(ProgressBarTitle + "Copy Colliders", "", 0.0f);


            GameObject colliderParent = new GameObject("Colliders");
            colliderParent.transform.SetParent(newBaseObject.transform);
            
            var colliders = baseModel.GetComponentsInChildren<Collider>();

            foreach(var col in colliders)
            {
                var gameObj = col.gameObject;
                var newColliderObj = GameObject.Instantiate(gameObj);
                newColliderObj.transform.position = col.gameObject.transform.position;
                newColliderObj.transform.rotation = col.gameObject.transform.rotation;
                newColliderObj.transform.localScale = col.gameObject.transform.lossyScale;

                DeleteAllChildren(newColliderObj.transform);

                newColliderObj.transform.SetParent(newBaseObject.transform);

                newColliderObj.name += "_CD";

                newColliderObj.transform.SetParent(colliderParent.transform);

                var meshRenderer = newColliderObj.GetComponent<MeshRenderer>();
                var meshFilter = newColliderObj.GetComponent<MeshFilter>();

                if (meshRenderer) Object.DestroyImmediate(meshRenderer);
                if (meshFilter) Object.DestroyImmediate(meshFilter);
            }

            // 最後にBaseModelの場所を戻します。

            baseModel.transform.SetPositionAndRotation(baseModelPosition, baseModelRotation);
            baseModel.transform.localScale = baseModelScale;


            EditorUtility.ClearProgressBar();
        }

        private Mesh CombineMeshKai(Mesh[] meshes)
        {
            Debug.Log("StartCombine");

            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Color> newColors = new List<Color>();
            List<Vector4> newTangents = new List<Vector4>();
            List<Vector2> newUVs = new List<Vector2>();

            List<int> newTriangleList = new List<int>();

            int nowTrinangleOffset = 0;

            foreach (var mesh in meshes)
            {
                newVertices.AddRange(mesh.vertices);
                newNormals.AddRange(mesh.normals);
                newTangents.AddRange(mesh.tangents);

                if(mesh.colors.Length == 0)
                {
                    for(int i = 0; i < mesh.vertexCount; ++i)
                    {
                        newColors.Add(Color.white);
                    }
                }
                else
                {
                    newColors.AddRange(mesh.colors);
                }

                if (mesh.uv.Length == 0)
                {
                    for (int i = 0; i < mesh.vertexCount; ++i)
                    {
                        newUVs.Add(Vector2.zero);
                    }
                }
                else
                {
                    newUVs.AddRange(mesh.uv);
                }

                foreach(var triangle in mesh.triangles)
                {
                    newTriangleList.Add(triangle + nowTrinangleOffset);
                }

                nowTrinangleOffset += mesh.vertexCount;
            }

            Mesh newMesh = new Mesh();

            if (newVertices.Count > 65535)
            {
                // 結合メッシュの頂点数が65535以上の時はIndexFormatを32Bitに変更する。
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            newMesh.vertices = newVertices.ToArray();
            newMesh.normals = newNormals.ToArray();
            newMesh.tangents = newTangents.ToArray();
            newMesh.uv = newUVs.ToArray();

            newMesh.SetTriangles(newTriangleList.ToArray(), 0);
            newMesh.colors = newColors.ToArray();

            return newMesh;
        }


        private bool SpritMeshesByMaterial(MeshFilter meshFilter, out List<Mesh> newMeshList, bool useMatrices)
        {
            newMeshList = new List<Mesh>();

            var oldMesh = meshFilter.sharedMesh;

            var meshTransform = meshFilter.transform.localToWorldMatrix;
            var worldPosition = meshFilter.transform.position;

            // 反転スケールオブジェクトの場合はインデックスを逆順にする。
            var scaleSign = Mathf.Sign(meshFilter.transform.localScale.x) * Mathf.Sign(meshFilter.transform.localScale.y) * Mathf.Sign(meshFilter.transform.localScale.z);
            var parent = meshFilter.transform.parent;

            while (parent != null)
            {
                scaleSign *= Mathf.Sign(parent.localScale.x) * Mathf.Sign(parent.localScale.y) * Mathf.Sign(parent.localScale.z);

                parent = parent.parent;
            }

            if (oldMesh.subMeshCount == 1)
            {
                var newMesh = new Mesh();

                Vector3[] newVertices = new Vector3[oldMesh.vertices.Length];
                Vector3[] newNormals = new Vector3[oldMesh.vertices.Length];
                Vector4[] newTangents = new Vector4[oldMesh.vertices.Length];
                Vector2[] newUVs = new Vector2[oldMesh.vertices.Length];

                int[] newTriangles = new int[oldMesh.triangles.Length];

                
                for(int i = 0; i < oldMesh.triangles.Length; ++i)
                {
                    if (scaleSign > 0)
                    {
                        newTriangles[i] = oldMesh.triangles[i];
                    }
                    else
                    {
                        newTriangles[oldMesh.triangles.Length - 1 - i] = oldMesh.triangles[i];
                    }
                }

                if (useMatrices)
                {
                    for (int i = 0; i < oldMesh.vertices.Length; ++i)
                    {
                        newVertices[i] = meshTransform * oldMesh.vertices[i];
                        newVertices[i] += worldPosition;
                        newNormals[i] = meshTransform * oldMesh.normals[i];
                        newTangents[i] = meshTransform * oldMesh.tangents[i];

                        newUVs[i] = oldMesh.uv[i];
                    }
                }

                newMesh.vertices = newVertices;
                newMesh.normals = newNormals;
                newMesh.tangents = newTangents;
                newMesh.uv = newUVs;
                newMesh.SetTriangles(newTriangles, 0);
                newMesh.colors = oldMesh.colors;

                newMeshList.Add(newMesh);

                return true;
            }

            

            for(int subMeshIndex = 0; subMeshIndex < oldMesh.subMeshCount; ++subMeshIndex)
            {
                var triangles = oldMesh.GetTriangles(subMeshIndex);

                List<int> vertexIndexList;
                CheckTrianglesVertisesNum(triangles, out vertexIndexList);


                int newIndex = 0;
                Dictionary<int, int> IndexMatch = new Dictionary<int, int>();

                Vector3[] newVertices = new Vector3[vertexIndexList.Count];
                Vector3[] newNormals = new Vector3[vertexIndexList.Count];
                Color[] newColors = new Color[vertexIndexList.Count];
                Vector4[] newTangents = new Vector4[vertexIndexList.Count];
                Vector2[] newUVs = new Vector2[vertexIndexList.Count];

                foreach (var vertexIndex in vertexIndexList)
                {
                    IndexMatch.Add(vertexIndex, newIndex);

                    newVertices[newIndex] = oldMesh.vertices[vertexIndex];
                    newNormals[newIndex] = oldMesh.normals[vertexIndex];
                    newTangents[newIndex] = oldMesh.tangents[vertexIndex];
                    newUVs[newIndex] = oldMesh.uv[vertexIndex];

                    if (oldMesh.colors.Length > 0)
                    {
                        newColors[newIndex] = oldMesh.colors[vertexIndex];
                    }

                    if (useMatrices)
                    {
                        newVertices[newIndex] = meshTransform * newVertices[newIndex];
                        newVertices[newIndex] += worldPosition;
                        newNormals[newIndex] = meshTransform * newNormals[newIndex];
                        newTangents[newIndex] = meshTransform * newTangents[newIndex];
                    }

                    newIndex++;
                }
                int[] newTriangles = new int[triangles.Length];

                for (int j = 0; j < triangles.Length; ++j)
                {

                    if (scaleSign > 0)
                    {
                        newTriangles[j] = IndexMatch[triangles[j]];
                    }
                    else
                    {
                        newTriangles[triangles.Length - 1 - j] = IndexMatch[triangles[j]];
                    }
                }

                var newMesh = new Mesh();

                newMesh.vertices = newVertices;
                newMesh.normals = newNormals;
                newMesh.tangents = newTangents;
                newMesh.uv = newUVs;

                if (oldMesh.colors.Length > 0)
                {
                    newMesh.colors = newColors;
                }
                else
                {
                    newMesh.colors = new Color[0];
                }
                newMesh.SetTriangles(newTriangles, 0);

                newMeshList.Add(newMesh);
            }

            return true;
        }

        private void SpritByMaterial()
        {
            if (baseModel == null) return;

            var meshRenderer = baseModel.GetComponent<MeshRenderer>();
            var meshFilter = baseModel.GetComponent<MeshFilter>();

            if (meshRenderer == null || meshFilter == null)
            {
                Debug.LogError("MeshRenderer or MeshFilter not found");
                return;
            }

            List<Mesh> newMeshes;

            if (SpritMeshesByMaterial(meshFilter, out newMeshes, true))
            {
                int i = 0;
                foreach (var newMesh in newMeshes)
                {
                    GameObject newObject = new GameObject(baseModel.name + "_" + meshRenderer.sharedMaterials[i].name);
                    var newMeshFilter = newObject.AddComponent<MeshFilter>();
                    var newMeshRenderer = newObject.AddComponent<MeshRenderer>();

                    newMeshFilter.sharedMesh = newMesh;
                    newMeshRenderer.sharedMaterial = meshRenderer.sharedMaterials[i];

                    i++;
                }
            }

        }
        

        private void SaveMeshAsset(in string saveMeshFolder, in string prefabName ,in string filename, in Mesh mesh)
        {
            var savePath = Path.Combine("Assets", saveMeshFolder, prefabName);

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            AssetDatabase.CreateAsset(mesh, Path.Combine(savePath, filename + ".asset"));
            AssetDatabase.SaveAssets();
        }
    }

}

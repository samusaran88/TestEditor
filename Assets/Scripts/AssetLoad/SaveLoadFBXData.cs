using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using System.Linq;

public class SaveLoadFBXData : MonoBehaviour
{
    public Material sharedMaterial;
    private AssetLoaderOptions assetLoaderOptions;
    // Start is called before the first frame update
    void Start()
    {
        if (assetLoaderOptions == null)
        {
            assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    }
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
    }
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Materials loaded. Model fully loaded."); 
    }
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials."); 
    }
}
[System.Serializable]
public class SavedModelData
{
    ModelData[] models;
    SavedModelData(GameObject modelObject)
    {
        MeshRenderer[] meshRenderers = modelObject.transform.GetComponentsInChildren<MeshRenderer>(true);
        models = new ModelData[meshRenderers.Length];
    }
}
[System.Serializable]
public class ModelData
{
    public TransformData transformData;
    public SerializableMesh mesh;         
    public MaterialPropertyData[] materialProperties;   
}
[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    public void ApplyToTransform(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}
[System.Serializable]
public class SerializableMesh
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uv;

    public SerializableMesh(Mesh mesh)
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        normals = mesh.normals;
        uv = mesh.uv;
    }

    public Mesh ToMesh()
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.normals = normals;
        newMesh.uv = uv;
        newMesh.RecalculateBounds();
        return newMesh;
    }
}
[System.Serializable]
public class MaterialPropertyData
{
    public Color baseColor;
    public float metallic;
    public float smoothness;
    public Color emissionColor;

    public MaterialPropertyData(Material material)
    {
        baseColor = material.GetColor("_BaseColor");
        metallic = material.GetFloat("_Metallic");
        smoothness = material.GetFloat("_Smoothness");
        emissionColor = material.GetColor("_EmissionColor");
    }

    public void ApplyToMaterial(Material material)
    {
        material.SetColor("_BaseColor", baseColor);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        material.SetColor("_EmissionColor", emissionColor);
    }
}
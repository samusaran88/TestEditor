using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using System.Linq;
using BayatGames.SaveGameFree;

public class SaveLoadFBXData : MonoBehaviour
{
    public string identifier;
    public string password;
    public Material sharedMaterial;
    private AssetLoaderOptions assetLoaderOptions;
    private SaveData saveData;
    private List<LoadFBXData> listFBX = new List<LoadFBXData>();
    private GameObject loadedFBX;
    private bool isLoadComplete;
    // Start is called before the first frame update
    void Start()
    {
        if (assetLoaderOptions == null)
        {
            assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
        }
        saveData = new SaveData();
        isLoadComplete = false;
        StartCoroutine(CoroutineLoadFBX());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            listFBX.Add(new LoadFBXData(Application.persistentDataPath + "/Robot.FBX"));
        }
        //AssetLoader.LoadModelFromFile(ModelPath, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
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
        loadedFBX = assetLoaderContext.RootGameObject; 
        isLoadComplete = true;
        Debug.Log("Materials loaded. Model fully loaded.");
    }
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials."); 
    }
    IEnumerator CoroutineLoadFBX()
    {
        int index = 0;
        while (true)
        {
            if (listFBX.Count > index)
            {
                LoadFBXData fBXData = listFBX[index];
                AssetLoader.LoadModelFromFile(fBXData.path, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
                yield return new WaitUntil(() => isLoadComplete == true);
                yield return new WaitUntil(() => loadedFBX != null);
                fBXData.loadedGB = loadedFBX;

                yield return new WaitForSeconds(1.0f);
                SavedModelData modelData = new SavedModelData(loadedFBX);
                string json = JsonUtility.ToJson(modelData, true);
                string filePath = Application.persistentDataPath + "/" + loadedFBX.name + ".data";  
                System.IO.File.WriteAllText(filePath, json);
                index++;
            }
            yield return null;
        } 
    }
}
public class LoadFBXData
{
    public string path;
    public GameObject loadedGB; 
    public LoadFBXData(string path)
    { 
        this.path = path;
        loadedGB = null; 
    }
}
public class SaveData
{
    public SavedModelData data; 
    public void Save(string identifier, string password)
    {
        SaveGame.Save<SavedModelData>(identifier, data, password);
    }
    public bool Load(string identifier, string password)
    {
        data = SaveGame.Load<SavedModelData>(identifier, true, password);
        if (data == null) return false;
        return true;
    }
    public bool Exist(string identifier)
    {
        return SaveGame.Exists(identifier);
    }
    public SaveData()
    {
        SaveGame.Encode = true;
    }
}
[System.Serializable]
public class SavedModelData
{
    ModelData[] models;
    public SavedModelData(GameObject modelObject)
    {
        MeshRenderer[] meshRenderers = modelObject.transform.GetComponentsInChildren<MeshRenderer>(true);
        models = new ModelData[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            MeshFilter meshFilter = meshRenderers[i].GetComponent<MeshFilter>();
            models[i] = new ModelData();
            models[i].transformData = new TransformData(meshRenderers[i].transform);
            models[i].mesh = new SerializableMesh(meshFilter.mesh);
            List<MaterialPropertyData> listMaterialPropertyDatas = new List<MaterialPropertyData>();
            for (int j = 0; j < meshRenderers[i].materials.Length; j++)
            {
                listMaterialPropertyDatas.Add(new MaterialPropertyData(meshRenderers[i].materials[j]));
            }
            models[i].materialProperties = listMaterialPropertyDatas.ToArray();
        }
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
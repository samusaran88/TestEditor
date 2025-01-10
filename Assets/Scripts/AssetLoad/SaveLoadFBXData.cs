using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using System.Linq;
using BayatGames.SaveGameFree;
using TriLibCore.Fbx;
using System.Runtime.InteropServices;

public class SaveLoadFBXData : MonoBehaviour
{
    public string identifier;
    public string password;
    public SharedMaterial sharedMaterial;
    private AssetLoaderOptions assetLoaderOptions;
    private SaveData saveData;
    private List<LoadFBXData> listFBX = new List<LoadFBXData>();
    private GameObject loadedFBX;
    private bool isLoadComplete;
    private float time;
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
            listFBX.Add(new LoadFBXData(Application.persistentDataPath + "/robot_.FBX"));
            time = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            saveData.Load("robot_", password);
            saveData.data.LoadModelData(sharedMaterial);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            OpenFileName ofn = new OpenFileName();
            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.file = new string(new char[256]);
            ofn.filter = "All Files\0*.*\0\0";
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = Application.persistentDataPath;
            ofn.title = "Open FBX File";
            ofn.defExt = "fbx";
            ofn.flags = 0x00080000;// | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008; //OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECTION | OFN_NOCHANGEDIR

            if (ofnDll.OpenFileName(ofn))
            {
                Debug.Log("OFN SUCCESS!!");
            }
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
        float elapsedTime = Time.time - time;
        Debug.Log("Load Time : " + elapsedTime);
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

                saveData.data = new SavedModelData(loadedFBX);
                saveData.Save(loadedFBX.name, password);

                //SavedModelData modelData = new SavedModelData(loadedFBX);
                //string json = JsonUtility.ToJson(modelData, true);
                //string filePath = Application.persistentDataPath + "/" + loadedFBX.name + ".data";  
                //System.IO.File.WriteAllText(filePath, json);
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
[System.Serializable]
public class SharedMaterial
{
    [SerializeField] private Material sharedMaterial;
    private Dictionary<MaterialPropertyData, Material> dicMaterials = new Dictionary<MaterialPropertyData, Material>();
    public Material Get(MaterialPropertyData mpd)
    {
        foreach (var kvp in dicMaterials)
        {
            if (AreMaterialSimilar(kvp.Key, mpd)) return kvp.Value;
        }
        Material mat = new Material(sharedMaterial);
        mpd.ApplyToMaterial(mat);
        dicMaterials.Add(mpd, mat);
        return mat;
    }
    bool AreMaterialSimilar(MaterialPropertyData mpdA, MaterialPropertyData mpdB)
    {
        if (AreColorsSimilar(mpdA.baseColor, mpdB.baseColor) == false) return false;
        if (AreColorsSimilar(mpdA.emissionColor, mpdB.emissionColor) == false) return false;
        if (AreFloatsSimilar(mpdA.metallic, mpdB.metallic) == false) return false;
        if (AreFloatsSimilar(mpdA.smoothness, mpdB.smoothness) == false) return false;
        return true;
    }
    bool AreFloatsSimilar(float a, float b, float tolerance = 0.001f)
    {
        return Mathf.Abs(a - b) < tolerance;
    }
    bool AreColorsSimilar(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
}
public class SaveData
{
    public SavedModelData data; 
    public void Save(string identifier, string password)
    {
        data.name = identifier;
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
    public string name;
    public ModelData[] models;
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
    public GameObject LoadModelData(SharedMaterial sharedMaterial)
    {
        GameObject modelObject = new GameObject(name);
        for (int i = 0; i < models.Length; i++)
        {
            ModelData model = models[i];
            GameObject part = new GameObject("part" + i);
            part.transform.parent = modelObject.transform;
            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            model.transformData.ApplyToTransform(part.transform);
            meshFilter.mesh = model.mesh.ToMesh();
            Material[] materials = new Material[model.materialProperties.Length];
            for (int j = 0; j < materials.Length; j++)
            { 
                materials[j] = new Material(sharedMaterial.Get(model.materialProperties[j]));
                model.materialProperties[j].ApplyToMaterial(materials[j]);
            }
            meshRenderer.materials = materials; 
        }
        return modelObject;
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
        scale = transform.lossyScale;
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
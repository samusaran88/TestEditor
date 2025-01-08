using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;

public class TestLoadAsset : MonoBehaviour
{ 
    private string ModelPath
    {
        get
        {
#if UNITY_EDITOR
            return $"{Application.persistentDataPath}/Robot.FBX";
#else
                return "Models/TriLibSampleModel.obj";
#endif
        }
    } 
    private AssetLoaderOptions assetLoaderOptions;
     
    private void Start()
    {
        if (assetLoaderOptions == null)
        {
            assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions(false, true);
        }
        AssetLoader.LoadModelFromFile(ModelPath, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
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
        //var myLoadedGameObject = assetLoaderContext.RootGameObject;
        //myLoadedGameObject.SetActive(true);
    } 
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials.");
        //var myLoadedGameObject = assetLoaderContext.RootGameObject;
        //myLoadedGameObject.SetActive(false);
    }
}

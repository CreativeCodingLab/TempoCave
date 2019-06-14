using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SSceneManager : MonoBehaviour
{

    public DataLoader connectomeDataLoader;
    public ConnectomeBuilder connectomeBuilder;
    public ConnectomeFocus connectomeFocus;

    private string[] connectomeFolderNames;
    public List<Dictionary<string, string[][]>> connectomeList = new List<Dictionary<string, string[][]>>();
    private Dictionary<string, List<string>> _connectomeRepresentationDictionary = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> _connectomeClassificationDictionary = new Dictionary<string, List<string>>();
    private Dictionary<string, Color> ColorCoding;
    private Dictionary<string, bool> IsDynamic;
    private static int firstLoad = 0;
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        
        if (sceneName == "Selection")
        {
            if(firstLoad == 0)
            {
                connectomeList = connectomeDataLoader.LoadConnectomes(connectomeDataLoader.fullConnectomeFolders);
                firstLoad += 1;
            }     
            else
            {
                connectomeDataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
                connectomeList = connectomeDataLoader.connectomeGlobalList;
            }
            _connectomeClassificationDictionary = connectomeDataLoader.connectomeClassificationGlobalDictionary;
            _connectomeRepresentationDictionary = connectomeDataLoader.connectomeRepresentationGlobalDictionary;
            ColorCoding = connectomeDataLoader.colorCodingGlobal;
            IsDynamic = connectomeDataLoader.isDynamicDictionaryGlobal;
            connectomeFolderNames = connectomeDataLoader.GetConnectomeFolderName(connectomeDataLoader.fullConnectomeFolders);
            connectomeBuilder.Build(false, connectomeList, connectomeFolderNames, _connectomeRepresentationDictionary, _connectomeClassificationDictionary, ColorCoding, IsDynamic,0,connectomeDataLoader.connectomeTimeStep, connectomeDataLoader.modularityDwellingTimeDictionary, connectomeDataLoader.modularityFrequencyDictionary );
            connectomeFocus.GetRepresentationDictionary(_connectomeRepresentationDictionary);
            DontDestroyOnLoad(connectomeDataLoader.gameObject);
        }
    }
}
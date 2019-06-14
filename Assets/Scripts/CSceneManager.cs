using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSceneManager : MonoBehaviour
{

    public DataLoader connectomeDataLoader;
    public ConnectomeBuilder connectomeBuilder;

    private string[] connectomeSelectedFolderNames;
    private string[] connectomeFolderNames;
    private List<string> _selectedConnectomeName = new List<string>();
    private List<Dictionary<string, string[][]>> connectomeList = new List<Dictionary<string, string[][]>>();
    private List<Dictionary<string, string[][]>> connectomeSelectedList = new List<Dictionary<string, string[][]>>();
    private Dictionary<string, List<string>> _connectomeRepresentationDictionary = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> _connectomeClassificationDictionary = new Dictionary<string, List<string>>();
    private Dictionary<string, float[,]> _modularityChangeTrackingDictionary = new Dictionary<string, float[,]>();
    private Dictionary<string, Color> ColorCoding;
    private Dictionary<string, bool> IsDynamic;
    private List<int> selectedConnectomeIndex = new List<int>();
    //private Dictionary<string, Dictionary<string, List<string>>> distinctClassificationDictionary;

    void Start()
    {
        connectomeDataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
        connectomeFolderNames = connectomeDataLoader.GetConnectomeFolderName(connectomeDataLoader.fullConnectomeFolders);
        for (int i = 0; i < PlayerPrefs.GetInt("count"); i++)
        {
            string name = PlayerPrefs.GetString("SelectedConnectome_" + i);
            for(int j = 0; j< connectomeFolderNames.Length;j++)
            {
                if (connectomeFolderNames[j] == name)
                    selectedConnectomeIndex.Add(j);
            }
            _selectedConnectomeName.Add(connectomeDataLoader.m_Path + "/" + name);
        }
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        int isDuplicate = PlayerPrefs.GetInt("isDuplicate");
        if (sceneName == "Comparison")
        {
            connectomeList = connectomeDataLoader.connectomeGlobalList;
            for (int i = 0;i< selectedConnectomeIndex.Count; i++)
            {
                connectomeSelectedList.Add(connectomeList[selectedConnectomeIndex[i]]);
            }
            _connectomeRepresentationDictionary = connectomeDataLoader.connectomeRepresentationGlobalDictionary;
            _connectomeClassificationDictionary = connectomeDataLoader.connectomeClassificationGlobalDictionary;
            _modularityChangeTrackingDictionary = connectomeDataLoader.modularityDwellingTimeDictionary;
            ColorCoding = connectomeDataLoader.colorCodingGlobal;
            IsDynamic = connectomeDataLoader.isDynamicDictionaryGlobal;

            connectomeSelectedFolderNames = connectomeDataLoader.GetConnectomeFolderName(_selectedConnectomeName.ToArray());
            connectomeBuilder.Build(false, connectomeSelectedList, connectomeSelectedFolderNames, _connectomeRepresentationDictionary, _connectomeClassificationDictionary, ColorCoding, IsDynamic, isDuplicate, connectomeDataLoader.connectomeTimeStep, _modularityChangeTrackingDictionary, connectomeDataLoader.modularityFrequencyDictionary);
            DontDestroyOnLoad(connectomeDataLoader.gameObject);
        }
    }

}
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class DataLoader : MonoBehaviour
{

    public string m_Path;
    public Dictionary<string, string[][]> connectomeData;
    public string[] fullConnectomeFolders;
    public List<Dictionary<string, string[][]>> connectomeGlobalList = new List<Dictionary<string, string[][]>>();
    public Dictionary<string, List<string>> connectomeRepresentationGlobalDictionary = new Dictionary<string, List<string>>();
    public Dictionary<string, List<string>> connectomeClassificationGlobalDictionary = new Dictionary<string, List<string>>();
    public Dictionary<string, Color> colorCodingGlobal = new Dictionary<string, Color>();
    public Dictionary<string, bool> isDynamicDictionaryGlobal = new Dictionary<string, bool>();
    public Dictionary<string, bool> isEdgeBundlingDictionaryGlobal = new Dictionary<string, bool>();
    public Dictionary<string, float[,]> modularityDwellingTimeDictionary = new Dictionary<string, float[,]>();
    public Dictionary<string, float[]> modularityFrequencyDictionary = new Dictionary<string, float[]>();
    private int timeStep = 0;
    public Dictionary<string, int> connectomeTimeStep = new Dictionary<string, int>();
    //private int subject = 36;
    private List<string> _topologiesList = new List<string> { "anatomy", "isomap", "tsne", "mds" };
    //substring char count to cut the path from 'Assets/Resources/Data/' to 'Data/' for Resources
    private const int subStringStartChar = 17;
    private List<string> colorCodingList = new List<string>();
    private List<string> dynamicModularityList = new List<string>();
    private int NodeNumber = 0;
    private int[,] modularityDwellingTime;
    private float[,] modularityDwellingTimeNormalized;
    private float[] modularityFrequency;

    void Awake()
    {
        fullConnectomeFolders = System.IO.Directory.GetDirectories(m_Path, "*", System.IO.SearchOption.TopDirectoryOnly);
    }

    void ReadCSVFile(string connectomePath, string filePath, string FileType)
    {
        filePath = filePath.Substring(subStringStartChar);
        TextAsset initialdata = Resources.Load<TextAsset>(filePath);

        string[] data = initialdata.ToString().Split('\n');
        List<string[]> dataMatrix = new List<string[]>();

        for (int i = 0; i < data.Length; i++)
        {
            if (!string.IsNullOrEmpty(data[i]) && !string.IsNullOrWhiteSpace(data[i]))
            {
                dataMatrix.Add(data[i].Split(','));
                for (int j = 0; j < dataMatrix[i].Length; j++)
                    dataMatrix[i][j] = string.Concat(dataMatrix[i][j].Where(c => !char.IsWhiteSpace(c)));
            }
        }
        connectomeData.Add(FileType, dataMatrix.ToArray());
    }

    public string[] GetConnectomeFolderName(string[] connectomeFolders)
    {
        string[] connectomeFolderName = new string[connectomeFolders.Length];
        for (int i = 0; i < connectomeFolders.Length; i++)
            connectomeFolderName[i] = new DirectoryInfo(connectomeFolders[i]).Name;
        return connectomeFolderName;
    }


    public List<Dictionary<string, string[][]>> LoadConnectomes(string[] connectomeFolders)
    {
        List<Dictionary<string, string[][]>> connectomeList = new List<Dictionary<string, string[][]>>();
        isDynamicDictionaryGlobal = new Dictionary<string, bool>();
        isEdgeBundlingDictionaryGlobal = new Dictionary<string, bool>();
        connectomeRepresentationGlobalDictionary = new Dictionary<string, List<string>>();
        connectomeClassificationGlobalDictionary = new Dictionary<string, List<string>>();
        foreach (string connectome in connectomeFolders)
        {
            //print(connectome);
            Dictionary<string, string> filePath = new Dictionary<string, string>();
            dynamicModularityList = new List<string>();
            string connectomeName = new DirectoryInfo(connectome).Name;
            List<string> _connectomeRepresentation = new List<string>();
            List<string> _connectomeClassification = new List<string>();
            connectomeData = new Dictionary<string, string[][]>();
            filePath.Add("anatomy", connectome + "/topologies/anatomy");
            filePath.Add("isomap", connectome + "/topologies/isomap");
            filePath.Add("tsne", connectome + "/topologies/tsne");
            filePath.Add("mds", connectome + "/topologies/mds");
            filePath.Add("label", connectome + "/labels/label");
            filePath.Add("LookupTable", connectome + "/atlas/LookupTable");
            filePath.Add("NW", connectome + "/edges/NW");
            if (!isEdgeBundlingDictionaryGlobal.ContainsKey(connectomeName))
                isEdgeBundlingDictionaryGlobal.Add(connectomeName, false);
            
            string dynamicDir = connectome + "/atlas/DynamicModularity";
            if (System.IO.Directory.Exists(dynamicDir))
                timeStep = new DirectoryInfo(dynamicDir).GetFiles("*.csv").Count();
            else timeStep = 0;
            connectomeTimeStep.Add(connectomeName, timeStep);


            for (int i = 1; i < timeStep + 1; i++)
            {
                string filename = connectome + "/edges/" + i;
                if (System.IO.File.Exists(filename + ".csv"))
                {
                    filePath.Add("Time_" + i, filename);
                    isEdgeBundlingDictionaryGlobal[connectomeName] = true;
                }
                    
                filename = connectome + "/atlas/DynamicModularity/" + i;
                if (System.IO.File.Exists(filename + ".csv"))
                    filePath.Add("Mod_" + i, filename);

                //filename = connectome + "/edges/" + i + "_edgeInformation";
                //if (System.IO.File.Exists(filename + ".csv"))
                //{
                //    filePath.Add("EdgeBundlingTime_" + i + "_edgeInformation", filename);
                    
                //}

                //filename = connectome + "/edges/" + i + "_edgePosition";
                //if (System.IO.File.Exists(filename + ".csv"))
                //    filePath.Add("EdgeBundlingTime_" + i + "_edgePosition", filename);
            }
            ///Reading in the data
            foreach (KeyValuePair<string, string> file in filePath)
                if (System.IO.File.Exists(file.Value + ".csv"))
                {
                    ReadCSVFile(connectome, file.Value, file.Key);
                    if (_topologiesList.Contains(file.Key))
                        _connectomeRepresentation.Add(file.Key);
                }
                else
                    connectomeData.Add(file.Key, null);
            ///get the colorcodinglist
            for (int i = 1; i < connectomeData["LookupTable"].Length; i++)
            {
                for (int j = 1; j < connectomeData["LookupTable"][i].Length; j++)
                    colorCodingList.Add(connectomeData["LookupTable"][i][j]);
            }

            ///get the classification type for each connectome
            for (int i = 1; i < connectomeData["LookupTable"][0].Length; i++)
            {
                _connectomeClassification.Add(connectomeData["LookupTable"][0][i]);
            }
            
            if (System.IO.Directory.Exists(connectome + "/atlas/DynamicModularity"))
            {
                for (int i = 1; i < timeStep + 1; i++)
                {
                    if (connectomeData["Mod_" + i] != null)
                    {
                        NodeNumber = connectomeData["Mod_" + i].Length;
                        for (int j = 1; j < connectomeData["Mod_" + i].Length; j++)
                        {
                            colorCodingList.Add(connectomeData["Mod_" + i][j][1]);
                            dynamicModularityList.Add(connectomeData["Mod_" + i][j][1]);
                        }
                    }
                }
                dynamicModularityList = dynamicModularityList.Distinct().ToList();
                isDynamicDictionaryGlobal.Add(new DirectoryInfo(connectome).Name, true);
                _connectomeClassification.Add("Dynamic Modularity");

                modularityDwellingTime = new int[NodeNumber, dynamicModularityList.Count];
                for (int i = 0; i < NodeNumber; i++)
                {
                    for (int j = 0; j < dynamicModularityList.Count; j++)
                    {
                        modularityDwellingTime[i, j] = 0;
                    }
                }

                for (int i = 1; i < timeStep + 1; i++)
                {
                    if (connectomeData["Mod_" + i] != null)
                    {
                        for (int j = 1; j < connectomeData["Mod_" + i].Length; j++)
                        {
                            int modularity = int.Parse(connectomeData["Mod_" + i][j][1]);
                            modularityDwellingTime[j - 1, modularity - 1] += 1;
                        }
                    }
                }
                modularityFrequency = new float[NodeNumber];
                for (int i = 0; i < NodeNumber; i++)
                    modularityFrequency[i] = 0;
                
                for (int i = 2; i < timeStep + 1; i++)
                {
                    if (connectomeData["Mod_" + i] != null)
                    {
                        for (int j = 1; j < connectomeData["Mod_" + i].Length; j++)
                        {
                            int previousTimeStep = i - 1;
                            int previousModularity = int.Parse(connectomeData["Mod_" + previousTimeStep][j][1]);
                            int modularity = int.Parse(connectomeData["Mod_" + i][j][1]);
                            if (previousModularity != modularity)
                                modularityFrequency[j-1]++;
                        }
                    }
                }
                //print(connectomeName);
                for (int i = 0; i < NodeNumber; i++)
                {
                    modularityFrequency[i] = modularityFrequency[i] / timeStep;
                    //if(i == 38||i ==39 || i == 40|| i == 41)
                    //{
                    //    print(i+" "+modularityFrequency[i]);
                    //}
                    
                }
                    
                    
                modularityDwellingTimeNormalized = new float[NodeNumber, dynamicModularityList.Count];
                for (int i = 0; i < modularityDwellingTime.GetLength(0); i++)
                {
                    for (int j = 0; j < modularityDwellingTime.GetLength(1); j++)
                    {
                        modularityDwellingTimeNormalized[i, j] = float.Parse(modularityDwellingTime[i, j].ToString()) / float.Parse(timeStep.ToString());
                    }
                }
            }
            else isDynamicDictionaryGlobal.Add(new DirectoryInfo(connectome).Name, false);
            connectomeRepresentationGlobalDictionary.Add(new DirectoryInfo(connectome).Name, _connectomeRepresentation);
            connectomeClassificationGlobalDictionary.Add(new DirectoryInfo(connectome).Name, _connectomeClassification);
            modularityDwellingTimeDictionary.Add(new DirectoryInfo(connectome).Name, modularityDwellingTimeNormalized);
            modularityFrequencyDictionary.Add(new DirectoryInfo(connectome).Name, modularityFrequency);
            connectomeList.Add(connectomeData);
        }
        ///assigning colors to the list
        colorCodingGlobal = new Dictionary<string, Color>();
        colorCodingList = colorCodingList.Distinct().ToList();
        string[] colorList = null;
        string ColorListPath = m_Path + "/ColorList";

        if (System.IO.File.Exists(ColorListPath + ".csv"))
        {
            ColorListPath = ColorListPath.Substring(subStringStartChar);
            TextAsset colorListData = Resources.Load<TextAsset>(ColorListPath);
            colorList = colorListData.ToString().Split('\n');
        }
        
        for (int i = 0; i < colorCodingList.Count; i++)
        {

            Color32 new_color;
            string[] rgb_string = colorList[i].Split(',');
            new_color.r = (byte)int.Parse(rgb_string[0]);
            new_color.g = (byte)int.Parse(rgb_string[1]);
            new_color.b = (byte)int.Parse(rgb_string[2]);

            colorCodingGlobal.Add(colorCodingList[i], new Color32(new_color.r, new_color.g, new_color.b, 255));
        }

        connectomeGlobalList = connectomeList;
        return connectomeList;
    }
}

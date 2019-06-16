using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectomeBuilder : MonoBehaviour
{

    public GameObject nodePrefab;
    public GameObject edgePrefab;

    private GameObject _connectomeTopLevel;
    private GameObject _connectomeParentEmpty;
    private GameObject _connectomeGlassBrain;
    private List<GameObject> _connectomeTopLevelList = new List<GameObject>();
    private List<GameObject> _connectomeParentList = new List<GameObject>();
    private Dictionary<string, Material> classificationMaterialDictionary = new Dictionary<string, Material>();
    private float rotationRadius = 20;


    void Awake()
    {
        _connectomeTopLevel = Resources.Load("Prefabs/ConnectomeTopLevel") as GameObject;
        _connectomeParentEmpty = Resources.Load("Prefabs/SingleConnectome") as GameObject;
        _connectomeGlassBrain = Resources.Load("Prefabs/brain") as GameObject;

    }

    public void Build(bool isOverlay, List<Dictionary<string, string[][]>> connectomeList, string[] connectomeNames, Dictionary<string, List<string>> connectomeRepresentationDictionary, Dictionary<string, List<string>> connectomeClassificationDictionary, Dictionary<string, Color> ColorCoding, Dictionary<string, bool> isDynamic, int isDuplicate, Dictionary<string, int> connectomeTimeStep, Dictionary<string, float[,]> modularityChangeTrackingDictionary, Dictionary<string, float[]> modularityFrequencyDictionary)
    {
        float n = connectomeNames.Length;
        int numberConnectomes = int.Parse(n.ToString());
        float[] anglearray = new float[numberConnectomes];
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        bool isEven = false;
        if (n % 2 == 0)
            isEven = true;
        else isEven = false;


        if(sceneName == "Selection")
        {
            if (isEven)
            {
                int centerIndex = numberConnectomes / 2 - 1;
                anglearray[numberConnectomes / 2 - 1] = 0;
                for (int i = 0; i < centerIndex; i++)
                    anglearray[i] = 0 - (centerIndex - i);
                for (int i = centerIndex + 1; i < n; i++)
                    anglearray[i] = i - centerIndex;
            }
            else
            {
                anglearray[0] = -(n - 1) / 2;
                for (int j = 1; j < n; j++)
                    anglearray[j] = anglearray[j - 1] + 1;
            }  

        }
        else
        {
            if (isOverlay && n == 2.0f)
            {
                anglearray[0] = 0.0f;
                anglearray[1] = 0.0f;
            }
            else
            {
                anglearray[0]= -(n - 1) / 2;
                for (int j = 1; j < n; j++)
                    anglearray[j]= anglearray[j - 1] + 1;
            }
        }
        

        

        List<Vector3> connectomePositionForDesktopInspectScene = new List<Vector3>();
        connectomePositionForDesktopInspectScene.Add(new Vector3(-2.0f, 0, -20));
        connectomePositionForDesktopInspectScene.Add(new Vector3(1.5f, 0, -20));   
        //float[] anglearray = new float[connectomeList.Count];
        //anglearray[0] = -(n - 1) / 2;

        GameObject ConnectomeContainer = GameObject.Find("ConnectomeContainer");

        //for (int j = 1; j < connectomeList.Count; j++)
            //anglearray[j] = anglearray[j - 1] + 1;
        float angle = rotationRadius * 0.016f;

        for (int i = 0; i < connectomeNames.Length; i++)
        {
            float rotationAngle = angle * anglearray[i];
            float posZ = -Mathf.Cos(rotationAngle) * rotationRadius;
            float posX = Mathf.Sin(rotationAngle) * rotationRadius;

            GameObject ConnectomeTopLevel = Instantiate(_connectomeTopLevel, new Vector3(posX, 0, posZ), Quaternion.identity);
            ConnectomeTopLevel.transform.name = "ConnectomeTopLevel";
            ConnectomeTopLevel.transform.tag = "ConnectomeTopLevel";
            if (sceneName == "Selection")
                ConnectomeTopLevel.transform.parent = ConnectomeContainer.transform;
            _connectomeTopLevelList.Add(ConnectomeTopLevel);
            
            GameObject ConnectomeParent = Instantiate(_connectomeParentEmpty, new Vector3(posX, 0, posZ), Quaternion.identity);
            GameObject ConnectomeGlassBrain = Instantiate(_connectomeGlassBrain, new Vector3(posX, 0, posZ), Quaternion.identity);
            ConnectomeParent.transform.parent = ConnectomeTopLevel.transform;
            ConnectomeGlassBrain.transform.parent = ConnectomeParent.transform;
            ConnectomeParent.name = connectomeNames[i];

            if (isDuplicate == 0)
                ConnectomeParent.tag = "SingleConnectome";
            else if (isDuplicate == 1 && i == 0)
                ConnectomeParent.tag = "SingleConnectome";
            else if (isDuplicate == 1 && i == 1)
                ConnectomeParent.tag = "SingleConnectomeDuplicate";

            _connectomeParentList.Add(ConnectomeParent);
            _connectomeParentList[i].AddComponent<SingleConnectome>().attachConnectomeData(i+1,connectomeList[i], connectomeNames[i], nodePrefab, edgePrefab, rotationAngle, connectomeRepresentationDictionary[connectomeNames[i]].ToArray(), connectomeClassificationDictionary[connectomeNames[i]].ToArray(), ColorCoding, isDynamic[connectomeNames[i]],connectomeTimeStep[connectomeNames[i]], modularityChangeTrackingDictionary[connectomeNames[i]], modularityFrequencyDictionary[connectomeNames[i]]);//, distinctClassificationDictionary[connectomeNames[i]]);
            _connectomeParentList[i].AddComponent<GPUEdgeBundling>();
            _connectomeParentList[i].AddComponent<BoxCollider>();
            _connectomeParentList[i].GetComponent<BoxCollider>().size = new Vector3(2.5f, 2.5f, 2.5f);
            if (sceneName == "Selection")
            {
                _connectomeParentList[i].AddComponent<Rigidbody>();
                _connectomeParentList[i].GetComponent<Rigidbody>().useGravity = false;
                _connectomeParentList[i].GetComponent<Rigidbody>().isKinematic = true;
            }
            if (sceneName == "Comparison")
            {
                ConnectomeTopLevel.transform.position = connectomePositionForDesktopInspectScene[i];
                ConnectomeParent.transform.position = connectomePositionForDesktopInspectScene[i];
            }
        }
    }

    public void DestroyConnectomes()
    {
        for (int i = 0; i < _connectomeParentList.Count; i++)
        {
            if (_connectomeParentList[i] != null)
                Destroy(_connectomeParentList[i]);
        }
    }

}
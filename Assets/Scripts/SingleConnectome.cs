using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SingleConnectome : MonoBehaviour
{
    public string _representationType = string.Empty;
    public string _classificationType = string.Empty;
    public string TimeStage = "Time_1";
    private string _preTimeStage = "Time_1";
    private string _connectomeName = string.Empty;
    private string connectomeRegionName = string.Empty;
    private string _prevRepresentationType = "Random";
    private string _prevClassificationType = "Random";
    private string sceneName = string.Empty;
    private string atlas = "LookupTable";

    public float connectomeUpdatedScale;
    public float thresholdMax;
    public float thresholdMin;
    public float Opacity = 1;
    public float Glyph = 1;
    public float preGlyph = 1;
    public float Speed = 1;
    public float Gradient = 1;
    private float _preSpeed = 1;
    private float dashAmount = 0;
    private float _preThresholdMax = 1;
    private float _preThresholdMin = -1.0f;
    
    private float _preOpacity = 1;
    private float _preGradient = 1;
    private float _rotationAngle;
    private float edgeSize = 0.004f;

    public bool isPause = false;
    public bool isEdgeBundling = false;
    public bool isSync = false;
    public bool isSelectingNode = false;
    public bool PosNeg = false;
    private bool isFirstDrawConnectome = true;
    private bool isPlayingDynamicData = false;
    private bool _prevIsEdgeBundling = true;
    private bool _isDynamic;
    private bool _PreviousPosNeg = false;

    public int DynamicCurrentTime = 0;
    public int _DynamicPreviousTime = 0;
    private int _connectomeNumber = 0;
    private int edgeBundlingIterations = 5;
    private int _timeStep = 0;

    public string[] _connectomeRepresentation;
    public string[] _connectomeClassification;
    public string[][] _edgesMatrix;
    private float[,] _modularityChangeTracking;
    private float[] _modularityFrequency;

    public Dictionary<string, List<GameObject>> _classificationNodes = new Dictionary<string, List<GameObject>>();
    public Dictionary<int, List<GameObject>> NodeStartStraightEdgeStatus = new Dictionary<int, List<GameObject>>();
    public Dictionary<string, string[][]> _connectomeData = new Dictionary<string, string[][]>();
    private Dictionary<string, int> classificationColumn = new Dictionary<string, int>();
    private Dictionary<int, int> labelDictionary = new Dictionary<int, int>();
    //private Dictionary<string, GameObject> _connectomeMenu = new Dictionary<string, GameObject>();
    private Dictionary<GameObject, float> _edgeConnectivity = new Dictionary<GameObject, float>();
    private Dictionary<string, Color> _colorCoding;

    public List<string> ModuleList = new List<string>();
    public List<int> StartNodeList = new List<int>();
    public List<int> BundledStartNodeList = new List<int>();
    public List<GameObject> Nodes = new List<GameObject>();
    public List<GameObject> AllEdges = new List<GameObject>();
    public List<string> SelectedNode = new List<string>();
    public List<string> previousSelectedNode = new List<string>();
    private List<Color> NodeColorList = new List<Color>();

    public GameObject classificationGameObject;
    public GameObject ThresholdMinSlider;
    public GameObject ThresholdMaxSlider;
    private GameObject _nodePrefab;
    private GameObject _edgePrefab;
    private GameObject RepresentationTypePrefab;
    private GameObject _connectomeRepresentationType;
    private GameObject _SelectSphere;

    public Vector3 NodePrefabScale;
    private Vector3 oldEulerAngle;
    private Renderer edgeRend;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private GPUEdgeBundling gpuEdgeBundling;

    private Color32[] SpectrumColors =
    {
        new Color32(255,178,95, 255),
        new Color32(255,178,95, 255),
        new Color32(255,178,95, 255),
        new Color32(221,187,138, 255),
        new Color32(206,191,156, 255),
        new Color32(188,196,178,255),
        new Color32(168,202,204,255),
        new Color32(137,210,243,255),
        new Color32(134,211,246,255),
        new Color32(133,211,248,255)
    };
    private Color32[] NegativeEdges =
    {
        new Color32(220,240,255,255),
        new Color32(204,233,255,255),
        new Color32(175,211,255,255),
        new Color32(149,210,255,255),
        new Color32(128,200,255,255),
        new Color32(103,191,255,255),
        new Color32(85,183,255,255),
        new Color32(64,173,255,255),
        new Color32(40,165,255,255),
        new Color32(21,153,255,255)
    };

    private Color32[] PositiveEdges =
    {
        new Color32(248,220,205,255),
        new Color32(255,209,185,255),
        new Color32(255,191,156,255),
        new Color32(255,178,136,255),
        new Color32(255,164,115,255),
        new Color32(255,148,91,255),
        new Color32(255,143,81,255),
        new Color32(255,130,62,255),
        new Color32(255,114,38,255),
        new Color32(255,114,38,255),
    };

    
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        connectomeUpdatedScale = 1;
        _SelectSphere = Resources.Load("Prefabs/SelectionSphere") as GameObject;
        RepresentationTypePrefab = Resources.Load("Prefabs/RepresentationType") as GameObject; 
        //_connectomeMenu[_connectomeName] = null;
        _representationType = _connectomeRepresentation[0];
        _classificationType = _connectomeClassification[0];

        thresholdMax = _preThresholdMax;
        thresholdMin = _preThresholdMin;
        for (int i = 0; i < _connectomeClassification.Length; i++)
        {
            classificationColumn.Add(_connectomeClassification[i], i + 1);
        }
        oldEulerAngle = this.transform.rotation.eulerAngles;

        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        gpuEdgeBundling = GetComponent<GPUEdgeBundling>();
    }

    void Update()
    {
        
            
        connectomeUpdatedScale = this.transform.parent.transform.localScale.x;

        if (_prevRepresentationType != _representationType || _prevClassificationType != _classificationType && _classificationType!="Dynamic Modularity")
        {
            atlas = "LookupTable";
            ClearAllNodes();
            ClearAllEdges();
            DrawConnectome(_connectomeData, connectomeUpdatedScale, classificationColumn[_classificationType]);
            if (isFirstDrawConnectome)
            {
                SelectAllNode();
                isFirstDrawConnectome = false;
            }
            if (isEdgeBundling)
            {
                if(_prevRepresentationType != _representationType || _prevClassificationType != _classificationType)
                    gpuEdgeBundling.EdgeBundling();
                DrawEdgeBundling(thresholdMin, thresholdMax);
            } 
            else
            {
                foreach (int i in StartNodeList)
                {
                    DrawSingleEdge(i, thresholdMin, thresholdMax);
                }
            }
            
            if (_prevClassificationType != _classificationType && sceneName == "Comparison")
            {
                Transform[] classificationsInGamObject = classificationGameObject.GetComponentsInChildren<Transform>();
                foreach (Transform classification in classificationsInGamObject)
                {
                    if (classification.parent == classificationGameObject.transform)
                    {
                        if (classification.name == _classificationType)
                            classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                        else classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
                    }
                }
            }
            _prevRepresentationType = _representationType;
            _prevClassificationType = _classificationType;
           
        }
        if (_classificationType == "Dynamic Modularity")
        {
            _prevClassificationType = "Dynamic Modularity";
            Transform[] classificationsInGamObject = classificationGameObject.GetComponentsInChildren<Transform>();
            foreach (Transform classification in classificationsInGamObject)
            {
                if (classification.parent == classificationGameObject.transform)
                {
                    if (classification.name == _classificationType)
                        classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                    else classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
                }
            }
        }

        if (thresholdMin != _preThresholdMin || thresholdMax != _preThresholdMax)
        {
            _preThresholdMax = thresholdMax;
            _preThresholdMin = thresholdMin;
            RedrawEdges();
        }

        if (_preOpacity != Opacity)
        {
            _preOpacity = Opacity;
            foreach (GameObject edge in AllEdges)
            {
                if(edge)
                {
                    string edgeName = edge.transform.name;
                    string[] nodeList = edgeName.Split('_');
                    edge.transform.parent.Find(nodeList[0]).GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                    float opacity1 = _propBlock.GetFloat("_Alpha1");
                    edge.transform.parent.Find(nodeList[1]).GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                    float opacity2 = _propBlock.GetFloat("_Alpha1");
                    if (opacity1 != 0 && opacity2 != 0)
                    {
                        edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                        _propBlock.SetFloat("_Alpha", Opacity);
                        edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                    }
                }
            }
        }

        if (_preGradient != Gradient)
        {
            _preGradient = Gradient;
            foreach (GameObject edge in AllEdges)
            {
                if (edge)
                {
                    string edgeName = edge.transform.name;
                    string[] nodeList = edgeName.Split('_');
                    edge.transform.parent.Find(nodeList[0]).GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                    edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                    _propBlock.SetFloat("_WhiteBalance", Gradient);
                    edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                    
                }
            }
        }

        if (_prevIsEdgeBundling != isEdgeBundling)
        {
            _prevIsEdgeBundling = isEdgeBundling;
            ClearAllEdges();
            if (isEdgeBundling)
            {
                if (isRecomputingEdgeBundling())
                {
                    gpuEdgeBundling.EdgeBundling();
                    BundledStartNodeList = new List<int>(StartNodeList);
                }
                DrawEdgeBundling(thresholdMin, thresholdMax);
            }
            else
            {
                foreach (int i in StartNodeList)
                {
                    DrawSingleEdge(i, thresholdMin, thresholdMax);
                }
            }
        }

        if(_preTimeStage != TimeStage)
        {
            _edgesMatrix = _connectomeData[TimeStage];
            _preTimeStage = TimeStage;
        }

        if (_DynamicPreviousTime != DynamicCurrentTime)
        {
            if(DynamicCurrentTime!=0)
                DynamicSingleStep(DynamicCurrentTime);
            else
                FrequencySummary();
            _DynamicPreviousTime = DynamicCurrentTime;
        }

        if(preGlyph != Glyph)
        {
            preGlyph = Glyph;
            _nodePrefab.transform.localScale = Glyph * NodePrefabScale;
            foreach(GameObject node in Nodes)
            {
                if (node)
                    node.transform.localScale = Glyph * NodePrefabScale;
            }
        }

        if(_preSpeed != Speed)
        {
            _preSpeed = Speed;
            if (isPlayingDynamicData)
            {
                isPause = true;
                isPlayingDynamicData = false;
                DynamicDataFunction(DynamicCurrentTime, _timeStep);
            }
        }
        if (_PreviousPosNeg != PosNeg && PosNeg)
        {
            PosNegEdgesColorCoding();
            _PreviousPosNeg = PosNeg;
        }
        else if(_PreviousPosNeg !=PosNeg && !PosNeg)
        {
            _PreviousPosNeg = PosNeg;
            RedrawEdges();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            thresholdMin = 0;
            thresholdMax = 1;
            
            ThresholdMinSlider.GetComponent<Michsky.UI.ModernUIPack.RangeMinSlider>().value = 0;
            ThresholdMaxSlider.GetComponent<Michsky.UI.ModernUIPack.RangeMaxSlider>().value = -1;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            thresholdMin = -1;
            thresholdMax = 0;
            
            ThresholdMinSlider.GetComponent<Michsky.UI.ModernUIPack.RangeMinSlider>().value = -1;
            ThresholdMaxSlider.GetComponent<Michsky.UI.ModernUIPack.RangeMaxSlider>().value = 0;
        }
    }

    
    public void attachConnectomeData(int connectomeNumber,Dictionary<string, string[][]> connectomeData, string connectomeName, GameObject nodePrefab, GameObject edgePrefab, float rotationAngle, string[] connectomeRepresentation, string[] connectomeClassification, Dictionary<string, Color> colorCoding, bool isDynamic, int timeStep, float[,] modularityChangeTracking, float[] modularityFrequency)
    {
        _isDynamic = isDynamic;
        _connectomeData = connectomeData;
        _nodePrefab = nodePrefab;
        _edgePrefab = edgePrefab;
        _rotationAngle = rotationAngle;
        _connectomeName = connectomeName;
        _connectomeRepresentation = connectomeRepresentation;
        _connectomeClassification = connectomeClassification;
        _colorCoding = colorCoding;
        _connectomeNumber = connectomeNumber;
        _timeStep = timeStep;
        NodePrefabScale = _nodePrefab.transform.localScale;
        if (connectomeData["NW"] != null)
            _edgesMatrix = connectomeData["NW"];
        else if (connectomeData[TimeStage] != null)
            _edgesMatrix = connectomeData[TimeStage];
        //else if (connectomeData["EdgeBundling" + TimeStage + "_edgePosition"] != null)
        //    _edgesMatrix = connectomeData["EdgeBundling" + TimeStage + "_edgePosition"];
        _nodePrefab.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        _modularityChangeTracking = modularityChangeTracking;
        _modularityFrequency = modularityFrequency;
    }

    public void DrawConnectome(Dictionary<string, string[][]> connectomeData, float connectomeScale, int regionColumn)
    {
        this.transform.parent.transform.localScale = new Vector3(connectomeScale, connectomeScale, connectomeScale);
        int labelKey = 0;
        int label = 0;
        string regionName = string.Empty;
        Nodes = new List<GameObject>();
        _classificationNodes = new Dictionary<string, List<GameObject>>();
        NodeColorList = new List<Color>();
        ModuleList = new List<string>();

        for (int i = 1; i < connectomeData[atlas].Length; i++)
        {
            labelKey = int.Parse(connectomeData[atlas][i][0]);
            if (!labelDictionary.ContainsKey(labelKey))
                labelDictionary.Add(labelKey, i);
        }
        if (_representationType != string.Empty)
        {
            Material nodeMaterial = new Material(Shader.Find("Custom/Node"));
            for (int row = 1; row < connectomeData[_representationType].Length; row++)
            {
                GameObject node = Instantiate(_nodePrefab, new Vector3(float.Parse(connectomeData[_representationType][row][1]), float.Parse(connectomeData[_representationType][row][2]), float.Parse(connectomeData[_representationType][row][3])), Quaternion.identity);
                node.AddComponent<MeshRenderer>();
                label = int.Parse(connectomeData["label"][row][1]);

                connectomeRegionName = connectomeData[atlas][labelDictionary[label]][regionColumn];
                node.GetComponent<Renderer>().material = nodeMaterial;
                float alphaValue = 1;
                if (!isFirstDrawConnectome && !SelectedNode.Contains(row.ToString()))
                {
                    alphaValue = 0;
                }

                //if (row != 39 && row != 40 && row != 41 && row != 42)
                    //alphaValue = 0;
                node.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color1", _colorCoding[connectomeRegionName]);
                _propBlock.SetColor("_Color2", _colorCoding[connectomeRegionName]);
                _propBlock.SetFloat("_Alpha1", alphaValue);
                _propBlock.SetFloat("_Alpha2", alphaValue);

                node.GetComponent<Renderer>().SetPropertyBlock(_propBlock);

                NodeColorList.Add(_colorCoding[connectomeRegionName]);
                Vector3 tempPosition = node.transform.position;
                Vector3 tempScale = node.transform.localScale;
                Quaternion tempRotation = node.transform.localRotation;
                node.transform.parent = this.transform;
                node.transform.localScale = tempScale;
                node.transform.localRotation = tempRotation;
                node.transform.localPosition = tempPosition;
                node.name = row.ToString();
                node.tag = "Node";

                if (sceneName == "Selection")
                    node.GetComponent<BoxCollider>().enabled = false;
                else node.GetComponent<BoxCollider>().enabled = true;

                Nodes.Add(node);
                if (!_classificationNodes.ContainsKey(connectomeRegionName))
                {
                    ModuleList.Add(connectomeRegionName);
                    _classificationNodes.Add(connectomeRegionName, new List<GameObject>());
                }
                _classificationNodes[connectomeRegionName].Add(node);

                if (sceneName == "Comparison")
                {
                    node.AddComponent<SingleNodeBehaviour>();
                }
            }
            //print(Nodes.Count);
            if (sceneName == "Selection")
            {
                GameObject sphere = Instantiate(_SelectSphere, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.identity);
                Vector3 sphereScale = sphere.transform.localScale;
                sphere.transform.parent = this.transform;
                sphere.SetActive(false);
                sphere.tag = "SelectionConnectome";
                sphere.name = "SelectionSphere";
                sphere.transform.localScale = sphereScale;
            }
            
        }
    }

    public void DwellingTimeSummary()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i])
            {
                List<float> modlarityDistribution = new List<float>();
                for (int j = 0; j < _modularityChangeTracking.GetLength(1); j++)
                {
                    modlarityDistribution.Add(_modularityChangeTracking[i, j]);
                }
                int maxMod = int.Parse(Mathf.Floor(modlarityDistribution.Max() / 0.1f).ToString());
                if (maxMod == 10)
                    maxMod = 9;

                Nodes[i].GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color1", SpectrumColors[maxMod]);
                _propBlock.SetColor("_Color2", SpectrumColors[maxMod]);
                Nodes[i].GetComponent<Renderer>().SetPropertyBlock(_propBlock);
            }

        }

    }


    public void FrequencySummary()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i])
            {
                int Freq = int.Parse(Mathf.Floor(_modularityFrequency[i] *4 / 0.1f).ToString());
                if (Freq >= 10)
                    Freq = 9;

                Nodes[i].GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color1", SpectrumColors[Freq]);
                _propBlock.SetColor("_Color2", SpectrumColors[Freq]);
                Nodes[i].GetComponent<Renderer>().SetPropertyBlock(_propBlock);
            }

        }

    }
    public void RedrawEdges()
    {
        ClearAllEdges();

        if (isEdgeBundling)
            DrawEdgeBundling(thresholdMin, thresholdMax);
        else
        {
            foreach (int i in StartNodeList)
            {
                DrawSingleEdge(i, thresholdMin, thresholdMax);
            }
        }
    }

    public void PosNegEdgesColorCoding()
    {
        foreach(GameObject edge in AllEdges)
        {
            if(edge)
            {
                float connectivity = _edgeConnectivity[edge];
                int connectivityToInt = 0;
                Color32 edgeColor;
                if(connectivity >0 )
                    connectivityToInt  = int.Parse(Mathf.Floor(connectivity / 0.1f).ToString());
                else connectivityToInt = int.Parse(Mathf.Ceil(connectivity / 0.1f).ToString());
                if (connectivityToInt >= 9)
                    connectivityToInt = 9;
                else if (connectivityToInt <= -9)
                    connectivityToInt = -9;

                if (connectivityToInt > 0)
                    edgeColor = PositiveEdges[connectivityToInt];
                else
                    edgeColor = NegativeEdges[Mathf.Abs(connectivityToInt)];
                edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color1", edgeColor);
                _propBlock.SetColor("_Color2", edgeColor);
                edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
            }
        }
    }

    public bool isEdgeExisted(int startNode)
    {
        if (StartNodeList.Contains(startNode))
            return true;
        else 
            return false;
    }


    public bool isNodeSelected(string nodeName)
    {
        if (SelectedNode.Contains(nodeName))
            return true;
        else
            return false;
    }

    public void DestroyStraightEdge(int startNode)
    {
        foreach (GameObject edge in NodeStartStraightEdgeStatus[startNode])
            if (edge != null)
                Destroy(edge);
        NodeStartStraightEdgeStatus.Remove(startNode);
    }

    public void AddSelectedNodetoList(Transform nodeTransform)
    {
        string Name = nodeTransform.name;
        if (!SelectedNode.Contains(Name))
            SelectedNode.Add(Name);
        else
            SelectedNode.Remove(Name);
    }

    public void HighLightSelectedNode()
    {
        foreach(GameObject node in Nodes)
        {
            string name = node.name;
            node.transform.localScale = Glyph * NodePrefabScale;
            node.transform.GetComponent<SingleNodeBehaviour>().isEnlarged = false;
            if (!SelectedNode.Contains(name))
            {
                node.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetFloat("_Alpha1", 0);
                _propBlock.SetFloat("_Alpha2", 0);
                node.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                EdgesActionForSelectedNode(int.Parse(name), 0);
            }

        }
    }

    public void showAllNode()
    {
        foreach (GameObject node in Nodes)
        {
            string name = node.name;
            node.transform.localScale = Glyph * NodePrefabScale;
            node.transform.GetComponent<SingleNodeBehaviour>().isEnlarged = false;
            node.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_Alpha1", 1);
            _propBlock.SetFloat("_Alpha2", 1);
            node.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
            EdgesActionForSelectedNode(int.Parse(name), 1);
        }

    }

    public void SelectAllNode()
    {
        foreach(GameObject node in Nodes)
        {
            string Name = node.name;
            if (!SelectedNode.Contains(Name))
                SelectedNode.Add(Name);
        }
    }

    public void EdgesActionForSelectedNode(int NodeName, float opacity)
    {
        string alpha;

        if (_connectomeNumber == 1)
            alpha = "_Alpha1";
        else alpha = "_Alpha2";

        foreach (GameObject edge in AllEdges)
        {
            if (edge)
            {
                if (edge.transform.tag == "Edge")
                {
                    string Name = edge.transform.name;
                    {
                        string[] edgeName = edge.name.Split('_');
                        int EdgeStartNumber = int.Parse(edgeName[0]);
                        int EdgeEndNumber = int.Parse(edgeName[1]);
                        Nodes[EdgeStartNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                        float startNodeOpacity = _propBlock.GetFloat(alpha);
                        Nodes[EdgeEndNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                        float endNodeOpacity = _propBlock.GetFloat(alpha);
                        float currentOpacity = Mathf.Min(startNodeOpacity, endNodeOpacity);
                        currentOpacity = Mathf.Min(Opacity, currentOpacity);
                        if (EdgeStartNumber == NodeName || EdgeEndNumber == NodeName)
                        {
                            edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                            _propBlock.SetFloat("_Alpha", opacity);
                            edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                        }
                    }
                }
            }
        }
    }

    public void DrawSingleEdge(int nodeNumber, float newThresholdMin, float newThresholdMax)
    {
        Vector3 startPosition = new Vector3();
        Vector3 endPosition = new Vector3();
        Vector3 scale = new Vector3();
        Vector3 offset = new Vector3();
        Vector3 edgePosition = new Vector3();
        float connectivityScale  = 0;
        Material edgeMaterial = new Material(Shader.Find("Custom/EdgeGradient"));

        if (_connectomeData["NW"] != null)
        {
            _edgesMatrix = _connectomeData["NW"];
            connectivityScale = 0.1f;
        }      
        else if (_connectomeData[TimeStage] != null)
        {
            _edgesMatrix = _connectomeData[TimeStage];
            connectivityScale = 0.01f;
        }

        if (_connectomeNumber == 2)
            dashAmount = 40f;
        else dashAmount = 0;

        if (!NodeStartStraightEdgeStatus.ContainsKey(nodeNumber))
            NodeStartStraightEdgeStatus.Add(nodeNumber, new List<GameObject>());
        for (int i = 0; i < _edgesMatrix[nodeNumber].Length; i++)
        {
            float connectivity = float.Parse(_edgesMatrix[nodeNumber][i]);
            float absConnectivity = Mathf.Abs(connectivity);
            
            if (connectivity < newThresholdMax && connectivity > newThresholdMin && connectivity != 0)
            {
                startPosition = new Vector3(Nodes[nodeNumber - 1].transform.position.x, Nodes[nodeNumber - 1].transform.position.y, Nodes[nodeNumber - 1].transform.position.z);
                endPosition = new Vector3(Nodes[i].transform.position.x, Nodes[i].transform.position.y, Nodes[i].transform.position.z);
                offset = endPosition - startPosition;
                edgePosition = startPosition + offset / 2.0f;
                scale = new Vector3(Mathf.Max(Mathf.Min(absConnectivity * connectivityScale, 0.01f), 0.0005f), offset.magnitude / 2.0f, Mathf.Max(Mathf.Min(absConnectivity * connectivityScale, 0.01f), 0.0005f));

                Nodes[nodeNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                Color edgeColor1 = _propBlock.GetColor("_Color1");
                float opacity1 = _propBlock.GetFloat("_Alpha1");

                Nodes[i].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                Color edgeColor2 = _propBlock.GetColor("_Color2");
                float opacity2 = _propBlock.GetFloat("_Alpha1");
                float opacity = Mathf.Min(opacity1, opacity2);

                GameObject edge = Instantiate(_edgePrefab, edgePosition, Quaternion.identity);
                edge.AddComponent<MeshRenderer>();
                edge.GetComponent<Renderer>().material = edgeMaterial;
                edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetFloat("_isEdgeBundling", 0);
                if (connectivity<0)
                    _propBlock.SetFloat("_isNegativeConnectivity", 1);
                else
                    _propBlock.SetFloat("_isNegativeConnectivity", 0);

                if (PosNeg)
                {
                    int connectivityToInt = 0;
                    if (connectivity > 0)
                        connectivityToInt = int.Parse(Mathf.Floor(connectivity / 0.1f).ToString());
                    else connectivityToInt = int.Parse(Mathf.Ceil(connectivity / 0.1f).ToString());
                    if (connectivityToInt >= 9)
                        connectivityToInt = 9;
                    else if (connectivityToInt <= -9)
                        connectivityToInt = -9;

                    if (connectivityToInt > 0)
                    {
                        edgeColor1 = PositiveEdges[connectivityToInt];
                        edgeColor2 = PositiveEdges[connectivityToInt];
                    } 
                    else
                    {
                        edgeColor1 = NegativeEdges[Mathf.Abs(connectivityToInt)];
                        edgeColor2 = NegativeEdges[Mathf.Abs(connectivityToInt)];
                    }  
                }
                
                _propBlock.SetColor("_Color1", edgeColor1);
                _propBlock.SetColor("_Color2", edgeColor2);
                _propBlock.SetFloat("_Alpha", Mathf.Min(Opacity, opacity));
                _propBlock.SetFloat("_WhiteBalance", Gradient);
                _propBlock.SetFloat("_DashAmount", dashAmount);
                edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);

                string edgeName = nodeNumber + "_" + (i + 1);
                edge.transform.name = edgeName;
                Quaternion tempRotation = edge.transform.localRotation;
                edge.transform.localScale = scale;
                edge.transform.parent = this.transform;
                edge.transform.tag = "Edge";
                edge.transform.localRotation = tempRotation;
                edge.transform.up = offset;
                AllEdges.Add(edge);
                NodeStartStraightEdgeStatus[nodeNumber].Add(edge);

                if (!_edgeConnectivity.ContainsKey(edge))
                    _edgeConnectivity.Add(edge, 0);
                _edgeConnectivity[edge]= connectivity;
            }
        }

    }

    public void DrawEdgeBundling(float newThresholdMin, float newThresholdMax)
    {
        List<Vector3[]> SmoothingEdges = TextureToList();
        Vector3 startPosition = new Vector3();
        Vector3 endPosition = new Vector3();
        Vector3 scale = new Vector3();
        Vector3 offset = new Vector3();
        Vector3 edgePosition = new Vector3();
        Material edgeMaterial = new Material(Shader.Find("Custom/EdgeGradient"));
        float connectivityScale = 0;
        if (StartNodeList.Count == 0)
            SmoothingEdges = new List<Vector3[]>();
        if (SmoothingEdges.Count>0)
        {
            if (_connectomeData["NW"] != null)
            {
                _edgesMatrix = _connectomeData["NW"];
                connectivityScale = 0.1f;
            }
            else if (_connectomeData[TimeStage] != null)
            {
                _edgesMatrix = _connectomeData[TimeStage];
                connectivityScale = 0.01f;
            }

            if (_connectomeNumber == 2)
                dashAmount = 2f;
            else dashAmount = 0;
            for (int i = 0; i < SmoothingEdges.Count; i++)
            {
                int startNodeNumber = gpuEdgeBundling.edgeDataMatrix[i].startNodeID;
                int endNodeNumber = gpuEdgeBundling.edgeDataMatrix[i].endNodeID;
                float connectivity = gpuEdgeBundling.edgeDataMatrix[i].connectivity;
                Color32 startNodeColor = NodeColorList[startNodeNumber - 1];
                Color32 endNodeColor = NodeColorList[endNodeNumber - 1];
                Nodes[startNodeNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                float opacity1 = _propBlock.GetFloat("_Alpha1");
                Nodes[endNodeNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                float opacity2 = _propBlock.GetFloat("_Alpha1");
                float opacity = Mathf.Min(opacity1, opacity2);
                if (connectivity < newThresholdMax && connectivity > newThresholdMin && connectivity != 0)
                {
                    float absConnectivity = Mathf.Abs(connectivity);
                    int edgeSegment = SmoothingEdges[i].Length;
                    for (int j = 0; j < edgeSegment-1; j++)
                    {
                        startPosition = SmoothingEdges[i][j];
                        endPosition = SmoothingEdges[i][j+1];
                        //startPosition = new Vector3(tempStartPosition.r, tempStartPosition.g, tempStartPosition.b);
                        //endPosition = new Vector3(tempEndPosition.r, tempEndPosition.g, tempEndPosition.b);

                        startPosition = this.transform.TransformPoint(startPosition);
                        endPosition = this.transform.TransformPoint(endPosition);
                        offset = endPosition - startPosition;
                        edgePosition = startPosition + offset / 2.0f;
                        //scale = new Vector3(0.004f * connectivity * connectivityScale, offset.magnitude / 2.0f, 0.004f * connectivity * connectivityScale);
                        scale = new Vector3(Mathf.Max(Mathf.Min(absConnectivity * connectivityScale, 0.005f), 0.0005f), offset.magnitude / 2.0f, Mathf.Max(Mathf.Min(absConnectivity * connectivityScale, 0.005f), 0.0005f));
                        GameObject edge = Instantiate(_edgePrefab, edgePosition, Quaternion.identity);

                        edge.transform.localScale = scale;
                        edge.transform.parent = this.transform;
                        edge.transform.up = offset;
                        string edgeName = startNodeNumber + "_" + endNodeNumber;
                        edge.transform.name = edgeName;
                        edge.transform.tag = "Edge";
                        Color32 segmentColor;
                        segmentColor.r = (byte)(((float)(j + 1) / (float)edgeSegment) * (float)(endNodeColor.r - startNodeColor.r) + startNodeColor.r);
                        segmentColor.g = (byte)(((float)(j + 1) / (float)edgeSegment) * (float)(endNodeColor.g - startNodeColor.g) + startNodeColor.g);
                        segmentColor.b = (byte)(((float)(j + 1) / (float)edgeSegment) * (float)(endNodeColor.b - startNodeColor.b) + startNodeColor.b);


                        Color32 tempColor;

                        //if (connectivity < 0 && (j == 15 || j == 16 || j == 17))
                        //    tempColor = new Color32(255, 0, 0, 255);
                        //else if (connectivity > 0 && (j == 15 || j == 16 || j == 17))
                        //    tempColor = new Color32(0, 255, 0, 255);
                        //else
                        tempColor = new Color32(segmentColor.r, segmentColor.g, segmentColor.b, 255);


                        if (PosNeg)
                        {
                            int connectivityToInt = 0;
                            if (connectivity > 0)
                                connectivityToInt = int.Parse(Mathf.Floor(connectivity / 0.1f).ToString());
                            else connectivityToInt = int.Parse(Mathf.Ceil(connectivity / 0.1f).ToString());
                            if (connectivityToInt >= 9)
                                connectivityToInt = 9;
                            else if (connectivityToInt <= -9)
                                connectivityToInt = -9;

                            if (connectivityToInt > 0)
                                tempColor = PositiveEdges[connectivityToInt];
                            else
                                tempColor = NegativeEdges[Mathf.Abs(connectivityToInt)];
                        }

                        edge.AddComponent<MeshRenderer>();
                        edge.GetComponent<Renderer>().material = edgeMaterial;
                        edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                        _propBlock.SetFloat("_isEdgeBundling", 1);
                        _propBlock.SetInt("_edgeNumber", j);
                        if (connectivity < 0)
                            _propBlock.SetFloat("_isNegativeConnectivity", 1);
                        else
                            _propBlock.SetFloat("_isNegativeConnectivity", 0);
                        _propBlock.SetColor("_Color1", tempColor);
                        _propBlock.SetColor("_Color2", tempColor);
                        _propBlock.SetFloat("_Alpha", Mathf.Min(Opacity, opacity));
                        _propBlock.SetFloat("_WhiteBalance", Gradient);
                        _propBlock.SetFloat("_DashAmount", dashAmount);
                        edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);

                        AllEdges.Add(edge);

                        if (!_edgeConnectivity.ContainsKey(edge))
                            _edgeConnectivity.Add(edge, 0);
                        _edgeConnectivity[edge] = connectivity;
                    }
                }
            }
        }
    }
    public List<Vector3[]> TextureToList()
    {
        int segmentNum = (int)Mathf.Pow(2, edgeBundlingIterations);
        List<Vector3[]> EdgePosition = new List<Vector3[]>();
        Texture2D bundledEdges = gpuEdgeBundling.outputPositionTexture;
        if (bundledEdges)
        {
            for (int i = 0; i < bundledEdges.height; i++)
            {
                Vector3[] Edge = new Vector3[segmentNum+1];
                for (int j = 0; j < segmentNum+1; j++)
                {
                    Edge[j] = new Vector3(bundledEdges.GetPixel(j, i).r, bundledEdges.GetPixel(j, i).g, bundledEdges.GetPixel(j, i).b);
                }
                EdgePosition.Add(Edge);
            }
        }
        return EdgePosition;
    }

    public List<Vector3[]> CurveSmoothing(List<Vector3[]> inputCurve)
    {
        List<Vector3[]> SmoothedEdges = new List<Vector3[]>();
        if (inputCurve.Count>0)
        {
            for (int i = 0; i < inputCurve.Count; i++)
            {
                int inputCurveLength = inputCurve[i].Length;
                Vector3[] CurvedEdge = new Vector3[(inputCurveLength - 2)*2+2];
                CurvedEdge[0] = inputCurve[i][0];
                CurvedEdge[CurvedEdge.Length - 1] = inputCurve[i][inputCurveLength - 1];
                int k = 1;
                for (int j = 0; j < inputCurveLength - 2; j++)
                {
                    CurvedEdge[k] = inputCurve[i][j] + (inputCurve[i][j+1] - inputCurve[i][j]) * 0.75f;
                    CurvedEdge[k + 1] = inputCurve[i][j+1] + (inputCurve[i][j+2] - inputCurve[i][j+1]) * 0.25f;
                    k += 2;
                }
                SmoothedEdges.Add(CurvedEdge);
            }
        }
        return SmoothedEdges;
    }

    public void DynamicDataFunction(int startTime, int endTime)
    {
        if (endTime > startTime && !isPlayingDynamicData)
            StartCoroutine(DrawDynamicConnectome(startTime, endTime));
    }

    public void DynamicSingleStep (int TimeStep)
    {
        DynamicCurrentTime = TimeStep;
        ClearAllNodes();
        atlas = "Mod_" + TimeStep;
        TimeStage = "Time_" + TimeStep;
        DrawConnectome(_connectomeData, connectomeUpdatedScale, 1);
        ClearAllEdges();
        if (isEdgeBundling)
        {
            gpuEdgeBundling.EdgeBundling();
            DrawEdgeBundling(thresholdMin, thresholdMax);
        }
            
        else
        {
            foreach (int j in StartNodeList)
            {
                DrawSingleEdge(j, thresholdMin, thresholdMax);
            }
        }
        
    }

    IEnumerator DrawDynamicConnectome(int startTime, int endTime)
    {
        isPlayingDynamicData = true;
        for (int i = startTime; i < endTime + 1; i++)
        {
            if (isPause)
            {
                isPause = false;
                isPlayingDynamicData = false;
                DynamicCurrentTime = i-1;
                _DynamicPreviousTime = i - 1;
                yield break;
            }
            else
            {
                DynamicCurrentTime = i;
                _DynamicPreviousTime = i;
                atlas = "Mod_" + i;
                TimeStage = "Time_" + i;
                ClearAllNodes();
                ClearAllEdges();
                DrawConnectome(_connectomeData, connectomeUpdatedScale, 1);

                if (isEdgeBundling)
                {
                    gpuEdgeBundling.EdgeBundling();
                    DrawEdgeBundling(thresholdMin, thresholdMax);
                }
                    
                else
                {
                    foreach (int j in StartNodeList)
                    {
                        DrawSingleEdge(j, thresholdMin, thresholdMax);
                    }
                }
                
                yield return new WaitForSeconds(Speed);
            }      
        }
        isPlayingDynamicData = false;
        yield return null;
    }

    public void backToStaticState()
    {
        atlas = "LookupTable";
        _classificationType = _connectomeClassification[0];
        DynamicCurrentTime = 0;
    }

    public void ClearAllEdges()
    {
        foreach (GameObject edge in AllEdges)
            if (edge != null)
                Destroy(edge);
    }

    public void ClearAllNodes()
    {
        foreach (GameObject node in Nodes)
            if (node != null)
                Destroy(node);
    }

    public bool isRecomputingEdgeBundling()
    {
        if (BundledStartNodeList.SequenceEqual(StartNodeList))
            return false;
        else return true;
    }
}

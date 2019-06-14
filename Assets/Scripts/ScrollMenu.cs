using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ScrollMenu : MonoBehaviour
{

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    public string connectomeNumber;
    private DataLoader connectomeDataLoader;
    private DesktopController DC;
    private GameObject connectomeParent;
    
    private string[] _preLegendList;
    private string[] _preSelectedNode;
    private List<GameObject> RepresentationButtonList;
    private List<GameObject> ClassificationButtonList;
    private List<GameObject> LegendButtonList;
    private GameObject _ScrollText;
    private GameObject _ScrollTextWithColorBox;
    private Dictionary<string, Color> ColorCoding = new Dictionary<string, Color>();
    private string connectomeName;
    private int isDuplicate;
    private string scrollMenu;
    private bool isFirstUpdate = true;
    private int EdgeStartNumber ;
    private int EdgeEndNumber ;
    private List<GameObject> LegendItems = new List<GameObject>();
    private List<GameObject> SelectedNodeItems = new List<GameObject>();
    private Dictionary<string, float> regionOpacity = new Dictionary<string, float>();
    private Dictionary<string, float> previousRegionOpacity = new Dictionary<string, float>();
    private Renderer edgeRend;
    private float edgeTransparencyOpacity = 0f;
    private MaterialPropertyBlock _propBlock;
    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        _propBlock = new MaterialPropertyBlock();

        scrollMenu = this.transform.name;
        connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
        isDuplicate = PlayerPrefs.GetInt("isDuplicate");
        connectomeDataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
        DC = GameObject.Find("Canvas").GetComponent<DesktopController>();
        RepresentationButtonList = new List<GameObject>();
        ClassificationButtonList = new List<GameObject>();
        LegendButtonList = new List<GameObject>();
        _ScrollText = Resources.Load("Prefabs/ListItemButton") as GameObject;
        _ScrollTextWithColorBox = Resources.Load("Prefabs/ListItemButtonWithColorBox") as GameObject;
        string[] representations = connectomeDataLoader.connectomeRepresentationGlobalDictionary[connectomeName].ToArray();
        string[] classifications = connectomeDataLoader.connectomeClassificationGlobalDictionary[connectomeName].ToArray();
        ColorCoding = connectomeDataLoader.colorCodingGlobal;
        
        foreach(string regionName in ColorCoding.Keys)
        {
            regionOpacity.Add(regionName, 1);
            previousRegionOpacity.Add(regionName, 1);
        }
            

        if (scrollMenu == "Representation")
        {
            for (int i = 0; i < representations.Length; i++)
            {
                GameObject ScrollText = Instantiate(_ScrollText, new Vector3(0, 0, 0), Quaternion.identity);
                ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = representations[i];
                Vector3 scale = ScrollText.transform.localScale;
                ScrollText.transform.name = representations[i];
                ScrollText.transform.tag = "Representation";
                ScrollText.transform.parent = this.transform;
                ScrollText.transform.localScale = scale;
                RepresentationButtonList.Add(ScrollText);
                if(i == 0)
                    ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
            }
        }

        else if (scrollMenu == "Classification")
        {
            for (int i = 0; i < classifications.Length; i++)
            {
                GameObject ScrollText = Instantiate(_ScrollText, new Vector3(0, 0, 0), Quaternion.identity);
                ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = classifications[i];
                Vector3 scale = ScrollText.transform.localScale;
                ScrollText.transform.tag = "Classification";
                ScrollText.transform.parent = this.transform;
                ScrollText.transform.name = classifications[i];
                ScrollText.transform.localScale = scale;
                ClassificationButtonList.Add(ScrollText);
                if (i == 0)
                    ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f); 
            }
        }
    }

    void Update()
    {
        if (isFirstUpdate)
        {
            if (isDuplicate == 0)
                connectomeParent = GameObject.Find(connectomeName);
            else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
                connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectome");
            else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
                connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");

            _preLegendList = connectomeParent.GetComponent<SingleConnectome>().ModuleList.ToArray();
            _preSelectedNode = connectomeParent.GetComponent<SingleConnectome>().previousSelectedNode.ToArray();
            isFirstUpdate = !isFirstUpdate;
            if(scrollMenu == "Classification")
                connectomeParent.GetComponent<SingleConnectome>().classificationGameObject = this.gameObject;
        }
        string[] LegendList = connectomeParent.GetComponent<SingleConnectome>().ModuleList.ToArray();
        if (!_preLegendList.SequenceEqual(LegendList))
        {
            foreach(GameObject LegendItem in LegendItems)
            {
                if(LegendItem!= null)
                    Destroy(LegendItem);
            }
            if (scrollMenu == "Legend")
            {
                _preLegendList = LegendList;
                for (int i = 0; i < LegendList.Length; i++)
                {
                    GameObject ScrollText = Instantiate(_ScrollTextWithColorBox, new Vector3(0, 0, 0), Quaternion.identity);
                    LegendItems.Add(ScrollText);
                    ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = LegendList[i];
                    ScrollText.transform.Find("ColorBox").GetComponent<Image>().color = ColorCoding[LegendList[i]];
                    Vector3 scale = ScrollText.transform.localScale;
                    ScrollText.transform.name = LegendList[i];
                    ScrollText.transform.tag = "Legend";
                    ScrollText.transform.parent = this.transform;
                    ScrollText.transform.localScale = scale;
                    LegendButtonList.Add(ScrollText);
                }
            }
        }

        string[] seletedNode = connectomeParent.GetComponent<SingleConnectome>().SelectedNode.ToArray();
        if (!_preSelectedNode.SequenceEqual(seletedNode))
        {
            foreach (GameObject selectedNodeItem in SelectedNodeItems)
            {
                if (selectedNodeItem != null)
                    Destroy(selectedNodeItem);
            }
            if (scrollMenu == "SelectNode")
            {
                _preSelectedNode = seletedNode;
                for (int i = 0; i < seletedNode.Length; i++)
                {
                    GameObject ScrollText = Instantiate(_ScrollTextWithColorBox, new Vector3(0, 0, 0), Quaternion.identity);
                    SelectedNodeItems.Add(ScrollText);
                    ScrollText.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = seletedNode[i].ToString();
                    //ScrollText.transform.Find("ColorBox").GetComponent<Image>().color = ColorCoding[LegendList[i]];
                    Vector3 scale = ScrollText.transform.localScale;
                    ScrollText.transform.name = seletedNode[i].ToString();
                    //ScrollText.transform.tag = "SelectedNode";
                    ScrollText.transform.parent = this.transform;
                    ScrollText.transform.localScale = scale;
                    //LegendButtonList.Add(ScrollText);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();
            
            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.tag == "Representation")
                {
                    DC.ChangeRepresentation(connectomeParent, result.gameObject.name);
                    result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                    foreach (GameObject representation in RepresentationButtonList)
                    {
                        if(representation.transform.name != result.gameObject.name)
                            representation.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1,1);
                    }
                }

                if (result.gameObject.tag == "Classification")
                {
                    DC.ChangeClassification(connectomeParent, result.gameObject.name);
                    result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                    if (result.gameObject.name == "Dynamic Modularity")
                    {
                        int currentTime = connectomeParent.GetComponent<SingleConnectome>().DynamicCurrentTime ;
                        int previousTime = connectomeParent.GetComponent<SingleConnectome>().DynamicCurrentTime + 1 ;
                        connectomeParent.GetComponent<SingleConnectome>()._DynamicPreviousTime = previousTime;
                    }
                        

                    foreach (GameObject classification in ClassificationButtonList)
                    {
                        if (classification.transform.name != result.gameObject.name)
                            classification.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1);
                    }
                }
                
                if(result.gameObject.tag == "Legend")
                {
                    string regionName = result.gameObject.name;
                    Color buttonColor = result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color;
                    Color textColor = result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color;
                    float _previousRegionOpacity = previousRegionOpacity[regionName];
                    float _regionOpacity = regionOpacity[regionName];
                    GameObject[] nodeInRegion = connectomeParent.GetComponent<SingleConnectome>()._classificationNodes[regionName].ToArray();
                    Dictionary<int, List<GameObject>> EdgeOnStartNode = connectomeParent.GetComponent<SingleConnectome>().NodeStartStraightEdgeStatus;
                    GameObject[] nodes = connectomeParent.GetComponent<SingleConnectome>().Nodes.ToArray();

                    foreach (GameObject Node in nodeInRegion)
                    {
                        int NodeName = int.Parse(Node.transform.name);

                        Node.transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                        
                        if (_previousRegionOpacity == 1 && _regionOpacity == 1)
                        {
                            _propBlock.SetFloat("_Alpha1", 0.3f);
                            _propBlock.SetFloat("_Alpha2", 0.3f);
                            regionOpacity[regionName] = 0.3f;

                            buttonColor.a = 0.3f;
                            result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color = buttonColor;
                            textColor.a = 0.5f;
                            result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
                        }
                        if (_previousRegionOpacity == 1 && _regionOpacity == 0.3f)
                        {
                            _propBlock.SetFloat("_Alpha1", edgeTransparencyOpacity);
                            _propBlock.SetFloat("_Alpha2", edgeTransparencyOpacity);
                            regionOpacity[regionName] = edgeTransparencyOpacity;
                            previousRegionOpacity[regionName] = 0.3f;

                            buttonColor.a = 0.1f;
                            result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color = buttonColor;
                            textColor.a = 0.1f;
                            result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
                        }
                        if (_previousRegionOpacity == 0.3f && _regionOpacity == edgeTransparencyOpacity)
                        {
                            _propBlock.SetFloat("_Alpha1", 0.3f);
                            _propBlock.SetFloat("_Alpha2", 0.3f);
                            regionOpacity[regionName] = 0.3f;
                            previousRegionOpacity[regionName] = edgeTransparencyOpacity;

                            buttonColor.a = 0.3f;
                            result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color = buttonColor;
                            textColor.a = 0.5f;
                            result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
                        }
                        if (_previousRegionOpacity == edgeTransparencyOpacity && _regionOpacity == 0.3f)
                        {
                            _propBlock.SetFloat("_Alpha1", 1);
                            _propBlock.SetFloat("_Alpha2", 1);
                            regionOpacity[regionName] = 1;
                            previousRegionOpacity[regionName] = 0.3f;

                            buttonColor.a = 1f;
                            result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color = buttonColor;
                            textColor.a = 1f;
                            result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
                        }
                        if (_previousRegionOpacity == 0.3f && _regionOpacity == 1f)
                        {
                            _propBlock.SetFloat("_Alpha1", 0.3f);
                            _propBlock.SetFloat("_Alpha2", 0.3f);
                            regionOpacity[regionName] = 0.3f;
                            previousRegionOpacity[regionName] = 1;

                            buttonColor.a = 0.3f;
                            result.gameObject.transform.Find("ColorBox").GetComponent<Image>().color = buttonColor;
                            textColor.a = 0.5f;
                            result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
                        }
                        Node.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                        string alpha;
                        
                        if (int.Parse(connectomeNumber) == 0)
                            alpha = "_Alpha1";
                        else alpha = "_Alpha2";

                        float AlphaValue;
                        Node.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                        AlphaValue = _propBlock.GetFloat(alpha);
                        GameObject[] edges;
                        if (GameObject.Find("connectomeOverlay") != null)
                        {
                            GameObject overlayComparison = GameObject.Find("connectomeOverlay");
                            GameObject[] OverlayNodes = GameObject.FindGameObjectsWithTag("NodeComparison");
                            OverlayNodes[NodeName-1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                            _propBlock.SetFloat(alpha, AlphaValue);
                            OverlayNodes[NodeName-1].GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                        }
                        edges = connectomeParent.GetComponent<SingleConnectome>().AllEdges.ToArray();
                        float Opacity = connectomeParent.GetComponent<SingleConnectome>().Opacity;
                        foreach (GameObject edge in edges)
                        {
                            if (edge)
                            {
                                if (edge.transform.tag == "Edge")
                                {
                                    string Name = edge.transform.name;
                                    {
                                        string[] edgeName = edge.name.Split('_');
                                        EdgeStartNumber = int.Parse(edgeName[0]);
                                        EdgeEndNumber = int.Parse(edgeName[1]);
                                        nodes[EdgeStartNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                        float startNodeOpacity = _propBlock.GetFloat(alpha);
                                        nodes[EdgeEndNumber - 1].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                        float endNodeOpacity = _propBlock.GetFloat(alpha);
                                        float currentOpacity = Mathf.Min(startNodeOpacity, endNodeOpacity);
                                        currentOpacity = Mathf.Min(Opacity, currentOpacity);
                                        if (EdgeStartNumber == NodeName || EdgeEndNumber == NodeName)
                                        {
                                            edge.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                            if (_previousRegionOpacity == 1 && _regionOpacity == 1)
                                                _propBlock.SetFloat("_Alpha", currentOpacity / 2);
                                            if (_previousRegionOpacity == 1 && _regionOpacity == 0.3f)
                                                _propBlock.SetFloat("_Alpha", edgeTransparencyOpacity);
                                            if (_previousRegionOpacity == 0.3f && _regionOpacity == edgeTransparencyOpacity)
                                                _propBlock.SetFloat("_Alpha", currentOpacity / 2);
                                            if (_previousRegionOpacity == edgeTransparencyOpacity && _regionOpacity == 0.3f)
                                                _propBlock.SetFloat("_Alpha", currentOpacity);
                                            if (_previousRegionOpacity == 0.3f && _regionOpacity == 1f)
                                                _propBlock.SetFloat("_Alpha", currentOpacity / 2);
                                            edge.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                }
            }
            
        }
    }
}

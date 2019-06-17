using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DesktopController : MonoBehaviour
{

    public float cameraHorizontalSpeed = 2.0f;
    public float cameraScrollSpeed = 1.0f;
    public float connectomeRotateSpeed = 5.0f;
    public List<string> selectedConnectome = new List<string>();
    public GameObject nodePrefab;
    public float connectomeHorizontalSpeed = 2.0f;
    public float connectomeVerticalSpeed = 2.0f;
    public List<GameObject> ComparisonNodeList = new List<GameObject>();
    public bool isRepresenationChanged = false;

    public GameObject Warnings;
    private int timeStep1 = 0;
    private int timeStep2 = 0;
    private Camera _cam;
    private int _currentScene;
    private enum _scenesList { Selection, Comparison };

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private int nConnectomeForDesktop = 2;
    private DataLoader dataLoader;
    private bool isDynamic1;
    private bool isDynamic2;
    private bool initialRotationClick = true;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    private float EdgeThresholdMin = 0.5f;
    private float EdgeThresholdMax = 1;
    private bool isCompare = true;
    private bool isInspect = false;
    private MaterialPropertyBlock _propBlock;
    private bool isFirstUpdate = true;
    //public GameObject _cube;
    public GameObject syncLeft;
    public GameObject syncRight;
    public List<GameObject> LeftDynamicControllers;
    public List<GameObject> RightDynamicControllers;
    public GameObject leftTimeStep;
    public GameObject rightTimeStep;
    Scene currentScene;
    string sceneName;
    GameObject[] singleConnectomes;
    GameObject Connectome1;
    GameObject Connectome2;
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        _currentScene = SceneManager.GetActiveScene().buildIndex;
        _propBlock = new MaterialPropertyBlock();
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        //if (_cube)
        //_cube.SetActive(false);
        currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

    }
    
    void CameraAction()
    {
        if (Input.GetMouseButton(2))
            _cam.transform.Rotate(cameraHorizontalSpeed * -Input.GetAxis("Mouse Y"), cameraHorizontalSpeed * Input.GetAxis("Mouse X"), 0);

        _cam.transform.Translate(0, 0, cameraScrollSpeed * Input.GetAxis("Mouse ScrollWheel"), Space.Self);
    }

    void SelectionUI()
    {

        if (Input.GetKey(KeyCode.Return))
            isInspect = true;

        if (isInspect && selectedConnectome.Count <= 2 && selectedConnectome.Count > 0)
        {
            isInspect = false;
            if (selectedConnectome.Count == 1)
            {
                PlayerPrefs.SetInt("Count", 2);
                PlayerPrefs.SetString("SelectedConnectome_" + 0, selectedConnectome[0]);
                PlayerPrefs.SetString("SelectedConnectome_" + 1, selectedConnectome[0]);
                PlayerPrefs.SetInt("isDuplicate", 1);
            }
            else if (selectedConnectome.Count == 2)
            {
                PlayerPrefs.SetInt("count", selectedConnectome.Count);
                PlayerPrefs.SetInt("isDuplicate", 0);
                for (int i = 0; i < selectedConnectome.Count; i++)
                    PlayerPrefs.SetString("SelectedConnectome_" + i, selectedConnectome[i]);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene((int)_scenesList.Comparison);
        }


        GameObject connectomeContainer = GameObject.Find("ConnectomeContainer");
        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(KeyCode.RightArrow))
            connectomeContainer.transform.Rotate(0, connectomeRotateSpeed, 0, Space.Self);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(KeyCode.LeftArrow))
            connectomeContainer.transform.Rotate(0, -connectomeRotateSpeed, 0, Space.Self);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform.tag == "SingleConnectome")
                {
                    GameObject selectSphere = hit.transform.Find("SelectionSphere").gameObject;
                    if (!selectSphere.activeSelf)
                    {
                        if (selectedConnectome.Count < nConnectomeForDesktop)
                        {
                            selectSphere.SetActive(true);

                            string connectomeName = hit.transform.name;
                            if (!selectedConnectome.Contains(connectomeName))
                                selectedConnectome.Add(connectomeName);
                        }
                        else
                        {
                            GameObject limitWarning = Warnings.transform.Find("ConnectomeLimit").gameObject;
                            limitWarning.SetActive(true);
                            StartCoroutine(DisableWarnings(limitWarning));
                        }
                    }
                    else
                    {
                        selectSphere.SetActive(false);
                        string connectomeName = hit.transform.name;
                        if (selectedConnectome.Contains(connectomeName))
                            selectedConnectome.Remove(connectomeName);
                    }
                }
            }

            

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
                if (result.gameObject.name == "Inspect" && selectedConnectome.Count <= 2 && selectedConnectome.Count > 0)
                    isInspect = true;

                if (result.gameObject.name == "Reset")
                {
                    _cam.transform.position = new Vector3(0, 0, -27);
                    _cam.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

                    connectomeContainer.transform.rotation = Quaternion.Euler(0, 0, 0);

                    GameObject[] connectomeList = GameObject.FindGameObjectsWithTag("SingleConnectome");
                    foreach (GameObject connectome in connectomeList)
                    {
                        connectome.transform.parent.transform.localScale = new Vector3(1, 1, 1);
                        if (!connectome.GetComponent<Rigidbody>())
                        {
                            connectome.AddComponent<Rigidbody>();
                            connectome.GetComponent<Rigidbody>().useGravity = false;
                            connectome.GetComponent<Rigidbody>().isKinematic = true;
                        }
                        string connectomeName = connectome.transform.name;
                        if (selectedConnectome.Contains(connectomeName))
                        {
                            selectedConnectome.Remove(connectomeName);
                            GameObject selectSphere = connectome.transform.Find("SelectionSphere").gameObject;
                            selectSphere.SetActive(false);
                        }
                    }
                }
                if (result.gameObject.name == "Quit")
                    Application.Quit();
            }
        }
    }

    void ComparisonUI()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hit = Physics.RaycastAll(ray);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].transform.gameObject.tag == "Node" )
                hit[i].transform.parent.GetComponent<BoxCollider>().enabled = false;
            
            if(hit[i].transform.gameObject.tag == "NodeComparisonCollider")
                hit[i].transform.parent.parent.GetComponent<BoxCollider>().enabled = false;

            if (hit[i].transform.gameObject.tag == "SingleConnectome" || hit[i].transform.gameObject.tag == "SingleConnectomeDuplicate" || hit[i].transform.gameObject.tag == "OverlayComparison")
            {
                if (Input.GetMouseButton(0))
                {
                    Transform selectedSingleConnectome = hit[i].transform;
                    float initialYaw = 0;
                    float initialPitch = 0;
                    if (initialRotationClick)
                    {
                        initialRotationClick = false;
                        initialYaw = Input.GetAxis("Mouse X");
                        initialPitch = Input.GetAxis("Mouse Y");
                    }
                    float differentYaw = (Input.GetAxis("Mouse X") - initialYaw) * connectomeHorizontalSpeed;
                    float differentPitch = (Input.GetAxis("Mouse Y") - initialPitch) * connectomeVerticalSpeed;
                    Vector3 difference = new Vector3(differentPitch, differentYaw, 0);
                    selectedSingleConnectome.eulerAngles += difference;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    initialRotationClick = true;
                }
            }

            if (hit[i].transform.tag == "OverlayComparison")
            {
                GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
                GameObject Connectome1 = singleConnectomes[0];
                GameObject Connectome2;
                GameObject overlayComparison = hit[i].transform.gameObject;
                if (singleConnectomes.Length == 1)
                    Connectome2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
                else
                    Connectome2 = singleConnectomes[1];

                Connectome1.transform.rotation = overlayComparison.transform.rotation;
                Connectome2.transform.rotation = overlayComparison.transform.rotation;
            }
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            cameraScrollSpeed = 1.0f;

        }
        else
        {
            cameraScrollSpeed = 0.0f;
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
                if (result.gameObject.name == "Back")
                    UnityEngine.SceneManagement.SceneManager.LoadScene((int)_scenesList.Selection);

                if (result.gameObject.name == "Reset")
                    UnityEngine.SceneManagement.SceneManager.LoadScene((int)_scenesList.Comparison);

                if (result.gameObject.name == "Quit")
                    Application.Quit();

                if (result.gameObject.name == "Compare")
                {
                    GameObject[] allConnectomes = GameObject.FindGameObjectsWithTag("ConnectomeTopLevel");
                    
                    if (isCompare)
                    {
                        foreach (GameObject connectome in allConnectomes)
                            connectome.transform.position = new Vector3(0, 0, -20);
                        GameObject connectomeTopLevel = Resources.Load("Prefabs/ConnectomeTopLevel") as GameObject;
                        GameObject connectomeOverlay = Instantiate(connectomeTopLevel, new Vector3(0, 0, -20), Quaternion.identity);
                        connectomeOverlay.transform.name = "connectomeOverlay";
                        connectomeOverlay.transform.tag = "OverlayComparison";
                        connectomeOverlay.AddComponent<OverlayComparison>();
                        connectomeOverlay.AddComponent<BoxCollider>();
                        connectomeOverlay.GetComponent<BoxCollider>().enabled = true;
                        connectomeOverlay.GetComponent<BoxCollider>().size = new Vector3(2.5f, 2.5f, 2.5f);
                        GameObject[] nodes1 = Connectome1.GetComponent<SingleConnectome>().Nodes.ToArray();
                        GameObject[] nodes2 = Connectome2.GetComponent<SingleConnectome>().Nodes.ToArray();
                        GameObject[] edge1 = Connectome1.GetComponent<SingleConnectome>().AllEdges.ToArray();
                        GameObject[] edge2 = Connectome2.GetComponent<SingleConnectome>().AllEdges.ToArray();
                        bool isEdgeBundling = Connectome2.GetComponent<SingleConnectome>().isEdgeBundling;
                        Connectome1.transform.rotation = Quaternion.identity;
                        Connectome2.transform.rotation = Quaternion.identity;

                        float glyph1 = Connectome1.GetComponent<SingleConnectome>().Glyph;
                        float glyph2 = Connectome2.GetComponent<SingleConnectome>().Glyph;
                        float Glyph = Mathf.Max(glyph1, glyph2);
                        nodePrefab.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f) * Glyph;
                        if (nodes1.Length == nodes2.Length)
                        {
                            Connectome1.GetComponent<BoxCollider>().enabled = false;
                            Connectome2.GetComponent<BoxCollider>().enabled = false;
                            Material comparisonNodeMaterial = new Material(Shader.Find("Custom/NodeComparison"));
                            for (int i = 0; i < nodes1.Length; i++)
                            {
                                GameObject nodeComparison = Instantiate(nodePrefab, new Vector3(nodes1[i].transform.localPosition.x, nodes1[i].transform.localPosition.y, nodes1[i].transform.localPosition.z), Quaternion.identity);
                                nodeComparison.transform.name = (i + 1).ToString();
                                Vector3 tempPosition = nodeComparison.transform.position;
                                Vector3 tempScale = nodeComparison.transform.localScale;
                                Quaternion tempRotation = nodeComparison.transform.localRotation;
                                nodeComparison.transform.parent = connectomeOverlay.transform;
                                nodeComparison.transform.localScale = tempScale;
                                nodeComparison.transform.localRotation = tempRotation;
                                nodeComparison.transform.localPosition = tempPosition;
                                nodeComparison.transform.tag = "NodeComparison";
                                nodes1[i].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                Color leftColor = _propBlock.GetColor("_Color1");
                                float leftAlpha = _propBlock.GetFloat("_Alpha1");
                                nodes2[i].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                Color rightColor = _propBlock.GetColor("_Color2");
                                float rightAlpha = _propBlock.GetFloat("_Alpha2");
                                nodeComparison.GetComponent<Renderer>().material = comparisonNodeMaterial;
                                nodeComparison.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                                _propBlock.SetColor("_Color1", leftColor);
                                _propBlock.SetColor("_Color2", rightColor);
                                _propBlock.SetFloat("_Alpha1", leftAlpha);
                                _propBlock.SetFloat("_Alpha2", rightAlpha);

                                nodeComparison.GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                                nodes1[i].SetActive(false);
                                nodes2[i].SetActive(false);
                                nodeComparison.transform.Find("LeftCollider").gameObject.AddComponent<SingleNodeBehaviour>();
                                nodeComparison.transform.Find("RightCollider").gameObject.AddComponent<SingleNodeBehaviour>();

                            } 
                        }
                        result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Seperate";
                    }
                    else
                    {
                        GameObject[] nodes1 = Connectome1.GetComponent<SingleConnectome>().Nodes.ToArray();
                        GameObject[] nodes2 = Connectome2.GetComponent<SingleConnectome>().Nodes.ToArray();
                        GameObject[] edge1 = Connectome1.GetComponent<SingleConnectome>().AllEdges.ToArray();
                        GameObject[] edge2 = Connectome2.GetComponent<SingleConnectome>().AllEdges.ToArray();
                        GameObject ConnectomeOverlay = GameObject.Find("connectomeOverlay");
                        for (int i = 0; i < nodes1.Length; i++)
                        {
                            nodes1[i].SetActive(true);
                        }
                        for (int i = 0; i < nodes2.Length; i++)
                        {
                            nodes2[i].SetActive(true);
                        }
                        Destroy(GameObject.Find("connectomeOverlay"));
                        allConnectomes[0].transform.position = new Vector3(-2.0f, 0, -20);
                        allConnectomes[1].transform.position = new Vector3(1.5f, 0, -20);
                        transform.Find("ComparisionSettings").gameObject.SetActive(false);
                        result.gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Compare";
                    }
                    isCompare = !isCompare;
                }

            }

        }


        if(syncLeft.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn && isDynamic2)
        {
            syncRight.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn = false;
            syncRight.GetComponent<GraphicRaycaster>().enabled = false;

            foreach (GameObject rightDynamicController in RightDynamicControllers)
            {
                Color rightControllerColor = rightDynamicController.GetComponent<Image>().color;
                rightControllerColor.a = 0.2f;
                rightDynamicController.GetComponent<Image>().color = rightControllerColor;
                rightDynamicController.GetComponent<GraphicRaycaster>().enabled = false;
                rightDynamicController.GetComponent<DynamicDataUIControl>().enabled = false; 
            }
        }
        else if(!syncLeft.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn && isDynamic2)
        {
            syncRight.GetComponent<GraphicRaycaster>().enabled = true;
            foreach (GameObject rightDynamicController in RightDynamicControllers)
            {
            
                Color rightControllerColor = rightDynamicController.GetComponent<Image>().color;
                rightControllerColor.a = 1f;
                rightDynamicController.GetComponent<Image>().color = rightControllerColor;
                rightDynamicController.GetComponent<GraphicRaycaster>().enabled = true;
                rightDynamicController.GetComponent<DynamicDataUIControl>().enabled = true;
               
            }
        }
            

        if (syncRight.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn && isDynamic1)
        {
            syncLeft.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn = false ;
            syncLeft.GetComponent<GraphicRaycaster>().enabled = false;
            foreach (GameObject leftDynamicController in LeftDynamicControllers)
            {
                Color leftControllerColor = leftDynamicController.GetComponent<Image>().color;
                leftControllerColor.a = 0.2f;
                leftDynamicController.GetComponent<Image>().color = leftControllerColor;
                leftDynamicController.GetComponent<GraphicRaycaster>().enabled = false;
                leftDynamicController.GetComponent<DynamicDataUIControl>().enabled = false;
            }
        }
        else if(!syncRight.GetComponent<Michsky.UI.ModernUIPack.SwitchAnim>().isOn && isDynamic1)
        {
            syncLeft.GetComponent<GraphicRaycaster>().enabled = true;
            foreach (GameObject leftDynamicController in LeftDynamicControllers)
            {
                Color leftControllerColor = leftDynamicController.GetComponent<Image>().color;
                leftControllerColor.a = 1f;
                leftDynamicController.GetComponent<Image>().color = leftControllerColor;
                leftDynamicController.GetComponent<GraphicRaycaster>().enabled = true;
                leftDynamicController.GetComponent<DynamicDataUIControl>().enabled = true;
            }
        }
        
        leftTimeStep.GetComponent<Text>().text = Connectome1.GetComponent<SingleConnectome>().DynamicCurrentTime.ToString() + '/' + timeStep1;
        rightTimeStep.GetComponent<Text>().text = Connectome2.GetComponent<SingleConnectome>().DynamicCurrentTime.ToString() + '/' + timeStep2;


        if (!isDynamic1|| !isDynamic2)
        {
            syncLeft.transform.parent.parent.Find("Title").GetComponent<Text>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            syncRight.transform.parent.parent.Find("Title").GetComponent<Text>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        }
        
    }

    public void ChangeRepresentation(GameObject currentConnectome, string option)
    {
        currentConnectome.transform.GetComponent<SingleConnectome>()._representationType = option;
    }

    public void ChangeClassification(GameObject currentConnectome, string option)
    {
        currentConnectome.transform.GetComponent<SingleConnectome>()._classificationType = option;
    }

    public void ChangeSyncOption(GameObject connectome1, GameObject connectome2, bool isSync)
    {
        if (isSync)
        {
            int connectome2CurrentTime = connectome2.GetComponent<SingleConnectome>().DynamicCurrentTime;
            int connectome1CurrentTime = connectome1.GetComponent<SingleConnectome>().DynamicCurrentTime;
            if(connectome1CurrentTime != connectome2CurrentTime)
            {
                connectome2.GetComponent<SingleConnectome>().DynamicCurrentTime = connectome1CurrentTime;
                connectome2.GetComponent<SingleConnectome>().DynamicDataFunction(connectome1CurrentTime, connectome1CurrentTime);
            }

        }
    }


    // Update is called once per frame
    void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
            Connectome1 = singleConnectomes[0];
            
            if (singleConnectomes.Length == 1)
                Connectome2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
            else
                Connectome2 = singleConnectomes[1];
            dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
            if (sceneName == "Comparison")
            {
                isDynamic1 = dataLoader.isDynamicDictionaryGlobal[Connectome1.name];
                isDynamic2 = dataLoader.isDynamicDictionaryGlobal[Connectome2.name];
                timeStep1 = dataLoader.connectomeTimeStep[Connectome1.name];
                timeStep2 = dataLoader.connectomeTimeStep[Connectome2.name];
            }
            
           
        }
        if (_currentScene == (int)_scenesList.Selection)
            SelectionUI();

        if (_currentScene == (int)_scenesList.Comparison)
        {
            CameraAction();
            ComparisonUI();
        }
        
        if(sceneName == "Comparison")
        {
            bool isSync1 = Connectome1.GetComponent<SingleConnectome>().isSync;
            bool isSync2 = Connectome2.GetComponent<SingleConnectome>().isSync;
            int currentTimeStep = 0;
            if (isSync1)
            {
                currentTimeStep = Connectome1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                Connectome2.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep;
            }
            if (isSync2)
            {
                currentTimeStep = Connectome2.GetComponent<SingleConnectome>().DynamicCurrentTime;
                Connectome1.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep;
            }
        }
    }

    IEnumerator DisableWarnings(GameObject warningToBeDisabled)
    {
        yield return new WaitForSeconds(3);
        warningToBeDisabled.gameObject.SetActive(false);
    }

}

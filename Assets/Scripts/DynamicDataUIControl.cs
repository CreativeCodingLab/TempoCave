using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DynamicDataUIControl : MonoBehaviour
{

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public string connectomeNumber;
    public GameObject classificationGameObject;
    private string connectomeName;
    private GameObject connectomeParent1;
    //private GameObject connectomeParent;
    private GameObject connectomeParent2;
    private bool isFirstUpdate = true;
    private GameObject timeStepText;
    private int timeStep = 16;
    private DataLoader dataLoader;
    private bool isDynamic;
    private int isDuplicate;
    //private DesktopController DC;
    // Start is called before the first frame update
    void Start()
    {
        connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
        isDuplicate = PlayerPrefs.GetInt("isDuplicate");
        timeStepText = this.transform.parent.parent.Find("timestep").gameObject;
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
       // DC = GameObject.Find("Canvas").GetComponent<DesktopController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFirstUpdate)
        {
            dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
            isDynamic = dataLoader.isDynamicDictionaryGlobal[connectomeName];
            timeStep = dataLoader.connectomeTimeStep[connectomeName];
            isFirstUpdate = false;
            GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");

            if (isDuplicate == 0 && int.Parse(connectomeNumber) == 0)
            {
                connectomeParent1 = singleConnectomes[0];
                connectomeParent2 = singleConnectomes[1];
            }
            else if (isDuplicate == 0 && int.Parse(connectomeNumber) == 1)
            {
                connectomeParent1 = singleConnectomes[1];
                connectomeParent2 = singleConnectomes[0];
            }

            else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
            {
                connectomeParent1 = GameObject.FindGameObjectWithTag("SingleConnectome");
                connectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
            }

            else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
            {
                connectomeParent1 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
                connectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectome");
            }
        }
        
        if (isDynamic)
        {
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
                    //Debug.Log("Hit " + result.gameObject.name);
                    if (result.gameObject.name == "Play")
                    {
                        result.gameObject.GetComponent<Image>().color = new Color(1,0,0,1);
                        result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                        int currentTimeStep = connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                        if (currentTimeStep == 0 || currentTimeStep == 1)
                            currentTimeStep = 1;
                        connectomeParent1.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep, timeStep);
                        connectomeParent1.transform.GetComponent<SingleConnectome>().isPause = false;

                        ///// the classification should be modularity
                        Transform[] classificationsInGamObject = classificationGameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform classification in classificationsInGamObject)
                        {
                            if (classification.parent == classificationGameObject.transform)
                            {
                                if (classification.name == "Dynamic Modularity")
                                    classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                                else classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
                            }
                        }
                        connectomeParent1.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                        connectomeParent2.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                    }
                    else if (result.gameObject.name == "Pause")
                    {
                        result.gameObject.GetComponent<Image>().color = Color.red;
                        result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                        connectomeParent1.transform.GetComponent<SingleConnectome>().isPause = true;
                    }
                    else if (result.gameObject.name == "Left_Arrow")
                    {
                        result.gameObject.GetComponent<Image>().color = Color.red;
                        result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                        int currentTimeStep = connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;

                        if (currentTimeStep > 0)
                        {
                            int currentTime = connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                            connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTime - 1;
                        }

                        Transform[] classificationsInGamObject = classificationGameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform classification in classificationsInGamObject)
                        {
                            if (classification.parent == classificationGameObject.transform)
                            {
                                if (classification.name == "Dynamic Modularity")
                                    classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                                else classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
                            }
                        }
                        connectomeParent1.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                        connectomeParent2.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                    }
                    else if (result.gameObject.name == "Right_Arrow")
                    {
                        result.gameObject.GetComponent<Image>().color = Color.red;
                        result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                        result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                        int currentTimeStep = connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                        
                        if (currentTimeStep < timeStep)
                        {
                            int currentTime = connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                            connectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTime + 1;
                        }

                        Transform[] classificationsInGamObject = classificationGameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform classification in classificationsInGamObject)
                        {
                            if (classification.parent == classificationGameObject.transform)
                            {
                                if (classification.name == "Dynamic Modularity")
                                    classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.98f, 0.57f, 0.44f);
                                else classification.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
                            }
                        }
                        connectomeParent1.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                        connectomeParent2.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                    }
                }
            }
        }
        else
        {
            GetComponent<Image>().color = new Color(.2f, .2f, .2f, 1f);
            timeStepText.GetComponent<Text>().color = new Color(.2f, .2f, .2f, 1f);
        }
    }
}

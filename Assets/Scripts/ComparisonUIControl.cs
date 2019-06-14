using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComparisonUIControl : MonoBehaviour
{

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    //public string connectomeNumber;

    private GameObject ConnectomeParent1;
    private GameObject ConnectomeParent2;
    private GameObject timeStepText;
    private bool isFirstUpdate = true;
    private int timeStep = 0;
    private DataLoader dataLoader;

    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();

        timeStepText = this.transform.parent.parent.Find("timestep").gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        if (isFirstUpdate)
        {
            dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();

            GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
            ConnectomeParent1 = singleConnectomes[0];
            if (singleConnectomes.Length == 1)
                ConnectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
            else if(singleConnectomes.Length>1)
                ConnectomeParent2 = singleConnectomes[1];
            
            timeStep = Mathf.Min(dataLoader.connectomeTimeStep[ConnectomeParent1.name], dataLoader.connectomeTimeStep[ConnectomeParent2.name]);
            isFirstUpdate = false;
        }
        int connectome1CurrentTime = ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
        int connectome2CurrentTime = ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime;
        int currentTimeStep = Mathf.Min(connectome1CurrentTime, connectome2CurrentTime);
        timeStepText.GetComponent<Text>().text = currentTimeStep.ToString() + '/'+ timeStep;

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
                if (result.gameObject.name == "Play")
                {
                    result.gameObject.GetComponent<Image>().color = Color.red;
                    result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                    connectome1CurrentTime = ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    connectome2CurrentTime = ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    currentTimeStep = Mathf.Min(connectome1CurrentTime, connectome2CurrentTime);
                    if (currentTimeStep == 0 || currentTimeStep ==1)
                        currentTimeStep = 1;

                    
                    ConnectomeParent1.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep, timeStep);
                    ConnectomeParent1.transform.GetComponent<SingleConnectome>().isPause = false;
                    
                    ConnectomeParent2.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep, timeStep);
                    ConnectomeParent2.transform.GetComponent<SingleConnectome>().isPause = false;
                }
                else if (result.gameObject.name == "Pause")
                {
                    result.gameObject.GetComponent<Image>().color = Color.red;
                    result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                    ConnectomeParent1.transform.GetComponent<SingleConnectome>().isPause = true;
                    ConnectomeParent2.transform.GetComponent<SingleConnectome>().isPause = true;
                }
                else if (result.gameObject.name == "Left_Arrow")
                {
                    result.gameObject.GetComponent<Image>().color = Color.red;
                    result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Right_Arrow").GetComponent<Image>().color = Color.white;
                    connectome1CurrentTime = ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    connectome2CurrentTime = ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    currentTimeStep = Mathf.Min(connectome1CurrentTime, connectome2CurrentTime);
                    if (currentTimeStep == 1)
                    {
                        currentTimeStep = 0;
                        ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime = 0;
                        ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime = 0;
                        ConnectomeParent1.GetComponent<SingleConnectome>().backToStaticState();
                        ConnectomeParent2.GetComponent<SingleConnectome>().backToStaticState();
                    }
                    if (currentTimeStep > 1)
                    {

                        //int currentTime = ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                        ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep - 1;
                        ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep - 1;

                        //ConnectomeParent1.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep - 1, currentTimeStep - 1);
                        //ConnectomeParent2.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep - 1, currentTimeStep - 1);
                    } 
                }
                else if (result.gameObject.name == "Right_Arrow")
                {
                    result.gameObject.GetComponent<Image>().color = Color.red;
                    result.gameObject.transform.parent.Find("Pause").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Left_Arrow").GetComponent<Image>().color = Color.white;
                    result.gameObject.transform.parent.Find("Play").GetComponent<Image>().color = Color.white;
                    connectome1CurrentTime = ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    connectome2CurrentTime = ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime;
                    currentTimeStep = Mathf.Min(connectome1CurrentTime, connectome2CurrentTime);

                    if (currentTimeStep < timeStep)
                    {
                        ConnectomeParent1.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep + 1;
                        ConnectomeParent2.GetComponent<SingleConnectome>().DynamicCurrentTime = currentTimeStep + 1;
                        //ConnectomeParent1.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep + 1, currentTimeStep + 1);
                        //ConnectomeParent2.GetComponent<SingleConnectome>().DynamicDataFunction(currentTimeStep + 1, currentTimeStep + 1);
                    }   
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ClearEdges : MonoBehaviour
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    private int isDuplicate;
    private GameObject connectomeParent;
    private string connectomeName;
    public string connectomeNumber;
    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        isDuplicate = PlayerPrefs.GetInt("isDuplicate");
        connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDuplicate == 0)
            connectomeParent = GameObject.Find(connectomeName);
        else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
            connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectome");
        else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
            connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");

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
                if (result.gameObject.name == "ClearEdges")
                {
                    connectomeParent.GetComponent<SingleConnectome>().ClearAllEdges();
                }

            }
        }
    }
}


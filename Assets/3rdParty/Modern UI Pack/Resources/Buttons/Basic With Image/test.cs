using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class test1 : MonoBehaviour
{
    // Normal raycasts do not work on UI elements, they require a special kind
    GraphicRaycaster raycaster;

    void Awake()
    {
        // Get both of the components we need to do this
        this.raycaster = GetComponent<GraphicRaycaster>();
    }

    void Update()
    {
        //Check if the left Mouse button is clicked
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Set up the new Pointer Event
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            pointerData.position = Input.mousePosition;
            this.raycaster.Raycast(pointerData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
               if(result.gameObject.name == "Play")
               {

               }
               else if (result.gameObject.name == "Pause")
               {

               }
               else if (result.gameObject.name == "Left_Arrow")
               {

               }
               else if (result.gameObject.name == "Right_Arrow")
               {

               }
            }
        }
    }
}


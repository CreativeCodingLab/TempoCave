using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleNodeBehaviour : MonoBehaviour
{

    public bool isEnlarged = false;
    private float EdgeThresholdMin = 0.5f;
    private float EdgeThresholdMax = 1;
    private Transform currentTransform;
    private bool isComparison = false;
    private GPUEdgeBundling gpuEdgeBundling;
    private Vector3 NodeLocalScale;
    private float Glyph;
    // Start is called before the first frame update
    void Start()
    {
        //gpuEdgeBundling = GetComponentInParent<GPUEdgeBundling>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnMouseOver()
    {
        GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
        GameObject Connectome1 = singleConnectomes[0];
        GameObject Connectome2;
        if (singleConnectomes.Length == 1)
            Connectome2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
        else
            Connectome2 = singleConnectomes[1];

        if (transform.tag == "NodeComparisonCollider")
        {
            currentTransform = transform.parent;
            isComparison = true;
            float glyph1 = Connectome1.GetComponent<SingleConnectome>().Glyph;
            float glyph2 = Connectome2.GetComponent<SingleConnectome>().Glyph;
            Glyph = Mathf.Max(glyph1, glyph2);
            NodeLocalScale = new Vector3(0.02f, 0.02f, 0.02f) * Glyph;
        }
        else
        {
            currentTransform = transform;
            isComparison = false;
            Glyph = currentTransform.parent.GetComponent<SingleConnectome>().Glyph;
            NodeLocalScale = currentTransform.parent.GetComponent<SingleConnectome>().NodePrefabScale * Glyph;
        }
        int nodeName = int.Parse(currentTransform.name);

       
        if (isEnlarged == false)
            currentTransform.localScale = 1.5f * NodeLocalScale;

        if (Input.GetMouseButtonDown(0))
        {
            currentTransform.localScale = 2 * NodeLocalScale;
            bool isSelectingNode1 = Connectome1.GetComponent<SingleConnectome>().isSelectingNode;
            bool isSelectingNode2 = Connectome2.GetComponent<SingleConnectome>().isSelectingNode;

            if (isComparison)
            {
                if (isSelectingNode1 || isSelectingNode2)
                {
                    Connectome1.GetComponent<SingleConnectome>().AddSelectedNodetoList(currentTransform);
                    Connectome2.GetComponent<SingleConnectome>().AddSelectedNodetoList(currentTransform);
                }
                else
                {
                    if (transform.name == "LeftCollider")
                        DrawEdge(Connectome1, currentTransform);
                    if (transform.name == "RightCollider")
                        DrawEdge(Connectome2, currentTransform);
                }
            }
            else
            {
                if(isSelectingNode1||isSelectingNode2)
                {
                    currentTransform.parent.GetComponent<SingleConnectome>().AddSelectedNodetoList(currentTransform);
                }
                else
                    DrawEdge(currentTransform.parent.gameObject, currentTransform);
            }

            bool isEdgeExisted1 = Connectome1.GetComponent<SingleConnectome>().isEdgeExisted(nodeName);
            bool isEdgeExisted2 = Connectome2.GetComponent<SingleConnectome>().isEdgeExisted(nodeName);
            bool isNodeSelected1 = Connectome1.GetComponent<SingleConnectome>().isNodeSelected(nodeName.ToString());
            bool isNodeSelected2 = Connectome2.GetComponent<SingleConnectome>().isNodeSelected(nodeName.ToString());
            if (isEdgeExisted1 || isEdgeExisted2)
                isEnlarged = true;
            else
                isEnlarged = false;
        }
    }

    private void DrawEdge(GameObject ConnectomeParent, Transform currentTransform)
    {
        int edgeName = int.Parse(currentTransform.name);
        bool isEdgeBundling = ConnectomeParent.GetComponent<SingleConnectome>().isEdgeBundling;
        EdgeThresholdMax = ConnectomeParent.GetComponent<SingleConnectome>().thresholdMax;
        EdgeThresholdMin = ConnectomeParent.GetComponent<SingleConnectome>().thresholdMin;
        gpuEdgeBundling = ConnectomeParent.GetComponent<GPUEdgeBundling>();
        bool isEdgeExisted = ConnectomeParent.GetComponent<SingleConnectome>().isEdgeExisted(edgeName);
        if (isEdgeExisted)
        {
            ConnectomeParent.GetComponent<SingleConnectome>().StartNodeList.Remove(edgeName);
            //print(ConnectomeParent.GetComponent<SingleConnectome>().StartNodeList.Count);
            if (isEdgeBundling)
            {
                ConnectomeParent.GetComponent<SingleConnectome>().ClearAllEdges();
                if (ConnectomeParent.GetComponent<SingleConnectome>().StartNodeList.Count > 0)
                {
                    gpuEdgeBundling.EdgeBundling();
                    ConnectomeParent.GetComponent<SingleConnectome>().DrawEdgeBundling(EdgeThresholdMin, EdgeThresholdMax);
                }
            }
            else
                ConnectomeParent.GetComponent<SingleConnectome>().DestroyStraightEdge(edgeName);
        }
        else
        {
            ConnectomeParent.GetComponent<SingleConnectome>().StartNodeList.Add(edgeName);

            if (isEdgeBundling)
            {
                ConnectomeParent.GetComponent<SingleConnectome>().ClearAllEdges();
                gpuEdgeBundling.EdgeBundling();
                ConnectomeParent.GetComponent<SingleConnectome>().DrawEdgeBundling(EdgeThresholdMin, EdgeThresholdMax);
            }
            else
                ConnectomeParent.GetComponent<SingleConnectome>().DrawSingleEdge(edgeName, EdgeThresholdMin, EdgeThresholdMax);
        }
    }
    public void OnMouseExit()
    {
        if (isEnlarged == false)
            currentTransform.localScale = NodeLocalScale;
        currentTransform.parent.GetComponent<BoxCollider>().enabled = true;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayComparison : MonoBehaviour
{
    private bool isFirstUpdate = true;
    private DataLoader dataLoader;
    
    private GameObject ConnectomeParent1;
    private GameObject ConnectomeParent2;
    private int timeStep = 0;
    private GameObject[] edge1Initial;
    private GameObject[] edge2Initial;
    private GameObject[] node1Initial;
    private GameObject[] node2Initial;
    private MaterialPropertyBlock _propBlock;
    void Start()
    {
        _propBlock = new MaterialPropertyBlock();
    }
    void Update()
    {
        if (isFirstUpdate)
        {
            dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
            GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
            ConnectomeParent1 = singleConnectomes[0];
            if (singleConnectomes.Length == 1)
                ConnectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
            else
                ConnectomeParent2 = singleConnectomes[1];
            timeStep = Mathf.Min(dataLoader.connectomeTimeStep[ConnectomeParent1.name], dataLoader.connectomeTimeStep[ConnectomeParent2.name]);
            edge1Initial = ConnectomeParent1.GetComponent<SingleConnectome>().AllEdges.ToArray();
            edge2Initial = ConnectomeParent2.GetComponent<SingleConnectome>().AllEdges.ToArray();
            node1Initial = ConnectomeParent1.GetComponent<SingleConnectome>().Nodes.ToArray();
            node2Initial = ConnectomeParent2.GetComponent<SingleConnectome>().Nodes.ToArray();
            isFirstUpdate = false;
        }
        ConnectomeParent1.transform.rotation = this.transform.rotation;
        ConnectomeParent2.transform.rotation = this.transform.rotation;
        GameObject[] nodes1 = ConnectomeParent1.GetComponent<SingleConnectome>().Nodes.ToArray();
        GameObject[] nodes2 = ConnectomeParent2.GetComponent<SingleConnectome>().Nodes.ToArray();
        GameObject[] OverlayNodes = GameObject.FindGameObjectsWithTag("NodeComparison");
        if (!node1Initial.Equals(nodes1) || !node2Initial.Equals(nodes2))
        {
            node1Initial = nodes1;
            node2Initial = nodes2;
            
            for (int i = 0; i < OverlayNodes.Length; i++)
            {
                nodes1[i].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                Color leftColor = _propBlock.GetColor("_Color1");
                float leftAlpha = _propBlock.GetFloat("_Alpha1");
                nodes2[i].transform.GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                Color rightColor = _propBlock.GetColor("_Color2");
                float rightAlpha = _propBlock.GetFloat("_Alpha2");

                OverlayNodes[i].GetComponent<Renderer>().GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color1", leftColor);
                _propBlock.SetColor("_Color2", rightColor);
                _propBlock.SetFloat("_Alpha1", leftAlpha);
                _propBlock.SetFloat("_Alpha2", rightAlpha);
                OverlayNodes[i].GetComponent<Renderer>().SetPropertyBlock(_propBlock);
                nodes1[i].SetActive(false);
                nodes2[i].SetActive(false);
            }
        }
        GameObject[] edge1 = ConnectomeParent1.GetComponent<SingleConnectome>().AllEdges.ToArray();
        GameObject[] edge2 = ConnectomeParent2.GetComponent<SingleConnectome>().AllEdges.ToArray();
        //GameObject[] OverlayNodes = GameObject.FindGameObjectsWithTag("NodeComparison");
        Vector3 Node1Size = nodes1[0].transform.localScale;
        Vector3 Node2Size = nodes2[0].transform.localScale;
        Vector3 overlayNodeSize = OverlayNodes[0].transform.localScale;
        Vector3 localSize;
        if (Node1Size != overlayNodeSize || Node2Size != overlayNodeSize)
        {
            if (Node1Size.magnitude > Node2Size.magnitude)
               localSize  = Node1Size;
            else
               localSize = Node2Size;

            for (int i = 0; i < OverlayNodes.Length; i++)
            {
                OverlayNodes[i].transform.localScale = localSize;
                //    _preGlyph = Glyph;
                //    _nodePrefab.transform.localScale = Glyph * NodePrefabScale;
                //    foreach (GameObject node in Nodes)
                //    {
                //        if (node)
                //            node.transform.localScale = Glyph * NodePrefabScale;
                //    }
            }
        }
    }
}

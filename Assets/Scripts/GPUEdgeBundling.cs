using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUEdgeBundling : MonoBehaviour
{
    public float compatibilityThreshold = 0.8f;
    public float Kattract = 0.08f;
   
    public float Kspring = 0.05f;
    public float KdiminishingFactor = 0.75f;

    public float distanceScalingFactor = 10.0f;
    public float maxAttractionForce = 0.6f;
    public float maxSpringForce = 1.5f;
    public float minControlPointDistance = 0.1f; ///can modify to make it smaller to make edge closer
    public int I0 = 50;

    // Initial number of iterations and diminishing rate between cycles
    private float _Kattract = 0.8f;
    private int _I0 = 50;
    private const float Ifactor = 0.66f;

    public int edgeBundlingIterations = 5;
    public int maxSegmentNumber;

    public ComputeShader shader;
    public Texture2D outputPositionTexture;
    public edgeData[] edgeDataMatrix;

    private const int groupSizeHeight = 1023;
    private const int groupSizeWidth = 8;
    private int groupCountHeight;
    private int groupCountWidth;
    private int globalStepKernel;
    private int localStepKernel;
    private int smoothingKernel;
   
    private SingleConnectome singleConnectome;

    private Texture2D edgeSegments;
    public struct edgeData
    {
        public float connectivity;
        public int startNodeID;
        public int endNodeID;
    };

    private int edgeStructSize = 12;
    private ComputeBuffer controlPointsIndexBuffer;
    private ComputeBuffer edgeDataBuffer;

    void RunComputeShader()
    {
        _I0 = I0;
        _Kattract = Kattract;
        List<int> startNodeList = singleConnectome.StartNodeList;
        if (startNodeList.Count > 0)
        {
            List<int> controlPointsIndex = new List<int>();

            groupCountHeight = Mathf.CeilToInt((float)edgeSegments.height / groupSizeHeight);
            RenderTexture outputPositionRenderTexture = new RenderTexture(edgeSegments.width, edgeSegments.height, 24, RenderTextureFormat.ARGBFloat);
            outputPositionRenderTexture.enableRandomWrite = true;
            outputPositionRenderTexture.Create();

            globalStepKernel = shader.FindKernel("GlobalStep");
            localStepKernel = shader.FindKernel("LocalStep");
            smoothingKernel = shader.FindKernel("SmoothingStep");
            shader.SetInt("maxSegmentNumber", maxSegmentNumber);
            shader.SetInt("nEdges", edgeSegments.height);
            shader.SetFloat("compatibilityThreshold", compatibilityThreshold);
            shader.SetFloat("Kspring", Kspring);
            shader.SetFloat("KdiminishingFactor", KdiminishingFactor);
            shader.SetFloat("distanceScalingFactor", distanceScalingFactor);
            shader.SetFloat("maxAttractionForce", maxAttractionForce);
            shader.SetFloat("maxSpringForce", maxSpringForce);
            shader.SetFloat("minControlPointDistance", minControlPointDistance);

            edgeDataBuffer = new ComputeBuffer(edgeDataMatrix.Length, edgeStructSize);
            edgeDataBuffer.SetData(edgeDataMatrix);
            shader.SetBuffer(globalStepKernel, "edgeDataBuffer", edgeDataBuffer);
            shader.SetBuffer(localStepKernel, "edgeDataBuffer", edgeDataBuffer);
            shader.SetBuffer(smoothingKernel, "edgeDataBuffer", edgeDataBuffer);

            for (int iteration = 0; iteration < edgeBundlingIterations; iteration++)
            {
                shader.SetFloat("Kattract", _Kattract);
                controlPointsIndex.Clear();
                int startPointIndex = 0;
                controlPointsIndex.Add(startPointIndex);
                int interval = (int)Mathf.Pow(2, (edgeBundlingIterations - iteration - 1));
                for (int segmentIndex = 0; segmentIndex < (int)Mathf.Pow(2, (iteration + 1)); segmentIndex++)
                {
                    startPointIndex = startPointIndex + interval;
                    controlPointsIndex.Add(startPointIndex);
                }

                controlPointsIndexBuffer = new ComputeBuffer(controlPointsIndex.Count, sizeof(int));
                controlPointsIndexBuffer.SetData(controlPointsIndex.ToArray());
                shader.SetBuffer(globalStepKernel, "controlPointsIndex", controlPointsIndexBuffer);
                shader.SetBuffer(localStepKernel, "controlPointsIndex", controlPointsIndexBuffer);
                shader.SetBuffer(smoothingKernel, "controlPointsIndex", controlPointsIndexBuffer);
                shader.SetInt("nControlPoints", controlPointsIndex.Count);

                for (int iterationSteps = 0; iterationSteps < _I0; iterationSteps++)
                {
                    shader.SetTexture(globalStepKernel, "edgePositions", edgeSegments);
                    shader.SetTexture(globalStepKernel, "outputTexture", outputPositionRenderTexture);
                    shader.Dispatch(globalStepKernel, 1, groupCountHeight, 1);
                    Graphics.CopyTexture(outputPositionRenderTexture, edgeSegments);

                    shader.SetTexture(localStepKernel, "edgePositions", edgeSegments);
                    shader.SetTexture(localStepKernel, "outputTexture", outputPositionRenderTexture);
                    shader.Dispatch(localStepKernel, 1, groupCountHeight, 1);
                    Graphics.CopyTexture(outputPositionRenderTexture, edgeSegments);

                    shader.SetTexture(smoothingKernel, "edgePositions", edgeSegments);
                    shader.SetTexture(smoothingKernel, "outputTexture", outputPositionRenderTexture);
                    shader.Dispatch(smoothingKernel, 1, groupCountHeight, 1);
                    Graphics.CopyTexture(outputPositionRenderTexture, edgeSegments);
                }

                _I0 = (int)((float)_I0 * Ifactor);
                _Kattract = _Kattract * KdiminishingFactor;
                edgeDataBuffer.Release();
                controlPointsIndexBuffer.Release();
            }

            
            outputPositionTexture = new Texture2D(outputPositionRenderTexture.width, outputPositionRenderTexture.height, TextureFormat.RGBAFloat, false);
            RenderTexture.active = outputPositionRenderTexture;
            outputPositionTexture.ReadPixels(new Rect(0, 0, outputPositionRenderTexture.width, outputPositionRenderTexture.height), 0, 0);
            outputPositionTexture.Apply();

            outputPositionRenderTexture.Release();

        }


        //for (int i = 0; i < outputPositionTexture.height; i++)
        //    for (int j = 0; j < outputPositionTexture.width; j++)
        //        Debug.Log(outputPositionTexture.GetPixel(j, i).a);
    }

    public void EdgeBundling()
    {
        maxSegmentNumber = (int)Mathf.Pow(2, edgeBundlingIterations);
        Dictionary<string, string[][]> connectomeData = singleConnectome._connectomeData;
        string[][] edgesMatrix = singleConnectome._edgesMatrix;
        List<int> startNodeList = singleConnectome.StartNodeList;
        if (connectomeData["NW"] != null)
            edgesMatrix = connectomeData["NW"];
        else if (connectomeData[singleConnectome.TimeStage] != null)
            edgesMatrix = connectomeData[singleConnectome.TimeStage];
        int totalNodeNumber = edgesMatrix.Length - 1;
        List<GameObject> Nodes = singleConnectome.Nodes;
        edgeDataMatrix = new edgeData[startNodeList.Count * totalNodeNumber];
        edgeSegments = new Texture2D(maxSegmentNumber + 1, startNodeList.Count * totalNodeNumber, TextureFormat.RGBAFloat, false);
        for (int i = 0; i < startNodeList.Count; i++)
        {
            int startNodeNumber = startNodeList[i];
            for (int j = 0; j< edgesMatrix[startNodeNumber].Length; j++)
            {
                int endNodeNumber = j+1;
                edgeDataMatrix[i * totalNodeNumber + j].startNodeID = startNodeNumber;
                edgeDataMatrix[i * totalNodeNumber + j].endNodeID = endNodeNumber;
                edgeDataMatrix[i * totalNodeNumber + j].connectivity = float.Parse(edgesMatrix[startNodeNumber][endNodeNumber-1]);
                Vector3 startPosition = Nodes[startNodeNumber-1].transform.localPosition;
                Vector3 endPosition = Nodes[endNodeNumber-1].transform.localPosition;
                edgeSegments.SetPixel(0, i * totalNodeNumber + j, new Color(startPosition.x, startPosition.y, startPosition.z, 0));
                edgeSegments.SetPixel(maxSegmentNumber, i * totalNodeNumber + j, new Color(endPosition.x, endPosition.y, endPosition.z, 0));
                Vector3 segmentPosition = new Vector3(0, 0, 0);
                for (int k = 1; k < edgeSegments.width - 1; k++)
                {
                    segmentPosition = ((float)k / (float)maxSegmentNumber) * (endPosition - startPosition) + startPosition;
                    edgeSegments.SetPixel(k, i * totalNodeNumber + j, new Color(segmentPosition.x, segmentPosition.y, segmentPosition.z, 0));
                }   
            }
        }
        edgeSegments.Apply();
        RunComputeShader();
    }

    void Start()
    {
        shader = (ComputeShader)Resources.Load("Shaders/EdgeBundling");
        singleConnectome = GetComponent<SingleConnectome>();
    }
}

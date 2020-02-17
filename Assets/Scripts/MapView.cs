﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapView : MonoBehaviour
{
    public enum MapOrientation
    {
        BottomToTop,
        TopToBottom,
        RightToLeft,
        LeftToRight
    }
    
    public MapOrientation orientation;
    public List<NodeBlueprint> blueprints;
    public GameObject nodePrefab;
    [Header("Line settings")]
    public GameObject linePrefab;
    public int linePointsCount = 10;
    public float offsetFromNodes = 0.5f;
    
    private List<float> layerDistances;
    private GameObject mapParent;
    private List<List<Point>> paths;
    // ALL nodes by layer:
    private readonly List<MapNode> mapNodes = new List<MapNode>();

    public static MapView Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void ClearMap()
    {
        if (mapParent != null)
            Destroy(mapParent);
        
        mapNodes.Clear();
    }

    public void ShowMap(Map m)
    {
        if (m == null)
        {
            Debug.LogWarning("Map was null in MapView.ShowMap()");
            return;
        }

        ClearMap();
        
        mapParent = new GameObject("MapParent");

        CreateNodes(m.nodes);

        SetOrientation();
        
        DrawLines();

        SetFirstLayerAttainable();
    }

    private void CreateNodes(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            var mapNode = CreateMapNode(node);
            mapNodes.Add(mapNode);
        }
    }

    private MapNode CreateMapNode(Node node)
    {
        return null;
    }

    private void SetFirstLayerAttainable()
    {
        foreach (var node in mapNodes.Where(n => n.Node.point.x == 0))
            node.SetState(NodeStates.Attainable);
    }

    private void SetOrientation()
    {
        switch (orientation)
        {
            case MapOrientation.BottomToTop:
                // do nothing
                break;
            case MapOrientation.TopToBottom:
                mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                break;
            case MapOrientation.RightToLeft:
                mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                break;
            case MapOrientation.LeftToRight:
                mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawLines()
    {
        foreach (var node in mapNodes)
        {
            // reset rotation to fix after orientation changes:
            node.transform.rotation = Quaternion.identity;
            foreach (var connection in node.Node.outgoing)
                AddLineConnection(node, GetNode(connection));
        }
    }

    private float GetDistanceToLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex > layerDistances.Count) return 0f;
        
        return layerDistances.Take(layerIndex + 1).Sum();
    }

    /*
    private void PlaceLayer(int layerIndex)
    {
        var layer = config.layers[layerIndex];
        var layerParentObject = new GameObject("Layer " + layerIndex + " Parent");
        layerParentObject.transform.SetParent(mapParent.transform);
        var nodesOnThisLayer = new List<MapNode>();
        for (var i = 0; i < config.GridWidth; i++)
        {
            var nodeObject = Instantiate(nodePrefab, layerParentObject.transform);
            nodeObject.transform.localPosition = new Vector3(i * layer.nodesApartDistance, 0f, 0f);
            var node = nodeObject.GetComponent<MapNode>();
            nodesOnThisLayer.Add(node);
            var blueprint = UnityEngine.Random.Range(0f, 1f) < layer.randomizeNodes ? GetRandomNode() : layer.node;
            node.SetUp(blueprint, layerIndex);
        }

        nodes.Add(nodesOnThisLayer);
        // offset of this layer to make all the nodes centered:
        var offset = (nodesOnThisLayer[nodesOnThisLayer.Count - 1].transform.localPosition.x -
                      nodesOnThisLayer[0].transform.localPosition.x) / 2f;
        layerParentObject.transform.localPosition = new Vector3(- offset, GetDistanceToLayer(layerIndex), 0f);
    }*/

    public void AddLineConnection(MapNode from, MapNode to)
    {
        var lineObject = Instantiate(linePrefab, mapParent.transform);
        var lineRenderer = lineObject.GetComponent<LineRenderer>();
        var fromPoint = from.transform.position +
                        (to.transform.position - from.transform.position).normalized * offsetFromNodes;

        var toPoint = to.transform.position +
                      (from.transform.position - to.transform.position).normalized * offsetFromNodes;

        // line renderer with 2 points only does not handle transparency properly:
        lineRenderer.positionCount = linePointsCount;
        for (var i = 0; i < linePointsCount; i++)
        {
            lineRenderer.SetPosition(i,
                Vector3.Lerp(fromPoint, toPoint, (float) i / (linePointsCount - 1)));
        }
        
        var dottedLine = lineObject.GetComponent<DottedLineRenderer>();
        if(dottedLine != null) dottedLine.ScaleMaterial();
    }

    private MapNode GetNode(Point p)
    {
        return mapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
    }

    public NodeBlueprint GetBlueprint(NodeType type)
    {
        return blueprints.FirstOrDefault(n => n.nodeType == type);
    }
}
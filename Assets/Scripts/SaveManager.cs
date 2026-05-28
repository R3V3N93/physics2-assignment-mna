using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;

class SaveManager : MonoBehaviour
{
    public string fileName = "config.json";
    public string path = Application.dataPath + "\\";
    public void Save()
    {
        List<JeilNode> nodes = GameManager.GetNodes();
        List<JeilEdge> edges = GameManager.GetEdges();
        
        // Set index for each node for easier classification from JSON
        for (int i = 0; i < nodes.Count; i++) nodes[i].index = i;
        
        SaveData dataArray =  new SaveData();
        
        // Write all necessary stuffs to JSON
        dataArray.startNode = (GameManager.obj.managerPathfinding.startNode != null ? GameManager.obj.managerPathfinding.startNode.index : -1);
        dataArray.destinationNode = (GameManager.obj.managerPathfinding.destinationNode != null ? GameManager.obj.managerPathfinding.destinationNode.index : -1);
        foreach(JeilNode i in nodes) dataArray.nodes.Add(NodeSaveData.Create(i));
        foreach(JeilEdge i in edges) dataArray.edges.Add(EdgeSaveData.Create(i));
        
        Debug.Log(JsonUtility.ToJson(dataArray, true));
        File.WriteAllText(path + fileName, JsonUtility.ToJson(dataArray));
        Debug.Log("Saved config to " + path);
    }
    
    public void Load()
    {
        SaveData dataArray = new SaveData();
        string _jsonRawString = File.ReadAllText(path + fileName);
        
        JsonUtility.FromJsonOverwrite(_jsonRawString, dataArray);

        EditorManager editor = GameManager.obj.managerEditor;
        PathfindingManager pathfinding = GameManager.obj.managerPathfinding;
        
        // Remove all things from the scene
        foreach(JeilNode i in GameManager.GetNodes()) editor.DeleteNode(i);
        
        if (GameManager.obj.poolNode.transform.childCount != 0)
            Debug.LogError("There's something wrong with node purge process!");
        if (GameManager.obj.poolEdge.transform.childCount != 0)
            Debug.LogError("There's something wrong with edge purge process!");
        
        // Create nodes first
        foreach (NodeSaveData it in dataArray.nodes)
        {
            JeilNode createdNode = editor.CreateNode(it.pos, it.index, it.isLandmark);
            
            // Assign start/destination node in advance
            if (it.index == dataArray.startNode) pathfinding.startNode = createdNode;
            if (it.index == dataArray.destinationNode) pathfinding.destinationNode = createdNode;
        }

        JeilNode FindNodeFromIndex(int index)
        {
            foreach (JeilNode it in GameManager.GetNodes()) if (it.index == index) return it;
            return null;
        };
        
        foreach (EdgeSaveData it in dataArray.edges)
        {
            editor.ConnectNodes(FindNodeFromIndex(it.indexL), FindNodeFromIndex(it.indexR), it.cost);
        }
    }
}

[Serializable]
public class SaveData
{
    [SerializeField] public int startNode;
    [SerializeField] public int destinationNode;
    [SerializeField] public List<NodeSaveData> nodes = new List<NodeSaveData>();
    [SerializeField] public List<EdgeSaveData> edges = new List<EdgeSaveData>();
}

[Serializable]
public class NodeSaveData
{
    [SerializeField] public int index;
    [SerializeField] public Vector2 pos;
    [SerializeField] public bool isLandmark;

    static public NodeSaveData Create(JeilNode from)
    {
        NodeSaveData data = new NodeSaveData();
        data.pos = from.transform.position;
        data.index = from.index;
        data.isLandmark = from.visibleInPathfinding;
        return data;
    }
}

[Serializable]
public class EdgeSaveData
{
    [SerializeField] public int indexL;
    [SerializeField] public int indexR;
    [SerializeField] public int cost;
    static public EdgeSaveData Create(JeilEdge from)
    {
        EdgeSaveData data = new EdgeSaveData();
        data.indexL = from.connectedNodes[0].index;
        data.indexR = from.connectedNodes[1].index;
        data.cost = from.cost;
        return data;
    }
}

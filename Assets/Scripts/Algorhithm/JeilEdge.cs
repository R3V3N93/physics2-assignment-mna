using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;

public class JeilEdge : JeilElement
{
    public int cost = 1;
    public List<JeilNode> connectedNodes = new List<JeilNode>(); 

    [HideInInspector] public LineRenderer line;
    [HideInInspector] public TMP_InputField input;
    [HideInInspector] public EdgeCollider2D col;

    void Awake()
    {
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        input = GetComponent<TMP_InputField>();
        col =  GetComponent<EdgeCollider2D>();
        SetCost(1);
    }
    
    
    public void ConnectNodes(JeilNode what1, JeilNode what2)
    {
        if(what1 == null ||  what2 == null)
            Debug.LogError("why the fuck they don't exist");
        connectedNodes.Add(what1);
        connectedNodes.Add(what2);
    }

    void Update()
    {
        if (connectedNodes.Count == 2)
        {
            line.SetPosition(0, connectedNodes[0].transform.position);
            line.SetPosition(1, connectedNodes[1].transform.position);
            this.transform.position = ((connectedNodes[0].transform.position + connectedNodes[1].transform.position) / 2f) + Vector3.forward;
        
            List<Vector2> points = new List<Vector2>();
            points.Add(connectedNodes[0].transform.position - this.transform.position);
            points.Add(connectedNodes[1].transform.position - this.transform.position); // This is dumb. I need to make 2 separate arrays because their types need to be different?
            col.SetPoints(points);
        }
        
    }

    public void SetCost(int what)
    {
        this.cost = what;
        input.text = this.cost.ToString();
    }
}

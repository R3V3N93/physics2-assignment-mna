using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;

[Serializable]
public struct Components
{
    public GameObject root;
    public JeilBattery battery;
    public JeilResistor resistor;
}

public class JeilEdge : JeilElement
{
    public enum ComponentTypeT
    {
        Wire,
        Battery,
        Resistor
    };
    
    public ComponentTypeT componentType = ComponentTypeT.Wire;
    public Components components;
    
    public List<JeilNode> connectedNodes = new List<JeilNode>(); 
    [HideInInspector] public LineRenderer line;
    [HideInInspector] public EdgeCollider2D col;
    void Awake()
    {
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        col =  GetComponent<EdgeCollider2D>();
        SetComponent(ComponentTypeT.Wire);
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

            float y = this.transform.position.y - connectedNodes[0].transform.position.y;
            float x = this.transform.position.x - connectedNodes[0].transform.position.x;
            components.root.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(y, x)) * Mathf.Rad2Deg;
        }
    }
    
    public void SetComponent(ComponentTypeT to)
    {
        Debug.Log("Set Componenet to " + to);
        componentType = to;
        
        components.battery.gameObject.SetActive(to == ComponentTypeT.Battery);
        components.resistor.gameObject.SetActive(to == ComponentTypeT.Resistor);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class JeilNode : JeilElement
{
    public int index = -1;
    public float voltage = 0f;
    public List<JeilNode> neighbors = new List<JeilNode>();
    public Dictionary<JeilNode, JeilEdge> neighborEdges = new Dictionary<JeilNode, JeilEdge>();

    public void Hold()
    {
        gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layers.nodeHeld);
        // TODO : Add outline?
    }
    
    public void Unhold()
    {
        gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layers.node);
        // TODO : Disable outline?
    }
}
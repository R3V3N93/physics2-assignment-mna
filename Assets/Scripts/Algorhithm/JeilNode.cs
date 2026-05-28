using UnityEngine;
using System.Collections.Generic;

public class JeilNode : JeilElement
{
    public int index = -1;
    public List<JeilNode> neighbors = new List<JeilNode>();
    public Dictionary<JeilNode, JeilEdge> neighborEdges = new Dictionary<JeilNode, JeilEdge>();
    public bool visibleInPathfinding = false;
    
    public void Hold()
    {
        gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layerNodeHeld);
        // TODO : Add outline?
    }
    
    public void Unhold()
    {
        gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layerNode);
        // TODO : Disable outline?
    }
}
using UnityEngine;
using System.Collections.Generic;

public class JeilSelection
{
    public enum StatesT
    {
        Node,
        Edge
    };
    public StatesT state = StatesT.Node;
    public List<JeilNode> nodes;
    public JeilEdge edge;
}

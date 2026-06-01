using UnityEngine;

[CreateAssetMenu(fileName = "Layers", menuName = "Scriptable Objects/LayerSO")]
public class LayerSO : ScriptableObject
{
    public LayerMask node;
    public LayerMask nodeHeld;
    public LayerMask edge;
}

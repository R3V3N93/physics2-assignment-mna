using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    Queue<Operation> undoBuffer;
}

public abstract class Operation
{
    public virtual void Undo()
    {
        
    }
}
/*
class OP_CreateNode : Operation
{
    JeilNode createdNode;
    
    public override void Undo()
    {
        createdNode.Remove();
    }
}
class OP_MoveNode : Operation
{
    JeilNode targetNode;
    Vector2 originalPos;
    public override void Undo()
    {
        targetNode.MoveTo(OriginalPos);
    }
}
class OP_DeleteNode : Operation
{
    NodeSaveData removedNodeData;

    public override void Undo()
    {
        EditorManager.CreateNodefromDatum(removedNodeData);
    }
}*/
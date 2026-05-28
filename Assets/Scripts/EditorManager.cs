using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditorManager : MonoBehaviour
{
    enum StatesT
    {
        Selecting,
        Connecting,
        Moving
    }
    [SerializeField] private StatesT state = StatesT.Selecting;
    public JeilElement selected;
    public List<JeilElement> selections = new List<JeilElement>();
    [Header("UI")] 
    public GameObject ui;
    public GameObject menuBG;
    public GameObject menuNode;
    public GameObject menuEdge;
    public Toggle menuNodeLandmarkToggle;
    public TMP_InputField menuEdgeCostInput;

    [Header("Connecting")] 
    [SerializeField] private JeilNode connectStart;
    [SerializeField] private JeilNode connectEnd;
    
    [Header("Selecting")]
    [SerializeField] private Rect dragRect = Rect.zero;
    [SerializeField] private bool isDragging;
    [SerializeField] private Texture2D dragImage;
    [SerializeField] private Texture2D testImage;
    
    private void Update()
    {
        switch (state)
        {
            case StatesT.Selecting:
                break;
            case StatesT.Connecting:
                if(connectEnd != null) connectEnd.transform.position = GameManager.MousePosition();
                break;
            case StatesT.Moving:
                break;
        }
    }

    private void OnGUI()
    {
        UpdateDragging();
    }

    private void OnDisable()
    {
        GameManager.obj.pinput.eventRightClick -= RightClick;
        GameManager.obj.pinput.eventClickOn      -= LeftClickOn;
        GameManager.obj.pinput.eventClickOff      -= LeftClickOff;
        
        GameManager.obj.pinput.eventDelete     -= Delete;
        
        GameManager.obj.pinput.eventCancel     -= Cancel;
        
        ClosePropertyMenu(true);
        
        ui.SetActive(false);
    }
    
    private void OnEnable()
    {
        GameManager.obj.pinput.eventRightClick += RightClick;
        GameManager.obj.pinput.eventClickOn      += LeftClickOn;
        GameManager.obj.pinput.eventClickOff      += LeftClickOff;
        
        GameManager.obj.pinput.eventDelete     += Delete;
        
        GameManager.obj.pinput.eventCancel     += Cancel;
        
        GameManager.obj.state = GameManager.GameState.Editing;
        
        ui.SetActive(true);
    }
    
    public void Delete()
    {
        JeilElement elem = GameManager.GetElementOnMouse();
        if(elem is JeilNode)
            DeleteNode(elem as JeilNode);
    }
    
    public void LeftClickOn()
    {
        switch (state)
        {
            case StatesT.Selecting:
                StartDragging();
                break;
            case StatesT.Connecting:
                break;
            case StatesT.Moving:
                break;
        }
        
    }
    public void LeftClickOff()
    {
        JeilElement elem = GameManager.GetElementOnMouse();
        switch (state)
        {
            case StatesT.Selecting:
                StopDragging();
                if (elem == null)
                    break;
                
                selected = elem;
                OpenPropertyMenu();
                break;
            case StatesT.Connecting:
                
                if (elem == null || elem is JeilEdge)
                {
                    connectEnd.Unhold();
                    ConnectNodes(connectStart, connectEnd);
                    connectStart = null;
                    connectEnd = null;
                    state = StatesT.Selecting;
                    break;
                }

                if (elem is JeilNode)
                {
                    DeleteNode(connectEnd);
                    ConnectNodes(connectStart, (JeilNode)elem);
                    connectStart = null;
                    connectEnd = null;
                    state = StatesT.Selecting;
                    break;
                }
                break;
            case StatesT.Moving:
                break;
        }
        
    }
    
    public void RightClick()
    {
        JeilElement elem = GameManager.GetElementOnMouse();
        switch (state)
        {
            case StatesT.Selecting:
                if (elem == null)
                {
                    state = StatesT.Connecting;
                    connectStart = CreateNode(GameManager.MousePosition());
                    connectEnd = CreateNode(GameManager.MousePosition());
                    connectEnd.Hold();
                    break;
                }
                if (elem is JeilNode)
                {
                    if (GameManager.obj.pinput.ctrl)
                    {
                        state = StatesT.Connecting;
                        connectStart = (JeilNode)elem;
                        connectEnd = CreateNode(GameManager.MousePosition());
                        connectEnd.Hold();
                        break;
                    }
                    break;
                }
                break;
            case StatesT.Connecting:
                if (elem == null || elem is JeilEdge)
                {
                    ConnectNodes(connectStart, connectEnd);
                    connectEnd.Unhold();
                    connectStart = connectEnd;
                    connectEnd = CreateNode(GameManager.MousePosition());
                    connectEnd.Hold();
                    break;
                }

                if (elem is JeilNode)
                {
                    DeleteNode(connectEnd);
                    ConnectNodes(connectStart, (JeilNode)elem);
                    
                    connectStart = null;
                    connectEnd = null;
                    state = StatesT.Selecting;
                    break;
                }
                break;
            case StatesT.Moving:
                break;
        }
    }

    private void StartDragging()
    {
        dragRect.Set(GameManager.obj.pinput.mousePosition.x, Screen.height - GameManager.obj.pinput.mousePosition.y, 0, 0);
        selections.Clear();
        isDragging = true;
    }

    private void UpdateDragging()
    {
        if (!isDragging) return;
        float width = GameManager.obj.pinput.mousePosition.x - dragRect.x;
        float height = (Screen.height - GameManager.obj.pinput.mousePosition.y) - dragRect.y;
        dragRect.Set(dragRect.x, dragRect.y, width, height);
        GUI.DrawTexture(dragRect, dragImage, ScaleMode.StretchToFill, true);
    }
    
    private void StopDragging()
    {
        if (!isDragging) return;

        if (dragRect.width * dragRect.height < 100)
        {
            Collider2D raycasted = Physics2D.OverlapPoint(GameManager.MousePosition(), GameManager.obj.layerEdge|GameManager.obj.layerNode);
            if (raycasted != null)
            {
                if (raycasted.gameObject.layer == GameManager.GetRealLayer(GameManager.obj.layerEdge))
                {
                    
                }
                else
                {
                }
            }
        }
        
        isDragging = false;
        dragRect = Rect.zero;
    }

    public void Cancel()
    {
        ClosePropertyMenu(true);
    }

    public JeilNode CreateNode(Vector2 pos, int index = -1, bool landmark = false)
    {   
        JeilNode product = Instantiate(GameManager.obj.prefabNode, pos, Quaternion.identity, GameManager.obj.poolNode.transform).GetComponent<JeilNode>();
        if(index != -1 && index >= 0)
            product.index = index;
        product.visibleInPathfinding = landmark;
        return product;
    }

    public void DeleteNode(JeilNode what)
    {
        foreach (JeilNode neighbor in what.neighbors)
        {
            if (neighbor == null)
                continue;
            // 후에 반드시 Destroy()으로 바뀌어야함 !!!!!!!!!!!!!!!!!!!!
            DestroyImmediate(what.neighborEdges[neighbor].gameObject);
        }
        DestroyImmediate(what.gameObject);
    }
    
    public void ConnectNodes(JeilNode what1, JeilNode what2, int cost = 1)
    {
        Debug.Log("Connecting from what1 to what2 ");
        if(!what1 || !what2)
            return;

        if(what1.neighbors.Contains(what2))
            return;

        what1.neighbors.Add(what2);
        what2.neighbors.Add(what1);
        
        what1.gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layerNode);
        what2.gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layerNode);

        GameObject _edge = Instantiate(GameManager.obj.prefabEdge, (what1.transform.position + what2.transform.position) / 2, Quaternion.identity, GameManager.obj.poolEdge.transform);

        JeilEdge edge = _edge.GetComponent<JeilEdge>();
        edge.SetCost(cost);
        edge.ConnectNodes(what1, what2);
        what1.neighborEdges[what2] = edge;
        what2.neighborEdges[what1] = edge;
    }

    public void OpenPropertyMenu()
    {
        ClosePropertyMenu();
        if (selected == null) return;
        
        menuBG.SetActive(true);
        
        if(selected is JeilNode)
        {
            menuNode.SetActive(true);
            menuNodeLandmarkToggle.isOn = ((JeilNode)selected).visibleInPathfinding;
        }
        else if(selected is JeilEdge)
        {
            menuEdge.SetActive(true);
            menuEdgeCostInput.text = ((JeilEdge)selected).cost.ToString();
        }
    }

    public void ClosePropertyMenu(bool clearSelected = false)
    {
        if (clearSelected) selected = null;
        menuBG.SetActive(false);
        menuNode.SetActive(false);
        menuEdge.SetActive(false);
    }

    public void SetToStartNode()
    {
        if (selected is not JeilNode) return;
        if ((JeilNode)selected == GameManager.obj.managerPathfinding.destinationNode) return;
        GameManager.obj.managerPathfinding.startNode = (JeilNode)selected;
    }
    
    public void SetToDestinationNode()
    {
        if (selected is not JeilNode) return;
        if ((JeilNode)selected == GameManager.obj.managerPathfinding.startNode) return;
        GameManager.obj.managerPathfinding.destinationNode = (JeilNode)selected;
    }

    public void ToggleLandmark(bool toggle)
    {
        if (selected is not JeilNode) return;
        JeilNode sel =  selected as JeilNode;
        sel.visibleInPathfinding = toggle;
    }
    
    public void SetEdgeCost(string to) // Stupid ngl. I just solely want int input.
    {
        if (selected is not JeilEdge) return;
        JeilEdge sel =  selected as JeilEdge;
        sel.SetCost(int.Parse(to));
    }
}

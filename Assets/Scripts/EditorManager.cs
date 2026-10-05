using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Rendering;

public class EditorManager : MonoBehaviour
{
    public JeilEdge voltageSource; // one and only.
    public JeilNode referenceNode;
    
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
            Collider2D raycasted = Physics2D.OverlapPoint(GameManager.MousePosition(), GameManager.obj.layers.edge|GameManager.obj.layers.node);
            if (raycasted != null)
            {
                if (raycasted.gameObject.layer == GameManager.GetRealLayer(GameManager.obj.layers.edge))
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
        JeilNode product = Instantiate(GameManager.obj.prefabs.node, pos, Quaternion.identity, GameManager.obj.poolNode.transform).GetComponent<JeilNode>();
        if(index != -1 && index >= 0)
            product.index = index;
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
    
    public void ConnectNodes(JeilNode what1, JeilNode what2)
    {
        Debug.Log("Connecting from what1 to what2 ");
        if(!what1 || !what2)
            return;

        if(what1.neighbors.Contains(what2))
            return;

        what1.neighbors.Add(what2);
        what2.neighbors.Add(what1);
        
        what1.gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layers.node);
        what2.gameObject.layer = GameManager.GetRealLayer(GameManager.obj.layers.node);

        GameObject _edge = Instantiate(GameManager.obj.prefabs.edge, (what1.transform.position + what2.transform.position) / 2, Quaternion.identity, GameManager.obj.poolEdge.transform);

        JeilEdge edge = _edge.GetComponent<JeilEdge>();
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
        }
        else if(selected is JeilEdge)
        {
            menuEdge.SetActive(true);
        }
    }

    public void ClosePropertyMenu(bool clearSelected = false)
    {
        if (clearSelected) selected = null;
        menuBG.SetActive(false);
        menuNode.SetActive(false);
        menuEdge.SetActive(false);
    }
    
    public void SetComponent(int to)
    {
        if (selected is JeilEdge)
        {
            JeilEdge.ComponentTypeT toEnum = (JeilEdge.ComponentTypeT)to;
            if (toEnum == JeilEdge.ComponentTypeT.Battery)
            {
                if (voltageSource != null)
                {
                    voltageSource.connectedNodes[0].voltage = 0;
                    voltageSource.SetComponent(JeilEdge.ComponentTypeT.Wire);
                }
                voltageSource = ((JeilEdge)selected);
                referenceNode = voltageSource.connectedNodes[1];
            }

            
            ((JeilEdge)selected).SetComponent((JeilEdge.ComponentTypeT)to);
        }
    }

    public void SetComponentValue(string to)
    {
        if (selected is JeilEdge)
        {
            JeilEdge edge = ((JeilEdge)selected);
            switch (edge.componentType)
            {
                case JeilEdge.ComponentTypeT.Battery:
                    edge.components.battery.deltaVoltage = float.Parse(to);
                    edge.connectedNodes[0].voltage = edge.components.battery.deltaVoltage;
                    edge.connectedNodes[1].voltage = 0;
                    break;
                case JeilEdge.ComponentTypeT.Resistor:
                    edge.components.resistor.resistance.value = float.Parse(to);
                    break;
            }
        }
    }
    
    public void RunMNA()
    {
        if(!referenceNode) Debug.LogError("Reference node is not defined retard!");
        if(!voltageSource) Debug.LogError("Voltage Source is not defined retard!");
        referenceNode.voltage = 0; // sets it as GND
        
        List<JeilNode> nodes = GameManager.GetNodes();
        List<JeilEdge> wiresBatteries = new List<JeilEdge>();
        List<JeilEdge> resistors = new List<JeilEdge>();

        foreach (JeilEdge edge in GameManager.GetEdges())
        {
            if(edge.componentType == JeilEdge.ComponentTypeT.Resistor) resistors.Add(edge);
            else wiresBatteries.Add(edge);
        }
        
        // Ax = Z

        int xSize = wiresBatteries.Count + nodes.Count - 1;
        
        double[,] A = new double[xSize, xSize]; // -1(ground node) + 1(voltage source)
        
        JeilElement[] x = new JeilElement[xSize]; // Deducts the ground node
        // node -> wireNBatteries
        double[] Z = new double[xSize];

        void DisplayMatrices()
        {
            string As = "";
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < xSize; y++)
                {
                    As += A[x, y].ToString();
                    As += ", ";
                }

                As += "\n";
            }

            string Zs = "";
            for (int i = 0; i < xSize; i++)
            {
                Zs += Z[i];
                Zs += "\n";
            }
            Debug.Log(As);
            Debug.Log(Zs);
        }
        
        // x 먼저 채움
        // 노드 먼저, 그 이후 가상 전류들(전류원, 와이어)
        int index = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            JeilNode curNode = nodes[i];
            if (curNode == referenceNode) continue;

            x[index] = curNode;
            index++;
        }
        for (int i = 0; i < wiresBatteries.Count; i++)
        {
            x[index] = wiresBatteries[i];
            index++;
        }
        if(index != xSize) Debug.LogError("xSize wrong?");
        
        for (int i = 0; i < xSize; i++)
        {
            Debug.Log("x : ", x[i].gameObject);
        }
        
        // 이후 각 연립 방정식 변환

        int FindInX(JeilElement what)
        {
            for (int i = 0; i < xSize; i++)
            {
                if (x[i] == what)
                    return i;
            }

            return -1;
        }
        
        index = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            JeilNode node = nodes[i];
            if (node == referenceNode) continue;
            foreach (JeilNode neighbor in node.neighbors)
            {
                JeilEdge nEdge = node.neighborEdges[neighbor];
                switch (nEdge.componentType)
                {
                    case JeilEdge.ComponentTypeT.Wire:
                        Debug.Log("test" + index.ToString() +  FindInX(nEdge).ToString());
                        A[index, FindInX(nEdge)] += 1.0;
                        break;
                    case JeilEdge.ComponentTypeT.Resistor: 
                        // v1 = neighbor, v0 = node
                        // v1 - v0 / r
                        if(neighbor != referenceNode) A[index, FindInX(neighbor)] += 1.0 / nEdge.components.resistor.resistance.value;
                        A[index, FindInX(node)] -= 1.0 / nEdge.components.resistor.resistance.value;
                        break;
                    case JeilEdge.ComponentTypeT.Battery:
                        A[index, FindInX(nEdge)] += 1.0;
                        break;
                }
            }

            index++;
        }

        A[index, FindInX(voltageSource.connectedNodes[0])] += 1.0;
        Z[index] = voltageSource.connectedNodes[0].voltage;
        index++;

        for (int i = 0; i < wiresBatteries.Count; i++)
        {
            JeilEdge edge =  wiresBatteries[i];
            if (edge.componentType == JeilEdge.ComponentTypeT.Battery) continue;

            if(edge.connectedNodes[0] != referenceNode) A[index, FindInX(edge.connectedNodes[0])] += 1;
            if(edge.connectedNodes[1] != referenceNode) A[index, FindInX(edge.connectedNodes[1])] -= 1;

            index++;
        }
        
        if(index != xSize) Debug.LogError("xSize wrong?");
        DisplayMatrices();
        
        // Calculate Asnwer using Gauss Michael Jordan
        double[] xAnswer = GaussJordan(A, Z);
        string xs = "";
        for (int i = 0; i < xSize; i++)
        {
            xs += xAnswer[i].ToString();

            xs += "\n";
        }
        
        // And put answers accordingly
        
        for (int i = 0; i < nodes.Count -1; i++)
        {
            JeilElement element = x[i];

            if (element is JeilNode)
            {
                JeilNode node =  element as JeilNode;
                node.voltage = (float)xAnswer[i]; // float conversion is bullshit.
            }
        }

        for (int i = 0; i < resistors.Count; i++)
        {
            JeilElement element = resistors[i];
            if (element is JeilEdge)
            {
                JeilEdge edge = element as JeilEdge;

                if (edge.componentType == JeilEdge.ComponentTypeT.Resistor)
                {
                    edge.components.resistor.voltage.value =
                        Math.Abs(edge.connectedNodes[0].voltage - edge.connectedNodes[1].voltage);
                    edge.components.resistor.current.value = edge.components.resistor.voltage.value /
                                                             edge.components.resistor.resistance.value; // 오옴! 나이스! 으시안 오옴!
                    
                }
            }
        }

        Debug.Log(xs);
        Debug.Log("MNA Completed!");
    }
    
    double[]? GaussJordan(double[,] A, double[] b)
    {
        int n = b.Length;
        
        double[,] M = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                M[i, j] = A[i, j];
            M[i, n] = b[i];
        }
 
        for (int col = 0; col < n; col++)
        {
            int pivotRow = col;
            double maxVal = Math.Abs(M[col, col]);
            for (int row = col + 1; row < n; row++)
            {
                if (Math.Abs(M[row, col]) > maxVal)
                {
                    maxVal = Math.Abs(M[row, col]);
                    pivotRow = row;
                }
            }
 
            if (Math.Abs(M[pivotRow, col]) < 1e-12)
            {
                Debug.Log("Matrix is singular or nearly singular — no unique solution.");
                return null;
            }
            
            if (pivotRow != col)
                for (int j = 0; j <= n; j++)
                    (M[col, j], M[pivotRow, j]) = (M[pivotRow, j], M[col, j]);
            
            double pivot = M[col, col];
            for (int j = 0; j <= n; j++)
                M[col, j] /= pivot;
            
            for (int row = 0; row < n; row++)
            {
                if (row == col) continue;
                double factor = M[row, col];
                for (int j = 0; j <= n; j++)
                    M[row, j] -= factor * M[col, j];
            }
        }
        
        double[] x = new double[n];
        for (int i = 0; i < n; i++)
            x[i] = M[i, n];
 
        return x;
    }
}

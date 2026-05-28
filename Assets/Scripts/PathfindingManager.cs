using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PathfindingManager : MonoBehaviour
{
    [Header("UI")] 
    public GameObject ui;
    enum Algorithm
    {
        BreadthFirstSearch,
        Dijkstra,
        Astar
    };
    [Header("Global Algorhithm thingys")]
    public JeilNode startNode; 
    public JeilNode destinationNode;
    [SerializeField] Algorithm selectedAlgorhithm = Algorithm.BreadthFirstSearch;

    [Header("Debug")]
    [SerializeField] List<JeilNode> shortestPath = new List<JeilNode>();

    [SerializeField] private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }
    
    private void OnDisable()
    {
        GameManager.obj.pinput.eventRightClick -= RightClick;
        GameManager.obj.pinput.eventClickOn      -= LeftClickOn;
        GameManager.obj.pinput.eventClickOff      -= LeftClickOff;
        
        ui.SetActive(false);
    }
    
    private void OnEnable()
    {
        GameManager.obj.pinput.eventRightClick += RightClick;
        GameManager.obj.pinput.eventClickOn      += LeftClickOn;
        GameManager.obj.pinput.eventClickOff      += LeftClickOff;
        
        GameManager.obj.state = GameManager.GameState.PathFinding;
        
        ui.SetActive(true);
    }
    
    public void SetAlgorithm(int to)
    {
        Debug.Log(to);
        selectedAlgorhithm = (Algorithm)to;
    }
    
    public void LeftClickOn()
    {   
    }

    public void LeftClickOff()
    {   
    }

    public void RightClick()
    {
        
    }

    public void StartPathFinding()
    {
        line.positionCount = 0;
        switch (selectedAlgorhithm)
        {
            case Algorithm.BreadthFirstSearch:
                BreadthFirstSearch();
                break;
            case Algorithm.Dijkstra:
                Dijkstra();
                break;
            case Algorithm.Astar:
                Astar();
                break;
        }

        if (shortestPath.Count == 0)
        {
            Debug.LogError("Something's wrong with Algorhithm execution. Check log.");
            return;
        }

        line.positionCount = shortestPath.Count;
        for (int i = 0; i < shortestPath.Count; i++)
        {
            line.SetPosition(i, shortestPath[i].transform.position);
        }
        
        // Pathfinding is over. Trim the list
        shortestPath.Clear();
    }

    public void BreadthFirstSearch()
    {
        if (startNode == null && destinationNode == null)
        {
            Debug.Log("Dijkstra: startNode or destinationNode is null");
            return; 
        }
        //시작 노드랑 끝 노드 없을때 오류 방지
        
        Queue<JeilNode> allNodeQueue = new Queue<JeilNode>();
        allNodeQueue.Enqueue((startNode));
        //고리 큐(allNodeQueue) 할당 완료
        
        Dictionary<JeilNode, JeilNode> came_from = new Dictionary<JeilNode, JeilNode>();
        came_from[startNode] = null;
        //경로 설정
        
        while (allNodeQueue.Count > 0)
        {
            JeilNode currentNode = allNodeQueue.Dequeue(); // 고리 큐의 첫번째 원소를 고름
            foreach (JeilNode neighbor in  currentNode.neighbors)// 고른 노드의 이웃 노드 중에서
            {
                if (!came_from.ContainsKey(neighbor)) // 도착하지 않은 노드가 있다면, 여기 고쳐야됨!!!!!!!!!!!!!!!!!!!!!!!!!! JeilNode에 약간의 변화 필요 변수 하나 isVisited 정도?
                {
                    allNodeQueue.Enqueue(neighbor); // 그 노드에서 부터 다시 이웃 탐색 하기 위해 고리에 추가
                    came_from[neighbor] = currentNode;  // 그 노드에 도착! 경로를 저장함
                }	
            }		
        }

        JeilNode sizak = destinationNode;
        shortestPath.Add(destinationNode);
        while (came_from[sizak] != null)
        {
            shortestPath.Add(came_from[sizak]);
            sizak = came_from[sizak];
        }
        shortestPath.Add(startNode);
        shortestPath.Reverse();
    }

    public void Dijkstra()
    {
        
    }

    public void Astar()
    {
        
    }
}

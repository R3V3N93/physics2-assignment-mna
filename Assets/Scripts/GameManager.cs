using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    static public GameManager obj;
    public InputSO pinput;
    public Camera playerCamera;
    

    [Header("SO")] 
    public PrefabSO prefabs;

    public LayerSO layers;
    
    [Header("Pools")]
    public GameObject poolNode;
    public GameObject poolEdge;
    
    [Header("Settings")] 
    [Tooltip("Max amount to scroll in orthographic")]
    [SerializeField] private uint maxScroll = 50;
    [SerializeField] private float panningSpeed = 0.05f;

    [Header("Managers")] 
    public EditorManager managerEditor;

    void Awake()
    {
        if(obj == null) obj = this;
        else Destroy(this);
        
        managerEditor.gameObject.SetActive(true);
    }

    void Update()
    {
        Scroll();
        Panning();
    }
    
    void Scroll()
    {
        if (pinput.scroll.sqrMagnitude > 0)
        {
            playerCamera.orthographicSize -= pinput.scroll.y;
            playerCamera.orthographicSize = Mathf.Clamp(playerCamera.orthographicSize, 1, maxScroll);
        }
    }

    void Panning()
    {
        if (pinput.middleClicked)
        {
            playerCamera.transform.position += -new Vector3(pinput.mouseDelta.x, pinput.mouseDelta.y, 0) * panningSpeed;
        }
    }

    static public Vector2 MousePosition()
    {
        Vector3 screenPosition = new Vector3(obj.pinput.mousePosition.x, obj.pinput.mousePosition.y, obj.playerCamera.nearClipPlane);
        if (obj.pinput.shift && obj.pinput.mouseDelta.x != obj.pinput.mouseDelta.y)
        {
            screenPosition.x -= obj.pinput.mouseDelta.x;
            screenPosition.y -= obj.pinput.mouseDelta.y;

            if (obj.pinput.mouseDelta.x > obj.pinput.mouseDelta.y)
                screenPosition.x += obj.pinput.mouseDelta.x;
            else screenPosition.y += obj.pinput.mouseDelta.y;
        }
        return obj.playerCamera.ScreenToWorldPoint(screenPosition);
    }

    static public JeilElement GetElementOnMouse()
    {
        Collider2D raycasted = Physics2D.OverlapPoint(MousePosition(), obj.layers.node|obj.layers.edge);
        if (raycasted != null)
        {
            JeilElement element = raycasted.gameObject.GetComponent<JeilElement>();
            return element;
        }
        return null;
    }

    static public int GetRealLayer(LayerMask from)
    {
        return (int)Math.Log(from.value, 2);
    }

    static public List<JeilNode> GetNodes()
    {
        List<JeilNode> temp = new List<JeilNode>();
        for (int i = 0; i < GameManager.obj.poolNode.transform.childCount; i++)
        {
            Transform child = GameManager.obj.poolNode.transform.GetChild(i);
            JeilNode nodeFromChild = child.GetComponent<JeilNode>();
            temp.Add(nodeFromChild);
        }

        return temp;
    }
    
    static public List<JeilEdge> GetEdges()
    {
        List<JeilEdge> temp = new List<JeilEdge>();
        for (int i = 0; i < GameManager.obj.poolEdge.transform.childCount; i++)
        {
            Transform child = GameManager.obj.poolEdge.transform.GetChild(i);
            JeilEdge edgeFromChild = child.GetComponent<JeilEdge>();
            temp.Add(edgeFromChild);
        }

        return temp;
    }

    
}

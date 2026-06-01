using System;
using UnityEngine;
using TMPro;

[Serializable]
public struct Property
{
    public float value;
    public TextMeshPro label;
}

public class JeilResistor : JeilComponent
{
    public Property voltage;
    public Property current;
    public Property resistance;
    
    void Update()
    {
        voltage.label.text = voltage.value.ToString();
        current.label.text = current.value.ToString();
        resistance.label.text = resistance.value.ToString();
    }
}

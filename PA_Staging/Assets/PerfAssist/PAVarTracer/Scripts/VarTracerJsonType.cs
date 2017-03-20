using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;


[System.Serializable]
public class Group 
{
    public Group(string name ,VariableParm[] vp =null)
    {
        Name = name;
        VarItems = vp;       
    }

    public Group(string name, EventParm[] ep = null)
    {
        Name = name;
        EventItems = ep;
    }

    public Group(string name, VariableParm[] vp,EventParm[] ep)
    {
        Name = name;
        VarItems = vp;
        EventItems = ep;
    }

    public string Name;
    public VariableParm[] VarItems;
    public EventParm[] EventItems;
    public int RuningState;
    public long TimeStamp;
}

public struct VariableParm
{
    public string VariableName;
    public float VariableValue;
};

public struct EventParm
{
    public string EventName;
    public float EventDuration;
    public string EventDesc;
};

[System.Serializable]
public class VarTracerJsonType
{
    public string logicName;
    public string[] variableName;
    public float[] variableValue;
    public string[] eventName;
    public float[] eventDuration;
    public string[] eventDesc;
    public int runingState;
    public long timeStamp;
}

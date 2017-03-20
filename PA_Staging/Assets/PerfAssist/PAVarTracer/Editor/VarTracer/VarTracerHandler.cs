using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VarTracerHandler
{
    public static void ResoloveJsonMsg(VarTracerJsonType resolved)
    {
        int variableCount = resolved.variableName.Length;
        if (variableCount != resolved.variableValue.Length)
            Debug.LogErrorFormat("Parameter Resolove Json Error ,variableCount = {0}", variableCount);
        int eventCount = resolved.eventName.Length;
        if (eventCount != resolved.eventDuration.Length || eventCount != resolved.eventDesc.Length)
            Debug.LogErrorFormat("Parameter Resolove Json Error ,eventCount = {0}", eventCount);

        long timeStamp = resolved.timeStamp;
        if (VarTracerNet.Instance.StartTimeStamp == 0)
        {
            VarTracerNet.Instance.StartTimeStamp = VarTracerUtils.GetTimeStamp();
            VarTracerNet.Instance.NetDeltaTime = VarTracerNet.Instance.StartTimeStamp - timeStamp;
        }
        timeStamp += VarTracerNet.Instance.NetDeltaTime;

        bool hasLogicalName = !string.IsNullOrEmpty(resolved.logicName);

        for (int i = 0; i < variableCount; i++)
        {
            if (hasLogicalName)
                DefineVariable(resolved.variableName[i], resolved.logicName);
            UpdateVariable(timeStamp, resolved.variableName[i], resolved.variableValue[i]);
        }

        for (int i = 0; i < eventCount; i++)
        {
            if (hasLogicalName)
                DefineEvent(resolved.eventName[i], resolved.logicName);
            if (resolved.eventDuration[i] != -1)
                SendEvent(timeStamp, resolved.eventName[i], resolved.eventDuration[i], resolved.eventDesc[i]);
        }

        if (resolved.runingState == (int)VarTracerConst.RunningState.RunningState_Start)
        {
            StartVarTracer();
        }
        else if (resolved.runingState == (int)VarTracerConst.RunningState.RunningState_Pause)
        {
            StopVarTracer();
        }
    }

    public static void DefineVariable(string variableName, string variableBody)
    {
#if UNITY_EDITOR
        foreach (var varBody in VarTracer.Instance.VariableBodys.Values)
        {
            if (varBody.VariableDict.ContainsKey(variableName))
            {
                //Debug.LogFormat("variableName {0} ,Already Exsit!", variableName);
                return;
            }
        }

        if (!VarTracer.Instance.VariableBodys.ContainsKey(variableBody))
        {
            var body = new VarTracerLogicalBody(variableBody);
            body.VariableDict[variableName] = new VarTracerVariable(variableName,variableBody);
            VarTracer.Instance.VariableBodys[variableBody] = body;
        }

        var variableDict = VarTracer.Instance.VariableBodys[variableBody].VariableDict;
        if (!variableDict.ContainsKey(variableName))
        {
            variableDict[variableName] = new VarTracerVariable(variableName,variableBody);
        }
#endif
    }

    public static void UpdateVariable(long timeStamp ,string variableName, float value)
    {
        if (!GraphItWindow.isVarTracerStart())
            return ;
#if UNITY_EDITOR
        foreach (var VarBody in VarTracer.Instance.VariableBodys.Values)
        {
            if (VarBody.VariableDict.ContainsKey(variableName))
            {
                var var = VarBody.VariableDict[variableName];
                var.InsertValue(new VarDataInfo(value, VarTracerNet.Instance.GetCurrentFrameFromTimestamp(timeStamp)));
            }
        }
#endif
    }

    public static void DefineEvent(string eventName, string variableBody)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(eventName))
            return;

        if (!VarTracer.Instance.VariableBodys.ContainsKey(variableBody))
        {
            var body = new VarTracerLogicalBody(variableBody);
            VarTracer.Instance.VariableBodys[variableBody] = body;
        }

        foreach (var varBody in VarTracer.Instance.VariableBodys)
        {
            foreach (var eName in  varBody.Value.EventInfos.Keys)
            {
                if(eventName.Equals(eName))
                {
                    //Debug.LogErrorFormat("Define Event Name Already Exist!");
                    return;
                }
            }
        }
        VarTracer.Instance.VariableBodys[variableBody].RegistEvent(eventName);
#endif
    }

    public static void SendEvent(long timeStamp, string eventName, float duration = 0, string desc = "")
    {
        if (!GraphItWindow.isVarTracerStart())
            return;
        foreach (var varBody in VarTracer.Instance.VariableBodys)
        {
            foreach (var eName in varBody.Value.EventInfos.Keys)
            {
                if (eventName.Equals(eName))
                {
                    List<EventData> listEvent;
                    varBody.Value.EventInfos.TryGetValue(eventName, out listEvent);
                    listEvent.Add(new EventData(VarTracerNet.Instance.GetCurrentFrameFromTimestamp(timeStamp), eventName, desc, duration));
                    break;
                }
            }
        }
    }

    public static void StartVarTracer()
    {
        GraphItWindow.StartVarTracer();
    }

    public static void StopVarTracer()
    {
        GraphItWindow.StopVarTracer();
    }
}

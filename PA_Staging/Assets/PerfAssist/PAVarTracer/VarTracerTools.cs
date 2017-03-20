using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

//Demo
//VarTracerTools.DefineEvent("ATTACK", "Camera");
//VarTracerTools.UpdateVariable("CameraV_X", Camera.main.velocity.x);
//VarTracerTools.SendEvent("JUMP", 1.0f, "PLAYER JUMP");
//VarTracerTools.SendEvent("ATTACK");
//VarTracerTools.DefineEvent("ATTACK", "Camera");

//{
//    VariableParm vp ;
//    vp.VariableName = "ValueName";
//    vp.VariableValue = 1.0f;

//    EventParm ep;
//    ep.EventName = "EventName";
//    ep.EventDuration = 1.5f;
//    ep.EventDesc = "EventDesc";

//    VarTracerTools.SendGroupLoop(
//        new Group("Test", new VariableParm[] { vp }, new EventParm[] { ep })
//    );
//}

public class VarTracerTools
{
    public static void DefineVariable(string variableName, string LogicalName)
    {
        VarTracerJsonType vtjt = new VarTracerJsonType();
        vtjt.logicName = LogicalName;
        vtjt.variableName = new string[] { variableName };
        vtjt.variableValue = new float[] { 0 };
        VarTracerSender.Instance.SendJsonMsg(vtjt);
    }

    public static void UpdateVariable(string variableName, float value)
    {
        VarTracerJsonType vtjt = new VarTracerJsonType();
        vtjt.variableName = new string[] { variableName };
        vtjt.variableValue = new float[] { value };
        VarTracerSender.Instance.SendJsonMsg(vtjt);
    }

    public static void DefineEvent(string eventName, string variableBody)
    {
        VarTracerJsonType vtjt = new VarTracerJsonType();
        vtjt.logicName = variableBody;
        vtjt.eventName = new string[] { eventName };
        vtjt.eventDuration = new float[] { -1 };
        vtjt.eventDesc = new string[] { "" };
        VarTracerSender.Instance.SendJsonMsg(vtjt);
    }

    public static void SendEvent(string eventName, float duration = 0, string desc = "")
    {
        VarTracerJsonType vtjt = new VarTracerJsonType();
        vtjt.eventName = new string[] { eventName };
        vtjt.eventDuration = new float[] { duration };
        vtjt.eventDesc = new string[] { desc };
        VarTracerSender.Instance.SendJsonMsg(vtjt);
    }

    public static void SendGroupLoop(Group vjp)
    {
        if (vjp == null)
            return;

        VarTracerJsonType vtjt = new VarTracerJsonType();
        vtjt.logicName = vjp.Name;
        vtjt.runingState = vjp.RuningState;

        if(vjp.VarItems !=null)
        {
            int count = vjp.VarItems.Length;
            vtjt.variableName  = new string [count];
            vtjt.variableValue = new float[count];
            for (int i = 0; i < count; i++)
            {
                vtjt.variableName[i] = vjp.VarItems[i].VariableName;
                vtjt.variableValue[i] = vjp.VarItems[i].VariableValue;
            }
        }

        if(vjp.EventItems !=null)
        {
            int count = vjp.EventItems.Length;
            vtjt.eventName = new string[count];
            vtjt.eventDuration = new float[count];
            vtjt.eventDesc = new string[count];
            for (int i = 0; i < count; i++)
            {
                vtjt.eventName[i] = vjp.EventItems[i].EventName;
                vtjt.eventDuration[i] = vjp.EventItems[i].EventDuration;
                vtjt.eventDesc[i] = vjp.EventItems[i].EventDesc;
            }
        }

        VarTracerSender.Instance.SendJsonMsg(vtjt);
    }

    //public void StartVarTracer()
    //{
    //    VarTracerJsonType vtjt = new VarTracerJsonType();
    //    vtjt.runingState = (int)VarTracerConst.RunningState.RunningState_Start;
    //    SendJsonMsg(vtjt);
    //}

    //public void StopVarTracer()
    //{
    //    VarTracerJsonType vtjt = new VarTracerJsonType();
    //    vtjt.runingState = (int)VarTracerConst.RunningState.RunningState_Pause;
    //    sendMsgList.Add(vtjt);
    //}
}

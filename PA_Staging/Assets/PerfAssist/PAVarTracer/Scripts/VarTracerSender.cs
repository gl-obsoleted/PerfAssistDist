using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

public class VarTracerSender : MonoBehaviour
{
    public static VarTracerSender mInstance;
    private static List<VarTracerJsonType> sendMsgList = new List<VarTracerJsonType>();
    private static List<VarTracerJsonType> sendMsgTempList = new List<VarTracerJsonType>();
    private static bool isMainMsgList = true;
    private readonly static object _locker = new object();
    private Thread m_MsgThread;

    private float m_lastHandleJsonTime;
    void Start()
    {
        m_MsgThread = new Thread(new ThreadStart(SendMsgAsyn));
        m_MsgThread.Start();
    }

    private void SendMsgAsyn()
    {
        while (true)
        {
            if (sendMsgList.Count > 0 || sendMsgTempList.Count>0)
            {
                List<VarTracerJsonType> msgList;
                lock (_locker)
                {
                    if (isMainMsgList)
                        msgList = sendMsgList;
                    else
                        msgList = sendMsgTempList;
                    isMainMsgList = !isMainMsgList;
                }

                foreach (var vtjt in msgList)
                {
                    UsCmd pkt = new UsCmd();
                    pkt.WriteNetCmd(eNetCmd.SV_VarTracerJsonParameter);
                    pkt.WriteString(JsonUtility.ToJson(vtjt));
                    UsNet.Instance.SendCommand(pkt);
                }
                msgList.Clear();
            }
        }
    }

    public void SendJsonMsg(VarTracerJsonType vtjt)
    {
        if (vtjt.timeStamp == 0)
            vtjt.timeStamp = VarTracerUtils.GetTimeStamp();
        lock (_locker)
        {
            if (isMainMsgList)
                sendMsgList.Add(vtjt);
            else
                sendMsgTempList.Add(vtjt);
        }
    }

    void Update()
    {
    }

    public static VarTracerSender Instance
    {
        get
        {
            if (mInstance == null)
            {
                if (UsNet.Instance == null && UsNet.Instance.CmdExecutor == null)
                {
                    UnityEngine.Debug.LogError("UsNet not available");
                    return null;
                }

                GameObject go = new GameObject("VarTracerSender");
                go.hideFlags = HideFlags.HideAndDontSave;
                mInstance = go.AddComponent<VarTracerSender>();
            }
            return mInstance;
        }
    }
}

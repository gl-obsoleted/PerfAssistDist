using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Threading;

public class VarTracerNet
{
    public static VarTracerNet mInstance = null;

    private  float m_lastHandleJsonTime;

    private long m_startTimeStamp;
    public long StartTimeStamp
    {
        get { return m_startTimeStamp; }
        set { m_startTimeStamp = value; }
    }

    private long m_netDeltaTime;
    public long NetDeltaTime
    {
        get { return m_netDeltaTime; }
        set { m_netDeltaTime = value; }
    }

    List<string> vartracerJsonMsgList = new List<string>();
    public List<string> VartracerJsonMsgList
    {
        get { return vartracerJsonMsgList; }
    }

    static VartracerJsonAsynObj vartracerJsonObj = new VartracerJsonAsynObj();

    private readonly  object _locker = new object();

    public void Upate()
    {
        if (Time.realtimeSinceStartup - m_lastHandleJsonTime >= VarTracerConst.HANDLE_JASON_INTERVAL)
        {
            m_lastHandleJsonTime = Time.realtimeSinceStartup;
            if(VartracerJsonMsgList.Count > 0)
            {
                Thread mThread = new Thread(new ParameterizedThreadStart(handleMsgAsyn));
                lock (_locker)
                {
                    mThread.Start(VartracerJsonMsgList.ToArray());
                    VartracerJsonMsgList.Clear();
                }
            }

            if (vartracerJsonObj.readerFlag)
            {
                var result = vartracerJsonObj.readResovleJsonResult();
                if (result != null)
                {
                    foreach (var vjt in result)
                    {
                        VarTracerHandler.ResoloveJsonMsg(vjt);
                    }
                }
            }
        }
    }

    public void handleMsgAsyn(object o)
    {
        string[] strArray = (string[])o;
        VarTracerJsonType[] writeVjt = new VarTracerJsonType[strArray.Length];
        for (int i = 0; i < strArray.Length; i++)
		{
            var str = strArray[i];
            var resolved = JsonUtility.FromJson<VarTracerJsonType>(str);
            //Debug.LogFormat("Rec = {0}",resolved.testIndex);
            writeVjt[i] = resolved;        			 
		}
        vartracerJsonObj.writeResovleJsonResult(writeVjt);
    }

    public int GetCurrentFrameFromTimestamp(long timeStamp)
    {
        int currentFrame = (int)((timeStamp - m_startTimeStamp) / 1000.0f * VarTracerConst.FPS);
        return currentFrame;
    }


    public bool Handle_VarTracerJsonParameter(eNetCmd cmd, UsCmd c)
    {

        var varTracerInfo = c.ReadString();
        if (string.IsNullOrEmpty(varTracerInfo))
            return false;
        lock (_locker)
            VarTracerNet.Instance.VartracerJsonMsgList.Add(varTracerInfo);
        //NetUtil.Log("varTracer info{0}", varTracerInfo);
        return true;
    }


    public bool Handle_ServerLogging(eNetCmd cmd, UsCmd c)
    {
        return true;
    }

    
    public static VarTracerNet Instance
    {
        get
        {
#if UNITY_EDITOR
            if (mInstance == null)
            {
                mInstance = new VarTracerNet();
            }
            return mInstance;
#else
            return null;
#endif
        }
    }
}

public class VartracerJsonAsynObj
{
    public bool readerFlag = false;
    public VarTracerJsonType[] m_resovleResult;
    public VarTracerJsonType[] readResovleJsonResult()
    {
        if (m_resovleResult == null)
            return null;
        lock (this)
        {
            if (!readerFlag)
            {
                try
                {
                    Monitor.Wait(this);
                }
                catch (SynchronizationLockException e)
                {
                    Console.WriteLine(e);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);
                }
            }
            readerFlag = false;
            Monitor.Pulse(this);
        }
        return m_resovleResult;
    }

    public void writeResovleJsonResult(VarTracerJsonType[] resovleResult)
    {
        lock (this)
        {
            if (readerFlag)
            {
                try
                {
                    Monitor.Wait(this);
                }
                catch (SynchronizationLockException e)
                {
                    Console.WriteLine(e);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);
                }
            }
            m_resovleResult = resovleResult;
            readerFlag = true;
            Monitor.Pulse(this);
        }
    }
}


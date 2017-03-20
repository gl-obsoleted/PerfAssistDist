using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VarTracerVariable
{
    private string m_varName;
    private string m_varBodyName;
    public string VarBodyName
    {
        get { return m_varBodyName; }
        set { m_varBodyName = value; }
    }
    private List<VarDataInfo> m_dataList = new List<VarDataInfo>();
    Dictionary<string, string> m_channelDict = new Dictionary<string, string>();
    private Rect m_popupRect;
    public Rect PopupRect
    {
        get { return m_popupRect; }
        set { m_popupRect = value; }
    }

    public Dictionary<string, string> ChannelDict
    {
        get { return m_channelDict; }
        set { m_channelDict = value; }
    }

    public string VarName
    {
        get { return m_varName; }
        set { m_varName = value; }
    }

    public VarTracerVariable(string varName, string varBodyName)
    {
        m_varName = varName;
        m_varBodyName = varBodyName;
    }

    public void InsertValue(VarDataInfo dataInfo)
    {
        m_dataList.Add(dataInfo);

        foreach (var channel in m_channelDict.Keys)
        {
#if UNITY_EDITOR
            if (VarTracer.Instance.Graphs.ContainsKey(channel))
            {
                VarTracerGraphItData g = VarTracer.Instance.Graphs[channel];
                if (!g.mData.ContainsKey(m_varName))
                {
                    g.mData[m_varName] = new VarTracerDataInternal(g.mData.Count);
                }
                g.mData[m_varName].mCurrentValue = dataInfo.Value;
                g.mData[m_varName].mDataInfos.Add(dataInfo);
            }
#endif
        }
    }

    public void AttchChannel(string channel)
    {
        if(string.IsNullOrEmpty(channel))
            return;
        if (!m_channelDict.ContainsKey(channel))
        {
            m_channelDict[channel] = channel;
#if UNITY_EDITOR
            if (VarTracer.Instance.Graphs.ContainsKey(channel))
            {
                VarTracerGraphItData g = VarTracer.Instance.Graphs[channel];
                if (!g.mData.ContainsKey(m_varName))
                {
                    g.mData[m_varName] = new VarTracerDataInternal(g.mData.Count);
                }

                g.mData[m_varName].mDataInfos.Clear();
                g.mData[m_varName].mDataInfos.AddRange(m_dataList);
            }
#endif
        }
    }

    public void DetachChannel(string channel)
    {
        if (string.IsNullOrEmpty(channel))
            return;
        if (m_channelDict.ContainsKey(channel))
        {
            m_channelDict.Remove(channel);
#if UNITY_EDITOR
            if (VarTracer.Instance.Graphs.ContainsKey(channel))
            {
                VarTracerGraphItData g = VarTracer.Instance.Graphs[channel];
                if (g.mData.ContainsKey(m_varName))
                {
                    g.mData.Remove(m_varName);
                }
            }
#endif
        }
    }
}


public class VarDataInfo
{
    private float m_value;
    private int m_frameIndex;
    public int FrameIndex
    {
        get { return m_frameIndex; }
        set { m_frameIndex = value; }
    }

    public float Value
    {
        get { return m_value; }
    }
    public VarDataInfo(float value, int frameIndex)
    {
        m_value = value;
        m_frameIndex = frameIndex;
    }
}


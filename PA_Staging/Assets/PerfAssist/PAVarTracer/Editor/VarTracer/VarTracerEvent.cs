using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventData
{
    private string m_eventName;
    public string EventName
    {
        get { return m_eventName; }
        set { m_eventName = value; }
    }
    private int m_eventFrameIndex;

    public int EventFrameIndex
    {
        get { return m_eventFrameIndex; }
        set { m_eventFrameIndex = value; }
    }
    private float m_duration;

    public float Duration
    {
        get { return m_duration; }
        set { m_duration = value; }
    }
    private string m_desc;
    public string Desc
    {
        get { return m_desc; }
        set { m_desc = value; }
    }


    public EventData(int eventFrameIndex, string eventName, string desc = "", float duration = 0)
    {
        m_eventFrameIndex = eventFrameIndex;
        m_eventName = eventName;
        m_duration = duration;
        m_desc = desc;
    }
}

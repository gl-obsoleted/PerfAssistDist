using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GraphItWindow : EditorWindow
{
    const int InValidNum = -1;
    static Vector2 mGraphViewScrollPos;

    static float mWidth;

    static int mMouseOverGraphIndex = InValidNum;
    static float mMouseX = 0;

    static float x_offset = 250.0f;
    static float y_gap = 80.0f;
    static float y_offset = 20;

    static Material mLineMaterial;

    public static float m_winWidth = 0.0f;
    public static float m_winHeight = 0.0f;

    public static float m_controlScreenHeight = 0.0f;
    public static float m_controlScreenPosY = 0.0f;

    public static float m_navigationScreenHeight = 0.0f;
    public static float m_navigationScreenPosY = 0.0f;

    public static int m_variableBarIndex = InValidNum;

    const int variableNumPerLine = 12;
    const int variableLineHight = 20;

    const int variableLineStartY = 25;

    string _IPField = VarTracerConst.RemoteIPDefaultText;

    static List<string> variableCombineList = new List<string>();
    static string[] graphNumOption = { "0", "1", "2" };
    static int graphNumIndex = 0;

    bool _connectPressed = false;

    static bool m_isDrawLine = true;

    public static bool m_isStart = true;
    [MenuItem("Window/PerfAssist" + "/VarTracer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GraphItWindow window = (GraphItWindow)EditorWindow.GetWindow(typeof(GraphItWindow), false, "GraphItVariable");
        window.minSize = new Vector2(230f, 50f);
        window.Show();
    }
    void Awake()
    {
        InitNet();
    }

    void InitNet()
    {
        if (NetManager.Instance == null)
        {
            NetUtil.LogHandler = Debug.LogFormat;
            NetUtil.LogErrorHandler = Debug.LogErrorFormat;

            NetManager.Instance = new NetManager();
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_VarTracerJsonParameter, VarTracerNet.Instance.Handle_VarTracerJsonParameter);
            NetManager.Instance.RegisterCmdHandler(eNetCmd.SV_App_Logging, VarTracerNet.Instance.Handle_ServerLogging);
        }
    }


    void OnEnable()
    {
        EditorApplication.update += Update;
        if (VarTracer.Instance != null)
        {
            if (VarTracer.Instance.Graphs.Count == 0)
                VarTracer.AddChannel();

            bool constainsCamera = VarTracer.Instance.VariableBodys.ContainsKey("Camera");
            if (!constainsCamera || VarTracer.Instance.VariableBodys["Camera"].VariableDict.Count == 0)
            {
                VarTracerHandler.DefineVariable("CameraV_X", "Camera");
                VarTracerHandler.DefineVariable("CameraV_Y", "Camera");
                VarTracerHandler.DefineVariable("CameraV_Z", "Camera");
                VarTracerHandler.DefineVariable("CameraV_T", "Camera");

                VarTracerHandler.DefineVariable("PlayerV_X", "Player");
                VarTracerHandler.DefineVariable("PlayerV_Y", "Player");
                VarTracerHandler.DefineVariable("PlayerV_Z", "Player");
                VarTracerHandler.DefineVariable("CameraV_T", "Camera");

                VarTracerHandler.DefineVariable("FPS", "System");

                VarTracerHandler.DefineEvent("JUMP","Camera");
                VarTracerHandler.DefineVariable("NpcV_X", "Npc");
                VarTracerHandler.DefineVariable("NpcV_Y", "Npc");
                VarTracerHandler.DefineVariable("NpcV_Z", "Npc");
                VarTracerHandler.DefineVariable("NpcV_T", "Npc");
            }
        }

        VarTracer.AddChannel();
        VarTracer.AddChannel();
    }


    public static void StartVarTracer()
    {
        m_isStart = true;
        EditorApplication.isPaused = false;
    }

    public static void StopVarTracer()
    {
        VarTracerUtils.StopTimeStamp = VarTracerUtils.GetTimeStamp();
        m_isStart = false;
        EditorApplication.isPaused = true;
    }

    public static bool isVarTracerStart()
    {
        return m_isStart;
    }

    void OnDestroy()
    {
        if (NetManager.Instance != null)
        {
            NetManager.Instance.Dispose();
            NetManager.Instance = null;
        }
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
        VarTracer.Instance.Graphs.Clear();
        VarTracer.Instance.VariableBodys.Clear();
    }

    void Update()
    {
        if (_connectPressed)
        {
            VarTracerNetUtils.Connect(_IPField);
            _connectPressed = false;
        }
        VarTracerNet.Instance.Upate();
        Repaint();
    }
    public void CheckForResizing()
    {
        if (Mathf.Approximately(position.width, m_winWidth) &&
            Mathf.Approximately(position.height, m_winHeight))
            return;

        m_winWidth = position.width;
        m_winHeight = position.height;

        UpdateVariableAreaHight();

        m_controlScreenPosY = 0.0f;

        m_navigationScreenHeight = m_winHeight - m_controlScreenHeight;
        m_navigationScreenPosY = m_controlScreenHeight;
    }


    void OnGUI()
    {
        CheckForResizing();
        InitializeStyles();

        Handles.BeginGUI();
        Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
        //control窗口内容
        GUILayout.BeginArea(new Rect(0, m_controlScreenPosY, m_winWidth, m_controlScreenHeight));
        {
            DrawVariableBar();
        }
        GUILayout.EndArea();
        ////navigation窗口内容
        GUILayout.BeginArea(new Rect(0, m_navigationScreenPosY, m_winWidth, m_winHeight));
        {
            DrawGraphs(position, this);
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    void UpdateVariableAreaHight()
    {
        var lineNum = CalculateVariableLineNum();
        var ry = variableLineStartY * 2 + lineNum * variableLineHight;

        y_offset = ry;
        m_controlScreenHeight = ry;
    }

    int CalculateVariableLineNum()
    {
        List<VarTracerVariable> variableList = new List<VarTracerVariable>();
        foreach (var varBody in VarTracer.Instance.VariableBodys.Values)
        {
            foreach (var var in varBody.VariableDict.Values)
            {
                variableList.Add(var);
            }
        }

        int lineNum = variableList.Count / variableNumPerLine;
        int mod = variableList.Count % variableNumPerLine;
        if (mod > 0)
            lineNum += 1;

        return lineNum;
    }

    List<VarTracerVariable> GetVariableList()
    {
        List<VarTracerVariable> variableList = new List<VarTracerVariable>();
        foreach (var varBody in VarTracer.Instance.VariableBodys.Values)
        {
            foreach (var var in varBody.VariableDict.Values)
            {
                variableList.Add(var);
            }
        }
        return variableList;
    }

    void DrawVariableBar()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);

        int currentIndex = GUILayout.SelectionGrid(graphNumIndex, graphNumOption, 3, GUILayout.Width(90));
        if (graphNumIndex != currentIndex)
        {
            graphNumIndex = currentIndex;
            ShowVariableCombine();
        }

        GUILayout.Space(30);
        for (int i = 0; i < variableCombineList.Count; i++)
        {
            if (GUILayout.Button(variableCombineList[i],EventInstantButtonStyle, GUILayout.Width(90)))
            {
                variableCombineList.Remove(variableCombineList[i]);
                ShowVariableCombine();
            }
        }
        GUILayout.Space(20);

        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50), GUILayout.Height(25)))
        {
            variableCombineList.Clear();
            ShowVariableCombine();
        }

        if (GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(100)))
            VarTracer.ClearAll();

        GUI.SetNextControlName("LoginIPTextField");
        var currentStr = GUILayout.TextField(_IPField, GUILayout.Width(120));
        if (!_IPField.Equals(currentStr))
        {
            _IPField = currentStr;
        }

        if (GUI.GetNameOfFocusedControl().Equals("LoginIPTextField") && _IPField.Equals(VarTracerConst.RemoteIPDefaultText))
        {
            _IPField = "";
        }

        bool savedState = GUI.enabled;

        bool connected = NetManager.Instance != null && NetManager.Instance.IsConnected;

        GUI.enabled = !connected;
        if (GUILayout.Button("Connect", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            _connectPressed = true;
        }
        GUI.enabled = connected;
        GUI.enabled = savedState;

        string buttonName;
        if (EditorApplication.isPaused)
            buttonName = "Resume";
        else
            buttonName = "Pause";
        if (GUILayout.Button(buttonName, EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
            if (EditorApplication.isPaused)
                StopVarTracer();
            else
                StartVarTracer();
        }

        if (m_isDrawLine)
            buttonName = "Draw Point";
        else
            buttonName = "Draw Line";
        if (GUILayout.Button(buttonName, EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            m_isDrawLine = !m_isDrawLine;
        }
        GUILayout.EndHorizontal();

        var lineNum = CalculateVariableLineNum();
        var varList = GetVariableList();

        for (int i = 0; i < lineNum; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            for (int j = 0; j < variableNumPerLine; j++)
            {
                if (j + i * variableNumPerLine >= varList.Count)
                    continue;
                var var = varList[j + i * variableNumPerLine];
                var saveColor = GUI.color;
                if (VarTracer.IsVariableOnShow(var.VarName))
                    GUI.color = Color.white;

                if (GUILayout.Button(var.VarName, EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    if (!variableCombineList.Contains(var.VarName))
                    {
                        variableCombineList.Add(var.VarName);
                        ShowVariableCombine();
                    }
                    else {
                        variableCombineList.Remove(variableCombineList[i]);
                        ShowVariableCombine();
                    }
                }

                GUI.color = saveColor;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private static void ShowVariableCombine()
    {
        VarTracer.ClearGraph(graphNumIndex.ToString());
        foreach (var varName in variableCombineList)
        {
            VarTracer.AttachVariable(varName, graphNumIndex.ToString());
        }
    }

    static void DrawGraphGridLines(float y_pos, float width, float height, bool draw_mouse_line)
    {
        GL.Color(new Color(0.3f, 0.3f, 0.3f));
        float steps = 8;
        float x_step = width / steps;
        float y_step = height / steps;
        for (int i = 0; i < steps + 1; ++i)
        {
            Plot(x_offset + x_step * i, y_pos, x_offset + x_step * i, y_pos + height);
            Plot(x_offset, y_pos + y_step * i, x_offset + width, y_pos + y_step * i);
        }

        GL.Color(new Color(0.4f, 0.4f, 0.4f));
        steps = 4;
        x_step = width / steps;
        y_step = height / steps;
        for (int i = 0; i < steps + 1; ++i)
        {
            Plot(x_offset + x_step * i, y_pos, x_offset + x_step * i, y_pos + height);
            Plot(x_offset, y_pos + y_step * i, x_offset + width, y_pos + y_step * i);
        }

        if (draw_mouse_line)
        {
            GL.Color(new Color(0.8f, 0.8f, 0.8f));
            Plot(mMouseX, y_pos, mMouseX, y_pos + height);
        }
    }

    static void Plot(float x0, float y0, float x1, float y1)
    {
        GL.Vertex3(x0, y0, 0);
        GL.Vertex3(x1, y1, 0);
    }

    static void CreateLineMaterial()
    {
        if (!mLineMaterial)
        {
            mLineMaterial = new Material(Shader.Find("Custom/VarTracerGraphIt"));
            mLineMaterial.hideFlags = HideFlags.HideAndDontSave;
            mLineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public static void DrawGraphs(Rect rect, EditorWindow window)
    {
        if (VarTracer.Instance)
        {
            bool isEditorPaused = EditorApplication.isPaused;

            CreateLineMaterial();

            mLineMaterial.SetPass(0);

            int graph_index = 0;

            //use this to get the starting y position for the GL rendering
            Rect find_y = EditorGUILayout.BeginVertical(GUIStyle.none);
            EditorGUILayout.EndVertical();

            int currentFrameIndex = VarTracerNet.Instance.GetCurrentFrameFromTimestamp(VarTracerUtils.GetTimeStamp());
            if (isEditorPaused)
                currentFrameIndex = VarTracerNet.Instance.GetCurrentFrameFromTimestamp(VarTracerUtils.StopTimeStamp);

            float scrolled_y_pos = y_offset - mGraphViewScrollPos.y;
            if (Event.current.type == EventType.Repaint)
            {
                GL.PushMatrix();
                float start_y = find_y.y;
                GL.Viewport(new Rect(0, 0, rect.width, rect.height - start_y));
                GL.LoadPixelMatrix(0, rect.width, rect.height - start_y, 0);

                //Draw grey BG
                GL.Begin(GL.QUADS);
                GL.Color(new Color(0.2f, 0.2f, 0.2f));

                foreach (KeyValuePair<string, VarTracerGraphItData> kv in VarTracer.Instance.Graphs)
                {
                    float height = kv.Value.GetHeight();

                    GL.Vertex3(x_offset, scrolled_y_pos, 0);
                    GL.Vertex3(x_offset + mWidth, scrolled_y_pos, 0);
                    GL.Vertex3(x_offset + mWidth, scrolled_y_pos + height, 0);
                    GL.Vertex3(x_offset, scrolled_y_pos + height, 0);

                    scrolled_y_pos += (height + y_gap);
                }
                GL.End();

                scrolled_y_pos = y_offset - mGraphViewScrollPos.y;
                //Draw Lines
                GL.Begin(GL.LINES);

                foreach (KeyValuePair<string, VarTracerGraphItData> kv in VarTracer.Instance.Graphs)
                {
                    graph_index++;

                    float height = kv.Value.GetHeight();
                    DrawGraphGridLines(scrolled_y_pos, mWidth, height, graph_index == mMouseOverGraphIndex);

                    foreach (KeyValuePair<string, VarTracerDataInternal> entry in kv.Value.mData)
                    {
                        VarTracerDataInternal g = entry.Value;

                        float y_min = kv.Value.GetMin(entry.Key);
                        float y_max = kv.Value.GetMax(entry.Key);
                        float y_range = Mathf.Max(y_max - y_min, 0.00001f);

                        //draw the 0 line
                        if (y_min != 0.0f)
                        {
                            GL.Color(Color.white);
                            float y = scrolled_y_pos + height * (1 - (0.0f - y_min) / y_range);
                            Plot(x_offset, y, x_offset + mWidth, y);
                        }

                        GL.Color(g.mColor);

                        float previous_value = 0, value = 0;
                        int dataInfoIndex = 0, frameIndex = 0;
                        for (int i = 0; i <= currentFrameIndex; i++)
                        {
                            int dataCount = g.mDataInfos.Count;
                            if (dataCount != 0)
                            {
                                int lastFrame = g.mDataInfos[dataCount - 1].FrameIndex;
                                float lastValue = g.mDataInfos[dataCount - 1].Value;
                                frameIndex = g.mDataInfos[dataInfoIndex].FrameIndex;

                                if (dataInfoIndex >= 1)
                                    value = g.mDataInfos[dataInfoIndex - 1].Value;

                                if (dataInfoIndex == 0 && i < frameIndex)
                                    value = 0;

                                if (i >= frameIndex)
                                {
                                    while (g.mDataInfos[dataInfoIndex].FrameIndex == frameIndex && dataInfoIndex < dataCount - 1)
                                    {
                                        dataInfoIndex++;
                                    }
                                }

                                if (i > lastFrame)
                                    value = lastValue;
                            }
                            else
                            {
                                value = 0;
                            }

                            if (i >= 1)
                            {
                                float x0 = x_offset + (i - 1) * kv.Value.XStep - kv.Value.ScrollPos.x;
                                if (x0 <= x_offset - kv.Value.XStep) continue;
                                if (x0 >= mWidth + x_offset) break;
                                float y0 = scrolled_y_pos + height * (1 - (previous_value - y_min) / y_range);

                                if (i == 1)
                                {
                                    x0 = x_offset;
                                    y0 = scrolled_y_pos + height;
                                }

                                float x1 = x_offset + i * kv.Value.XStep - kv.Value.ScrollPos.x;
                                float y1 = scrolled_y_pos + height * (1 - (value - y_min) / y_range);
    
                                if (m_isDrawLine)
                                    Plot(x0, y0, x1, y1);
                                else
                                    Plot(x0, y0, x0+1, y0+1);
                            }
                            previous_value = value;
                        }
                    }

                    scrolled_y_pos += (height + y_gap);
                }
                GL.End();

                scrolled_y_pos = y_offset - mGraphViewScrollPos.y;
                scrolled_y_pos = ShowEventLabel(scrolled_y_pos);
                GL.PopMatrix();

                GL.Viewport(new Rect(0, 0, rect.width, rect.height));
                GL.LoadPixelMatrix(0, rect.width, rect.height, 0);
            }

            mGraphViewScrollPos = EditorGUILayout.BeginScrollView(mGraphViewScrollPos, GUIStyle.none);

            graph_index = 0;
            mWidth = window.position.width - x_offset;
            foreach (KeyValuePair<string, VarTracerGraphItData> kv in VarTracer.Instance.Graphs)
            {
                graph_index++;

                float height = kv.Value.GetHeight();
                float width = currentFrameIndex * kv.Value.XStep;
                if (width < mWidth)
                {
                    width = mWidth - x_offset;
                }
                else
                {
                    if (!EditorApplication.isPaused)
                        kv.Value.ScrollPos = new Vector2(width - mWidth, kv.Value.ScrollPos.y);
                }

                GUIStyle s = new GUIStyle();
                s.fixedHeight = height + y_gap;
                s.stretchWidth = true;
                Rect r = EditorGUILayout.BeginVertical(s);

                //skip subgraph title if only one, and it's the same.
                NameLabel.normal.textColor = Color.white;

                r.height = height + 50;
                r.width = width;
                r.x = x_offset - 35;
                r.y = (height + y_gap) * (graph_index - 1) - 10;

                if (kv.Value.mData.Count > 0)
                {
                    GUILayout.BeginArea(r);
                    GUILayout.BeginVertical();

                    float GraphGap = kv.Value.m_maxValue - kv.Value.m_minValue;
                    float unitHeight = GraphGap / VarTracerConst.Graph_Grid_Row_Num;

                    for (int i = 0; i < VarTracerConst.Graph_Grid_Row_Num + 1; i++)
                    {
                        GUILayout.Space(6);
                        if (unitHeight == 0)
                            EditorGUILayout.LabelField("",NameLabel);
                        else
                            EditorGUILayout.LabelField((kv.Value.m_maxValue - i * unitHeight).ToString(VarTracerConst.NUM_FORMAT_1),NameLabel);
                    }

                    GUILayout.BeginHorizontal();
                    kv.Value.ScrollPos = GUILayout.BeginScrollView(kv.Value.ScrollPos, GUILayout.Width(mWidth), GUILayout.Height(0));
                    GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(0));
                    GUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }

                DrawGraphAttribute(kv);

                ////Respond to mouse input!
                if (Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition - Event.current.delta))
                {
                    if (Event.current.button == 0)
                    {
                        kv.Value.ScrollPos = new Vector2(kv.Value.ScrollPos.x + Event.current.delta.x, kv.Value.ScrollPos.y);
                    }
                    window.Repaint();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private static void DrawGraphAttribute(KeyValuePair<string, VarTracerGraphItData> kv)
    {
        EditorGUILayout.LabelField(kv.Key,NameLabel);

        foreach (var varBodyName in GetAllVariableBodyFromChannel(kv.Key))
        {
            NameLabel.normal.textColor = Color.white;
            EditorGUILayout.LabelField("{" + varBodyName + "}:", NameLabel);

            foreach (var entry in kv.Value.mData)
            {
                var variable = VarTracer.GetGraphItVariableByVariableName(entry.Key);
                VarTracerDataInternal g = entry.Value;
                if (variable.VarBodyName.Equals(varBodyName))
                {
                    if (kv.Value.mData.Count >= 1)
                    {
                        NameLabel.normal.textColor = g.mColor;
                    }
                    EditorGUILayout.LabelField("     [" + entry.Key + "]" + "   Value: " + g.mCurrentValue.ToString(VarTracerConst.NUM_FORMAT_2),NameLabel);
                }
            }

            var varBody = VarTracer.Instance.VariableBodys[varBodyName];

            foreach (var eventName in varBody.EventInfos.Keys)
            {
                //NameLabel.normal.textColor = varBody.EventColors[eventName];
                NameLabel.normal.textColor = Color.white;
                EditorGUILayout.LabelField("     <Event>    " + eventName,NameLabel);
            }
        }

        if (kv.Value.mData.Count >= 1)
        {
            HoverText.normal.textColor = Color.white;
            EditorGUILayout.LabelField("duration:" + (mWidth / kv.Value.XStep / VarTracerConst.FPS).ToString(VarTracerConst.NUM_FORMAT_3) + "(s)", HoverText, GUILayout.Width(140));
            kv.Value.XStep = GUILayout.HorizontalSlider(kv.Value.XStep, 0.1f, 15, GUILayout.Width(160));
        }
    }

    static List<string> GetAllVariableBodyFromChannel(string channelName)
    {
        List<string> result = new List<string>();
        foreach (var varBody in VarTracer.Instance.VariableBodys)
        {
            foreach (var var in varBody.Value.VariableDict.Values)
            {
                foreach (var channel in var.ChannelDict.Values)
                {
                    if (channelName.Equals(channel))
                    {
                        if (!result.Contains(var.VarBodyName))
                            result.Add(var.VarBodyName);
                    }
                }
            }
        }
        return result;
    }

    private static bool IsEventBtnIntersect(float x1, float x2, float width1, float width2)
    {
        return System.Math.Abs(x1 - x2) <= (width1 + width2) / 2;
    }

    private static float ShowEventLabel(float scrolled_y_pos)
    {
        foreach (KeyValuePair<string, VarTracerGraphItData> kv in VarTracer.Instance.Graphs)
        {
            float height = kv.Value.GetHeight();
            List<EventData> sortedEventList = new List<EventData>();
            foreach (var varBodyName in GetAllVariableBodyFromChannel(kv.Key))
            {
                var varBody = VarTracer.Instance.VariableBodys[varBodyName];
                foreach (var eventInfo in varBody.EventInfos.Values)
                {
                    foreach (var data in eventInfo)
                    {
                        if (data.EventFrameIndex > 0)
                        {
                            float x = x_offset + data.EventFrameIndex * kv.Value.XStep - kv.Value.ScrollPos.x;
                            if (x <= x_offset - kv.Value.XStep) continue;
                            if (x >= mWidth + x_offset) break;

                            sortedEventList.Add(data);
                        }
                    }
                    sortedEventList.Sort((EventData e1, EventData e2) =>
                    {
                        return e1.EventFrameIndex.CompareTo(e2.EventFrameIndex);
                    });

                    float startY = scrolled_y_pos + height - VarTracerConst.EventStartHigh;
                    Rect preEventRect = new Rect(0, startY, 0, VarTracerConst.EventButtonHeight);
                    for (int i = 0; i < sortedEventList.Count; i++)
                    {
                        var currentEvent = sortedEventList[i];
                        GL.Color(Color.white);

                        GUIStyle style = null;
                        int buttonWidth = 0;
                        if (currentEvent.Duration == 0)
                        {
                            style = EventInstantButtonStyle;
                            buttonWidth = (int)(VarTracerConst.INSTANT_EVENT_BTN_DURATION * VarTracerConst.FPS * kv.Value.XStep);
                        }
                        else
                        {
                            style = EventDurationButtonStyle;
                            buttonWidth = (int)(currentEvent.Duration * VarTracerConst.FPS * kv.Value.XStep);
                        }

                        float x = x_offset + currentEvent.EventFrameIndex * kv.Value.XStep - kv.Value.ScrollPos.x;
                        Rect tooltip_r;
                        if (IsEventBtnIntersect(x - buttonWidth / 2, preEventRect.x, buttonWidth, preEventRect.width))
                        {
                            if (preEventRect.y > height + int.Parse(kv.Key) * (height + y_gap))
                                tooltip_r = new Rect(x - buttonWidth / 2, startY, buttonWidth, VarTracerConst.EventButtonHeight);
                            else
                                tooltip_r = new Rect(x - buttonWidth / 2, preEventRect.y + 20, buttonWidth, VarTracerConst.EventButtonHeight);
                        }
                        else
                            tooltip_r = new Rect(x - buttonWidth / 2, startY, buttonWidth, VarTracerConst.EventButtonHeight);
                        preEventRect = tooltip_r;
                        //style.normal.textColor = ;

                        if (currentEvent.Duration == 0)
                            GUI.Button(tooltip_r, currentEvent.EventName, style);
                        else
                        {
                            if (string.IsNullOrEmpty(currentEvent.Desc))
                            {
                                GUI.Button(tooltip_r, currentEvent.EventName + " (" + currentEvent.Duration + "s)", style);
                            }
                            else
                            {
                                GUI.Button(tooltip_r, currentEvent.EventName + " (" + currentEvent.Duration + "s)" + " [" + currentEvent.Desc + "]", style);
                            }
                        }
                    }
                }
            }
            scrolled_y_pos += (height + y_gap);
        }
        return scrolled_y_pos;
    }

    public static GUIStyle NameLabel;
    public static GUIStyle SmallLabel;
    public static GUIStyle HoverText;
    public static GUIStyle FracGS;
    public static GUIStyle EventInstantButtonStyle;
    public static GUIStyle EventDurationButtonStyle;

    public static void InitializeStyles()
    {
        if (NameLabel == null)
        {
            NameLabel = new GUIStyle(EditorStyles.whiteBoldLabel);
            NameLabel.normal.textColor = Color.white;
            SmallLabel = new GUIStyle(EditorStyles.whiteLabel);
            SmallLabel.normal.textColor = Color.white;

            HoverText = new GUIStyle(EditorStyles.whiteLabel);
            HoverText.alignment = TextAnchor.UpperRight;
            HoverText.normal.textColor = Color.white;

            FracGS = new GUIStyle(EditorStyles.whiteLabel);
            FracGS.alignment = TextAnchor.LowerLeft;

            EventInstantButtonStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
            EventInstantButtonStyle.normal.background = Resources.Load("instantButton") as Texture2D;
            EventInstantButtonStyle.normal.textColor = Color.white;
            EventInstantButtonStyle.alignment = TextAnchor.MiddleCenter;

            EventDurationButtonStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
            EventDurationButtonStyle.normal.background = Resources.Load("durationButton") as Texture2D;
            EventDurationButtonStyle.normal.textColor = Color.white;
            EventDurationButtonStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}
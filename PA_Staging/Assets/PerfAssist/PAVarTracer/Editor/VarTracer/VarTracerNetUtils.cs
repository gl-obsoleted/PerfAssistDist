using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

public class VarTracerNetUtils
{
    private static Regex _ipReg = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
    public static bool ValidateIPString(string ip)
    {
        return !string.IsNullOrEmpty(ip) && _ipReg.IsMatch(ip);
    }

    public static void Connect(string ip)
    {
        if (NetManager.Instance == null)
            return;

        if (NetManager.Instance.IsConnected)
            NetManager.Instance.Disconnect();

        try
        {
            if (!ValidateIPString(ip))
                throw new Exception("Invaild IP");

            if (!NetManager.Instance.Connect(ip))
                throw new Exception("Bad Connect");

            EditorPrefs.SetString(VarTracerConst.LastConnectedIP,ip);
        }
        catch (Exception ex)
        {
            EditorWindow.focusedWindow.ShowNotification(new GUIContent(string.Format("Connecting '{0}' failed: {1}", ip, ex.Message)));
            Debug.LogException(ex);

            if (NetManager.Instance.IsConnected)
                NetManager.Instance.Disconnect();
        }
    }
}

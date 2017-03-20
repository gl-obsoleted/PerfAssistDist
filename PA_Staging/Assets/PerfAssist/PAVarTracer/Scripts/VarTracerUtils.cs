using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VarTracerUtils
{
    private static long stopTimeStamp;
    public static long StopTimeStamp
    {
        get { return VarTracerUtils.stopTimeStamp; }
        set { VarTracerUtils.stopTimeStamp = value; }
    }


    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }
}

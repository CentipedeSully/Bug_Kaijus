using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogDebug 
{
    public static void Log(string statement, IDebugLoggable script = null)
    {
        if (script == null)
            Debug.Log(statement);

        else
        {
            Debug.Log($"Source: '{script.LoggableName()}', (ID: {script.LoggableID()})\n{statement}");
        }
            
    }

    public static void Warn(string statement, IDebugLoggable script = null)
    {
        if (script == null)
            Debug.LogWarning(statement);

        else
        {
            Debug.LogWarning($"Source: '{script.LoggableName()}', (ID: {script.LoggableID()})\n{statement}");
        }

    }

    public static void Error(string statement, IDebugLoggable script = null)
    {
        if (script == null)
            Debug.LogError(statement);

        else
        {
            Debug.LogError($"Source: '{script.LoggableName()}', (ID: {script.LoggableID()})\n{statement}");
        }

    }
}

public interface IDebugLoggable
{
    int LoggableID();

    string LoggableName();
}

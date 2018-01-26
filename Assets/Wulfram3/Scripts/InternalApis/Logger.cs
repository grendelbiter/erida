using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;


public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Exception,
    Trace
}

public class Logger
{
    private static bool consoleLog = true;

    private static bool analyticsLog = false;

    public static void ActivateConsoleLog(bool isOn)
    {
        consoleLog = isOn;
    }

    public static void ActivateAnalyticsLog(bool isOn)
    {
        analyticsLog = isOn;
    }

    public static bool Log(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleLog(LogLevel.Debug, message, callerMethodName, callerType, callerMethodParameters);
    }

    public static bool Info(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleLog(LogLevel.Info, message, callerMethodName, callerType, callerMethodParameters);
    }

    public static bool Warning(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleLog(LogLevel.Warning, message, callerMethodName, callerType, callerMethodParameters);
    }

    public static bool Error(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleLog(LogLevel.Error, message, callerMethodName, callerType, callerMethodParameters);
    }

    public static bool Exception(Exception ex, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleException(ex, callerMethodName, callerType, callerMethodParameters);
    }

    public static bool Trace(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath]string callerType = "", string callerMethodParameters = "")
    {
        return ConsoleLog(LogLevel.Trace, message, callerMethodName, callerType, callerMethodParameters);
    }

    private static bool ConsoleLog(LogLevel level, string message, string callerMethodName, string callerType, string callerMethodParameters)
    {
        var msg = "";
        if (string.IsNullOrEmpty(callerMethodParameters))
        {
            msg = string.Format("[{5}][{4}]{3} - {0}.{1}", callerType, callerMethodName, callerMethodParameters, message, Enum.GetName(typeof(LogLevel), level), DateTime.UtcNow.ToString());
        }
        else
        {
            msg = string.Format("[{5}][{4}]{3} - {0}.{1} with Params:{2}", callerType, callerMethodName, callerMethodParameters, message, Enum.GetName(typeof(LogLevel), level), DateTime.UtcNow.ToString());
        }

        if(consoleLog)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(msg);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(msg);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(msg);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(msg);
                    break;
                case LogLevel.Trace:
                    UnityEngine.Debug.Log(msg);
                    break;
                default:
                    break;
            }
        }

        if(analyticsLog)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Analytics.CustomEvent("LoggerDebug", new Dictionary<string, object>
                          {
                            { "message", msg },
                          });
                    break;
                case LogLevel.Info:
                    Analytics.CustomEvent("LoggerInfo", new Dictionary<string, object>
                          {
                            { "message", msg },
                          });
                    break;
                case LogLevel.Warning:
                    Analytics.CustomEvent("LoggerWarning", new Dictionary<string, object>
                          {
                            { "message", msg },
                          });
                    break;
                case LogLevel.Error:
                    Analytics.CustomEvent("LoggerError", new Dictionary<string, object>
                          {
                            { "message", msg },
                          });
                    break;
                case LogLevel.Trace:
                    Analytics.CustomEvent("LoggerTrace", new Dictionary<string, object>
                          {
                            { "message", msg },
                          });
                    break;
                default:
                    break;
            }
        }


        return true;
    }

    private static bool ConsoleException(Exception ex, string callerMethodName, string callerType, string callerMethodParameters)
    {
        var msg = "";
        if (string.IsNullOrEmpty(callerMethodParameters))
        {
            msg = string.Format("[{5}][{4}]{3} - {0}.{1}", callerType, callerMethodName, callerMethodParameters, ex.Message, Enum.GetName(typeof(LogLevel), LogLevel.Exception), DateTime.UtcNow.ToString());
        }
        else
        {
            msg = string.Format("[{5}][{4}]{3} - {0}.{1} with Params:{2}", callerType, callerMethodName, callerMethodParameters, ex.Message, Enum.GetName(typeof(LogLevel), LogLevel.Exception), DateTime.UtcNow.ToString());
        }

        if (consoleLog)
        {
            UnityEngine.Debug.LogException(ex);
        }

        if (analyticsLog)
        {
            Analytics.CustomEvent("Logger.Exception", new Dictionary<string, object>
            {
                { "message", ex.Message },
                { "stack", ex.StackTrace },
            });
        }


        return true;
    }
}

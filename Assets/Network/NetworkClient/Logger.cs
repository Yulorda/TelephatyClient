// A simple logger class that uses Console.WriteLine by default.
// Can also do Logger.LogMethod = Debug.Log for Unity etc.
// (this way we don't have to depend on UnityEngine.DLL and don't need a
//  different version for every UnityEngine version here)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telepathy
{
    public class Logger
    {
        private static Action<string> log = Debug.Log;
        private static Action<string> logWarning = Debug.LogWarning;
        private static Action<string> logError = Debug.LogError;

        private static Dictionary<string, Action<string>> loggerDependence = new Dictionary<string, Action<string>>();

        public static void Log(string value)
        {
            log?.Invoke(value);
        }

        public static void LogWarning(string value)
        {
            logWarning?.Invoke(value);
        }

        public static void LogError(string value)
        {
            logError?.Invoke(value);
        }

        public static void Log(string value, string loggerKey)
        {
            if (loggerDependence.ContainsKey(loggerKey))
                loggerDependence[loggerKey]?.Invoke(value);
        }

        public static void AddLogger(string loggerKey, Action<string> action)
        {
            if (action != null)
            {
                if (loggerDependence.ContainsKey(loggerKey))
                {
                    loggerDependence.Remove(loggerKey);
                }

                loggerDependence.Add(loggerKey, action);
            }
        }

        public static void Initialize()
        {
            Logger.AddLogger("ClientConnected", Debug.Log);

            Logger.AddLogger("ClientDisconnected", Debug.Log);

            Logger.AddLogger("Message", Debug.Log);
        }
    }
}
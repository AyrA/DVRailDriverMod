using System;
using UnityEngine;

namespace DVRailDriverMod
{
    internal static class Logging
    {
        private const string ModName = "DVRailDriverMod";

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message, params object[] args) => LogDebug(string.Format(message, args));

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            System.Diagnostics.Debug.Print("[{0}] {1}", ModName, message);
            Debug.LogFormat("[{0}] {1}", ModName, message);
        }

        public static void LogInfo(string message, params object[] args) => LogInfo(string.Format(message, args));

        public static void LogInfo(string message)
        {
            System.Diagnostics.Debug.Print("[{0}] {1}", ModName, message);
            Debug.LogFormat("[{0}] {1}", ModName, message);
        }

        public static void LogWarning(string message, params object[] args) => LogWarning(string.Format(message, args));

        public static void LogWarning(string message)
        {
            System.Diagnostics.Debug.Print("[{0}] {1}", ModName, message);
            Debug.LogWarningFormat("[{0}] {1}", ModName, message);
        }

        public static void LogError(string message, params object[] args) => LogError(string.Format(message, args));

        public static void LogError(string message)
        {
            System.Diagnostics.Debug.Print("[{0}] {1}", ModName, message);
            Debug.LogErrorFormat("[{0}] {1}", ModName, message);
        }

        public static void LogException(Exception ex, string message, params object[] args) => LogError(ex, string.Format(message, args));

        public static void LogError(Exception ex, string message)
        {
            LogError(message);
            while (ex != null)
            {
                LogError("== {0} ==", ex.GetType().Name);
                LogError("Info: {0}", ex.Message);
                foreach (var line in ex.StackTrace.Split('\n'))
                {
                    LogError("Stack: {0}", line.TrimEnd());
                }
                ex = ex.InnerException;
            }
        }
    }
}

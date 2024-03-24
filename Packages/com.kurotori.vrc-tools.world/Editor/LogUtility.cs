using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace KurotoriTools
{
    public enum LogType
    {
        LOG,
        WARNING,
        ERROR,
    };


    public static class LogSystem
    {
        public struct Log
        {
            public string tag;
            public string context;
        }

        public static IReadOnlyList<Log> LogList {get{ return LogStrage.instance.logList.AsReadOnly(); }}

        private static List<ILogUpdateObserver> observers = new List<ILogUpdateObserver>();

        public static void AddObserver(ILogUpdateObserver observer)
        {
            if(!observers.Contains(observer))
                observers.Add(observer);
        }

        public static void DeleteObserber(ILogUpdateObserver observer)
        {
            observers.Remove(observer);
        }

        private static void NortifyAll()
        {
            foreach(var observer in observers)
            {
                observer.OnUpdateLog();
            }
        }

        public static void AddLog(string tag, string context)
        {
            var instance = LogStrage.instance;

            Log log;
            log.tag = tag;
            log.context = context;

            instance.logList.Add(log);
            NortifyAll();
        }

        public static string CreateLogText()
        {
            var logList = LogStrage.instance.logList;

            string output = "";

            foreach(var log in logList)
            {
                output += string.Format("[{0}]:{1}\n", log.tag, log.context);
            }

            return output;
        }

        public static void SystemCopyLogText()
        {
            EditorGUIUtility.systemCopyBuffer = CreateLogText(); 
        }

        public static void ClearLog()
        {
            Debug.Log("LogSystem : ログをクリアしました");
            LogStrage.instance.logList.Clear();
            NortifyAll();
        }

        public interface ILogUpdateObserver
        {
            void OnUpdateLog();
        }

        private class LogStrage : ScriptableSingleton<LogStrage>
        {
            public List<Log> logList = new List<Log>();
            
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XiRenameTool
{
    public static class XiRenameLogger 
    {
        public static string UserName => Environment.UserName;
        public static string MachineName => Environment.MachineName;
        private static string logFileName;
        public static string LogFileName => logFileName;
        static XiRenameLogger()
        {
            var dir = $"{Application.streamingAssetsPath}/XiRename";
            System.IO.Directory.CreateDirectory(dir);
            logFileName = $"{dir}/XiRename.log";
        }

        public static void Log(string prefix, string message)
        {
            try
            {
                System.IO.File.AppendAllText(LogFileName, $"{TimeStamp.GetStamp()} : [{prefix}] : {UserName}@{MachineName} : {message}\n");
            }
            catch { }
        }

        /// <summary>
        /// This class should produce string version of current time.
        /// This string can be added to the log entry.
        /// </summary>
        public static class TimeStamp
        {
            private static DateTime lastTime;
            private static string lastTimeStamp;

            /// <summary>Get current time stamp</summary>
            public static string GetStamp()
            {
                var timeNow = DateTime.Now;
                if (lastTime != timeNow)
                {
                    lastTime = timeNow;
                    lastTimeStamp = $"{timeNow.Hour.ToString("00")}:{timeNow.Minute.ToString("00")}:{timeNow.Second.ToString("00")}";
                }
                return lastTimeStamp;
            }
        }
    }
}

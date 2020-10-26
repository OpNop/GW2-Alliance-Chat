using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Chat_Client.utils.Log
{
    public class Logger
    {
        //Fields
        private bool _mConsoleOutput;
        private bool _mConsoleTimestamp;
        private readonly object _mLock = new object();
        private string _mLogDirectory;
        private LogIntervalType _mLogInterval;
        private LogLevel _mLogLevel;
        private string _mPostfix;
        private string _mPrefix;
        private bool _logMethod;

        //Public Methods
        public static Logger getInstance()
        {
            return SingletonHolder._instance;
        }

        public ErrorCode Initialize(string prefix, string postfix, string logDirectory, LogIntervalType logInterval, LogLevel logLevel, bool consoleOutput, bool consoleTimestamp, bool logMethod = false)
        {
            _mLogDirectory = logDirectory;
            _mLogInterval = logInterval;
            _mPrefix = prefix;
            _mPostfix = postfix;
            _mLogLevel = logLevel;
            _mConsoleOutput = consoleOutput;
            _mConsoleTimestamp = consoleTimestamp;
            _logMethod = logMethod;

            if (((logInterval == LogIntervalType.IT_ONE_FILE) && (prefix == "")) && (postfix == ""))
            {
                return ErrorCode.EC_ONE_FILE_LOG_REQUIRES_PREFIX_OR_POSTFIX;
            }
            try
            {
                Directory.CreateDirectory(logDirectory);
            }
            catch
            {
                return ErrorCode.EC_CANNOT_CREATE_DIRECTORY;
            }
            return ErrorCode.EC_SUCCESS;
        }

        public void Add(LogLevel logLevel, string logMessage, params object[] list)
        {
            PutLog(logLevel, logMessage, list);
        }

        public void AddDebug(string logMessage, params object[] list)
        {
            PutLog(LogLevel.D, logMessage, list);
        }

        public void AddNotice(string logMessage, params object[] list)
        {
            PutLog(LogLevel.N, logMessage, list);
        }

        public void AddWarning(string logMessage, params object[] list)
        {
            PutLog(LogLevel.W, logMessage, list);
        }

        public void AddInfo(string logMessage, params object[] list)
        {
            PutLog(LogLevel.I, logMessage, list);
        }

        public void AddError(string logMessage, params object[] list)
        {
            PutLog(LogLevel.E, logMessage, list);
        }

        public void AddBinary(byte[] data)
        {
            var hexForm = "";
            foreach (byte b in data)
            {
                hexForm = hexForm + b.ToString("X2");
            }
            PutLog(LogLevel.D, "RAW: {0}", hexForm);
        }

        //Private Methods 
        private void PutLog(LogLevel logLevel, string logFormat, params object[] list)
        {
            //Drop out if log is below minimum log level
            if (logLevel > _mLogLevel) return;
            
            string logMessage;

            if (list.Length > 0)
            {
                logMessage = string.Format(logFormat, list);
            } else
            {
                //treat the format as the message
                logMessage = logFormat;
            }
            var now = DateTime.Now;

            if (_logMethod)
            {
                StackTrace stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(2);
                logMessage = $"{frame.GetMethod().DeclaringType.Name}::{frame.GetMethod().Name}:: {logMessage}";// stackTrace.GetFrame(1).GetMethod().Name;
            }



            if (_mConsoleOutput)
            {
                if (_mConsoleTimestamp)
                    PrintLog(string.Format("{0} {1} {2}\r\n", now.ToString("MM/dd/yy HH:mm"), logLevel, logMessage), logLevel);
                else
                    PrintLog(string.Format("{0} {1}\r\n", logLevel, logMessage), logLevel);
            }

            logMessage = string.Format("{0} {1} {2}\r\n", now.ToString("MM/dd/yy HH:mm"), logLevel, logMessage);
            var logFile = "";
            switch (_mLogInterval)
            {
                case LogIntervalType.IT_ONE_FILE:
                    logFile = _mPrefix + _mPostfix + ".log";
                    break;

                case LogIntervalType.IT_PER_DAY:
                    logFile = _mPrefix + now.ToString("yyyyMMdd") + _mPostfix + ".log";
                    break;

                case LogIntervalType.IT_PER_HOUR:
                    logFile = _mPrefix + now.ToString("yyyyMMdd_HH") + _mPostfix + ".log";
                    break;

                case LogIntervalType.IT_PER_MIN:
                    logFile = _mPrefix + now.ToString("yyyyMMdd_HHmm") + _mPostfix + ".log";
                    break;
            }
            string path;
            var ch = _mLogDirectory[_mLogDirectory.Length - 1];
            if (@"\" == ch.ToString())
            {
                path = _mLogDirectory + logFile;
            }
            else
            {
                path = _mLogDirectory + @"\" + logFile;
            }
            lock (_mLock)
            {
                var bytes = Encoding.UTF8.GetBytes(logMessage);
                try
                {
                    var stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error writing to log file " + logFile);
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
                }
            }
        }

        private static void PrintLog(string logMessage, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.D:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.E:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.I:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.N:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.W:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.Write(logMessage);
            Console.ResetColor();
        }

        private static class SingletonHolder
        {
            internal static readonly Logger _instance = new Logger();
        }
    }
}

// 2022-05-24-0553
//
// mod from https://gist.github.com/heiswayi/69ef5413c0f28b3a58d964447c275058
//
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace UTIL
{
    internal class LogFile
    {
        [Flags]
        private enum LogLevel
        {
            DEBUG,
            ERROR,
            FATAL,
            INFO,
            TRACE,
            WARN,
        }

        private readonly string logFilePath;
        private readonly string dateTimeFormat;
        private readonly object fileLock = new object();

        //Constructor
        public LogFile(string pLogFilePath = "", string pDateTimeFormat = "")
        {
            if (pLogFilePath == String.Empty)
                logFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".log";
            else
                logFilePath = pLogFilePath;

            if (pDateTimeFormat == String.Empty) {
                dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            } else
            {
                dateTimeFormat = pDateTimeFormat; 
            }
        }
        public void Clear()
        {
            File.Delete(logFilePath);
        }
        public void Debug(string text)
        {
            WriteLineFormatted(LogLevel.DEBUG, text);
        }
        public void Error(string text)
        {
            WriteLineFormatted(LogLevel.ERROR, text);
        }
        public void Fatal(string text)
        {
            WriteLineFormatted(LogLevel.FATAL, text);
        }
        public void Info(string text)
        {
            WriteLineFormatted(LogLevel.INFO, text);
        }
        public void Trace(string text)
        {
            WriteLineFormatted(LogLevel.TRACE, text);
        }
        public void Warning(string text)
        {
            WriteLineFormatted(LogLevel.WARN, text);
        }
        private void WriteLineFormatted(LogLevel level, string text)
        {
            text = String.Format("{0} [{1,-5}] {2}",
                DateTime.Now.ToString(dateTimeFormat),
                level.ToString(),
                text);

            WriteLine(text, true);
        }
        public void WriteLine(string text, bool append = true)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                lock (fileLock)
                {
                    //If the file exists, it is appended; otherwise, a new file is created.
                    using (StreamWriter writer = new StreamWriter(logFilePath, append, Encoding.UTF8))
                    {
                        writer.WriteLine(text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

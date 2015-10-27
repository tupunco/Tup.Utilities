using System.Diagnostics;

namespace Tup.Utilities
{
    /// <summary>
    /// 日志类
    /// </summary>
    /// <remarks>
    /// 调用 Log4Net 日志模块
    /// </remarks>
    public class LogManager
    {
        static LogManager()
        {
            //初始化 Log4Net
            LogHelper.Init();
        }

        public static LogManager Instance = new LogManager();

        #region Debug Log(调试模式有效)

        /// <summary>
        /// Debug Log(调试模式有效)
        /// </summary>
        /// <param name="msg"></param>
        [Conditional("DEBUG")]
        public void Debug(string msg)
        {
            LogHelper.Write(msg, LogHelper.LogMessageType.Debug);
        }

        /// <summary>
        /// Debug Log(调试模式有效)
        /// </summary>
        /// <param name="msg"></param>
        [Conditional("DEBUG")]
        public void Debug(string format, params object[] msgs)
        {
            Debug(string.Format(format, msgs));
        }

        #endregion Debug Log(调试模式有效)

        #region Error Log

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string msg)
        {
            LogHelper.Write(msg, LogHelper.LogMessageType.Error);
        }

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string format, params object[] msgs)
        {
            Error(string.Format(format, msgs));
        }

        #endregion Error Log

        #region Info Log

        /// <summary>
        /// Info Log
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string msg)
        {
            LogHelper.Write(msg, LogHelper.LogMessageType.Info);
        }

        /// <summary>
        /// Info Log
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string format, params object[] msgs)
        {
            Info(string.Format(format, msgs));
        }

        #endregion Info Log

        #region Fatal Log

        /// <summary>
        /// Fatal Log
        /// </summary>
        /// <param name="msg"></param>
        public void Fatal(string msg)
        {
            LogHelper.Write(msg, LogHelper.LogMessageType.Fatal);
        }

        /// <summary>
        /// Fatal Log
        /// </summary>
        /// <param name="msg"></param>
        public void Fatal(string format, params object[] msgs)
        {
            Fatal(string.Format(format, msgs));
        }

        #endregion Fatal Log
    }
}
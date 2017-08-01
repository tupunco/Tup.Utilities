using System;

namespace Tup.Utilities.Logging
{
    /// <summary>
    /// ILogger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Error(object message, Exception exception = null);
        /// Logs a Error format message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        /// Warn
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Warn(object message, Exception exception = null);
        /// <summary>
        /// Logs a Warning format message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void WarnFormat(string format, params object[] args);

        void Info(object message, Exception exception = null);
        /// <summary>
        /// Logs an Info format message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Debug(object message, Exception exception = null);
        /// <summary>
        /// Logs a Debug format message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void DebugFormat(string format, params object[] args);

        /// <summary>
        /// IsDebugEnabled
        /// </summary>
        bool IsDebugEnabled { get; }
        /// <summary>
        /// IsInfoEnabled
        /// </summary>
        bool IsInfoEnabled { get; }
        /// <summary>
        /// IsWarnEnabled
        /// </summary>
        bool IsWarnEnabled { get; }
        /// <summary>
        /// IsErrorEnabled
        /// </summary>
        bool IsErrorEnabled { get; }
    }
}

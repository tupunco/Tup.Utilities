using System;

namespace Tup.Utilities.Logging.Null
{
    /// <summary>
    /// NullLogger
    /// </summary>
    internal class NullLogger : ILogger
    {
        /// <summary>
        /// Default Instance
        /// </summary>
        public readonly static ILogger Default = new NullLogger();

        public void Error(object message, Exception exception = null)
        {
        }
        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void Warn(object message, Exception exception = null)
        {
        }
        public void WarnFormat(string format, params object[] args)
        {
        }

        public void Info(object message, Exception exception = null)
        {
        }
        public void InfoFormat(string format, params object[] args)
        {
        }

        public void Debug(object message, Exception exception = null)
        {
        }
        public void DebugFormat(string format, params object[] args)
        {
        }

        public bool IsDebugEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return false;
            }
        }
    }
}

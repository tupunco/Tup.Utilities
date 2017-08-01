using System;

namespace Tup.Utilities.Logging
{
    /// <summary>
    /// LogManager
    /// </summary>
    public class LogManager
    {
        private static ILoggerFactory logFactory;

        /// <summary>
        /// LogManager
        /// </summary>
        public static ILoggerFactory LogFactory
        {
            get
            {
                if (logFactory == null)
                    return Null.NullLogFactory.Default;

                return logFactory;
            }
            set
            {
                logFactory = value;
            }
        }

        /// <summary>
        /// GetLogger
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ILogger GetLogger(Type type)
        {
            return LogFactory.GetLogger(type);
        }

        /// <summary>
        /// GetLogger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger GetLogger<T>()
        {
            return LogFactory.GetLogger(typeof(T));
        }

        /// <summary>
        /// GetLogger
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static ILogger GetLogger(string typeName)
        {
            return LogFactory.GetLogger(typeName);
        }
    }
}

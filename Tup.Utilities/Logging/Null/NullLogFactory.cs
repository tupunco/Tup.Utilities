using System;

namespace Tup.Utilities.Logging.Null
{
    /// <summary>
    /// Creates a Debug Logger, that logs all messages to: System.Diagnostics.Debug
    /// Made public so its testable
    /// </summary>
    internal class NullLogFactory : ILoggerFactory
    {
        /// <summary>
        /// Default Instance
        /// </summary>
        public readonly static ILoggerFactory Default = new NullLogFactory();

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILogger GetLogger(Type type)
        {
            return NullLogger.Default;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public ILogger GetLogger(string typeName)
        {
            return NullLogger.Default;
        }
    }
}

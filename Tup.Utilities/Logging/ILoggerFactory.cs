using System;

namespace Tup.Utilities.Logging
{
    /// <summary>
    /// ILoggerFactory
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// GetLogger
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ILogger GetLogger(Type type);

        /// <summary>
        /// GetLogger
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        ILogger GetLogger(string typeName);
    }
}

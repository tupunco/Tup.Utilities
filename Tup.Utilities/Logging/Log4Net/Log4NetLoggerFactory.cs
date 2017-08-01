using System;
using System.IO;

namespace Tup.Utilities.Logging.Log4Net
{
    /// <summary>
    /// Log4Net LoggerFactory
    /// </summary>
    /// <remarks>
    /// https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack.Logging.Log4Net/Log4NetFactory.cs
    /// </remarks>
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Default_Config
        /// </summary>
        public readonly static string Default_Config = "Log4Net.config";

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLoggerFactory"/> class.
        /// </summary>
        public Log4NetLoggerFactory()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLoggerFactory"/> class.
        /// </summary>
        /// <param name="configureLog4Net">if set to <c>true</c> [will use the xml definition in App.Config to configure log4 net].</param>
        public Log4NetLoggerFactory(bool configureLog4Net)
        {
            if (configureLog4Net)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLoggerFactory"/> class.
        /// </summary>
        /// <param name="log4NetConfigurationFile">The log4 net configuration file to load and watch. If not found configures from App.Config.</param>
        public Log4NetLoggerFactory(string log4NetConfigurationFile)
        {
            //Restart logging if necessary
            var rootRepository = log4net.LogManager.GetRepository();
            if (rootRepository != null)
                rootRepository.Shutdown();

            if (File.Exists(log4NetConfigurationFile))
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigurationFile));
            else
                log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public ILogger GetLogger(Type type)
        {
            return new Log4NetLogger(type);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public ILogger GetLogger(string typeName)
        {
            return new Log4NetLogger(typeName);
        }
    }
}

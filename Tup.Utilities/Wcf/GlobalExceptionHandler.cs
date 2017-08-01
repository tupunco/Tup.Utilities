using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using log4net;

namespace Tup.Utilities.Wcf
{
    /// <summary>
    ///     GlobalExceptionHandler
    /// </summary>
    /// <remarks>
    ///     FROM:http://www.cnblogs.com/seesea125/archive/2012/10/26/2741652.html
    /// </remarks>
    public class GlobalExceptionHandler : IErrorHandler
    {
        /// <summary>
        ///     测试log4net
        /// </summary>
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GlobalExceptionHandler));

        #region IErrorHandler Members

        /// <summary>
        ///     HandleError
        /// </summary>
        /// <param name="ex">ex</param>
        /// <returns>true</returns>
        public bool HandleError(Exception ex)
        {
            return true;
        }

        /// <summary>
        ///     ProvideFault
        /// </summary>
        /// <param name="ex">ex</param>
        /// <param name="version">version</param>
        /// <param name="msg">msg</param>
        public void ProvideFault(Exception ex, MessageVersion version, ref Message msg)
        {
            /// 写入log4net
            log.Error("WCF 异常", ex);

            var newEx = new FaultException(string.Format("WCF 接口出错 {0}", ex.TargetSite.Name));
            var msgFault = newEx.CreateMessageFault();
            msg = Message.CreateMessage(version, msgFault, newEx.Action);
        }

        #endregion
    }

    /// <summary>
    ///     GlobalExceptionHandler Behaviour Attribute
    /// </summary>
    public class GlobalExceptionHandlerBehaviourAttribute : Attribute, IServiceBehavior
    {
        private readonly Type _errorHandlerType;

        public GlobalExceptionHandlerBehaviourAttribute(Type errorHandlerType)
        {
            _errorHandlerType = errorHandlerType;
        }

        #region IServiceBehavior Members

        public void Validate(ServiceDescription description,
                             ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription description,
                                         ServiceHostBase serviceHostBase,
                                         Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription description,
                                          ServiceHostBase serviceHostBase)
        {
            var handler = (IErrorHandler)Activator.CreateInstance(_errorHandlerType);

            foreach (var dispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = dispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                    channelDispatcher.ErrorHandlers.Add(handler);
            }
        }

        #endregion
    }
}

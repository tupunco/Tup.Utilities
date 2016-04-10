using System;
using System.Runtime.Serialization;

namespace Tup.Utilities.Wcf
{
    /// <summary>
    ///     Message Base
    /// </summary>
    [DataContract]
    public abstract class MessageBase
    {
        /// <summary>
        /// </summary>
        protected MessageBase()
        {
            MessageGuid = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     当前消息 GUID
        /// </summary>
        public string MessageGuid { get; set; }
    }
}

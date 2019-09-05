using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// Internal exception that in theory should never ben thrown, it is only thrown in circumstances that should never happen
    /// </summary>
    [Serializable]
    internal class PanicException : Exception
    {
        public PanicException()
        {
        }

        public PanicException(string message) : base(message)
        {
        }

        public PanicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PanicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

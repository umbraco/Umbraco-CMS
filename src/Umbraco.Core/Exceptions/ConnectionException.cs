using System;

namespace Umbraco.Core.Exceptions
{
    internal class ConnectionException : Exception
    {
        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}
using System;

namespace Umbraco.Core.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

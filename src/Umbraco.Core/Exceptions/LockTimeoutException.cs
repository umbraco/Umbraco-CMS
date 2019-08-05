using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown if the a query as waited for the lock too long
    /// </summary>
    public class LockTimeoutException : Exception
    {
        public LockTimeoutException(Exception inner)
            : base(string.Empty, inner)
        { }

        public short Reason { get; set; }
    }
}

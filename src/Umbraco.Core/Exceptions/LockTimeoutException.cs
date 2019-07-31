using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown if the a query as waited for the lock too long
    /// </summary>
    public class LockTimeoutException : Exception
    {
        public override string Message => Reason ?? base.Message;

        public LockTimeoutException(Exception inner)
            : base(string.Empty, inner)
        { }

        public string Reason { get; set; }
    }
}

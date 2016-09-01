using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown if the Umbraco application cannnot boot.
    /// </summary>
    public class BootFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public BootFailedException(string message)
            : base(message)
        { }
    }
}
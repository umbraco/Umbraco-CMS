using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown if the Umbraco application cannnot boot.
    /// </summary>
    public class BootFailedException : Exception
    {
        /// <summary>
        /// Defines the default boot failed exception message.
        /// </summary>
        public const string DefaultMessage = "Boot failed: Umbraco cannot run. Sad. See Umbraco's log file for more details.";

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public BootFailedException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class with a specified error message
        /// and a reference to the inner exception which is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="inner">The inner exception, or null.</param>
        public BootFailedException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}

using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown if the umbraco application cannnot boot
    /// </summary>
    public class UmbracoStartupFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public UmbracoStartupFailedException(string message) : base(message)
        {
        }
    }
}
using System;

namespace Umbraco.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a null reference, or an empty argument,
    /// is passed to a method that does not accept it as a valid argument.
    /// </summary>
    public class ArgumentNullOrEmptyException : ArgumentNullException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException"/> class
        /// with the name of the parameter that caused this exception.
        /// </summary>
        /// <param name="paramName">The named of the parameter that caused the exception.</param>
        public ArgumentNullOrEmptyException(string paramName)
            : base(paramName)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentNullOrEmptyException"/> class
        /// with a specified error message and the name of the parameter that caused this exception.
        /// </summary>
        /// <param name="paramName">The named of the parameter that caused the exception.</param>
        /// <param name="message">A message that describes the error.</param>
        public ArgumentNullOrEmptyException(string paramName, string message)
            : base(paramName, message)
        { }
    }
}
